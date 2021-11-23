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

namespace BH.Tests.Diffing.Revit
{
    [TestClass]
    public class IDiffingTests 
    {
        [TestMethod]
        public void IDiffing_DiffOneByOne_RandomObjects_SameRevitParameters_IgnorePropertyDifferences_AllSeenEqual()
        {
            // Generate 10 random objects
            int numElements = 10;
            Type revitType = typeof(BH.oM.Adapters.Revit.Elements.ModelInstance);
            List<IBHoMObject> pastObjects = BH.Engine.Diffing.Tests.Create.RandomObjects(revitType, numElements);
            List<IBHoMObject> followingObjs = BH.Engine.Diffing.Tests.Create.RandomObjects(revitType, numElements);

            // Add RevitParameters on them
            pastObjects.ForEach(o => o.SetRevitParameter("testParameter", 10));
            followingObjs.ForEach(o => o.SetRevitParameter("testParameter", 10));

            DiffingConfig dc = new DiffingConfig();
            dc.ComparisonConfig = new RevitComparisonConfig()
            {
                PropertiesToConsider = new List<string>() { "Only consider RevitParameter differences." } // an improbable "PropertyName" in PropertiesToConsider means we will ignore differences that are not in RevitParameters.
            };

            Diff diff = BH.Engine.Diffing.Compute.IDiffing(pastObjects, followingObjs, dc);

            Assert.IsTrue(diff.AddedObjects.Count() == 0, "Incorrect number of object identified as new/ToBeCreated.");
            Assert.IsTrue(diff.ModifiedObjects.Count() == 0, "Incorrect number of object identified as modified/ToBeUpdated.");
            Assert.IsTrue(diff.RemovedObjects.Count() == 0, "Incorrect number of object identified as old/ToBeDeleted.");
            Assert.IsTrue(diff.UnchangedObjects.Count() == 10, "Incorrect number of object identified as unchanged.");
        }
    }
}
