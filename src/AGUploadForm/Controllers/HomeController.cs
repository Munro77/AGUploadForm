using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
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
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Http;

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
            if (Request.Cookies != null && Request.Cookies.Keys.Contains("RememberLocation"))
            {
                formViewModel.RememberLocation = true;
                if (string.IsNullOrEmpty(formViewModel.SelectedOfficeName))
                {
                    formViewModel.SelectedOfficeName = Request.Cookies["RememberLocation"];
                }
                if (!string.IsNullOrEmpty(formViewModel.SelectedOfficeName) && string.IsNullOrEmpty(formViewModel.SelectedDepartmentName))
                {
                    List<SelectListItem> departmentSelectList = new List<SelectListItem>();
                    departmentSelectList.Add(new SelectListItem() { Value = string.Empty, Text = "Select One" });
                    foreach (Department department in _settings.Offices.Find(o => o.Name.Equals(formViewModel.SelectedOfficeName)).Departments.ToList())
                    {
                        departmentSelectList.Add(new SelectListItem() { Value = department.Name, Text = department.Name });
                    }
                    formViewModel.DepartmentSelectList = new SelectList(departmentSelectList, "Value", "Text");
                    formViewModel.SelectedDepartmentName = Request.Cookies["RememberDepartment"];
                }
            }
            if (Request.Cookies != null && Request.Cookies.Keys.Contains("RememberDepartment"))
            {
                formViewModel.RememberDepartment = true;
                if (!string.IsNullOrEmpty(formViewModel.SelectedOfficeName) && string.IsNullOrEmpty(formViewModel.SelectedDepartmentName))
                {
                    formViewModel.SelectedDepartmentName = Request.Cookies["RememberDepartment"];
                }
            }
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
            CookieOptions cookieOptions = new CookieOptions()
            {
                Expires = DateTime.Now.AddDays(30)
            };

            if (formViewModel.RememberLocation)
            {
                Response.Cookies.Append(
                    "RememberLocation", 
                    (string.IsNullOrEmpty(formViewModel.SelectedOfficeName) ? string.Empty : formViewModel.SelectedOfficeName), 
                    cookieOptions);
            }
            else
            {
                Response.Cookies.Delete("RememberLocation", cookieOptions);
            }
            if (formViewModel.RememberDepartment)
            {
                Response.Cookies.Append(
                    "RememberDepartment", 
                    (string.IsNullOrEmpty(formViewModel.SelectedDepartmentName) ? string.Empty : formViewModel.SelectedDepartmentName), 
                    cookieOptions);
            }
            else
            {
                Response.Cookies.Delete("RememberDepartment", cookieOptions);
            }

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

            Office office = null;
            Department department = null;

            bool reRouted; // track if the fallback location used
            GetOfficeDepartment(formViewModel.SelectedOfficeName, formViewModel.SelectedDepartmentName, out office, out department, out reRouted);

            string saveLocation = office.SaveLocation;
            string saveEmailAlias = office.SaveEmailAlias;

            List<string> departmentEmails = new List<string>(office.Email.Split(';'));
            if (department != null)
            {
                if (!string.IsNullOrEmpty(department.SaveLocation))
                {
                    saveLocation = department.SaveLocation;
                }
                if (!string.IsNullOrEmpty(department.SaveEmailAlias))
                {
                    saveEmailAlias = department.SaveEmailAlias;
                }
                if (!string.IsNullOrEmpty(department.Email))
                {
                    departmentEmails = new List<string>(department.Email.Split(';'));
                }
            }

            if (!string.IsNullOrEmpty(id) && _vipsettings != null)
            {
                VIP vip = _vipsettings.GetVIPByID(id);
                if ((vip != null) && !string.IsNullOrEmpty(vip.CcEmail))
                {
                    departmentEmails.AddRange(vip.CcEmail.Split(';'));
                }
            }

            string dateTimeStamp = DateTime.Now.ToString("yyyy-MM-dd--hh-mm-ss.fff");
            saveLocation = Path.Combine(saveLocation, dateTimeStamp);

            //Add multiple Email Alias's
            List<string> saveEmailAliasList = new List<string>();
            foreach(string alias in saveEmailAlias.Split(';'))
            {
                UriBuilder pathBuilder = new UriBuilder(alias);
                pathBuilder.Path += "/" + dateTimeStamp;
                if(pathBuilder.Uri.IsUnc)
                    saveEmailAliasList.Add(pathBuilder.Uri.LocalPath);
                else
                    saveEmailAliasList.Add(pathBuilder.Uri.ToString());
            }
            //saveEmailAlias = Path.Combine(saveEmailAlias, dateTimeStamp);

            IList<FileInfo> uploadedFiles = MoveUploadedFiles(saveLocation, formViewModel, errors);

            Job job = CreateJob(office.Name, department.Name, formViewModel);
            try
            {
                _context.Add(job);
                await _context.SaveChangesAsync();
            }
            catch (Exception e)
            {
                LogError(string.Format("Job failed to persist: {0}", e.Message), formViewModel, errors);
            }

            //TODO: Try/Catch this in case the folder cannot be reached...
            try
            {
                using (StreamWriter streamWriter = new StreamWriter(Path.Combine(saveLocation, "!JOB_INSTRUCTIONS!.txt")))
                {
                    streamWriter.Write(
                        GetInstructionMessageBody(
                            saveLocation,
                            saveEmailAliasList,
                            job,
                            formViewModel.UploadedFilenames,
                            uploadedFiles, errors));
                }
            }
            catch (Exception e)
            {
                LogError(string.Format("Unable to write instructions to saveLocation: {0}.  Error Message: {1}", saveLocation, e.Message), formViewModel, errors);
                //throw (e);
            }

            //string jobSubjectLine = createJobSubjectLine(formViewModel.SelectedOfficeName, formViewModel.SelectedDepartmentName, job );
            string jobSubjectLine = createJobSubjectLine(formViewModel.SelectedOfficeName, formViewModel.SelectedDepartmentName, job, reRouted );
            string confirmSubjectLine = createConfirmSubjectLine();

            //Send job and confirmation emails
            if ((departmentEmails != null) && (departmentEmails.Count > 0))
            {
                SendMail(
                    _appSettings.SmtpSettings.FromAddress,
                    departmentEmails,
                    jobSubjectLine,
                    GetJobMailMessageBody(saveLocation, saveEmailAliasList, job, formViewModel.UploadedFilenames, uploadedFiles, errors));

                //Send the confirmation email
                if ((!String.IsNullOrEmpty(job.ContactEmail)))
                {
                    //Choose the first department email as the sender for the confirmation
                    string fromEmail = departmentEmails.First();
                    string fromName = string.Format("{0} {1}", office.Name, department.Name);

                    SendMail(
                        fromEmail,
                        job.ContactEmail,
                        confirmSubjectLine,
                        GetConfirmationMailMessageBody(saveLocation, saveEmailAliasList, job, formViewModel.UploadedFilenames, uploadedFiles, errors, reRouted),
                        fromName);

                }
            }

            return RedirectToAction("FormSubmitted");
        }

        /// <summary>
        /// Creates the subject line for the job submission email
        /// </summary>
        /// <param name="selectedOffice"></param>
        /// <param name="selectedDepartment"></param>
        /// <param name="destinationOffice"></param>
        /// <param name="job"></param>
        /// <returns></returns>
        private string createJobSubjectLine(string selectedOffice, string selectedDepartment, Job job, bool reRouted)
        {
            string subjectLine = string.Format(
                        "File(s) Uploaded by {0}{1}{2}",
                        job.ContactName,
                        (string.IsNullOrEmpty(job.ContactCompanyName) ? string.Empty : string.Format(" ({0})", job.ContactCompanyName)),
                        (string.IsNullOrEmpty(job.DueDateTime) ? string.Empty : string.Format(" -- due: {0}", job.DueDateTime)));

            //Append some details if the job was forwarded (during "fallback" hours)
            if (reRouted)
                subjectLine = string.Format("[From {0}: {1}] {2}", selectedOffice, selectedDepartment, subjectLine);

            return subjectLine;
        }

        /// <summary>
        /// Creates the subject line for the job submission email
        /// </summary>
        /// <returns></returns>
        private string createConfirmSubjectLine()
        {
            return @"Astley Gilbert has received your files!";
        }
        
        /// <summary>
        /// Gets the office and department objects to send the request to based on the selected office and department name.  Depends on the time and the fallback periods in the configuration.
        /// </summary>
        /// <param name="officeName"></param>
        /// <param name="departmentName"></param>
        /// <param name="office"></param>
        /// <param name="department"></param>
        private void GetOfficeDepartment(string officeName, string departmentName, out Office office, out Department department, out bool reRouted)
        {
            office = _settings.Offices.Find(o => o.Name.Equals(officeName));
            department = null;
            int currentHour = DateTime.Now.Hour;
            reRouted = false;

            TimeSpan currentTime = DateTime.Now.TimeOfDay;
            string[] formats = { "%H", "%H:mm" };
            string start = office.FallbackHoursStart.ToString();
            string finish = office.FallbackHoursFinish.ToString();
            if (finish == "24") finish = "23:59";
            DateTime fallBackStart;
            DateTime.TryParseExact(start, formats, System.Globalization.CultureInfo.CurrentCulture, System.Globalization.DateTimeStyles.None, out fallBackStart);
            DateTime fallBackFinish;
            DateTime.TryParseExact(finish, formats, System.Globalization.CultureInfo.CurrentCulture, System.Globalization.DateTimeStyles.None, out fallBackFinish);

            if (office != null)
            {
                string fallbackOfficeName = office.FallbackOfficeName;
                //CompareTo results:
                //Less than zero
                //This instance is earlier than value.
                //Zero
                //This instance is the same as value.
                //Greater than zero
                //This instance is later than value.

                //First determine if the start time is later than the finish - for periods that span midnight
                bool isStartLaterThanFinish = fallBackStart.TimeOfDay.CompareTo(fallBackFinish.TimeOfDay) > 0;

                if (fallBackStart.TimeOfDay.CompareTo(currentTime) < 0 && fallBackFinish.TimeOfDay.CompareTo(currentTime) >= 0)
                {
                    //The current time is greater than the start and less than the finish
                    office = _settings.Offices.Find(o => o.Name.Equals(fallbackOfficeName));
                    reRouted = true;
                }
                else if (isStartLaterThanFinish &&
                    (fallBackStart.TimeOfDay.CompareTo(currentTime) < 0 ||
                    fallBackFinish.TimeOfDay.CompareTo(currentTime) >= 0)
                    )
                {
                    //The current time is greater than the start OR less than the finish, if the start/finish spans midnight (into the AM for example)
                    office = _settings.Offices.Find(o => o.Name.Equals(fallbackOfficeName));
                    reRouted = true;
                }

                //Get the department that matches or the default of the selected office
                department = office.Departments.Find(d => d.Name.Equals(departmentName));
                if (department == null)
                {
                    department = office.Departments.First(d => d.Default);
                }
            }

        }

        /// <summary>
        /// Logger for errors.  Can decide to include the job details or not.
        /// </summary>
        /// <param name="error"></param>
        /// <param name="formViewModel"></param>
        /// <param name="errors"></param>
        private void LogError(string error, FormViewModel formViewModel = null, IList<string> errors = null)
        {
            string errorToLog = error;
            if (formViewModel != null)
            {
                errorToLog = string.Format(error + Environment.NewLine + "Job Details:" + Environment.NewLine + "{0}", JsonConvert.SerializeObject(formViewModel, Formatting.Indented));
            }

            _logger.LogError(errorToLog);

            if (errors != null)
                errors.Add(errorToLog);
        }
        
        private IList<FileInfo> MoveUploadedFiles(string saveLocation, FormViewModel formViewModel, IList<string> errors)
        {
            //Get the source directory from Backload, not hardcoded
            Backload.Contracts.FileHandler.IFileHandler handler = Backload.FileHandler.Create();
            handler.Init(HttpContext,_hostingEnvironment);
            string uploadDirectoryPath = Path.Combine(handler.BasicStorageInfo.FileDirectory, formViewModel.ObjectContextId.ToString());            
           
            //string uploadDirectoryPath = Path.Combine(Path.Combine(_hostingEnvironment.WebRootPath, "Uploads"), formViewModel.ObjectContextId.ToString());
            IList<FileInfo> uploadedFiles = new List<FileInfo>();
            if (Directory.Exists(uploadDirectoryPath))
            {
                string uploadedFilePath;

                foreach (string uploadedFilename in formViewModel.UploadedFilenames)
                {
                    //Titles are escaped in the filesubmission javascript so must be URL decoded here to replce %20 with spaces etc.

                    //Have to decode twice, since we're double encoding in the .js.  (Once for the + signs and other legit URL characters, and once for javascript).  
                    uploadedFilePath = Path.Combine(uploadDirectoryPath, WebUtility.UrlDecode(WebUtility.UrlDecode(uploadedFilename)));
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

                        //Check if the path exists, and if not, create it
                        FileInfo droppedFile = new FileInfo(fileUploadPath);
                        if (!droppedFile.Directory.Exists)
                            try
                            {
                                droppedFile.Directory.Create();
                            }
                            catch (Exception e)
                            {
                                LogError(string.Format("Path {0} does not exist and cannot be created. Error: {1}.", droppedFile.Directory, e.Message), formViewModel, errors);
                            }

                        try
                        {
                            fileInfo.MoveTo(fileUploadPath);
                            droppedFile.Refresh();
                            uploadedFiles.Add(droppedFile);
                        }
                        catch (Exception e)
                        {
                            LogError(string.Format("The uploaded file {0} cannot be moved: {1}", droppedFile.Directory, e.Message), formViewModel, errors);
                        }
                    }
                    else
                    {
                        LogError(string.Format("The uploaded file {0} cannot be found", uploadedFilePath), formViewModel, errors);
                    }
                }
                try
                {
                    Directory.Delete(uploadDirectoryPath, true);
                }
                catch (Exception)
                {
                    LogError(string.Format("The upload directory {0} cannot be deleted", uploadDirectoryPath), formViewModel, errors);
                }
            }
            else
            {
                LogError(string.Format("The upload directory {0} does not exist", uploadDirectoryPath), formViewModel, errors);
            }
            return uploadedFiles;
        }

        private Job CreateJob(string officeName, string departmentName, FormViewModel formViewModel)
        {
            Job job = new Models.Job();
            job.OfficeName = officeName;
            job.DepartmentName = departmentName;
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

        private string GetInstructionMessageBody(string saveLocation, IList<string>saveEmailAliasList, Job job, IList<string> originalUploadedFilePaths, IList<FileInfo> uploadedFiles, IList<string> errors)
        {
            string viewName = "FileSubmissionInstructionTemplate";
            ViewData.Model = new JobEmailViewModel(saveLocation, saveEmailAliasList, job, originalUploadedFilePaths, uploadedFiles, errors);
            string instructionMessageBody = string.Empty;
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
                instructionMessageBody = stringWriter.GetStringBuilder().ToString();
            }
            return instructionMessageBody;
        }

        /// <summary>
        /// Generate the Confirmation email body
        /// </summary>
        /// <param name="saveLocation"></param>
        /// <param name="saveEmailAliasList"></param>
        /// <param name="job"></param>
        /// <param name="originalUploadedFilePaths"></param>
        /// <param name="uploadedFiles"></param>
        /// <param name="errors"></param>
        /// <returns></returns>
        private string GetConfirmationMailMessageBody(string saveLocation, IList<string> saveEmailAliasList, Job job, IList<string> originalUploadedFilePaths, IList<FileInfo> uploadedFiles, IList<string> errors, bool reRouted)
        {
            string viewName = "FileConfirmationEmailTemplate";
            var model = new ConfirmationEmailViewModel(saveLocation, saveEmailAliasList, job, originalUploadedFilePaths, uploadedFiles, errors, reRouted);
            //return GetMailMessageBody(viewName, saveLocation, saveEmailAliasList, job, originalUploadedFilePaths, uploadedFiles, errors);
            return GetMailMessageBody(viewName, model);

        }

        /// <summary>
        /// Generate the job submission email body
        /// </summary>
        /// <param name="saveLocation"></param>
        /// <param name="saveEmailAliasList"></param>
        /// <param name="job"></param>
        /// <param name="originalUploadedFilePaths"></param>
        /// <param name="uploadedFiles"></param>
        /// <param name="errors"></param>
        /// <returns></returns>
        private string GetJobMailMessageBody(string saveLocation, IList<string> saveEmailAliasList, Job job, IList<string> originalUploadedFilePaths, IList<FileInfo> uploadedFiles, IList<string> errors)
        {
            string viewName = "FileSubmissionEmailTemplate";
            var model = new JobEmailViewModel(saveLocation, saveEmailAliasList, job, originalUploadedFilePaths, uploadedFiles, errors);
            //return GetMailMessageBody(viewName, saveLocation, saveEmailAliasList, job, originalUploadedFilePaths, uploadedFiles, errors);
            return GetMailMessageBody(viewName, model);
        }

        /// <summary>
        /// Generates a message body based on a view and the necessary job information
        /// </summary>
        /// <param name="viewName"></param>
        /// <param name="saveLocation"></param>
        /// <param name="saveEmailAliasList"></param>
        /// <param name="job"></param>
        /// <param name="originalUploadedFilePaths"></param>
        /// <param name="uploadedFiles"></param>
        /// <param name="errors"></param>
        /// <returns></returns>
        //private string GetMailMessageBody(string viewName, string saveLocation, IList<string> saveEmailAliasList, Job job, IList<string> originalUploadedFilePaths, IList<FileInfo> uploadedFiles, IList<string> errors)
        //{
        //    ViewData.Model = new JobEmailViewModel(saveLocation, saveEmailAliasList, job, originalUploadedFilePaths, uploadedFiles, errors);
        //    string mailMessageBody = string.Empty;
        //    using (StringWriter stringWriter = new StringWriter())
        //    {
        //        ICompositeViewEngine compositeViewEngine = _serviceProvider.GetService(typeof(ICompositeViewEngine)) as ICompositeViewEngine;
        //        ViewEngineResult viewEngineResult = compositeViewEngine.FindView(ControllerContext, viewName, false);
        //        ViewContext viewContext = new ViewContext(
        //            ControllerContext,
        //            viewEngineResult.View,
        //            ViewData,
        //            TempData,
        //            stringWriter,
        //            new Microsoft.AspNetCore.Mvc.ViewFeatures.HtmlHelperOptions());

        //        Task task = viewEngineResult.View.RenderAsync(viewContext);
        //        task.Wait();
        //        mailMessageBody = stringWriter.GetStringBuilder().ToString();
        //    }
        //    return mailMessageBody;
        //}

        /// <summary>
        /// Generates a message body based on a view and a model to match the view
        /// </summary>
        /// <returns></returns>
        private string GetMailMessageBody(string viewName, object model)
        {
            ViewData.Model = model;
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

        private void SendMail(string fromAddress, string toAddress, string subject, string body, string fromName=null)
        {
            List<string> toAddresses = new List<string>{toAddress};
            SendMail(fromAddress, toAddresses, subject, body, fromName);
        }

        private void SendMail(string fromAddress, IList<string> toAddresses, string subject, string body, string fromName=null)
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
                    if (string.IsNullOrEmpty(fromName))
                    {
                        mailMessage.From = new MailAddress(fromAddress);
                    }
                    else
                    {
                        mailMessage.From = new MailAddress(fromAddress, fromName);
                    }
                    foreach (string toAddress in toAddresses)
                    {
                        if (!string.IsNullOrWhiteSpace(toAddress))
                        {
                            mailMessage.To.Add(toAddress);
                        }
                    }
                    mailMessage.Subject = subject;
                    mailMessage.Body = body;
                    smtpClient.Send(mailMessage);
                }
            }
            catch (Exception e)
            {
                _logger.LogError(string.Format("Email failed to be sent. Error: {0}" + Environment.NewLine + "From: {1} " + Environment.NewLine + "To: {2} " + Environment.NewLine + "Body: {3}", 
                    e.Message,
                    fromAddress,
                    string.Join(", ", toAddresses.ToArray()),
                    body));
            }
        }
    }
}
