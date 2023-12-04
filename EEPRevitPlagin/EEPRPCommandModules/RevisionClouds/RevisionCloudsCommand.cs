using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace EEPRevitPlagin.EEPRPCommandModules.RevisionClouds
{
    [Autodesk.Revit.Attributes.Transaction(Autodesk.Revit.Attributes.TransactionMode.Manual)]
    public class RevisionCloudsCommand : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIApplication uiApp = commandData.Application;
            UIDocument uidoc = uiApp.ActiveUIDocument;
            Document doc = uidoc.Document;

            // Creating a WPF Form
            RevisionCloudsWPF form = new RevisionCloudsWPF();

            // Fill the drop-down list with sets
            FilteredElementCollector collector = new FilteredElementCollector(doc);
            ICollection<ViewSheet> sheets = collector.OfCategory(BuiltInCategory.OST_Sheets)
                .WhereElementIsNotElementType()
                .Cast<ViewSheet>()
                .ToList();

            List<string> sets = new List<string>();

            foreach (ViewSheet sheet in sheets)
            {
                Parameter parameter = sheet.LookupParameter("ADSK_Комплект чертежей");
                if (parameter != null)
                {
                    string set = parameter.AsString();
                    if (!string.IsNullOrEmpty(set) && !sets.Contains(set))
                    {
                        sets.Add(set);
                    }
                }
            }
            List<string> sortedSets = sets.OrderBy(s => s).ToList();
            form.comboBoxSets.ItemsSource = sortedSets;

            // Display form
            if (form.ShowDialog() == true)
            {
                string selectedSet = form.SelectedSet;
                // check the status checkbox  amountCloud, workingDrawings, titlePage, permissionChange
                bool isAmountCloudChecked = form.amountCloud.IsChecked ?? false;
                bool isWorkingDrawingsChecked = form.workingDrawings.IsChecked ?? false;
                bool isTitlePageChecked = form.titlePage.IsChecked ?? false;
                bool isPermissionChange = form.permissionChange.IsChecked ?? false;
                // Проверка, что выбрано значение в comboBoxSets и нажата кнопка doButton
                if ((isAmountCloudChecked || isWorkingDrawingsChecked || isTitlePageChecked || isPermissionChange) && !string.IsNullOrEmpty(selectedSet) && form.DialogResult == true)
                {
                    // Создание словаря для хранения значений облаков ревизий по выбранным комплектам
                    Dictionary<string, List<string>> cloudValuesBySet = new Dictionary<string, List<string>>();
                    Dictionary<string, List<string>> dateValuesBySet = new Dictionary<string, List<string>>();

                    // Подсчет облаков для каждого листа, соответствующего выбранному комплекту
                    using (Transaction tx = new Transaction(doc))
                    {
                        tx.Start("Update Cloud Counts");
                        foreach (ViewSheet sheet in sheets)
                        {
                            Parameter setNameParam = sheet.LookupParameter("ADSK_Комплект чертежей");
                            if (setNameParam != null)
                            {
                                string sheetSetName = setNameParam.AsString();
                                if (sheetSetName == selectedSet)
                                {
                                    Dictionary<string, int> cloudCountByType = new Dictionary<string, int>();
                                    // Сбор облака ревизий на листе
                                    FilteredElementCollector cloudCollector = new FilteredElementCollector(doc, sheet.Id);
                                    ICollection<Element> cloudsOnSheet = cloudCollector.OfCategory(BuiltInCategory.OST_RevisionClouds).WhereElementIsNotElementType().ToList();
                                    foreach (Element cloudElem in cloudsOnSheet)
                                    {
                                        RevisionCloud cloud = cloudElem as RevisionCloud;
                                        Parameter descriptionParam = cloud.get_Parameter(BuiltInParameter.REVISION_CLOUD_REVISION_DESCRIPTION);

                                        if (descriptionParam != null)
                                        {
                                            string description = descriptionParam.AsString();

                                            if (description == selectedSet)
                                            {
                                                Parameter typeParam = cloud.get_Parameter(BuiltInParameter.REVISION_CLOUD_REVISION_ISSUED_TO);

                                                if (typeParam != null && typeParam.HasValue)
                                                {
                                                    string cloudType = typeParam.AsString();

                                                    if (cloudCountByType.ContainsKey(cloudType))
                                                    {
                                                        cloudCountByType[cloudType]++;
                                                    }
                                                    else
                                                    {
                                                        cloudCountByType[cloudType] = 1;
                                                    }
                                                }

                                                // Запись значения REVISION_CLOUD_REVISION_ISSUED_BY в словарь
                                                Parameter issuedByParam = cloud.get_Parameter(BuiltInParameter.REVISION_CLOUD_REVISION_ISSUED_BY);
                                                if (issuedByParam != null && issuedByParam.HasValue)
                                                {
                                                    string issuedByValue = issuedByParam.AsString();
                                                    if (!cloudValuesBySet.ContainsKey(selectedSet))
                                                    {
                                                        cloudValuesBySet[selectedSet] = new List<string>();
                                                    }
                                                    cloudValuesBySet[selectedSet].Add(issuedByValue);
                                                }
                                                // Запись значения REVISION_CLOUD_REVISION_DATE в словарь
                                                Parameter dateParam = cloud.get_Parameter(BuiltInParameter.REVISION_CLOUD_REVISION_DATE);
                                                if (dateParam != null && dateParam.HasValue)
                                                {
                                                    string dateValue = dateParam.AsString();
                                                    if (!dateValuesBySet.ContainsKey(selectedSet))
                                                    {
                                                        dateValuesBySet[selectedSet] = new List<string>();
                                                    }
                                                    dateValuesBySet[selectedSet].Add(dateValue);
                                                }
                                            }
                                        }

                                    }
                                    // Получение дополнительных облаков ревизий
                                    ICollection<ElementId> additionalRevisionIds = sheet.GetAdditionalRevisionIds();
                                    foreach (ElementId revisionId in additionalRevisionIds)
                                    {
                                        // Получение элемента облака ревизии по идентификатору
                                        Element revisionElement = doc.GetElement(revisionId);

                                        if (revisionElement != null)
                                        {
                                            // Получение значения параметра PROJECT_REVISION_REVISION_ISSUED_TO
                                            Parameter issuedToParam = revisionElement.get_Parameter(BuiltInParameter.PROJECT_REVISION_REVISION_ISSUED_TO);

                                            if (issuedToParam != null && issuedToParam.HasValue)
                                            {
                                                string issuedToValue = issuedToParam.AsString();

                                                // Добавление значения в словарь с ключом issuedToValue и значением -1000
                                                if (!cloudCountByType.ContainsKey(issuedToValue))
                                                {
                                                    cloudCountByType.Add(issuedToValue, -1000);
                                                }
                                                else
                                                {
                                                    // Если ключ уже существует, увеличьте его значение на 1
                                                    cloudCountByType[issuedToValue]++;
                                                }
                                            }
                                        }
                                    }
                                    // Сбор облака ревизий на видах, расположенных на этом листе
                                    FilteredElementCollector viewportCollector = new FilteredElementCollector(doc, sheet.Id);
                                    ICollection<Element> viewportsOnSheet = viewportCollector.OfCategory(BuiltInCategory.OST_Viewports).WhereElementIsNotElementType().ToElements();
                                    foreach (Element viewportElem in viewportsOnSheet)
                                    {
                                        Viewport viewport = viewportElem as Viewport;
                                        if (viewport != null)
                                        {
                                            View view = doc.GetElement(viewport.ViewId) as View;
                                            if (view != null)
                                            {
                                                cloudCollector = new FilteredElementCollector(doc, view.Id);
                                                ICollection<Element> cloudsOnView = cloudCollector.OfCategory(BuiltInCategory.OST_RevisionClouds).WhereElementIsNotElementType().ToElements();

                                                foreach (Element cloudElem in cloudsOnView)
                                                {
                                                    RevisionCloud cloud = cloudElem as RevisionCloud;
                                                    Parameter descriptionParam = cloud.get_Parameter(BuiltInParameter.REVISION_CLOUD_REVISION_DESCRIPTION);

                                                    if (descriptionParam != null)
                                                    {
                                                        string description = descriptionParam.AsString();

                                                        if (description == selectedSet)
                                                        {
                                                            Parameter typeParam = cloud.get_Parameter(BuiltInParameter.REVISION_CLOUD_REVISION_ISSUED_TO);

                                                            if (typeParam != null && typeParam.HasValue)
                                                            {
                                                                string cloudType = typeParam.AsString();

                                                                if (cloudCountByType.ContainsKey(cloudType))
                                                                {
                                                                    cloudCountByType[cloudType]++;
                                                                }
                                                                else
                                                                {
                                                                    cloudCountByType[cloudType] = 1;
                                                                }
                                                            }

                                                            // Запись значения REVISION_CLOUD_REVISION_ISSUED_BY в словарь
                                                            Parameter issuedByParam = cloud.get_Parameter(BuiltInParameter.REVISION_CLOUD_REVISION_ISSUED_BY);
                                                            if (issuedByParam != null && issuedByParam.HasValue)
                                                            {
                                                                string issuedByValue = issuedByParam.AsString();
                                                                if (!cloudValuesBySet.ContainsKey(selectedSet))
                                                                {
                                                                    cloudValuesBySet[selectedSet] = new List<string>();
                                                                }
                                                                cloudValuesBySet[selectedSet].Add(issuedByValue);
                                                            }
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                    }
                                    // Ведомость рабочих чертежей запись в параметр листа "ADSK_Примечание" при установке checkbox "workingDrawings".
                                    if (isWorkingDrawingsChecked)
                                    {
                                        foreach (ViewSheet sheetset in sheets)
                                        {
                                            Parameter setParameter = sheet.LookupParameter("ADSK_Комплект чертежей");
                                            if (setParameter != null && setParameter.AsString() == selectedSet)
                                            {
                                                List<string> combinedList = GetCloudNumbers(sheet);
                                                string cloudNumbersAsString = string.Join(", ", combinedList);

                                                if (!string.IsNullOrEmpty(cloudNumbersAsString)) // Проверяем, что cloudNumbersAsString не пустая
                                                {
                                                    Parameter adsK_PrimechanieParam = sheet.LookupParameter("ADSK_Примечание");
                                                    if (adsK_PrimechanieParam != null)
                                                    {
                                                        adsK_PrimechanieParam.Set("Изм." + cloudNumbersAsString);
                                                    }
                                                }
                                            }
                                        }
                                    }
                                    // Record the number of clouds in the corresponding sheet settings during installation checkbox "amountCloud".
                                    if (isAmountCloudChecked)
                                    {
                                        var sortedCloudCountByType = cloudCountByType.OrderBy(kvp => int.Parse(kvp.Key)).ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

                                        int paramIndex = 1;
                                        Guid statusParam1To4Guid = new Guid("15a2a838-1771-4a1a-b7e5-de985e7fe7ad");
                                        Guid statusParam5To8Guid = new Guid("461ae756-75c9-4215-8374-2ffa8dad32ee");
                                        Parameter status1To4Value = sheet.get_Parameter(statusParam1To4Guid);
                                        Parameter status5To8Value = sheet.get_Parameter(statusParam5To8Guid);
                                        string statusDigits1to4 = status1To4Value.AsValueString();
                                        string statusDigits5to8 = status5To8Value.AsValueString();
                                        string combinedStatus = $"{statusDigits1to4}{statusDigits5to8}";

                                        foreach (var kvp in sortedCloudCountByType)
                                        {
                                            string cloudType = kvp.Key;
                                            int count = kvp.Value;

                                            char statusDigit = '0';
                                            if (paramIndex <= combinedStatus.Length)
                                            {
                                                statusDigit = combinedStatus[paramIndex - 1];
                                            }

                                            string paramName = "WE_Количество участков " + paramIndex;
                                            Parameter countParam = sheet.LookupParameter(paramName);
                                            if (countParam != null)
                                            {
                                                if (statusDigit == '1')
                                                {
                                                    if (count >= 0)
                                                    {
                                                        countParam.Set(count.ToString());
                                                    }
                                                    else
                                                    {
                                                    }
                                                }
                                                else
                                                {
                                                    countParam.Set("-");
                                                }
                                            }
                                            paramIndex++;
                                        }
                                        // Заполнить оставшиеся позиции "WE_Количество участков" значением " "
                                        while (paramIndex <= 8)
                                        {
                                            string paramName = "WE_Количество участков " + paramIndex;
                                            Parameter countParam = sheet.LookupParameter(paramName);
                                            if (countParam != null)
                                            {
                                                countParam.Set(" ");
                                            }
                                            paramIndex++;
                                        }
                                    }
                                    // Obtaining RevisionCloudsin information Front page from when installing checkbox "titlePage".
                                    if (isTitlePageChecked)
                                    {
                                        ViewSheet titleSheet = sheets.FirstOrDefault(s => s.LookupParameter("ADSK_Комплект чертежей")?.AsString() == selectedSet && s.Name == "Титул");
                                        if (titleSheet != null)
                                        {
                                            // Найдем экземпляр семейства "WE_Титул_KR" на листе
                                            FamilyInstance titleFamilyInstance = FindTitleFamilyInstance(titleSheet);

                                            if (titleFamilyInstance != null)
                                            {
                                                for (int i = 0; i < 8; i++)
                                                {
                                                    string numberParam = "Номер разрешения изм." + (i + 1);
                                                    Parameter paramNumber = titleFamilyInstance.LookupParameter(numberParam);
                                                    string dateParamName = "Дата изм." + (i + 1);
                                                    Parameter paramDate = titleFamilyInstance.LookupParameter(dateParamName);

                                                    if (paramNumber != null && paramDate != null)
                                                    {
                                                        // Найдем облако ревизии с соответствующим REVISION_CLOUD_REVISION_ISSUED_TO
                                                        string targetRevisionNum = (i + 1).ToString(); // Предполагаем, что REVISION_CLOUD_REVISION_ISSUED_TO соответствует (i + 1)
                                                        ElementId targetCloudId = null;

                                                        FilteredElementCollector cloudCollectorDoc = new FilteredElementCollector(doc);
                                                        ICollection<Element> clouds = cloudCollectorDoc.OfCategory(BuiltInCategory.OST_Revisions).WhereElementIsNotElementType().ToElements();

                                                        foreach (Element cloudElem in clouds)
                                                        {
                                                            Revision cloud = cloudElem as Revision;
                                                            Parameter revisionNumParam = cloud.get_Parameter(BuiltInParameter.PROJECT_REVISION_REVISION_ISSUED_TO);
                                                            Parameter descParam = cloud.get_Parameter(BuiltInParameter.PROJECT_REVISION_REVISION_DESCRIPTION);
                                                            if (revisionNumParam != null && descParam.AsString() == selectedSet && revisionNumParam.AsString() == targetRevisionNum)
                                                            {
                                                                targetCloudId = cloud.Id;
                                                                break;
                                                            }
                                                        }
                                                        if (targetCloudId != null)
                                                        {
                                                            // Получим значение REVISION_CLOUD_REVISION_ISSUED_BY из найденного облака
                                                            Element targetCloud = doc.GetElement(targetCloudId);
                                                            Parameter issuedByParam = targetCloud.get_Parameter(BuiltInParameter.PROJECT_REVISION_REVISION_ISSUED_BY);
                                                            Parameter dateParam = targetCloud.get_Parameter(BuiltInParameter.PROJECT_REVISION_REVISION_DATE);
                                                            if (issuedByParam != null && issuedByParam.HasValue)
                                                            {
                                                                string issuedByValue = issuedByParam.AsString();

                                                                // Установим значение параметра "Номер разрешения изм." + (i + 1)
                                                                paramNumber.Set(issuedByValue);
                                                            }

                                                            if (dateParam != null && dateParam.HasValue)
                                                            {
                                                                string dateValue = dateParam.AsString();

                                                                // Установим значение параметра "Дата изм." + (i + 1)
                                                                paramDate.Set(dateValue);
                                                            }
                                                        }
                                                        else
                                                        {
                                                            // Облако с указанным REVISION_CLOUD_REVISION_NUM не найдено, установите параметры как пустые или по вашему выбору.
                                                            paramNumber.Set(string.Empty);
                                                            paramDate.Set(string.Empty);
                                                        }
                                                    }
                                                }
                                            }
                                            else
                                            {
                                                MessageBox.Show("Семейство 'WE_Титул_KR' не найдено на листе 'Титул' с выбранным комплектом.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                                                return Result.Failed;
                                            }
                                        }
                                    }
                                    // Resolution sheet update parameter "WE_Описание Номер Листов" based on the unique values of "WE_Описание Изм."
                                    if (isPermissionChange)
                                    {
                                        // Создание словаря для хранения уникальных значений "WE_Описание Изм." и соответствующих им номеров листов
                                        Dictionary<string, List<string>> descriptionsToSheetNumbers = new Dictionary<string, List<string>>();

                                        // Первый проход: собрать значения "WE_Описание Изм." и соответствующие им номера листов
                                        foreach (ViewSheet sheetToUpdate in sheets)
                                        {
                                            Parameter descriptionParam = sheetToUpdate.LookupParameter("WE_Описание Изм.");
                                            if (descriptionParam != null)
                                            {
                                                string descriptionValue = descriptionParam.AsString();

                                                if (!string.IsNullOrEmpty(descriptionValue))
                                                {
                                                    if (!descriptionsToSheetNumbers.ContainsKey(descriptionValue))
                                                    {
                                                        descriptionsToSheetNumbers[descriptionValue] = new List<string>();
                                                    }

                                                    descriptionsToSheetNumbers[descriptionValue].Add(sheetToUpdate.SheetNumber);
                                                }
                                            }
                                        }

                                        // Второй проход: записать номера листов в параметр "WE_Описание Номер Листов" через запятую
                                        foreach (ViewSheet sheetToUpdate in sheets)
                                        {
                                            Parameter numberParam = sheetToUpdate.LookupParameter("WE_Описание Номер Листов");
                                            if (numberParam != null)
                                            {
                                                Parameter descriptionParam = sheetToUpdate.LookupParameter("WE_Описание Изм.");
                                                if (descriptionParam != null)
                                                {
                                                    string descriptionValue = descriptionParam.AsString();

                                                    if (!string.IsNullOrEmpty(descriptionValue) && descriptionsToSheetNumbers.ContainsKey(descriptionValue))
                                                    {
                                                        List<string> sheetNumbers = descriptionsToSheetNumbers[descriptionValue];
                                                        string combinedSheetNumbers = string.Join(", ", sheetNumbers);
                                                        numberParam.Set(combinedSheetNumbers);
                                                    }
                                                    else
                                                    {
                                                        // Если значения "WE_Описание Изм." нет в словаре или оно пустое, можно выполнить дополнительные действия по вашему выбору.
                                                        // Например, установить значение по умолчанию или оставить параметр пустым.
                                                        numberParam.Set(string.Empty);
                                                    }
                                                }
                                            }
                                        }

                                    }
                                }
                            }
                        }
                        tx.Commit();
                    }
                    return Result.Succeeded;
                }
                else
                {
                    // Если не выполнены условия, показать окно с ошибкой
                    MessageBox.Show("Ошибка: Вы должны выбрать хотя бы одну из опций и комплект.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return Result.Cancelled;
                }
            }
            return Result.Cancelled;
        }
        private List<string> GetCloudNumbers(ViewSheet sheet)
        {
            List<string> cloudNumbers = new List<string>();
            List<string> combinedList = new List<string>();

            // Получить параметры WE_ШифрСтатусЛиста1-4 и WE_ШифрСтатусЛиста5-8
            Guid statusParam1To4Guid = new Guid("15a2a838-1771-4a1a-b7e5-de985e7fe7ad"); // WE_ШифрСтатусЛиста1-4
            Guid statusParam5To8Guid = new Guid("461ae756-75c9-4215-8374-2ffa8dad32ee"); // WE_ШифрСтатусЛиста5-8
            Parameter status1To4Value = sheet.get_Parameter(statusParam1To4Guid);
            Parameter status5To8Value = sheet.get_Parameter(statusParam5To8Guid);
            string statusDigits1to4 = status1To4Value.AsValueString();
            string statusDigits5to8 = status5To8Value.AsValueString();

            // Получить все идентификаторы ревизий на листе с помощью GetAllRevisionIds
            ICollection<ElementId> revisionIds = sheet.GetAllRevisionIds();

            // Объединить значения параметров WE_ШифрСтатусЛиста1-4 и WE_ШифрСтатусЛиста5-8
            string combinedStatus = $"{statusDigits1to4}{statusDigits5to8}";
            var validValues = new List<string>();

            foreach (ElementId revisionId in revisionIds)
            {
                // Получить ревизию по идентификатору
                Revision revision = sheet.Document.GetElement(revisionId) as Revision;

                if (revision != null)
                {
                    string issuedTo = revision.IssuedTo;

                    if (!string.IsNullOrEmpty(issuedTo) && int.TryParse(issuedTo, out _))
                    {
                        validValues.Add(issuedTo);

                        // Эта часть кода была восстановлена
                        FilteredElementCollector cloudCollector = new FilteredElementCollector(sheet.Document, sheet.Id);
                        ICollection<Element> cloudsOnSheet = cloudCollector
                            .OfCategory(BuiltInCategory.OST_RevisionClouds)
                            .WhereElementIsNotElementType()
                            .ToElements();

                        foreach (Element cloudElem in cloudsOnSheet)
                        {
                            RevisionCloud cloud = cloudElem as RevisionCloud;
                            Parameter typeParam = cloud.get_Parameter(BuiltInParameter.REVISION_CLOUD_REVISION_ISSUED_TO);

                            if (typeParam != null && typeParam.HasValue)
                            {
                                string cloudType = typeParam.AsString();

                                // Разделить значения параметра на отдельные части
                                string[] issuedToValues = cloudType.Split(',');

                                // Проверить, что issuedToValues не пуст и содержит числа
                                validValues.AddRange(issuedToValues
                                    .Select(value => value.Trim())
                                    .Where(value => !string.IsNullOrEmpty(value) && int.TryParse(value, out _)));
                            }
                        }
                        // Конец восстановленной логики
                    }
                }
            }

            // Получить уникальные значения и отсортировать их по возрастанию
            validValues = validValues.Distinct().OrderBy(value => int.Parse(value)).ToList();

            foreach (var issuedToValue in validValues)
            {
                cloudNumbers.Add($"{issuedToValue}");
                if (combinedStatus.Length > cloudNumbers.Count - 1)
                {
                    combinedList.Add($"{issuedToValue}{GetCodeInBrackets(combinedStatus[cloudNumbers.Count - 1])}");
                }
            }

            return combinedList;
        }
        private string GetCodeInBrackets(char code)
        {
            switch (code)
            {
                case '2':
                    return "(.Зам)";
                case '3':
                    return "(.Нов)";
                case '4':
                    return "(.Аннул)";
                default:
                    return string.Empty; // If code is not 2, 3 or 4, return empty string
            }
        }
        private FamilyInstance FindTitleFamilyInstance(ViewSheet sheet)
        {
            // Найти экземпляр семейства с именем "WE_Титул_KR" на листе
            FilteredElementCollector collector = new FilteredElementCollector(sheet.Document, sheet.Id);
            collector.OfCategory(BuiltInCategory.OST_TitleBlocks);
            collector.OfClass(typeof(FamilyInstance));

            // Получите первый экземпляр семейства с заданным именем
            FamilyInstance titleInstance = collector.FirstOrDefault() as FamilyInstance;

            return titleInstance;
        }
    }
}