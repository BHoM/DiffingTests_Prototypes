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
        [Test]
        [TestCase(1.2345, 1.1, 1.1)]
        [TestCase(1.2345, 1, 1)]
        [TestCase(0.9999, 1, 0)]
        [TestCase(12, 20, 0)]
        [TestCase(12, 200, 0)]
        [TestCase(121, 2, 120)]
        [TestCase(0.014, 0.01, 0.01)]
        [TestCase(0.014, 0.02, 0)]
        [TestCase(0.016, 0.02, 0)]
        [TestCase(0.02, 0.02, 0.02)]
        [TestCase(0.03, 0.02, 0.02)]
        [TestCase(-0.02, 0.02, -0.02)]
        [TestCase(-0.016, 0.02, 0)]
        [TestCase(-121, 2, -120)]
        [TestCase(-122, 2, -122)]
        public void RoundWithTolerance(double number, double tolerance, double expected)
        {
            var result = BH.Engine.Base.Query.RoundWithTolerance(number, tolerance);
            Assert.IsTrue(result == expected, $"Returned value was {result} instead of {expected}.");
        }

        [Test]
        [TestCase(10, 10, 10)]
        [TestCase(10, 20, 0)]
        [TestCase(30, 20, 20)]
        [TestCase(40, 20, 40)]
        [TestCase(-30, 20, -20)]
        [TestCase(-40, 20, -40)]
        public void RoundWithTolerance_Integers(int number, double tolerance, int expected)
        {
            // This method tests the integer-specific version.
            var result = BH.Engine.Base.Query.RoundWithTolerance(number, tolerance);
            Assert.IsTrue(result == expected, $"Returned value was {result} instead of {expected}.");
        }
    }
}
