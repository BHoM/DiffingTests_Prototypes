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

using BH.Engine.Base;
using BH.oM.Analytical.Elements;
using BH.oM.Base;
using BH.oM.Geometry;
using BH.oM.Structure.Elements;
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
        [TestCase]
        public void IsEqual_SameBar()
        {
            Bar sameBar = BH.Engine.Diffing.Tests.Create.RandomObject<Bar>();
            BH.Engine.Diffing.Query.IsEqual(sameBar, sameBar).Should().BeTrue();
        }

        [TestCase]
        public void IsEqual_PropertyExceptions()
        {
            Bar bar1 = BH.Engine.Diffing.Tests.Create.RandomObject<Bar>();
            Bar bar2 = bar1.DeepClone();

            bar2.Name = "newName";
            bar2.FEAType = bar1.FEAType + 1;

            ComparisonConfig comparisonConfig = new() { PropertyExceptions = new() { nameof(Bar.Name), nameof(Bar.FEAType) } };

            BH.Engine.Diffing.Query.IsEqual(bar1, bar2).Should().BeFalse();
            BH.Engine.Diffing.Query.IsEqual(bar1, bar2, comparisonConfig).Should().BeTrue();
        }

        [TestCase]
        public void IsEqual_Geometry_Mesh()
        {
            Mesh mesh1 = BH.Engine.Diffing.Tests.Create.RandomObject<Mesh>();
            BH.Engine.Diffing.Query.IsEqual(mesh1, mesh1).Should().BeTrue();

            Mesh mesh2 = mesh1.DeepClone();
            mesh2 = BH.Engine.Geometry.Modify.Translate(mesh2, new() { X = 1, Y = 1, Z = 1 });
            BH.Engine.Diffing.Query.IsEqual(mesh1, mesh2).Should().BeFalse();

            ComparisonConfig comparisonConfig = new() { PropertyExceptions = new() { nameof(Mesh.Vertices) } };
            BH.Engine.Diffing.Query.IsEqual(mesh1, mesh2, comparisonConfig).Should().BeTrue();
        }

        [TestCase]
        public void IsEqual_Dictionary()
        {
            CustomObject customObject = BH.Engine.Diffing.Tests.Create.RandomObject<CustomObject>();
            Dictionary<string, object> dict = customObject.CustomData;
            BH.Engine.Diffing.Query.IsEqual(dict, dict).Should().BeTrue();
        }

        [TestCase]
        public void IsEqual_ListOfLists()
        {
            var lst1 = new List<List<string>>();
            lst1.Add(new List<string> { "a", "b", "c" });
            BH.Engine.Diffing.Query.IsEqual(lst1, lst1).Should().BeTrue();

            var lst2 = new List<List<string>>();
            lst2.Add(new List<string> { "d", "e", "f" });
            BH.Engine.Diffing.Query.IsEqual(lst1, lst2).Should().BeFalse();
        }
    }
}

