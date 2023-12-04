using Autodesk.Revit.UI;
using Autodesk.Revit.DB;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.UI.Events;
using System.Collections.Generic;

namespace EEPRevitPlagin.EEPRPCommandModules.WallSplit
{
    [Transaction(TransactionMode.Manual)]
    internal class WallSplitCommand : IExternalCommand
    {
        public static UIApplication uiapp;
        public static UIDocument uidoc;
        public static Document doc;
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            try
            {
                uiapp = commandData.Application;
                Autodesk.Revit.ApplicationServices.Application app = uiapp.Application;
                uidoc = uiapp.ActiveUIDocument;
                doc = uidoc.Document;
                using (Transaction tran = new Transaction(doc, "Adding Walls"))
                {
                    tran.Start();
                    IList<ElementId> selectedElements = (IList<ElementId>)uidoc.Selection.GetElementIds();
                    List<ElementId> walls = new List<ElementId>();
                    FilteredElementCollector f1 = new FilteredElementCollector(doc, selectedElements);
                    f1.OfClass(typeof(Wall));
                    walls = (List<ElementId>)f1.ToElementIds();
                    foreach (ElementId elementId in walls)
                    {
                        Wall wall = (Wall)doc.GetElement(elementId);
                        WallUtils.DisallowWallJoinAtEnd(wall, 0);
                        WallUtils.DisallowWallJoinAtEnd(wall, 1);
                        double S = 0;
                        foreach (CompoundStructureLayer layer in wall.WallType.GetCompoundStructure().GetLayers())
                        {
                            double th = layer.Width;
                            S += th;
                            IList<ElementId> newWall = (IList<ElementId>)ElementTransformUtils.CopyElement(doc, elementId, new XYZ(0, 0, 0));
                            Wall newWallele = (Wall)doc.GetElement(newWall[0]);
                            int requiredLayerId = layer.LayerId;
                            IList<Element> wallTypes = new FilteredElementCollector(doc).OfClass(typeof(WallType)).ToElements();
                            string wallTypeName = doc.GetElement(layer.MaterialId).Name + " - " + requiredLayerId.ToString();
                            WallType newWallType = null;
                            foreach (Element wallType in wallTypes)
                            {
                                if (wallType.Name == wallTypeName)
                                {
                                    newWallType = (WallType)wallType;
                                    break;
                                }
                            }
                            if (newWallType == null)
                            {
                                newWallType = (WallType)newWallele.WallType.Duplicate(wallTypeName);
                            }
                            CompoundStructure ss = newWallType.GetCompoundStructure();
                            foreach (CompoundStructureLayer layer1 in ss.GetLayers())
                            {
                                int layerId = layer1.LayerId;
                                if (layerId != requiredLayerId)
                                {
                                    for (int i = 0; i < ss.GetLayers().Count; i++)
                                    {
                                        if (ss.GetLayers()[i].LayerId == layerId)
                                        {
                                            ss.DeleteLayer(i);
                                            break;
                                        }
                                    }
                                }
                            }
                            ss.SetLayers(ss.GetLayers());
                            newWallType.SetCompoundStructure(ss);
                            newWallele.WallType = newWallType;
                            double wallTh = wall.Width;
                            LocationCurve dir = (LocationCurve)newWallele.Location;
                            Line l = (Line)dir.Curve;
                            XYZ v = l.Direction;
                            double dist = 0.5 * (wallTh - th) - (S - th);
                            newWallele.Location.Move(new XYZ(-v.Y, v.X, 0).Multiply(dist));
                        }
                        doc.Delete(elementId);
                    }
                    tran.Commit();
                }
            }
            catch
            {

            }
            return Result.Succeeded;
        }
    }
}
