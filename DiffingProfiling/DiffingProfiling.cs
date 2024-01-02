/*
 * This file is part of the Buildings and Habitats object Model (BHoM)
 * Copyright (c) 2015 - 2024, the respective contributors. All rights reserved.
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
using BH.oM.Diffing;
using System.Diagnostics;

namespace BH.Tests.Diffing
{
    public static partial class DiffingProfiling
    {
        public static void GeneralProfiling()
        {
            Console.WriteLine("\n/**********************************/");
            Console.WriteLine("\n\t\t\tDIFFING PROFILING");
            Console.WriteLine("Profiles using increasingly large sets of objects (n = 100, 1000, 2000, 5000, 10000)." +
                "\nA set of n objects is generated; then a new set is generated by taking the old set" +
                "\nand modifying a fixed percentage (first 10%, then 50%) of the objects picked at random." +
                "\nThen, the two sets so obtained are diffed using the Revision diffing, " +
                "\nwhich is the most representative Diffing method (leverages Hashes, Fragments, and finds also ModifiedObjects)." +
                "\nThe above procedure is repeated once to cover `EnablePropertyDiffing` disabled/enabled.");
            Console.WriteLine("\n/**********************************/");

            string path = @"C:\temp\Diffing_Engine-ProfilingTask01.txt";
            File.Delete(path);

            List<int> numberOfObjects = null;
            List<float> percentagesModified = new List<float>() { 0.1F, 0.5F };
            DiffingConfig DiffingConfig = null;

            // First profiling. Only collection-level diffing.
            numberOfObjects = new List<int>() { 100, 1000, 2000, 5000, 10000 };

            DiffingConfig = new DiffingConfig() { EnablePropertyDiffing = false };

            numberOfObjects.ForEach(objTot => ProfilingTask(objTot, percentagesModified, DiffingConfig, path));

            // Second profiling. Collection-level + property-level. 
            numberOfObjects = new List<int>() { 100, 1000, 2000, 5000 };
            DiffingConfig = new DiffingConfig() { EnablePropertyDiffing = true };

            numberOfObjects.ForEach(objTot => ProfilingTask(objTot, percentagesModified, DiffingConfig, path));

            Console.WriteLine("\nProfiling concluded.");
        }

        private static void ProfilingTask(int totalObjs, List<float> percentagesModified, DiffingConfig DiffingConfig, string path = null)
        {
            string introMessage = $"\n---Diffing for {totalObjs} randomly generated objects.";
            introMessage += DiffingConfig.EnablePropertyDiffing ? " Collection-level + property-level." : " Only collection-level.";
            introMessage += "---";
            Console.WriteLine(introMessage);

            Stopwatch totalSw = new Stopwatch();
            totalSw.Start();

            if (path != null)
            {
                string fName = Path.GetFileNameWithoutExtension(path);
                string ext = Path.GetExtension(path);
                fName += DiffingConfig.EnablePropertyDiffing ? "_propLevel" : "_onlyCollLevel";
                path = Path.Combine(Path.GetDirectoryName(path), fName + ext);
            }

            // Generate random objects
            List<IBHoMObject> currentObjs = BH.Engine.Diffing.Tests.Create.RandomBHoMObjects(typeof(Bar), totalObjs);

            // Create Stream. This assigns the Hashes.
            Stopwatch swRev1 = new Stopwatch();
            swRev1.Start();

            Revision revision = BH.Engine.Diffing.Create.Revision(currentObjs, Guid.NewGuid(), "RevisionName", "comment", DiffingConfig);

            swRev1.Stop();
            Console.WriteLine($"1st Revision (hashComputing) - total elapsed ms: {swRev1.ElapsedMilliseconds}");

            totalSw.Stop();
            var totElapsMs = totalSw.ElapsedMilliseconds;
            totalSw.Reset();

            // Modify randomly some percentage of the objects.
            var allIdxs = Enumerable.Range(0, currentObjs.Count).ToList();

            foreach (float percMod in percentagesModified)
            {
                Console.WriteLine($"Pick randomly {percMod * 100}% of objects, modify them, then create new Revision.");

                var readObjs = revision.Objects.Cast<IBHoMObject>().Select(obj => BH.Engine.Base.Query.DeepClone(obj)).ToList();

                int numberModified = (int)(currentObjs.Count * percMod);
                List<int> randIdxs = allIdxs.OrderBy(g => Guid.NewGuid()).Take(numberModified).ToList();
                List<int> remainingIdx = allIdxs.Except(randIdxs).ToList();

                List<IBHoMObject> updatedSet = randIdxs.Select(idx => readObjs.ElementAt(idx)).ToList();
                updatedSet.ForEach(obj => obj.Name = "ModifiedName");
                updatedSet.AddRange(remainingIdx.Select(idx => readObjs.ElementAt(idx)).Cast<IBHoMObject>().ToList());

                // Update stream revision
                Stopwatch swRev2 = new Stopwatch();
                swRev2.Start();
                totalSw.Start();

                Revision updatedRevision = BH.Engine.Diffing.Create.Revision(updatedSet, Guid.NewGuid());

                swRev2.Stop();
                Console.WriteLine($"\t2nd Revision (hashComputing) - total elapsed ms: {swRev2.ElapsedMilliseconds}");

                // Actual diffing
                Stopwatch deltaSw = new Stopwatch();
                deltaSw.Start();

                Delta delta = BH.Engine.Diffing.Create.Delta(revision, updatedRevision, DiffingConfig);
                Diff diff = delta.Diff;

                deltaSw.Stop();
                Console.WriteLine($"\tDelta (diffing computing) elapsed ms: {deltaSw.ElapsedMilliseconds}");

                totalSw.Stop();
                Console.WriteLine($"\tTotal elapsed milliseconds: {totalSw.ElapsedMilliseconds + totElapsMs}");
                totalSw.Reset();

                if (delta.Diff.ModifiedObjects.Count() == numberModified) Console.WriteLine("\n\n\t\t\tERROR: Diffing didn't work\n.");

                //if (path != null)
                //{
                //    System.IO.File.AppendAllText(path, Environment.NewLine + introMessage + Environment.NewLine + endMessage);
                //    Console.WriteLine($"Results appended in {path}");
                //}
            }
        }

    }
}



