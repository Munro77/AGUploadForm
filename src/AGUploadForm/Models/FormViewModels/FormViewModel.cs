using AGUploadForm.Models.Settings;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace AGUploadForm.Models.FormViewModels
{
    public class FormViewModel
    {
        public Guid ObjectContextId { get; set; }
        [Required(ErrorMessage = "*")]
        [Display(Name = "Choose Printer Location")]
        public string SelectedOfficeName { get; set; }
        public SelectList OfficeSelectList { get; set; }

        [Required(ErrorMessage = "*")]
        [Display(Name = "Choose Department")]
        public string SelectedDepartmentName { get; set; }
        public SelectList DepartmentSelectList { get; set; }

        public JobInformationViewModel JobInformation { get; }
        public ContactInformationViewModel ContactInformation { get; }

        //Field Settings used to store VIP field setup info
        public VIP VIPInfo { get; }
        //public VIPFieldSettingsModel VIPFieldSettings { get; }

        public FormViewModel(FormSettings formSettings, VIPSettings vipSettings = null, string VIPQstring = "") // Default with no VIP, use VIPQString: "Value" if it exists when constructing
        {
            ObjectContextId = Guid.NewGuid();
            OfficeSelectList = new SelectList(formSettings.Offices, "Name", "Name");
            DepartmentSelectList = new SelectList(string.Empty, "Value", "Text");
            JobInformation = new JobInformationViewModel();
            ContactInformation = new ContactInformationViewModel();

            //set the VIP Field Options if a query string was passed in (easier to work with in the form)
            if (vipSettings != null && !String.IsNullOrEmpty(VIPQstring))
            {
                VIPInfo = vipSettings.VIPs.Find(x => x.QueryStringCode.Contains(VIPQstring));
                //VIPFieldSettings = FormViewModels.VIPFieldSettingsModel.GetSettings(vipSettings, VIPQstring);
            }
            
        }

        public FormViewModel()
        {
            JobInformation = new JobInformationViewModel();
            ContactInformation = new ContactInformationViewModel();
        }
    }
}
