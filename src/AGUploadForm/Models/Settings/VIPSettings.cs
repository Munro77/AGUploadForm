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
        public string AGFieldId { get; set; }
        public string Value { get; set; }
        public bool Disabled { get; set; } = true;
        public bool Visible { get; set; } = true;
    }

    public class VIPSettings
    {
        public List<VIP> VIPs { get; set; }
    }

}
