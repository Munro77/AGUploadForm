using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace AGUploadForm.Models.FormViewModels
{
    public class JobEmailViewModel
    {
        public Job Job { get; set; }
        [Display(Name = "File(s) Uploaded")]
        public IList<string> UploadedFilePaths { get; set; }

        public JobEmailViewModel(Job job, IList<string> uploadedFilePaths)
        {
            Job = job;
            UploadedFilePaths = uploadedFilePaths;
        }
    }
}
