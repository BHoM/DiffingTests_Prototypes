using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace BH.Engine.Diffing.Tests
{
    public static partial class Compute
    {
        public static bool TryLoadAssemblyFileWithoutDependencies(string assemblyFile, out Assembly assembly)
        {
            Console.Write($"\nTrying to load {assemblyFile} without dependencies (ReflectionOnlyLoad): ");

            assembly = null;
            try
            {
                assembly = Assembly.ReflectionOnlyLoad(assemblyFile); // this prevents problems with e.g. Revit where certain dependencies cannot be fully loaded (as if for execution).

                Console.Write($"Done\n");

                return true;

            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }

            return false;
        }


    }
}