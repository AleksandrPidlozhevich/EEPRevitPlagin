using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.DB.Electrical;
using Autodesk.Revit.DB.Mechanical;
using Autodesk.Revit.DB.Plumbing;
using Autodesk.Revit.DB.Structure;
using Autodesk.Revit.UI.Selection;
using EEPRevitPlagin.SecondaryCommand;
using Document = Autodesk.Revit.DB.Document;
using Transaction = Autodesk.Revit.DB.Transaction;
using System.Windows.Controls;
using System.Dynamic;

namespace EEPRevitPlagin.EEPRPCommandModules.CuttingOpening
{
    [Autodesk.Revit.Attributes.Transaction(Autodesk.Revit.Attributes.TransactionMode.Manual)]
    internal class CuttingOpeningCommand_old : IExternalCommand

    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

            double gap = 50 * 2 / 304.8;

            List<FamilyInstance> intersectionWallRectangularList = new List<FamilyInstance>();
            List<FamilyInstance> intersectionWallRectangularCombineList = new List<FamilyInstance>();
            List<FamilyInstance> intersectionWallRoundList = new List<FamilyInstance>();
            ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
            ConvUnits convert = new ConvUnits();

            UIApplication uiApp = commandData.Application;
            Document doc = uiApp.ActiveUIDocument.Document;
            Selection sel = commandData.Application.ActiveUIDocument.Selection;

            GeneralInfo generalInfo = new GeneralInfo(doc); //Получение элементов отверстий

            List<RevitLinkInstance> linkInstances = new FilteredElementCollector(doc)
                .OfClass(typeof(RevitLinkInstance))
                .Cast<RevitLinkInstance>()
                .ToList();

            LevelAndMechanical items = new LevelAndMechanical(doc, sel);

            List<WallAndFloorInfo> wallInfo = new List<WallAndFloorInfo>();
            List<WallAndFloorInfo> floorInfo = new List<WallAndFloorInfo>();

            List<FamilySymbol> familySymbols = generalInfo.GetGeneralFamilySymbols();

            CuttingOpeningWPF wpf = new CuttingOpeningWPF(familySymbols);
            wpf.ShowDialog();

            FamilySymbol wallRectangularOpening = wpf.wallRectangularOpening;
            FamilySymbol floorRectangularOpening = wpf.floorRectangularOpening;
            FamilySymbol wallRoundOpening = wpf.wallRoundOpening;
            FamilySymbol floorRoundOpening = wpf.floorRoundOpening;

            //List<Pipe> pipesList = new List<Pipe>();
            //List<Duct> ductsList = new List<Duct>();
            //List<CableTray> cableTrayList = new List<CableTray>();

            PicObjects picObjects = new PicObjects();

            try
            {
                items.GetMechanical();
            }
            catch (Autodesk.Revit.Exceptions.OperationCanceledException)
            {
                return Result.Cancelled;
            }

            foreach (RevitLinkInstance i in items.LinkInstances)
            {
                Document linkDoc = i.GetLinkDocument();
                Transform transform = i.GetTotalTransform();

                Options opt = new Options();
                opt.ComputeReferences = true;
                opt.DetailLevel = ViewDetailLevel.Fine;

                ICollection<Element> linkWallsList = new FilteredElementCollector(i.GetLinkDocument())
                    .OfCategory(BuiltInCategory.OST_Walls).WhereElementIsNotElementType().ToElements();

                ICollection<Element> linkFloorsList = new FilteredElementCollector(i.GetLinkDocument())
                    .OfCategory(BuiltInCategory.OST_Floors).WhereElementIsNotElementType().ToElements();

                foreach (Element linkWallList in linkWallsList)
                {
                    Wall wall = linkWallList as Wall;

                    //Добавление информации о каждой стене связанного файла
                    wallInfo.Add(new WallAndFloorInfo(wall, linkDoc, transform, opt));
                }
                foreach (Element linkFloorList in linkFloorsList)
                {
                    Floor floor = linkFloorList as Floor;

                    //Добавление информации о каждой стене связанного файла
                    floorInfo.Add(new WallAndFloorInfo(floor, linkDoc, transform, opt));
                }
            }

