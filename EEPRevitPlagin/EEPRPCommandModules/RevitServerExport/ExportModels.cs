using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI.Events;
using System;
using System.Collections.Generic;
using System.IO;

namespace EEPRevitPlagin.EEPRPCommandModules.RevitServerExport
{
    internal class ExportModels
    {
        public static void ExportrvtModelFromServer(string path, string outFolder, Application app)
        {
            try
            {
                //app. DialogBoxShowing += UiAppOnDialogBoxShowing;
                ModelPath modelPath = ModelPathUtils.ConvertUserVisiblePathToModelPath(path);
                string modelName = Path.GetFileName(path);
                string destModelPath = outFolder + @"\" + modelName;
                Directory.CreateDirectory(outFolder + @"\temp");
                string destModelPathTemp = outFolder + @"\temp\a_" + modelName;
                ModelPath modelPathtemp = ModelPathUtils.ConvertUserVisiblePathToModelPath(destModelPathTemp);
                app.CopyModel(modelPath, destModelPathTemp, true);
                UnloadRevitLinks(destModelPathTemp);
                OpenOptions openOptions = new OpenOptions();
                openOptions.Audit = true;
                openOptions.DetachFromCentralOption = DetachFromCentralOption.DetachAndPreserveWorksets;
                openOptions.AllowOpeningLocalByWrongUser = true;
                //Document doc = app.OpenDocumentFile(destModelPathTemp);
                Document doc = app.OpenDocumentFile(modelPathtemp, openOptions);
                SaveAsOptions saveAsOptions = new SaveAsOptions();
                WorksharingSaveAsOptions worksharingSaveAsOptions = new WorksharingSaveAsOptions();
                worksharingSaveAsOptions.SaveAsCentral = true;
                worksharingSaveAsOptions.OpenWorksetsDefault = SimpleWorksetConfiguration.AllWorksets;
                saveAsOptions.OverwriteExistingFile = true;
                saveAsOptions.SetWorksharingOptions(worksharingSaveAsOptions);
                doc.SaveAs(destModelPath, saveAsOptions);
                doc.Close(false);
                File.Delete(destModelPathTemp);
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show(ex.Message);
            }

        }
        private static void UiAppOnDialogBoxShowing(object sender, DialogBoxShowingEventArgs args)
        {
            args.OverrideResult(1001);
            args.Cancel();
        }


        public static ElementId GetNavisView(Document document, string navisString)
        {
            ICollection<Element> view3Ds = new FilteredElementCollector(document).OfClass(typeof(View3D)).ToElements();
            StringComparison comparer = StringComparison.OrdinalIgnoreCase;
            foreach (View3D view3D in view3Ds)
            {
                if (!(view3D.Name.ToString().IndexOf(navisString, comparer) == -1) && view3D.IsTemplate == false)
                {
                    return view3D.Id;
                    //break;
                }
            }
            return null;
        }
        public static ElementId GetIFCView(Document document, string ifcString)
        {
            // Get the first view
            ICollection<Element> view3Ds = new FilteredElementCollector(document).OfClass(typeof(View3D)).ToElements();
            StringComparison comparer = StringComparison.OrdinalIgnoreCase;
            foreach (View3D view3D in view3Ds)
            {
                if (!(view3D.Name.ToString().IndexOf(ifcString, comparer) == -1) && view3D.IsTemplate == false)
                {
                    return view3D.Id;
                    //break;
                }
            }
            return null;
        }
        public static ElementId GetFBXView(Document document, string fbxString)
        {
            ICollection<Element> view3Ds = new FilteredElementCollector(document).OfClass(typeof(View3D)).ToElements();
            StringComparison comparer = StringComparison.OrdinalIgnoreCase;
            foreach (View3D view3D in view3Ds)
            {
                if (!(view3D.Name.ToString().IndexOf(fbxString, comparer) == -1) && !view3D.IsTemplate)
                {
                    return view3D.Id;
                }
            }
            return null;
        }
        public static void UnloadRevitLinks(string path)
        {
            ModelPath location = ModelPathUtils.ConvertUserVisiblePathToModelPath(path);
            TransmissionData transData = TransmissionData.ReadTransmissionData(location);
            if (transData != null)
            {
                ICollection<ElementId> externalReferences = transData.GetAllExternalFileReferenceIds();
                foreach (ElementId refId in externalReferences)
                {
                    ExternalFileReference extRef = transData.GetLastSavedReferenceData(refId);
                    if (extRef.ExternalFileReferenceType == ExternalFileReferenceType.RevitLink)
                    {
                        transData.SetDesiredReferenceData(refId, extRef.GetPath(), extRef.PathType, false);
                    }
                }
                transData.IsTransmitted = true;
                TransmissionData.WriteTransmissionData(location, transData);
            }
        }
        public static Document OpenModel(string path, Application app)
        {
            ModelPath modelPath = ModelPathUtils.ConvertUserVisiblePathToModelPath(path);
            OpenOptions openOptions = new OpenOptions();
            openOptions.Audit = true;
            openOptions.DetachFromCentralOption = DetachFromCentralOption.DetachAndPreserveWorksets;
            return app.OpenDocumentFile(modelPath, openOptions);
        }
    }
}
