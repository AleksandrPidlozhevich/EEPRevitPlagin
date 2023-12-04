using Autodesk.Revit.UI;
using Autodesk.Revit.DB;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.UI.Events;
using System.Collections.Generic;

namespace EEPRevitPlagin.EEPRPCommandModules.FindOverrideViewElements
{

    [Transaction(TransactionMode.Manual)]
    internal class FindOverrideViewElementsCommand : IExternalCommand
    {
        public static UIApplication uiapp;
        public static UIDocument uidoc;
        public static Document doc;
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            try
            {
                uiapp = commandData.Application;
                Autodesk.Revit.ApplicationServices.Application app = uiapp.Application;
                uidoc = uiapp.ActiveUIDocument;
                doc = uidoc.Document;
                View curView = uidoc.ActiveView;
                List<ElementId> reqElems = new List<ElementId>();
                List<Element> elems = new List<Element>();
                FilteredElementCollector f1 = new FilteredElementCollector(doc, curView.Id);
                f1.WhereElementIsNotElementType();
                elems.AddRange(f1.ToElements());
                foreach (Element elem in elems)
                {
                    if(elem.Category != null)
                    {
                        if (elem.Category.CategoryType == CategoryType.Model)
                        {
                            OverrideGraphicSettings overrideGraphicSettings = curView.GetElementOverrides(elem.Id);
                            if (overrideGraphicSettings.CutBackgroundPatternColor.IsValid == true
                                || overrideGraphicSettings.CutForegroundPatternColor.IsValid == true
                                || overrideGraphicSettings.CutLineColor.IsValid == true
                                || overrideGraphicSettings.ProjectionLineColor.IsValid == true
                                || overrideGraphicSettings.SurfaceBackgroundPatternColor.IsValid == true
                                || overrideGraphicSettings.SurfaceForegroundPatternColor.IsValid == true
                                || overrideGraphicSettings.CutBackgroundPatternId.IntegerValue > 0
                                || overrideGraphicSettings.CutForegroundPatternId.IntegerValue > 0
                                || overrideGraphicSettings.CutLinePatternId.IntegerValue > 0
                                || overrideGraphicSettings.ProjectionLinePatternId.IntegerValue > 0
                                || overrideGraphicSettings.SurfaceBackgroundPatternId.IntegerValue > 0
                                || overrideGraphicSettings.SurfaceForegroundPatternId.IntegerValue > 0
                                || overrideGraphicSettings.CutLineWeight > 0
                                || overrideGraphicSettings.ProjectionLineWeight > 0
                                || overrideGraphicSettings.Halftone
                                || overrideGraphicSettings.Transparency != 0
                                || overrideGraphicSettings.DetailLevel.ToString() != "Undefined"
                                || !overrideGraphicSettings.IsCutBackgroundPatternVisible
                                || !overrideGraphicSettings.IsSurfaceBackgroundPatternVisible
                                || !overrideGraphicSettings.IsSurfaceForegroundPatternVisible
                                || !overrideGraphicSettings.IsCutForegroundPatternVisible)
                            {
                                reqElems.Add(elem.Id);
                            }
                        }
                    }                    
                }
                uidoc.Selection.SetElementIds(reqElems);                
            }
            catch
            {

            }
            return Result.Succeeded;
        }

    }
}
