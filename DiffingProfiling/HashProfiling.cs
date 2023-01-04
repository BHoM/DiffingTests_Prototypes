/*
 * This file is part of the Buildings and Habitats object Model (BHoM)
 * Copyright (c) 2015 - 2023, the respective contributors. All rights reserved.
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

using BH.oM.Structure.Elements;
using BH.oM.Geometry;
using BH.Engine;
using BH.oM.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Security.Cryptography;
using BH.Engine.Serialiser;
using BH.oM.Diffing;
using System.Diagnostics;
using BH.Engine.Base.Objects;
using System.Collections;
using BH.Engine.Base;

namespace BH.Tests.Diffing
{
    public static partial class HashProfiling
    {
        public static void HashObjects(int totalObjsPerType = 5000, List<Type> types = null, BaseComparisonConfig comparisonConfig = null)
        {
            var asteriskRow = $"\n{string.Join("", Enumerable.Repeat("*", 50))}";

            types = types ?? new List<Type>() { typeof(Bar), typeof(NurbsCurve) };
            string introMessage = $"{asteriskRow}" +
                $"\nHashing {totalObjsPerType} randomly generated object of types:" +
                $"\n{string.Join(", ", types.Select(t => t.Name))}" +
                $"\nfor a total of {totalObjsPerType * types.Count} objects." +
                $"{asteriskRow}";
        
            Console.WriteLine(introMessage);

            comparisonConfig = comparisonConfig ?? new ComparisonConfig();

            Stopwatch totalSw = new Stopwatch();
            totalSw.Start();

            // Generate random objects
            List<IObject> objects = new List<IObject>();
            types.ForEach(t => objects.AddRange(BH.Engine.Diffing.Tests.Create.RandomIObjects(t, totalObjsPerType)));
            totalSw.Stop();
            Console.WriteLine($"\nObject creation time: {totalSw.ElapsedMilliseconds}ms");

            totalSw.Restart();
            objects.ForEach(o => o.Hash());
            totalSw.Stop();
            Console.WriteLine($"\nObject hashing time: {totalSw.ElapsedMilliseconds}ms");
        }
    }
}
