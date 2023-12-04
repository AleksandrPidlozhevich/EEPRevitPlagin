using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace EEPRevitPlagin.EEPRPCommandModules.WallFinishCreatorProperties
{
    public class WallFinishCreatorPropertiesSettings
    {
        public string FloorCreationOptionValue { get; set; }
        public static WallFinishCreatorPropertiesSettings GetSettings()
        {
            WallFinishCreatorPropertiesSettings wallFinishCreatorOptionsSettings = null;
            string assemblyPath = Path.Combine(Path.GetDirectoryName(typeof(App).Assembly.Location), "WallFinishCreatorOptionsSettings.xml");
            if (File.Exists(assemblyPath))
            {
                using (FileStream fs = new FileStream(assemblyPath, FileMode.Open))
                {
                    XmlSerializer xSer = new XmlSerializer(typeof(WallFinishCreatorPropertiesSettings));
                    wallFinishCreatorOptionsSettings = xSer.Deserialize(fs) as WallFinishCreatorPropertiesSettings;
                    fs.Close();
                }
            }
            else
            {
                wallFinishCreatorOptionsSettings = new WallFinishCreatorPropertiesSettings();
            }

            return wallFinishCreatorOptionsSettings;
        }

        public void SaveSettings()
        {
            string assemblyPath = Path.Combine(Path.GetDirectoryName(typeof(App).Assembly.Location), "WallFinishCreatorOptionsSettings.xml");

            if (File.Exists(assemblyPath))
            {
                File.Delete(assemblyPath);
            }

            using (FileStream fs = new FileStream(assemblyPath, FileMode.Create))
            {
                XmlSerializer xSer = new XmlSerializer(typeof(WallFinishCreatorPropertiesSettings));
                xSer.Serialize(fs, this);
                fs.Close();
            }
        }
    }
}