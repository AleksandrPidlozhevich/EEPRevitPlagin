using Autodesk.Revit.UI;
using Autodesk.Revit.DB;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.UI.Events;


namespace EEPRevitPlagin.EEPRPCommandModules.DuctInsulationCheck
{
    [Transaction(TransactionMode.Manual)]
    internal class DuctInsulationCheckCommand : IExternalCommand
    {    
        public static UIApplication uiapp;
        public static UIDocument uidoc;
        public static Document doc;
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            uiapp = commandData.Application;
            uidoc = uiapp.ActiveUIDocument;
            Autodesk.Revit.ApplicationServices.Application app = uiapp.Application;
            doc = uidoc.Document;
            DuctInsulationCheckWPF ductInsulationCheckWPF = new DuctInsulationCheckWPF();
            ductInsulationCheckWPF.Topmost=true;
            ductInsulationCheckWPF.Show();    
            
            return Result.Succeeded;
        }
    }
}
