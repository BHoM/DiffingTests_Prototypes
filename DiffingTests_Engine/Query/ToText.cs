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

using BH.oM.Diffing;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BH.Test.Engine.Diffing
{
    public static partial class Query
    {
        public static string ToText(this ObjectDifferences objectDifferences) 
        {
            if (objectDifferences == null)
                return "";

            return Newtonsoft.Json.JsonConvert.SerializeObject(
                objectDifferences.Differences
                    .Select(d => $"{d.FullName} went from `{d.PastValue}` to `{d.FollowingValue}`. Description: {d.Description}"), Formatting.Indented);
        }

        public static string ToText(this IEnumerable<ObjectDifferences> objectDifferencesList)
        {
            if (objectDifferencesList == null)
                return "";

            return string.Join(",\n\n", objectDifferencesList.Select(d => d.ToText()));
        }
    }
}
