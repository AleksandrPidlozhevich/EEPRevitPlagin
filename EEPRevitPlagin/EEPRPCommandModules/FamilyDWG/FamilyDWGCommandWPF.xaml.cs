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

namespace EEPRevitPlagin.EEPRPCommandModules.FamilyDWG
{
    /// <summary>
    /// Interaction logic for FamilyDWGCommandWPF.xaml
    /// </summary>
    public partial class FamilyDWGCommandWPF : Window
    {
        // Свойство для хранения пути к файлу
        public string FilePath { get; set; }

        public FamilyDWGCommandWPF()
        {
            InitializeComponent();
        }

        // Обработчик события для кнопки "Browse"
        private void BrowseButton_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.SaveFileDialog saveFileDialog = new Microsoft.Win32.SaveFileDialog();
            saveFileDialog.Filter = "Text files (*.txt)|*.txt|All files (*.*)|*.*";
            if (saveFileDialog.ShowDialog() == true)
            {
                FilePathTextBox.Text = saveFileDialog.FileName;
            }
        }

        // Обработчик события для кнопки "Save"
        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            // Установите свойство FilePath, которое будет использовано в команде FamilyDWGCommand
            FilePath = FilePathTextBox.Text;

            // Закройте окно WPF
            DialogResult = true;
            Close();
        }
    }
}
