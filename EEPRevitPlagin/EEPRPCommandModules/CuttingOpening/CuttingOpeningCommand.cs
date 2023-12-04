using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.DB.Electrical;
using Autodesk.Revit.DB.Mechanical;
using Autodesk.Revit.DB.Plumbing;
using Autodesk.Revit.DB.Structure;
using Autodesk.Revit.UI.Selection;
using EEPRevitPlagin.SecondaryCommand;
using Document = Autodesk.Revit.DB.Document;
using Transaction = Autodesk.Revit.DB.Transaction;
using System.Windows.Controls;
using System.Dynamic;

namespace EEPRevitPlagin.EEPRPCommandModules.CuttingOpening
{
    [Autodesk.Revit.Attributes.Transaction(Autodesk.Revit.Attributes.TransactionMode.Manual)]
    internal class CuttingOpeningCommand : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIApplication uiApp = commandData.Application;
            UIDocument uiDoc = uiApp.ActiveUIDocument;
            Document doc = uiApp.ActiveUIDocument.Document;
            Selection sel = commandData.Application.ActiveUIDocument.Selection;

            List <FamilySymbol> openingSymbols = GetOpeningSymbol(doc);
            return Result.Cancelled;
        }

        private List<FamilySymbol> GetOpeningSymbol(Document doc)
        {
            return new FilteredElementCollector(doc)
                .OfClass(typeof(FamilySymbol))
                .OfCategory(BuiltInCategory.OST_GenericModel)
                .Cast<FamilySymbol>()
                .Where(fs => fs.Family.FamilyPlacementType == FamilyPlacementType.OneLevelBased)
                .Where(fs => fs.get_Parameter(BuiltInParameter.ALL_MODEL_MODEL).ToString() == "Пересечение")
                .ToList();
        }

    }

}
