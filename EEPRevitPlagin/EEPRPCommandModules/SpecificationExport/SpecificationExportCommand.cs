using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace EEPRevitPlagin.EEPRPCommandModules.SpecificationExport
{
    [Transaction(TransactionMode.Manual)]
    public class SpecificationExportCommand : IExternalCommand
    {
        private string selectedSpecificationName; // Добавляем поле для хранения выбранной спецификации
        private string folderPathSelect = string.Empty;

        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            try
            {
                // Получите доступ к текущему документу Revit
                UIDocument uidoc = commandData.Application.ActiveUIDocument;
                Document doc = uidoc.Document;

                // Получите доступ к вашему WPF окну
                SpecificationExportCommandWPF wpfWindow = new SpecificationExportCommandWPF(GetSpecificationNames(doc));

                wpfWindow.ShowDialog();

                if (wpfWindow.SelectedSpecification != null)
                {
                    selectedSpecificationName = wpfWindow.SelectedSpecification; // Сохраняем выбранное имя спецификации
                    folderPathSelect = wpfWindow.FolderPath; // Здесь устанавливаем folderPathSelect
                    List<string> parameterNames = GetSpecificationParameters(doc, selectedSpecificationName);

                    if (parameterNames.Count > 0)
                    {
                        // Создайте текстовый файл и запишите в него параметры
                        string fileName = selectedSpecificationName;
                        string filePath = Path.Combine(folderPathSelect, fileName + ".csv");
                        // Используйте StringBuilder для построения CSV строки
                        StringBuilder csvContent = new StringBuilder();
                        foreach (string parameterName in parameterNames)
                        {
                            csvContent.AppendLine(parameterName);
                        }
                        File.WriteAllText(filePath, csvContent.ToString(), Encoding.UTF8);
                        TaskDialog.Show("Успешный успех", "Параметры спецификации успешно экспортированы в " + filePath);
                    }
                    else
                    {
                        TaskDialog.Show("Ошибка провал трагедия", "Выбранная спецификация не содержит параметров.");
                    }
                }

                return Result.Succeeded;
            }
            catch (Exception ex)
            {
                message = ex.Message;
                return Result.Failed;
            }
        }

        // Метод для получения списка имен спецификаций в документе
        private List<string> GetSpecificationNames(Document doc)
        {
            List<string> specificationNames = new List<string>();

            // Получите все спецификации в документе
            FilteredElementCollector collector = new FilteredElementCollector(doc);
            collector.OfClass(typeof(ViewSchedule));
            List<ViewSchedule> schedules = collector.Cast<ViewSchedule>().ToList();

            // Извлеките имена спецификаций и добавьте их в список
            foreach (ViewSchedule schedule in schedules)
            {
                specificationNames.Add(schedule.Name);
            }
            //спецификаций в алфавитном порядке
            specificationNames.Sort();
            return specificationNames;
        }

        private List<string> GetSpecificationParameters(Document doc, string selectedSpecificationName)
        {
            List<string> parameterNames = new List<string>();
            FilteredElementCollector collector = new FilteredElementCollector(doc);
            collector.OfClass(typeof(ViewSchedule));
            ViewSchedule selectedSchedule = collector.Cast<ViewSchedule>()
                .FirstOrDefault(schedule => schedule.Name == selectedSpecificationName);

            if (selectedSchedule != null)
            {
                ScheduleDefinition definition = selectedSchedule.Definition;

                for (int i = 0; i < definition.GetFieldCount(); i++)
                {
                    ScheduleField field = definition.GetField(i);
                    string parameterName = field.GetName();
                    bool isCalculatedField = field.IsCalculatedField;
                    bool isCombinedParameterField = field.IsCombinedParameterField;

                    if (isCalculatedField)
                    {
                        parameterName += " ,расчетный";
                    }

                    if (isCombinedParameterField)
                    {
                        parameterName += " ,составное";
                    }

                    parameterNames.Add(parameterName);
                }
            }
            return parameterNames;
        }
    }
}