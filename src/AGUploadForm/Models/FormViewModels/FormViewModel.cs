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
        public IList<string> UploadedFilenames { get; set; }
        public bool BranchAndDepartmentSelected { get; set; } = false;

        //Field Settings used to store VIP field setup info
        public VIP VIPInfo { get; set; }
        //public VIPFieldSettingsModel VIPFieldSettings { get; }

        public void SetDropDowns(FormSettings formSettings, VIPSettings vipSettings, string vipQString)
        {
            VIP item = GetVIPByID(vipSettings, vipQString);
            SetDropDowns(formSettings, item);
        }

        public void SetDropDowns(FormSettings formSettings, VIP vip)
        {

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
            if (vip != null)
            {
                //Lookup and see if the Branch or Department fields are set, if so, configure the dropdowns appropriately
                Field BranchField;
                if (vip.Fields.Count > 0 &&
                    (BranchField = vip.Fields.Find(x => x.AGFieldId.Contains("Branch"))) != null)
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
                    Field DepartmentField = vip.Fields.Find(x => x.AGFieldId.Contains("Department"));
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

                        //DepartmentSelectList = new SelectList(Branch.Departments, "Name", "Name");
                    }
                }
            }
        }

        public VIP GetVIPByID(VIPSettings vipSettings, string VIPQString)
        {

            //set the VIP Field Options if a query string was passed in (easier to work with in the form)
            if (vipSettings != null && !String.IsNullOrEmpty(VIPQString))
            {
                VIP info = vipSettings.VIPs.Find(x => x.QueryStringCode.Contains(VIPQString));
                //VIPFieldSettings = FormViewModels.VIPFieldSettingsModel.GetSettings(vipSettings, VIPQstring);
                return info;
            }
            return null;
        }

        public FormViewModel(FormSettings formSettings, VIPSettings vipSettings = null, string vipQstring = "") // Default with no VIP, use VIPQString: "Value" if it exists when constructing
        {
            ObjectContextId = Guid.NewGuid();
            VIPInfo = GetVIPByID(vipSettings, vipQstring);
            SetDropDowns(formSettings, VIPInfo);
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
