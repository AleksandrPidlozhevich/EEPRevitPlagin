using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EEPRevitPlagin.SecondaryСommand
{
    public class SymboolParameters
    {
        public Parameter ParameterValue { get; set; }
        public FamilySymbol FamilySymbolValue { get; set; }
        public SymboolParameters(Parameter parameter, FamilySymbol familySymbol)
        {
            ParameterValue = parameter;
            FamilySymbolValue = familySymbol;
        }
    }
}