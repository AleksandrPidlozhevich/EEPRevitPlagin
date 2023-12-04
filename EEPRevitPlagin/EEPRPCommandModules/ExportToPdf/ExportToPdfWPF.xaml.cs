using System.Linq;
using System.Windows;
using System.Windows.Controls;
//using System.Windows.Forms;
using System.Windows.Input;
using System.Xml;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using Autodesk.Revit.DB;
using System.Data;
using System.Collections.Generic;
using System;
using System.Windows.Media;
using System.Collections.ObjectModel;

namespace EEPRevitPlagin.EEPRPCommandModules.ExportToPdf
{
    /// <summary>
    /// Interaction logic for ExportToPdfWPF.xaml
    /// </summary>
    public partial class ExportToPdfWPF : Window
    {
        ExternalEvent externalEvent;
        DataTable dt = new DataTable();
        public static ObservableCollection<Sheet> sheets = new ObservableCollection<Sheet>();
        public ExportToPdfWPF(ExternalEvent e)
        {
            InitializeComponent();
            externalEvent = e;
            sheets.Clear();
            foreach (ElementId id in ExportToPdfCommand.viewSheetsIds)
            {
                Element sheetElement = ExportToPdfCommand.doc.GetElement(id);
                Element sheetTemplate = null;
                try
                {
                    ElementCategoryFilter filter = new ElementCategoryFilter(BuiltInCategory.OST_TitleBlocks);
                    sheetTemplate = ExportToPdfCommand.doc.GetElement(sheetElement.GetDependentElements(filter)[0]);
                }
                catch
                {

                }
                if (sheetTemplate != null)
                {
                    Sheet sheet = new Sheet()
                    {
                        ToExport = false,
                        Id = id,
                        Name = sheetElement.Name,
                        Number = sheetElement.get_Parameter(BuiltInParameter.SHEET_NUMBER).AsString(),
                        Template = sheetTemplate.Name,
                        Height = Convert.ToInt16(sheetTemplate.get_Parameter(BuiltInParameter.SHEET_HEIGHT).AsValueString()),
                        Width = Convert.ToInt16(sheetTemplate.get_Parameter(BuiltInParameter.SHEET_WIDTH).AsValueString()),
                        //Orientation = sheetTemplate.get_Parameter(BuiltInParameter.INSERT_ORIENTATION).AsValueString(),
                        ColourSetting = "one"
                    };
                    sheets.Add(sheet);
                }
            }

            DataGridCheckBoxColumn exportColumn = new DataGridCheckBoxColumn();
            DataGridTextColumn sheetNameColumn = new DataGridTextColumn();
            DataGridTextColumn sheetNumberColumn = new DataGridTextColumn();
            DataGridTextColumn sheetTemplateColumn = new DataGridTextColumn();
            DataGridTextColumn heightColumn = new DataGridTextColumn();
            DataGridTextColumn widthColumn = new DataGridTextColumn();
            DataGridTextColumn orientationColumn = new DataGridTextColumn();
            //DataGridTemplateColumn exportColumn = new DataGridTemplateColumn();
            DataGridComboBoxColumn colourSettingColumn = new DataGridComboBoxColumn();

            exportColumn.Header = "Export";
            sheetNameColumn.Header = "Sheet Name";
            sheetNumberColumn.Header = "Sheet Number";
            sheetTemplateColumn.Header = "Sheet Template";
            heightColumn.Header = "Height";
            widthColumn.Header = "Width";
            orientationColumn.Header = "orientation";
            colourSettingColumn.Header = "Colour Setting";
            
            colourSettingColumn.ItemsSource = new string[] { "one", "two" };

            System.Windows.Data.Binding exportBinding = new System.Windows.Data.Binding("ToExport");
            System.Windows.Data.Binding nameBinding = new System.Windows.Data.Binding("Name");
            System.Windows.Data.Binding numbertBinding = new System.Windows.Data.Binding("Number");
            System.Windows.Data.Binding templateBinding = new System.Windows.Data.Binding("Template");
            System.Windows.Data.Binding heightBinding = new System.Windows.Data.Binding("Height");
            System.Windows.Data.Binding widthBinding = new System.Windows.Data.Binding("Width");
            System.Windows.Data.Binding orientationBinding = new System.Windows.Data.Binding("Orientation");
            System.Windows.Data.Binding colourBinding = new System.Windows.Data.Binding("ColourSetting");

            exportBinding.Mode = System.Windows.Data.BindingMode.TwoWay;
            nameBinding.Mode = System.Windows.Data.BindingMode.TwoWay;
            numbertBinding.Mode = System.Windows.Data.BindingMode.TwoWay;
            templateBinding.Mode = System.Windows.Data.BindingMode.TwoWay;
            heightBinding.Mode = System.Windows.Data.BindingMode.TwoWay;
            widthBinding.Mode = System.Windows.Data.BindingMode.TwoWay;
            orientationBinding.Mode = System.Windows.Data.BindingMode.OneWay;
            colourBinding.Mode = System.Windows.Data.BindingMode.TwoWay;

            exportColumn.Binding = exportBinding;
            sheetNameColumn.Binding = nameBinding;
            sheetNumberColumn.Binding = numbertBinding;
            sheetTemplateColumn.Binding = templateBinding;
            heightColumn.Binding = heightBinding;
            widthColumn.Binding = widthBinding;
            orientationColumn.Binding = orientationBinding;
            colourSettingColumn.SelectedValueBinding = colourBinding;
            /*
            DataTemplate dataTemplate = new DataTemplate();
            FrameworkElementFactory factory = new FrameworkElementFactory(typeof(CheckBox));
            factory.SetBinding(CheckBox.IsCheckedProperty,exportBinding);            
            dataTemplate.VisualTree = factory;
            exportColumn.CellTemplate = dataTemplate;
            */

            SheetTable.Columns.Add(exportColumn);
            SheetTable.Columns.Add(sheetNameColumn);
            SheetTable.Columns.Add(sheetNumberColumn);
            SheetTable.Columns.Add(sheetTemplateColumn);
            SheetTable.Columns.Add(heightColumn);
            SheetTable.Columns.Add(widthColumn);
            SheetTable.Columns.Add(orientationColumn);
            SheetTable.Columns.Add(colourSettingColumn);            

            SheetTable.SelectionMode = DataGridSelectionMode.Extended;
            SheetTable.IsReadOnly = false;
            SheetTable.AutoGenerateColumns = false;
            SheetTable.ItemsSource = sheets;
            SheetTable.CanUserAddRows = false;

        }
        private void exB1_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
                DragMove();
        }

