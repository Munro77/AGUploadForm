using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AGUploadForm.Models
{
    public class Job
    {
        public int ID { get; set; }
        public string OfficeName { get; set; }
        public string DepartmentName { get; set; }
        public DateTime DueDateTime { get; set; }
        public string AccountNumber { get; set; }
        public string ProjectNumber { get; set; }
        public string Instructions { get; set; }
        public string ContactName { get; set; }
        public string ContactCompanyName { get; set; }
        public string ContactAddress { get; set; }
        public string ContactEmail { get; set; }
        public string ContactPhoneNumber { get; set; }
        public DateTime Created { get; protected set; }

        public Job()
        {
            Created = DateTime.Now;
        }
    }
}
