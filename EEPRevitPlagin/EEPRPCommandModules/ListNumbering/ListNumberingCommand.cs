using Autodesk.Revit.UI;
using Autodesk.Revit.DB;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.UI.Events;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;
using System.Windows.Controls;

namespace EEPRevitPlagin.EEPRPCommandModules.ListNumbering
{
    [Transaction(TransactionMode.Manual)]
    internal class ListNumberingCommand : IExternalCommand
    {
        public static UIApplication uiapp;
        public static UIDocument uidoc;
        public static Document doc;
        public static int levelsCount;
        public static List<List<string>> levels = new List<List<string>>();
        public static List<Sheet> sheets = new List<Sheet>();
        public static List<Parameter> SheetParameters = new List<Parameter>();
        public static List<string> sheetsToAddNumber = new List<string>();
        public static string param;
        public static string seperator;
        public static string seperatorIn;


        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            uiapp = commandData.Application;
            uidoc = uiapp.ActiveUIDocument;
            Autodesk.Revit.ApplicationServices.Application app = uiapp.Application;
            doc = uidoc.Document;
            SheetParameters.Clear();
            IList<ElementId> viewSheetsIds = new FilteredElementCollector(doc).OfClass(typeof(ViewSheet)).ToElementIds().ToList();
            foreach (Parameter para in doc.GetElement(viewSheetsIds[0]).Parameters)
            {
                if (!para.IsReadOnly && para.StorageType == StorageType.String)
                {
                    if (!SheetParameters.Contains(para))
                    {
                        SheetParameters.Add(para);
                    }
                }
            }
            SheetParameters.Sort(new CompareParameters());
            BrowserOrganization browserOrganization = BrowserOrganization.GetCurrentBrowserOrganizationForSheets(doc);
            levelsCount = browserOrganization.GetFolderItems(viewSheetsIds[0]).Count;
            for (int i = 0; i < levelsCount; i++)
            {
                levels.Add(new List<string>());
            }
            sheets.Clear();
            foreach (ElementId viewSheetsId in viewSheetsIds)
            {
                string level1 = "";
                string level2 = "";
                string level3 = "";
                string level4 = "";
                string level5 = "";
                List<FolderItemInfo> items = (List<FolderItemInfo>)browserOrganization.GetFolderItems(viewSheetsId);
                for (int i = items.Count - 1; i > -1; i--)
                {
                    try
                    {
                        switch (i + 1)
                        {
                            case 1:
                                level1 = items[i].Name;
                                break;
                            case 2:
                                level2 = items[i].Name;
                                break;
                            case 3:
                                level3 = items[i].Name;
                                break;
                            case 4:
                                level4 = items[i].Name;
                                break;
                            case 5:
                                level5 = items[i].Name;
                                break;
                        }
                    }
                    catch
                    {

                    }
                }
                sheets.Add(new Sheet() { Id = viewSheetsId.IntegerValue, Level1 = level1, Level2 = level2, Level3 = level3, Level4 = level4, Level5 = level5, Name = doc.GetElement(viewSheetsId).Name, NumberText = ((ViewSheet)doc.GetElement(viewSheetsId)).SheetNumber });
                List<string> itemsNames = new List<string>();
                for (int i = 0; i < items.Count; i++)
                {
                    itemsNames.Add(items[i].Name);
                }
                if (!levels.Contains(itemsNames))
                {
                    levels.Add(itemsNames);
                }

            }
            IExternalEventHandler handler_event = new ExternalEventMy();
            ExternalEvent exEvent = ExternalEvent.Create(handler_event);
            ListNumberingWPF listNumberingWPF = new ListNumberingWPF(exEvent);
            listNumberingWPF.Topmost = true;
            listNumberingWPF.Show();

            return Result.Succeeded;
        }

