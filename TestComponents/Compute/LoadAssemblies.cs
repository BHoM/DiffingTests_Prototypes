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
        public static List<Assembly> LoadAssemblies(string dllDirectory = @"C:\ProgramData\BHoM\Assemblies\Assemblies", bool onlyBHoMAssemblies = true, bool tryLoadWithoutDependencies = false)
        {
            var assemblyFiles = Directory.GetFiles(dllDirectory, "*.dll").ToList();
            List<Assembly> assemblies = new List<Assembly>();

            foreach (var assemblyFile in assemblyFiles)
            {
                Assembly assembly = null;

                if (tryLoadWithoutDependencies)
                    TryLoadAssemblyFileWithoutDependencies(assemblyFile, out assembly);

                if (assembly == null)
                    TryLoadAssemblyFile(assemblyFile, out assembly);
                
                if (assembly == null && !tryLoadWithoutDependencies)
                    TryLoadAssemblyFileWithoutDependencies(assemblyFile, out assembly); // as last resort if it wasn't tried before.

                if (assembly != null)
                    assemblies.Add(assembly);
            }

            if (onlyBHoMAssemblies)
                assemblies = assemblies.Where(assembly => Query.HasBHoMCopyright(assembly)).ToList();

            return assemblies;
        }
    }
}