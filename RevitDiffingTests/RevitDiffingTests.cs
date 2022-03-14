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
using BH.Engine.Reflection;
using BH.oM.Physical.Elements;

namespace BH.Tests.Diffing.Revit
{
    [TestClass]
    [System.Runtime.InteropServices.Guid("43440C9F-5000-4FB2-8858-DD50F1BA9FAF")]
    public class RevitDiffing
    {
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
            List<ModelInstance> pastObjects = Utilities.GetDataset<List<ModelInstance>>("210917_Tower1_MechanicalEquipment_past.json");
            List<ModelInstance> followingObjects = Utilities.GetDataset<List<ModelInstance>>("210917_Tower1_MechanicalEquipment_following.json"); ;

            RevitComparisonConfig rcc = null;
            Diff diff = BH.Engine.Adapters.Revit.Compute.RevitDiffing(pastObjects, followingObjects, "UniqueId", rcc);

            Assert.IsTrue(diff != null);
            Assert.AreEqual(71, diff.ModifiedObjectsDifferences.Count());
            var totalDifferences = diff.ModifiedObjectsDifferences.ToList().SelectMany(d => d.Differences);
            var revitParameterDifferences = diff.ModifiedObjectsDifferences.First().Differences.OfType<RevitParameterDifference>().ToList();
            Assert.AreEqual(305, totalDifferences.Count());
            Assert.AreEqual(35, revitParameterDifferences.Count());
        }

        [TestMethod]
        public void TowerMechanicalEquipment_ParametersToConsider()
        {
            List<ModelInstance> pastObjects = Utilities.GetDataset<List<ModelInstance>>("210917_Tower1_MechanicalEquipment_past.json");
            List<ModelInstance> followingObjects = Utilities.GetDataset<List<ModelInstance>>("210917_Tower1_MechanicalEquipment_following.json"); ;

            DiffingConfig dc = new DiffingConfig();
            dc.ComparisonConfig = new RevitComparisonConfig()
            {
                ParametersToConsider = new List<string>() { "Dim Operating Weight", "ID Location" }
            };

            Diff diff = BH.Engine.Adapters.Revit.Compute.RevitDiffing(pastObjects, followingObjects, "UniqueId", dc);

            Assert.IsTrue(diff != null);
            int gas = diff.ModifiedObjects.Count();
            Assert.AreEqual(21, diff.ModifiedObjectsDifferences.Count());
            Assert.AreEqual(4, diff.ModifiedObjectsDifferences.ToList()[20].Differences.Count);
        }

