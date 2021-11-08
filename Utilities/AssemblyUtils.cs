using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Utilities
{
    public static class AssemblyUtils
    {
        public static List<Assembly> LoadAssemblies(string dllDirectory = @"C:\ProgramData\BHoM\Assemblies", bool onlyBHoMAssemblies = true, bool tryLoadWithoutDependencies = false)
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
                assemblies = assemblies.Where(assembly => HasBHoMCopyright(assembly)).ToList();

            return assemblies;
        }

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

        // Checks if this assembly has copyright information, and if that contains "BHoM" as substring.
        // Useful to distinguish BHoM VS. non-BHoM assemblies contained in a folder.
        private static bool HasBHoMCopyright(Assembly assembly)
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