﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace AGUploadForm.Models.FormViewModels
{
    public class JobInformationViewModel
    {
        [Required]
        [Display(Name = "Due Date/Time")]
        public string DueDateTime { get; set; }
        [Display(Name = "Account No.")]
        public string AccountNumber { get; set; }
        [Display(Name = "Project/PO No.")]
        public string ProjectNumber { get; set; }
        [Required]
        [Display(Name = "Instructions")]
        public string Instructions { get; set; }
    }
}
