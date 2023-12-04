using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EEPRevitPlagin.EEPRPCommandModules.WallFinishCreator
{
    public class WallFinishCreatorItem
    {
        public string MaterialName { get; set; }
        public string WallFinishTypeName { get; set; }

        public WallFinishCreatorItem()
        {

        }
        public WallFinishCreatorItem(string materialName, string wallFinishTypeName)
        {
            MaterialName = materialName;
            WallFinishTypeName = wallFinishTypeName;
        }
    }
}