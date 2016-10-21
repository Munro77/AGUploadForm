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

namespace AGUploadForm.Controllers
{
    public class HomeController : Controller
    {

        private readonly FormSettings _settings;
        private readonly ApplicationDbContext _context;

        public HomeController(IOptions<FormSettings> settingsOptions, ApplicationDbContext context)
        {
            _settings = settingsOptions.Value;
            _context = context;
            
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
        public async Task<IActionResult> Submit(FormViewModel formViewModel)
        {
            // Use FallbackOffice if outside office hours
            Office office = _settings.Offices.Find(o => o.Name.Equals(formViewModel.SelectedOfficeName));
            int currentHour = DateTime.Now.Hour;
            string officeName = formViewModel.SelectedOfficeName;
            if (currentHour < office.HoursStart || currentHour >= office.HoursFinish)
            {
                officeName = office.FallbackOfficeName;
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
