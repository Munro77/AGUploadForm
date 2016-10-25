using AGUploadForm.Models.Settings;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace AGUploadForm.Models.FormViewModels
{
    public class VIPFieldSettingsModel
    {
        //Using a static creator as the settings should be null if the lookup fails
        //Should use a factory but this is pretty small

        private VIPFieldSettingsModel(List<Field> _fields)
        {
            //Assign the looked up values
            Fields = _fields;
        }

        //Lookup the field and other settings based on the passed in lookup string
        public static VIPFieldSettingsModel GetSettings(VIPSettings _settings, string VIPLookupString)
        {
            VIP a;
            if ((a = _settings.VIPs.Find(x => x.QueryStringCode.Contains(VIPLookupString))) != null)
            {
                return new VIPFieldSettingsModel(a.Fields);
            }
            else return null;
        }

        public List<Field> Fields;
    }
}
