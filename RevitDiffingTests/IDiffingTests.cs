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

namespace BH.Tests.Diffing.Revit
{
    [TestClass]
    public class IDiffingTests 
    {
        [TestMethod]
        public void IDiffing_DiffOneByOne_RandomObjects_SameRevitParameters_ConsiderOnlyRevitParameters_AllSeenEqual()
        {
            // Test that IDiffing() calls DiffOneByOne,
            // and that the diffing result for some RandomObjects with an identical RevitParameter assigned
            // where we consider only RevitParameters
            // is that all objects are seen as equal.

            // Generate 10 random objects
            int numElements = 10;
            Type revitType = typeof(BH.oM.Adapters.Revit.Elements.ModelInstance);
            List<IBHoMObject> pastObjects = BH.Engine.Diffing.Tests.Create.RandomBHoMObjects(revitType, numElements);
            List<IBHoMObject> followingObjs = BH.Engine.Diffing.Tests.Create.RandomBHoMObjects(revitType, numElements);

            // Add RevitParameters on them
            pastObjects.ForEach(o => o.SetRevitParameter("testParameter", 10));
            followingObjs.ForEach(o => o.SetRevitParameter("testParameter", 10));

            DiffingConfig dc = new DiffingConfig();
            dc.ComparisonConfig = new RevitComparisonConfig()
            {
                PropertiesToConsider = new List<string>() { "Only consider RevitParameter differences." } // an improbable "PropertyName" in PropertiesToConsider means we will ignore differences that are not in RevitParameters.
            };

            // Despite the objects not having an identifier assigned, IDiffing will return a result by comparing each object one by one (it will call DiffOneByOne).
            Diff diff = BH.Engine.Diffing.Compute.IDiffing(pastObjects, followingObjs, dc);

            Assert.IsTrue(diff.AddedObjects.Count() == 0, "Incorrect number of object identified as new/ToBeCreated.");
            Assert.IsTrue(diff.ModifiedObjects.Count() == 0, "Incorrect number of object identified as modified/ToBeUpdated.");
            Assert.IsTrue(diff.RemovedObjects.Count() == 0, "Incorrect number of object identified as old/ToBeDeleted.");
            Assert.IsTrue(diff.UnchangedObjects.Count() == 10, "Incorrect number of object identified as unchanged.");
        }
    }
}

