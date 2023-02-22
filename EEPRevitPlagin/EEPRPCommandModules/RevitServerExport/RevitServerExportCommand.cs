using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.UI;
using Autodesk.Revit.DB;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.UI.Events;

namespace EEPRevitPlagin.EEPRPСommandModules.RevitServerExport
{
    [Transaction(TransactionMode.Manual)]
    internal class RevitServerExportCommand : IExternalCommand
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
            uiapp.DialogBoxShowing += UiAppOnDialogBoxShowing;
            RevitServerExportWPF exportWPF = new RevitServerExportWPF();
            exportWPF.ShowDialog();
            TaskDialog.Show("ok", "ok");
            return Result.Succeeded;
        }
        private static void UiAppOnDialogBoxShowing(object sender, DialogBoxShowingEventArgs args)
        {
            args.OverrideResult(1001);
            args.Cancel();
        }
    }
}