        class CompareParameters : IComparer<Parameter>
        {
            public int Compare(Parameter x, Parameter y)
            {
                List<string> strings = new List<string>();
                strings.Add(x.Definition.Name);
                strings.Add(y.Definition.Name);
                strings.Sort();
                if (strings[0] == x.Definition.Name)
                {
                    return -1;
                }
                else
                {
                    return 1;
                }
            }
        }
        class ExternalEventMy : IExternalEventHandler
        {
            public void Execute(UIApplication uiapp)
            {
                UIDocument uidoc = ListNumberingCommand.uiapp.ActiveUIDocument;
                if (null == uidoc)
                {
                    return; // no document, nothing to do
                }
                //Document doc = uidoc.Document;
                using (Transaction tx = new Transaction(doc, "NumberLists"))
                {
                    tx.Start();
                    for (int i = 0; i < sheetsToAddNumber.Count; i++)
                    {
                        //get the list
                        ElementId elementId = new ElementId(Convert.ToInt32(sheetsToAddNumber[i].ToString().Split('$')[1]));
                        ViewSheet sheet = (ViewSheet)doc.GetElement(elementId);
                        //get Value
                        string newListNumber = "";

                        for (int j = 0; j < 6; j++)
                        {
                            if (ListNumberingWPF.parts[j].IsActive)
                            {
                                string partNumber = "";
                                int t = 0;
                                for (int k = 0; k < 5; k++)
                                {

                                    string partIn = "";
                                    switch (k)
                                    {
                                        case 0:
                                            if (ListNumberingWPF.parts[j].NeedToPrefixparam)
                                            {
                                                partIn = sheet.LookupParameter(ListNumberingWPF.parts[j].PrefixParameterName).AsString();
                                            }
                                            break;
                                        case 1:
                                            if (ListNumberingWPF.parts[j].Prefix != "")
                                            {
                                                partIn = ListNumberingWPF.parts[j].Prefix;
                                            }
                                            break;
                                        case 2:
                                            partIn = (ListNumberingWPF.parts[j].StartNumber + i * ListNumberingWPF.parts[j].Step).ToString("D" + ListNumberingWPF.parts[j].Format.ToString());

                                            break;
                                        case 3:
                                            if (ListNumberingWPF.parts[j].Suffix != "")
                                            {
                                                partIn = ListNumberingWPF.parts[j].Suffix;
                                            }
                                            break;
                                        case 4:
                                            if (ListNumberingWPF.parts[j].NeedToSuffixparam)
                                            {
                                                partIn = sheet.LookupParameter(ListNumberingWPF.parts[j].SuffixParameterName).AsString();
                                            }
                                            break;
                                    }
                                    if (k != 0)
                                    {
                                        if (partIn != "")
                                        {
                                            if (t == 0)
                                            {
                                                partNumber = partIn;
                                                t++;
                                            }
                                            else
                                            {
                                                partNumber = partNumber + ListNumberingWPF.parts[j].PartSeperator + partIn;
                                                t++;
                                            }
                                        }
                                    }
                                    else
                                    {
                                        if (partIn != "")
                                        {
                                            partNumber = partIn;
                                            t++;
                                        }
                                    }
                                }
                                if (j != 0)
                                {
                                    newListNumber = newListNumber + seperator + partNumber;
                                }
                                else
                                {
                                    newListNumber = partNumber;
                                }
                            }
                        }
                        //number on the list
                        //sheet.LookupParameter(param).Set(newListNumber);
                        //if(ListNumberingWPF.use)

                        sheet.LookupParameter(param).Set(sheetsToAddNumber[i].ToString().Split('$')[0]);
                        //set the sheetNumber
                        sheet.SheetNumber = sheetsToAddNumber[i].ToString().Split('$')[2] + "@@@";
                    }
                    for (int i = 0; i < sheetsToAddNumber.Count; i++)
                    {
                        //get the list
                        ElementId elementId = new ElementId(Convert.ToInt32(sheetsToAddNumber[i].ToString().Split('$')[1]));
                        ViewSheet sheet = (ViewSheet)doc.GetElement(elementId);
                        sheet.SheetNumber = sheet.SheetNumber.Replace("@@@", "");
                    }
                    tx.Commit();
                }
                DockablePaneId dpId = DockablePanes.BuiltInDockablePanes.ProjectBrowser;
                DockablePane dP = new DockablePane(dpId);
                dP.Show();

                using (Transaction tx2 = new Transaction(doc, "DateLinesVisibility"))
                {
                    tx2.Start();
                    for (int i = 0; i < sheetsToAddNumber.Count; i++)
                    {
                        ElementId elementId = new ElementId(Convert.ToInt32(sheetsToAddNumber[i].ToString().Split('$')[1]));
                        ElementCategoryFilter elementCategoryFilter = new ElementCategoryFilter(BuiltInCategory.OST_TitleBlocks);
                        Element sheet = doc.GetElement(doc.GetElement(elementId).GetDependentElements(elementCategoryFilter)[0]);
                        Element sheetType = doc.GetElement(sheet.GetTypeId());
                        //check for cancelling the opearation
                        for (int j = 1; j < 7; j++)
                        {
                            if (sheet.LookupParameter(string.Format("Подпись {0} по листу", j)) == null || sheetType.LookupParameter(string.Format("Подпись {0} по комплекту", j)) == null || sheet.LookupParameter(string.Format("Строка {0} Дата", j)) == null)
                            {
                                //TaskDialog.Show("Error", "Add required parameters");
                                continue;
                            }
                        }
                        for (int j = 1; j < 7; j++)
                        {
                            try
                            {
                                if (sheet.LookupParameter(string.Format("Подпись {0} по листу", j)).AsValueString() == "022_Нет подписи" & sheetType.LookupParameter(string.Format("Подпись {0} по комплекту", j)).AsValueString() == "022_Нет подписи")
                                {
                                    sheet.LookupParameter(string.Format("Строка {0} Дата", j)).Set((int)0);
                                }
                                else
                                {
                                    sheet.LookupParameter(string.Format("Строка {0} Дата", j)).Set((int)1);
                                }
                            }
                            catch
                            {

                            }

                        }
                    }
                    tx2.Commit();
                }

            }

            public string GetName()
            {
                return "my event";
            }
        }
        public class Sheet
        {
            public string Name { get; set; }
            public string Level1 { get; set; }
            public string Level2 { get; set; }
            public string Level3 { get; set; }
            public string Level4 { get; set; }
            public string Level5 { get; set; }
            public int Id { get; set; }
            public string NumberText { get; set; }

        }

    }
}

