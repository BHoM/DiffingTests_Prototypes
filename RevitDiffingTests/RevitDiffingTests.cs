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
using BH.oM.Diffing.Test;
using System.IO;
using Newtonsoft.Json;
using BH.oM.Adapters.Revit.Elements;

namespace BH.Tests.Diffing.Revit
{
    [TestClass]
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
        public void SerialisedRevitObjects()
        {
            // Test that different objects with the same RevitParameter all get a different Hash.
            string filePath_pastObjects = Path.GetFullPath(Path.Combine(System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location), @"..\..\..\..\Datasets\210917_Tower1_MechanicalEquipment_past.json"));
            string filePath_followingObjects = Path.GetFullPath(Path.Combine(System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location), @"..\..\..\..\Datasets\210917_Tower1_MechanicalEquipment_following.json"));

            Assert.IsTrue(filePath_pastObjects.IsValidFilePath(), $"Check that the filepath for the serialised object is valid and that the file can be found on disk: {filePath_pastObjects}.");
            Assert.IsTrue(filePath_followingObjects.IsValidFilePath(), $"Check that the filepath for the serialised object is valid and that the file can be found on disk: {filePath_followingObjects}.");

            // Set newtonsoft serialization settings to handle automatically any type.
            JsonSerializerSettings settings = new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.Auto };

            List<ModelInstance> pastObjects;
            List<ModelInstance> followingObjects;

            using (StreamReader file = File.OpenText(filePath_pastObjects))
                pastObjects = JsonConvert.DeserializeObject<List<ModelInstance>>(file.ReadToEnd(), settings);

            using (StreamReader file = File.OpenText(filePath_followingObjects))
                followingObjects = JsonConvert.DeserializeObject<List<ModelInstance>>(file.ReadToEnd(), settings);

            DiffingConfig dc = new DiffingConfig();
            dc.ComparisonConfig = new RevitComparisonConfig()
            {
                ParametersToConsider = new List<string>() { "Dim Operating Weight", "ID Location" }
            };

            Diff diff = BH.Engine.Diffing.Compute.IDiffing(pastObjects, followingObjects, dc);

        }

    }
}
