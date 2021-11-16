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
using BH.Engine.Diffing.Tests;
using BH.oM.Diffing.Test;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace BH.Tests.Diffing
{
    [TestClass]
    public class DiffingTests
    {
        [TestMethod]
        public void IDiffing_HashDiffing()
        {
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

            Assert.IsTrue(diff.AddedObjects.Count() == 3, "Incorrect number of object identified as new/Added.");
            Assert.IsTrue(diff.ModifiedObjects == null || diff.ModifiedObjects.Count() == 0, "Incorrect number of object identified as modified.");
            Assert.IsTrue(diff.RemovedObjects.Count() == 3, "Incorrect number of object identified as old/Removed.");
            var objectDifferences = diff.ModifiedObjectsDifferences?.FirstOrDefault();
            Assert.IsTrue(!objectDifferences?.Differences?.Any() ?? true, "HashDiffing cannot return property Differences, but some were returned.");
        }

        [TestMethod]
        public void DiffWithFragmentId_allModifiedObjects()
        {
            DiffingConfig DiffingConfig = new DiffingConfig();

            List<IBHoMObject> firstBatch = BH.Engine.Diffing.Tests.Query.GenerateRandomObjects(typeof(Bar), 3, true, true);
            List<IBHoMObject> secondBatch = BH.Engine.Diffing.Tests.Query.GenerateRandomObjects(typeof(Bar), 3, true, true);

            Diff diff = BH.Engine.Diffing.Compute.DiffWithFragmentId(firstBatch, secondBatch, typeof(TestIdFragment), "Id");

            Assert.IsTrue(diff.AddedObjects.Count() == 0, "Incorrect number of object identified as new/ToBeCreated.");
            Assert.IsTrue(diff.ModifiedObjects.Count() == 3, "Incorrect number of object identified as modified/ToBeUpdated.");
            Assert.IsTrue(diff.RemovedObjects.Count() == 0, "Incorrect number of object identified as old/ToBeDeleted.");
            //var modifiedPropsPerObj = diff.ModifiedPropsPerObject?.FirstOrDefault().Value;
            //Assert.IsTrue(modifiedPropsPerObj == null, "Incorrect number of changed properties identified by the property-level diffing.");
        }

        [TestMethod]
        public void DiffWithFragmentId_allUnchangedObjects()
        {
            DiffingConfig DiffingConfig = new DiffingConfig();

            List<IBHoMObject> firstBatch = BH.Engine.Diffing.Tests.Query.GenerateRandomObjects(typeof(Bar), 3, true, true);
            List<IBHoMObject> secondBatch = new List<IBHoMObject>();
            secondBatch.AddRange(firstBatch);

            Diff diff = BH.Engine.Diffing.Compute.DiffWithFragmentId(firstBatch, secondBatch, typeof(TestIdFragment), "Id");

            Assert.IsTrue(diff.AddedObjects.Count() == 0, "Incorrect number of object identified as new/ToBeCreated.");
            Assert.IsTrue(diff.ModifiedObjects.Count() == 0, "Incorrect number of object identified as modified/ToBeUpdated.");
            Assert.IsTrue(diff.RemovedObjects.Count() == 0, "Incorrect number of object identified as old/ToBeDeleted.");
            Assert.IsTrue(diff.UnchangedObjects.Count() == 3, "Incorrect number of object identified as unchanged.");
        }

        [TestMethod]
        public void ObjectDifferences_DifferentBars()
        {
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

            Assert.IsTrue(objectDifferences.FollowingObject == bar2);
            Assert.IsTrue(objectDifferences.PastObject == bar1);
            Assert.IsTrue(objectDifferences.Differences.Count == 3, "Incorrect number of propertyDifferences found.");

            var differences_BarStartNodePositionZ = objectDifferences.Differences.Where(d => d.FullName == "BH.oM.Structure.Elements.Bar.StartNode.Position.Z");
            Assert.IsTrue(differences_BarStartNodePositionZ.Count() == 1);
            Assert.IsTrue(differences_BarStartNodePositionZ.FirstOrDefault().DisplayName == "StartNode.Position.Z");
            Assert.IsTrue(differences_BarStartNodePositionZ.FirstOrDefault().PastValue as double? == 10);
            Assert.IsTrue(differences_BarStartNodePositionZ.FirstOrDefault().FollowingValue as double? == 99);

            var differences_BarStartNodeName = objectDifferences.Differences.Where(d => d.FullName == "BH.oM.Structure.Elements.Bar.StartNode.Name");
            Assert.IsTrue(differences_BarStartNodeName.Count() == 1);
            Assert.IsTrue(differences_BarStartNodeName.FirstOrDefault().DisplayName == "StartNode.Name");
            Assert.IsTrue(differences_BarStartNodeName.FirstOrDefault().PastValue as string == "startNode1");
            Assert.IsTrue(differences_BarStartNodeName.FirstOrDefault().FollowingValue as string == "startNode2");

            var differences_BarName = objectDifferences.Differences.Where(d => d.FullName == "BH.oM.Structure.Elements.Bar.Name");
            Assert.IsTrue(differences_BarName.Count() == 1);
            Assert.IsTrue(differences_BarName.FirstOrDefault().DisplayName == "Name");
            Assert.IsTrue(differences_BarName.FirstOrDefault().PastValue as string == "bar1");
            Assert.IsTrue(differences_BarName.FirstOrDefault().FollowingValue as string == "bar2");
        }

        [TestMethod]
        public void DifferentProperties_DifferentBars()
        {
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

            Assert.IsTrue(differentProperties.Count == 3, "Incorrect number of propertyDifferences found.");

            var differences_BarStartNodePositionZ = differentProperties["BH.oM.Structure.Elements.Bar.StartNode.Position.Z"];
            Assert.IsTrue(differences_BarStartNodePositionZ.Item1 as double? == 10);
            Assert.IsTrue(differences_BarStartNodePositionZ.Item2 as double? == 99);

            var differences_BarStartNodeName = differentProperties["BH.oM.Structure.Elements.Bar.StartNode.Name"];
            Assert.IsTrue(differences_BarStartNodeName.Item1 as string == "startNode1");
            Assert.IsTrue(differences_BarStartNodeName.Item2 as string == "startNode2");

            var differences_BarName = differentProperties["BH.oM.Structure.Elements.Bar.Name"];
            Assert.IsTrue(differences_BarName.Item1 as string == "bar1");
            Assert.IsTrue(differences_BarName.Item2 as string == "bar2");
        }

        [TestMethod]
        public void ObjectDifferences_PropertiesToInclude_FullName_Equals()
        {
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

            Assert.IsTrue(objectDifferences == null || objectDifferences.Differences.Count == 0, "No difference should have been found.");
        }

        [TestMethod]
        public void ObjectDifferences_PropertiesToInclude_PartialName_Equals()
        {
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

            Assert.IsTrue(objectDifferences == null || objectDifferences.Differences.Count == 0, "No difference should have been found.");
        }

        [TestMethod]
        public void ObjectDifferences_PropertiesToInclude_WildCardPrefix_Equals()
        {
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

            Assert.IsTrue(objectDifferences == null || objectDifferences.Differences.Count == 0, "No difference should have been found.");

            // Or equivalently, without any wildcard: `Name`. Result should be the same, we should not find any.
            cc = new ComparisonConfig() { PropertiesToConsider = new List<string>() { "Name" } };
            objectDifferences = BH.Engine.Diffing.Query.ObjectDifferences(bar1, bar2, cc);

            Assert.IsTrue(objectDifferences == null || objectDifferences.Differences.Count == 0, "No difference should have been found.");
        }

        [TestMethod]
        public void ObjectDifferences_PropertiesToInclude_WildCardMiddle_Equals()
        {
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

            Assert.IsTrue(objectDifferences == null || objectDifferences.Differences.Count == 0, "No difference should have been found.");
        }

        [TestMethod]
        public void ObjectDifferences_PropertiesExceptions_Equals()
        {
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
                    Name = "startNode2"  // DIFFERENT `Bar.StartNode.Name`
                },
                EndNode = new Node()
                {
                    Position = new Point() { X = 0, Y = 0, Z = 77 }, // DIFFERENT `Bar.EndNode.Position.Z`
                    Name = "endNode2"  // DIFFERENT `Bar.EndNode.Name`
                },
                Name = "bar2" // DIFFERENT Bar.Name
            };

            // Ignore changes in: Bar.StartNode.Z; Bar.EndNode.Z; Name.
            ComparisonConfig cc = new ComparisonConfig() { PropertyExceptions = { "Bar.*.Position.Z", "Name" } };
            ObjectDifferences objectDifferences = BH.Engine.Diffing.Query.ObjectDifferences(bar1, bar2, cc);

            Assert.IsTrue(objectDifferences == null || objectDifferences.Differences.Count == 0, "No difference should have been found.");
        }

        [TestMethod]
        public void IDiffingTest_DiffWithFragmentId_allDifferentFragments()
        {
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

            Assert.IsTrue(diff.AddedObjects.Count() == 3, "Incorrect number of object identified as new/ToBeCreated.");
            Assert.IsTrue(diff.ModifiedObjects == null || diff.ModifiedObjects.Count() == 0, "Incorrect number of object identified as modified/ToBeUpdated.");
            Assert.IsTrue(diff.RemovedObjects.Count() == 3, "Incorrect number of object identified as old/ToBeDeleted.");
            var objectDifferences = diff.ModifiedObjectsDifferences?.FirstOrDefault();
            Assert.IsTrue(!objectDifferences?.Differences?.Any() ?? true, "Incorrect number of changed properties identified by the property-level diffing.");
        }
    }
}
