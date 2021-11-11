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
        public static Type[] TryGetTypes(this Assembly a)
        {
            try
            {
                return a.GetTypes();
            }
            catch (ReflectionTypeLoadException e )
            {
                Console.WriteLine($"Could not get types for assembly {a.GetName().Name}. Exception:\n {string.Join("\n ", e.LoaderExceptions.Select(le => le.Message).Distinct())}");
            }

            return new Type[] { };
        }
    }
}
