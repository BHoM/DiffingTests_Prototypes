/*
 * This file is part of the Buildings and Habitats object Model (BHoM)
 * Copyright (c) 2015 - 2022, the respective contributors. All rights reserved.
 *
 * Each contributor holds copyright over their respective contributions.
 * The project versioning (Git) records all such contribution source information.
 *                                           
 *                                                                              
 * The BHoM is free software: you can redistribute it and/or modify         
 * it under the terms of the GNU Lesser General Public License as published by  
 * the Free Software Foundation, either version 3.0 of the License, or          
 * (at your option) any later version.                                          
 *                                                                              
 * The BHoM is distributed in the hope that it will be useful,              
 * but WITHOUT ANY WARRANTY; without even the implied warranty of               
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the                 
 * GNU Lesser General Public License for more details.                          
 *                                                                            
 * You should have received a copy of the GNU Lesser General Public License     
 * along with this code. If not, see <https://www.gnu.org/licenses/lgpl-3.0.html>.      
 */

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
