#region Usings
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Autodesk.Revit.UI;
using System.Windows.Media.Imaging;
using System.Reflection;
#endregion

namespace EEPRevitPlagin
{
    [Autodesk.Revit.Attributes.Transaction(Autodesk.Revit.Attributes.TransactionMode.Manual)]
    class App : IExternalApplication
    {
        public static string assemblyPath;
        public static string assemblyFolder;
        public static string ribbonPath;
        public Result OnStartup(UIControlledApplication application)
        {
            assemblyPath = typeof(App).Assembly.Location;
            assemblyFolder = Path.GetDirectoryName(assemblyPath);
            ribbonPath = Path.Combine(assemblyFolder, "EEPRPCommandModules");

            string tabName = "EEP";
            try { application.CreateRibbonTab(tabName); } catch { }

            try
            {
                CreateRibbon(application, tabName);

                return Result.Succeeded;
            }
            catch (Exception ex)
            {
                TaskDialog.Show("Ribbon Sample", ex.ToString());

                return Result.Failed;
            }

        }
        public Result OnShutdown(UIControlledApplication application)
        {
            return Result.Succeeded;
        }
        private void CreateRibbon(UIControlledApplication uiApp, string tabName)
        {
            RibbonPanel panelAR = uiApp.CreateRibbonPanel(tabName, "АР");
            panelAR.AddItem(CreateButtonData("InstallationDoorWindowLintels", "InstallationDoorWindowLintelsCommand"));
            panelAR.AddSeparator();
            //RibbonPanel panelKR = uiApp.CreateRibbonPanel(tabName, "КР");
            //panelKR.AddItem(CreateButtonData("Test", "TestCommand"));
            //panelKR.AddSeparator();
            RibbonPanel panelHVAC = uiApp.CreateRibbonPanel(tabName, "ИОС");
            panelHVAC.AddItem(CreateButtonData("CuttingOpening", "CuttingOpeningCommand"));
            panelHVAC.AddSeparator();

            RibbonPanel panelServer = uiApp.CreateRibbonPanel(tabName, "Revit Server");
            panelServer.AddItem(CreateButtonData("RevitServerExport", "RevitServerExportCommand"));
        }
        public PushButtonData CreateButtonData(string assemblyName, string className)
        {
<<<<<<< HEAD
            string fullClassname = "EEPRevitPlagin.EEPRPCommandModules." + assemblyName + "." + className;
=======
            string fullClassname = "EEPRevitPlagin.EEPRPCommandModules."+ assemblyName + "." + className;
>>>>>>> removed a small error
            string dataPath = Path.Combine(ribbonPath, assemblyName, "data");
            string largeIcon = Path.Combine(dataPath, className + "_large.png");
            string smallIcon = Path.Combine(dataPath, className + "_small.png");
            string textPath = Path.Combine(dataPath, className + ".txt");
            string[] text = File.ReadAllLines(textPath);
            string title = text[0].Replace("\\n", "\n");
            string tooltip = text[1];
            string url = text[2];

            PushButtonData data = new PushButtonData(className, title, assemblyPath, fullClassname);
            data.LargeImage = new BitmapImage(new Uri(largeIcon, UriKind.Absolute));
            data.Image = new BitmapImage(new Uri(smallIcon, UriKind.Absolute));
            data.ToolTip = text[1];
            ContextualHelp chelp = new ContextualHelp(ContextualHelpType.Url, url);
            data.SetContextualHelp(chelp);
            return data;
        }
    }
}
