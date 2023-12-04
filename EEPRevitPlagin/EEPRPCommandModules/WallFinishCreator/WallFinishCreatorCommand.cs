using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using EEPRevitPlagin.EEPRPCommandModules.WallFinishCreatorProperties;
using EEPRevitPlagin.SecondaryCommand;
using EEPRevitPlagin.SecondaryСommand;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Serialization;

namespace EEPRevitPlagin.EEPRPCommandModules.WallFinishCreator
{
    [Autodesk.Revit.Attributes.Transaction(Autodesk.Revit.Attributes.TransactionMode.Manual)]
    class WallFinishCreatorCommand : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            Document doc = commandData.Application.ActiveUIDocument.Document;
            Document linkDoc = null;
            Selection sel = commandData.Application.ActiveUIDocument.Selection;

            string wallFinishCreatorOption = GetWallFinishCreatorOptionsSettings();
            System.Resources.ResourceManager rs = new System.Resources.ResourceManager("EEPRevitPlagin.SecondaryCommand.LangResources.Language", System.Reflection.Assembly.GetExecutingAssembly());
            string createWallFinish =rs.GetString("CreateWallFinish");
            string chooseRoom= rs.GetString("Choose_room");
            string buildingWalls = rs.GetString("Building_walls");
            string mergingWalls = rs.GetString("Merging_walls");
            string sorryCuttingOpenings = rs.GetString("Sorry_сutting_openings_in_walls_through_the_API_appeared_only_in_the_2021_version");
            string selectLinkedFile = rs.GetString("Select_a_linked_file");
            string linkedFileNotFound = rs.GetString("Linked_file_not_found");
            string cuttingHoles = rs.GetString("Cutting_holes");
            string somethingWrongWallFinishProperties = rs.GetString("Something_wrong!WallFinishProperties");
            using (TransactionGroup tg = new TransactionGroup(doc))
            {
                tg.Start(createWallFinish);
                if (wallFinishCreatorOption == "rbt_ByCurrentFile")
                {
                    List<Room> roomList = new List<Room>();
                    if (roomList.Count == 0)
                    {
                        RoomSelectionFilter roomSelectionFilter = new RoomSelectionFilter();
                        IList<Reference> selRooms = null;
                        try
                        {
                            selRooms = sel.PickObjects(ObjectType.Element, roomSelectionFilter, chooseRoom);
                        }
                        catch (Autodesk.Revit.Exceptions.OperationCanceledException)
                        {
                            return Result.Cancelled;
                        }

                        foreach (Reference roomRef in selRooms)
                        {
                            roomList.Add(doc.GetElement(roomRef.ElementId) as Room);
                        }
                    }

                    List<Material> materialsList = GetMaterialsListFromBoundarySegments(doc, roomList);
                    List<WallType> wallTypesList = new FilteredElementCollector(doc)
                        .OfClass(typeof(WallType))
                        .Where(wt => wt.Category.Id.IntegerValue.Equals((int)BuiltInCategory.OST_Walls))
                        .Cast<WallType>()
                        .Where(wt => wt.get_Parameter(BuiltInParameter.ALL_MODEL_MODEL).AsString() == "Отделка стен"|| wt.get_Parameter(BuiltInParameter.ALL_MODEL_MODEL).AsString() == "Finish wall")
                        .OrderBy(wt => wt.Name, new ComparatorString())
                        .ToList();

                    //Form call
                    WallFinishCreatorWPF wallFinishCreatorWPF = new WallFinishCreatorWPF(materialsList, wallTypesList);
                    wallFinishCreatorWPF.ShowDialog();
                    if (wallFinishCreatorWPF.DialogResult != true)
                    {
                        return Result.Cancelled;
                    }
                    List<WallFinishCreatorCollectionItem> wallFinishCreatorCollectionItemList = wallFinishCreatorWPF.WallFinishCreatorCollectionItemsList.ToList();
                    double wallFinishHeight;
                    double.TryParse(wallFinishCreatorWPF.WallFinishHeight, out wallFinishHeight);
                    wallFinishHeight = wallFinishHeight / 304.8;
                    bool roomBoundary = wallFinishCreatorWPF.RoomBoundary;

                    List<Wall> wallFinishList = new List<Wall>();
                    using (Transaction t = new Transaction(doc))
                    {
                        t.Start(buildingWalls);
                        foreach (Room room in roomList)
                        {
                            Level selectedLevel = room.Level as Level;
                            IList<IList<BoundarySegment>> roomBoundarySegmentsListsList = room.GetBoundarySegments(new SpatialElementBoundaryOptions());

                            foreach (IList<BoundarySegment> roomBoundarySegmentsList in roomBoundarySegmentsListsList)
                            {
                                XYZ resultingStartPoint = null;
                                XYZ resultingEndPoint = null;
                                XYZ resultingCurveOfBoundarySegmentDirection = null;
                                Material resultingElementOfBoundarySegmentMaterial = null;
                                foreach (BoundarySegment roomBoundarySegment in roomBoundarySegmentsList)
                                {
                                    Curve curveOfBoundarySegment = roomBoundarySegment.GetCurve();

                                    Element elementOfBoundarySegment = doc.GetElement(roomBoundarySegment.ElementId);
                                    Material elementOfBoundarySegmentMaterial = null;
                                    if (elementOfBoundarySegment != null && elementOfBoundarySegment.Category.Id
                                        .IntegerValue.Equals((int)BuiltInCategory.OST_Walls))
                                    {
                                        if (!(elementOfBoundarySegment is FamilyInstance))
                                        {
                                            if ((elementOfBoundarySegment as Wall).CurtainGrid == null)
                                            {
                                                elementOfBoundarySegmentMaterial = doc.GetElement((elementOfBoundarySegment as Wall)
                                                    .WallType
                                                    .GetCompoundStructure()
                                                    .GetMaterialId(0)) as Material;
                                            }
                                        }
                                        else if (elementOfBoundarySegment is FamilyInstance)
                                        {
                                            Parameter elementOfBoundarySegmentMaterialParam = (elementOfBoundarySegment as FamilyInstance)
                                                .Symbol
                                                .get_Parameter(BuiltInParameter.STRUCTURAL_MATERIAL_PARAM);
                                            if (elementOfBoundarySegmentMaterialParam != null)
                                            {
                                                elementOfBoundarySegmentMaterial = doc.GetElement(elementOfBoundarySegmentMaterialParam.AsElementId()) as Material;
                                            }
                                            else
                                            {
                                                elementOfBoundarySegmentMaterialParam = (elementOfBoundarySegment as FamilyInstance)
                                                    .get_Parameter(BuiltInParameter.STRUCTURAL_MATERIAL_PARAM);
                                                if (elementOfBoundarySegmentMaterialParam != null)
                                                {
                                                    elementOfBoundarySegmentMaterial = doc.GetElement(elementOfBoundarySegmentMaterialParam.AsElementId()) as Material;
                                                }
                                            }
                                        }
                                    }
                                    //If the border is a load bearing column
                                    else if (elementOfBoundarySegment != null && elementOfBoundarySegment.Category.Id
                                        .IntegerValue.Equals((int)BuiltInCategory.OST_StructuralColumns))
                                    {
                                        Parameter elementOfBoundarySegmentMaterialParam = (elementOfBoundarySegment as FamilyInstance)
                                            .Symbol
                                            .get_Parameter(BuiltInParameter.STRUCTURAL_MATERIAL_PARAM);
                                        if (elementOfBoundarySegmentMaterialParam != null)
                                        {
                                            elementOfBoundarySegmentMaterial = doc.GetElement(elementOfBoundarySegmentMaterialParam.AsElementId()) as Material;
                                        }
                                        if (elementOfBoundarySegmentMaterial == null)
                                        {
                                            elementOfBoundarySegmentMaterial = doc.GetElement((elementOfBoundarySegment as FamilyInstance)
                                                .get_Parameter(BuiltInParameter.STRUCTURAL_MATERIAL_PARAM)
                                                .AsElementId()) as Material;
                                        }
                                    }

                                    if (elementOfBoundarySegmentMaterial != null)
                                    {
                                        if (curveOfBoundarySegment is Line)
                                        {
                                            XYZ startPoint = curveOfBoundarySegment.GetEndPoint(0);
                                            XYZ endPoint = curveOfBoundarySegment.GetEndPoint(1);
                                            XYZ curveOfBoundarySegmentDirection = (curveOfBoundarySegment as Line).Direction;
                                            if (resultingStartPoint != null
                                                && resultingEndPoint != null
                                                && resultingCurveOfBoundarySegmentDirection != null
                                                && resultingElementOfBoundarySegmentMaterial != null)
                                            {
                                                if (resultingCurveOfBoundarySegmentDirection.IsAlmostEqualTo(curveOfBoundarySegmentDirection)
                                                    && resultingElementOfBoundarySegmentMaterial.Id == elementOfBoundarySegmentMaterial.Id)
                                                {
                                                    if ((roomBoundarySegmentsList.Count - 1) != roomBoundarySegmentsList.IndexOf(roomBoundarySegment))
                                                    {
                                                        resultingEndPoint = endPoint;
                                                        continue;
                                                    }
                                                    else
                                                    {
                                                        resultingEndPoint = endPoint;
                                                        Curve resultingWallFinishCurve = Line.CreateBound(resultingStartPoint, resultingEndPoint) as Curve;
                                                        WallType wt = wallFinishCreatorCollectionItemList.FirstOrDefault(i => i.BaseWallMaterial.Id == resultingElementOfBoundarySegmentMaterial.Id).WallFinishType;
                                                        Wall w = Wall.Create(doc, resultingWallFinishCurve, wt.Id, selectedLevel.Id, 10, 0, false, false) as Wall;
                                                        w.get_Parameter(BuiltInParameter.WALL_KEY_REF_PARAM).Set(3);
                                                        w.get_Parameter(BuiltInParameter.WALL_USER_HEIGHT_PARAM).Set(wallFinishHeight);
                                                        if (roomBoundary) w.get_Parameter(BuiltInParameter.WALL_ATTR_ROOM_BOUNDING).Set(1);
                                                        else w.get_Parameter(BuiltInParameter.WALL_ATTR_ROOM_BOUNDING).Set(0);
                                                        wallFinishList.Add(w);
                                                        XYZ wallOrientationVector = (resultingWallFinishCurve as Line).Direction.CrossProduct(XYZ.BasisZ).Negate();
                                                        ElementTransformUtils.MoveElement(doc, w.Id, wallOrientationVector * (w.WallType.Width / 2));
                                                        continue;
                                                    }
                                                }
                                                else
                                                {
                                                    if ((roomBoundarySegmentsList.Count - 1) != roomBoundarySegmentsList.IndexOf(roomBoundarySegment))
                                                    {
                                                        Curve resultingWallFinishCurve = Line.CreateBound(resultingStartPoint, resultingEndPoint) as Curve;
                                                        WallType wt = wallFinishCreatorCollectionItemList.FirstOrDefault(i => i.BaseWallMaterial.Id == resultingElementOfBoundarySegmentMaterial.Id).WallFinishType;
                                                        Wall w = Wall.Create(doc, resultingWallFinishCurve, wt.Id, selectedLevel.Id, 10, 0, false, false) as Wall;
                                                        w.get_Parameter(BuiltInParameter.WALL_KEY_REF_PARAM).Set(3);
                                                        w.get_Parameter(BuiltInParameter.WALL_USER_HEIGHT_PARAM).Set(wallFinishHeight);
                                                        if (roomBoundary) w.get_Parameter(BuiltInParameter.WALL_ATTR_ROOM_BOUNDING).Set(1);
                                                        else w.get_Parameter(BuiltInParameter.WALL_ATTR_ROOM_BOUNDING).Set(0);
                                                        wallFinishList.Add(w);
                                                        XYZ wallOrientationVector = (resultingWallFinishCurve as Line).Direction.CrossProduct(XYZ.BasisZ).Negate();
                                                        ElementTransformUtils.MoveElement(doc, w.Id, wallOrientationVector * (w.WallType.Width / 2));

                                                        resultingStartPoint = startPoint;
                                                        resultingEndPoint = endPoint;
                                                        resultingCurveOfBoundarySegmentDirection = curveOfBoundarySegmentDirection;
                                                        resultingElementOfBoundarySegmentMaterial = elementOfBoundarySegmentMaterial;
                                                    }
                                                    else
                                                    {
                                                        Curve resultingWallFinishCurve = Line.CreateBound(resultingStartPoint, resultingEndPoint) as Curve;
                                                        WallType wt = wallFinishCreatorCollectionItemList.FirstOrDefault(i => i.BaseWallMaterial.Id == resultingElementOfBoundarySegmentMaterial.Id).WallFinishType;
                                                        Wall w = Wall.Create(doc, resultingWallFinishCurve, wt.Id, selectedLevel.Id, 10, 0, false, false) as Wall;
                                                        w.get_Parameter(BuiltInParameter.WALL_KEY_REF_PARAM).Set(3);
                                                        w.get_Parameter(BuiltInParameter.WALL_USER_HEIGHT_PARAM).Set(wallFinishHeight);
                                                        if (roomBoundary) w.get_Parameter(BuiltInParameter.WALL_ATTR_ROOM_BOUNDING).Set(1);
                                                        else w.get_Parameter(BuiltInParameter.WALL_ATTR_ROOM_BOUNDING).Set(0);
                                                        wallFinishList.Add(w);
                                                        XYZ wallOrientationVector = (resultingWallFinishCurve as Line).Direction.CrossProduct(XYZ.BasisZ).Negate();
                                                        ElementTransformUtils.MoveElement(doc, w.Id, wallOrientationVector * (w.WallType.Width / 2));

                                                        resultingWallFinishCurve = curveOfBoundarySegment as Curve;
                                                        wt = wallFinishCreatorCollectionItemList.FirstOrDefault(i => i.BaseWallMaterial.Id == elementOfBoundarySegmentMaterial.Id).WallFinishType;
                                                        w = Wall.Create(doc, resultingWallFinishCurve, wt.Id, selectedLevel.Id, 10, 0, false, false) as Wall;
                                                        w.get_Parameter(BuiltInParameter.WALL_KEY_REF_PARAM).Set(3);
                                                        w.get_Parameter(BuiltInParameter.WALL_USER_HEIGHT_PARAM).Set(wallFinishHeight);
                                                        if (roomBoundary) w.get_Parameter(BuiltInParameter.WALL_ATTR_ROOM_BOUNDING).Set(1);
                                                        else w.get_Parameter(BuiltInParameter.WALL_ATTR_ROOM_BOUNDING).Set(0);
                                                        wallFinishList.Add(w);
                                                        wallOrientationVector = (resultingWallFinishCurve as Line).Direction.CrossProduct(XYZ.BasisZ).Negate();
                                                        ElementTransformUtils.MoveElement(doc, w.Id, wallOrientationVector * (w.WallType.Width / 2));
                                                    }
                                                }
                                            }
                                            else
                                            {
                                                if ((roomBoundarySegmentsList.Count - 1) != roomBoundarySegmentsList.IndexOf(roomBoundarySegment))
                                                {
                                                    resultingStartPoint = startPoint;
                                                    resultingEndPoint = endPoint;
                                                    resultingCurveOfBoundarySegmentDirection = curveOfBoundarySegmentDirection;
                                                    resultingElementOfBoundarySegmentMaterial = elementOfBoundarySegmentMaterial;
                                                    continue;
                                                }
                                                else
                                                {
                                                    Curve resultingWallFinishCurve = curveOfBoundarySegment as Curve;
                                                    WallType wt = wallFinishCreatorCollectionItemList.FirstOrDefault(i => i.BaseWallMaterial.Id == elementOfBoundarySegmentMaterial.Id).WallFinishType;
                                                    Wall w = Wall.Create(doc, resultingWallFinishCurve, wt.Id, selectedLevel.Id, 10, 0, false, false) as Wall;
                                                    w.get_Parameter(BuiltInParameter.WALL_KEY_REF_PARAM).Set(3);
                                                    w.get_Parameter(BuiltInParameter.WALL_USER_HEIGHT_PARAM).Set(wallFinishHeight);
                                                    if (roomBoundary) w.get_Parameter(BuiltInParameter.WALL_ATTR_ROOM_BOUNDING).Set(1);
                                                    else w.get_Parameter(BuiltInParameter.WALL_ATTR_ROOM_BOUNDING).Set(0);
                                                    wallFinishList.Add(w);
                                                    XYZ wallOrientationVector = (resultingWallFinishCurve as Line).Direction.CrossProduct(XYZ.BasisZ).Negate();
                                                    ElementTransformUtils.MoveElement(doc, w.Id, wallOrientationVector * (w.WallType.Width / 2));
                                                }
                                            }
                                        }
                                        else if (curveOfBoundarySegment is Arc)
                                        {
                                            if (resultingStartPoint != null
                                                && resultingEndPoint != null
                                                && resultingCurveOfBoundarySegmentDirection != null
                                                && resultingElementOfBoundarySegmentMaterial != null)
                                            {
                                                Curve resultingWallFinishCurve = Line.CreateBound(resultingStartPoint, resultingEndPoint) as Curve;
                                                WallType wt = wallFinishCreatorCollectionItemList.FirstOrDefault(i => i.BaseWallMaterial.Id == resultingElementOfBoundarySegmentMaterial.Id).WallFinishType;
                                                Wall w = Wall.Create(doc, resultingWallFinishCurve, wt.Id, selectedLevel.Id, 10, 0, false, false) as Wall;
                                                w.get_Parameter(BuiltInParameter.WALL_KEY_REF_PARAM).Set(3);
                                                w.get_Parameter(BuiltInParameter.WALL_USER_HEIGHT_PARAM).Set(wallFinishHeight);
                                                if (roomBoundary) w.get_Parameter(BuiltInParameter.WALL_ATTR_ROOM_BOUNDING).Set(1);
                                                else w.get_Parameter(BuiltInParameter.WALL_ATTR_ROOM_BOUNDING).Set(0);
                                                wallFinishList.Add(w);
                                                XYZ wallOrientationVector = (resultingWallFinishCurve as Line).Direction.CrossProduct(XYZ.BasisZ).Negate();
                                                ElementTransformUtils.MoveElement(doc, w.Id, wallOrientationVector * (w.WallType.Width / 2));

                                                resultingStartPoint = null;
                                                resultingEndPoint = null;
                                                resultingCurveOfBoundarySegmentDirection = null;
                                                resultingElementOfBoundarySegmentMaterial = null;

                                                wt = wallFinishCreatorCollectionItemList
                                                    .FirstOrDefault(i => i.BaseWallMaterial.Id == elementOfBoundarySegmentMaterial.Id)
                                                    .WallFinishType;
                                                double arcOfset = wt.Width / 2;

                                                XYZ arcVector = null;
                                                if (elementOfBoundarySegment is Wall)
                                                {
                                                    arcVector = (curveOfBoundarySegment as Arc).Normal.Negate();
                                                }
                                                else if (elementOfBoundarySegment is FamilyInstance)
                                                {
                                                    arcVector = (curveOfBoundarySegment as Arc).Normal;
                                                }

                                                resultingWallFinishCurve = (curveOfBoundarySegment as Arc).CreateOffset(arcOfset, arcVector) as Curve;
                                                w = Wall.Create(doc, resultingWallFinishCurve, wt.Id, selectedLevel.Id, 10, 0, false, false) as Wall;
                                                w.get_Parameter(BuiltInParameter.WALL_KEY_REF_PARAM).Set(3);
                                                w.get_Parameter(BuiltInParameter.WALL_USER_HEIGHT_PARAM).Set(wallFinishHeight);
                                                if (roomBoundary) w.get_Parameter(BuiltInParameter.WALL_ATTR_ROOM_BOUNDING).Set(1);
                                                else w.get_Parameter(BuiltInParameter.WALL_ATTR_ROOM_BOUNDING).Set(0);
                                                wallFinishList.Add(w);
                                            }
                                            else
                                            {
                                                WallType wt = wallFinishCreatorCollectionItemList.FirstOrDefault(i => i.BaseWallMaterial.Id == elementOfBoundarySegmentMaterial.Id).WallFinishType;
                                                double arcOfset = wt.Width / 2;

                                                XYZ arcVector = null;
                                                if (elementOfBoundarySegment is Wall)
                                                {
                                                    arcVector = (curveOfBoundarySegment as Arc).Normal.Negate();
                                                }
                                                else if (elementOfBoundarySegment is FamilyInstance)
                                                {
                                                    arcVector = (curveOfBoundarySegment as Arc).Normal;
                                                }

                                                Curve resultingWallFinishCurve = (curveOfBoundarySegment as Arc).CreateOffset(arcOfset, arcVector) as Curve;
                                                Wall w = Wall.Create(doc, resultingWallFinishCurve, wt.Id, selectedLevel.Id, 10, 0, false, false) as Wall;
                                                w.get_Parameter(BuiltInParameter.WALL_KEY_REF_PARAM).Set(3);
                                                w.get_Parameter(BuiltInParameter.WALL_USER_HEIGHT_PARAM).Set(wallFinishHeight);
                                                if (roomBoundary) w.get_Parameter(BuiltInParameter.WALL_ATTR_ROOM_BOUNDING).Set(1);
                                                else w.get_Parameter(BuiltInParameter.WALL_ATTR_ROOM_BOUNDING).Set(0);
                                                wallFinishList.Add(w);
                                            }
                                        }
                                    }
                                    else
                                    {
                                        if (resultingStartPoint != null
                                        && resultingEndPoint != null
                                        && resultingCurveOfBoundarySegmentDirection != null
                                        && resultingElementOfBoundarySegmentMaterial != null)
                                        {
                                            Curve resultingWallFinishCurve = Line.CreateBound(resultingStartPoint, resultingEndPoint) as Curve;
                                            WallType wt = wallFinishCreatorCollectionItemList.FirstOrDefault(i => i.BaseWallMaterial.Id == resultingElementOfBoundarySegmentMaterial.Id).WallFinishType;
                                            Wall w = Wall.Create(doc, resultingWallFinishCurve, wt.Id, selectedLevel.Id, 10, 0, false, false) as Wall;
                                            w.get_Parameter(BuiltInParameter.WALL_KEY_REF_PARAM).Set(3);
                                            w.get_Parameter(BuiltInParameter.WALL_USER_HEIGHT_PARAM).Set(wallFinishHeight);
                                            if (roomBoundary) w.get_Parameter(BuiltInParameter.WALL_ATTR_ROOM_BOUNDING).Set(1);
                                            else w.get_Parameter(BuiltInParameter.WALL_ATTR_ROOM_BOUNDING).Set(0);
                                            wallFinishList.Add(w);
                                            XYZ wallOrientationVector = (resultingWallFinishCurve as Line).Direction.CrossProduct(XYZ.BasisZ).Negate();
                                            ElementTransformUtils.MoveElement(doc, w.Id, wallOrientationVector * (w.WallType.Width / 2));

                                            resultingStartPoint = null;
                                            resultingEndPoint = null;
                                            resultingCurveOfBoundarySegmentDirection = null;
                                            resultingElementOfBoundarySegmentMaterial = null;
                                        }
                                        else
                                        {
                                            continue;
                                        }
                                    }
                                }
                            }
                        }
                        t.Commit();

                        t.Start(mergingWalls);
                        List<Wall> tmpWallList = wallFinishList.ToList();
                        foreach (Wall w in tmpWallList)
                        {
                            try
                            {
                                ElementId testElemId = w.Id;
                            }
                            catch
                            {
                                wallFinishList.Remove(w);
                            }
                        }
                        foreach (Wall w1 in wallFinishList)
                        {
                            BoundingBoxXYZ bb = w1.get_BoundingBox(null);
                            Outline outLn = new Outline(bb.Min, bb.Max);
                            BoundingBoxIntersectsFilter filter = new BoundingBoxIntersectsFilter(outLn);

                            List<Wall> intersectingWallList = new FilteredElementCollector(doc)
                                .OfClass(typeof(Wall))
                                .WhereElementIsNotElementType()
                                .WherePasses(filter)
                                .Where(w => w.Category.Id.IntegerValue.Equals((int)BuiltInCategory.OST_Walls))
                                .Cast<Wall>()
                                .ToList();
                            foreach (Wall w2 in intersectingWallList)
                            {
                                try
                                {
                                    JoinGeometryUtils.JoinGeometry(doc, w1, w2);
                                }
                                catch
                                {

                                }
                            }
                        }
                        t.Commit();
                    }
                }
                else if (wallFinishCreatorOption == "rbt_ByLinkedFile")
                {
                    int.TryParse(commandData.Application.Application.VersionNumber, out int versionNumber);
                    if (versionNumber < 2021)
                    {
                        TaskDialog.Show("Revit", sorryCuttingOpenings);
                        return Result.Cancelled;
                    }

                    //Select a linked file
                    RevitLinkInstanceSelectionFilter selFilterRevitLinkInstance = new RevitLinkInstanceSelectionFilter();
                    Reference selRevitLinkInstance = null;
                    try
                    {
                        selRevitLinkInstance = sel.PickObject(ObjectType.Element, selFilterRevitLinkInstance, selectLinkedFile);
                    }
                    catch (Autodesk.Revit.Exceptions.OperationCanceledException)
                    {
                        return Result.Cancelled;
                    }

                    IEnumerable<RevitLinkInstance> revitLinkInstance = new FilteredElementCollector(doc)
                        .OfClass(typeof(RevitLinkInstance))
                        .Where(li => li.Id == selRevitLinkInstance.ElementId)
                        .Cast<RevitLinkInstance>();
                    if (revitLinkInstance.Count() == 0)
                    {

                        TaskDialog.Show("Revit", linkedFileNotFound);
                        return Result.Cancelled;
                    }
                    linkDoc = revitLinkInstance.First().GetLinkDocument();
                    Transform transform = revitLinkInstance.First().GetTransform();

                    List<Room> roomList = new List<Room>();
                    if (roomList.Count == 0)
                    {
                        ElementInLinkSelectionFilter<Room> selFilter = new ElementInLinkSelectionFilter<Room>(doc);

                        IList<Reference> selRooms = null;
                        try
                        {
                            selRooms = sel.PickObjects(ObjectType.LinkedElement, selFilter, chooseRoom);
                        }
                        catch (Autodesk.Revit.Exceptions.OperationCanceledException)
                        {
                            return Result.Cancelled;
                        }

                        foreach (Reference roomRef in selRooms)
                        {
                            roomList.Add(linkDoc.GetElement(roomRef.LinkedElementId) as Room);
                        }
                    }

                    List<Material> materialsList = GetMaterialsListFromBoundarySegments(linkDoc, roomList);
                    List<WallType> wallTypesList = new FilteredElementCollector(doc)
                        .OfClass(typeof(WallType))
                        .Where(wt => wt.Category.Id.IntegerValue.Equals((int)BuiltInCategory.OST_Walls))
                        .Cast<WallType>()
                        .Where(wt => wt.get_Parameter(BuiltInParameter.ALL_MODEL_MODEL).AsString() == "Отделка стен"|| wt.get_Parameter(BuiltInParameter.ALL_MODEL_MODEL).AsString() == "Finish wall")
                        .OrderBy(wt => wt.Name, new ComparatorString())
                        .ToList();

                    //Form call
                    WallFinishCreatorWPF wallFinishCreatorWPF = new WallFinishCreatorWPF(materialsList, wallTypesList);
                    wallFinishCreatorWPF.ShowDialog();
                    if (wallFinishCreatorWPF.DialogResult != true)
                    {
                        return Result.Cancelled;
                    }
                    List<WallFinishCreatorCollectionItem> wallFinishCreatorCollectionItemList = wallFinishCreatorWPF.WallFinishCreatorCollectionItemsList.ToList();
                    double wallFinishHeight;
                    double.TryParse(wallFinishCreatorWPF.WallFinishHeight, out wallFinishHeight);
                    wallFinishHeight = wallFinishHeight / 304.8;
                    bool roomBoundary = wallFinishCreatorWPF.RoomBoundary;

                    List<Wall> wallFinishList = new List<Wall>();
                    using (Transaction t = new Transaction(doc))
                    {
                        t.Start(buildingWalls);
                        foreach (Room room in roomList)
                        {
                            Level selectedLevel = GetClosestLevel(doc, linkDoc, room);
                            IList<IList<BoundarySegment>> roomBoundarySegmentsListsList = room.GetBoundarySegments(new SpatialElementBoundaryOptions());
                            foreach (IList<BoundarySegment> roomBoundarySegmentsList in roomBoundarySegmentsListsList)
                            {
                                XYZ resultingStartPoint = null;
                                XYZ resultingEndPoint = null;
                                XYZ resultingCurveOfBoundarySegmentDirection = null;
                                Material resultingElementOfBoundarySegmentMaterial = null;
                                foreach (BoundarySegment roomBoundarySegment in roomBoundarySegmentsList)
                                {
                                    Curve curveOfBoundarySegment = roomBoundarySegment.GetCurve();

                                    Element elementOfBoundarySegment = linkDoc.GetElement(roomBoundarySegment.ElementId);
                                    Material elementOfBoundarySegmentMaterial = null;
                                    if (elementOfBoundarySegment != null && elementOfBoundarySegment.Category.Id
                                        .IntegerValue.Equals((int)BuiltInCategory.OST_Walls))
                                    {
                                        if (!(elementOfBoundarySegment is FamilyInstance))
                                        {
                                            if ((elementOfBoundarySegment as Wall).CurtainGrid == null)
                                            {
                                                elementOfBoundarySegmentMaterial = linkDoc.GetElement((elementOfBoundarySegment as Wall)
                                                    .WallType
                                                    .GetCompoundStructure()
                                                    .GetMaterialId(0)) as Material;
                                            }
                                        }
                                        else if (elementOfBoundarySegment is FamilyInstance)
                                        {
                                            Parameter elementOfBoundarySegmentMaterialParam = (elementOfBoundarySegment as FamilyInstance)
                                                .Symbol
                                                .get_Parameter(BuiltInParameter.STRUCTURAL_MATERIAL_PARAM);
                                            if (elementOfBoundarySegmentMaterialParam != null)
                                            {
                                                elementOfBoundarySegmentMaterial = linkDoc.GetElement(elementOfBoundarySegmentMaterialParam.AsElementId()) as Material;
                                            }
                                            else
                                            {
                                                elementOfBoundarySegmentMaterialParam = (elementOfBoundarySegment as FamilyInstance)
                                                    .get_Parameter(BuiltInParameter.STRUCTURAL_MATERIAL_PARAM);
                                                if (elementOfBoundarySegmentMaterialParam != null)
                                                {
                                                    elementOfBoundarySegmentMaterial = linkDoc.GetElement(elementOfBoundarySegmentMaterialParam.AsElementId()) as Material;
                                                }
                                            }
                                        }
                                    }
                                    //If the boundary is a load-bearing column
                                    else if (elementOfBoundarySegment != null && elementOfBoundarySegment.Category.Id
                                        .IntegerValue.Equals((int)BuiltInCategory.OST_StructuralColumns))
                                    {
                                        Parameter elementOfBoundarySegmentMaterialParam = (elementOfBoundarySegment as FamilyInstance)
                                            .Symbol
                                            .get_Parameter(BuiltInParameter.STRUCTURAL_MATERIAL_PARAM);
                                        if (elementOfBoundarySegmentMaterialParam != null)
                                        {
                                            elementOfBoundarySegmentMaterial = linkDoc.GetElement(elementOfBoundarySegmentMaterialParam.AsElementId()) as Material;
                                        }
                                        if (elementOfBoundarySegmentMaterial == null)
                                        {
                                            elementOfBoundarySegmentMaterial = linkDoc.GetElement((elementOfBoundarySegment as FamilyInstance)
                                                .get_Parameter(BuiltInParameter.STRUCTURAL_MATERIAL_PARAM)
                                                .AsElementId()) as Material;
                                        }
                                    }

                                    if (elementOfBoundarySegmentMaterial != null)
                                    {
                                        if (curveOfBoundarySegment is Line)
                                        {
                                            XYZ startPoint = curveOfBoundarySegment.GetEndPoint(0);
                                            XYZ endPoint = curveOfBoundarySegment.GetEndPoint(1);
                                            XYZ curveOfBoundarySegmentDirection = (curveOfBoundarySegment as Line).Direction;
                                            if (resultingStartPoint != null
                                                && resultingEndPoint != null
                                                && resultingCurveOfBoundarySegmentDirection != null
                                                && resultingElementOfBoundarySegmentMaterial != null)
                                            {
                                                if (resultingCurveOfBoundarySegmentDirection.IsAlmostEqualTo(curveOfBoundarySegmentDirection)
                                                    && resultingElementOfBoundarySegmentMaterial.Id == elementOfBoundarySegmentMaterial.Id)
                                                {
                                                    if ((roomBoundarySegmentsList.Count - 1) != roomBoundarySegmentsList.IndexOf(roomBoundarySegment))
                                                    {
                                                        resultingEndPoint = endPoint;
                                                        continue;
                                                    }
                                                    else
                                                    {
                                                        resultingEndPoint = endPoint;
                                                        Curve resultingWallFinishCurve = Line.CreateBound(resultingStartPoint, resultingEndPoint) as Curve;
                                                        WallType wt = wallFinishCreatorCollectionItemList.FirstOrDefault(i => i.BaseWallMaterial.Id == resultingElementOfBoundarySegmentMaterial.Id).WallFinishType;
                                                        Wall w = Wall.Create(doc, resultingWallFinishCurve, wt.Id, selectedLevel.Id, 10, 0, false, false) as Wall;
                                                        ElementTransformUtils.MoveElement(doc, w.Id, transform.Origin);
                                                        w.get_Parameter(BuiltInParameter.WALL_KEY_REF_PARAM).Set(3);
                                                        w.get_Parameter(BuiltInParameter.WALL_USER_HEIGHT_PARAM).Set(wallFinishHeight);
                                                        if (roomBoundary) w.get_Parameter(BuiltInParameter.WALL_ATTR_ROOM_BOUNDING).Set(1);
                                                        else w.get_Parameter(BuiltInParameter.WALL_ATTR_ROOM_BOUNDING).Set(0);
                                                        wallFinishList.Add(w);
                                                        XYZ wallOrientationVector = (resultingWallFinishCurve as Line).Direction.CrossProduct(XYZ.BasisZ).Negate();
                                                        ElementTransformUtils.MoveElement(doc, w.Id, wallOrientationVector * (w.WallType.Width / 2));
                                                        continue;
                                                    }
                                                }
                                                else
                                                {
                                                    if ((roomBoundarySegmentsList.Count - 1) != roomBoundarySegmentsList.IndexOf(roomBoundarySegment))
                                                    {
                                                        Curve resultingWallFinishCurve = Line.CreateBound(resultingStartPoint, resultingEndPoint) as Curve;
                                                        WallType wt = wallFinishCreatorCollectionItemList.FirstOrDefault(i => i.BaseWallMaterial.Id == resultingElementOfBoundarySegmentMaterial.Id).WallFinishType;
                                                        Wall w = Wall.Create(doc, resultingWallFinishCurve, wt.Id, selectedLevel.Id, 10, 0, false, false) as Wall;
                                                        ElementTransformUtils.MoveElement(doc, w.Id, transform.Origin);
                                                        w.get_Parameter(BuiltInParameter.WALL_KEY_REF_PARAM).Set(3);
                                                        w.get_Parameter(BuiltInParameter.WALL_USER_HEIGHT_PARAM).Set(wallFinishHeight);
                                                        if (roomBoundary) w.get_Parameter(BuiltInParameter.WALL_ATTR_ROOM_BOUNDING).Set(1);
                                                        else w.get_Parameter(BuiltInParameter.WALL_ATTR_ROOM_BOUNDING).Set(0);
                                                        wallFinishList.Add(w);
                                                        XYZ wallOrientationVector = (resultingWallFinishCurve as Line).Direction.CrossProduct(XYZ.BasisZ).Negate();
                                                        ElementTransformUtils.MoveElement(doc, w.Id, wallOrientationVector * (w.WallType.Width / 2));

                                                        resultingStartPoint = startPoint;
                                                        resultingEndPoint = endPoint;
                                                        resultingCurveOfBoundarySegmentDirection = curveOfBoundarySegmentDirection;
                                                        resultingElementOfBoundarySegmentMaterial = elementOfBoundarySegmentMaterial;
                                                    }
                                                    else
                                                    {
                                                        Curve resultingWallFinishCurve = Line.CreateBound(resultingStartPoint, resultingEndPoint) as Curve;
                                                        WallType wt = wallFinishCreatorCollectionItemList.FirstOrDefault(i => i.BaseWallMaterial.Id == resultingElementOfBoundarySegmentMaterial.Id).WallFinishType;
                                                        Wall w = Wall.Create(doc, resultingWallFinishCurve, wt.Id, selectedLevel.Id, 10, 0, false, false) as Wall;
                                                        ElementTransformUtils.MoveElement(doc, w.Id, transform.Origin);
                                                        w.get_Parameter(BuiltInParameter.WALL_KEY_REF_PARAM).Set(3);
                                                        w.get_Parameter(BuiltInParameter.WALL_USER_HEIGHT_PARAM).Set(wallFinishHeight);
                                                        if (roomBoundary) w.get_Parameter(BuiltInParameter.WALL_ATTR_ROOM_BOUNDING).Set(1);
                                                        else w.get_Parameter(BuiltInParameter.WALL_ATTR_ROOM_BOUNDING).Set(0);
                                                        wallFinishList.Add(w);
                                                        XYZ wallOrientationVector = (resultingWallFinishCurve as Line).Direction.CrossProduct(XYZ.BasisZ).Negate();
                                                        ElementTransformUtils.MoveElement(doc, w.Id, wallOrientationVector * (w.WallType.Width / 2));

                                                        resultingWallFinishCurve = curveOfBoundarySegment as Curve;
                                                        wt = wallFinishCreatorCollectionItemList.FirstOrDefault(i => i.BaseWallMaterial.Id == elementOfBoundarySegmentMaterial.Id).WallFinishType;
                                                        w = Wall.Create(doc, resultingWallFinishCurve, wt.Id, selectedLevel.Id, 10, 0, false, false) as Wall;
                                                        ElementTransformUtils.MoveElement(doc, w.Id, transform.Origin);
                                                        w.get_Parameter(BuiltInParameter.WALL_KEY_REF_PARAM).Set(3);
                                                        w.get_Parameter(BuiltInParameter.WALL_USER_HEIGHT_PARAM).Set(wallFinishHeight);
                                                        if (roomBoundary) w.get_Parameter(BuiltInParameter.WALL_ATTR_ROOM_BOUNDING).Set(1);
                                                        else w.get_Parameter(BuiltInParameter.WALL_ATTR_ROOM_BOUNDING).Set(0);
                                                        wallFinishList.Add(w);
                                                        wallOrientationVector = (resultingWallFinishCurve as Line).Direction.CrossProduct(XYZ.BasisZ).Negate();
                                                        ElementTransformUtils.MoveElement(doc, w.Id, wallOrientationVector * (w.WallType.Width / 2));
                                                    }
                                                }
                                            }
                                            else
                                            {
                                                if ((roomBoundarySegmentsList.Count - 1) != roomBoundarySegmentsList.IndexOf(roomBoundarySegment))
                                                {
                                                    resultingStartPoint = startPoint;
                                                    resultingEndPoint = endPoint;
                                                    resultingCurveOfBoundarySegmentDirection = curveOfBoundarySegmentDirection;
                                                    resultingElementOfBoundarySegmentMaterial = elementOfBoundarySegmentMaterial;
                                                    continue;
                                                }
                                                else
                                                {
                                                    Curve resultingWallFinishCurve = curveOfBoundarySegment as Curve;
                                                    WallType wt = wallFinishCreatorCollectionItemList.FirstOrDefault(i => i.BaseWallMaterial.Id == elementOfBoundarySegmentMaterial.Id).WallFinishType;
                                                    Wall w = Wall.Create(doc, resultingWallFinishCurve, wt.Id, selectedLevel.Id, 10, 0, false, false) as Wall;
                                                    ElementTransformUtils.MoveElement(doc, w.Id, transform.Origin);
                                                    w.get_Parameter(BuiltInParameter.WALL_KEY_REF_PARAM).Set(3);
                                                    w.get_Parameter(BuiltInParameter.WALL_USER_HEIGHT_PARAM).Set(wallFinishHeight);
                                                    if (roomBoundary) w.get_Parameter(BuiltInParameter.WALL_ATTR_ROOM_BOUNDING).Set(1);
                                                    else w.get_Parameter(BuiltInParameter.WALL_ATTR_ROOM_BOUNDING).Set(0);
                                                    wallFinishList.Add(w);
                                                    XYZ wallOrientationVector = (resultingWallFinishCurve as Line).Direction.CrossProduct(XYZ.BasisZ).Negate();
                                                    ElementTransformUtils.MoveElement(doc, w.Id, wallOrientationVector * (w.WallType.Width / 2));
                                                }
                                            }

                                        }
                                        else if (curveOfBoundarySegment is Arc)
                                        {
                                            if (resultingStartPoint != null
                                                && resultingEndPoint != null
                                                && resultingCurveOfBoundarySegmentDirection != null
                                                && resultingElementOfBoundarySegmentMaterial != null)
                                            {
                                                Curve resultingWallFinishCurve = Line.CreateBound(resultingStartPoint, resultingEndPoint) as Curve;
                                                WallType wt = wallFinishCreatorCollectionItemList.FirstOrDefault(i => i.BaseWallMaterial.Id == resultingElementOfBoundarySegmentMaterial.Id).WallFinishType;
                                                Wall w = Wall.Create(doc, resultingWallFinishCurve, wt.Id, selectedLevel.Id, 10, 0, false, false) as Wall;
                                                ElementTransformUtils.MoveElement(doc, w.Id, transform.Origin);
                                                w.get_Parameter(BuiltInParameter.WALL_KEY_REF_PARAM).Set(3);
                                                w.get_Parameter(BuiltInParameter.WALL_USER_HEIGHT_PARAM).Set(wallFinishHeight);
                                                if (roomBoundary) w.get_Parameter(BuiltInParameter.WALL_ATTR_ROOM_BOUNDING).Set(1);
                                                else w.get_Parameter(BuiltInParameter.WALL_ATTR_ROOM_BOUNDING).Set(0);
                                                wallFinishList.Add(w);
                                                XYZ wallOrientationVector = (resultingWallFinishCurve as Line).Direction.CrossProduct(XYZ.BasisZ).Negate();
                                                ElementTransformUtils.MoveElement(doc, w.Id, wallOrientationVector * (w.WallType.Width / 2));

                                                resultingStartPoint = null;
                                                resultingEndPoint = null;
                                                resultingCurveOfBoundarySegmentDirection = null;
                                                resultingElementOfBoundarySegmentMaterial = null;

                                                wt = wallFinishCreatorCollectionItemList
                                                    .FirstOrDefault(i => i.BaseWallMaterial.Id == elementOfBoundarySegmentMaterial.Id)
                                                    .WallFinishType;
                                                double arcOfset = wt.Width / 2;

                                                XYZ arcVector = null;
                                                if (elementOfBoundarySegment is Wall)
                                                {
                                                    arcVector = (curveOfBoundarySegment as Arc).Normal.Negate();
                                                }
                                                else if (elementOfBoundarySegment is FamilyInstance)
                                                {
                                                    arcVector = (curveOfBoundarySegment as Arc).Normal;
                                                }

                                                resultingWallFinishCurve = (curveOfBoundarySegment as Arc).CreateOffset(arcOfset, arcVector) as Curve;
                                                w = Wall.Create(doc, resultingWallFinishCurve, wt.Id, selectedLevel.Id, 10, 0, false, false) as Wall;
                                                ElementTransformUtils.MoveElement(doc, w.Id, transform.Origin);
                                                w.get_Parameter(BuiltInParameter.WALL_KEY_REF_PARAM).Set(3);
                                                w.get_Parameter(BuiltInParameter.WALL_USER_HEIGHT_PARAM).Set(wallFinishHeight);
                                                if (roomBoundary) w.get_Parameter(BuiltInParameter.WALL_ATTR_ROOM_BOUNDING).Set(1);
                                                else w.get_Parameter(BuiltInParameter.WALL_ATTR_ROOM_BOUNDING).Set(0);
                                                wallFinishList.Add(w);
                                            }
                                            else
                                            {
                                                WallType wt = wallFinishCreatorCollectionItemList.FirstOrDefault(i => i.BaseWallMaterial.Id == elementOfBoundarySegmentMaterial.Id).WallFinishType;
                                                double arcOfset = wt.Width / 2;

                                                XYZ arcVector = null;
                                                if (elementOfBoundarySegment is Wall)
                                                {
                                                    arcVector = (curveOfBoundarySegment as Arc).Normal.Negate();
                                                }
                                                else if (elementOfBoundarySegment is FamilyInstance)
                                                {
                                                    arcVector = (curveOfBoundarySegment as Arc).Normal;
                                                }

                                                Curve resultingWallFinishCurve = (curveOfBoundarySegment as Arc).CreateOffset(arcOfset, arcVector) as Curve;
                                                Wall w = Wall.Create(doc, resultingWallFinishCurve, wt.Id, selectedLevel.Id, 10, 0, false, false) as Wall;
                                                ElementTransformUtils.MoveElement(doc, w.Id, transform.Origin);
                                                w.get_Parameter(BuiltInParameter.WALL_KEY_REF_PARAM).Set(3);
                                                w.get_Parameter(BuiltInParameter.WALL_USER_HEIGHT_PARAM).Set(wallFinishHeight);
                                                if (roomBoundary) w.get_Parameter(BuiltInParameter.WALL_ATTR_ROOM_BOUNDING).Set(1);
                                                else w.get_Parameter(BuiltInParameter.WALL_ATTR_ROOM_BOUNDING).Set(0);
                                                wallFinishList.Add(w);
                                            }
                                        }
                                    }
                                    else
                                    {
                                        if (resultingStartPoint != null
                                        && resultingEndPoint != null
                                        && resultingCurveOfBoundarySegmentDirection != null
                                        && resultingElementOfBoundarySegmentMaterial != null)
                                        {
                                            Curve resultingWallFinishCurve = Line.CreateBound(resultingStartPoint, resultingEndPoint) as Curve;
                                            WallType wt = wallFinishCreatorCollectionItemList.FirstOrDefault(i => i.BaseWallMaterial.Id == resultingElementOfBoundarySegmentMaterial.Id).WallFinishType;
                                            Wall w = Wall.Create(doc, resultingWallFinishCurve, wt.Id, selectedLevel.Id, 10, 0, false, false) as Wall;
                                            ElementTransformUtils.MoveElement(doc, w.Id, transform.Origin);
                                            w.get_Parameter(BuiltInParameter.WALL_KEY_REF_PARAM).Set(3);
                                            w.get_Parameter(BuiltInParameter.WALL_USER_HEIGHT_PARAM).Set(wallFinishHeight);
                                            if (roomBoundary) w.get_Parameter(BuiltInParameter.WALL_ATTR_ROOM_BOUNDING).Set(1);
                                            else w.get_Parameter(BuiltInParameter.WALL_ATTR_ROOM_BOUNDING).Set(0);
                                            wallFinishList.Add(w);
                                            XYZ wallOrientationVector = (resultingWallFinishCurve as Line).Direction.CrossProduct(XYZ.BasisZ).Negate();
                                            ElementTransformUtils.MoveElement(doc, w.Id, wallOrientationVector * (w.WallType.Width / 2));

                                            resultingStartPoint = null;
                                            resultingEndPoint = null;
                                            resultingCurveOfBoundarySegmentDirection = null;
                                            resultingElementOfBoundarySegmentMaterial = null;
                                        }
                                        else
                                        {
                                            continue;
                                        }
                                    }
                                }
                            }
                        }
                        t.Commit();

                        t.Start(mergingWalls);
                        List <Wall> tmpWallList = wallFinishList.ToList();
                        foreach (Wall w in tmpWallList)
                        {
                            try
                            {
                                ElementId testElemId = w.Id;
                            }
                            catch
                            {
                                wallFinishList.Remove(w);
                            }
                        }
                        foreach (Wall w1 in wallFinishList)
                        {
                            BoundingBoxXYZ bb = w1.get_BoundingBox(null);
                            Outline outLn = new Outline(bb.Min, bb.Max);
                            BoundingBoxIntersectsFilter filter = new BoundingBoxIntersectsFilter(outLn);

                            List<Wall> intersectingWallList = new FilteredElementCollector(doc)
                                .OfClass(typeof(Wall))
                                .WhereElementIsNotElementType()
                                .WherePasses(filter)
                                .Where(w => w.Category.Id.IntegerValue.Equals((int)BuiltInCategory.OST_Walls))
                                .Cast<Wall>()
                                .Where(w => w.WallType.get_Parameter(BuiltInParameter.ALL_MODEL_MODEL).AsString().Contains("Finish wall") || w.WallType.get_Parameter(BuiltInParameter.ALL_MODEL_MODEL).AsString().Contains("Отделка стен"))
                                .ToList();
                            foreach (Wall w2 in intersectingWallList)
                            {
                                try
                                {
                                    JoinGeometryUtils.JoinGeometry(doc, w1, w2);
                                }
                                catch
                                {

                                }
                            }
                        }
                        t.Commit();

                        t.Start(cuttingHoles);

                        List<FamilyInstance> doorList = new FilteredElementCollector(linkDoc)
                            .OfCategory(BuiltInCategory.OST_Doors)
                            .OfClass(typeof(FamilyInstance))
                            .WhereElementIsNotElementType()
                            .Cast<FamilyInstance>()
                            .ToList();

                        Options options = new Options();
                        options.IncludeNonVisibleObjects = false;
                        options.ComputeReferences = true;
                        options.DetailLevel = ViewDetailLevel.Coarse;
                        foreach (FamilyInstance door in doorList)
                        {
                            XYZ orientation = door.FacingOrientation;
                            List<XYZ> points = new List<XYZ>();
                            GeometryElement geomElem = door.get_Geometry(options);
                            foreach (GeometryObject geoObject in geomElem)
                            {
                                GeometryInstance instance = geoObject as GeometryInstance;
                                if (null != instance)
                                {
                                    GeometryElement instanceGeometryElement = instance.GetInstanceGeometry(transform);
                                    foreach (GeometryObject o in instanceGeometryElement)
                                    {
                                        Solid solid = o as Solid;
                                        if (solid != null && solid.Volume != 0)
                                        {
                                            foreach (Face face in solid.Faces)
                                            {
                                                if (face as PlanarFace == null) continue;
                                                PlanarFace pFace = face as PlanarFace;
                                                if (orientation.Negate().IsAlmostEqualTo(pFace.FaceNormal))
                                                {
                                                    IList<CurveLoop> loops = face.GetEdgesAsCurveLoops();
                                                    foreach (CurveLoop curves in loops)
                                                    {
                                                        foreach (Curve curve in curves)
                                                        {
                                                            points.Add(curve.GetEndPoint(0));
                                                            points.Add(curve.GetEndPoint(1));
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                            if (points.Count == 0) continue;
                            XYZ p1 = null;
                            XYZ p2 = null;
                            double distance = 0;
                            foreach (XYZ i in points)
                            {
                                foreach (XYZ j in points)
                                {
                                    if (i.DistanceTo(j) > distance)
                                    {
                                        p1 = i;
                                        p2 = j;
                                        distance = i.DistanceTo(j);
                                    }
                                }
                            }
                            if (p1.Z > p2.Z)
                            {
                                (p1, p2) = (p2, p1);
                            }

                            foreach (Wall wall in wallFinishList)
                            {
                                XYZ midPoint = (p1 + p2) / 2;
                                Options geomOptions = new Options();
                                geomOptions.ComputeReferences = true;
                                geomOptions.DetailLevel = ViewDetailLevel.Fine;
                                GeometryElement elemGeometry = wall.get_Geometry(geomOptions);
                                foreach (GeometryObject elemPrimitive in elemGeometry)
                                {
                                    Solid solid = elemPrimitive as Solid;
                                    if (solid == null || solid.Volume == 0)
                                    {
                                        continue;
                                    }

                                    Curve curve1 = Line.CreateBound(midPoint, midPoint + (500 / 304.8) * orientation);
                                    Curve curve2 = Line.CreateBound(midPoint, midPoint + (500 / 304.8) * orientation.Negate());
                                    SolidCurveIntersectionOptions intersectOptions = new SolidCurveIntersectionOptions();
                                    SolidCurveIntersection intersectCurve1 = solid.IntersectWithCurve(curve1, intersectOptions);
                                    SolidCurveIntersection intersectCurve2 = solid.IntersectWithCurve(curve2, intersectOptions);
                                    if (intersectCurve1.SegmentCount != 0 || intersectCurve2.SegmentCount != 0)
                                    {
                                        try
                                        {
                                            doc.Create.NewOpening(wall, p1, p2);
                                        }
                                        catch
                                        {

                                        }
                                        doc.Regenerate();
                                    }
                                }
                            }
                        }

                        List<FamilyInstance> windowsList = new FilteredElementCollector(linkDoc)
                            .OfCategory(BuiltInCategory.OST_Windows)
                            .OfClass(typeof(FamilyInstance))
                            .WhereElementIsNotElementType()
                            .Cast<FamilyInstance>()
                            .ToList();

                        foreach (FamilyInstance window in windowsList)
                        {
                            XYZ orientation = window.FacingOrientation;
                            List<XYZ> points = new List<XYZ>();
                            GeometryElement geomElem = window.get_Geometry(options);
                            foreach (GeometryObject geoObject in geomElem)
                            {
                                GeometryInstance instance = geoObject as GeometryInstance;
                                if (null != instance)
                                {
                                    GeometryElement instanceGeometryElement = instance.GetInstanceGeometry(transform);
                                    foreach (GeometryObject o in instanceGeometryElement)
                                    {
                                        Solid solid = o as Solid;
                                        if (solid != null && solid.Volume != 0)
                                        {
                                            foreach (Face face in solid.Faces)
                                            {
                                                if (face as PlanarFace == null) continue;
                                                PlanarFace pFace = face as PlanarFace;
                                                if (orientation.Negate().IsAlmostEqualTo(pFace.FaceNormal))
                                                {
                                                    IList<CurveLoop> loops = face.GetEdgesAsCurveLoops();
                                                    foreach (CurveLoop curves in loops)
                                                    {
                                                        foreach (Curve curve in curves)
                                                        {
                                                            points.Add(curve.GetEndPoint(0));
                                                            points.Add(curve.GetEndPoint(1));
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                            if (points.Count == 0) continue;
                            XYZ p1 = null;
                            XYZ p2 = null;
                            double distance = 0;
                            foreach (XYZ i in points)
                            {
                                foreach (XYZ j in points)
                                {
                                    if (i.DistanceTo(j) > distance)
                                    {
                                        p1 = i;
                                        p2 = j;
                                        distance = i.DistanceTo(j);
                                    }
                                }
                            }
                            if (p1.Z > p2.Z)
                            {
                                (p1, p2) = (p2, p1);
                            }

                            foreach (Wall wall in wallFinishList)
                            {
                                XYZ midPoint = (p1 + p2) / 2;
                                Options geomOptions = new Options();
                                geomOptions.ComputeReferences = true;
                                geomOptions.DetailLevel = ViewDetailLevel.Fine;
                                GeometryElement elemGeometry = wall.get_Geometry(geomOptions);
                                foreach (GeometryObject elemPrimitive in elemGeometry)
                                {
                                    Solid solid = elemPrimitive as Solid;
                                    if (solid == null || solid.Volume == 0)
                                    {
                                        continue;
                                    }

                                    Curve curve1 = Line.CreateBound(midPoint, midPoint + (500 / 304.8) * orientation);
                                    Curve curve2 = Line.CreateBound(midPoint, midPoint + (500 / 304.8) * orientation.Negate());
                                    SolidCurveIntersectionOptions intersectOptions = new SolidCurveIntersectionOptions();
                                    SolidCurveIntersection intersectCurve1 = solid.IntersectWithCurve(curve1, intersectOptions);
                                    SolidCurveIntersection intersectCurve2 = solid.IntersectWithCurve(curve2, intersectOptions);
                                    if (intersectCurve1.SegmentCount != 0 || intersectCurve2.SegmentCount != 0)
                                    {
                                        try
                                        {
                                            doc.Create.NewOpening(wall, p1, p2);
                                        }
                                        catch
                                        {

                                        }
                                        doc.Regenerate();
                                    }
                                }
                            }
                        }
                        t.Commit();
                    }
                }
                else
                {
                    TaskDialog.Show("Revit", somethingWrongWallFinishProperties);
                    return Result.Cancelled;
                }
                tg.Assimilate();
            }
            return Result.Succeeded;
        }

        private static Level GetClosestLevel(Document doc, Document linkDoc, Room room)
        {
            List<Level> levelsList = new FilteredElementCollector(doc)
                .OfCategory(BuiltInCategory.OST_Levels)
                .WhereElementIsNotElementType()
                .Cast<Level>()
                .OrderBy(lv => lv.Elevation)
                .ToList();
            Level sLv = null;
            double lvDistance = 100000;
            foreach (Level level in levelsList)
            {
                if (Math.Abs(level.Elevation - (linkDoc.GetElement(room.LevelId) as Level).Elevation) < lvDistance)
                {
                    lvDistance = Math.Abs(level.Elevation - (linkDoc.GetElement(room.LevelId) as Level).Elevation);
                    sLv = level;
                }
            }

            return sLv;
        }

        private static List<Material> GetMaterialsListFromBoundarySegments(Document linkDoc, List<Room> roomList)
        {
            List<Material> tempMaterialsList = new List<Material>();
            List<ElementId> tempMaterialIdList = new List<ElementId>();
            foreach (Room room in roomList)
            {
                IList<IList<BoundarySegment>> roomBoundarySegmentsListsList = room.GetBoundarySegments(new SpatialElementBoundaryOptions());
                foreach (IList<BoundarySegment> roomBoundarySegmentsList in roomBoundarySegmentsListsList)
                {
                    foreach (BoundarySegment roomBoundarySegment in roomBoundarySegmentsList)
                    {
                        Element elementOfBoundarySegment = linkDoc.GetElement(roomBoundarySegment.ElementId);
                        //If the border is a wall
                        if (elementOfBoundarySegment != null && elementOfBoundarySegment.Category.Id.IntegerValue.Equals((int)BuiltInCategory.OST_Walls))
                        {
                            Material elementOfBoundarySegmentMaterial = null;

                            if (!(elementOfBoundarySegment is FamilyInstance))
                            {
                                if ((elementOfBoundarySegment as Wall).CurtainGrid == null)
                                {
                                    elementOfBoundarySegmentMaterial = linkDoc.GetElement((elementOfBoundarySegment as Wall)
                                        .WallType
                                        .GetCompoundStructure()
                                        .GetMaterialId(0)) as Material;
                                    if (tempMaterialIdList.IndexOf(elementOfBoundarySegmentMaterial.Id) == -1)
                                    {
                                        tempMaterialIdList.Add(elementOfBoundarySegmentMaterial.Id);
                                    }
                                }
                                else
                                {
                                    continue;
                                }
                            }
                            else if (elementOfBoundarySegment is FamilyInstance)
                            {
                                Parameter elementOfBoundarySegmentMaterialParam = (elementOfBoundarySegment as FamilyInstance)
                                    .Symbol
                                    .get_Parameter(BuiltInParameter.STRUCTURAL_MATERIAL_PARAM);
                                if (elementOfBoundarySegmentMaterialParam != null)
                                {
                                    elementOfBoundarySegmentMaterial = linkDoc.GetElement(elementOfBoundarySegmentMaterialParam.AsElementId()) as Material;
                                }
                                else
                                {
                                    elementOfBoundarySegmentMaterialParam = (elementOfBoundarySegment as FamilyInstance)
                                        .get_Parameter(BuiltInParameter.STRUCTURAL_MATERIAL_PARAM);
                                    if (elementOfBoundarySegmentMaterialParam != null)
                                    {
                                        elementOfBoundarySegmentMaterial = linkDoc.GetElement(elementOfBoundarySegmentMaterialParam.AsElementId()) as Material;
                                    }
                                    if (elementOfBoundarySegmentMaterial == null) continue;
                                }
                                if (tempMaterialIdList.IndexOf(elementOfBoundarySegmentMaterial.Id) == -1)
                                {
                                    tempMaterialIdList.Add(elementOfBoundarySegmentMaterial.Id);
                                }
                            }
                            else
                            {
                                continue;
                            }
                        }
                        //If the boundary is a load-bearing column
                        else if (elementOfBoundarySegment != null && elementOfBoundarySegment.Category.Id.IntegerValue.Equals((int)BuiltInCategory.OST_StructuralColumns))
                        {
                            Material elementOfBoundarySegmentMaterial = null;
                            Parameter elementOfBoundarySegmentMaterialParam = (elementOfBoundarySegment as FamilyInstance)
                                .Symbol
                                .get_Parameter(BuiltInParameter.STRUCTURAL_MATERIAL_PARAM);
                            if (elementOfBoundarySegmentMaterialParam != null)
                            {
                                elementOfBoundarySegmentMaterial = linkDoc.GetElement(elementOfBoundarySegmentMaterialParam.AsElementId()) as Material;
                            }
                            if (elementOfBoundarySegmentMaterial == null)
                            {
                                elementOfBoundarySegmentMaterial = linkDoc.GetElement((elementOfBoundarySegment as FamilyInstance)
                                    .get_Parameter(BuiltInParameter.STRUCTURAL_MATERIAL_PARAM)
                                    .AsElementId()) as Material;
                                if (elementOfBoundarySegmentMaterial == null) continue;
                            }
                            if (tempMaterialIdList.IndexOf(elementOfBoundarySegmentMaterial.Id) == -1)
                            {
                                tempMaterialIdList.Add(elementOfBoundarySegmentMaterial.Id);
                            }
                        }
                        //Ignore other cases
                        else
                        {
                            continue;
                        }
                    }
                }
            }
            foreach (ElementId elemId in tempMaterialIdList)
            {
                tempMaterialsList.Add(linkDoc.GetElement(elemId) as Material);
            }
            tempMaterialsList = tempMaterialsList.OrderBy(m => m.Name, new ComparatorString()).ToList();
            return tempMaterialsList;
        }

        public static string GetWallFinishCreatorOptionsSettings()
        {
            WallFinishCreatorPropertiesSettings wallFinishCreatorPropertiesSettings = null;
            string fileName = "WallFinishCreatorOptionsSettings.xml";
            string assemblyPath = Path.Combine(Path.GetDirectoryName(typeof(App).Assembly.Location), fileName);
            if (File.Exists(assemblyPath))
            {
                using (FileStream fs = new FileStream(assemblyPath, FileMode.Open))
                {
                    XmlSerializer xSer = new XmlSerializer(typeof(WallFinishCreatorPropertiesSettings));
                    wallFinishCreatorPropertiesSettings = xSer.Deserialize(fs) as WallFinishCreatorPropertiesSettings;
                    fs.Close();
                }
            }
            else
            {
                wallFinishCreatorPropertiesSettings = new WallFinishCreatorPropertiesSettings();
                wallFinishCreatorPropertiesSettings.FloorCreationOptionValue = "rbt_ByCurrentFile";
            }
            return wallFinishCreatorPropertiesSettings.FloorCreationOptionValue;
        }
    }
}
