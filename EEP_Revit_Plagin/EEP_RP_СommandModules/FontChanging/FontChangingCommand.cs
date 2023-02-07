using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.UI;
using Autodesk.Revit.DB;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace EEP_Revit_Plagin.EEP_RP_СommandModules.FontChanging
{
    [Transaction(TransactionMode.Manual)]
    internal class FontChangingCommand : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIApplication uiApp = commandData.Application;
            Document doc = uiApp.ActiveUIDocument.Document;

            
            /*using (System.Windows.Forms.Form form = new FontChangingForm(doc))
            {
                if (form.ShowDialog() == DialogResult.OK)
                {
                    return Result.Succeeded;
                }
                else
                {
                    return Result.Cancelled;
                }
            }*/
            
            Application.Run(new FontChangingForm(doc));
            return Result.Succeeded;
        }
    }
}
