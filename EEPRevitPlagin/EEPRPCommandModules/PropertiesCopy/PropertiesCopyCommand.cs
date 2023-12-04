#region usings
using System;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using EEPRevitPlagin.SecondaryCommand;
using Autodesk.Revit.Attributes;
using System.Windows.Controls.Primitives;
#endregion

namespace EEPRevitPlagin.EEPRPCommandModules.PropertiesCopy
{

    [Transaction(TransactionMode.Manual)]
    class PropertiesCopyCommand : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            #region message
            System.Resources.ResourceManager rs = new System.Resources.ResourceManager("EEPRevitPlagin.SecondaryCommand.LangResources.Language", System.Reflection.Assembly.GetExecutingAssembly());
            string selectItemWhichCopyProperties = rs.GetString("Select_the_ite_from_which_to_copy_properties");
            string somethingWrong =rs.GetString(" Error_messages");
            string propertyCopying = rs.GetString("Property_copying");
            string selectedNotRetrieved=rs.GetString("Selected_item_not_retrieved");
            string selectCopyPropertiesOrEsc = rs.GetString("Select_copy_properties_or_Esc");
            #endregion
            Document doc = commandData.Application.ActiveUIDocument.Document;
            Selection sel = commandData.Application.ActiveUIDocument.Selection;
            Element firstElem;

            List<ElementId> selIds = sel.GetElementIds().ToList();
            if (selIds.Count > 0)
            {
                firstElem = doc.GetElement(selIds.First());
            }
            else
            {
                Reference r1;
                try
                {
                    r1 = sel.PickObject(ObjectType.Element, selectItemWhichCopyProperties);
                }
                catch
                {
                    return Result.Cancelled;
                }

                firstElem = doc.GetElement(r1.ElementId);
            }

            if (firstElem == null)
            {
                message += somethingWrong;
                return Result.Failed;
            }

            ParameterMap parameters = firstElem.ParametersMap;

            while (true)
            {
                Reference r;
                try
                {
                    r = sel.PickObject(ObjectType.Element, selectCopyPropertiesOrEsc);
                    if (r == null) break; // The selection is canceled, the operation is completed
                }
                catch
                {
                    break; // The selection is canceled, the operation is completed
                }

                ElementId curId = r.ElementId;
                if (curId == null || curId == ElementId.InvalidElementId)
                {
                    message += selectedNotRetrieved;
                    continue; // Skip the current object and move on to the next
                }

                Element curElem = doc.GetElement(curId);
                if (curElem == null)
                {
                    message += selectedNotRetrieved;
                    continue; // Skip the current object and move on to the next
                }

                try
                {
                    using (Transaction t = new Transaction(doc))
                    {
                        t.Start(propertyCopying);

                        foreach (Parameter param in parameters)
                        {
                            if (param == null || !param.HasValue) continue;

                            try
                            {
                                Parameter curParam = curElem.get_Parameter(param.Definition);

                                switch (param.StorageType)
                                {
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
                            }
                            catch (Exception ex)
                            {
                                continue;
                            }
                        }

                        t.Commit();
                    }
                }
                catch (Exception ex)
                {
                    continue; // Skip the current object and move on to the next
                }
            }

            return Result.Succeeded;
        }
    }
}