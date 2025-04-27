//using Autodesk.Revit.Attributes;
//using Autodesk.Revit.DB;
//using Autodesk.Revit.DB.Structure;
//using Autodesk.Revit.UI;
//using System;
//using System.Collections.Generic;
//using System.IO;
//using System.Linq;

//namespace EEPRevitPlagin.EEPRPCommandModules.OffAnalyticsModelGroup
//{
//    [Transaction(TransactionMode.Manual)]
//    public class OffAnalyticsModelGroupCommand : IExternalCommand
//    {
//        public Result Execute(
//            ExternalCommandData commandData,
//            ref string message,
//            ElementSet elements)
//        {
//            try
//            {
//                // Путь к файлу группы
//                string groupFilePath = "путь_к_файлу.rvt";

//                // Получение доступа к текущему документу Revit
//                Document doc = commandData.Application.ActiveUIDocument.Document;

//                // Загрузка файла группы как группы в проект
//                Group group = doc.LoadGroup(groupFilePath);

//                if (group != null)
//                {
//                    return Result.Succeeded;
//                }
//                else
//                {
//                    message = "Не удалось загрузить группу.";
//                    return Result.Failed;
//                }
//            }
//            catch (Exception ex)
//            {
//                message = ex.Message;
//                return Result.Failed;
//            }
//        }
//    }
//}