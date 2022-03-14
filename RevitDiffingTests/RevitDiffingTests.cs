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
using System.Collections.Specialized;

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

            VerifyTotalDifferences(diff, 305, 273, 71);
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

            VerifyTotalDifferences(diff, 45, 13, 21);
        }

        [TestMethod]
        [DataRow(true, true, true, true, 18, 15)]
        [DataRow(false, false, false, false, 6, 3)]
        [DataRow(true, false, false, false, 9, 6)]
        [DataRow(false, true, false, false, 9, 6)]
        [DataRow(false, false, true, false, 9, 6)]
        [DataRow(false, false, false, true, 9, 6)]
        [DataRow(true, true, false, false, 12, 9)]
        [DataRow(true, false, true, false, 12, 9)]
        [DataRow(true, false, false, true, 12, 9)]
        [DataRow(false, true, false, true, 12, 9)]
        [DataRow(false, true, true, false, 12, 9)]
        [DataRow(false, false, true, true, 12, 9)]
        [DataRow(true, false, true, true, 15, 12)]
        [DataRow(true, true, false, true, 15, 12)]
        [DataRow(true, true, true, false, 15, 12)]
        public void RevitPulledParameters_wallCustomParams_ConsiderAssignedParameters(
            bool RevitParams_ConsiderAddedAssigned,
            bool RevitParams_ConsiderAddedUnassigned,
            bool RevitParams_ConsiderRemovedAssigned,
            bool RevitParams_ConsiderRemovedUnassigned,
            int expected_totalDifferences, int expected_revitParameterDifferences)
        {
            BH.oM.Physical.Elements.Wall wall_past = Utilities.GetDataset<BH.oM.Physical.Elements.Wall>("RevitPulledParams_wallCustomParams_past.json");
            BH.oM.Physical.Elements.Wall wall_following = Utilities.GetDataset<BH.oM.Physical.Elements.Wall>("RevitPulledParams_wallCustomParams_following.json"); ;

            RevitComparisonConfig rcc = new RevitComparisonConfig();

            rcc.RevitParams_ConsiderAddedAssigned = RevitParams_ConsiderAddedAssigned;
            rcc.RevitParams_ConsiderAddedUnassigned = RevitParams_ConsiderAddedUnassigned;
            rcc.RevitParams_ConsiderRemovedAssigned = RevitParams_ConsiderRemovedAssigned;
            rcc.RevitParams_ConsiderRemovedUnassigned = RevitParams_ConsiderRemovedUnassigned;
            Diff diff = BH.Engine.Adapters.Revit.Compute.RevitDiffing(new List<object>() { wall_past }, new List<object>() { wall_following }, "UniqueId", rcc);

            VerifyTotalDifferences(diff, expected_totalDifferences, expected_revitParameterDifferences);
        }

        [TestMethod]
        public void RevitPulledParameters_wallCustomParams_ConsiderOnlyParameterDifferences()
        {
            BH.oM.Physical.Elements.Wall wall_past = Utilities.GetDataset<BH.oM.Physical.Elements.Wall>("RevitPulledParams_wallCustomParams_past.json");
            BH.oM.Physical.Elements.Wall wall_following = Utilities.GetDataset<BH.oM.Physical.Elements.Wall>("RevitPulledParams_wallCustomParams_following.json"); ;

            RevitComparisonConfig rcc = BH.Engine.Adapters.Revit.Create.RevitComparisonConfig(null, true);

            Diff diff = BH.Engine.Adapters.Revit.Compute.RevitDiffing(new List<object>() { wall_past }, new List<object>() { wall_following }, "UniqueId", rcc);

            VerifyTotalDifferences(diff, 15, 15);
        }

        public void VerifyTotalDifferences(Diff diff, int totalDifferences, int totalRevitParameterDifferences, int totalModifiedObjectDifferences = 1)
        {
            Assert.IsTrue(diff != null);
            Assert.AreEqual(totalModifiedObjectDifferences, diff.ModifiedObjectsDifferences.Count());

            var allDifferences = diff.ModifiedObjectsDifferences.ToList().SelectMany(d => d.Differences).ToList();
            var revitParameterDifferences = diff.ModifiedObjectsDifferences.SelectMany(d => d.Differences.OfType<RevitParameterDifference>()).ToList();
            Assert.AreEqual(totalDifferences, allDifferences.Count());
            Assert.AreEqual(totalRevitParameterDifferences, revitParameterDifferences.Count());
        }


        [TestMethod]
        public void RevitPulledParameters_Wall()
        {
            BH.oM.Physical.Elements.Wall wall_past = Utilities.GetDataset<BH.oM.Physical.Elements.Wall>("RevitPulledParams_modifiedWall_past.json");
            BH.oM.Physical.Elements.Wall wall_following = Utilities.GetDataset<BH.oM.Physical.Elements.Wall>("RevitPulledParams_modifiedWall_following.json");

            Diff diff = BH.Engine.Adapters.Revit.Compute.RevitDiffing(new List<object>() { wall_past }, new List<object>() { wall_following }, new List<string>(), new List<string>());

            VerifyTotalDifferences(diff, 17, 14);
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

            VerifyTotalDifferences(diff, 10, 10, 6);
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



