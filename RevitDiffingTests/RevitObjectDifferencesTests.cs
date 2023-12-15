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
using FluentAssertions;

namespace BH.Tests.Diffing.Revit
{
    public class RevitObjectDifferencesTests
    {
        [Test]
        public void ParameterNumericTolerances_Equal()
        {
            // Testing property-specific Significant Figures.
            // Set SignificantFigures (different from the default value).
            RevitComparisonConfig cc = new RevitComparisonConfig()
            {
                NumericTolerance = 1E-3,
                ParameterNumericTolerances = new HashSet<NamedNumericTolerance>() { new NamedNumericTolerance() { Name = "*.Z", Tolerance = 1E-1 } }
            };

            // Create one bhomobject.
            IBHoMObject bhomobj1 = new BHoMObject();
            bhomobj1 = bhomobj1.SetRevitParameter(new RevitParameter() { Name = "Somename.X", Value = 412.0009 });
            bhomobj1 = bhomobj1.SetRevitParameter(new RevitParameter() { Name = "Somename.Z", Value = 123.16 });
            bhomobj1 = bhomobj1.SetRevitParameter(new RevitParameter() { Name = "Somename.Y", Value = 0 });

            // Create another node with similar coordinates. 
            IBHoMObject bhomobj2 = new BHoMObject();
            bhomobj2 = bhomobj2.SetRevitParameter(new RevitParameter() { Name = "Somename.X", Value = 412.001 });
            bhomobj2 = bhomobj2.SetRevitParameter(new RevitParameter() { Name = "Somename.Y", Value = 0 });
            bhomobj2 = bhomobj2.SetRevitParameter(new RevitParameter() { Name = "Somename.Z", Value = 123.2 });

            // The difference should be that 1E-3 is applied to the X, while 1E-1 is applied to the Z. Objs should be equal.
            var diff = BH.Engine.Adapters.Revit.Compute.RevitDiffing(
                new List<IBHoMObject>() { bhomobj1 }.SetProgressiveRevitIdentifier(),
                new List<IBHoMObject>() { bhomobj2 }.SetProgressiveRevitIdentifier(),
                cc);

            ObjectDifferences objectDifferences = diff?.ModifiedObjectsDifferences?.FirstOrDefault();
            Assert.IsTrue(objectDifferences == null || objectDifferences.Differences.Count == 0, $"No difference should have been found. Differences: {objectDifferences?.ToText()}");
        }

        [Test]
        public void ParameterNumericTolerances_Different()
        {
            // Testing property-specific Significant Figures.
            // Set SignificantFigures (different from the default value).
            RevitComparisonConfig cc = new RevitComparisonConfig()
            {
                NumericTolerance = 1E-2,
                ParameterNumericTolerances = new HashSet<NamedNumericTolerance>()
                {
                    new NamedNumericTolerance() { Name = "*.Y", Tolerance = 1E-3 },
                    new NamedNumericTolerance() { Name = "*.Z", Tolerance = 1E-3 },
                }
            };

            // Create one bhomobject.
            IBHoMObject bhomobj1 = new BHoMObject();
            bhomobj1 = bhomobj1.SetRevitParameter(new RevitParameter() { Name = "Somename.X", Value = 412.09 });
            bhomobj1 = bhomobj1.SetRevitParameter(new RevitParameter() { Name = "Somename.Y", Value = 0.001 });
            bhomobj1 = bhomobj1.SetRevitParameter(new RevitParameter() { Name = "Somename.Z", Value = 0.002 });

            // Create another node with similar coordinates. 
            IBHoMObject bhomobj2 = new BHoMObject();
            bhomobj2 = bhomobj2.SetRevitParameter(new RevitParameter() { Name = "Somename.X", Value = 412.08 });
            bhomobj2 = bhomobj2.SetRevitParameter(new RevitParameter() { Name = "Somename.Y", Value = 0.0004 });
            bhomobj2 = bhomobj2.SetRevitParameter(new RevitParameter() { Name = "Somename.Z", Value = 0.0004 });

            // The differences should be that:
            // - the difference in X (0.01) is <= 1E-2, so it must be ignored;
            // - the difference in Y (0.0006) is <= 1E-3, so it must be ignored;
            // - the difference in Z (0.0016) is NOT <= 1E-3, so it must be considered as a difference.
            var diff = BH.Engine.Adapters.Revit.Compute.RevitDiffing(
                new List<IBHoMObject>() { bhomobj1 }.SetProgressiveRevitIdentifier(),
                new List<IBHoMObject>() { bhomobj2 }.SetProgressiveRevitIdentifier(),
                cc);

            ObjectDifferences objectDifferences = diff?.ModifiedObjectsDifferences?.FirstOrDefault()!;
            objectDifferences?.Differences.Should().ContainSingle($"A single difference should have been found");
            objectDifferences?.Differences.First().Name.Should().Contain("Somename.Z", "A single difference in Z value should have been found");
        }

