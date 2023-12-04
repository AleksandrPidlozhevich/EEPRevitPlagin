using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Grid = System.Windows.Controls.Grid;

namespace EEPRevitPlagin.EEPRPCommandModules.CuttingOpening
{
    /// <summary>
    /// Логика взаимодействия для CuttingOpeningWPF.xaml
    /// </summary>
    public partial class CuttingOpeningWPF : Window
    {
        public FamilySymbol wallRectangularOpening;
        public FamilySymbol floorRectangularOpening;
        public FamilySymbol wallRoundOpening;
        public FamilySymbol floorRoundOpening;

        public CuttingOpeningWPF(List<FamilySymbol> cutFamilies)
        {
            InitializeComponent();
            //foreach (FamilySymbol cutFamily in cutFamilies)
            //{
            //    ComboBox_cutFamily.Items.Add(cutFamily.Name);
            //}

            ComboBox_cutFamily1.ItemsSource = new ObservableCollection<FamilySymbol>(cutFamilies
                .Where(fs => fs.Family.Name == "Пересечение_Стена_Прямоугольное").OrderBy(fs => fs.Name));
            ComboBox_cutFamily1.DisplayMemberPath = "Name";

            ComboBox_cutFamily2.ItemsSource = new ObservableCollection<FamilySymbol>(cutFamilies
                .Where(fs => fs.Family.Name == "Пересечение_Плита_Прямоугольное").OrderBy(fs => fs.Name));
            ComboBox_cutFamily2.DisplayMemberPath = "Name";

            ComboBox_cutFamily3.ItemsSource = new ObservableCollection<FamilySymbol>(cutFamilies
                .Where(fs => fs.Family.Name == "Пересечение_Стена_Круглое").OrderBy(fs => fs.Name));
            ComboBox_cutFamily3.DisplayMemberPath = "Name";

            ComboBox_cutFamily4.ItemsSource = new ObservableCollection<FamilySymbol>(cutFamilies
                .Where(fs => fs.Family.Name == "Пересечение_Плита_Круглое").OrderBy(fs => fs.Name));
            ComboBox_cutFamily4.DisplayMemberPath = "Name";
        }

        private void Select(object sender, RoutedEventArgs e)
        {
            wallRectangularOpening = ComboBox_cutFamily1.SelectedItem as FamilySymbol;
            floorRectangularOpening = ComboBox_cutFamily2.SelectedItem as FamilySymbol;
            wallRoundOpening = ComboBox_cutFamily3.SelectedItem as FamilySymbol;
            floorRoundOpening = ComboBox_cutFamily4.SelectedItem as FamilySymbol;

            this.DialogResult = true;
            this.Close();
        }
    }
}
