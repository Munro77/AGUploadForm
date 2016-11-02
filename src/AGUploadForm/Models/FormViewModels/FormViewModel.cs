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
        [Required]
        [Display(Name = "Choose Printer Location")]
        public string SelectedOfficeName { get; set; }
        public SelectList OfficeSelectList { get; set; }

        [Required]
        [Display(Name = "Choose Department")]
        public string SelectedDepartmentName { get; set; }
        public SelectList DepartmentSelectList { get; set; }

        public JobInformationViewModel JobInformation { get; }
        public ContactInformationViewModel ContactInformation { get; }
        public IList<string> UploadedFilenames { get; set; }
        public bool BranchAndDepartmentSelected { get; set; } = false;

        public string VIPId { get; set; }
        public VIP Vip { get; set; }

        public void SetDropDowns(FormSettings formSettings, VIPSettings vipSettings = null, string vipQString = "")
        {
            Vip = null;
            //VIP item = GetVIPByID(vipSettings, vipQString);

            if (!string.IsNullOrEmpty(vipQString) && vipSettings != null)
            {
                Vip = vipSettings.GetVIPByID(vipQString);
            }

            //Create the OfficeSelectList
            //Not using the Razor tag helper way of having "Select One" as we don't want that to appear if we are pre-selecting the 
            //item (using the VIP settings)
            List<Office> items = formSettings.Offices.ToList();
            Office a = new Office() { Name = "Select One" };
            items.Insert(0, a);
            OfficeSelectList = new SelectList(items, "Name", "Name");
            OfficeSelectList.First().Value = ""; // Change the first item to have an empty value since we're using the name field as the value

            //Create an empty Department Selectlist with the Choose a Location choice, not using the razor tag helper so we can pre-select
            //it and not have the other value appear
            DepartmentSelectList = new SelectList(new List<SelectListItem> {
                        new SelectListItem { Selected = true, Text = "Choose a Location", Value = ""} }, "Value", "Text");


            //TODO:  Pre-select branch and populate/select the department if the VIP Settings choose them
            if (Vip != null)
            {
                //Lookup and see if the Branch or Department fields are set, if so, configure the dropdowns appropriately
                Field BranchField;
                if (Vip.Fields.Count > 0 &&
                    (BranchField = Vip.Fields.Find(x => x.AGFieldId.Contains("Branch"))) != null)
                {
                    Office Branch = formSettings.Offices.Find(x => x.Name.Contains(BranchField.Value));
                    if (Branch == null)
                    {
                        throw new Exception("VIP Settings, Branch set to name that doesn't exist in formsettings");
                    }

                    //Change the Office Select List to just be the specified branch and select it
                    OfficeSelectList = new SelectList(new List<SelectListItem> {
                        new SelectListItem { Selected = true, Text = BranchField.Value, Value = BranchField.Value } },
                        "Text",
                        "Value");

                    SelectedOfficeName = BranchField.Value;

                    //If the branch field has been set, then set department.  Department cannot be set unless branch field is set
                    Field DepartmentField = Vip.Fields.Find(x => x.AGFieldId.Contains("Department"));
                    if (DepartmentField != null)
                    {
                        DepartmentSelectList = new SelectList(new List<SelectListItem> {
                        new SelectListItem { Selected = true, Text = DepartmentField.Value, Value = DepartmentField.Value } }, "Text", "Value");
                        SelectedDepartmentName = DepartmentField.Value;
                        BranchAndDepartmentSelected = true;
                    }
                    else
                    {
                        //If the branch field is set and no department specified, need to populate the department field with the options
                        List<Department> list = Branch.Departments.ToList();
                        list.Insert(0, new Department() { Name = "Select One" });
                        DepartmentSelectList = new SelectList(list, "Name", "Name");
                        DepartmentSelectList.First().Value = "";
                    }
                }
            }
        }

        public FormViewModel(FormSettings formSettings, VIPSettings vipSettings = null, string vipQstring = "") // Default with no VIP, use VIPQString: "Value" if it exists when constructing
        {
            ObjectContextId = Guid.NewGuid();
            VIPId = vipQstring;
            if (!string.IsNullOrEmpty(vipQstring) && vipSettings != null)
            {
                Vip = vipSettings.GetVIPByID(vipQstring);
            }
            SetDropDowns(formSettings, vipSettings, vipQstring);
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
