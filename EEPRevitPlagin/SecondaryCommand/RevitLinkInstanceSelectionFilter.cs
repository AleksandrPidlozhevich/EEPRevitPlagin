using Autodesk.Revit.DB;
using Autodesk.Revit.UI.Selection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EEPRevitPlagin.SecondaryCommand
{
    class RevitLinkInstanceSelectionFilter : ISelectionFilter
    {
        public bool AllowElement(Autodesk.Revit.DB.Element elem)
        {

            if (elem is RevitLinkInstance)
            {
                return true;
            }
            return false;
        }

        public bool AllowReference(Autodesk.Revit.DB.Reference reference, Autodesk.Revit.DB.XYZ position)
        {
            return false;
        }
    }
}