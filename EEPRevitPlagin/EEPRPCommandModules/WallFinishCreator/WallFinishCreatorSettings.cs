using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace EEPRevitPlagin.EEPRPCommandModules.WallFinishCreator
{
    partial class WallFinishCreatorSettings
    {
        public List<WallFinishCreatorItem> GetSettings()
        {
            List<WallFinishCreatorItem> wallFinishCreatorItemList = null;
            string assemblyPath = Path.Combine(Path.GetDirectoryName(typeof(App).Assembly.Location), "WallFinishCreatorSettings.xml");

            if (File.Exists(assemblyPath))
            {
                using (FileStream fs = new FileStream(assemblyPath, FileMode.Open))
                {
                    XmlSerializer xSer = new XmlSerializer(typeof(List<WallFinishCreatorItem>));
                    wallFinishCreatorItemList = xSer.Deserialize(fs) as List<WallFinishCreatorItem>;
                    fs.Close();
                }
            }
            else
            {
                wallFinishCreatorItemList = new List<WallFinishCreatorItem>();
            }

            return wallFinishCreatorItemList;
        }
        public string GetHeightSettings()
        {
            string heightSettings = null;
            string assemblyPath = Path.Combine(Path.GetDirectoryName(typeof(App).Assembly.Location), "WallFinishCreatorHightSettings.xml");

            if (File.Exists(assemblyPath))
            {
                using (FileStream fs = new FileStream(assemblyPath, FileMode.Open))
                {
                    XmlSerializer xSer = new XmlSerializer(typeof(string));
                    heightSettings = xSer.Deserialize(fs) as string;
                    fs.Close();
                }
            }
            else
            {
                heightSettings = null;
            }

            return heightSettings;
        }

        public bool GetRoomBoundarySettings()
        {
            bool roomBoundarySettings = false;
            string assemblyPath = Path.Combine(Path.GetDirectoryName(typeof(App).Assembly.Location), "WallFinishCreatorRoomBoundarySettings.xml");

            if (File.Exists(assemblyPath))
            {
                using (FileStream fs = new FileStream(assemblyPath, FileMode.Open))
                {
                    XmlSerializer xSer = new XmlSerializer(typeof(bool));
                    roomBoundarySettings = (bool)xSer.Deserialize(fs);
                    fs.Close();
                }
            }
            else
            {
                roomBoundarySettings = false;
            }

            return roomBoundarySettings;
        }

        public void Save(List<WallFinishCreatorItem> wallFinishCreatorItem, string hight, bool roomBoundary)
        {
            string assemblyPath = Path.Combine(Path.GetDirectoryName(typeof(App).Assembly.Location), "WallFinishCreatorSettings.xml");
            string assemblyPathHight = Path.Combine(Path.GetDirectoryName(typeof(App).Assembly.Location), "WallFinishCreatorHightSettings.xml");
            string assemblyPathRoomBoundary = Path.Combine(Path.GetDirectoryName(typeof(App).Assembly.Location), "WallFinishCreatorRoomBoundary");

            if (File.Exists(assemblyPath))
            {
                File.Delete(assemblyPath);
            }

            if (File.Exists(assemblyPathHight))
            {
                File.Delete(assemblyPathHight);
            }

            if (File.Exists(assemblyPathRoomBoundary))
            {
                File.Delete(assemblyPathRoomBoundary);
            }

            using (FileStream fs = new FileStream(assemblyPath, FileMode.Create))
            {
                XmlSerializer xSer = new XmlSerializer(typeof(List<WallFinishCreatorItem>));
                xSer.Serialize(fs, wallFinishCreatorItem);
                fs.Close();
            }

            using (FileStream fs = new FileStream(assemblyPathHight, FileMode.Create))
            {
                XmlSerializer xSer = new XmlSerializer(typeof(string));
                xSer.Serialize(fs, hight);
                fs.Close();
            }

            using (FileStream fs = new FileStream(assemblyPathRoomBoundary, FileMode.Create))
            {
                XmlSerializer xSer = new XmlSerializer(typeof(bool));
                xSer.Serialize(fs, roomBoundary);
                fs.Close();
            }
        }
    }
}
