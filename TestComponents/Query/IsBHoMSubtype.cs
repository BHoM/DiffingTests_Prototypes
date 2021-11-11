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
        public static bool IsBHoMSubtype(this Type t)
        {
            return t.IsClass && (typeof(BHoMObject).IsSubclassOf(t) || typeof(IObject).IsAssignableFrom(t)) && t != typeof(BHoMObject);
        }
    }
}