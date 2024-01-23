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
using NUnit.Framework;
using BH.Test.Engine.Diffing;
using FluentAssertions;

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
            var asd = BH.Engine.Geometry.Query.GeometryHash(default(IGeometry));

            DiffingConfig diffingConfig = new DiffingConfig();

            List<IBHoMObject> firstBatch = new List<IBHoMObject>();
            List<IBHoMObject> secondBatch = new List<IBHoMObject>();

            for (int i = 0; i < 3; i++)
            {
                Bar? obj = BH.Engine.Base.Create.RandomObject(typeof(Bar)) as Bar;
                obj!.Name = "bar_" + i.ToString();
                firstBatch.Add(obj as dynamic);
            }

            for (int i = 0; i < 3; i++)
            {
                Bar? obj = BH.Engine.Base.Create.RandomObject(typeof(Bar)) as Bar;
                obj!.Name = "bar_" + i.ToString();
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

            diff.AddedObjects.Count().Should().Be(3, "Incorrect number of object identified as new/Added.");
            Assert.IsTrue(diff.ModifiedObjects == null || diff.ModifiedObjects.Count() == 0, "Incorrect number of object identified as modified.");
            diff.RemovedObjects.Count().Should().Be(0, "Incorrect number of object identified as old/Removed.");
            var objectDifferences = diff.ModifiedObjectsDifferences?.FirstOrDefault();
            Assert.IsTrue(!objectDifferences?.Differences?.Any() ?? true, "HashDiffing cannot return property Differences, but some were returned.");
        }

        [Test]
        public void DiffWithFragmentId_allModifiedObjects()
        {
            // Test that the `DiffWithFragmentId()` correctly identifies two sets of different objects with the same ID as modified.

            // Generate some randomObjects and assign the same ID fragment, with a progressive ID, to first/second batch.
            List<IBHoMObject> firstBatch = BH.Engine.Diffing.Tests.Create.RandomBHoMObjects(typeof(Bar), 3, true, true);
            List<IBHoMObject> secondBatch = BH.Engine.Diffing.Tests.Create.RandomBHoMObjects(typeof(Bar), 3, true, true);

            Diff diff = BH.Engine.Diffing.Compute.DiffWithFragmentId(firstBatch, secondBatch, typeof(TestIdFragment), "Id");

            diff.AddedObjects.Count().Should().Be(0, "Incorrect number of object identified as new/ToBeCreated.");
            diff.ModifiedObjects.Count().Should().Be(3, "Incorrect number of object identified as modified/ToBeUpdated.");
            diff.RemovedObjects.Count().Should().Be(0, "Incorrect number of object identified as old/ToBeDeleted.");
        }

        [Test]
        public void DiffWithFragmentId_allUnchangedObjects()
        {
            DiffingConfig DiffingConfig = new DiffingConfig();

            List<IBHoMObject> firstBatch = BH.Engine.Diffing.Tests.Create.RandomBHoMObjects(typeof(Bar), 3, true, true);
            List<IBHoMObject> secondBatch = new List<IBHoMObject>();
            secondBatch.AddRange(firstBatch);

            Diff diff = BH.Engine.Diffing.Compute.DiffWithFragmentId(firstBatch, secondBatch, typeof(TestIdFragment), "Id");

            diff.AddedObjects.Count().Should().Be(0, "Incorrect number of object identified as new/ToBeCreated.");
            diff.ModifiedObjects.Count().Should().Be(0, "Incorrect number of object identified as modified/ToBeUpdated.");
            diff.RemovedObjects.Count().Should().Be(0, "Incorrect number of object identified as old/ToBeDeleted.");
            Assert.IsTrue(diff.UnchangedObjects.Count() == 3, "Incorrect number of object identified as unchanged.");
        }

        [Test]
        public void ObjectDifferences_DifferentBars()
        {
            Bar bar1 = new Bar()
            {
                Start = new Node()
                {
                    Position = new Point() { X = 0, Y = 0, Z = 10 },
                    Name = "startNode1"
                },
                Name = "bar1"
            };

            Bar bar2 = new Bar()
            {
                Start = new Node()
                {
                    Position = new Point() { X = 0, Y = 0, Z = 99 }, // Different `Bar.Start.Position.Z`
                    Name = "startNode2"  // Different `Bar.Start.Name`
                },
                Name = "bar2" // Different `Bar.Name`
            };

            ObjectDifferences objectDifferences = BH.Engine.Diffing.Query.ObjectDifferences(bar1, bar2);

            objectDifferences.FollowingObject.Should().Be(bar2);
            objectDifferences.PastObject.Should().Be(bar1);
            objectDifferences.Differences.Count.Should().Be(3, "the number of propertyDifferences should be 3");

            var differences_BarStartPositionZ = objectDifferences.Differences.Where(d => d.FullName == $"{typeof(Bar).FullName}.Start.Position.Z");
            differences_BarStartPositionZ.Count().Should().Be(1);
            differences_BarStartPositionZ.FirstOrDefault()?.Name.Should().Be("Start.Position.Z");
            (differences_BarStartPositionZ.FirstOrDefault()?.PastValue as double?).Should().Be(10);
            (differences_BarStartPositionZ.FirstOrDefault()?.FollowingValue as double?).Should().Be(99);

            var differences_BarStartName = objectDifferences.Differences.Where(d => d.FullName == $"{typeof(Bar).FullName}.Start.Name");
            differences_BarStartName.Count().Should().Be(1);
            differences_BarStartName.FirstOrDefault()?.Name.Should().Be("Start.Name");
            (differences_BarStartName.FirstOrDefault()?.PastValue as string).Should().Be("startNode1");
            (differences_BarStartName.FirstOrDefault()?.FollowingValue as string).Should().Be("startNode2");

            var differences_BarName = objectDifferences.Differences.Where(d => d.FullName == $"{typeof(Bar).FullName}.Name");
            differences_BarName.Count().Should().Be(1);
            differences_BarName.FirstOrDefault()?.Name.Should().Be("Name");
            (differences_BarName.FirstOrDefault()?.PastValue as string).Should().Be("bar1");
            (differences_BarName.FirstOrDefault()?.FollowingValue as string).Should().Be("bar2");
        }

        [Test]
        public void ObjectDifferences_DifferentBars_DescriptionNoName()
        {
            Bar bar1 = new Bar()
            {
                Start = new Node()
                {
                    Position = new Point() { X = 0, Y = 0, Z = 10 },
                    Name = "startNode1"
                },
                Name = "bar1"
            };

            Bar bar2 = new Bar()
            {
                Start = new Node()
                {
                    Position = new Point() { X = 0, Y = 0, Z = 99 }, // Different `Bar.Start.Position.Z`
                    Name = "startNode2"  // Different `Bar.Start.Name`
                },
                Name = "bar2" // Different `Bar.Name`
            };

            ObjectDifferences objectDifferences = BH.Engine.Diffing.Query.ObjectDifferences(bar1, bar2);

            string? descriptionNoName = objectDifferences.Differences.Where(d => !d.Name.Contains("Name")).FirstOrDefault()?.Description;

            descriptionNoName.Should().NotContain("with Name");

            bar1.Name = null;
            bar2.Name = null;
            descriptionNoName.Should().NotContain("with Name");
        }

        [Test]
        public void ObjectDifferences_DifferentBars_DescriptionIncludesName()
        {
            Bar bar1 = new Bar()
            {
                Start = new Node()
                {
                    Position = new Point() { X = 0, Y = 0, Z = 10 },
                    Name = "startNode1"
                },
                Name = "bar1"
            };

            Bar bar2 = new Bar()
            {
                Start = new Node()
                {
                    Position = new Point() { X = 0, Y = 0, Z = 99 }, // Different `Bar.Start.Position.Z`
                    Name = "startNode2"  // Different `Bar.Start.Name`
                },
                Name = "bar1" // SAME `Bar.Name`
            };

            ObjectDifferences objectDifferences = BH.Engine.Diffing.Query.ObjectDifferences(bar1, bar2);

            string? descriptionWithName = objectDifferences.Differences.Where(d => !d.Name.Contains("Name")).FirstOrDefault()?.Description;
            descriptionWithName.Should().Contain("with Name");
        }

        [Test]
        public void DifferentProperties_DifferentBars()
        {
            Bar bar1 = new Bar()
            {
                Start = new Node()
                {
                    Position = new Point() { X = 0, Y = 0, Z = 10 },
                    Name = "startNode1"
                },
                Name = "bar1"
            };

            Bar bar2 = new Bar()
            {
                Start = new Node()
                {
                    Position = new Point() { X = 0, Y = 0, Z = 99 }, // Different `Bar.Start.Position.Z`
                    Name = "startNode2"  // Different `Bar.Start.Name`(differences_BarName.Item2 as string).Should().Be(
                },
                Name = "bar2" // Different `Bar.Name`
            };

            Dictionary<string, Tuple<object, object>> differentProperties = BH.Engine.Diffing.Query.DifferentProperties(bar1, bar2);

            Assert.IsTrue(differentProperties.Count == 3, "Incorrect number of propertyDifferences found.");

            var differences_BarStartPositionZ = differentProperties[$"{typeof(BH.oM.Structure.Elements.Bar).FullName}.Start.Position.Z"];
            Assert.IsTrue(differences_BarStartPositionZ.Item1 as double? == 10);
            Assert.IsTrue(differences_BarStartPositionZ.Item2 as double? == 99);

            var differences_BarStartName = differentProperties[$"{typeof(BH.oM.Structure.Elements.Bar).FullName}.Start.Name"];
            Assert.IsTrue(differences_BarStartName.Item1 as string == "startNode1");
            Assert.IsTrue(differences_BarStartName.Item2 as string == "startNode2");

            var differences_BarName = differentProperties[$"{typeof(BH.oM.Structure.Elements.Bar).FullName}.Name"];
            (differences_BarName.Item1 as string).Should().Be("bar1");
            (differences_BarName.Item2 as string).Should().Be("bar2");
        }

        [Test]
        public void ObjectDifferences_PropertiesToConsider_NonExistingPropertyToConsider_Equal()
        {
            Bar bar1 = Engine.Diffing.Tests.Create.RandomObject<Bar>();
            Bar bar2 = Engine.Diffing.Tests.Create.RandomObject<Bar>();

            ComparisonConfig cc = new ComparisonConfig(
                propertiesToConsider: new HashSet<string>() { "SomeRandomNotExistingPropertyName" }
            );

            ObjectDifferences objectDifferences = BH.Engine.Diffing.Query.ObjectDifferences(bar1, bar2, cc);

            Assert.IsTrue(objectDifferences == null || objectDifferences.Differences.Count == 0, "Objects should be seen as equal.");
        }

        [Test]
        public void ObjectDifferences_PropertiesToConsider_FullName_Equal()
        {
            Bar bar1 = new Bar()
            {
                Start = new Node()
                {
                    Position = new Point() { X = 0, Y = 0, Z = 10 },
                    Name = "startNode1"
                },
                Name = "bar1"
            };

            Bar bar2 = new Bar()
            {
                Start = new Node()
                {
                    Position = new Point() { X = 0, Y = 0, Z = 99 }, // Different `Bar.Start.Position.Z`
                    Name = bar1.Start.Name  // SAME `Bar.Start.Name`
                },
                Name = "bar2" // Different `Bar.Name`
            };

            // Consider ONLY differences in terms of `BH.oM.Structure.Elements.Bar.Start.Name`. We should not find any.
            ComparisonConfig cc = new ComparisonConfig(propertiesToConsider: new HashSet<string>() { $"{typeof(BH.oM.Structure.Elements.Bar).FullName}.Start.Name" });
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
            ComparisonConfig cc = new ComparisonConfig(propertiesToConsider: new HashSet<string>() { "Location.Position" }); // using "partial property path"
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
            ComparisonConfig cc = new ComparisonConfig(propertiesToConsider: new HashSet<string>() { "Location.Position" }); // using "partial property path"
            ObjectDifferences objectDifferences = BH.Engine.Diffing.Query.ObjectDifferences(object1, object2, cc);

            Assert.IsTrue(objectDifferences.Differences.Where(d => d.Name.Contains("Location.Position")).Count() == 3, "3 differences in terms of Location.Position should have been found.");
            Assert.IsTrue(objectDifferences.Differences.Count() == 3, "3 differences should have been found in total.");
        }

        [Test]
        public void ObjectDifferences_PropertiesToConsider_WildCardPrefix_Equal()
        {
            Bar bar1 = new Bar()
            {
                Start = new Node()
                {
                    Position = new Point() { X = 0, Y = 0, Z = 10 },
                    Name = "startNode1"
                },
                Name = "bar1"
            };

            Bar bar2 = new Bar()
            {
                Start = new Node()
                {
                    Position = new Point() { X = 0, Y = 0, Z = 99 }, // Different `Bar.Start.Position.Z`
                    Name = bar1.Start.Name  // SAME `Bar.Start.Name`
                },
                Name = bar1.Name // SAME `Bar.Name`
            };

            // Consider ONLY differences in terms of names: `*.Name`.  We should not find any.
            ComparisonConfig cc = new ComparisonConfig(propertiesToConsider: new HashSet<string>() { "*.Name" });
            ObjectDifferences objectDifferences = BH.Engine.Diffing.Query.ObjectDifferences(bar1, bar2, cc);

            Assert.IsTrue(objectDifferences == null || objectDifferences.Differences.Count == 0, $"No difference should have been found. Differences: {objectDifferences?.ToText()}");

            // Or equivalently, without any wildcard: `Name`. Result should be the same, we should not find any.
            cc = new ComparisonConfig(propertiesToConsider: new HashSet<string>() { "Name" });
            objectDifferences = BH.Engine.Diffing.Query.ObjectDifferences(bar1, bar2, cc);

            Assert.IsTrue(objectDifferences == null || objectDifferences.Differences.Count == 0, $"No difference should have been found. Differences: {objectDifferences?.ToText()}");
        }

        [Test]
        public void ObjectDifferences_PropertiesToConsider_WildCardMiddle_Equal()
        {
            Bar bar1 = new Bar()
            {
                Start = new Node()
                {
                    Position = new Point() { X = 0, Y = 0, Z = 0 },
                    Name = "startNode1"
                },
                End = new Node()
                {
                    Position = new Point() { X = 0, Y = 0, Z = 99 },
                    Name = "endNode1"
                },
                Name = "bar1"
            };

            Bar bar2 = new Bar()
            {
                Start = new Node()
                {
                    Position = new Point() { X = 0, Y = 0, Z = 55 }, // DIFFERENT `Bar.Start.Position.Z`
                    Name = bar1.Start.Name  // SAME `Bar.Start.Name`
                },
                End = new Node()
                {
                    Position = new Point() { X = 0, Y = 0, Z = 77 }, // DIFFERENT `Bar.End.Position.Z`
                    Name = bar1.End.Name  // SAME `Bar.End.Name`
                },
                Name = "bar2" // DIFFERENT Bar.Name
            };

            // Consider ONLY differences in terms of Start AND End names: `Bar.*.Name`. We should not find any.
            ComparisonConfig cc = new ComparisonConfig(propertiesToConsider: new HashSet<string>() { "Bar.*.Name" });
            ObjectDifferences objectDifferences = BH.Engine.Diffing.Query.ObjectDifferences(bar1, bar2, cc);

            Assert.IsTrue(objectDifferences == null || objectDifferences.Differences.Count == 0, $"No difference should have been found. Differences: {objectDifferences?.ToText()}");
        }

        [Test]
        public void ObjectDifferences_PropertiesExceptions_Equal()
        {
            Bar bar1 = new Bar()
            {
                Start = new Node()
                {
                    Position = new Point() { X = 0, Y = 0, Z = 0 },
                    Name = "startNode1"
                },
                End = new Node()
                {
                    Position = new Point() { X = 0, Y = 0, Z = 99 },
                    Name = "endNode1"
                },
                Name = "bar1"
            };

            Bar bar2 = new Bar()
            {
                Start = new Node()
                {
                    Position = new Point() { X = 0, Y = 0, Z = 55 }, // DIFFERENT `Bar.Start.Position.Z`
                    Name = "startNode2"  // DIFFERENT `Bar.Start.Name`
                },
                End = new Node()
                {
                    Position = new Point() { X = 0, Y = 0, Z = 77 }, // DIFFERENT `Bar.End.Position.Z`
                    Name = "endNode2"  // DIFFERENT `Bar.End.Name`
                },
                Name = "bar2" // DIFFERENT Bar.Name
            };

            // Ignore changes in: Bar.Start.Z; Bar.End.Z; Name.
            ComparisonConfig cc = new ComparisonConfig() { PropertyExceptions = { "Bar.*.Position.Z", "Name" } };
            ObjectDifferences objectDifferences = BH.Engine.Diffing.Query.ObjectDifferences(bar1, bar2, cc);

            Assert.IsTrue(objectDifferences == null || objectDifferences.Differences.Count == 0, $"No difference should have been found. Differences: {objectDifferences?.ToText()}");
        }

        [Test]
        public void ObjectDifferences_NumericTolerance_Equal()
        {
            // Set a numerical tolerance (different from the default value).
            ComparisonConfig cc = new ComparisonConfig(numericTolerance: 0.01);

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
            ComparisonConfig cc = new ComparisonConfig(significantFigures: 3);

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
            cc = new ComparisonConfig(significantFigures: 3);
            ObjectDifferences objectDifferences = BH.Engine.Diffing.Query.ObjectDifferences(node1, node2, cc);

            Assert.IsTrue(objectDifferences == null || objectDifferences.Differences.Count == 1, $"Wrong number of differences identified. Differences: {objectDifferences?.ToText()}");

            // Set significantFigures so that X and Y are rounded to 0.31, while Z is rounded to 120.
            // This means that only X and Y should be identified as different.
            cc = new ComparisonConfig(significantFigures: 2);
            Assert.IsTrue(objectDifferences == null || objectDifferences.Differences.Count != 2, $"Wrong number of differences identified. Differences: {objectDifferences?.ToText()}");
        }

        [Test]
        public void ObjectDifferences_PropertySignificantFigures_Equal()
        {
            // Testing property-specific Significant Figures.
            // Set SignificantFigures (different from the default value).
            ComparisonConfig cc = new ComparisonConfig(
                significantFigures: 3,
                propertySignificantFigures: new HashSet<NamedSignificantFigures>() { new NamedSignificantFigures() { Name = "*.Z", SignificantFigures = 1 } }
            );

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
            ComparisonConfig cc = new ComparisonConfig(
                numericTolerance: 1E-3,
                propertyNumericTolerances: new HashSet<NamedNumericTolerance>() { new NamedNumericTolerance() { Name = "*.Z", Tolerance = 1E-1 } }
            );

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
            ComparisonConfig cc = new ComparisonConfig(
                numericTolerance: 1E-2,
                propertyNumericTolerances: new HashSet<NamedNumericTolerance>()
                {
                    new NamedNumericTolerance() { Name = "*.Y", Tolerance = 1E-3 },
                    new NamedNumericTolerance() { Name = "*.Z", Tolerance = 1E-3 },
                }
            );

            // Create one node.
            Node node1 = new Node();
            node1.Position = new Point() { X = 412.09, Y = 0.001, Z = 0.002 };

            // Create another node with similar coordinates. 
            Node node2 = new Node();
            node2.Position = new Point() { X = 412.08, Y = 0.0006, Z = 0.0006 };

            // The differences should be so that, by applying the rule from BH.Engine.Diffing.Query.NumericalDifferenceInclusion():
            // - the difference in X (0.01) is <= 1E-2, so it must be ignored;
            // - the difference in Y (0.0004) is <= 1E-3, so it must be ignored;
            // - the difference in Z (0.0016) is NOT <= 1E-3, so it must be considered as a difference.
            ObjectDifferences objectDifferences = BH.Engine.Diffing.Query.ObjectDifferences(node1, node2, cc);

            objectDifferences.Should().NotBeNull();
            objectDifferences.Differences.Count.Should().Be(1, $"a single difference should have been found");
            objectDifferences.Differences.First().FullName.Should().EndWith("Position.Z");
        }

        [Test]
        public void IDiffingTest_DiffWithFragmentId_allDifferentFragments_DiffOneByOne()
        {
            DiffingConfig diffingConfig = new DiffingConfig();

            List<IBHoMObject> firstBatch = new List<IBHoMObject>();
            List<IBHoMObject> secondBatch = new List<IBHoMObject>();

            for (int i = 0; i < 3; i++)
            {
                Bar? obj = BH.Engine.Base.Create.RandomObject(typeof(Bar)) as Bar;
                TestIdFragment testIdFragment = new TestIdFragment() { Id = i };
                RandomNumberFragment randomNumberFragment = new RandomNumberFragment();

                obj!.Name = "bar_" + i.ToString();
                obj.AddFragment(testIdFragment);
                obj.AddFragment(randomNumberFragment);


                firstBatch.Add(obj as dynamic);
            }

            for (int i = 0; i < 3; i++)
            {
                Bar? obj = BH.Engine.Base.Create.RandomObject(typeof(Bar)) as Bar;
                TestIdFragment testIdFragment = new TestIdFragment() { Id = i };
                RandomNumberFragment randomNumberFragment = new RandomNumberFragment();

                obj!.Name = "bar_" + i.ToString();
                obj.AddFragment(testIdFragment);
                obj.AddFragment(randomNumberFragment);

                secondBatch.Add(obj as dynamic);
            }

            // This should internally trigger the method "DiffOneByOne". 
            Diff diff = BH.Engine.Diffing.Compute.IDiffing(firstBatch, secondBatch, diffingConfig);

            diff.AddedObjects.Count().Should().Be(0, "Incorrect number of object identified as new/ToBeCreated.");
            Assert.IsTrue(diff.ModifiedObjects != null && diff.ModifiedObjects.Count() != 0, "Incorrect number of object identified as modified/ToBeUpdated.");
            diff.RemovedObjects.Count().Should().Be(0, "Incorrect number of object identified as old/ToBeDeleted.");
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



