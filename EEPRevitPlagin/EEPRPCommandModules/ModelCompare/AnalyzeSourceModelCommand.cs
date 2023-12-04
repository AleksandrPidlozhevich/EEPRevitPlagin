using Autodesk.Revit.UI;
using Autodesk.Revit.DB;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.UI.Events;


namespace EEPRevitPlagin.EEPRPCommandModules.ModelCompare
{
    [Transaction(TransactionMode.Manual)]
    internal class AnalyzeSourceModelCommand : IExternalCommand
    {
        public static UIApplication uiapp;
        public static UIDocument uidoc;
        public static Document doc;
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            uiapp = commandData.Application;
            Autodesk.Revit.ApplicationServices.Application app = uiapp.Application;
            uidoc = uiapp.ActiveUIDocument;
            doc = uidoc.Document;
            AnalyzeSourceModelWPF exportWPF = new AnalyzeSourceModelWPF();
            exportWPF.Topmost = true;
            exportWPF.Show();
            return Result.Succeeded;
        }
    }
}
