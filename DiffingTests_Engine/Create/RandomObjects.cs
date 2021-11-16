/*
 * This file is part of the Buildings and Habitats object Model (BHoM)
 * Copyright (c) 2015 - 2020, the respective contributors. All rights reserved.
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
using BH.oM.Diffing.Test;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BH.Engine.Diffing.Tests
{
    public static partial class Create
    {
        public static List<T> RandomObjects<T>(int count = 100, bool assignIdFragment = false, bool setName = false, string namePrefix = "bar_") where T : IObject
        {
            return RandomObjects(typeof(T), count, assignIdFragment, setName, namePrefix).OfType<T>().ToList();
        }

        public static List<IBHoMObject> RandomObjects(Type t, int count = 100, bool assignIdFragment = false, bool setName = false, string namePrefix = "bar_")
        {
            List<IBHoMObject> objs = new List<IBHoMObject>();

            for (int i = 0; i < count; i++)
            {
                IObject obj = BH.Engine.Base.Create.RandomObject(t);

                if (assignIdFragment)
                {
                    IBHoMObject bhomObj = obj as IBHoMObject;
                    TestIdFragment testIdFragment = new TestIdFragment() { Id = i };
                    bhomObj = bhomObj.AddFragment(testIdFragment);
                    obj = bhomObj;
                }

                if (setName)
                {
                    IBHoMObject bhomObj = obj as IBHoMObject;
                    bhomObj.Name = namePrefix + i.ToString();
                    obj = bhomObj;
                }

                objs.Add(obj as dynamic);
            }

            return objs;
        }
    }
}
