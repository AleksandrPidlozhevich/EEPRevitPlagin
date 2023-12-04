using Autodesk.Revit.UI;
using Autodesk.Revit.DB;
using Autodesk.Revit.Attributes;
using System.Collections.Generic;
using System.Linq;
using System.Data;
using System;

namespace EEPRevitPlagin.EEPRPCommandModules.ModelCompare
{
    [Transaction(TransactionMode.Manual)]
    internal class ModelCompareCommand : IExternalCommand
    {
        public static UIApplication uiapp;
        public static UIDocument uidoc;
        public static Document doc;
        public static ElementId resultsViewId;
        public static string exportTo;
        public static string resViewName = "";
        public static bool needToCreat;
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            uiapp = commandData.Application;
            Autodesk.Revit.ApplicationServices.Application app = uiapp.Application;
            uidoc = uiapp.ActiveUIDocument;
            doc = uidoc.Document;
            IExternalEventHandler handler_event = new ExternalEventMy();
            ExternalEvent exEvent = ExternalEvent.Create(handler_event);
            ModelCompareWPF exportWPF = new ModelCompareWPF(exEvent);
            exportWPF.Topmost = true;
            exportWPF.Show();
            return Result.Succeeded;
        }
        class ExternalEventMy : IExternalEventHandler
        {
            public void Execute(UIApplication uiapp)
            {
                UIDocument uidoc = ModelCompareCommand.uiapp.ActiveUIDocument;
                if (null == uidoc)
                {
                    return; // no document, nothing to do
                }

                //Document doc = uidoc.Document;
                using (Transaction tx = new Transaction(doc))
                {
                    tx.Start("Creat result view");
                    if (needToCreat)
                    {
                        IEnumerable<ViewFamilyType> viewFamilyTypes = from elem in new FilteredElementCollector(doc).OfClass(typeof(ViewFamilyType))
                                                                      let type = elem as ViewFamilyType
                                                                      where type.ViewFamily == ViewFamily.ThreeDimensional
                                                                      select type;
                        View3D view3D = View3D.CreateIsometric(doc, viewFamilyTypes.First().Id);
                        view3D.Name = "results";
                        resultsViewId = view3D.Id;
                    }
                    else
                    {
                        FilteredElementCollector viewCollector = new FilteredElementCollector(doc);
                        viewCollector.OfClass(typeof(View3D));
                        foreach (View3D view3D in viewCollector)
                        {
                            if (view3D.Name == resViewName)
                            {
                                resultsViewId = view3D.Id;
                                break;
                            }
                        }
                    }
                    try
                    {
                        DataSet data = new DataSet();
                        data.ReadXml(@exportTo);
                        DataTable dataTable1 = data.Tables[0];
                        //Autodesk.Revit.DB.Color color = new Autodesk.Revit.DB.Color(255, 0, 0);
                        OverrideGraphicSettings ogs = new OverrideGraphicSettings();
                        ogs.SetSurfaceForegroundPatternId(FillPatternElement.GetFillPatternElementByName(doc, FillPatternTarget.Drafting, "<Сплошная заливка>").Id);
                        //ogs.SetSurfaceForegroundPatternColor(color);
                        ogs.SetSurfaceBackgroundPatternVisible(false);
                        ogs.SetCutBackgroundPatternVisible(false);
                        ogs.SetCutForegroundPatternVisible(false);
                        ElementId elementId1;
                        //externalEvent.Raise();                
                        FilteredElementCollector viewCollector = new FilteredElementCollector(doc);
                        viewCollector.OfClass(typeof(View3D));
                        View view1 = (View)doc.GetElement(resultsViewId);
                        foreach (DataRow row in dataTable1.Rows)
                        {
                            if (Convert.ToInt32(row[1].ToString()) == 0)
                            {
                                Color color = new Color(255, 0, 0);
                                ogs.SetSurfaceForegroundPatternColor(color);
                                elementId1 = new ElementId(Convert.ToInt32(row[0].ToString()));
                                view1.SetElementOverrides(elementId1, ogs);
                            }
                            else if (Convert.ToInt32(row[1].ToString()) == 2)
                            {
                                Color color = new Color(0, 0, 255);
                                ogs.SetSurfaceForegroundPatternColor(color);
                                elementId1 = new ElementId(Convert.ToInt32(row[0].ToString()));
                                view1.SetElementOverrides(elementId1, ogs);
                            }
                        }
                    }
                    catch
                    {

                    }
                    tx.Commit();
                }
            }
            public string GetName()
            {
                return "my event";
            }
        }
    }
}
