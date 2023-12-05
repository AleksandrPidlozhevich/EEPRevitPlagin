#region usings
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Structure;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
#endregion

#if R2020 || R2021 || R2022
namespace EEPRevitPlagin.EEPRPCommandModules.OffAnalyticsModel
{
    [Transaction(TransactionMode.Manual)]
    public class OffAnalyticsModelCommand : IExternalCommand
    {
        public Result Execute(
            ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            try
            {
                // Получение доступа к текущему документу Revit
                Document doc = commandData.Application.ActiveUIDocument.Document;

                // Определение категорий элементов
                BuiltInCategory[] categoriesToInclude = new BuiltInCategory[]
                {
                    BuiltInCategory.OST_Walls,
                    BuiltInCategory.OST_Floors,
                    BuiltInCategory.OST_StructuralFraming,
                    BuiltInCategory.OST_StructuralFoundation,
                    BuiltInCategory.OST_StructuralColumns,
                };

                // Создание пустого списка для элементов
                List<Element> elementsToProcess = new List<Element>();

                // Проход по категориям и добавление элементов в список
                foreach (BuiltInCategory category in categoriesToInclude)
                {
                    FilteredElementCollector collector = new FilteredElementCollector(doc)
                        .OfCategory(category)
                        .WhereElementIsNotElementType();

                    elementsToProcess.AddRange(collector);
                }

                // Исключение групп и элементов внутри групп
                var groupElementIds = new FilteredElementCollector(doc)
                    .OfClass(typeof(Group))
                    .SelectMany(g => ((Group)g).GetMemberIds())
                    .ToList();

                elementsToProcess = elementsToProcess
                    .Where(elem => !groupElementIds.Contains(elem.Id))
                    .ToList();

                // Отключение аналитической модели для каждого элемента
                using (Transaction tx = new Transaction(doc, "Отключение аналитической модели"))
                {
                    tx.Start();

                    foreach (Element elem in elementsToProcess)
                    {
                        AnalyticalModel analyticalModel = elem.GetAnalyticalModel();
                        if (analyticalModel != null)
                        {
                            // Отключение аналитической модели элемента
                            analyticalModel.Enable(false);
                        }
                    }

                    tx.Commit();
                }

                return Result.Succeeded;
            }
            catch (Exception ex)
            {
                message = ex.Message;
                return Result.Failed;
            }
        }
    }
}
#else
#endif