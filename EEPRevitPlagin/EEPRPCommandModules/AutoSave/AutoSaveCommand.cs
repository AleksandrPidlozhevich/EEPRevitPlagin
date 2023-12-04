using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.UI;
using Autodesk.Revit.DB;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.UI.Events;
using System.Windows.Forms;
using System.Threading;

namespace EEPRevitPlagin.EEPRPCommandModules.AutoSave
{
    [Transaction(TransactionMode.Manual)]
    internal class AutoSaveCommand : IExternalCommand
    {
        public static UIApplication uiapp;
        public static Autodesk.Revit.ApplicationServices.Application app;
        public static UIDocument uidoc;
        public static Document doc;
        public static int changes;       

        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            try
            {
                uiapp = commandData.Application;
                app = uiapp.Application;
                uidoc = uiapp.ActiveUIDocument;
                doc = uidoc.Document;
                app.DocumentChanged += DocumentChanged;
            }
            catch
            {

            }
            return Result.Succeeded;
        }

        private static void DocumentChanged(object sender, Autodesk.Revit.DB.Events.DocumentChangedEventArgs args)
        {
            changes++;
            if (changes > 20)
            {
                changes = 0;
                IExternalEventHandler handler_event = new ExternalEventMy();
                ExternalEvent exEvent = ExternalEvent.Create(handler_event);
                exEvent.Raise();
            }
        }
    }
}

internal class ExternalEventMy : IExternalEventHandler
{
    public void Execute(UIApplication uiapp)
    {
        if (!uiapp.ActiveUIDocument.Document.IsModifiable)
        {
            SaveAsOptions saveAsOptions = new SaveAsOptions();
            saveAsOptions.MaximumBackups = 1;
            saveAsOptions.OverwriteExistingFile = true;
            //ModelPath modelPath = ModelPathUtils.ConvertUserVisiblePathToModelPath(@"C:\temp.rvt");
            //uiapp.ActiveUIDocument.Document.SaveAs(@"Z:\temp.rvt", saveAsOptions);
            uiapp.ActiveUIDocument.Document.Save();
            TaskDialog.Show("info", "the doc will be save locally");
        }
    }

    public string GetName()
    {
        return "my event";
    }
}

