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
using System.Security.Cryptography;
using BH.Engine.Diffing;
using System.Diagnostics;
using BH.oM.Diffing;
using BH.Engine.Base;
using System.IO;
using Newtonsoft.Json;
using BH.Engine.Diffing.Tests;
using BH.oM.Diffing.Tests;
using AutoBogus;
using Bogus;
using Shouldly;
using NUnit.Framework;
using BH.Test.Engine.Diffing;

namespace BH.Tests.Diffing
{
    public class DiffingTests
    {
        [Test]
        public void IDiffing_DiffWithHash()
        {
            // Test that the IDiffing() calls the eneric diffing method `DiffWithHash()` when:
            // - the input objects do not have any ID assigned;
            // - the input lists have different length.

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

            // Add the first batch to the second.
            // This means we will have more followingobjects than pastObjects.
            secondBatch.AddRange(firstBatch);

            // Since:
            // - the input objects do not have any ID assigned
            // - the input lists have different length
            // the IDiffing will trigger the most generic diffing method, "DiffWithHash". This method cannot recognize modified objects, so "modifiedObjects" must be null.
            Diff diff = BH.Engine.Diffing.Compute.IDiffing(firstBatch, secondBatch, diffingConfig);

            Assert.IsTrue(diff.AddedObjects.Count() == 3, "Incorrect number of object identified as new/Added.");
            Assert.IsTrue(diff.ModifiedObjects == null || diff.ModifiedObjects.Count() == 0, "Incorrect number of object identified as modified.");
            Assert.IsTrue(diff.RemovedObjects.Count() == 0, "Incorrect number of object identified as old/Removed.");
            var objectDifferences = diff.ModifiedObjectsDifferences?.FirstOrDefault();
            Assert.IsTrue(!objectDifferences?.Differences?.Any() ?? true, "HashDiffing cannot return property Differences, but some were returned.");
        }

        /// <summary>
        /// Tests the resistance of the Diffing() method against null IEnumerables set in the DiffingConfig.ComparisonConfig.
        /// </summary>
        [Test]
        public void Diffing_NullIEnumerablesPropertiesInComparisonConfig()
        {
            // Diffing() is the private method called by DiffWithCustomIds(), DiffWithCustomDataKeyId() and DiffWithFragmentId().

            // Generate some randomObjects and assign the same ID fragment, with a progressive ID, to first/second batch.
            int totalObjectCount = 3;
            List<IBHoMObject> firstBatch = BH.Engine.Diffing.Tests.Create.RandomBHoMObjects(typeof(Bar), totalObjectCount, true, true);
            List<IBHoMObject> secondBatch = BH.Engine.Diffing.Tests.Create.RandomBHoMObjects(typeof(Bar), totalObjectCount, true, true);
            List<string> allObjIds = Enumerable.Range(0, totalObjectCount).Select(i => i.ToString()).ToList();

            DiffingConfig diffingConfig = new DiffingConfig();
            diffingConfig.ComparisonConfig = new ComparisonConfig();

            // Deliberately set the ComparisonConfig options that are IEnumerables to null.
            diffingConfig.ComparisonConfig.CustomdataKeysExceptions = null;
            diffingConfig.ComparisonConfig.CustomdataKeysToConsider = null;
            diffingConfig.ComparisonConfig.PropertiesToConsider = null;
            diffingConfig.ComparisonConfig.PropertyExceptions = null;
            diffingConfig.ComparisonConfig.NamespaceExceptions = null;
            diffingConfig.ComparisonConfig.PropertyNumericTolerances = null;
            diffingConfig.ComparisonConfig.PropertySignificantFigures = null;

            // Compute diffing.
            // This should fail if there is no guard against null IEnumerables.
            Diff diff = BH.Engine.Diffing.Compute.DiffWithCustomIds(firstBatch.OfType<object>().ToList(), allObjIds, secondBatch.OfType<object>().ToList(), allObjIds, diffingConfig);
        }

