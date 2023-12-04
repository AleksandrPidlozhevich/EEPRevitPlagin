using System;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI.Selection;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EEPRevitPlagin.SecondaryСommand
{
    class WindowsAndDoorsSelectionFilter : ISelectionFilter
    {
        public bool AllowElement(Element elem)
        {
            if (elem is FamilyInstance
                && (elem.Category.Id.IntegerValue.Equals((int)BuiltInCategory.OST_Windows)
                || elem.Category.Id.IntegerValue.Equals((int)BuiltInCategory.OST_Doors)))
            {
                return true;
            }
            return false;
        }

        public bool AllowReference(Reference reference, XYZ position)
        {
            return false;
        }
    }
}