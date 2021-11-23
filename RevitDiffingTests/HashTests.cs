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

namespace BH.Tests.Diffing.Revit
{
    [TestClass]
    public class HashTests
    {
        [TestMethod]
        public void HashTest_ExtensionMethod_SameObject_SameHash()
        {
            RevitParameter someParameter = new RevitParameter() { Name = "TestParameter", Value = "SomeValue" };

            string hashString = someParameter.HashString();
            string hashFromExtensionMethod = BH.Engine.Diffing.Tests.Query.SHA256Hash(hashString);

            string hashFromBaseMethod = someParameter.Hash();

            Assert.IsTrue(hashFromBaseMethod == hashFromExtensionMethod, "The Hash should have been the same.");
        }


        [TestMethod]
        public void HashTest_DifferentObjects_DifferentHashes()
        {
            // Generate 10 random objects. Make sure the past/following lists contain completely different objects by setting a name in them.
            int numElements = 10;
            Type revitType = typeof(BH.oM.Adapters.Revit.Elements.ModelInstance);
            List<IBHoMObject> pastObjects = BH.Engine.Diffing.Tests.Create.RandomObjects(revitType, numElements, true, true, "pastObject_");
            List<IBHoMObject> followingObjs = BH.Engine.Diffing.Tests.Create.RandomObjects(revitType, numElements, true, true, "followingObject_");

            // Add RevitParameters on them.
            // This will ensure that we will trigger the HashString() extension method for RevitParameters.
            pastObjects = pastObjects.Select(o => o.SetRevitParameter("testParameter", "someValue")).ToList();
            followingObjs = followingObjs.Select(o => o.SetRevitParameter("testParameter", "someValue")).ToList();

            BaseComparisonConfig cc = new RevitComparisonConfig();

            List<string> pastObjectsHashes = pastObjects.Select(o => o.Hash(cc)).ToList();
            List<string> followingObjectsHashes = followingObjs.Select(o => o.Hash(cc)).ToList();

            Assert.IsTrue(pastObjectsHashes.Intersect(followingObjectsHashes).Count() == 0, "All object hashes should have been different, but they were not.");
        }
    }
}
