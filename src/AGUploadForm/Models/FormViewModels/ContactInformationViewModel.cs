using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace AGUploadForm.Models.FormViewModels
{
    public class ContactInformationViewModel
    {
        [Required(ErrorMessage = "*")]
        [Display(Name = "Name")]
        public string Name { get; set; }
        [Display(Name = "Company")]
        public string Company { get; set; }
        [Required(ErrorMessage = "*")]
        [Display(Name = "Address")]
        public string Address { get; set; }
        [Required(ErrorMessage = "*")]
        [Display(Name = "Email")]
        public string Email { get; set; }
        [Required(ErrorMessage = "*")]
        [Display(Name = "Phone Number")]
        public string PhoneNumber { get; set; }
    }
}
