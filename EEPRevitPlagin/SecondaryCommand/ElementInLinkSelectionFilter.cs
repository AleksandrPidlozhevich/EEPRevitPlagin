using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI.Selection;

namespace EEPRevitPlagin.SecondaryCommand
{
    public class ElementInLinkSelectionFilter<T> : ISelectionFilter where T : Element
    {
        private Document Doc;

        public ElementInLinkSelectionFilter(Document doc)
        {
            Doc = doc;
        }

        public Document LinkedDocument { get; private set; } = null;

        public bool LastCheckedWasFromLink
        {
            get { return null != LinkedDocument; }
        }

        public bool AllowElement(Element e)
        {
            return true;
        }

        public bool AllowReference(Reference r, XYZ p)
        {
            LinkedDocument = null;

            Element e = Doc.GetElement(r);

            if (e is RevitLinkInstance)
            {
                RevitLinkInstance li = e as RevitLinkInstance;

                LinkedDocument = li.GetLinkDocument();

                e = LinkedDocument.GetElement(r.LinkedElementId);
            }
            return e is T;
        }
    }
}
