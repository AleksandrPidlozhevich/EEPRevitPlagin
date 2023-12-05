#region Usings
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Events;
using Autodesk.Revit.UI;
using EEPRevitPlagin.EEPRPCommandModules.ScheduleRowCounter;
using EEPRevitPlagin.EEPRPCommandModules.SpecificationChecker;
using Newtonsoft.Json;
using System;
using System.Globalization;
using System.IO;
using System.Threading;
using System.Windows.Media.Imaging;
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
            //application.ControlledApplication.DocumentOpened += OnDocumentOpened;
            //Read from config.json which language to use
            string path = Path.Combine(assemblyFolder, "SecondaryCommand", "LangResources", "config.json");
            if (File.Exists(path))
            {
                string json = File.ReadAllText(path);
                dynamic settings = JsonConvert.DeserializeObject(json);
                string language = settings.Language;
                if (!string.IsNullOrEmpty(language))
                {
                    CultureInfo ci = new CultureInfo(language);
                    Thread.CurrentThread.CurrentCulture = ci;
                    Thread.CurrentThread.CurrentUICulture = ci;
                }
            }
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
            System.Resources.ResourceManager langRes = new System.Resources.ResourceManager("EEPRevitPlagin.SecondaryCommand.LangResources.Language", System.Reflection.Assembly.GetExecutingAssembly());
            RibbonPanel panelAR = uiApp.CreateRibbonPanel(tabName, langRes.GetString("Architectural_PanelName"));
            panelAR.AddItem(CreateButtonData("InstallationDoorWindowLintels", "InstallationDoorWindowLintelsCommand", false));
            panelAR.AddSeparator();
            #region SplitButtonWallFinish
            SplitButtonData splitButtonWallFinish = new SplitButtonData("WallFinishSplitGroup", "WallFinishSplitGroup");
            RibbonItem ribbonItemWallFinish = panelAR.AddItem(splitButtonWallFinish);
            SplitButton splitWallFinish = ribbonItemWallFinish as SplitButton;
            PushButtonData pushButtonWallFinishCreator = CreateButtonData("WallFinishCreator", "WallFinishCreatorCommand", false);
            splitWallFinish.AddPushButton(pushButtonWallFinishCreator);
            PushButtonData pushButtonWallFinishCreatorProperties = CreateButtonData("WallFinishCreatorProperties", "WallFinishCreatorPropertiesCommand", false);
            splitWallFinish.AddPushButton(pushButtonWallFinishCreatorProperties);
            #endregion
            //panelAR.AddItem(CreateButtonData("WallSplit", "WallSplitCommand", false));
            //RibbonPanel panelKR = uiApp.CreateRibbonPanel(tabName, langRes.GetString("Structural_PanelName"));
            RibbonPanel panelHVAC = uiApp.CreateRibbonPanel(tabName, langRes.GetString("MEP_PanelName"));
            panelHVAC.AddItem(CreateButtonData("DuctInsulationCheck", "DuctInsulationCheckCommand", false));
            //panelHVAC.AddItem(CreateButtonData("CuttingOpening", "CuttingOpeningCommand", false));
            //panelHVAC.AddSeparator();
            panelAR.AddItem(CreateButtonData("RoomBoundaryLines", "RoomBoundaryLinesCommand", false));
            RibbonPanel panelBIM = uiApp.CreateRibbonPanel(tabName, "BIM");
            panelBIM.AddItem(CreateButtonData("FontChanging", "FontChangingCommand", false));
            panelBIM.AddSeparator();
            panelBIM.AddItem(CreateButtonData("LanguageChange", "LanguageChangeCommand", true));
            panelBIM.AddSeparator();
            panelBIM.AddItem(CreateButtonData("RevitServerExport", "RevitServerExportCommand", true));
            panelBIM.AddSeparator();
            panelBIM.AddItem(CreateButtonData("FindOverrideViewElements", "FindOverrideViewElementsCommand", false));
            panelBIM.AddSeparator();
            panelBIM.AddItem(CreateButtonData("ScheduleRowCounter", "ScheduleRowCounterCommand", false));
            panelBIM.AddSeparator();
            panelBIM.AddItem(CreateButtonData("ListNumbering", "ListNumberingCommand", false));
            panelBIM.AddSeparator();
            panelBIM.AddItem(CreateButtonData("ModelCompare", "AnalyzeSourceModelCommand", false));
            panelBIM.AddSeparator();
            panelBIM.AddItem(CreateButtonData("ModelCompare", "ModelCompareCommand", false));
            panelBIM.AddSeparator();
            panelBIM.AddItem(CreateButtonData("CheckModelChanges", "CheckModelChangesCommand", false));
            panelBIM.AddSeparator();
            panelBIM.AddItem(CreateButtonData("RevisionClouds", "RevisionCloudsCommand", false));
            panelBIM.AddItem(CreateButtonData("SpecificationExport", "SpecificationExportCommand", false));
            #region splitButtonPropertiesCopy
            SplitButtonData splitButtonOffAnalytics = new SplitButtonData("OffAnalytics", "OffAnalytics");
            RibbonItem ribbonItemOffAnalytics = panelBIM.AddItem(splitButtonOffAnalytics);
            SplitButton splitOffAnalytics = ribbonItemOffAnalytics as SplitButton;
            PushButtonData pushButtonOffAnalyticsModel = CreateButtonData("OffAnalyticsModel", "OffAnalyticsModelCommand", false);
            splitOffAnalytics.AddPushButton(pushButtonOffAnalyticsModel);
            PushButtonData pushButtonOffAnalyticsModelGroupl = CreateButtonData("OffAnalyticsModelGroup", "OffAnalyticsModelGroupCommand", false);
            splitOffAnalytics.AddPushButton(pushButtonOffAnalyticsModelGroupl);
            #endregion
            //panelBIM.AddItem(CreateButtonData("ExportToPdf", "ExportToPdfCommand", false));
            #region splitButtonPropertiesCopy
            SplitButtonData splitButtonPropertiesCopy = new SplitButtonData("PropertiesCopyGroup", "PropertiesCopyGroup");
            RibbonItem ribbonItemPropertiesCopy = panelBIM.AddItem(splitButtonPropertiesCopy);
            SplitButton splitPropertiesCopy = ribbonItemPropertiesCopy as SplitButton;
            PushButtonData pushButtonPropertiesCopy = CreateButtonData("PropertiesCopy", "PropertiesCopyCommand", false);
            splitPropertiesCopy.AddPushButton(pushButtonPropertiesCopy);
            PushButtonData pushButtonChangeTypeCopyPropertis = CreateButtonData("PropertiesCopy", "ChangeTypeCopyPropertisCommand", false);
            splitPropertiesCopy.AddPushButton(pushButtonChangeTypeCopyPropertis);
            #endregion
            //panelBIM.AddItem(CreateButtonData("AutoSave", "AutoSaveCommand", false));
            //panelBIM.AddSeparator();
        }
        public PushButtonData CreateButtonData(string assemblyName, string className, bool availableOnStartup)
        {
            string fullClassname = "EEPRevitPlagin.EEPRPCommandModules." + assemblyName + "." + className;
            string dataPath = Path.Combine(ribbonPath, assemblyName, "data");
            string largeIcon = Path.Combine(dataPath, className + "_large.png");
            string smallIcon = Path.Combine(dataPath, className + "_small.png");
            //Take the name of the buttons from Language.resx
            System.Resources.ResourceManager langRes = new System.Resources.ResourceManager("EEPRevitPlagin.SecondaryCommand.LangResources.Language", System.Reflection.Assembly.GetExecutingAssembly());
            string title = langRes.GetString(className + "_Title").Replace("\\n", "\n");
            string tooltip = langRes.GetString(className + "_Tooltip");
            string url = langRes.GetString(className + "_Url");
            PushButtonData data = new PushButtonData(className, title, assemblyPath, fullClassname);
            data.LargeImage = new BitmapImage(new Uri(largeIcon, UriKind.Absolute));
            data.Image = new BitmapImage(new Uri(smallIcon, UriKind.Absolute));
            data.ToolTip = tooltip;
            ContextualHelp chelp = new ContextualHelp(ContextualHelpType.Url, url);
            data.SetContextualHelp(chelp);
            if (availableOnStartup)
            {
                data.AvailabilityClassName = "EEPRevitPlagin.Availability";
            }
            return data;
        }
        //private void OnDocumentOpened(object sender, DocumentOpenedEventArgs e)
        //{
        //    // Получите Application из Document
        //    Document doc = e.Document;
        //    Autodesk.Revit.ApplicationServices.Application revitApp = doc.Application;

        //    // Создайте экземпляр SpecificationCheckerHandler и выполните мониторинг
        //    UIApplication uiApp = new UIApplication(revitApp);
        //    SpecificationCheckerHandler handler = new SpecificationCheckerHandler();
        //    handler.StartMonitoring(uiApp);
        //}
    }
    public class Availability : IExternalCommandAvailability
    {
        public bool IsCommandAvailable(UIApplication a, CategorySet b)
        {
            return true;
        }
    }
}