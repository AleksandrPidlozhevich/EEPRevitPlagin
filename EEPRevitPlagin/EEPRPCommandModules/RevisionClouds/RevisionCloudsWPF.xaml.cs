using System;
using System.Windows;

namespace EEPRevitPlagin.EEPRPCommandModules.RevisionClouds
{
    /// <summary>
    /// Interaction logic for RevisionCloudsWPF.xaml
    /// </summary>
    public partial class RevisionCloudsWPF : Window
    {
        public string SelectedSet { get; private set; }

        public RevisionCloudsWPF()
        {
            InitializeComponent();
            doButton.Click += DoButton_Click;
            exButton.Click += ExButton_Click;
            
        }

        private void DoButton_Click(object sender, RoutedEventArgs e)
        {
            SelectedSet = comboBoxSets.SelectedItem as string;
            DialogResult = true;
            Close();
        }

        private void ExButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private void workingDrawings_Checked(object sender, RoutedEventArgs e)
        {

        }
    }
}