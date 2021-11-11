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
        public static bool TryLoadAssemblyFile(string assemblyFile, out Assembly assembly)
        {
            Console.Write($"\nTrying to load {assemblyFile} with dependencies (LoadFrom): ");

            assembly = null;
            try
            {
                assembly = Assembly.LoadFrom(assemblyFile); // using LoadFrom() instead of Load() makes it work better for some reason! Especially when running from VS instead of Linqpad. https://stackoverflow.com/a/20607325/3873799

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