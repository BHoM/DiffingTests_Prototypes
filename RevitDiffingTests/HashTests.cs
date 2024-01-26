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
using BH.oM.Adapters.Revit.Elements;
using NUnit.Framework;

namespace BH.Tests.Diffing.Revit
{
    public class HashTests
    {
        [Test]
        public void RevitParameter_ExtensionMethod_SameHash()
        {
            // Test that the RevitParameter's dedicated HashString() extension method is correctly called when computing its Hash().

            // Create a RevitParameter.
            RevitParameter someParameter = new RevitParameter() { Name = "TestParameter", Value = "SomeValue" };

            // Get the HashString from the RevitParameter's dedicated extension method, and compute its SHA256 Hash (the same algorithm we use for every Hash).
            string hashString = someParameter.HashString();
            string hashFromExtensionMethod = BH.Engine.Diffing.Tests.Query.SHA256Hash(hashString);

            // Get the Hash() from the generic BH.Engine.Base.Hash() method. This must be the same as the `hashFromExtensionMethod` above.
            string hashFromBaseMethod = someParameter.Hash();
            Assert.IsTrue(hashFromBaseMethod == hashFromExtensionMethod, "The Hash should have been the same.");
        }

        [Test]
        public void DifferentObjects_SameRevitParameter_ConsiderEverything_DifferentHashes()
        {
            // Test that different objects with the same RevitParameter all get a different Hash.

            // Generate random objects.
            // Make sure the past/following lists contain completely different objects by setting a name with a progressive number in them (done via the `Create.RandomObjects()` method).
            int numElements = 10;
            Type revitType = typeof(ModelInstance);
            List<IBHoMObject> pastObjects = BH.Engine.Diffing.Tests.Create.RandomBHoMObjects(revitType, numElements, true, true, "pastObject_");
            List<IBHoMObject> followingObjs = BH.Engine.Diffing.Tests.Create.RandomBHoMObjects(revitType, numElements, true, true, "followingObject_");

            // Add a same RevitParameters on all the object.
            // (Having a RevitParameter on the objects ensures that the HashString() extension method for RevitParameter is triggered when computing the Hash()).
            RevitParameter someParameter = new RevitParameter() { Name = "TestParameter", Value = "SomeValue" };

            pastObjects = pastObjects.Select(o => o.SetRevitParameter(someParameter.Name, someParameter.Value)).ToList();
            followingObjs = followingObjs.Select(o => o.SetRevitParameter(someParameter.Name, someParameter.Value)).ToList();

            // Check that the objects are seen as different.
            List<string> pastObjectsHashes = pastObjects.Select(o => o.Hash()).ToList();
            List<string> followingObjectsHashes = followingObjs.Select(o => o.Hash()).ToList();
            Assert.IsTrue(pastObjectsHashes.Intersect(followingObjectsHashes).Count() == 0, "All object hashes should have been different, but they were not.");
        }

        [Test]
        public void DifferentObjects_SameRevitParameter_ConsiderOnlyRevitParameters_SameHash()
        {
            // Test that, by providing a RevitComparisonConfig that asks to only consider RevitParameter differences, different objects with the same RevitParameter all get the same Hash.

            // Generate random objects.
            // Make sure the past/following lists contain completely different objects by setting a name with a progressive number in them (done via the `Create.RandomObjects()` method).
            int numElements = 10;
            Type revitType = typeof(BH.oM.Adapters.Revit.Elements.ModelInstance);
            List<IBHoMObject> pastObjects = BH.Engine.Diffing.Tests.Create.RandomBHoMObjects(revitType, numElements, true, true, "pastObject_");
            List<IBHoMObject> followingObjs = BH.Engine.Diffing.Tests.Create.RandomBHoMObjects(revitType, numElements, true, true, "followingObject_");

            // Add a same RevitParameters on all the object.
            // (Having a RevitParameter on the objects ensures that the HashString() extension method for RevitParameter is triggered when computing the Hash()).
            RevitParameter someParameter = new RevitParameter() { Name = "TestParameter", Value = "SomeValue" };

            pastObjects = pastObjects.Select(o => o.SetRevitParameter(someParameter.Name, someParameter.Value)).ToList();
            followingObjs = followingObjs.Select(o => o.SetRevitParameter(someParameter.Name, someParameter.Value)).ToList();

            // By specifying an unlikely PropertyName in PropertiesToConsider, we can get the differences in terms of RevitParameters only.
            RevitComparisonConfig rcc = new RevitComparisonConfig() { PropertiesToConsider = new() { "Only Revit Parameters" } };

            // Check that the objects are seen as the same.
            List<string> pastObjectsHashes = pastObjects.Select(o => o.Hash(rcc)).ToList();
            List<string> followingObjectsHashes = followingObjs.Select(o => o.Hash(rcc)).ToList();
            Assert.IsTrue(pastObjectsHashes.Distinct().Count() == 1, "All pastObjects hashes should have seen as the same, but they were not.");
            Assert.IsTrue(followingObjectsHashes.Distinct().Count() == 1, "All followingObjs hashes should have seen as the same, but they were not.");
            Assert.IsTrue(pastObjectsHashes.Intersect(followingObjectsHashes).Count() == 1, "All object hashes should have seen as the same, but they were not.");
        }
    }
}



