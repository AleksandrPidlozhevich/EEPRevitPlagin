using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.DB.Events;
using System.Collections.Generic;

namespace EEPRevitPlagin.EEPRPCommandModules.SpecificationChecker
{
    public class SpecificationCheckerHandler : IExternalEventHandler
    {
        private Dictionary<ElementId, int> initialScheduleCount = new Dictionary<ElementId, int>();
        private bool isSyncInProgress = false;

        public void Execute(UIApplication app)
        {

        }

        public string GetName()
        {
            return "SpecificationCheckerHandler";
        }

        public void StartMonitoring(UIApplication app)
        {
            // Подписываемся на событие DocumentChanged
            app.Application.DocumentChanged += HandleDocumentChanged;

            // Подписываемся на событие DocumentSynchronizingWithCentral
            app.Application.DocumentSynchronizingWithCentral += HandleSyncStart;

            // Подписываемся на событие DocumentSynchronizedWithCentral
            app.Application.DocumentSynchronizedWithCentral += HandleSyncEnd;

            // Получаем список начальных количеств записей спецификаций на каждом листе
            InitializeInitialScheduleCounts(app.ActiveUIDocument.Document);
        }

        public void HandleDocumentChanged(object sender, DocumentChangedEventArgs e)
        {
            if (isSyncInProgress)
                return;

            Document doc = e.GetDocument();

            // Проверяем, что документ существует
            if (doc != null)
            {
                foreach (ElementId addedElementId in e.GetAddedElementIds())
                {
                    Element addedElement = doc.GetElement(addedElementId);

                    // Проверяем, является ли добавленный элемент ScheduleSheetInstance
                    if (addedElement != null && addedElement is ScheduleSheetInstance scheduleInstance)
                    {
                        // Получаем связанный Schedule для ScheduleSheetInstance
                        ElementId scheduleId = scheduleInstance.ScheduleId;
                        ViewSchedule schedule = doc.GetElement(scheduleId) as ViewSchedule;

                        if (schedule != null)
                        {
                            // Получаем определение спецификации
                            ScheduleDefinition scheduleDefinition = schedule.Definition;

                            // Получаем имя первого поля спецификации
                            string firstFieldName = scheduleDefinition.GetField(0)?.GetName();

                            // Проверяем, соответствует ли имя первого поля требуемым значениям
                            if (firstFieldName == "Арм_Спецификация на ЖБК" || 
                                firstFieldName == "Арм_Спецификация на ЖБК_Детали" || 
                                firstFieldName == "Арм_Спецификация на изделие" || 
                                firstFieldName == "Арм_Спецификация на изделие_часть 2" ||
                                firstFieldName == "Арм_Спецификация на изделие_Форма 8")
                            {
                                // Проверяем, изменилось ли количество записей спецификаций на листе
                                if (HasScheduleCountChanged(scheduleInstance, doc))
                                {
                                    // Обнаружены изменения, выполняем необходимые действия
                                    TaskDialog.Show("Спецификация обнаружена", "На листе появилась новая запись спецификации.");
                                }
                            }
                        }
                    }
                }
            }
        }

        private void InitializeInitialScheduleCounts(Document doc)
        {
            // Получаем начальное количество записей спецификаций на каждом листе
            FilteredElementCollector sheets = new FilteredElementCollector(doc)
                .OfCategory(BuiltInCategory.OST_Sheets);
            foreach (ViewSheet sheet in sheets)
            {
                int scheduleCount = GetScheduleCount(sheet);
                initialScheduleCount[sheet.Id] = scheduleCount;
            }
        }

        private bool HasScheduleCountChanged(ScheduleSheetInstance scheduleInstance, Document doc)
        {
            // Проверяем, изменилось ли количество записей спецификаций на листе
            ViewSheet sheet = GetOwningSheet(scheduleInstance, doc);
            if (sheet != null)
            {
                int initialCount = initialScheduleCount[sheet.Id];
                int currentCount = GetScheduleCount(sheet);
                return initialCount != currentCount;
            }
            return false;
        }

        private ViewSheet GetOwningSheet(ScheduleSheetInstance scheduleInstance, Document doc)
        {
            // Получаем лист, которому принадлежит ScheduleSheetInstance
            ElementId sheetId = scheduleInstance.OwnerViewId;
            return doc.GetElement(sheetId) as ViewSheet;
        }

        private int GetScheduleCount(ViewSheet sheet)
        {
            // Получаем количество записей спецификаций на листе
            ICollection<ElementId> dependentElements = sheet.GetDependentElements(new ElementClassFilter(typeof(ScheduleSheetInstance)));
            return dependentElements.Count;
        }

        // Обработчик события начала синхронизации с центральной моделью
        private void HandleSyncStart(object sender, DocumentSynchronizingWithCentralEventArgs e)
        {
            isSyncInProgress = true;
        }

        // Обработчик события окончания синхронизации с центральной моделью
        private void HandleSyncEnd(object sender, DocumentSynchronizedWithCentralEventArgs e)
        {
            isSyncInProgress = false;
        }
    }
}