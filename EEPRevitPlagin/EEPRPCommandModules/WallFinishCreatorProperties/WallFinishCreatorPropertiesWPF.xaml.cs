using System;
using System.Collections.Generic;
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

namespace EEPRevitPlagin.EEPRPCommandModules.WallFinishCreatorProperties
{
    /// <summary>
    /// Interaction logic for WallFinishCreatorPropertiesWPF.xaml
    /// </summary>
    public partial class WallFinishCreatorPropertiesWPF : Window
    {
        WallFinishCreatorPropertiesSettings WallFinishCreatorPropertiesSettingsParam = null;
        public WallFinishCreatorPropertiesWPF()
        {
            InitializeComponent();
            WallFinishCreatorPropertiesSettingsParam = WallFinishCreatorPropertiesSettings.GetSettings();
            if (WallFinishCreatorPropertiesSettingsParam.FloorCreationOptionValue != null)
            {
                if (WallFinishCreatorPropertiesSettingsParam.FloorCreationOptionValue == "rbt_ByCurrentFile")
                {
                    (groupBox_FloorCreationOption.Content as System.Windows.Controls.Grid)
                    .Children.OfType<RadioButton>()
                    .FirstOrDefault(rb => rb.Name == "rbt_ByCurrentFile").IsChecked = true;
                }
                else
                {
                    (groupBox_FloorCreationOption.Content as System.Windows.Controls.Grid)
                        .Children.OfType<RadioButton>()
                        .FirstOrDefault(rb => rb.Name == "rbt_ByLinkedFile").IsChecked = true;
                }
            }
        }
        private void btn_Ok_Click(object sender, RoutedEventArgs e)
        {
            SaveSettings();
            DialogResult = true;
            Close();
        }
        private void btn_Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
        private void WallFinishCreatorOptionsWPF_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter || e.Key == Key.Space)
            {
                SaveSettings();
                DialogResult = true;
                Close();
            }

            else if (e.Key == Key.Escape)
            {
                DialogResult = false;
                Close();
            }
        }
        private void SaveSettings()
        {
            WallFinishCreatorPropertiesSettingsParam.FloorCreationOptionValue = (groupBox_FloorCreationOption.Content as System.Windows.Controls.Grid)
                .Children.OfType<RadioButton>()
                .FirstOrDefault(rb => rb.IsChecked.Value == true)
                .Name;
            WallFinishCreatorPropertiesSettingsParam.SaveSettings();
        }
    }
}