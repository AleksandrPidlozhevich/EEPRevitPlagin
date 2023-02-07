using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace EEPRevitPlagin.EEPRPCommandModules.InstallationDoorWindowLintels
{
    public class LintelCreatorSettings
    {
        public string SelectedLintelFamilieName { get; set; }
        public string SelectedFamilySymbolName { get; set; }
        public string SelectedOpeningHeightParameterName { get; set; }
        public string SelectedOpeningWidthParameterName { get; set; }

        public static LintelCreatorSettings GetSettings()
        {
            LintelCreatorSettings lintelCreatorSettings = null;
            string fileName = "LintelCreatorSettings.xml";
            string assemblyDirectory = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
            string filePath = Path.Combine(assemblyDirectory, "EEPRPCommandModules\\InstallationDoorWindowLintels", fileName);

            if (File.Exists(filePath))
            {
                using (FileStream fs = new FileStream(filePath, FileMode.Open))
                {
                    XmlSerializer xSer = new XmlSerializer(typeof(LintelCreatorSettings));
                    lintelCreatorSettings = xSer.Deserialize(fs) as LintelCreatorSettings;
                    fs.Close();
                }
            }
            else
            {
                lintelCreatorSettings = new LintelCreatorSettings();
            }

            return lintelCreatorSettings;
        }

        public void Save()
        {
            string fileName = "LintelCreatorSettings.xml";
            string assemblyDirectory = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
            string filePath = Path.Combine(assemblyDirectory, "EEPRPCommandModules\\InstallationDoorWindowLintels", fileName);

            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }

            using (FileStream fs = new FileStream(filePath, FileMode.Create))
            {
                XmlSerializer xSer = new XmlSerializer(typeof(LintelCreatorSettings));
                xSer.Serialize(fs, this);
                fs.Close();
            }
        }
    }
}