            using (Transaction t = new Transaction(doc))
            {
                t.Start("Расстановка объектов");
                foreach (WallAndFloorInfo i in wallInfo)
                {
                    Level lvl = items.GetClosestBottomWallLevel(i.Doc, i.Wall);
                    XYZ wallOrientation = i.Wall.Orientation;
                    //Level lvl = GetClosestBottomWallLevel(items.Levels, i.Doc, i.Wall);
                    foreach (Solid solid in i.GetAndTransformSolid())
                    {
                        foreach (Duct duct in items.Ducts)
                        {
                            Curve ductCurve = GetLocationCurve(duct);
                            SolidCurveIntersection intersection = GetIntersection(solid, ductCurve);

                            if (intersection.SegmentCount > 0)
                            {
                                XYZ originIntersectionCurve;
                                FamilyInstance point;
                                double delta1;
                                double delta2;
                                //Если воздуховод круглый
                                if (duct.get_Parameter(BuiltInParameter.RBS_CURVE_DIAMETER_PARAM) != null)
                                {
                                    double ductDiameter =
                                        Math.Round(duct.get_Parameter(BuiltInParameter.RBS_CURVE_DIAMETER_PARAM).AsDouble(), 6);

                                    double intersectionPointHeight = ductDiameter + gap;
                                    double intersectionPointThickness = i.Wall.Width + (convert.MMToFoot(20));

                                    CreateItem(doc, wallOrientation, ductCurve, intersection, i, wallRoundOpening,
                                        lvl, ductDiameter,
                                        out originIntersectionCurve, out point, out delta1, out delta2);

                                    CheckWallOrientation(doc, wallOrientation, point, originIntersectionCurve);

                                    double intersectionPointWidth = delta1 * 2 + delta2 * 2 + gap;
                                    point.get_Parameter(generalInfo.PointThickness).Set(intersectionPointThickness);
                                    point.get_Parameter(generalInfo.PointDiameter).Set(intersectionPointWidth);

                                    SetElevation(doc, point, generalInfo, originIntersectionCurve);

                                    intersectionWallRoundList.Add(point);
                                    /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
                                }
                                //Если воздуховод прямоугольный
                                else
                                {
                                    double ductHeight = Math.Round(duct.get_Parameter(BuiltInParameter.RBS_CURVE_HEIGHT_PARAM).AsDouble(), 6);
                                    double ductWidth = Math.Round(duct.get_Parameter(BuiltInParameter.RBS_CURVE_WIDTH_PARAM).AsDouble(), 6);
                                    double intersectionPointHeight = ductHeight + gap;
                                    double intersectionPointThickness = i.Wall.Width + gap;

                                    CreateItem(doc, wallOrientation, ductCurve, intersection, i, wallRectangularOpening,
                                        lvl, ductWidth,
                                        out originIntersectionCurve, out point, out delta1, out delta2);

                                    CheckWallOrientation(doc, wallOrientation, point, originIntersectionCurve);

                                    double intersectionPointWidth = delta1 * 2 + delta2 * 2 + gap;
                                    point.get_Parameter(generalInfo.PointHeight).Set(intersectionPointHeight);
                                    point.get_Parameter(generalInfo.PointThickness).Set(intersectionPointThickness);
                                    point.get_Parameter(generalInfo.PointWidth).Set(intersectionPointWidth);

                                    SetElevation(doc, point, generalInfo, originIntersectionCurve);

                                    intersectionWallRectangularList.Add(point);
                                    intersectionWallRectangularCombineList.Add(point);
                                }
                            }
                        }

                        foreach (Pipe pipe in items.Pipes)
                        {
                            Curve pipeCurve = GetLocationCurve(pipe);
                            SolidCurveIntersection intersection = GetIntersection(solid, pipeCurve);
                            if (intersection.SegmentCount > 0)
                            {
                                XYZ originIntersectionCurve;
                                FamilyInstance point;
                                double delta1;
                                double delta2;

                                double pipeDiameter = Math.Round(pipe.get_Parameter(BuiltInParameter.RBS_PIPE_OUTER_DIAMETER).AsDouble(), 6);
                                double intersectionPointHeight = pipeDiameter + gap;
                                double intersectionPointThickness = i.Wall.Width + (convert.MMToFoot(20));

                                CreateItem(doc, wallOrientation, pipeCurve, intersection, i, wallRoundOpening,
                                    lvl, pipeDiameter,
                                    out originIntersectionCurve, out point, out delta1, out delta2);

                                CheckWallOrientation(doc, wallOrientation, point, originIntersectionCurve);

                                double intersectionPointWidth = delta1 * 2 + delta2 * 2 + gap;
                                point.get_Parameter(generalInfo.PointHeight).Set(intersectionPointHeight);
                                point.get_Parameter(generalInfo.PointThickness).Set(intersectionPointThickness);
                                point.get_Parameter(generalInfo.PointWidth).Set(intersectionPointWidth);

                                SetElevation(doc, point, generalInfo, originIntersectionCurve);

                                intersectionWallRectangularList.Add(point);
                                intersectionWallRectangularCombineList.Add(point);
                            }
                        }

                        foreach (CableTray cableTray in items.Cables)
                        {
                            Curve trayCurve = GetLocationCurve(cableTray);
                            SolidCurveIntersection intersection = GetIntersection(solid, trayCurve);

                            if (intersection.SegmentCount > 0)
                            {
                                XYZ originIntersectionCurve;
                                FamilyInstance point;
                                double delta1;
                                double delta2;

                                double trayHeight = Math.Round(cableTray.get_Parameter(BuiltInParameter.RBS_CURVE_HEIGHT_PARAM).AsDouble(), 6);
                                double trayWight = Math.Round(cableTray.get_Parameter(BuiltInParameter.RBS_CURVE_WIDTH_PARAM).AsDouble(), 6);
                                double intersectionPointHeight = trayHeight + gap;
                                double intersectionPointThickness = i.Wall.Width + gap;

                                CreateItem(doc, wallOrientation, trayCurve, intersection, i, wallRectangularOpening,
                                    lvl, trayWight,
                                    out originIntersectionCurve, out point, out delta1, out delta2);

                                CheckWallOrientation(doc, wallOrientation, point, originIntersectionCurve);
                            }
                        }
                    }
                }

                foreach (WallAndFloorInfo i in floorInfo)
                {
                    GeometryElement geomElem = i.Floor.get_Geometry(i.Options);
                    foreach (Solid solid in i.GetAndTransformSolid())
                    {
                        foreach (Duct duct in items.Ducts)
                        {
                            
                        }
                    }
                }
                t.Commit();
            }

