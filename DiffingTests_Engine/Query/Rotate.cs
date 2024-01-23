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
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace BH.Engine.Diffing.Tests
{
    public static partial class Query
    {
        /***************************************************/
        /**** Public Methods                            ****/
        /***************************************************/

        public static IEnumerable<T> Rotate<T>(this IEnumerable<T> ienum, int offset)
        {
            if (offset > 0)
                return ienum.RotateLeft(offset);
            else if (offset < 0)
                return ienum.RotateRight(-offset);

            return ienum;
        }

        /***************************************************/

        public static IEnumerable<T> RotateLeft<T>(this IEnumerable<T> e, int n) =>
            n >= 0 ? e.Skip(n).Concat(e.Take(n)) : e.RotateRight(-n);

        /***************************************************/

        public static IEnumerable<T> RotateRight<T>(this IEnumerable<T> e, int n) =>
            e.Reverse().RotateLeft(n).Reverse();
    }
}