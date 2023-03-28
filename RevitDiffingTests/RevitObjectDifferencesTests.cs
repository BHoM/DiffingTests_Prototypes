/*
 * This file is part of the Buildings and Habitats object Model (BHoM)
 * Copyright (c) 2015 - 2023, the respective contributors. All rights reserved.
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
using Shouldly;
using BH.Test.Engine.Diffing;

namespace BH.Tests.Diffing.Revit
{
    public class RevitObjectDifferencesTests
    {
        [Test]
        public void PropertyNumericTolerances_Equal()
        {
            // Testing property-specific Significant Figures.
            // Set SignificantFigures (different from the default value).
            RevitComparisonConfig cc = new RevitComparisonConfig()
            {
                NumericTolerance = 1E-3,
                ParameterNumericTolerances = new HashSet<NamedNumericTolerance>() { new NamedNumericTolerance() { Name = "*.Z", Tolerance = 1E-1 } }
            };

            // Create one bhomobject.
            BHoMObject bhomobj1 = new BHoMObject();
            bhomobj1 = (BHoMObject)bhomobj1.SetRevitParameter(new RevitParameter() { Name = "Somename.X", Value = 412.0009 });
            bhomobj1 = (BHoMObject)bhomobj1.SetRevitParameter(new RevitParameter() { Name = "Somename.Z", Value = 123.16 });
            bhomobj1 = (BHoMObject)bhomobj1.SetRevitParameter(new RevitParameter() { Name = "Somename.Y", Value = 0 });

            // Create another node with similar coordinates. 
            BHoMObject bhomobj2 = new BHoMObject();
            bhomobj2 = (BHoMObject)bhomobj2.SetRevitParameter(new RevitParameter() { Name = "Somename.X", Value = 412.001 });
            bhomobj2 = (BHoMObject)bhomobj2.SetRevitParameter(new RevitParameter() { Name = "Somename.Y", Value = 0 });
            bhomobj2 = (BHoMObject)bhomobj2.SetRevitParameter(new RevitParameter() { Name = "Somename.Z", Value = 123.2 });

            // The difference should be that 1E-3 is applied to the X, while 1E-1 is applied to the Z. Objs should be equal.
            ObjectDifferences objectDifferences = BH.Engine.Diffing.Query.ObjectDifferences(bhomobj1, bhomobj2, cc);
            var diff = BH.Engine.Adapters.Revit.Compute.RevitDiffing(
                new List<BHoMObject>() { bhomobj1 }.SetProgressiveRevitIdentifier(), 
                new List<BHoMObject>() { bhomobj2 }.SetProgressiveRevitIdentifier(), 
                cc);
            
            objectDifferences = diff?.ModifiedObjectsDifferences?.FirstOrDefault();
            Assert.IsTrue(objectDifferences == null || objectDifferences.Differences.Count == 0, $"No difference should have been found. Differences: {objectDifferences?.ToText()}");
        }

        [Test]
        public void PropertyNumericTolerances_Different()
        {
            // Testing property-specific Significant Figures.
            // Set SignificantFigures (different from the default value).
            RevitComparisonConfig cc = new RevitComparisonConfig()
            {
                NumericTolerance = 1E-1,
                ParameterNumericTolerances = new HashSet<NamedNumericTolerance>()
                {
                    new NamedNumericTolerance() { Name = "*.Z", Tolerance = 1E-4 },
                    new NamedNumericTolerance() { Name = "*.Y", Tolerance = 1E-4 },
                }
            };

            // Create one bhomobject.
            BHoMObject bhomobj1 = new BHoMObject();
            bhomobj1.SetRevitParameter(new RevitParameter() { Name = "Somename.X", Value = 412.09 });
            bhomobj1.SetRevitParameter(new RevitParameter() { Name = "Somename.Y", Value = 0.0001 });
            bhomobj1.SetRevitParameter(new RevitParameter() { Name = "Somename.Z", Value = 0.0002 });

            // Create another node with similar coordinates. 
            BHoMObject bhomobj2 = new BHoMObject();
            bhomobj2.SetRevitParameter(new RevitParameter() { Name = "Somename.X", Value = 412.08 });
            bhomobj2.SetRevitParameter(new RevitParameter() { Name = "Somename.Y", Value = 0.00016 });
            bhomobj2.SetRevitParameter(new RevitParameter() { Name = "Somename.Z", Value = 0.00016 });

            // The differences should be that:
            // - 1E-2 is applied to the X, so it must be ignored;
            // - 1E-4 is applied to the Y, whose second value rounded up should show as different;
            // - 1E-4 is applied to the Z, whose second value rounded up should be ignored.
            ObjectDifferences objectDifferences = BH.Engine.Diffing.Query.ObjectDifferences(bhomobj1, bhomobj2, cc);
            var diff = BH.Engine.Adapters.Revit.Compute.RevitDiffing(
                new List<BHoMObject>() { bhomobj1 }.SetProgressiveRevitIdentifier(),
                new List<BHoMObject>() { bhomobj2 }.SetProgressiveRevitIdentifier(),
                cc);

            objectDifferences = diff?.ModifiedObjectsDifferences?.FirstOrDefault();
            Assert.IsTrue(objectDifferences != null && objectDifferences.Differences.Count != 1, $"A difference in Y should have been found. Differences: {objectDifferences?.ToText()}");
            objectDifferences.Differences.First().FullName.ShouldEndWith("Position.Y");
        }
    }
}




