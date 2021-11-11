using BH.oM.Base;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace BH.Engine.Diffing.Tests
{
    public static partial class Query
    {
        // Checks if this assembly has copyright information, and if that contains "BHoM" as substring.
        // Useful to distinguish BHoM VS. non-BHoM assemblies contained in a folder.
        public static bool HasBHoMCopyright(Assembly assembly)
        {
            string copyright = "";
            try
            {
                object[] attribs = assembly.GetCustomAttributes(typeof(AssemblyCopyrightAttribute), true);
                if (attribs.Length > 0)
                {
                    copyright = ((AssemblyCopyrightAttribute)attribs[0]).Copyright;
                }

                return copyright.Contains("BHoM");
            }
            catch (Exception e)
            {
                $"Could not obtain copyright information for assembly {assembly.GetName().Name}".Dump();
            }

            return false;
        }
    }
}
