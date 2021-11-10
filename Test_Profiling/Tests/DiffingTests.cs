/*
 * This file is part of the Buildings and Habitats object Model (BHoM)
 * Copyright (c) 2015 - 2020, the respective contributors. All rights reserved.
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
using System.Security.Cryptography;
using BH.Engine.Diffing;
using System.Diagnostics;
using BH.oM.Diffing;
using BH.Engine.Base;
using System.IO;
using Newtonsoft.Json;


namespace Tests
{
    internal static partial class DiffingTests
    {
        public static void IDiffing_HashDiffing()
        {
            var currentMethod = MethodBase.GetCurrentMethod();
            Console.WriteLine($"\nRunning {currentMethod.DeclaringType.Name}.{currentMethod.Name}");
            Stopwatch sw = Stopwatch.StartNew();

            DiffingConfig diffingConfig = new DiffingConfig();

            List<IBHoMObject> firstBatch = new List<IBHoMObject>();
            List<IBHoMObject> secondBatch = new List<IBHoMObject>();

            for (int i = 0; i < 3; i++)
            {
                Bar obj = BH.Engine.Base.Create.RandomObject(typeof(Bar)) as Bar;
                obj.Name = "bar_" + i.ToString();
                firstBatch.Add(obj as dynamic);
            }

            for (int i = 0; i < 3; i++)
            {
                Bar obj = BH.Engine.Base.Create.RandomObject(typeof(Bar)) as Bar;
                obj.Name = "bar_" + i.ToString();
                secondBatch.Add(obj as dynamic);
            }

            // This should internally trigger the method "DiffWithHash", which cannot return "modifiedObjects".
            Diff diff = BH.Engine.Diffing.Compute.IDiffing(firstBatch, secondBatch, diffingConfig);
            sw.Stop();
            long timespan = sw.ElapsedMilliseconds;

            Debug.Assert(diff.AddedObjects.Count() == 3, "Incorrect number of object identified as new/Added.");
            Debug.Assert(diff.ModifiedObjects == null || diff.ModifiedObjects.Count() == 0, "Incorrect number of object identified as modified.");
            Debug.Assert(diff.RemovedObjects.Count() == 3, "Incorrect number of object identified as old/Removed.");
            var objectDifferences = diff.ModifiedObjectsDifferences?.FirstOrDefault();
            Debug.Assert(!objectDifferences?.Differences?.Any() ?? true, "HashDiffing cannot return property Differences, but some were returned.");

            Console.WriteLine($"Concluded successfully in {timespan}");
        }

        public static void DiffWithFragmentId_allModifiedObjects()
        {
            var currentMethod = MethodBase.GetCurrentMethod();
            Console.WriteLine($"\nRunning {currentMethod.DeclaringType.Name}.{currentMethod.Name}");
            Stopwatch sw = Stopwatch.StartNew();

            DiffingConfig DiffingConfig = new DiffingConfig();

            List<IBHoMObject> firstBatch = Utils.GenerateRandomObjects(typeof(Bar), 3, true, true);
            List<IBHoMObject> secondBatch = Utils.GenerateRandomObjects(typeof(Bar), 3, true, true);

            Diff diff = BH.Engine.Diffing.Compute.DiffWithFragmentId(firstBatch, secondBatch, typeof(TestIdFragment), "Id");
            sw.Stop();
            long timespan = sw.ElapsedMilliseconds;

            Debug.Assert(diff.AddedObjects.Count() == 0, "Incorrect number of object identified as new/ToBeCreated.");
            Debug.Assert(diff.ModifiedObjects.Count() == 3, "Incorrect number of object identified as modified/ToBeUpdated.");
            Debug.Assert(diff.RemovedObjects.Count() == 0, "Incorrect number of object identified as old/ToBeDeleted.");
            //var modifiedPropsPerObj = diff.ModifiedPropsPerObject?.FirstOrDefault().Value;
            //Debug.Assert(modifiedPropsPerObj == null, "Incorrect number of changed properties identified by the property-level diffing.");

            Console.WriteLine($"Concluded successfully in {timespan}");
        }

        public static void DiffWithFragmentId_allUnchangedObjects()
        {
            var currentMethod = MethodBase.GetCurrentMethod();
            Console.WriteLine($"\nRunning {currentMethod.DeclaringType.Name}.{currentMethod.Name}");
            Stopwatch sw = Stopwatch.StartNew();

            DiffingConfig DiffingConfig = new DiffingConfig();

            List<IBHoMObject> firstBatch = Utils.GenerateRandomObjects(typeof(Bar), 3, true, true);
            List<IBHoMObject> secondBatch = new List<IBHoMObject>();
            secondBatch.AddRange(firstBatch);

            Diff diff = BH.Engine.Diffing.Compute.DiffWithFragmentId(firstBatch, secondBatch, typeof(TestIdFragment), "Id");
            sw.Stop();
            long timespan = sw.ElapsedMilliseconds;

            Debug.Assert(diff.AddedObjects.Count() == 0, "Incorrect number of object identified as new/ToBeCreated.");
            Debug.Assert(diff.ModifiedObjects.Count() == 0, "Incorrect number of object identified as modified/ToBeUpdated.");
            Debug.Assert(diff.RemovedObjects.Count() == 0, "Incorrect number of object identified as old/ToBeDeleted.");
            Debug.Assert(diff.UnchangedObjects.Count() == 3, "Incorrect number of object identified as unchanged.");

            Console.WriteLine($"Concluded successfully in {timespan}");
        }

        public static void ObjectDifferences_DifferentBars()
        {
            var currentMethod = MethodBase.GetCurrentMethod();
            Console.WriteLine($"\nRunning {currentMethod.DeclaringType.Name}.{currentMethod.Name}");
            Stopwatch sw = Stopwatch.StartNew();

            Bar bar1 = new Bar()
            {
                StartNode = new Node()
                {
                    Position = new Point() { X = 0, Y = 0, Z = 10 },
                    Name = "startNode1"
                },
                Name = "bar1"
            };

            Bar bar2 = new Bar()
            {
                StartNode = new Node()
                {
                    Position = new Point() { X = 0, Y = 0, Z = 99 }, // Different `Bar.StartNode.Position.Z`
                    Name = "startNode2"  // Different `Bar.StartNode.Name`
                },
                Name = "bar2" // Different `Bar.Name`
            };

            ObjectDifferences objectDifferences = BH.Engine.Diffing.Query.ObjectDifferences(bar1, bar2);

            Debug.Assert(objectDifferences.FollowingObject == bar2);
            Debug.Assert(objectDifferences.PastObject == bar1);
            Debug.Assert(objectDifferences.Differences.Count == 3, "Incorrect number of propertyDifferences found.");

            var differences_BarStartNodePositionZ = objectDifferences.Differences.Where(d => d.FullName == "BH.oM.Structure.Elements.Bar.StartNode.Position.Z");
            Debug.Assert(differences_BarStartNodePositionZ.Count() == 1);
            Debug.Assert(differences_BarStartNodePositionZ.FirstOrDefault().DisplayName == "StartNode.Position.Z");
            Debug.Assert(differences_BarStartNodePositionZ.FirstOrDefault().PastValue as double? == 10);
            Debug.Assert(differences_BarStartNodePositionZ.FirstOrDefault().FollowingValue as double? == 99);

            var differences_BarStartNodeName = objectDifferences.Differences.Where(d => d.FullName == "BH.oM.Structure.Elements.Bar.StartNode.Name");
            Debug.Assert(differences_BarStartNodeName.Count() == 1);
            Debug.Assert(differences_BarStartNodeName.FirstOrDefault().DisplayName == "StartNode.Name");
            Debug.Assert(differences_BarStartNodeName.FirstOrDefault().PastValue as string == "startNode1");
            Debug.Assert(differences_BarStartNodeName.FirstOrDefault().FollowingValue as string == "startNode2");

            var differences_BarName = objectDifferences.Differences.Where(d => d.FullName == "BH.oM.Structure.Elements.Bar.Name");
            Debug.Assert(differences_BarName.Count() == 1);
            Debug.Assert(differences_BarName.FirstOrDefault().DisplayName == "Name");
            Debug.Assert(differences_BarName.FirstOrDefault().PastValue as string == "bar1");
            Debug.Assert(differences_BarName.FirstOrDefault().FollowingValue as string == "bar2");

            sw.Stop();
            long timespan = sw.ElapsedMilliseconds;
            Console.WriteLine($"Concluded successfully in {timespan}");
        }

        public static void DifferentProperties_DifferentBars()
        {
            var currentMethod = MethodBase.GetCurrentMethod();
            Console.WriteLine($"\nRunning {currentMethod.DeclaringType.Name}.{currentMethod.Name}");
            Stopwatch sw = Stopwatch.StartNew();

            Bar bar1 = new Bar()
            {
                StartNode = new Node()
                {
                    Position = new Point() { X = 0, Y = 0, Z = 10 },
                    Name = "startNode1"
                },
                Name = "bar1"
            };

            Bar bar2 = new Bar()
            {
                StartNode = new Node()
                {
                    Position = new Point() { X = 0, Y = 0, Z = 99 }, // Different `Bar.StartNode.Position.Z`
                    Name = "startNode2"  // Different `Bar.StartNode.Name`
                },
                Name = "bar2" // Different `Bar.Name`
            };

            Dictionary<string, Tuple<object, object>> differentProperties = BH.Engine.Diffing.Query.DifferentProperties(bar1, bar2);

            Debug.Assert(differentProperties.Count == 3, "Incorrect number of propertyDifferences found.");

            var differences_BarStartNodePositionZ = differentProperties["BH.oM.Structure.Elements.Bar.StartNode.Position.Z"];
            Debug.Assert(differences_BarStartNodePositionZ.Item1 as double? == 10);
            Debug.Assert(differences_BarStartNodePositionZ.Item2 as double? == 99);

            var differences_BarStartNodeName = differentProperties["BH.oM.Structure.Elements.Bar.StartNode.Name"];
            Debug.Assert(differences_BarStartNodeName.Item1 as string == "startNode1");
            Debug.Assert(differences_BarStartNodeName.Item2 as string == "startNode2");

            var differences_BarName = differentProperties["BH.oM.Structure.Elements.Bar.Name"];
            Debug.Assert(differences_BarName.Item1 as string == "bar1");
            Debug.Assert(differences_BarName.Item2 as string == "bar2");

            sw.Stop();
            long timespan = sw.ElapsedMilliseconds;
            Console.WriteLine($"Concluded successfully in {timespan}");
        }

        public static void ObjectDifferences_PropertiesToInclude_FullName_Equals()
        {
            var currentMethod = MethodBase.GetCurrentMethod();
            Console.WriteLine($"\nRunning {currentMethod.DeclaringType.Name}.{currentMethod.Name}");
            Stopwatch sw = Stopwatch.StartNew();

            Bar bar1 = new Bar()
            {
                StartNode = new Node()
                {
                    Position = new Point() { X = 0, Y = 0, Z = 10 },
                    Name = "startNode1"
                },
                Name = "bar1"
            };

            Bar bar2 = new Bar()
            {
                StartNode = new Node()
                {
                    Position = new Point() { X = 0, Y = 0, Z = 99 }, // Different `Bar.StartNode.Position.Z`
                    Name = bar1.StartNode.Name  // SAME `Bar.StartNode.Name`
                },
                Name = "bar2" // Different `Bar.Name`
            };

            // Consider ONLY differences in terms of `BH.oM.Structure.Elements.Bar.StartNode.Name`. We should not find any.
            ComparisonConfig cc = new ComparisonConfig() { PropertiesToConsider = new List<string>() { "BH.oM.Structure.Elements.Bar.StartNode.Name" } };
            ObjectDifferences objectDifferences = BH.Engine.Diffing.Query.ObjectDifferences(bar1, bar2, cc);

            Debug.Assert(objectDifferences == null || objectDifferences.Differences.Count == 0, "No difference should have been found.");

            sw.Stop();
            long timespan = sw.ElapsedMilliseconds;
            Console.WriteLine($"Concluded successfully in {timespan}");
        }

        public static void ObjectDifferences_PropertiesToInclude_PartialName_Equals()
        {
            var currentMethod = MethodBase.GetCurrentMethod();
            Console.WriteLine($"\nRunning {currentMethod.DeclaringType.Name}.{currentMethod.Name}");
            Stopwatch sw = Stopwatch.StartNew();

            Bar bar1 = new Bar()
            {
                StartNode = new Node()
                {
                    Position = new Point() { X = 0, Y = 0, Z = 10 },
                    Name = "startNode1"
                },
                Name = "bar1"
            };

            Bar bar2 = new Bar()
            {
                StartNode = new Node()
                {
                    Position = new Point() { X = 0, Y = 0, Z = 99 }, // Different `Bar.StartNode.Position.Z`
                    Name = bar1.StartNode.Name  // SAME `Bar.StartNode.Name`
                },
                Name = "bar2" // Different `Bar.Name`
            };

            // Consider ONLY differences in terms of `BH.oM.Structure.Elements.Bar.StartNode.Name`. We should not find any.
            ComparisonConfig cc = new ComparisonConfig() { PropertiesToConsider = new List<string>() { "StartNode.Name" } }; // using "partial property path", instead of fully specifying `BH.oM.Structure.Elements.Bar.StartNode.Name`
            ObjectDifferences objectDifferences = BH.Engine.Diffing.Query.ObjectDifferences(bar1, bar2, cc);

            Debug.Assert(objectDifferences == null || objectDifferences.Differences.Count == 0, "No difference should have been found.");

            sw.Stop();
            long timespan = sw.ElapsedMilliseconds;
            Console.WriteLine($"Concluded successfully in {timespan}");
        }

        public static void ObjectDifferences_PropertiesToInclude_WildCardPrefix_Equals()
        {
            var currentMethod = MethodBase.GetCurrentMethod();
            Console.WriteLine($"\nRunning {currentMethod.DeclaringType.Name}.{currentMethod.Name}");
            Stopwatch sw = Stopwatch.StartNew();

            Bar bar1 = new Bar()
            {
                StartNode = new Node()
                {
                    Position = new Point() { X = 0, Y = 0, Z = 10 },
                    Name = "startNode1"
                },
                Name = "bar1"
            };

            Bar bar2 = new Bar()
            {
                StartNode = new Node()
                {
                    Position = new Point() { X = 0, Y = 0, Z = 99 }, // Different `Bar.StartNode.Position.Z`
                    Name = bar1.StartNode.Name  // SAME `Bar.StartNode.Name`
                },
                Name = bar1.Name // SAME `Bar.Name`
            };

            // Consider ONLY differences in terms of names: `*.Name`.  We should not find any.
            ComparisonConfig cc = new ComparisonConfig() { PropertiesToConsider = new List<string>() { "*.Name" } }; 
            ObjectDifferences objectDifferences = BH.Engine.Diffing.Query.ObjectDifferences(bar1, bar2, cc);

            Debug.Assert(objectDifferences == null || objectDifferences.Differences.Count == 0, "No difference should have been found.");

            // Or equivalently, without any wildcard: `Name`. Result should be the same, we should not find any.
            cc = new ComparisonConfig() { PropertiesToConsider = new List<string>() { "Name" } };
            objectDifferences = BH.Engine.Diffing.Query.ObjectDifferences(bar1, bar2, cc);

            Debug.Assert(objectDifferences == null || objectDifferences.Differences.Count == 0, "No difference should have been found.");

            sw.Stop();
            long timespan = sw.ElapsedMilliseconds;
            Console.WriteLine($"Concluded successfully in {timespan}");
        }

        public static void ObjectDifferences_PropertiesToInclude_WildCardMiddle_Equals()
        {
            var currentMethod = MethodBase.GetCurrentMethod();
            Console.WriteLine($"\nRunning {currentMethod.DeclaringType.Name}.{currentMethod.Name}");
            Stopwatch sw = Stopwatch.StartNew();

            Bar bar1 = new Bar()
            {
                StartNode = new Node()
                {
                    Position = new Point() { X = 0, Y = 0, Z = 0 },
                    Name = "startNode1"
                },
                EndNode = new Node()
                {
                    Position = new Point() { X = 0, Y = 0, Z = 99 },
                    Name = "endNode1"
                },
                Name = "bar1"
            };

            Bar bar2 = new Bar()
            {
                StartNode = new Node()
                {
                    Position = new Point() { X = 0, Y = 0, Z = 55 }, // DIFFERENT `Bar.StartNode.Position.Z`
                    Name = bar1.StartNode.Name  // SAME `Bar.StartNode.Name`
                },
                EndNode = new Node()
                {
                    Position = new Point() { X = 0, Y = 0, Z = 77 }, // DIFFERENT `Bar.EndNode.Position.Z`
                    Name = bar1.EndNode.Name  // SAME `Bar.EndNode.Name`
                },
                Name = "bar2" // DIFFERENT Bar.Name
            };

            // Consider ONLY differences in terms of StartNode AND EndNode names: `Bar.*.Name`. We should not find any.
            ComparisonConfig cc = new ComparisonConfig() { PropertiesToConsider = new List<string>() { "Bar.*.Name" } };
            ObjectDifferences objectDifferences = BH.Engine.Diffing.Query.ObjectDifferences(bar1, bar2, cc);

            Debug.Assert(objectDifferences == null || objectDifferences.Differences.Count == 0, "No difference should have been found.");

            sw.Stop();
            long timespan = sw.ElapsedMilliseconds;
            Console.WriteLine($"Concluded successfully in {timespan}");
        }


        public static void IDiffingTest_DiffWithFragmentId_allDifferentFragments()
        {
            var currentMethod = MethodBase.GetCurrentMethod();
            Console.WriteLine($"\nRunning {currentMethod.DeclaringType.Name}.{currentMethod.Name}");
            Stopwatch sw = Stopwatch.StartNew();

            DiffingConfig diffingConfig = new DiffingConfig();

            List<IBHoMObject> firstBatch = new List<IBHoMObject>();
            List<IBHoMObject> secondBatch = new List<IBHoMObject>();

            for (int i = 0; i < 3; i++)
            {
                Bar obj = BH.Engine.Base.Create.RandomObject(typeof(Bar)) as Bar;
                TestIdFragment testIdFragment = new TestIdFragment() { Id = i };
                RandomNumberFragment randomNumberFragment = new RandomNumberFragment();

                obj.Name = "bar_" + i.ToString();
                obj.AddFragment(testIdFragment);
                obj.AddFragment(randomNumberFragment);


                firstBatch.Add(obj as dynamic);
            }

            for (int i = 0; i < 3; i++)
            {
                Bar obj = BH.Engine.Base.Create.RandomObject(typeof(Bar)) as Bar;
                TestIdFragment testIdFragment = new TestIdFragment() { Id = i };
                RandomNumberFragment randomNumberFragment = new RandomNumberFragment();

                obj.Name = "bar_" + i.ToString();
                obj.AddFragment(testIdFragment);
                obj.AddFragment(randomNumberFragment);

                secondBatch.Add(obj as dynamic);
            }

            // This should internally trigger the method "DiffWithHash", which cannot return "modifiedObjects".
            Diff diff = BH.Engine.Diffing.Compute.IDiffing(firstBatch, secondBatch, diffingConfig);
            sw.Stop();
            long timespan = sw.ElapsedMilliseconds;

            Debug.Assert(diff.AddedObjects.Count() == 3, "Incorrect number of object identified as new/ToBeCreated.");
            Debug.Assert(diff.ModifiedObjects == null || diff.ModifiedObjects.Count() == 0, "Incorrect number of object identified as modified/ToBeUpdated.");
            Debug.Assert(diff.RemovedObjects.Count() == 3, "Incorrect number of object identified as old/ToBeDeleted.");
            var objectDifferences = diff.ModifiedObjectsDifferences?.FirstOrDefault();
            Debug.Assert(!objectDifferences.Differences?.Any() ?? true, "Incorrect number of changed properties identified by the property-level diffing.");

            Console.WriteLine($"Concluded successfully in {timespan}");
        }
    }
}