        [TestCase(true)]
        [TestCase(false)]
        public void ParametersToConsider(bool directRevitDiff)
        {
            // Create one bhomobject with some Revit Parameters called X, Y and Z.
            IBHoMObject bhomobj1 = new BHoMObject();
            bhomobj1 = bhomobj1.SetRevitParameter(new RevitParameter() { Name = "Somename.X", Value = 1 });
            bhomobj1 = bhomobj1.SetRevitParameter(new RevitParameter() { Name = "Somename.Y", Value = 2 });
            bhomobj1 = bhomobj1.SetRevitParameter(new RevitParameter() { Name = "Somename.Z", Value = 3 });

            // Create another node with the same X value but different Y and Z
            IBHoMObject bhomobj2 = new BHoMObject();
            bhomobj2 = bhomobj2.SetRevitParameter(new RevitParameter() { Name = "Somename.X", Value = 1 });
            bhomobj2 = bhomobj2.SetRevitParameter(new RevitParameter() { Name = "Somename.Y", Value = 999 });
            bhomobj2 = bhomobj2.SetRevitParameter(new RevitParameter() { Name = "Somename.Z", Value = 999 });

            // Make sure that the difference between the objects can be found, with no option set.
            ObjectDifferences objectDifferences = BH.Engine.Diffing.Query.ObjectDifferences(bhomobj1, bhomobj2);
            Diff diff = null;
            if (directRevitDiff)
            {
                diff = BH.Engine.Adapters.Revit.Compute.RevitDiffing(
                new List<IBHoMObject>() { bhomobj1 }.SetProgressiveRevitIdentifier(),
                new List<IBHoMObject>() { bhomobj2 }.SetProgressiveRevitIdentifier(), null);
            }
            else
            {
                diff = BH.Engine.Diffing.Compute.IDiffing(
                    new List<IBHoMObject>() { bhomobj1 }.SetProgressiveRevitIdentifier(),
                    new List<IBHoMObject>() { bhomobj2 }.SetProgressiveRevitIdentifier(), new DiffingConfig());
            }

            objectDifferences = diff?.ModifiedObjectsDifferences?.FirstOrDefault()!;
            objectDifferences?.Differences.Count.Should().Be(2, $"two differences should have been found");
            var diffNames = objectDifferences?.Differences.Select(d => d.Name);
            (diffNames!.Any(d => d.Contains("Somename.Y")) && diffNames!.Any(d => d.Contains("Somename.Z")))
                .Should().BeTrue("The differences should include both a difference in Y and Z.");

            // Set ParametersToConsider so that only the X Parameter is to be considered,
            // and verify that no difference is then found.
            RevitComparisonConfig cc = new RevitComparisonConfig()
            {
                ParametersToConsider = new List<string>() { "Somename.X" }
            };

            if (directRevitDiff)
            {
                diff = BH.Engine.Adapters.Revit.Compute.RevitDiffing(
                new List<IBHoMObject>() { bhomobj1 }.SetProgressiveRevitIdentifier(),
                new List<IBHoMObject>() { bhomobj2 }.SetProgressiveRevitIdentifier(), cc);
            }
            else
            {
                diff = BH.Engine.Diffing.Compute.IDiffing(
                    new List<IBHoMObject>() { bhomobj1 }.SetProgressiveRevitIdentifier(),
                    new List<IBHoMObject>() { bhomobj2 }.SetProgressiveRevitIdentifier(),
                    new DiffingConfig() { ComparisonConfig = cc });
            }

            objectDifferences = diff?.ModifiedObjectsDifferences?.FirstOrDefault()!;
            objectDifferences.Should().BeNull($"No difference should have been found after {nameof(RevitComparisonConfig)}.{nameof(RevitComparisonConfig.ParametersToConsider)} was set.");
        }


