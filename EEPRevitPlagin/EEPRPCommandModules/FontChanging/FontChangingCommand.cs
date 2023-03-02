using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Text;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Form = System.Windows.Forms.Form;

namespace EEPRevitPlagin.EEPRPCommandModules.FontChanging
{
    public partial class FontChangingForm : Form
    {
        Document Doc;
        public FontChangingForm(Document doc)
        {
            InitializeComponent();
            Doc = doc;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            //Получение списка доступных шрифтов
            InstalledFontCollection installedFontCollection = new InstalledFontCollection();
            FontFamily[] fontFamilies = installedFontCollection.Families;

            int count = fontFamilies.Length;
            for (int i = 0; i < count; ++i)
            {
                fontList.Items.Add(fontFamilies[i].Name);
            }
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {

        }
        private void button1_Click(object sender, EventArgs e)
        {
            string choiceFont = fontList.SelectedItem.ToString();
            TextNoteMethod(Doc, choiceFont);
            TextElementTypeMethod(Doc, choiceFont);
            DimensionTypeMethod(Doc, choiceFont);
            ChangingFamilyMethod(Doc, choiceFont);
            MessageBox.Show( "Операция выполнена");
        }


        private void TextNoteMethod(Document doc, string choiceFont) //Изменение типов текста в документе
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

                tran.Commit();
            }//end using
        }

        private void ChangingFamilyMethod(Document doc, string choiceFont) //Изменение семейств аннотаций
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

                    // фильтрация повторяющихся типов
                    if (!filteredElements.Contains(familyName))
                    {
                        filteredElements.Add(familyName);

                        FamilySymbol fs = e as FamilySymbol;
                        Family f = fs.Family;
                        Document editFamily = doc.EditFamily(f);
                        doc.EditFamily((e as FamilySymbol).Family);
                        
                        //Запуск изменения вложенных семейств
                        ICollection<Element> elementTextType = new FilteredElementCollector(editFamily).OfClass(typeof(TextNoteType)).ToElements();
                        if (elementTextType.Count > 0 ) ChangingFamilyMethod(editFamily, choiceFont);

                        TextNoteMethod(editFamily, choiceFont);
                        TextElementTypeMethod(editFamily, choiceFont);
                        editFamily.LoadFamily(doc, new FamilyOptions());
                    }
                }
            }
        }

        private void TextElementTypeMethod(Document doc, string choiceFont) //Изменение типов марок в документе
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

                tran.Commit();
            }
        }
        private void DimensionTypeMethod(Document doc, string choiceFont)
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

                tran.Commit();
            }
        }
    }
    [Transaction(TransactionMode.Manual)]
    
    internal class FontChangingCommand : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIApplication uiApp = commandData.Application;
            Document doc = uiApp.ActiveUIDocument.Document;
            
            //Запуск команды
            try
            {
                Application.Run(new FontChangingForm(doc));
                return Result.Succeeded;
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
                return Result.Cancelled;
            }
            
        }
    }

    public class FamilyOptions : IFamilyLoadOptions //реализация интерфейса автоматической загрузки семейства
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




