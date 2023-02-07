using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.Attributes;

namespace EEP_Revit_Plagin.EEP_RP_СommandModules.СuttingOpening
{
    [Transaction(TransactionMode.Manual)]
    internal class СuttingOpeningCommand : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            throw new NotImplementedException();
        }
    }
}
