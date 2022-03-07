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

using BH.oM.Base;
using BH.oM.Diffing;
using BH.oM.Adapters.Revit;
using BH.Engine.Base;
using BH.Engine.Diffing.Tests;
using BH.Engine.Adapters.Revit;
using Microsoft.VisualStudio.TestTools.UnitTesting;
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

namespace BH.Tests.Diffing.Revit
{
    [TestClass]
    [System.Runtime.InteropServices.Guid("43440C9F-5000-4FB2-8858-DD50F1BA9FAF")]
    public class RevitDiffing
    {
        [TestMethod]
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
                pastObjects[i].SetRevitParameter(revitparamter);
                followingObjs[i].SetRevitParameter(revitparamter);
            }

            for (int i = 0; i < followingObjs.Count(); i++)

                // Set progressive identifier on all objects.
                pastObjects.SetProgressiveRevitIdentifier();
            followingObjs.SetProgressiveRevitIdentifier();

            // Check that the objects are seen as different.
            List<string> propertiesToConsider = new List<string>() { "Property1" };
            List<string> parametersToConsider = new List<string>();

            Diff diff = Engine.Adapters.Revit.Compute.RevitDiffing(pastObjects, followingObjs, propertiesToConsider, parametersToConsider);

            Assert.IsTrue(diff.ModifiedObjects.Count() == 1);
        }

        [TestMethod]
        public void TowerMechanicalEquipment()
        {
            List<ModelInstance> pastObjects = GetDataset<List<ModelInstance>>("210917_Tower1_MechanicalEquipment_past.json");
            List<ModelInstance> followingObjects = GetDataset<List<ModelInstance>>("210917_Tower1_MechanicalEquipment_following.json"); ;

            RevitComparisonConfig rcc = null;
            Diff diff = BH.Engine.Adapters.Revit.Compute.RevitDiffing(pastObjects, followingObjects, "UniqueId", rcc);

            Assert.IsTrue(diff != null);
            Assert.IsTrue(diff.ModifiedObjectsDifferences.Count() == 200);
            int totalDifferences = diff.ModifiedObjectsDifferences.ToList().SelectMany(d => d.Differences).Count();
            Assert.IsTrue(totalDifferences == 21128);
        }

        [TestMethod]
        public void TowerMechanicalEquipment_ParametersToConsider()
        {
            List<ModelInstance> pastObjects = GetDataset<List<ModelInstance>>("210917_Tower1_MechanicalEquipment_past.json");
            List<ModelInstance> followingObjects = GetDataset<List<ModelInstance>>("210917_Tower1_MechanicalEquipment_following.json"); ;

            DiffingConfig dc = new DiffingConfig();
            dc.ComparisonConfig = new RevitComparisonConfig()
            {
                ParametersToConsider = new List<string>() { "Dim Operating Weight", "ID Location" }
            };

            Diff diff = BH.Engine.Adapters.Revit.Compute.RevitDiffing(pastObjects, followingObjects, "UniqueId", dc);

            Assert.IsTrue(diff != null);
            int gas = diff.ModifiedObjects.Count();
            Assert.IsTrue(diff.ModifiedObjectsDifferences.Count() == 21);
            Assert.IsTrue(diff.ModifiedObjectsDifferences.ToList()[20].Differences.Count == 4);
        }

        [TestMethod]
        public void Wall_RevitPulledParameters_NotConsiderDeletedAddedParameters()
        {
            BH.oM.Physical.Elements.Wall wall_past = GetDataset<BH.oM.Physical.Elements.Wall>("RevitPulledParams_modifiedWall_past.json");
            BH.oM.Physical.Elements.Wall wall_following = GetDataset<BH.oM.Physical.Elements.Wall>("RevitPulledParams_modifiedWall_following.json"); ;

            RevitComparisonConfig rcc = new RevitComparisonConfig();
            Diff diff = null;
            int totalDifferences = -1;
            int revitParameterDifferencesCount = -1;

            rcc.ConsiderAddedParameters = false;
            rcc.ConsiderRemovedParameters = false;
            diff = BH.Engine.Adapters.Revit.Compute.RevitDiffing(new List<object>() { wall_past }, new List<object>() { wall_following }, "UniqueId", rcc);
            Assert.IsTrue(diff != null);
            Assert.IsTrue(diff.ModifiedObjectsDifferences.Count() == 1);
            totalDifferences = diff.ModifiedObjectsDifferences.ToList().SelectMany(d => d.Differences).Count();
            revitParameterDifferencesCount = diff.ModifiedObjectsDifferences.First().Differences.OfType<RevitParameterDifference>().Count();
            Assert.IsTrue(totalDifferences == 33);
            Assert.IsTrue(revitParameterDifferencesCount == 30);

            rcc.ConsiderAddedParameters = true;
            rcc.ConsiderRemovedParameters = false;
            diff = BH.Engine.Adapters.Revit.Compute.RevitDiffing(new List<object>() { wall_past }, new List<object>() { wall_following }, "UniqueId", rcc);
            Assert.IsTrue(diff != null);
            Assert.IsTrue(diff.ModifiedObjectsDifferences.Count() == 1);
            totalDifferences = diff.ModifiedObjectsDifferences.ToList().SelectMany(d => d.Differences).Count();
            revitParameterDifferencesCount = diff.ModifiedObjectsDifferences.First().Differences.OfType<RevitParameterDifference>().Count();
            Assert.IsTrue(totalDifferences == 43);
            Assert.IsTrue(revitParameterDifferencesCount == 40);

            rcc.ConsiderAddedParameters = false;
            rcc.ConsiderRemovedParameters = true;
            diff = BH.Engine.Adapters.Revit.Compute.RevitDiffing(new List<object>() { wall_past }, new List<object>() { wall_following }, "UniqueId", rcc);
            Assert.IsTrue(diff != null);
            Assert.IsTrue(diff.ModifiedObjectsDifferences.Count() == 1);
            totalDifferences = diff.ModifiedObjectsDifferences.ToList().SelectMany(d => d.Differences).Count();
            revitParameterDifferencesCount = diff.ModifiedObjectsDifferences.First().Differences.OfType<RevitParameterDifference>().Count();
            Assert.IsTrue(totalDifferences == 37);
            Assert.IsTrue(revitParameterDifferencesCount == 34);
        }

        [TestMethod]
        public void Wall_RevitPulledParameters_UnassignedParameters()
        {
            BH.oM.Physical.Elements.Wall wall_past = GetDataset<BH.oM.Physical.Elements.Wall>("RevitPulledParams_modifiedWall_past.json");
            BH.oM.Physical.Elements.Wall wall_following = GetDataset<BH.oM.Physical.Elements.Wall>("RevitPulledParams_modifiedWall_following.json"); ;

            RevitComparisonConfig rcc = new RevitComparisonConfig();
            Diff diff = null;
            int totalDifferences = -1;
            int revitParameterDifferencesCount = -1;

            rcc.ConsiderUnassignedParameters = false;

            rcc.ConsiderAddedParameters = false;
            rcc.ConsiderRemovedParameters = false;
            diff = BH.Engine.Adapters.Revit.Compute.RevitDiffing(new List<object>() { wall_past }, new List<object>() { wall_following }, "UniqueId", rcc);
            Assert.IsTrue(diff != null);
            Assert.IsTrue(diff.ModifiedObjectsDifferences.Count() == 1);
            totalDifferences = diff.ModifiedObjectsDifferences.ToList().SelectMany(d => d.Differences).Count();
            revitParameterDifferencesCount = diff.ModifiedObjectsDifferences.First().Differences.OfType<RevitParameterDifference>().Count();
            Assert.IsTrue(totalDifferences == 33);
            Assert.IsTrue(revitParameterDifferencesCount == 30);


            rcc.ConsiderAddedParameters = true;
            rcc.ConsiderRemovedParameters = false;
            diff = BH.Engine.Adapters.Revit.Compute.RevitDiffing(new List<object>() { wall_past }, new List<object>() { wall_following }, "UniqueId", rcc);
            Assert.IsTrue(diff != null);
            Assert.IsTrue(diff.ModifiedObjectsDifferences.Count() == 1);
            totalDifferences = diff.ModifiedObjectsDifferences.ToList().SelectMany(d => d.Differences).Count();
            revitParameterDifferencesCount = diff.ModifiedObjectsDifferences.First().Differences.OfType<RevitParameterDifference>().Count();
            Assert.IsTrue(totalDifferences == 41);
            Assert.IsTrue(revitParameterDifferencesCount == 38);


            rcc.ConsiderAddedParameters = false;
            rcc.ConsiderRemovedParameters = true;
            diff = BH.Engine.Adapters.Revit.Compute.RevitDiffing(new List<object>() { wall_past }, new List<object>() { wall_following }, "UniqueId", rcc);
            Assert.IsTrue(diff != null);
            Assert.IsTrue(diff.ModifiedObjectsDifferences.Count() == 1);
            totalDifferences = diff.ModifiedObjectsDifferences.ToList().SelectMany(d => d.Differences).Count();
            revitParameterDifferencesCount = diff.ModifiedObjectsDifferences.First().Differences.OfType<RevitParameterDifference>().Count();
            Assert.IsTrue(totalDifferences == 35);
            Assert.IsTrue(revitParameterDifferencesCount == 32);

        }

        [TestMethod]
        public void Wall_RevitPulledParameters()
        {
            BH.oM.Physical.Elements.Wall wall_past = GetDataset<BH.oM.Physical.Elements.Wall>("RevitPulledParams_modifiedWall_past.json");
            BH.oM.Physical.Elements.Wall wall_following = GetDataset<BH.oM.Physical.Elements.Wall>("RevitPulledParams_modifiedWall_following.json"); ;

            Diff diff = BH.Engine.Adapters.Revit.Compute.RevitDiffing(new List<object>() { wall_past }, new List<object>() { wall_following }, new List<string>(), new List<string>());

            int totalDifferences = -1;
            int revitParameterDifferencesCount = -1;

            Assert.IsTrue(diff.ModifiedObjectsDifferences.Count() == 1);

            totalDifferences = diff.ModifiedObjectsDifferences.ToList().SelectMany(d => d.Differences).Count();
            revitParameterDifferencesCount = diff.ModifiedObjectsDifferences.First().Differences.OfType<RevitParameterDifference>().Count();
            Assert.IsTrue(totalDifferences == 47);
            Assert.IsTrue(revitParameterDifferencesCount == 44);

            Assert.IsTrue(diff.ModifiedObjectsDifferences.ToList()[0].Differences.OfType<RevitParameterDifference>().First().Name == "Enable Analytical Model (RevitParameter)");
            Assert.IsTrue((diff.ModifiedObjectsDifferences.ToList()[0].Differences.OfType<RevitParameterDifference>().First().PastValue as bool? ?? false) == false);
            Assert.IsTrue((diff.ModifiedObjectsDifferences.ToList()[0].Differences.OfType<RevitParameterDifference>().First().FollowingValue as bool? ?? false) == true);
        }

        private static T GetDataset<T>(string fileName = "RevitPulledParams_modifiedWall_past.json") where T : class
        {
            var result = BH.Engine.Diffing.Tests.Compute.DeserialiseFromJsonFile<T>(fileName, Path.Combine(System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location), @$"..\..\..\..\Datasets\"));

            return result;
        }

        [TestMethod]
        public void NullChecks_RevitDiffingMethods()
        {
            List<object> pastObjs = null;
            List<object> follObjs = null;
            List<string> propertiesToConsider = null;
            List<string> parametersToConsider = null;

            string id = null;
            DiffingConfig dc = null;
            RevitComparisonConfig cc = null;

            BH.Engine.Adapters.Revit.Compute.RevitDiffing(pastObjs, follObjs, id, dc);
            BH.Engine.Adapters.Revit.Compute.RevitDiffing(pastObjs, follObjs, id, cc);
            BH.Engine.Adapters.Revit.Compute.RevitDiffing(pastObjs, follObjs, parametersToConsider, false);
            BH.Engine.Adapters.Revit.Compute.RevitDiffing(pastObjs, follObjs, propertiesToConsider, parametersToConsider);
        }
    }
}



