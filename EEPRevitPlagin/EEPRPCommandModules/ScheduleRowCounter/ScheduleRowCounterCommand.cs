using Autodesk.Revit.UI;
using Autodesk.Revit.DB;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.UI.Events;
using System.Collections.Generic;
using System.Linq;
using System.Data;
using System;
using System.Collections;

namespace EEPRevitPlagin.EEPRPCommandModules.ScheduleRowCounter
{
    [Transaction(TransactionMode.Manual)]
    public class ScheduleRowCounterCommand : IExternalCommand
    {
        public static UIApplication uiapp;
        public static UIDocument uidoc;
        public static Document doc;
        public static IList<ElementId> selEles;
        public static TableData tableData;
        public static ViewSchedule viewSchedule;
        public static List<Element> elementsForEachRow;

        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            uiapp = commandData.Application;
            uidoc = uiapp.ActiveUIDocument;
            Autodesk.Revit.ApplicationServices.Application app = uiapp.Application;
            doc = uidoc.Document;
            ViewSchedule viewSchedule = (ViewSchedule)doc.ActiveView;
            tableData = viewSchedule.GetTableData();
            selEles = new FilteredElementCollector(doc, viewSchedule.Id).ToElementIds().ToList();
            IExternalEventHandler handler_event = new ExternalEventMy();
            ExternalEvent exEvent = ExternalEvent.Create(handler_event);
            ScheduleRowCounterWPF scheduleRowCounterWPF = new ScheduleRowCounterWPF(exEvent);
            scheduleRowCounterWPF.Topmost = true;
            scheduleRowCounterWPF.Show();
            return Result.Succeeded;
        }
        public static List<ElementId> GetElementsOfRow(Document doc, ViewSchedule viewSchedule, int rowNumber, List<ElementId> elemIds)
        {
            TableData tableData = viewSchedule.GetTableData();
            TableSectionData tableSectionData = tableData.GetSectionData(SectionType.Body);
            List<ElementId> elementOfRow = new List<ElementId>();
            List<ElementId> otherIds = null;
            using (SubTransaction tx = new SubTransaction(doc))
            {
                tx.Start();
                using (SubTransaction tx2 = new SubTransaction(doc))
                {
                    tx2.Start();
                    try
                    {
                        tableSectionData.RemoveRow(rowNumber);
                    }
                    catch
                    {
                        return null;
                    }
                    tx2.Commit();
                }
                otherIds = new FilteredElementCollector(doc, viewSchedule.Id).ToElementIds().ToList();
                tx.RollBack();
            }
            foreach (ElementId id in elemIds)
            {
                if (otherIds.Contains(id))
                {
                    continue;
                }
                elementOfRow.Add(id);
            }
            return elementOfRow;
        }
    }
    class ExternalEventMy : IExternalEventHandler
    {
        public void Execute(UIApplication uiapp)
        {
            try
            {
                UIDocument uidoc = ScheduleRowCounterCommand.uiapp.ActiveUIDocument;
                ViewSchedule viewSchedule = (ViewSchedule)uidoc.Document.ActiveView;
                TableData tableData = viewSchedule.GetTableData();
                TableSectionData tableSectionData = tableData.GetSectionData(SectionType.Body);
                Dictionary<string, List<ElementId>> rowElements = new Dictionary<string, List<ElementId>>();
                string paraId = ScheduleRowCounterWPF.paraId;
                string paraName = ScheduleRowCounterWPF.paraName;
                string paraType = ScheduleRowCounterWPF.paraType;
                int paraOrder = ScheduleRowCounterWPF.paraOrder;
                int j;
                List<int> ints = new List<int>();
                List<ElementId> elemIds = new FilteredElementCollector(uidoc.Document, viewSchedule.Id).ToElementIds().ToList();
                using (Transaction tx = new Transaction(uidoc.Document, "ss"))
                {
                    tx.Start();
                    for (int i = ScheduleRowCounterWPF.endAtRow; i > ScheduleRowCounterWPF.startFromRow - 1; i--)
                    {
                        List<ElementId> elementOfRow = new List<ElementId>();
                        List<ElementId> otherIds = null;
                        j = (i - ScheduleRowCounterWPF.startFromRow) * ScheduleRowCounterWPF.step + ScheduleRowCounterWPF.startFrom;
                        //string value = ScheduleRowCounterWPF.prefix + j.ToString() + ScheduleRowCounterWPF.suffix;
                        using (SubTransaction tx1 = new SubTransaction(uidoc.Document))
                        {
                            tx1.Start();
                            using (SubTransaction tx2 = new SubTransaction(uidoc.Document))
                            {
                                tx2.Start();
                                try
                                {
                                    tableSectionData.RemoveRow(i);
                                }
                                catch
                                {
                                    ints.Add(i);
                                }
                                tx2.Commit();
                            }
                            otherIds = new FilteredElementCollector(uidoc.Document, viewSchedule.Id).ToElementIds().ToList();
                            //if (otherIds.Count == elemIds.Count)
                            //{
                            //j -= ScheduleRowCounterWPF.step;
                            //}
                        }
                        foreach (ElementId id in elemIds)
                        {
                            if (otherIds.Contains(id))
                            {
                                continue;
                            }
                            elementOfRow.Add(id);
                        }
                        elemIds = otherIds;
                        if (elementOfRow.Count > 0)
                        {
                            rowElements.Add(j.ToString(), elementOfRow);
                        }
                    }
                    tx.RollBack();
                }

                string valueIn = "";
                string value = "";
                using (Transaction tx3 = new Transaction(uidoc.Document, "Add numbers to schedule"))
                {
                    tx3.Start();
                    foreach (KeyValuePair<string, List<ElementId>> a in rowElements)
                    {
                        valueIn = a.Key;
                        foreach (int i in ints)
                        {
                            if (Convert.ToInt16(a.Key) > i * ScheduleRowCounterWPF.step + ScheduleRowCounterWPF.startFrom - ScheduleRowCounterWPF.startFromRow * ScheduleRowCounterWPF.step)
                            {
                                valueIn = (Convert.ToInt16(valueIn) - ScheduleRowCounterWPF.step).ToString();
                            }
                        }
                        value = ScheduleRowCounterWPF.prefix + valueIn + ScheduleRowCounterWPF.suffix;
                        foreach (ElementId id in a.Value)
                        {
                            if (paraType == "Instance")
                            {
                                uidoc.Document.GetElement(id).LookupParameter(paraName).Set(value);
                            }
                            if (paraType == "ElementType")
                            {
                                uidoc.Document.GetElement(uidoc.Document.GetElement(id).GetTypeId()).LookupParameter(paraName).Set(value);
                            }
                        }
                    }
                    tx3.Commit();
                }
            }
            catch
            {
            }

        }
        public string GetName()
        {
            return "my event";
        }
    }

}
