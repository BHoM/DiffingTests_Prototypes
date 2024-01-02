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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Utilities
{
    public static partial class Modify
    {
        public static void ConcatenateDictionaryValues<K, L>(this Dictionary<K, List<L>> dict1, Dictionary<K, List<L>> dict2, bool distinct = false)
        {
            foreach (var kv2 in dict2)
            {
                List<L> v1 = null;
                if (dict1.TryGetValue(kv2.Key, out v1))
                {
                    if (v1 != null)
                        dict1[kv2.Key].AddRange(kv2.Value);
                }
                else
                    dict1[kv2.Key] = kv2.Value;
            }

            if (distinct)
            {
                Dictionary<K, List<L>> distinctResult = new Dictionary<K, List<L>>();
                foreach (var kv1 in dict1)
                {
                    distinctResult[kv1.Key] = kv1.Value.Distinct().ToList();
                }

                dict1 = distinctResult;
            }
        }
    }
}



