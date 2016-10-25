using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AGUploadForm.Models.Settings
{
    public class VIP
    {
        public string Name { get; set; }
        public string QueryStringCode { get; set; }
        public List<Field> Fields { get; set; }
    }

    public class Field
    {
        public string ID { get; set; }
        public List<Option> Options { get; set; }
    }

    public class Option
    {
        public string Value { get; set; }
        public bool Editable { get; set; } = true;
        public bool Shown { get; set; } = true;
    }

    public class VIPSettings
    {
        public List<VIP> VIPs { get; set; }
    }

}
