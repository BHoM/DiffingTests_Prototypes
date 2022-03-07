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

using BH.oM.Diffing.Tests;
using Newtonsoft.Json;
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
    public static partial class Compute
    {
        public static List<object> DeserialiseFromJsonFile(string fileName, string directory)
        {
            object res = DeserialiseFromJsonFile<object>(fileName, directory);
            IEnumerable resList = res as IEnumerable;

            if (resList != null)
                return resList.Cast<object>().ToList();

            return new List<object>() { res };
        }

        public static T DeserialiseFromJsonFile<T>(string fileName, string directory) where T : class
        {
            T result = null;

            string filePath = Path.GetFullPath(Path.Combine(directory, fileName));

            try
            {
                // Set newtonsoft serialization settings to handle automatically any type.
                JsonSerializerSettings settings = new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.Auto };

                using (StreamReader file = File.OpenText(filePath))
                    result = JsonConvert.DeserializeObject<T>(file.ReadToEnd(), settings);
            }
            catch(Exception e)
            {
                Log.RecordError($"Error deserialising or reading from disk:\n\t{e.Message}");
            }

            return result;
        }
    }
}

