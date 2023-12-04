using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EEPRevitPlagin.EEPRPCommandModules.WallFinishCreator
{
    public class WallFinishCreatorCollectionItem
    {
        public Material BaseWallMaterial { get; set; }
        public WallType WallFinishType { get; set; }
        public WallFinishCreatorCollectionItem(Material material, WallType wallType)
        {
            BaseWallMaterial = material;
            WallFinishType = wallType;
        }
    }
}