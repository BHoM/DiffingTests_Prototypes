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

using BH.oM.Base;
using BH.oM.Diffing;
using BH.oM.Adapters.Revit;
using BH.Engine.Base;
using BH.Engine.Diffing.Tests;
using BH.Engine.Adapters.Revit;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Text;
using System.Linq;
using BH.oM.Adapters.Revit.Parameters;
using BH.oM.Diffing.Tests;
using System.IO;
using Newtonsoft.Json;
using BH.oM.Adapters.Revit.Elements;
using BH.Engine.Reflection;
using BH.oM.Physical.Elements;
using System.Collections.Specialized;
using NUnit.Framework;
using BH.Test.Engine.Diffing;
using FluentAssertions;

namespace BH.Tests.Diffing.Revit
{
    public class RevitDiffing
    {
        [Test]
        public void NullChecks_RevitDiffingMethods()
        {
            List<object> pastObjs = null!;
            List<object> follObjs = null!;

            string id = null!;
            DiffingConfig dc = null!;
            RevitComparisonConfig cc = null!;

            Assert.DoesNotThrow(() =>
            {
                BH.Engine.Adapters.Revit.Compute.RevitDiffing(pastObjs, follObjs, id, dc);
                BH.Engine.Adapters.Revit.Compute.RevitDiffing(pastObjs, follObjs, id, cc);
                BH.Engine.Adapters.Revit.Compute.RevitDiffing(pastObjs, follObjs, null!, false);
                BH.Engine.Adapters.Revit.Compute.RevitDiffing(pastObjs, follObjs, default(List<string>), null!);
            });
        }

        [Test]
        public void CustomObjects_PropertiesToConsider_And_ParametersToConsider()
        {
            // Test that different objects with the same RevitParameter all get a different Hash.

            // Generate random objects.
            // Make sure the past/following lists contain completely different objects by setting a name with a progressive number in them (done via the `Create.RandomObjects()` method).
            List<CustomObject> pastObjects = new List<CustomObject>() {
                BH.Engine.Diffing.Tests.Create.CustomObject(new Property("Property1", 1)),
                BH.Engine.Diffing.Tests.Create.CustomObject(new Property("Property2", 2)),
                BH.Engine.Diffing.Tests.Create.CustomObject(new Property("Property3", 3)),
            };

            List<CustomObject> followingObjs = new List<CustomObject>() {
                BH.Engine.Diffing.Tests.Create.CustomObject(new Property("Property1", 10)),
                BH.Engine.Diffing.Tests.Create.CustomObject(new Property("Property2", 20)),
                BH.Engine.Diffing.Tests.Create.CustomObject(new Property("Property3", 30)),
            };

            // Add the same RevitParameters on all the objects.
            for (int i = 0; i < pastObjects.Count(); i++)
            {
                RevitParameter revitparamter = new RevitParameter() { Name = $"RevitParameter{i}", Value = i };
                pastObjects[i] = (CustomObject)pastObjects[i].SetRevitParameter(revitparamter);
                followingObjs[i] = (CustomObject)followingObjs[i].SetRevitParameter(revitparamter);
            }

            // Set progressive identifier on all objects.
            pastObjects.SetProgressiveRevitIdentifier();
            followingObjs.SetProgressiveRevitIdentifier();

            // Check that the objects are seen as different.
            HashSet<string> propertiesToConsider = new() { "Property1" };
            HashSet<string> parametersToConsider = new();

            Diff diff = Engine.Adapters.Revit.Compute.RevitDiffing(pastObjects, followingObjs, propertiesToConsider, parametersToConsider);

            diff.ModifiedObjects.Count().Should().Be(1);
        }

        public void VerifyTotalDifferences(Diff diff, int totalDifferences, int totalRevitParameterDifferences, int totalModifiedObjectDifferences = 1)
        {
            diff.Should().NotBeNull();
            diff.ModifiedObjectsDifferences.Count().Should().Be(totalModifiedObjectDifferences);

            var allDifferences = diff.ModifiedObjectsDifferences.ToList().SelectMany(d => d.Differences).ToList();
            var revitParameterDifferences = diff.ModifiedObjectsDifferences.SelectMany(d => d.Differences.OfType<RevitParameterDifference>()).ToList();

            allDifferences.Count().Should().Be(totalDifferences, $"Total differences found: {allDifferences.Count()} instead of expected {totalDifferences}. Differences: {diff.ModifiedObjectsDifferences.ToText()}");
            revitParameterDifferences.Count().Should().Be(totalRevitParameterDifferences, $"Revit param differences found: {revitParameterDifferences.Count()} instead of expected {totalRevitParameterDifferences}. Differences: {diff.ModifiedObjectsDifferences.ToText()}");
        }

        [Test]
        public void PropertyFullNameValueGroups_RevitParameter()
        {
            CustomObject customObject = new CustomObject();
            customObject = (CustomObject)customObject.AddFragment(new RevitPulledParameters(new List<RevitParameter>() { new RevitParameter() { Name = "TestParam", Value = "SomeValue" } }));

            //var result_NonGenerics = customObject.PropertyFullNameValueGroups(typeof(RevitParameter)); Not currently working, cast issue.
            var result_Generics = customObject.PropertyFullNameValueGroups<RevitParameter>();

            //Assert.IsTrue(result_NonGenerics.Count == 1);
            Assert.IsTrue(result_Generics.Count == 1);

            // Let's add an object in a RevitParameter, and this object will have a nested RevitParameter.
            customObject = new CustomObject();
            Column columnNestedInParameter = new Column();
            columnNestedInParameter = (Column)columnNestedInParameter.AddFragment(new RevitPulledParameters(new List<RevitParameter>() { new RevitParameter() { Name = "SomeNestedParameter", Value = "SomeNestedValue" } }));
            RevitParameter nestedParameter = new RevitParameter() { Name = "NestedParameter", Value = columnNestedInParameter };
            RevitParameter parentParameter = new RevitParameter() { Name = "TestParam", Value = columnNestedInParameter };

            customObject = (CustomObject)customObject.AddFragment(new RevitPulledParameters(new List<RevitParameter>() { parentParameter }));

            //result_NonGenerics = customObject.PropertyFullNameValueGroups(typeof(RevitParameter));
            result_Generics = customObject.PropertyFullNameValueGroups<RevitParameter>();
            //Assert.IsTrue(result_NonGenerics.Count == 2);
            Assert.AreEqual(2, result_Generics.Count);
        }

        [Test]
        public void RevitIdsDuplicates_StructuralObjs()
        {
            var pastObjs = Utilities.GetDataset<List<object>>("RevitDiffing-duplicateIdsStructuralObjs_past.json");
            var followingObjs = Utilities.GetDataset<List<object>>("RevitDiffing-duplicateIdsStructuralObjs_following.json");

            IEnumerable<RevitIdentifiers> followingIdFragments = followingObjs.OfType<IBHoMObject>().Select(obj => obj.GetRevitIdentifiers()).Where(x => x != null);
            var followingIds = followingIdFragments.Select(x => x.PersistentId.ToString()).ToList();
            followingIds.Count.Should().NotBe(followingIds.Distinct().Count(), "the input followingObjects must have duplicate Ids for this test");

            Diff diff = BH.Engine.Adapters.Revit.Compute.RevitDiffing(pastObjs, followingObjs, new RevitComparisonConfig());
            diff.Should().BeNull("input followingObjects have duplicate ids so diff should be null.");
        }
    }
}
