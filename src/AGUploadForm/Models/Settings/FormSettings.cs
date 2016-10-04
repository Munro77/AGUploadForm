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
    }

    public class Office
    {
        public string Name { get; set; }
        public int HoursStart { get; set; }
        public int HoursFinish { get; set; }
        public string SaveLocation { get; set; }
        public string Email { get; set; }
        public List<Department> Departments { get; set; }

    }

    //public class Offices
    //{
    //    public List<Office> Office { get; set; }
    //}
    
    public class FormSettings
    {
        public string Title { get; set; }
        public string Updates { get; set; }
        //public Offices Offices { get; set; }
        public List<Office> Offices { get; set; }
    }

}
