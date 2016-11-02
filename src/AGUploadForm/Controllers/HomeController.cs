using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
//using Newtonsoft.Json;
using Microsoft.AspNetCore.Mvc;
using AGUploadForm.Models;
using AGUploadForm.Models.Settings;
using Microsoft.Extensions.Options;
using AGUploadForm.Models.FormViewModels;
using Microsoft.AspNetCore.Mvc.Rendering;
using AGUploadForm.Data;
using System.IO;
using Microsoft.AspNetCore.Hosting;
using System.Net.Mail;
using System.Net;
using System.Text;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.Extensions.Logging;

namespace AGUploadForm.Controllers
{
    public class HomeController : Controller
    {
        private readonly AppSettings _appSettings;
        private readonly FormSettings _settings;
        private readonly VIPSettings _vipsettings;
        private readonly ApplicationDbContext _context;
        private readonly IHostingEnvironment _hostingEnvironment;
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<HomeController> _logger;

        public HomeController(
            IOptions<AppSettings> appSettingsOptions,
            IOptions<FormSettings> settingsOptions,
            IOptions<VIPSettings> vipSettingsOptions,
            ApplicationDbContext context,
            IHostingEnvironment hostingEnvironment,
            IServiceProvider serviceProvider,
            ILogger<HomeController> logger)
        {
            _appSettings = appSettingsOptions.Value;
            _settings = settingsOptions.Value;
            _vipsettings = vipSettingsOptions.Value;
            _context = context;
            _hostingEnvironment = hostingEnvironment;
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        //Updated to include VIP Settings and the identifier to set them up in the view model
        public IActionResult Index(string id)
        {
            FormViewModel formViewModel = new FormViewModel(_settings, _vipsettings, id);
            return View(formViewModel);
        }

        public IActionResult FormSubmitted()
        {
            return View();
        }

        public IActionResult Error()
        {
            return View();
        }

        public JsonResult GetDepartmentSelectListByOfficeName(string officeName)
        {
            return Json(new SelectList(_settings.Offices.Find(o => o.Name.Equals(officeName)).Departments.ToList(), "Name", "Name"));
        }

        [HttpPost]
        public async Task<IActionResult> Index(FormViewModel formViewModel, string id)
        {
            if (!ModelState.IsValid)
            {
                //formViewModel.OfficeSelectList = new SelectList(_settings.Offices, "Name", "Name");
                formViewModel.SetDropDowns(_settings, _vipsettings, id);
                formViewModel.VIPId = id;
                if (!string.IsNullOrEmpty(id) && _vipsettings != null)
                {
                    formViewModel.Vip = _vipsettings.GetVIPByID(id);
                }
                if (string.IsNullOrEmpty(formViewModel.SelectedOfficeName))
                {
                    //formViewModel.DepartmentSelectList = new SelectList(string.Empty, "Name", "Name");
                    formViewModel.DepartmentSelectList = new SelectList(new List<SelectListItem> {
                        new SelectListItem { Selected = true, Text = "Choose a Location", Value = ""} }, "Value", "Text");
                }
                else if (!formViewModel.BranchAndDepartmentSelected)
                {
                    List<Department> list = _settings.Offices.Find(o => o.Name.Equals(formViewModel.SelectedOfficeName)).Departments.ToList();
                    list.Insert(0, new Department() { Name = "Select One" });
                    formViewModel.DepartmentSelectList = new SelectList(list, "Name", "Name");
                    formViewModel.DepartmentSelectList.First().Value = "";
                }
                return View(formViewModel);
            }

            IList<string> errors = new List<string>();

            // Use FallbackOffice if outside office hours
            string officeName = formViewModel.SelectedOfficeName;
            Office office = _settings.Offices.Find(o => o.Name.Equals(officeName));
            int currentHour = DateTime.Now.Hour;
            if (currentHour < office.HoursStart || currentHour >= office.HoursFinish)
            {
                officeName = office.FallbackOfficeName;
                office = _settings.Offices.Find(o => o.Name.Equals(officeName));
            }

            string saveLocation = office.SaveLocation;
            string email = office.Email;
            Department department = office.Departments.Find(d => d.Name.Equals(formViewModel.SelectedDepartmentName));
            if (department != null)
            {
                if (!string.IsNullOrEmpty(department.SaveLocation))
                {
                    saveLocation = department.SaveLocation;
                }
                if (!string.IsNullOrEmpty(department.Email))
                {
                    email = department.Email;
                }
            }

            IList<FileInfo> uploadedFiles = MoveUploadedFiles(saveLocation, formViewModel, errors);

            Job job = CreateJob(officeName, formViewModel);
            try
            {
                _context.Add(job);
                await _context.SaveChangesAsync();
            }
            catch (Exception e)
            {
                _logger.LogError(string.Format("Job failed to persist: {0}", e.Message));
                errors.Add(string.Format("Job failed to persist: {0}", e.Message));
            }

            if (!string.IsNullOrEmpty(email))
            {
                SendMail(
                    _appSettings.SmtpSettings.FromAddress,
                    new List<string>() { email },
                    string.Format(
                        "File(s) Uploaded by {0}{1}{2}",
                        job.ContactName,
                        (string.IsNullOrEmpty(job.ContactCompanyName) ? string.Empty : string.Format(" ({0})", job.ContactCompanyName)),
                        (string.IsNullOrEmpty(job.DueDateTime) ? string.Empty : string.Format(" -- due: {0}", job.DueDateTime))),
                    GetMailMessageBody(saveLocation, job, formViewModel.UploadedFilenames, uploadedFiles, errors));
            }

            return RedirectToAction("FormSubmitted");
        }

        private IList<FileInfo> MoveUploadedFiles(string saveLocation, FormViewModel formViewModel, IList<string> errors)
        {
            string uploadDirectoryPath = Path.Combine(Path.Combine(_hostingEnvironment.WebRootPath, "Uploads"), formViewModel.ObjectContextId.ToString());
            IList<FileInfo> uploadedFiles = new List<FileInfo>();
            if (Directory.Exists(uploadDirectoryPath))
            {
                foreach (string uploadedFilename in formViewModel.UploadedFilenames)
                {
                    string uploadedFilePath = Path.Combine(uploadDirectoryPath, uploadedFilename);
                    if (System.IO.File.Exists(uploadedFilePath))
                    {
                        FileInfo fileInfo = new FileInfo(uploadedFilePath);
                        int? suffix = null;
                        string filenameWithoutExtension = Path.GetFileNameWithoutExtension(fileInfo.FullName);
                        string fileExtension = Path.GetExtension(fileInfo.FullName);
                        string fileUploadPath = Path.Combine(saveLocation, (filenameWithoutExtension + fileExtension));
                        while (System.IO.File.Exists(fileUploadPath))
                        {
                            if (!suffix.HasValue)
                            {
                                suffix = 1;
                            }
                            fileUploadPath = Path.Combine(saveLocation, (filenameWithoutExtension + "_" + suffix++.ToString() + fileExtension));
                        }
                        try
                        {
                            fileInfo.MoveTo(fileUploadPath);
                            uploadedFiles.Add(new FileInfo(fileUploadPath));
                        }
                        catch (Exception e)
                        {
                            _logger.LogError(string.Format("The uploaded file {0} cannot be moved: {1}", uploadedFilePath, e.Message));
                            errors.Add(string.Format("The uploaded file {0} cannot be moved: {1}", uploadedFilePath, e.Message));
                        }
                    }
                    else
                    {
                        _logger.LogError(string.Format("The uploaded file {0} cannot be found", uploadedFilePath));
                        errors.Add(string.Format("The uploaded file {0} cannot be found", uploadedFilePath));
                    }
                }
                try
                {
                    Directory.Delete(uploadDirectoryPath, true);
                }
                catch (Exception)
                {
                    _logger.LogError(string.Format("The uploaded directory {0} cannot be deleted", uploadDirectoryPath));
                    errors.Add(string.Format("The uploaded directory {0} cannot be deleted", uploadDirectoryPath));
                }
            }
            else
            {
                _logger.LogError(string.Format("The upload directory {0} does not exist", uploadDirectoryPath));
                errors.Add(string.Format("The upload directory {0} does not exist", uploadDirectoryPath));
            }
            return uploadedFiles;
        }

        private Job CreateJob(string officeName, FormViewModel formViewModel)
        {
            Job job = new Models.Job();
            job.OfficeName = officeName;
            job.DepartmentName = formViewModel.SelectedDepartmentName;
            job.DueDateTime = formViewModel.JobInformation.DueDateTime;
            job.AccountNumber = formViewModel.JobInformation.AccountNumber;
            job.ProjectNumber = formViewModel.JobInformation.ProjectNumber;
            job.Instructions = formViewModel.JobInformation.Instructions;
            job.ContactName = formViewModel.ContactInformation.Name;
            job.ContactCompanyName = formViewModel.ContactInformation.Company;
            job.ContactAddress = formViewModel.ContactInformation.Address;
            job.ContactAddressUnitNumber = formViewModel.ContactInformation.UnitNumber;
            job.ContactAddressCity = formViewModel.ContactInformation.City;
            job.ContactAddressProvince = formViewModel.ContactInformation.Province;
            job.ContactAddressPostalCode = formViewModel.ContactInformation.PostalCode;
            job.ContactEmail = formViewModel.ContactInformation.Email;
            job.ContactPhoneNumber = formViewModel.ContactInformation.PhoneNumber;
            return job;
        }

        private string GetMailMessageBody(string saveLocation, Job job, IList<string> originalUploadedFilePaths, IList<FileInfo> uploadedFiles, IList<string> errors)
        {
            string viewName = "FileSubmissionEmailTemplate";
            ViewData.Model = new JobEmailViewModel(saveLocation, job, originalUploadedFilePaths, uploadedFiles, errors);
            string mailMessageBody = string.Empty;
            using (StringWriter stringWriter = new StringWriter())
            {
                ICompositeViewEngine compositeViewEngine = _serviceProvider.GetService(typeof(ICompositeViewEngine)) as ICompositeViewEngine;
                ViewEngineResult viewEngineResult = compositeViewEngine.FindView(ControllerContext, viewName, false);
                ViewContext viewContext = new ViewContext(
                    ControllerContext,
                    viewEngineResult.View,
                    ViewData,
                    TempData,
                    stringWriter,
                    new Microsoft.AspNetCore.Mvc.ViewFeatures.HtmlHelperOptions());

                Task task = viewEngineResult.View.RenderAsync(viewContext);
                task.Wait();
                mailMessageBody = stringWriter.GetStringBuilder().ToString();
            }
            return mailMessageBody;
        }

        private void SendMail(string fromAddress, IList<string> toAddresses, string subject, string body)
        {
            try
            {
                using (SmtpClient smtpClient = new SmtpClient())
                {
                    smtpClient.UseDefaultCredentials = false;
                    smtpClient.Host = _appSettings.SmtpSettings.Host;
                    smtpClient.Port = _appSettings.SmtpSettings.Port;
                    smtpClient.EnableSsl = _appSettings.SmtpSettings.EnableSsl;
                    smtpClient.Credentials = new NetworkCredential(_appSettings.SmtpSettings.Username, _appSettings.SmtpSettings.Password);

                    MailMessage mailMessage = new MailMessage();
                    mailMessage.IsBodyHtml = true;
                    mailMessage.From = new MailAddress(_appSettings.SmtpSettings.FromAddress);
                    foreach (string toAddress in toAddresses)
                    {
                        mailMessage.To.Add(toAddress);
                    }
                    mailMessage.Subject = subject;
                    mailMessage.Body = body;
                    smtpClient.Send(mailMessage);
                }
            }
            catch (Exception e)
            {
                _logger.LogError(string.Format("Email failed to be sent to {0}: {1}", string.Join(", ", toAddresses.ToArray()), e.Message));
            }
        }
    }
}