            return Result.Succeeded;
        }

        Curve GetLocationCurve(Element e)
        {
            return (e.Location as LocationCurve).Curve;
        }
        private double GetAngleFromMEPCurve(MEPCurve curve)
        {
            foreach (Connector c in curve.ConnectorManager.Connectors)
            {
                double rotationAngle = 0;
                if (Math.Round(c.CoordinateSystem.BasisY.AngleOnPlaneTo(XYZ.BasisY, XYZ.BasisZ), 6) * (180 / Math.PI) < 180)
                {
                    rotationAngle = -c.CoordinateSystem.BasisY.AngleTo(XYZ.BasisY);
                }
                else
                {
                    rotationAngle = c.CoordinateSystem.BasisY.AngleTo(XYZ.BasisY);
                }
                return rotationAngle;
            }
            return 0;
        }
        SolidCurveIntersection GetIntersection(Solid solid, Curve curve)
        {
            //Объявление условия поиска пересечений только внутри тела
            SolidCurveIntersectionOptions scio = new SolidCurveIntersectionOptions();
            scio.ResultType = SolidCurveIntersectionMode.CurveSegmentsInside;
            //Получение пересечений с геометрией стены / пола
            return solid.IntersectWithCurve(curve, scio);
        }
        void CheckWallOrientation(
            Document doc,
            XYZ wallOrientation,
            FamilyInstance point,
            XYZ originIntersectionCurve
            )
        {
            if (Math.Round(wallOrientation.AngleTo(point.FacingOrientation), 6) != 0)
            {
                Line rotationLine = Line.CreateBound(originIntersectionCurve, originIntersectionCurve + 1 * XYZ.BasisZ);
                double rotationAngle = 0;
                if (Math.Round(wallOrientation.AngleOnPlaneTo(point.FacingOrientation, XYZ.BasisZ), 6) * (180 / Math.PI) < 180)
                {
                    rotationAngle = -wallOrientation.AngleTo(point.FacingOrientation);
                }
                else
                {
                    rotationAngle = wallOrientation.AngleTo(point.FacingOrientation);
                }
                ElementTransformUtils.RotateElement(doc, point.Id, rotationLine, rotationAngle);

            }
        }

        private void CreateItem(
            Document doc,
            XYZ wallOrientation,
            Curve curve,
            SolidCurveIntersection intersection,
            WallAndFloorInfo i,
            FamilySymbol cuttingFamilySymbol,
            Level lvl,
            double itemSize,
            out XYZ originIntersectionCurve,
            out FamilyInstance point,
            out double delta1,
            out double delta2)
        {
            double a = Math.Round(
                wallOrientation.AngleTo((curve as Line).Direction) * (180 / Math.PI), 6);

            a = GetAngle(a);

            delta1 = Math.Abs(i.Wall.Width / 2 * Math.Tan(a));
            delta2 = Math.Abs(itemSize / 2 * Math.Tan(a));
            if (delta1 >= 9.84251968504 || delta2 >= 9.84251968504)
            {
                originIntersectionCurve = null;
                point = null;
                return;
            };

            XYZ intersectionCurveStartPoint = intersection.GetCurveSegment(0).GetEndPoint(0);
            XYZ intersectionCurveEndPoint = intersection.GetCurveSegment(0).GetEndPoint(1);
            originIntersectionCurve =
                ((intersectionCurveStartPoint + intersectionCurveEndPoint) / 2) -
                (itemSize / 2) * XYZ.BasisZ;

            originIntersectionCurve = new XYZ(originIntersectionCurve.X,
                originIntersectionCurve.Y,
                originIntersectionCurve.Z - lvl.Elevation);

            point = doc.Create.NewFamilyInstance(
                originIntersectionCurve,
                cuttingFamilySymbol,
                lvl,
                StructuralType.NonStructural);
        }

        private void SetElevation(Document doc, FamilyInstance point, GeneralInfo generalInfo, XYZ originIntersectionCurve)
        {
            point.get_Parameter(generalInfo.HeightOfBaseLevel).Set((doc.GetElement(point.LevelId) as Level).Elevation);
            point.get_Parameter(BuiltInParameter.INSTANCE_ELEVATION_PARAM).Set(originIntersectionCurve.Z);
            point.get_Parameter(generalInfo.LevelOffSet).Set(originIntersectionCurve.Z);
        }
        private double GetAngle(double a)
        {
            if (a > 90 && a < 180)
            {
                return (180 - a) * (Math.PI / 180);
            }
            else
            {
                return a * (Math.PI / 180);
            }
        }
        
        private double RoundUpToIncrement(double x, double m)
        {
            return (((int)Math.Ceiling(x * 304.8 / m)) * m) / 304.8;
        }

        /// <summary>
        /// Собрать информацию по каждому элементу *Стена* или *Перекрытие*
        /// </summary>
        private class WallAndFloorInfo
        {
            public Wall Wall { get; set; }
            public Floor Floor { get; set; }
            public Document Doc { get; set; }
            public Transform Transform { get; set; }
            public Options Options { get; set; }

            public WallAndFloorInfo(Wall wall, Document doc, Transform transform, Options options)
            {
                Wall = wall;
                Doc = doc;
                Transform = transform;
                Options = options;
            }
            public WallAndFloorInfo(Floor floor, Document doc, Transform transform, Options options)
            {
                Floor = floor;
                Doc = doc;
                Transform = transform;
                Options = options;
            }

            /// <summary>
            /// Получение геометрии из элемента и его трансформация в координаты проекта
            /// </summary>
            /// <returns>
            /// Возвращение листа с геометрией
            /// </returns>
            public List<Solid> GetAndTransformSolid()
            {
                Wall wall = Wall;
                Document doc = Doc;
                Transform transform = Transform;
                Options opt = Options;

                List<Solid> solidList = new List<Solid>();

                // Получение элемента геометрии, текущего элемента
                GeometryElement geomWall = wall.get_Geometry(opt);

                // Получение объекта геометрии
                foreach (GeometryObject geoObject in geomWall)
                {
                    // Попытка найти тело
                    Solid solid = geoObject as Solid;
                    if (solid != null)
                    {
                        //Трансформация элемента в координаты проекта
                        Solid newSolid = SolidUtils.CreateTransformed(solid, transform);
                        solidList.Add(newSolid);
                    }
                }

                return solidList;
            }


        }

        private class GeneralInfo
        {
            private readonly Document _document;
            
            Guid pointWidthGuid = new Guid("8f2e4f93-9472-4941-a65d-0ac468fd6a5d");
            Guid pointHeightGuid = new Guid("da753fe3-ecfa-465b-9a2c-02f55d0c2ff1");
            Guid pointThicknessGuid = new Guid("293f055d-6939-4611-87b7-9a50d0c1f50e");
            Guid pointDiameterGuid = new Guid("9b679ab7-ea2e-49ce-90ab-0549d5aa36ff");
            Guid heightOfBaseLevelGuid = new Guid("9f5f7e49-616e-436f-9acc-5305f34b6933");
            Guid nlevelOffsetGuid = new Guid("515dc061-93ce-40e4-859a-e29224d80a10");

            public Guid PointWidth { get { return pointWidthGuid; } }
            public Guid PointHeight { get { return pointHeightGuid; } }
            public Guid PointThickness { get { return pointThicknessGuid; } }
            public Guid PointDiameter { get { return pointDiameterGuid; } }
            public Guid HeightOfBaseLevel { get { return heightOfBaseLevelGuid; } }
            public Guid LevelOffSet { get { return nlevelOffsetGuid; } }

            public GeneralInfo(Document doc)
            {
                _document = doc;
            }

            public List<FamilySymbol> GetGeneralFamilySymbols()
            {
                Document doc = _document;
                List<FamilySymbol> familySymbols = new FilteredElementCollector(doc)
                    .OfClass(typeof(FamilySymbol))
                    .OfCategory(BuiltInCategory.OST_GenericModel)
                    .Cast<FamilySymbol>()
                    .Where(fs => fs.Family.FamilyPlacementType == FamilyPlacementType.OneLevelBased)
                    //.Where(fs => fs.Family.Name == "Пересечение_Стена_Круглое")
                    .ToList();
                return familySymbols;
            }
        }
        private class LevelAndMechanical
        {
            private Document Doc { get; set;}
            private Selection Selection { get; set; }
            public List<Level> Levels { get; set; }
            public List<Duct> Ducts { get; set; }
            public List<Pipe> Pipes { get; set; }
            public List<CableTray> Cables { get; set; }
            public List<RevitLinkInstance> LinkInstances { get; set; }

            public LevelAndMechanical(Document doc, Selection sel)
            {
                //Получение уровней файла
                Levels = new FilteredElementCollector(doc)
                    .OfClass(typeof(Level))
                    .OfCategory(BuiltInCategory.OST_Levels)
                    .Cast<Level>()
                    .ToList();

                //Получение связанных элементов файлов
                List<RevitLinkInstance> linkModels = new FilteredElementCollector(doc)
                    .OfClass(typeof(RevitLinkInstance))
                    .Cast<RevitLinkInstance>()
                    .ToList();

                this.Doc = doc;
                this.Selection = sel;
            }

            public void GetMechanical()
            {
                List<Pipe> pipesList = new List<Pipe>();
                List<Duct> ductsList = new List<Duct>();
                List<CableTray> cableTrayList = new List<CableTray>();

                PicObjects picObjects = new PicObjects();
                IList<Reference> references = Selection.PickObjects(
                    ObjectType.Element,
                    picObjects,
                    "Выберете трубу, воздуховод или кабельный лоток");

                references = Selection.PickObjects(ObjectType.Element, picObjects,
                        "Выберите трубу, воздуховод или кабельный лоток!");

                    foreach (Reference refElem in references)
                    {
                        if (Doc.GetElement(refElem).Category.Id.IntegerValue.Equals((int)BuiltInCategory.OST_PipeCurves))
                        {
                            pipesList.Add((Doc.GetElement(refElem) as Pipe));
                        }
                        else if (Doc.GetElement(refElem).Category.Id.IntegerValue
                                 .Equals((int)BuiltInCategory.OST_DuctCurves))
                        {
                            ductsList.Add((Doc.GetElement(refElem)) as Duct);
                        }
                        else if (Doc.GetElement(refElem).Category.Id.IntegerValue
                                 .Equals((int)BuiltInCategory.OST_CableTray))
                        {
                            cableTrayList.Add((Doc.GetElement(refElem)) as CableTray);
                        }

                    }
                    Ducts = ductsList;
                    Pipes = pipesList;
                    Cables = cableTrayList;
            }

            /// <summary>
                /// Get level of Wall or Floor
                /// </summary>
                /// <param name="doc">Put linkDocument as Document</param>
                /// <param name="element">Put Floor or Wall type</param>
                /// <returns></returns>
                public Level GetClosestBottomWallLevel(Document doc, Element element)
            {
                List<Level> docLvlList = Levels;
                Level lvl = null;

                if ((Wall)element == null && (Floor)element == null)
                {
                    return null;
                }
                
                double linkWallLevelElevation = (doc.GetElement(element.LevelId) as Level).Elevation;
                double heightDifference = 100000000;

                foreach (var docLvl in docLvlList)
                {
                    double tmpHeightDifference = Math.Abs(Math.Round(linkWallLevelElevation, 6) - Math.Round(docLvl.Elevation, 6));
                    if (tmpHeightDifference < heightDifference)
                    {
                        heightDifference = tmpHeightDifference;
                        lvl = docLvl;
                    }
                }
                return lvl;
            }
        }

        class PicObjects : ISelectionFilter
        {
            public bool AllowElement(Element e)
            {
                if (e is Pipe || e is Duct || e is CableTray)
                {
                    return true;
                }
                return false;
            }
            public bool AllowReference(Reference reference, XYZ position)
            {
                return false;
            }
        }
    }
}