        [Test]
        public void DiffWithFragmentId_allModifiedObjects()
        {
            // Test that the `DiffWithFragmentId()` correctly identifies two sets of different objects with the same ID as modified.

            // Generate some randomObjects and assign the same ID fragment, with a progressive ID, to first/second batch.
            List<IBHoMObject> firstBatch = BH.Engine.Diffing.Tests.Create.RandomBHoMObjects(typeof(Bar), 3, true, true);
            List<IBHoMObject> secondBatch = BH.Engine.Diffing.Tests.Create.RandomBHoMObjects(typeof(Bar), 3, true, true);

            Diff diff = BH.Engine.Diffing.Compute.DiffWithFragmentId(firstBatch, secondBatch, typeof(TestIdFragment), "Id");

            Assert.IsTrue(diff.AddedObjects.Count() == 0, "Incorrect number of object identified as new/ToBeCreated.");
            Assert.IsTrue(diff.ModifiedObjects.Count() == 3, "Incorrect number of object identified as modified/ToBeUpdated.");
            Assert.IsTrue(diff.RemovedObjects.Count() == 0, "Incorrect number of object identified as old/ToBeDeleted.");
        }

        [Test]
        public void DiffWithFragmentId_allUnchangedObjects()
        {
            DiffingConfig DiffingConfig = new DiffingConfig();

            List<IBHoMObject> firstBatch = BH.Engine.Diffing.Tests.Create.RandomBHoMObjects(typeof(Bar), 3, true, true);
            List<IBHoMObject> secondBatch = new List<IBHoMObject>();
            secondBatch.AddRange(firstBatch);

            Diff diff = BH.Engine.Diffing.Compute.DiffWithFragmentId(firstBatch, secondBatch, typeof(TestIdFragment), "Id");

            Assert.IsTrue(diff.AddedObjects.Count() == 0, "Incorrect number of object identified as new/ToBeCreated.");
            Assert.IsTrue(diff.ModifiedObjects.Count() == 0, "Incorrect number of object identified as modified/ToBeUpdated.");
            Assert.IsTrue(diff.RemovedObjects.Count() == 0, "Incorrect number of object identified as old/ToBeDeleted.");
            Assert.IsTrue(diff.UnchangedObjects.Count() == 3, "Incorrect number of object identified as unchanged.");
        }

        [Test]
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
            Assert.IsTrue(objectDifferences.Differences.Count == 3, $"Incorrect number of propertyDifferences found: {objectDifferences.Differences.Count} instead of 3.");

            var differences_BarStartNodePositionZ = objectDifferences.Differences.Where(d => d.FullName == "BH.oM.Structure.Elements.Bar.StartNode.Position.Z");
            Assert.IsTrue(differences_BarStartNodePositionZ.Count() == 1);
            Assert.IsTrue(differences_BarStartNodePositionZ.FirstOrDefault().Name == "StartNode.Position.Z");
            Assert.IsTrue(differences_BarStartNodePositionZ.FirstOrDefault().PastValue as double? == 10);
            Assert.IsTrue(differences_BarStartNodePositionZ.FirstOrDefault().FollowingValue as double? == 99);

            var differences_BarStartNodeName = objectDifferences.Differences.Where(d => d.FullName == "BH.oM.Structure.Elements.Bar.StartNode.Name");
            Assert.IsTrue(differences_BarStartNodeName.Count() == 1);
            Assert.IsTrue(differences_BarStartNodeName.FirstOrDefault().Name == "StartNode.Name");
            Assert.IsTrue(differences_BarStartNodeName.FirstOrDefault().PastValue as string == "startNode1");
            Assert.IsTrue(differences_BarStartNodeName.FirstOrDefault().FollowingValue as string == "startNode2");

            var differences_BarName = objectDifferences.Differences.Where(d => d.FullName == "BH.oM.Structure.Elements.Bar.Name");
            Assert.IsTrue(differences_BarName.Count() == 1);
            Assert.IsTrue(differences_BarName.FirstOrDefault().Name == "Name");
            Assert.IsTrue(differences_BarName.FirstOrDefault().PastValue as string == "bar1");
            Assert.IsTrue(differences_BarName.FirstOrDefault().FollowingValue as string == "bar2");
        }

