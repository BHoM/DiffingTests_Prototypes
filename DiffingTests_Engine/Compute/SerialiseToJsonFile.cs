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

using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BH.Engine.Diffing.Tests
{
    public static partial class Compute
    {
        public static bool SerialiseToJsonFile(List<object> obj, string filePath)
        {
            if (obj == null)
                return false;

            if (obj.Count() > 1)
                return SerialiseAndWriteJson(obj, filePath);
            else
                return SerialiseAndWriteJson(obj.FirstOrDefault(), filePath);
        }

        private static bool SerialiseAndWriteJson(object obj, string filePath)
        {
            if (!filePath.IsValidFilePath())
                return false;

            try
            {
                JsonSerializerSettings settings = new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.Auto };
                System.IO.File.WriteAllText(filePath, JsonConvert.SerializeObject(obj, settings));
            }
            catch (Exception e)
            {
                BH.Engine.Reflection.Compute.RecordError($"Error serialising or writing to disk:\n\t{e.Message}");
                return false;
            }

            return true;
        }

    }
}

