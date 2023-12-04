using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Text;
using System.Windows;
using System.Windows.Controls;

namespace EEPRevitPlagin.EEPRPCommandModules.FontChanging
{
    /// <summary>
    /// Interaction logic for FontChangingCommandWPF.xaml
    /// </summary>
    public partial class LanguageChangeCommandWPF : Window
    {
        private Document Doc;
        public LanguageChangeCommandWPF(Document doc)
        {
            InitializeComponent();
            Doc = doc;

            //Получение списка доступных шрифтов
            InstalledFontCollection installedFontCollection = new InstalledFontCollection();
            FontFamily[] fontFamilies = installedFontCollection.Families;

            int count = fontFamilies.Length;
            for (int i = 0; i < count; ++i)
            {
                comboBoxFonts.Items.Add(fontFamilies[i].Name);
            }
        }
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            string choiceFont = comboBoxFonts.SelectedItem.ToString();

            StyleTextDictionary boldStyle =
                new StyleTextDictionary("boldStyle", Bold.IsChecked, BuiltInParameter.TEXT_STYLE_BOLD);

            StyleTextDictionary italicStyle =
                new StyleTextDictionary("italicStyle", Italic.IsChecked, BuiltInParameter.TEXT_STYLE_ITALIC);

            StyleTextDictionary underlineStyle = 
                new StyleTextDictionary("underlineStyle", Underline.IsChecked, BuiltInParameter.TEXT_STYLE_UNDERLINE);

            StyleTextDictionary[] styles = { boldStyle, italicStyle, underlineStyle };

            FontChangingCommand textNote = new FontChangingCommand(); // создание экземпляра класса
            textNote.TextNoteMethod(styles, Doc, choiceFont);
            FontChangingCommand textElement = new FontChangingCommand();
            textElement.TextElementTypeMethod(styles, Doc, choiceFont);
            FontChangingCommand dimension = new FontChangingCommand();
            dimension.DimensionTypeMethod(styles, Doc, choiceFont);
            FontChangingCommand changingFamily = new FontChangingCommand();
            changingFamily.ChangingFamilyMethod(styles, Doc, choiceFont);
            MessageBox.Show("Operation completed");
        }
    }

    public class StyleTextDictionary
    {
        public string NameOfStyle { get; set; }
        public bool? IsStyleUsed { get; set; }
        public BuiltInParameter BuiltInParameterEnum { get; set; }
  
        public StyleTextDictionary(string nameOfStyle, bool? isStyleUsed, BuiltInParameter builtInParameterEnum)
        {
            NameOfStyle = nameOfStyle;
            IsStyleUsed = isStyleUsed;
            BuiltInParameterEnum = builtInParameterEnum;
        }
    }
}