        [TestMethod]
        public void RevitPulledParameters_wallCustomParams()
        {
            BH.oM.Physical.Elements.Wall wall_past = Utilities.GetDataset<BH.oM.Physical.Elements.Wall>("RevitPulledParams_wallCustomParams_past.json");
            BH.oM.Physical.Elements.Wall wall_following = Utilities.GetDataset<BH.oM.Physical.Elements.Wall>("RevitPulledParams_wallCustomParams_following.json"); ;

            RevitComparisonConfig rcc = new RevitComparisonConfig();
            Diff diff = null;
            List<IPropertyDifference> totalDifferences = null;
            List<RevitParameterDifference> revitParameterDifferences = null;

            // Default diffing, considering all RevitParameter differences
            diff = BH.Engine.Adapters.Revit.Compute.RevitDiffing(new List<object>() { wall_past }, new List<object>() { wall_following }, "UniqueId", rcc);
            Assert.IsTrue(diff != null);
            Assert.AreEqual(1, diff.ModifiedObjectsDifferences.Count());
            totalDifferences = diff.ModifiedObjectsDifferences.ToList().SelectMany(d => d.Differences).ToList();
            revitParameterDifferences = diff.ModifiedObjectsDifferences.First().Differences.OfType<RevitParameterDifference>().ToList();
            Assert.AreEqual(18, totalDifferences.Count());
            Assert.AreEqual(15, revitParameterDifferences.Count());

            int totalAddedAssigned = revitParameterDifferences.OfType(oM.Adapters.Revit.Enums.RevitParameterDifferenceType.AddedAssigned).Count();
            int totalRemovedAssigned = revitParameterDifferences.OfType(oM.Adapters.Revit.Enums.RevitParameterDifferenceType.RemovedAssigned).Count();
            int totalAddedUnassigned = revitParameterDifferences.OfType(oM.Adapters.Revit.Enums.RevitParameterDifferenceType.AddedUnassigned).Count();
            int totalRemovedUnassigned = revitParameterDifferences.OfType(oM.Adapters.Revit.Enums.RevitParameterDifferenceType.RemovedUnassigned).Count();
            int totalModified = revitParameterDifferences.OfType(oM.Adapters.Revit.Enums.RevitParameterDifferenceType.Modified).Count();

            rcc.RevitParams_ConsiderAddedAssigned = true;
            rcc.RevitParams_ConsiderAddedUnassigned = false;
            rcc.RevitParams_ConsiderRemovedAssigned = false;
            rcc.RevitParams_ConsiderRemovedUnassigned = false;
            diff = BH.Engine.Adapters.Revit.Compute.RevitDiffing(new List<object>() { wall_past }, new List<object>() { wall_following }, "UniqueId", rcc);
            Assert.IsTrue(diff != null);
            Assert.AreEqual(1, diff.ModifiedObjectsDifferences.Count());
            totalDifferences = diff.ModifiedObjectsDifferences.ToList().SelectMany(d => d.Differences).ToList();
            revitParameterDifferences = diff.ModifiedObjectsDifferences.First().Differences.OfType<RevitParameterDifference>().ToList();
            Assert.AreEqual(9, totalDifferences.Count());
            Assert.AreEqual(6, revitParameterDifferences.Count());

            rcc.RevitParams_ConsiderAddedAssigned = false;
            rcc.RevitParams_ConsiderAddedUnassigned = true;
            rcc.RevitParams_ConsiderRemovedAssigned = false;
            rcc.RevitParams_ConsiderRemovedUnassigned = false;
            diff = BH.Engine.Adapters.Revit.Compute.RevitDiffing(new List<object>() { wall_past }, new List<object>() { wall_following }, "UniqueId", rcc);
            Assert.IsTrue(diff != null);
            Assert.AreEqual(1, diff.ModifiedObjectsDifferences.Count());
            totalDifferences = diff.ModifiedObjectsDifferences.ToList().SelectMany(d => d.Differences).ToList();
            revitParameterDifferences = diff.ModifiedObjectsDifferences.First().Differences.OfType<RevitParameterDifference>().ToList();
            Assert.AreEqual(9, totalDifferences.Count());
            Assert.AreEqual(6, revitParameterDifferences.Count());

            rcc.RevitParams_ConsiderAddedAssigned = false;
            rcc.RevitParams_ConsiderAddedUnassigned = false;
            rcc.RevitParams_ConsiderRemovedAssigned = true;
            rcc.RevitParams_ConsiderRemovedUnassigned = false;
            diff = BH.Engine.Adapters.Revit.Compute.RevitDiffing(new List<object>() { wall_past }, new List<object>() { wall_following }, "UniqueId", rcc);
            Assert.IsTrue(diff != null);
            Assert.AreEqual(1, diff.ModifiedObjectsDifferences.Count());
            totalDifferences = diff.ModifiedObjectsDifferences.ToList().SelectMany(d => d.Differences).ToList();
            revitParameterDifferences = diff.ModifiedObjectsDifferences.First().Differences.OfType<RevitParameterDifference>().ToList();
            Assert.AreEqual(9, totalDifferences.Count());
            Assert.AreEqual(6, revitParameterDifferences.Count());

            rcc.RevitParams_ConsiderAddedAssigned = false;
            rcc.RevitParams_ConsiderAddedUnassigned = false;
            rcc.RevitParams_ConsiderRemovedAssigned = false;
            rcc.RevitParams_ConsiderRemovedUnassigned = true;
            diff = BH.Engine.Adapters.Revit.Compute.RevitDiffing(new List<object>() { wall_past }, new List<object>() { wall_following }, "UniqueId", rcc);
            Assert.IsTrue(diff != null);
            Assert.AreEqual(1, diff.ModifiedObjectsDifferences.Count());
            totalDifferences = diff.ModifiedObjectsDifferences.ToList().SelectMany(d => d.Differences).ToList();
            revitParameterDifferences = diff.ModifiedObjectsDifferences.First().Differences.OfType<RevitParameterDifference>().ToList();
            Assert.AreEqual(9, totalDifferences.Count());
            Assert.AreEqual(6, revitParameterDifferences.Count());

            rcc.RevitParams_ConsiderAddedAssigned = true;
            rcc.RevitParams_ConsiderAddedUnassigned = true;
            rcc.RevitParams_ConsiderRemovedAssigned = false;
            rcc.RevitParams_ConsiderRemovedUnassigned = false;
            diff = BH.Engine.Adapters.Revit.Compute.RevitDiffing(new List<object>() { wall_past }, new List<object>() { wall_following }, "UniqueId", rcc);
            Assert.IsTrue(diff != null);
            Assert.AreEqual(1, diff.ModifiedObjectsDifferences.Count());
            totalDifferences = diff.ModifiedObjectsDifferences.ToList().SelectMany(d => d.Differences).ToList();
            revitParameterDifferences = diff.ModifiedObjectsDifferences.First().Differences.OfType<RevitParameterDifference>().ToList();
            Assert.AreEqual(12, totalDifferences.Count());
            Assert.AreEqual(9, revitParameterDifferences.Count());
        }

