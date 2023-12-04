using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using System.Collections.Generic;

namespace EEPRevitPlagin.EEPRPCommandModules.RoomBoundaryLines
{
    [Transaction(TransactionMode.Manual)]
    public class RoomBoundaryLinesCommand : IExternalCommand
    {
        [System.Obsolete]
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIDocument uidoc = commandData.Application.ActiveUIDocument;
            Document doc = uidoc.Document;

            // Получение выбранных помещений
            IList<Reference> roomRefs = uidoc.Selection.PickObjects(ObjectType.Element, new RoomSelectionFilter());

            foreach (Reference roomRef in roomRefs)
            {
                Element room = doc.GetElement(roomRef.ElementId);

                // Получение контура помещения
                IList<IList<BoundarySegment>> boundarySegments = GetRoomBoundarySegments(room);

                // Список линий контура помещения, их длин и имен материалов стен
                List<Line> contourLines = new List<Line>();
                List<double> lineLengths = new List<double>();
                List<string> wallMaterialNames = new List<string>();

                // Суммарные значения длины для каждого материала стены
                double concreteTotalLength = 0.0;
                double brickTotalLength = 0.0;

                // Итерация по границам помещения
                foreach (IList<BoundarySegment> segments in boundarySegments)
                {
                    foreach (BoundarySegment segment in segments)
                    {
                        ElementId elementId = segment.ElementId;

                        // Проверка исключения Curtain Walls
                        if (elementId != null && doc.GetElement(elementId) is Wall wall)
                        {
                            if (wall.WallType.Kind != WallKind.Curtain)
                            {
                                Curve curve = segment.GetCurve();
                                if (curve is Line line)
                                {
                                    contourLines.Add(line);

                                    // Преобразование длины линии в миллиметры
                                    double length = line.Length * 304.8; // Преобразование из футов в миллиметры
                                    lineLengths.Add(length);

                                    // Получение материала стены
                                    ElementId materialId = wall.WallType.GetCompoundStructure().GetMaterialId(0);
                                    Element material = doc.GetElement(materialId);

                                    if (material != null)
                                    {
                                        string materialName = material.Name;
                                        wallMaterialNames.Add(materialName);

                                        // Суммирование длины для определенных материалов стен
                                        if (materialName == "ADSK_Бетон")
                                        {
                                            concreteTotalLength += length;
                                        }
                                        else if (materialName == "ADSK_Кладка_Кирпич")
                                        {
                                            brickTotalLength += length;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }

                // Запись значений длины в параметры помещения
                using (Transaction transaction = new Transaction(doc))
                {
                    transaction.Start("Запись длин в параметры");

                    // Получение параметра "Кол.бетонна"
                    Parameter concreteCountParam = room.LookupParameter("Кол.бетонна");

                    // Получение параметра "Кол.кирпичная"
                    Parameter brickCountParam = room.LookupParameter("Кол.кирпичная");

                    // Запись суммарных значений длины в параметры помещения
                    if (concreteCountParam != null)
                    {
                        concreteCountParam.SetValueString(concreteTotalLength.ToString());
                    }
                    if (brickCountParam != null)
                    {
                        brickCountParam.SetValueString(brickTotalLength.ToString());
                    }

                    transaction.Commit();
                }
            }

            return Result.Succeeded;
        }

        // Метод для получения границ помещения
        private IList<IList<BoundarySegment>> GetRoomBoundarySegments(Element room)
        {
            SpatialElementBoundaryOptions boundaryOptions = new SpatialElementBoundaryOptions();
            return (room as Room)?.GetBoundarySegments(boundaryOptions);
        }
    }

    // Фильтр выбора помещений
    public class RoomSelectionFilter : ISelectionFilter
    {
        public bool AllowElement(Element elem)
        {
            return elem is Room;
        }

        public bool AllowReference(Reference reference, XYZ position)
        {
            return true;
        }
    }
}