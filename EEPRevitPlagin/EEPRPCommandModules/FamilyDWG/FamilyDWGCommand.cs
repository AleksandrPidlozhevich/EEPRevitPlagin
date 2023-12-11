using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using EEPRevitPlagin.EEPRPCommandModules.FontChanging;
using System;
using System.Collections.Generic;
using System.IO;

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
                FamilyDWGCommandWPF wpfWindow = new FamilyDWGCommandWPF();

                // Отобразите WPF-окно и дождитесь его закрытия
                bool? result = wpfWindow.ShowDialog();

                // Проверьте, была ли нажата кнопка "Save" в WPF-окне
                if (result == true)
                {
                    // Вызовите ваш метод ChangingFamilyMethod с передачей FilePath
                    ChangingFamilyMethod(null, doc, wpfWindow.FilePath);
                }

                return Result.Succeeded;
            }
            catch (Exception ex)
            {
                message = ex.Message;
                return Result.Failed;
            }
        }
        public void ChangingFamilyMethod(StyleTextDictionary[] styles, Document doc, string filePath)
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
                            WriteFamilyNameToFile(filePath, familyName);
                        }
                        editFamily.Close(false);
                    }
                }
            }
        }


        private void WriteFamilyNameToFile(string filePath, string familyName)
        {
            // Вместо жестко закодированного пути используйте переданный filePath
            using (StreamWriter writer = new StreamWriter(filePath, true))
            {
                writer.WriteLine(familyName);
                writer.Flush(); // Гарантирует запись данных на диск
            }
        }
    }
}