using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace AGUploadForm.Models.FormViewModels
{
    public class ContactInformationViewModel
    {
        [Required]
        [Display(Name = "Name")]
        public string Name { get; set; }
        [Display(Name = "Company")]
        public string Company { get; set; }
        [Required]
        [Display(Name = "Address")]
        public string Address { get; set; }
        [Display(Name = "Unit Number")]
        public string UnitNumber { get; set; }
        [Display(Name = "City")]
        public string City { get; set; }
        [Display(Name = "Prov.")]
        public string Province { get; set; }
        [Display(Name = "Postal Code")]
        public string PostalCode { get; set; }
        [Required]
        [Display(Name = "Email")]
        public string Email { get; set; }
        [Required]
        [Display(Name = "Phone Number")]
        public string PhoneNumber { get; set; }
    }
}