        [TestMethod]
        public void RevitPulledParameters_Wall()
        {
            BH.oM.Physical.Elements.Wall wall_past = Utilities.GetDataset<BH.oM.Physical.Elements.Wall>("RevitPulledParams_modifiedWall_past.json");
            BH.oM.Physical.Elements.Wall wall_following = Utilities.GetDataset<BH.oM.Physical.Elements.Wall>("RevitPulledParams_modifiedWall_following.json");

            Diff diff = BH.Engine.Adapters.Revit.Compute.RevitDiffing(new List<object>() { wall_past }, new List<object>() { wall_following }, new List<string>(), new List<string>());

            int totalDifferences = -1;
            int revitParameterDifferencesCount = -1;

            Assert.AreEqual(1, diff.ModifiedObjectsDifferences.Count());

            totalDifferences = diff.ModifiedObjectsDifferences.ToList().SelectMany(d => d.Differences).Count();
            revitParameterDifferencesCount = diff.ModifiedObjectsDifferences.First().Differences.OfType<RevitParameterDifference>().Count();
            Assert.AreEqual(17, totalDifferences);
            Assert.AreEqual(14, revitParameterDifferencesCount);
        }

        [TestMethod]
        public void RevitPulledParams_BHoMObjsAndModelInstances()
        {
            var pastObjs = Utilities.GetDataset<List<object>>("RevitPulledParams_BHoMObjs-ModelInstances_past.json");
            var followingObjs = Utilities.GetDataset<List<object>>("RevitPulledParams_BHoMObjs-ModelInstances_following.json");

            bool considerOnlyParameterDifferences = true;
            bool considerAddedParameters = true;
            bool considerRemovedParameters = true;
            bool considerUnassignedParameters = true;

            RevitComparisonConfig rcc = BH.Engine.Adapters.Revit.Create.RevitComparisonConfig(null, null, considerOnlyParameterDifferences, considerAddedParameters, considerRemovedParameters, considerUnassignedParameters);
            Diff diff = BH.Engine.Adapters.Revit.Compute.RevitDiffing(pastObjs, followingObjs, rcc);

            int gfa = 1;

        }

        [TestMethod]
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
    }
}



