using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Resources;
using System.Windows;
using System.Windows.Controls;

namespace EEPRevitPlagin.EEPRPCommandModules.LanguageChange
{
    /// <summary>
    /// Interaction logic for LanguageChangeCommandWPF.xaml
    /// </summary>
    public partial class LanguageChangeCommandWPF : Window
    {
        private readonly string _langSettingsPath = Path.Combine(Path.GetDirectoryName(typeof(App).Assembly.Location), "SecondaryCommand", "LangResources", "langsettings.json");
        private readonly string _configPath = Path.Combine(Path.GetDirectoryName(typeof(App).Assembly.Location), "SecondaryCommand", "LangResources", "config.json");
        private List<Language> _languages;
        private Language _selectedLanguage;
        public LanguageChangeCommandWPF()
        {
            InitializeComponent();
            LoadLanguages();
            LoadConfig();
            // Set the selected language in the combobox
            LanguageComboBox.SelectedItem = _selectedLanguage;
        }
        private void LoadLanguages()
        {
            // Load the languages from the langsettings.json file
            try
            {
                string json = File.ReadAllText(_langSettingsPath);
                _languages = JsonConvert.DeserializeObject<List<Language>>(json);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to load languages from {_langSettingsPath}: {ex.Message}");
                Close();
            }
            // Set the languages as the data source for the combobox
            LanguageComboBox.ItemsSource = _languages;
        }

        private void LoadConfig()
        {
            // Load the language from the config.json file
            try
            {
                string json = File.ReadAllText(_configPath);
                var config = JsonConvert.DeserializeObject<Config>(json);
                var culture = config.Language;

                // Find the selected language in the list of available languages
                _selectedLanguage = _languages.FirstOrDefault(l => l.Culture.Equals(culture));
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to load config from {_configPath}: {ex.Message}");
                Close();
            }
        }

        private void SaveConfig()
        {
            // Save the selected language to the config.json file
            try
            {
                var config = new Config { Language = _selectedLanguage.Culture };
                string json = JsonConvert.SerializeObject(config, Formatting.Indented);
                File.WriteAllText(_configPath, json);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to save config to {_configPath}: {ex.Message}");
            }
        }

        private void LanguageComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Update the selected language
            _selectedLanguage = (Language)LanguageComboBox.SelectedItem;

            // Hide the restart message
            ReloadRevitLabel.Visibility = System.Windows.Visibility.Collapsed;

        }
        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            // Save the selected language and close the window
            SaveConfig();

            // Update the text in ReloadRevitLabel based on the selected language
            Language selectedLanguage = (Language)LanguageComboBox.SelectedItem;
            string languageResourceKey = "Please_restart_Revit_for_the_changes_to_take_effect";
            string localizedText = EEPRevitPlagin.SecondaryCommand.LangResources.Language.ResourceManager.GetString(languageResourceKey, new System.Globalization.CultureInfo(selectedLanguage.Culture));
            ReloadRevitLabel.Text = localizedText;

            // Show the ReloadRevitLabel
            ReloadRevitLabel.Visibility = Visibility.Visible;
        }
    }

    public class Language
    {
        public string Name { get; set; }
        public string Culture { get; set; }
    }

    public class Config
    {
        public string Language { get; set; }
    }
}