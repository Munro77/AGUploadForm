using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
//using Newtonsoft.Json;
using Microsoft.AspNetCore.Mvc;
using AGUploadForm.Models;
using AGUploadForm.Models.Settings;
using Microsoft.Extensions.Options;

namespace AGUploadForm.Controllers
{
    public class HomeController : Controller
    {

        private readonly FormSettings _settings;

        public HomeController(IOptions<FormSettings> settingsOptions)
        {
            _settings = settingsOptions.Value;
            
            //string output = JsonConvert.SerializeObject(_settings);

        }

        public IActionResult Index()
        {
            //Test the data from the config file
            /*ViewData["Title"] = _settings.Title;
            ViewData["Updates"] = _settings.Updates;
            _settings.Offices.ForEach(x => { ViewData["Office"] = x.Name; });
            ViewData["Office"] += " AND dept email = " + _settings.Offices[0].Departments[0].Email;*/
            return View();
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

    }
}
