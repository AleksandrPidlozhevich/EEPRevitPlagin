using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EEPRevitPlagin.EEPRPCommandModules.WallFinishCreatorProperties
{
    [Autodesk.Revit.Attributes.Transaction(Autodesk.Revit.Attributes.TransactionMode.Manual)]
    class WallFinishCreatorPropertiesCommand : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            WallFinishCreatorPropertiesWPF wallFinishCreatorPropertiesWPF = new WallFinishCreatorPropertiesWPF();
            wallFinishCreatorPropertiesWPF.ShowDialog();
            if (wallFinishCreatorPropertiesWPF.DialogResult != true)
            {
                return Result.Cancelled;
            }
            return Result.Succeeded;
        }
    }
}
