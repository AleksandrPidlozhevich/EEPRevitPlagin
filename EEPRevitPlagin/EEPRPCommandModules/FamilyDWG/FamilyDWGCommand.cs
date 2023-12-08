using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EEPRevitPlagin.EEPRPCommandModules.FamilyDWG
{
    [Autodesk.Revit.Attributes.Transaction(Autodesk.Revit.Attributes.TransactionMode.Manual)]
    internal class FamilyDWGCommand : IExternalCommand
    {
        public Result Execute(
            ExternalCommandData commandData,
            ref string message,
            ElementSet elements)
        {
            try
            {
                // Получаем доступ к текущему документу
                UIDocument uidoc = commandData.Application.ActiveUIDocument;
                Document doc = uidoc.Document;

                // Создаем фильтр для поиска семейств
                ElementClassFilter familyFilter = new ElementClassFilter(typeof(Family));

                // Перебираем все семейства в документе
                FilteredElementCollector collector = new FilteredElementCollector(doc);
                ICollection<Element> families = collector.WherePasses(familyFilter).ToElements();

                List<string> resultFamilyNames = new List<string>();

                using (Transaction transaction = new Transaction(doc))
                {
                    transaction.Start("Process Families");

                    foreach (Element family in families)
                    {
                        // Проверяем условие: GetDependentElements -> ImportInstance -> GetTypeId -> CADLinkType
                        List<ElementId> dependentElements = new List<ElementId>();
                        if (family is Family)
                        {
                            dependentElements = ((Family)family).GetDependentElements(null).ToList();
                        }

                        foreach (ElementId dependentElementId in dependentElements)
                        {
                            Element dependentElement = doc.GetElement(dependentElementId);

                            if (dependentElement is ImportInstance)
                            {
                                ElementId typeId = dependentElement.GetTypeId();
                                Element typeElement = doc.GetElement(typeId);

                                if (typeElement is CADLinkType)
                                {
                                    // Получаем имя семейства и добавляем его в список
                                    string familyName = typeElement.Name;
                                    resultFamilyNames.Add(familyName);
                                }
                            }
                        }
                    }

                    transaction.Commit();
                }

                // Сохраняем результат в текстовый файл
                string filePath = "C:\\Users\\a.n.pidlozhevich\\Downloads\\ResultFile.txt";
                File.WriteAllLines(filePath, resultFamilyNames);

                TaskDialog.Show("Success", "Plugin execution completed successfully.");
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