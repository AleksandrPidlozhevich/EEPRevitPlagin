using System;
using System.Collections;
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
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

namespace EEPRevitPlagin.EEPRPCommandModules.WallFinishCreator
{
    /// <summary>
    /// Interaction logic for WallFinishCreatorWPF.xaml
    /// </summary>
    public partial class WallFinishCreatorWPF : Window
    {
        public ObservableCollection<WallFinishCreatorCollectionItem> WallFinishCreatorCollectionItemsList;
        public string WallFinishHeight;
        public bool RoomBoundary;
        List<WallFinishCreatorItem> WallFinishCreatorItemsList;
        public WallFinishCreatorWPF(List<Material> materialsList, List<WallType> wallTypesList)
        {
            InitializeComponent();
            WallFinishCreatorCollectionItemsList = new ObservableCollection<WallFinishCreatorCollectionItem>();
            foreach (Material material in materialsList)
            {
                WallFinishCreatorCollectionItemsList.Add(new WallFinishCreatorCollectionItem(material, null));
            }
            //Создание списка для сопоставления отделки
            dataGrid_Mapping.ItemsSource = WallFinishCreatorCollectionItemsList;
            dataGridComboBoxColumnWallFinishType.ItemsSource = wallTypesList;
            dataGridComboBoxColumnWallFinishType.DisplayMemberPath = "Name";

            WallFinishCreatorItemsList = new List<WallFinishCreatorItem>();
            WallFinishCreatorItemsList = new WallFinishCreatorSettings().GetSettings();
            string heightSettings = new WallFinishCreatorSettings().GetHeightSettings();
            if (heightSettings != null)
            {
                textBox_WallFinishHeight.Text = heightSettings;
            }

            bool roomBoundarySettings = new WallFinishCreatorSettings().GetRoomBoundarySettings();
            if (roomBoundarySettings != null)
            {
                checkBox_RoomBoundary.IsChecked = roomBoundarySettings;
            }

            foreach (WallFinishCreatorItem item in WallFinishCreatorItemsList)
            {
                Material mat = materialsList.FirstOrDefault(m => m.Name == item.MaterialName);
                WallType wt = wallTypesList.FirstOrDefault(i => i.Name == item.WallFinishTypeName);
                foreach (WallFinishCreatorCollectionItem collectionItem in WallFinishCreatorCollectionItemsList)
                {
                    if (mat != null)
                    {
                        if (collectionItem.BaseWallMaterial.Name == mat.Name)
                        {
                            collectionItem.WallFinishType = wt;
                        }
                    }
                }
            }
        }
        private void btn_Ok_Click(object sender, RoutedEventArgs e)
        {
            bool flag = false;
            foreach (WallFinishCreatorCollectionItem item in WallFinishCreatorCollectionItemsList)
            {
                if (item.WallFinishType == null)
                {
                    flag = true;
                    continue;
                }
            }
            if (flag)
            {
                TaskDialog.Show("Revit", "Заполните тип отделки для всех материалов основы");
            }
            else
            {
                WallFinishHeight = textBox_WallFinishHeight.Text;
                SaveSettings();
                DialogResult = true;
                Close();
            }
        }

        private void btn_Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
        private void WallFinishCreatorWPF_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter || e.Key == Key.Space)
            {
                bool flag = false;
                foreach (WallFinishCreatorCollectionItem item in WallFinishCreatorCollectionItemsList)
                {
                    if (item.WallFinishType == null)
                    {
                        flag = true;
                        continue;
                    }
                }
                if (flag)
                {
                    TaskDialog.Show("Revit", "Заполните тип отделки для всех материалов основы");
                }
                else
                {
                    WallFinishHeight = textBox_WallFinishHeight.Text;
                    SaveSettings();
                    DialogResult = true;
                    Close();
                }
            }

            else if (e.Key == Key.Escape)
            {
                DialogResult = false;
                Close();
            }
        }
        private void SaveSettings()
        {
            IEnumerable dataGridItemCollection = dataGrid_Mapping.ItemsSource;
            foreach (WallFinishCreatorCollectionItem item in dataGridItemCollection)
            {
                if (item.WallFinishType != null)
                {
                    WallFinishCreatorItemsList.Add(new WallFinishCreatorItem(item.BaseWallMaterial.Name, item.WallFinishType.Name));
                }
                else
                {
                    WallFinishCreatorItemsList.Add(new WallFinishCreatorItem(item.BaseWallMaterial.Name, null));
                }
            }
            RoomBoundary = (bool)checkBox_RoomBoundary.IsChecked;

            new WallFinishCreatorSettings().Save(WallFinishCreatorItemsList, textBox_WallFinishHeight.Text, RoomBoundary);
        }
    }
}
