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
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Form = System.Windows.Forms.Form;

namespace EEP_Revit_Plagin.EEP_RP_СommandModules.FontChanging
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
                comboBox1.Items.Add(fontFamilies[i].Name);
            }
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {

        }
        private void comboBox2_SelectedIndexChanged(object sender, EventArgs e)
        {

        }
        private void button1_Click(object sender, EventArgs e)
        {
            string choiceFont = comboBox1.SelectedItem.ToString();
            //docMethod(choiceFont);
            familyMethod(choiceFont);
        }


        private void docMethod(string choiceFont) //Получение типов текста и запись нового шрифта в документе
        {
            using (Transaction tran = new Transaction(Doc, "Changing Font"))
            {
                tran.Start();

                ICollection<Element> textType =
                    new FilteredElementCollector(Doc).OfClass(typeof(TextNoteType)).ToElements();

                foreach (Element item in textType)
                {
                    item.get_Parameter(BuiltInParameter.TEXT_FONT).Set(choiceFont);
                }

                tran.Commit();
            }//end using
        }

        private void familyMethod(string choiceFont) //Получение типов текста и запись нового шрифта в семействе
        {
            ICollection<Element> families =
                new FilteredElementCollector(Doc).OfClass(typeof(FamilySymbol)).WhereElementIsElementType().ToElements();
            foreach (Element i in families)
            {
                if (i.Category.CategoryType.ToString() == "Annotation")
                {
                    comboBox2.Items.Add(i.);
                }
            }

            //TaskDialog.Show("123", c.ToString());
        }
    }
}



