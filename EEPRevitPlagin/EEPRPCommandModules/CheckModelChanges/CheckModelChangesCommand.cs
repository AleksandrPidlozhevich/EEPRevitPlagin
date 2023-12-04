using Autodesk.Revit.UI;
using Autodesk.Revit.DB;
using Autodesk.Revit.Attributes;
using System.Collections.Generic;
using System.Linq;
using System.Data;
using System;

namespace EEPRevitPlagin.EEPRPCommandModules.CheckModelChanges
{
    [Transaction(TransactionMode.Manual)]
    internal class CheckModelChangesCommand : IExternalCommand
    {
        public static UIApplication uiapp;
        public static UIDocument uidoc;
        public static Document doc;
        //public static ElementId resultsViewId;
        public static string exportTo;
        public static string resViewName = "";
        public static bool needToCreat;


        //public static Document linkedDoc;
        //public static RevitLinkInstance linkedDocInstance;
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            uiapp = commandData.Application;
            Autodesk.Revit.ApplicationServices.Application app = uiapp.Application;
            uidoc = uiapp.ActiveUIDocument;
            doc = uidoc.Document;
            IExternalEventHandler handler_event = new ExternalEventMy();
            ExternalEvent exEvent = ExternalEvent.Create(handler_event);
            CheckModelChangesWPF checkModelChangesWPF = new CheckModelChangesWPF(exEvent);
            checkModelChangesWPF.Topmost = true;
            checkModelChangesWPF.Show();
            return Result.Succeeded;
        }
        class ExternalEventMy : IExternalEventHandler
        {
            public void Execute(UIApplication uiapp)
            {
                UIDocument uidoc = CheckModelChangesCommand.uiapp.ActiveUIDocument;
                if (null == uidoc)
                {
                    return; // no document, nothing to do
                }

                //Document doc = uidoc.Document;
                using (Transaction tx = new Transaction(doc))
                {
                    //DocumentSet linkDocs = doc.Application.Documents;
                    //Reference reference = new Reference(linkedDoc.GetElement(new ElementId(Convert.ToInt32(256158)))).CreateLinkReference(linkedDocInstance);

                    //IndependentTag.Create(doc, doc.ActiveView.Id,reference , true, TagMode.TM_ADDBY_CATEGORY, TagOrientation.Horizontal, new XYZ(-20, -20, 0));
                    /*
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
                    */

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