        [Test]
        public void ObjectDifferences_DifferentBars_DescriptionNoName()
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

            string descriptionNoName = objectDifferences.Differences.Where(d => !d.Name.Contains("Name")).FirstOrDefault().Description;

            Assert.IsTrue(!descriptionNoName.Contains("with Name"));

            bar1.Name = null;
            bar2.Name = null;
            Assert.IsTrue(!descriptionNoName.Contains("with Name"));
        }

        [Test]
        public void ObjectDifferences_DifferentBars_DescriptionIncludesName()
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
                Name = "bar1" // SAME `Bar.Name`
            };

            ObjectDifferences objectDifferences = BH.Engine.Diffing.Query.ObjectDifferences(bar1, bar2);

            string descriptionWithName = objectDifferences.Differences.Where(d => !d.Name.Contains("Name")).FirstOrDefault().Description;
            Assert.IsTrue(descriptionWithName.Contains("with Name"));
        }

        [Test]
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

        [Test]
        public void ObjectDifferences_PropertiesToConsider_NonExistingPropertyToConsider_Equal()
        {
            Bar bar1 = Engine.Diffing.Tests.Create.RandomObject<Bar>();
            Bar bar2 = Engine.Diffing.Tests.Create.RandomObject<Bar>();

            ComparisonConfig cc = new ComparisonConfig()
            {
                PropertiesToConsider = new List<string>() { "SomeRandomNotExistingPropertyName" }
            };

            ObjectDifferences objectDifferences = BH.Engine.Diffing.Query.ObjectDifferences(bar1, bar2, cc);

            Assert.IsTrue(objectDifferences == null || objectDifferences.Differences.Count == 0, "Objects should be seen as equal.");
        }

        [Test]
        public void ObjectDifferences_PropertiesToConsider_FullName_Equal()
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

            Assert.IsTrue(objectDifferences == null || objectDifferences.Differences.Count == 0, $"No difference should have been found. Differences: {objectDifferences?.ToText()}");
        }

        [Test]
        public void ObjectDifferences_PropertiesToConsider_PartialName_Equal()
        {
            TestObject object1 = new TestObject()
            {
                Location = new TestLocation()
                {
                    Position = new Point() { X = 0, Y = 0, Z = 0 },
                    Name = "someLocation"
                },
                Name = "object1"
            };

            TestObject object2 = new TestObject()
            {
                Location = new TestLocation()
                {
                    Position = ((TestLocation)object1.Location).Position, // SAME `object2.Location.Position.Z`
                    Name = "someLocation" // DIFFERENT `object2.Location.Name`
                },
                Name = "object2" // DIFFERENT `object2.Name`
            };

            // Consider ONLY differences in terms of `Location.Position`. We should not find any.
            ComparisonConfig cc = new ComparisonConfig() { PropertiesToConsider = new List<string>() { "Location.Position" } }; // using "partial property path"
            ObjectDifferences objectDifferences = BH.Engine.Diffing.Query.ObjectDifferences(object1, object2, cc);

            Assert.IsTrue(objectDifferences == null || objectDifferences.Differences.Count == 0, $"No difference should have been found. Differences: {objectDifferences?.ToText()}");
        }

        [Test]
        public void ObjectDifferences_PropertiesToConsider_PartialName_Different()
        {
            TestObject object1 = new TestObject()
            {
                Location = new TestLocation()
                {
                    Position = new Point() { X = 10, Y = 10, Z = 10 },
                    Name = "someLocation1"
                },
                Name = "object1"
            };

            TestObject object2 = new TestObject()
            {
                Location = new TestLocation()
                {
                    Position = new Point() { X = 99, Y = 99, Z = 99 }, // Different `object2.Location`
                    Name = "someLocation2" // Different `object2.Location.Name`
                },
                Name = "object2" // Different `object2.Name`
            };

            // Consider ONLY differences in terms of `Location.Position`
            ComparisonConfig cc = new ComparisonConfig() { PropertiesToConsider = new List<string>() { "Location.Position" } }; // using "partial property path"
            ObjectDifferences objectDifferences = BH.Engine.Diffing.Query.ObjectDifferences(object1, object2, cc);

            Assert.IsTrue(objectDifferences.Differences.Where(d => d.Name.Contains("Location.Position")).Count() == 3, "3 differences in terms of Location.Position should have been found.");
            Assert.IsTrue(objectDifferences.Differences.Count() == 3, "3 differences should have been found in total.");
        }

        [Test]
        public void ObjectDifferences_PropertiesToConsider_WildCardPrefix_Equal()
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

            Assert.IsTrue(objectDifferences == null || objectDifferences.Differences.Count == 0, $"No difference should have been found. Differences: {objectDifferences?.ToText()}");

            // Or equivalently, without any wildcard: `Name`. Result should be the same, we should not find any.
            cc = new ComparisonConfig() { PropertiesToConsider = new List<string>() { "Name" } };
            objectDifferences = BH.Engine.Diffing.Query.ObjectDifferences(bar1, bar2, cc);

            Assert.IsTrue(objectDifferences == null || objectDifferences.Differences.Count == 0, $"No difference should have been found. Differences: {objectDifferences?.ToText()}");
        }

        [Test]
        public void ObjectDifferences_PropertiesToConsider_WildCardMiddle_Equal()
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

            Assert.IsTrue(objectDifferences == null || objectDifferences.Differences.Count == 0, $"No difference should have been found. Differences: {objectDifferences?.ToText()}");
        }

        [Test]
        public void ObjectDifferences_PropertiesExceptions_Equal()
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

            Assert.IsTrue(objectDifferences == null || objectDifferences.Differences.Count == 0, $"No difference should have been found. Differences: {objectDifferences?.ToText()}");
        }

        [Test]
        public void ObjectDifferences_NumericTolerance_Equal()
        {
            // Set a numerical tolerance (different from the default value).
            ComparisonConfig cc = new ComparisonConfig() { NumericTolerance = 0.01 };

            // Create one node.
            Node node1 = new Node();
            node1.Position = new Point() { X = 0, Y = 0, Z = 0 };

            // Create another node with similar coordinates. 
            Node node2 = new Node();
            node2.Position = new Point() { X = 0, Y = 0, Z = 0.0005 };

            // The difference should be so minimal that is ignored by the tolerance.
            ObjectDifferences objectDifferences = BH.Engine.Diffing.Query.ObjectDifferences(node1, node2, cc);

            Assert.IsTrue(objectDifferences == null || objectDifferences.Differences.Count == 0, $"No difference should have been found. Differences: {objectDifferences?.ToText()}");
        }

        [Test]
        public void ObjectDifferences_SignificantFigures_Equal()
        {
            // Set SignificantFigures (different from the default value).
            ComparisonConfig cc = new ComparisonConfig() { SignificantFigures = 3 };

            // Create one node.
            Node node1 = new Node();
            node1.Position = new Point() { X = 0, Y = 0, Z = 123.6 };

            // Create another node with similar coordinates. 
            Node node2 = new Node();
            node2.Position = new Point() { X = 0, Y = 0, Z = 124 };

            // The difference should be so minimal that is ignored by the SignificantFigures.
            ObjectDifferences objectDifferences = BH.Engine.Diffing.Query.ObjectDifferences(node1, node2, cc);

            Assert.IsTrue(objectDifferences == null || objectDifferences.Differences.Count == 0, $"No difference should have been found. Differences: {objectDifferences?.ToText()}");
        }

        [Test]
        public void ObjectDifferences_SignificantFigures_Different()
        {
            ComparisonConfig cc = new ComparisonConfig();

            // Create one node.
            Node node1 = new Node();
            node1.Position = new Point() { X = 0.312, Y = 0.312, Z = 120 };

            // Create another node with similar coordinates. 
            Node node2 = new Node();
            node2.Position = new Point() { X = 0.3123123, Y = 0.3123123, Z = 121.6 };

            // Set significantFigures so that X and Y are rounded to 0.312, while Z is rounded to 122.
            // This means that only Z should be identified as different.
            cc.SignificantFigures = 3;
            ObjectDifferences objectDifferences = BH.Engine.Diffing.Query.ObjectDifferences(node1, node2, cc);

            Assert.IsTrue(objectDifferences == null || objectDifferences.Differences.Count == 1, $"Wrong number of differences identified. Differences: {objectDifferences?.ToText()}");

            // Set significantFigures so that X and Y are rounded to 0.31, while Z is rounded to 120.
            // This means that only X and Y should be identified as different.
            cc.SignificantFigures = 2;
            Assert.IsTrue(objectDifferences == null || objectDifferences.Differences.Count != 2,  $"Wrong number of differences identified. Differences: {objectDifferences?.ToText()}");
        }

        [Test]
        public void ObjectDifferences_PropertySignificantFigures_Equal()
        {
            // Testing property-specific Significant Figures.
            // Set SignificantFigures (different from the default value).
            ComparisonConfig cc = new ComparisonConfig()
            {
                SignificantFigures = 3,
                PropertySignificantFigures = new HashSet<NamedSignificantFigures>() { new NamedSignificantFigures() { Name = "*.Z", SignificantFigures = 1 } }
            };

            // Create one node.
            Node node1 = new Node();
            node1.Position = new Point() { X = 412, Y = 0, Z = 123.6 };

            // Create another node with similar coordinates. 
            Node node2 = new Node();
            node2.Position = new Point() { X = 412, Y = 0, Z = 124 };

            // The difference should be so minimal that is ignored by the SignificantFigures.
            ObjectDifferences objectDifferences = BH.Engine.Diffing.Query.ObjectDifferences(node1, node2, cc);

            Assert.IsTrue(objectDifferences == null || objectDifferences.Differences.Count == 0, $"No difference should have been found. Differences: {objectDifferences?.ToText()}");
        }

        [Test]
        public void ObjectDifferences_PropertyNumericTolerances_Equal()
        {
            // Testing property-specific Significant Figures.
            // Set SignificantFigures (different from the default value).
            ComparisonConfig cc = new ComparisonConfig()
            {
                NumericTolerance = 1E-3,
                PropertyNumericTolerances = new HashSet<NamedNumericTolerance>() { new NamedNumericTolerance() { Name = "*.Z", Tolerance = 1E-1 } }
            };

            // Create one node.
            Node node1 = new Node();
            node1.Position = new Point() { X = 412.0009, Y = 0, Z = 123.16 };

            // Create another node with similar coordinates. 
            Node node2 = new Node();
            node2.Position = new Point() { X = 412.001, Y = 0, Z = 123.2 };

            // The difference should be that 1E-3 is applied to the X, while 1E-1 is applied to the Z. Nodes should be equal.
            ObjectDifferences objectDifferences = BH.Engine.Diffing.Query.ObjectDifferences(node1, node2, cc);

            Assert.IsTrue(objectDifferences == null || objectDifferences.Differences.Count == 0, $"No difference should have been found. Differences: {objectDifferences?.ToText()}");
        }

        [Test]
        public void ObjectDifferences_PropertyNumericTolerances_Different()
        {
            // Testing property-specific Significant Figures.
            // Set SignificantFigures (different from the default value).
            ComparisonConfig cc = new ComparisonConfig()
            {
                NumericTolerance = 1E-1,
                PropertyNumericTolerances = new HashSet<NamedNumericTolerance>() 
                { 
                    new NamedNumericTolerance() { Name = "*.Z", Tolerance = 1E-4 },
                    new NamedNumericTolerance() { Name = "*.Y", Tolerance = 1E-4 },
                }
            };

            // Create one node.
            Node node1 = new Node();
            node1.Position = new Point() { X = 412.09, Y = 0.001, Z = 0.002 };

            // Create another node with similar coordinates. 
            Node node2 = new Node();
            node2.Position = new Point() { X = 412.08, Y = 0.0006, Z = 0.0006 };

            // The differences should be that:
            // - 1E-2 is applied to the X, so it must be ignored;
            // - 1E-4 is applied to the Y, whose second value rounded up should show as different;
            // - 1E-4 is applied to the Z, whose second value rounded up should be ignored.
            ObjectDifferences objectDifferences = BH.Engine.Diffing.Query.ObjectDifferences(node1, node2, cc);

            Assert.IsTrue(objectDifferences != null || objectDifferences.Differences.Count != 1, $"A difference in Y should have been found. Differences: {objectDifferences?.ToText()}");
            objectDifferences.Differences.First().FullName.ShouldEndWith("Position.Y");
        }

        [Test]
        public void IDiffingTest_DiffWithFragmentId_allDifferentFragments_DiffOneByOne()
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

            // This should internally trigger the method "DiffOneByOne". 
            Diff diff = BH.Engine.Diffing.Compute.IDiffing(firstBatch, secondBatch, diffingConfig);

            Assert.IsTrue(diff.AddedObjects.Count() == 0, "Incorrect number of object identified as new/ToBeCreated.");
            Assert.IsTrue(diff.ModifiedObjects != null && diff.ModifiedObjects.Count() != 0, "Incorrect number of object identified as modified/ToBeUpdated.");
            Assert.IsTrue(diff.RemovedObjects.Count() == 0, "Incorrect number of object identified as old/ToBeDeleted.");
            var objectDifferences = diff.ModifiedObjectsDifferences?.FirstOrDefault();
            Assert.IsTrue(objectDifferences?.Differences?.Count() > 0, "Incorrect number of changed properties identified by the property-level diffing.");
        }

        [Test]
        public void ObjectDifferences_CustomObject()
        {
            CustomObject customObj_past = BH.Engine.Diffing.Tests.Create.CustomObject(new Property("Property1", 0));
            CustomObject customObj_following = BH.Engine.Diffing.Tests.Create.CustomObject(new Property("Property1", 99));

            ObjectDifferences objectDifferences = BH.Engine.Diffing.Query.ObjectDifferences(customObj_past, customObj_following);

            Assert.IsTrue(objectDifferences.Differences.Count() == 1);
            Assert.IsTrue(objectDifferences.Differences[0].Name == "Property1");
            Assert.IsTrue(objectDifferences.Differences[0].FullName == "BH.oM.Base.CustomObject.CustomData[Property1]");
            Assert.IsTrue(objectDifferences.Differences[0].PastValue.Equals(0));
            Assert.IsTrue(objectDifferences.Differences[0].FollowingValue.Equals(99));
        }

        [Test]
        public void ObjectDifferences_CustomData()
        {
            TestObject customObj_past = new TestObject();
            customObj_past.CustomData["CustomDataKey1"] = 0;
            TestObject customObj_following = new TestObject();
            customObj_following.CustomData["CustomDataKey1"] = 99;

            ObjectDifferences objectDifferences = BH.Engine.Diffing.Query.ObjectDifferences(customObj_past, customObj_following);

            Assert.IsTrue(objectDifferences.Differences.Count() == 1);
            Assert.IsTrue(objectDifferences.Differences[0].Name == "CustomDataKey1 (CustomData)");
            Assert.IsTrue(objectDifferences.Differences[0].FullName == "BH.oM.Diffing.Tests.TestObject.CustomData[CustomDataKey1]");
            Assert.IsTrue(objectDifferences.Differences[0].PastValue.Equals(0));
            Assert.IsTrue(objectDifferences.Differences[0].FollowingValue.Equals(99));
        }
    }
}


