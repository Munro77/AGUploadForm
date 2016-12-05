using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AGUploadForm.Models.Settings
{
    public class Department
    {
        public string Name { get; set; }
        public string Email { get; set; }
        public string SaveLocation { get; set; }
        public string SaveEmailAlias { get; set; }
        public bool Default { get; set; }
    }

    public class Office
    {
        public string Name { get; set; }
        public int FallbackHoursStart { get; set; }
        public int FallbackHoursFinish { get; set; }
        public string SaveLocation { get; set; }
        public string SaveEmailAlias { get; set; }
        public string Email { get; set; }
        public List<Department> Departments { get; set; }
        public string FallbackOfficeName { get; set; }

    }

    public class FormSettings
    {
        public List<Office> Offices { get; set; }
    }

}
