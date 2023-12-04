using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace EEPRevitPlagin.EEPRPCommandModules.SpecificationExport
{
    /// <summary>
    /// Interaction logic for SpecificationExportCommandWPF.xaml
    /// </summary>
    public partial class SpecificationExportCommandWPF : Window
    {
        public string SelectedSpecification { get; private set; }
        public string FolderPath { get; private set; }

        public SpecificationExportCommandWPF(List<string> specificationNames)
        {
            InitializeComponent();
            specificationComboBox.ItemsSource = specificationNames;

            // Здесь вызываем метод для установки значения пути
            UpdateFolderPathText("C:\\"); // Замените "C:\\" на значение по умолчанию

            // Добавляем обработчик события TextChanged
            folderTB.TextChanged += FolderTB_TextChanged;
        }

        private void UpdateFolderPathText(string path)
        {
            folderTB.Text = path; // Обновляем текст в TextBox
            FolderPath = path; // Устанавливаем значение FolderPath
        }

        private void ExportButton_Click(object sender, RoutedEventArgs e)
        {
            if (specificationComboBox.SelectedItem != null)
            {
                SelectedSpecification = specificationComboBox.SelectedItem.ToString();
                this.Close();
            }
            else
            {
                System.Windows.MessageBox.Show("Пожалуйста, выберите спецификацию.");
            }
        }

        private void FolderB_Click(object sender, RoutedEventArgs e)
        {
            using (var fbd = new FolderBrowserDialog())
            {
                fbd.Description = "Select a folder to export the models to:";
                DialogResult result = fbd.ShowDialog();
                if (result == System.Windows.Forms.DialogResult.OK && !string.IsNullOrWhiteSpace(fbd.SelectedPath))
                {
                    // Вызываем метод для обновления пути
                    UpdateFolderPathText(fbd.SelectedPath);
                }
            }
        }

        private void FolderTB_TextChanged(object sender, TextChangedEventArgs e)
        {
            // Обработчик изменения текста в TextBox
            FolderPath = folderTB.Text;
        }
    }
}