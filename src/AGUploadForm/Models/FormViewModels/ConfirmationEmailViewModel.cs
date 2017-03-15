using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace AGUploadForm.Models.FormViewModels
{
    public class ConfirmationEmailViewModel
    {
        public Job Job { get; set; }
        [Display(Name = "File(s) Uploaded")]
        public IList<FileInfo> UploadedFiles { get; set; }
        public IList<string> Errors { get; set; }
        public string UploadDirectoryPath { get; set; }
        public IList<string> OriginalUploadedFilePaths { get; set; }
        public IList<string> SaveEmailAliasList { get; set; }
        public bool ReRouted { get; }

        public string FormatToHumanReadableFileSize(object value)
        {
            try
            {
                string[] suffixNames = { "bytes", "KB", "MB", "GB", "TB", "PB", "EB", "ZB", "YB" };
                var counter = 0;
                decimal dValue = 0;
                Decimal.TryParse(value.ToString(), out dValue);
                while (Math.Round(dValue / 1024) >= 1)
                {
                    dValue /= 1024;
                    counter++;
                }

                return string.Format("{0:n1} {1}", dValue, suffixNames[counter]);
            }
            catch (Exception)
            {
                //catch and handle the exception
                return string.Empty;
            }
        }

        public ConfirmationEmailViewModel(
            string uploadDirectoryPath, 
            IList<string> saveEmailAliasList,
            Job job,
            IList<string> originalUploadedFilePaths, 
            IList<FileInfo> uploadedFiles, 
            IList<string> errors,
            bool reRouted = false)
        {
            UploadDirectoryPath = uploadDirectoryPath;
            SaveEmailAliasList = saveEmailAliasList;
            Job = job;
            OriginalUploadedFilePaths = ((originalUploadedFilePaths == null) ? new List<string>() : originalUploadedFilePaths);
            UploadedFiles = ((uploadedFiles == null) ? new List<FileInfo>() : uploadedFiles);
            Errors = ((errors == null) ? new List<string>() : errors);
            ReRouted = reRouted;
        }
    }
}
