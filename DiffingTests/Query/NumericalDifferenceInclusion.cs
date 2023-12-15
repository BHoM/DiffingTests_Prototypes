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

using BH.Engine.Base;
using BH.oM.Base;
using FluentAssertions;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BH.Test.Engine.Diffing
{
    public partial class Query
    {
        // Test parameters:
        // seenAsDifferent, number1, number2, tolerance
        [TestCase(true, 1.5, 2, double.MinValue)]
        [TestCase(false, 1.5, 2, 2)]
        [TestCase(false, 1.5, 2, 0.5)]
        [TestCase(true, 1.5, 2, 0.49)]
        [TestCase(false, 1.5, 2, 0.51)]
        public void NumericalDifferenceInclusion_Tolerance(bool seenAsDifferent, double number1, double number2, double tolerance)
        {
            var comparisonConfig = new ComparisonConfig()
            {
                NumericTolerance = tolerance,
            };

            BH.Engine.Diffing.Query.NumericalDifferenceInclusion(number1, number2, comparisonConfig: comparisonConfig).Should().Be(seenAsDifferent);
        }

        // Test parameters:
        // seenAsDifferent, number1, number2, significantFigures
        [TestCase(false, 1.5, 2, 0)]
        [TestCase(false, 1.5, 2, 1)]
        [TestCase(true, 1.5, 2, 2)]
        public void NumericalDifferenceInclusion_SignificantFigures(bool seenAsDifferent, double number1, double number2, int significantFigures)
        {
            var comparisonConfig = new ComparisonConfig()
            {
                SignificantFigures = significantFigures,
            };

            BH.Engine.Diffing.Query.NumericalDifferenceInclusion(number1, number2, comparisonConfig: comparisonConfig).Should().Be(seenAsDifferent);
        }
    }
}
