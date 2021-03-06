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

using BH.Engine.Base;
using BH.oM.Base;
using BH.oM.Diffing;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Text;

namespace BH.Tests.Diffing
{
    [TestClass]
    public class BackendFunctionsTests
    {
        [TestMethod]
        [DataRow(1.2345, 1.1, 1.1)]
        [DataRow(1.2345, 1, 1)]
        [DataRow(0.9999, 1, 0)]
        [DataRow(12, 20, 0)]
        [DataRow(12, 200, 0)]
        [DataRow(121, 2, 120)]
        [DataRow(0.014, 0.01, 0.01)]
        [DataRow(0.014, 0.02, 0)]
        [DataRow(0.016, 0.02, 0)]
        [DataRow(0.02, 0.02, 0.02)]
        [DataRow(0.03, 0.02, 0.02)]
        [DataRow(-0.02, 0.02, -0.02)]
        [DataRow(-0.016, 0.02, 0)]
        [DataRow(-121, 2, -120)]
        [DataRow(-122, 2, -122)]
        public void RoundWithTolerance(double number, double tolerance, double expected)
        {
            var result = BH.Engine.Base.Query.RoundWithTolerance(number, tolerance);
            Assert.IsTrue(result == expected, $"Returned value was {result} instead of {expected}.");
        }

        [TestMethod]
        [DataRow(10, 10, 10)]
        [DataRow(10, 20, 0)]
        [DataRow(30, 20, 20)]
        [DataRow(40, 20, 40)]
        [DataRow(-30, 20, -20)]
        [DataRow(-40, 20, -40)]
        public void RoundWithTolerance_Integers(int number, double tolerance, int expected)
        {
            // This method tests the integer-specific version.
            var result = BH.Engine.Base.Query.RoundWithTolerance(number, tolerance);
            Assert.IsTrue(result == expected, $"Returned value was {result} instead of {expected}.");
        }

        [TestMethod]
        [DataRow(1050.67, 1, 1000)]
        [DataRow(1050.67, 2, 1100)]
        [DataRow(1050.67, 3, 1050)]
        [DataRow(1050.67, 4, 1051)]
        [DataRow(1050.67, 5, 1050.7)]
        [DataRow(123456.123, 7, 123456.1)]
        [DataRow(123456.123, 1, 100000)]
        [DataRow(0.0000000000000000000123456789, 5, 1.2346E-20)]
        [DataRow(0.0000000000000000000123456789, 99, 1.23456789E-20)]
        public void SignificantFigures(double number, int significantFigures, double expected)
        {
            double result = BH.Engine.Base.Query.RoundToSignificantFigures(number, significantFigures);
            Assert.IsTrue(result == expected, $"Returned value was {result} instead of {expected}.");
        }
    }
}

