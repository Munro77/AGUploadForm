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

namespace AGUploadForm.Controllers
{
    public class HomeController : Controller
    {
        private readonly FormSettings _settings;
        private readonly ApplicationDbContext _context;
        private readonly IHostingEnvironment _hostingEnvironment;

        public HomeController(IOptions<FormSettings> settingsOptions, ApplicationDbContext context, IHostingEnvironment hostingEnvironment)
        {
            _settings = settingsOptions.Value;
            _context = context;
            _hostingEnvironment = hostingEnvironment;

            //string output = JsonConvert.SerializeObject(_settings);

        }

        public IActionResult Index()
        {
            //Test the data from the config file
            /*ViewData["Title"] = _settings.Title;
            ViewData["Updates"] = _settings.Updates;
            _settings.Offices.ForEach(x => { ViewData["Office"] = x.Name; });
            ViewData["Office"] += " AND dept email = " + _settings.Offices[0].Departments[0].Email;*/
            return View(new FormViewModel(_settings));
        }

        public IActionResult About()
        {
            ViewData["Message"] = "Your application description page.";

            return View();
        }

        public IActionResult Contact()
        {
            ViewData["Message"] = "Your contact page.";

            return View();
        }

        public IActionResult Error()
        {
            return View();
        }

        // jQuery UI theme for Bootloader - copied directly from Demo
        public IActionResult JQueryUI()
        {
            return View();
        }

        // BasicPlusUI theme for Bootloader - copied directly from Demo
        public IActionResult BasicPlusUI()
        {
            return View();
        }

        //A plain version of the BlueImp plugin wired up using packaging
        public IActionResult PlainBlueImp()
        {
            return View();
        }

        public JsonResult GetDepartmentSelectListByOfficeName(string officeName)
        {
            return Json(new SelectList(_settings.Offices.Find(o => o.Name.Equals(officeName)).Departments.ToList(), "Name", "Name"));
        }

        [HttpPost]
        public async Task<IActionResult> Index(FormViewModel formViewModel)
        {
            if (!ModelState.IsValid)
            {
                formViewModel.OfficeSelectList = new SelectList(_settings.Offices, "Name", "Name");
                if (string.IsNullOrEmpty(formViewModel.SelectedOfficeName))
                {
                    formViewModel.DepartmentSelectList = new SelectList(string.Empty, "Name", "Name");
                }
                else
                {
                    formViewModel.DepartmentSelectList = new SelectList(_settings.Offices.Find(o => o.Name.Equals(formViewModel.SelectedOfficeName)).Departments, "Name", "Name");
                }
                return View(formViewModel);
            }

            // Use FallbackOffice if outside office hours
            string officeName = formViewModel.SelectedOfficeName;
            Office office = _settings.Offices.Find(o => o.Name.Equals(officeName));
            int currentHour = DateTime.Now.Hour;
            if (currentHour < office.HoursStart || currentHour >= office.HoursFinish)
            {
                officeName = office.FallbackOfficeName;
                office = _settings.Offices.Find(o => o.Name.Equals(officeName));
            }

            // TODO: Replace Hardocded with Upload Path Lookup
            string uploadPath = Path.Combine(Path.Combine(_hostingEnvironment.WebRootPath, "Uploads"), formViewModel.ObjectContextId.ToString());
            string saveLocation = office.SaveLocation;
            Department department = office.Departments.Find(d => d.Name.Equals(formViewModel.SelectedDepartmentName));
            if (department != null)
            {
                saveLocation = department.SaveLocation;
            }
            if (Directory.Exists(uploadPath))
            {
                DirectoryInfo directoryInfo = new DirectoryInfo(uploadPath);
                foreach (FileInfo fileInfo in directoryInfo.GetFiles())
                {
                    fileInfo.MoveTo(Path.Combine(office.SaveLocation, fileInfo.Name));
                }
                directoryInfo.Delete(true);
            }

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
            job.ContactEmail = formViewModel.ContactInformation.Email;
            job.ContactPhoneNumber = formViewModel.ContactInformation.PhoneNumber;

            _context.Add(job);
            await _context.SaveChangesAsync();

            return RedirectToAction("About");
        }
    }
}