        [TestCase(true)]
        [TestCase(false)]
        public void ParametersExceptions(bool directRevitDiff)
        {
            // Create one bhomobject with some Revit Parameters called X, Y and Z.
            IBHoMObject bhomobj1 = new BHoMObject();
            bhomobj1 = bhomobj1.SetRevitParameter(new RevitParameter() { Name = "Somename.X", Value = 1 });
            bhomobj1 = bhomobj1.SetRevitParameter(new RevitParameter() { Name = "Somename.Y", Value = 2 });
            bhomobj1 = bhomobj1.SetRevitParameter(new RevitParameter() { Name = "Somename.Z", Value = 3 });

            // Create another node with the same X value but different Y and Z
            IBHoMObject bhomobj2 = new BHoMObject();
            bhomobj2 = bhomobj2.SetRevitParameter(new RevitParameter() { Name = "Somename.X", Value = 1 });
            bhomobj2 = bhomobj2.SetRevitParameter(new RevitParameter() { Name = "Somename.Y", Value = 999 });
            bhomobj2 = bhomobj2.SetRevitParameter(new RevitParameter() { Name = "Somename.Z", Value = 999 });

            // Make sure that the difference between the objects can be found, with no option set.
            ObjectDifferences objectDifferences = BH.Engine.Diffing.Query.ObjectDifferences(bhomobj1, bhomobj2);
            var diff = BH.Engine.Adapters.Revit.Compute.RevitDiffing(
                new List<IBHoMObject>() { bhomobj1 }.SetProgressiveRevitIdentifier(),
                new List<IBHoMObject>() { bhomobj2 }.SetProgressiveRevitIdentifier(), null);

            objectDifferences = diff?.ModifiedObjectsDifferences?.FirstOrDefault()!;
            objectDifferences?.Differences.Count.Should().Be(2, $"two differences should have been found");
            var diffNames = objectDifferences?.Differences.Select(d => d.Name);
            (diffNames!.Any(d => d.Contains("Somename.Y")) && diffNames!.Any(d => d.Contains("Somename.Z")))
                .Should().BeTrue("The differences should include both a difference in Y and Z.");

            // Set ParametersExceptions so that Y and Z parameters are to be ignored,
            // and verify that no difference is then found.
            RevitComparisonConfig cc = new RevitComparisonConfig()
            {
                ParametersExceptions = new List<string>() { "Somename.Y", "Somename.Z" }
            };

            if (directRevitDiff)
            {
                diff = BH.Engine.Adapters.Revit.Compute.RevitDiffing(
                new List<IBHoMObject>() { bhomobj1 }.SetProgressiveRevitIdentifier(),
                new List<IBHoMObject>() { bhomobj2 }.SetProgressiveRevitIdentifier(), cc);
            }
            else
            {
                diff = BH.Engine.Diffing.Compute.IDiffing(
                    new List<IBHoMObject>() { bhomobj1 }.SetProgressiveRevitIdentifier(),
                    new List<IBHoMObject>() { bhomobj2 }.SetProgressiveRevitIdentifier(),
                    new DiffingConfig() { ComparisonConfig = cc });
            }

            objectDifferences = diff?.ModifiedObjectsDifferences?.FirstOrDefault()!;
            objectDifferences.Should().BeNull($"No difference should have been found after {nameof(RevitComparisonConfig)}.{nameof(RevitComparisonConfig.ParametersExceptions)} was set.");
        }
    }
}