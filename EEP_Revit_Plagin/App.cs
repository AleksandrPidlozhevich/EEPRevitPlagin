#region Usings
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Autodesk.Revit.UI;
using System.Windows.Media.Imaging;
#endregion

namespace EEP_Revit_Plagin
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
            ribbonPath = Path.Combine(assemblyFolder, "EEP_Revit_Plagin");

            string tabName = "EEP_RP";
            application.CreateRibbonTab(tabName);
            return Result.Succeeded;
        }
        public Result OnShutdown(UIControlledApplication application)
        {
            return Result.Succeeded;
        }
        private void CreateRibbon(UIControlledApplication uiApp, string tabName)
        {
            RibbonPanel panelHVAC = uiApp.CreateRibbonPanel(tabName, "ИОС");
            panelHVAC.AddItem(CreateButtonData("СuttingOpening", "СuttingOpeningCommand"));
        }
        public PushButtonData CreateButtonData(string assemblyName, string className)
        {
            string dllPath = Path.Combine(ribbonPath, assemblyName, assemblyName + ".dll");
            string fullClassname = assemblyName + "." + className;
            string dataPath = Path.Combine(ribbonPath, assemblyName, "data");
            string largeIcon = Path.Combine(dataPath, className + "_large.png");
            string smallIcon = Path.Combine(dataPath, className + "_small.png");
            string textPath = Path.Combine(dataPath, className + ".txt");
            string[] text = File.ReadAllLines(textPath);
            string title = text[0].Replace("\\n", "\n");
            string tooltip = text[1];
            string url = text[2];

            PushButtonData data = new PushButtonData(fullClassname, title, dllPath, fullClassname);

            data.LargeImage = new BitmapImage(new Uri(largeIcon, UriKind.Absolute));
            data.Image = new BitmapImage(new Uri(smallIcon, UriKind.Absolute));

            data.ToolTip = text[1];

            ContextualHelp chelp = new ContextualHelp(ContextualHelpType.Url, url);
            data.SetContextualHelp(chelp);

            return data;
        }
        

    }
}
