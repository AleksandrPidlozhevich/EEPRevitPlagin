using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EEPRevitPlagin.SecondaryCommand
{
    public class ConvUnits
    {
        public double FootToMM(double foot)
        {
            return foot * 304.8;
        }

        public double MMToFoot(double mm)
        {
            return mm / 304.8;
        }
    }
}