        private void ExportB_Click(object sender, RoutedEventArgs e)
        {
            /*
            List<Element> printSettings = new FilteredElementCollector(ExportToPdfCommand.doc).OfClass(typeof(PrintSetting)).ToElements().ToList();
            PrintSetting printSetting = (PrintSetting)printSettings[0];
            foreach (Sheet sheet in sheets)
            {
                if (sheet.ToExport)
                {
                    PrintSheet(sheet);
                }
            }
            */
            externalEvent.Raise();
        }
        private void PrintSheet(Sheet sheet)
        {
            View view = (View)ExportToPdfCommand.doc.GetElement(sheet.Id);
            PrintManager printManager = ExportToPdfCommand.doc.PrintManager;
            printManager.PrintRange = PrintRange.Select;
            printManager.Apply();
            ViewSet viewSet = new ViewSet();
            viewSet.Insert(view);
            ViewSheetSetting viewSheetSetting = printManager.ViewSheetSetting;
            viewSheetSetting.CurrentViewSheetSet.Views=viewSet;
            viewSheetSetting.SaveAs("Current Print");
            printManager.SelectNewPrintDriver("PDF24");
            printManager.CombinedFile = true;
            printManager.PrintToFile = true;
            printManager.PrintToFileName = @"C:\as.pdf";
            printManager.Apply();
            printManager.SubmitPrint();
        }
        private void SettingsB_Click(object sender, RoutedEventArgs e)
        {
            ExportToPdfSettingsWPF exportToPdfSettingsWPF = new ExportToPdfSettingsWPF();
            exportToPdfSettingsWPF.ShowDialog();
        }
    }
    public class Sheet
    {
        public string Name { get; set; }
        public ElementId Id { get; set; }
        public bool ToExport { get; set; }
        public string Number { get; set; }
        public string Template { get; set; }
        public int Height { get; set; }
        public int Width { get; set; }
        public string Orientation => GetOrientaion();

        private string GetOrientaion()
        {
            if (Height > Width)
            {
                return "Portrait";
            }
            else
            {
                return "Landscape";
            }
        }

        public string ColourSetting { get; set; }

    }

}
