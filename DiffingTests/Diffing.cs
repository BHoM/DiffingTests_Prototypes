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
    public partial class DiffingTests : BH.oM.Test.NUnit.NUnitTest
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

    }
}



