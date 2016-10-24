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

        public FormViewModel(FormSettings formSettings)
        {
            ObjectContextId = Guid.NewGuid();
            OfficeSelectList = new SelectList(formSettings.Offices, "Name", "Name");
            DepartmentSelectList = new SelectList(string.Empty, "Value", "Text");
            JobInformation = new JobInformationViewModel();
            ContactInformation = new ContactInformationViewModel();
        }

        public FormViewModel()
        {
            JobInformation = new JobInformationViewModel();
            ContactInformation = new ContactInformationViewModel();
        }
    }
}
