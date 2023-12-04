using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace EEPRevitPlagin.SecondaryСommand
{
    internal class ComparatorString : IComparer<string>
    {
        static readonly Regex ChunkRe = new Regex(@"^(\d+|\D+)");
        static readonly Regex ChunkStringRe = new Regex(@"(\d+)(\D+)");

        public int Compare(string x, string y)
        {
            int xIndex = 0, yIndex = 0;
            while (xIndex < x.Length && yIndex < y.Length)
            {
                // Check if current characters of both strings are digits
                if (char.IsDigit(x[xIndex]) && char.IsDigit(y[yIndex]))
                {
                    // Extract the integers from the string
                    int xValue = GetIntValue(x, ref xIndex);
                    int yValue = GetIntValue(y, ref yIndex);

                    // Compare the integers and return the result if they are not equal
                    int result = xValue.CompareTo(yValue);
                    if (result != 0)
                        return result;
                }
                else
                {
                    // Compare the characters and return the result if they are not equal
                    int result = x[xIndex].CompareTo(y[yIndex]);
                    if (result != 0)
                        return result;

                    xIndex++;
                    yIndex++;
                }
            }

            // Return the comparison result based on the remaining length of the strings
            return x.Length.CompareTo(y.Length);
        }

        private int GetIntValue(string s, ref int index)
        {
            int value = 0;
            while (index < s.Length && char.IsDigit(s[index]))
            {
                value = value * 10 + (s[index] - '0');
                index++;
            }
            return value;
        }
    }
}