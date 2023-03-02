using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;

namespace EEPRevitPlagin.EEPRPCommandModules.IFCExport
{
    [Transaction(TransactionMode.Manual)]
    internal class IFCExportCommand : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            Document doc = commandData.Application.ActiveUIDocument.Document;

            using (var ts = new Transaction(doc, "exportIFC"))
            {
                ts.Start();

                var ifcOption = new IFCExportOptions();

                doc.Export(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "export.ifc", ifcOption);

                ts.Commit();
            }
            return Result.Succeeded;
        }
    }
}
