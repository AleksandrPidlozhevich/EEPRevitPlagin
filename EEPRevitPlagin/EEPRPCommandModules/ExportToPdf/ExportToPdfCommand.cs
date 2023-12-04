using Autodesk.Revit.UI;
using Autodesk.Revit.DB;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.UI.Events;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;
using System.Windows.Controls;

namespace EEPRevitPlagin.EEPRPCommandModules.ExportToPdf
{
    [Transaction(TransactionMode.Manual)]
    internal class ExportToPdfCommand : IExternalCommand
    {
        public static UIApplication uiapp;
        public static UIDocument uidoc;
        public static Document doc;
        public static List<ElementId> viewSheetsIds = new List<ElementId>();
        public static List<Element> printSettings=new List<Element>();
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            uiapp = commandData.Application;
            uidoc = uiapp.ActiveUIDocument;
            Autodesk.Revit.ApplicationServices.Application app = uiapp.Application;
            doc = uidoc.Document;            
            viewSheetsIds = new FilteredElementCollector(doc).OfClass(typeof(ViewSheet)).ToElementIds().ToList();            
            IExternalEventHandler handler_event = new ExternalEventMy();
            ExternalEvent exEvent = ExternalEvent.Create(handler_event);
            ExportToPdfWPF exportToPdfWPF = new ExportToPdfWPF(exEvent);
            exportToPdfWPF.Topmost = true;
            exportToPdfWPF.Show();
            return Result.Succeeded;
        }
        class ExternalEventMy : IExternalEventHandler
        {
            public void Execute(UIApplication uiapp)
            {
                UIDocument uidoc = ExportToPdfCommand.uiapp.ActiveUIDocument;
                if (null == uidoc)
                {
                    return;
                }
                printSettings = new FilteredElementCollector(ExportToPdfCommand.doc).OfClass(typeof(PrintSetting)).ToElements().ToList();
                //Document doc = uidoc.Document;

                using (Transaction tx = new Transaction(doc, "NumberLists"))
                {
                    tx.Start();
                    PrintSetting printSetting = (PrintSetting)printSettings[5];
                    PaperSizeSet paperSizeSet= doc.PrintManager.PaperSizes;
                    
                    doc.PrintManager.PrintSetup.SaveAs("We Got you Bitch");
                    //ElementId newElement= ElementTransformUtils.CopyElement(doc, printSettings[0].Id, new XYZ(0, 0, 0)).First();
                    //doc.GetElement(newElement).Name = "We_Get_it_Bitch";
                    //PrintParameters printParameters = printSetting.PrintParameters;
                    //((PrintSetting)printSettings[5]).PrintParameters.ColorDepth = ColorDepthType.GrayScale;
                    foreach (Sheet sheet in ExportToPdfWPF.sheets)
                    {
                        if (sheet.ToExport)
                        {
                            //PrintSheet(sheet);
                        }
                    }
                    tx.Commit();
                }
            }

            public string GetName()
            {
                return "my event";
            }
            private void PrintSheet(Sheet sheet)
            {
                


                View view = (View)doc.GetElement(sheet.Id);
                PrintManager printManager = doc.PrintManager;
               
                printManager.PrintRange = PrintRange.Select;
                printManager.Apply();
                ViewSet viewSet = new ViewSet();
                viewSet.Insert(view);
                ViewSheetSetting viewSheetSetting = printManager.ViewSheetSetting;
                viewSheetSetting.CurrentViewSheetSet.Views = viewSet;
                viewSheetSetting.SaveAs("Current Print");
                printManager.SelectNewPrintDriver("PDF24");
                printManager.CombinedFile = true;
                printManager.PrintToFile = true;                
                printManager.PrintToFileName = @"C:\as3.pdf";
                doc.PrintManager.PrintSetup.SaveAs("We Got you Bitch");
                

                printManager.PrintSetup.CurrentPrintSetting =(PrintSetting) printSettings[5];
                printManager.Apply();
                printManager.SubmitPrint();
                viewSheetSetting.Delete();
            }
        }

    }

}
