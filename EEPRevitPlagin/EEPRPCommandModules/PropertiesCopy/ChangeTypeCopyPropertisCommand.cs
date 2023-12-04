using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EEPRevitPlagin.EEPRPCommandModules.PropertiesCopy
{
    [Autodesk.Revit.Attributes.Transaction(Autodesk.Revit.Attributes.TransactionMode.Manual)]
    class ChangeTypeCopyPropertisCommand : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            Document doc = commandData.Application.ActiveUIDocument.Document;
            Selection sel = commandData.Application.ActiveUIDocument.Selection;
            Element firstElem;

            List<ElementId> selIds = sel.GetElementIds().ToList();
            Debug.WriteLine("Selected elements: " + selIds.Count.ToString());
            if (selIds.Count > 0)
            {
                firstElem = doc.GetElement(selIds.First());
            }
            else
            {
                Reference r1;
                try
                {
                    r1 = sel.PickObject(ObjectType.Element, "Выберите элемент, с которого нужно скопировать свойства");
                }
                catch
                {
                    return Result.Cancelled;
                }

                firstElem = doc.GetElement(r1.ElementId);
                Debug.WriteLine("First elem id: " + firstElem.Id.IntegerValue.ToString());
            }

            if (firstElem == null)
            {
                message += "Что-то не получилось. Сохраняйте спокойствие и порядок!";
                Debug.WriteLine("First elem is null");
                return Result.Failed;
            }

            ParameterMap parameters = firstElem.ParametersMap;
            Debug.WriteLine("Parameters found: " + parameters.Size.ToString());

            while (true)
            {
                try
                {
                    ISelectionFilter selFilter = new SelectionFilter(firstElem);
                    Reference r = sel.PickObject(ObjectType.Element, selFilter, "Выберите элементы для копирования свойств");
                    if (r == null) continue;
                    ElementId curId = r.ElementId;
                    if (curId == null || curId == ElementId.InvalidElementId) continue;
                    Element curElem = doc.GetElement(curId);
                    if (curElem == null) continue;
                    Debug.WriteLine("Cur element id: " + curId.IntegerValue.ToString());

                    try
                    {
                        ElementId firstTypeId = firstElem.GetTypeId();
                        ElementId curTypeId = curElem.GetTypeId();
                        if (firstTypeId != curTypeId)
                        {
                            using (Transaction t1 = new Transaction(doc))
                            {
                                t1.Start("Назначение типа");

                                curElem.get_Parameter(BuiltInParameter.ELEM_TYPE_PARAM).Set(firstTypeId);

                                t1.Commit();
                                Debug.WriteLine("Type of element is changed");
                            }
                        }
                    }
                    catch { }


                    using (Transaction t = new Transaction(doc))
                    {
                        t.Start("Копирование свойств");

                        foreach (Parameter param in parameters)
                        {
                            if (param == null) continue;
                            if (!param.HasValue) continue;
                            try
                            {
                                Parameter curParam = curElem.get_Parameter(param.Definition);

                                switch (param.StorageType)
                                {
                                    case StorageType.None:
                                        break;
                                    case StorageType.Integer:
                                        curParam.Set(param.AsInteger());
                                        break;
                                    case StorageType.Double:
                                        curParam.Set(param.AsDouble());
                                        break;
                                    case StorageType.String:
                                        curParam.Set(param.AsString());
                                        break;
                                    case StorageType.ElementId:
                                        curParam.Set(param.AsElementId());
                                        break;
                                    default:
                                        break;
                                }
                                Debug.WriteLine("Param value is written: " + curParam.Definition.Name + " = " + curParam.AsValueString());
                            }
                            catch (Exception ex)
                            {
                                Debug.WriteLine(ex.Message);
                                continue;
                            }
                        }
                        t.Commit();
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.Message);
                    return Result.Succeeded;
                }
            }
        }
    }

    public class SelectionFilter : ISelectionFilter
    {
        Element firstElem;

        public SelectionFilter(Element elem)
        {
            firstElem = elem;
        }

        public bool AllowElement(Element elem)
        {
            Type firstType = firstElem.GetType();
            Type curType = elem.GetType();
            bool check = firstType.Equals(curType);
            return check;
        }

        public bool AllowReference(Reference reference, XYZ position)
        {
            throw new NotImplementedException();
        }
    }
}