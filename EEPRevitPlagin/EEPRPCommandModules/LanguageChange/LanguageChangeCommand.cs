using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;
using System.Windows;

namespace EEPRevitPlagin.EEPRPCommandModules.LanguageChange
{
    [Autodesk.Revit.Attributes.Transaction(Autodesk.Revit.Attributes.TransactionMode.Manual)]
    internal class LanguageChangeCommand : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            {
                UIApplication uiApp = commandData.Application;

                try
                {
                    var window = new LanguageChangeCommandWPF();
                    window.ShowDialog();
                    return Result.Succeeded;
                }
                catch (Exception e)
                {
                    MessageBox.Show(e.Message);
                    return Result.Cancelled;
                }
            }
        }
    }
}