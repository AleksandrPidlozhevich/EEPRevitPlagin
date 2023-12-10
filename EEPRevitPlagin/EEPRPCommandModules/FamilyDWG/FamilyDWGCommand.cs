using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using EEPRevitPlagin.EEPRPCommandModules.FontChanging;
using System;
using static EEPRevitPlagin.EEPRPCommandModules.FontChanging.FontChangingCommand;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace EEPRevitPlagin.EEPRPCommandModules.FamilyDWG
{
    [Autodesk.Revit.Attributes.Transaction(Autodesk.Revit.Attributes.TransactionMode.Manual)]
    internal class FamilyDWGCommand : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            try
            {
                // Access the active document from the ExternalCommandData
                Document doc = commandData.Application.ActiveUIDocument.Document;

                // Call your ChangingFamilyMethod with appropriate parameters
                ChangingFamilyMethod(null, doc);

                return Result.Succeeded;
            }
            catch (Exception ex)
            {
                message = ex.Message;
                return Result.Failed;
            }
        }
        public void ChangingFamilyMethod(StyleTextDictionary[] styles, Document doc)
        {
            ICollection<Element> families =
                new FilteredElementCollector(doc)
                    .OfClass(typeof(FamilySymbol))
                    .WhereElementIsElementType()
                    .ToElements();

            List<string> filteredElements = new List<string>();

            foreach (Element e in families)
            {
                Category category = e.Category;

                // Проверка категории Generic Models и Doors
                if (category != null && (category.Id.IntegerValue == (int)BuiltInCategory.OST_GenericModel || category.Id.IntegerValue == (int)BuiltInCategory.OST_Doors))
                {
                    ElementType ele = e as ElementType;
                    string familyName = ele.FamilyName;

                    // Фильтрация повторяющихся типов
                    if (!filteredElements.Contains(familyName))
                    {
                        filteredElements.Add(familyName);

                        FamilySymbol fs = e as FamilySymbol;
                        Family f = fs.Family;
                        Document editFamily = doc.EditFamily(f);

                        // Поиск ImportInstance в семействе
                        FilteredElementCollector importInstancesCollector = new FilteredElementCollector(editFamily)
                            .OfClass(typeof(ImportInstance));

                        // Обработка найденных ImportInstance
                        foreach (Element importInstance in importInstancesCollector)
                        {
                            WriteFamilyNameToFile(familyName);
                        }
                        editFamily.Close(false);
                    }
                }
            }
        }


        private void WriteFamilyNameToFile(string familyName)
        {
            try
            {
                using (StreamWriter writer = new StreamWriter("C:\\Users\\user\\Desktop\\ResultFile.txt", true))
                {
                    writer.WriteLine(familyName);
                    writer.Flush(); // Гарантирует запись данных на диск
                }
            }
            catch (Exception ex)
            {
                LogError("Error writing to ResultFile.txt: " + ex.Message);
            }
        }

        private void LogError(string message)
        {
            try
            {
                using (StreamWriter writer = new StreamWriter("C:\\Users\\user\\Desktop\\Log.txt", true))
                {
                    writer.WriteLine(DateTime.Now.ToString() + ": " + message);
                }
            }
            catch (Exception)
            {
                // Handle any exceptions that might occur while writing to the log file.
            }
        }
    }
}