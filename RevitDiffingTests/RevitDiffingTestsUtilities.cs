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
using System.IO;
using System.Collections;
using Newtonsoft.Json.Linq;
using System.ComponentModel;

namespace BH.Tests.Diffing.Revit
{
    public static class Utilities
    {
        public static IBHoMObject SetRevitParameter(this IBHoMObject bhomobject, RevitParameter revitParameter)
        {
            return bhomobject.SetRevitParameter(revitParameter.Name, revitParameter.Value);
        }

        [Description("Adds a RevitIdentifier Fragment on the object. " +
            "The RevitIdentifier `PersistentId` and the `ElementId` property get populated with a progressive number starting from 0. E.g. 0,1,2,...")]
        public static List<T> SetProgressiveRevitIdentifier<T>(this List<T> bhomobjects) where T : IBHoMObject
        {
            int i = 0;
            bhomobjects.ForEach(obj =>
            {
                obj.Fragments.AddOrReplace(new RevitIdentifiers(i.ToString(), i));
                i++;
            });

            return bhomobjects;
        }

        public static T GetDataset<T>(string fileName = "RevitPulledParams_modifiedWall_past.json") where T : class
        {
            string datasetDirectory = Path.Combine(System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location), @$"..\..\..\..\Datasets\");
            try
            {
                var newtonSoftResult = BH.Engine.Diffing.Tests.Compute.DeserialiseFromJsonFile<T>(fileName, datasetDirectory);

                IList resList = newtonSoftResult as IList;
                if (resList == null || resList.OfType<JObject>().Count() != resList.Count)
                    return newtonSoftResult;
            }
            catch (Exception e)
            {
                string es = e.ToString();
            }
            
            // Try FileAdapter
            string fullPath = Path.Combine(datasetDirectory, fileName);
            var res = BH.Engine.Adapters.File.Compute.ReadFromJsonFile(fullPath, true);
            T result = res as T;
            return result;
        }
    }
}



