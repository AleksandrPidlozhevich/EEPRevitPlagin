using Autodesk.Revit.UI;
using Autodesk.Revit.DB;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.UI.Events;


namespace EEPRevitPlagin.EEPRPCommandModules.RevitServerExport
{
    [Transaction(TransactionMode.Manual)]
    internal class RevitServerExportCommand : IExternalCommand
    {
        public static UIApplication uiapp;
        //public static UIDocument uidoc;
        //public static Document doc;

        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            uiapp = commandData.Application;
            //uidoc = uiapp.ActiveUIDocument;
            Autodesk.Revit.ApplicationServices.Application app = uiapp.Application;
            //doc = uidoc.Document;
            //something
            //uiapp.DialogBoxShowing += UiAppOnDialogBoxShowing;
            RevitServerExportWPF exportWPF = new RevitServerExportWPF();
            exportWPF.ShowDialog();
            return Result.Succeeded;
        }
        private static void UiAppOnDialogBoxShowing(object sender, DialogBoxShowingEventArgs args)
        {

            var dialogId = args.DialogId;
            var dialogType = args.GetType();
            bool isCanceled = args.Cancellable;


            if (dialogId == "Dialog_Revit_DocWarnDialog")
            {
                args.OverrideResult((int)TaskDialogResult.Ok);
            }
            else
            {
                if (args.IsCancelled())
                {
                    args.Cancel();
                }
            }


        }
    }
}
