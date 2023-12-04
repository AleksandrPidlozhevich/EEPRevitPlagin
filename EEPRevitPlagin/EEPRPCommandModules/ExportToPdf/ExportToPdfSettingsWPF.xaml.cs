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

namespace EEPRevitPlagin.EEPRPCommandModules.ExportToPdf
{
    /// <summary>
    /// Interaction logic for ExportToPdfSettingsWPF.xaml
    /// </summary>
    public partial class ExportToPdfSettingsWPF : Window
    {
        List<Colour> colours = new List<Colour>();
        public ExportToPdfSettingsWPF()
        {
            InitializeComponent();
            ColourList.ItemsSource = colours;
        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                ColourRect.Fill = new SolidColorBrush(Color.FromRgb(Convert.ToByte(RedVT.Text), Convert.ToByte(GreenVT.Text), Convert.ToByte(BlueVT.Text)));
            }
            catch
            {

            }
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

        private void AddColour_Click(object sender, RoutedEventArgs e)
        {
            Colour colour = new Colour();
            colour.Color = ((SolidColorBrush)ColourRect.Fill).Color;
            colours.Add(colour);
        }
    }
    public partial class Colour
    {
        public Color Color { get; set; }
        public string Content => Color.ToString();
        override public string ToString()
        {
            return string.Format("R: {0}, G: {1}, B: {2}", Color.R, Color.G, Color.B); ;
        }
    }
}
