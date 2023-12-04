using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EEPRevitPlagin.EEPRPCommandModules.АmountСhange
{
    [Transaction(TransactionMode.ReadOnly)]
    internal class CountRevisionCommand : IExternalCommand
    {
        public Result Execute(
            ExternalCommandData commandData,
            ref string message,
            ElementSet elements)
        {
            UIApplication uiApp = commandData.Application;
            UIDocument uiDoc = uiApp.ActiveUIDocument;
            Document doc = uiDoc.Document;

            // Замените sheetNumber на номер листа, на котором вы хотите посчитать облачки обновлений.
            string sheetNumber = "3";

            // Найдем элемент листа по его номеру.
            ViewSheet sheet = new FilteredElementCollector(doc)
                .OfClass(typeof(ViewSheet))
                .Cast<ViewSheet>()
                .FirstOrDefault(s => s.SheetNumber == sheetNumber);

            if (sheet != null)
            {
                int numberOfClouds = new FilteredElementCollector(doc)
                    .OfCategory(BuiltInCategory.OST_RevisionClouds)
                    .WhereElementIsNotElementType()
                    .Cast<RevisionCloud>()
                    .Count(cloud => cloud.OwnerViewId == sheet.Id);

                TaskDialog.Show("Количество облачков обновлений",
                                $"На листе {sheetNumber} есть {numberOfClouds} облачков обновлений.");
            }
            else
            {
                TaskDialog.Show("Ошибка", $"Лист с номером {sheetNumber} не найден.");
            }

            return Result.Succeeded;
        }
    }
}