using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Input;

namespace EEPRevitPlagin.EEPRPCommandModules.FontChanging
{
    [Autodesk.Revit.Attributes.Transaction(Autodesk.Revit.Attributes.TransactionMode.Manual)]
    internal class FontChangingCommand : IExternalCommand
    {

        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIApplication uiApp = commandData.Application;
            Document doc = uiApp.ActiveUIDocument.Document;

            try
            {
                LanguageChangeCommandWPF window = new LanguageChangeCommandWPF(doc);
                window.ShowDialog();
                return Result.Succeeded;
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
                return Result.Cancelled;
            }
        }
        public void TextNoteMethod(StyleTextDictionary[] styles, Document doc, string choiceFont)
        {
            using (Transaction tran = new Transaction(doc, "Changing Font"))
            {
                tran.Start();

                ICollection<Element> textType =
                    new FilteredElementCollector(doc).OfClass(typeof(TextNoteType)).ToElements();

                foreach (Element item in textType)
                {
                    item.get_Parameter(BuiltInParameter.TEXT_FONT).Set(choiceFont);
                }

                StyleTextMethod(styles, textType);

                tran.Commit();
            }
        }
        public void TextElementTypeMethod(StyleTextDictionary[] styles, Document doc, string choiceFont)
        {
            using (Transaction tran = new Transaction(doc, "Changing TextElementType"))
            {
                tran.Start();

                ICollection<Element> textElementType =
                    new FilteredElementCollector(doc).OfClass(typeof(TextElementType)).ToElements();

                foreach (Element item in textElementType)
                {
                    item.get_Parameter(BuiltInParameter.TEXT_FONT).Set(choiceFont);
                }

                StyleTextMethod(styles, textElementType);

                tran.Commit();
            }
        }
        public void DimensionTypeMethod(StyleTextDictionary[] styles, Document doc, string choiceFont)
        {
            using (Transaction tran = new Transaction(doc, "Changing TextElementType"))
            {
                tran.Start();

                ICollection<Element> textElementType =
                    new FilteredElementCollector(doc).OfClass(typeof(DimensionType)).ToElements();

                foreach (Element item in textElementType)
                {
                    item.get_Parameter(BuiltInParameter.TEXT_FONT).Set(choiceFont);
                }

                StyleTextMethod(styles, textElementType);

                tran.Commit();
            }
        }
        public void ChangingFamilyMethod(StyleTextDictionary[] styles, Document doc, string choiceFont)
        {
            ICollection<Element> families =
                new FilteredElementCollector(doc).OfClass(typeof(FamilySymbol)).WhereElementIsElementType().ToElements();

            List<string> filteredElements = new List<string>();

            foreach (Element e in families)
            {
                if (e.Category.CategoryType.ToString() == "Annotation")
                {
                    ElementType ele = e as ElementType;
                    string familyName = ele.FamilyName;
                    if (!filteredElements.Contains(familyName))
                    {
                        filteredElements.Add(familyName);

                        FamilySymbol fs = e as FamilySymbol;
                        Family f = fs.Family;
                        Document editFamily = doc.EditFamily(f);
                        doc.EditFamily((e as FamilySymbol).Family);

                        ICollection<Element> elementTextType = new FilteredElementCollector(editFamily).OfClass(typeof(TextNoteType)).ToElements();
                        if (elementTextType.Count > 0) ChangingFamilyMethod(styles, editFamily, choiceFont);

                        TextNoteMethod(styles, editFamily, choiceFont);
                        TextElementTypeMethod(styles, editFamily, choiceFont);
                        editFamily.LoadFamily(doc, new FamilyOptions());
                    }
                }
            }
        }

        public void StyleTextMethod(StyleTextDictionary[] styles, ICollection<Element> elements)
        {
            foreach (StyleTextDictionary style in styles)
            {
                foreach (Element e in elements)
                {
                    if(style.IsStyleUsed == true) e.get_Parameter(style.BuiltInParameterEnum).Set(1);
                    else e.get_Parameter(style.BuiltInParameterEnum).Set(0);
                }
            }
        }
        public class FamilyOptions : IFamilyLoadOptions 
        {
            public bool OnFamilyFound(bool familyInUse, out bool overwriteParameterValues)
            {
                overwriteParameterValues = true;
                return true;
            }

            public bool OnSharedFamilyFound(Family sharedFamily, bool familyInUse, out FamilySource source,
                out bool overwriteParameterValues)
            {
                source = FamilySource.Family;
                overwriteParameterValues = true;
                return true;
            }
        }
    }
}