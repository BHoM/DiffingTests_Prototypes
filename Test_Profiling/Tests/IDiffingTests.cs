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

using BH.oM.Structure.Elements;
using BH.oM.Geometry;
using BH.Engine;
using BH.oM.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;
using BH.Engine.Diffing;
using System.Diagnostics;
using BH.oM.Diffing;
using BH.Engine.Base;
using System.IO;
using Newtonsoft.Json;


namespace Tests
{
    internal static partial class DiffingTests
    {
        public static void IDiffingTest_HashDiffing(bool logging = false)
        {
            string testName = MethodBase.GetCurrentMethod().Name;

            Console.WriteLine($"\nRunning {testName}");
            Stopwatch sw = Stopwatch.StartNew();

            DiffingConfig DiffingConfig = new DiffingConfig();

            List<IBHoMObject> firstBatch = new List<IBHoMObject>();
            List<IBHoMObject> secondBatch = new List<IBHoMObject>();

            for (int i = 0; i < 3; i++)
            {
                Bar obj = BH.Engine.Base.Create.RandomObject(typeof(Bar)) as Bar;
                obj.Name = "bar_" + i.ToString();
                firstBatch.Add(obj as dynamic);
            }

            for (int i = 0; i < 3; i++)
            {
                Bar obj = BH.Engine.Base.Create.RandomObject(typeof(Bar)) as Bar;
                obj.Name = "bar_" + i.ToString();
                secondBatch.Add(obj as dynamic);
            }
            
            // This should internally trigger the method "DiffWithHash", which cannot return "modifiedObjects".
            Diff diff = BH.Engine.Diffing.Compute.IDiffing(firstBatch, secondBatch);
            sw.Stop();
            long timespan = sw.ElapsedMilliseconds;

            Debug.Assert(diff.AddedObjects.Count() == 3, "Incorrect number of object identified as new/ToBeCreated.");
            Debug.Assert(diff.ModifiedObjects == null || diff.ModifiedObjects.Count() == 0, "Incorrect number of object identified as modified/ToBeUpdated.");
            Debug.Assert(diff.RemovedObjects.Count() == 3, "Incorrect number of object identified as old/ToBeDeleted.");
            var modifiedPropsPerObj = diff.ModifiedPropsPerObject?.FirstOrDefault().Value;
            Debug.Assert(modifiedPropsPerObj == null, "Incorrect number of changed properties identified by the property-level diffing.");

            Console.WriteLine($"Concluded successfully in {timespan}");
        }

        public static void DiffWithFragmentId_allDifferent(bool logging = false)
        {
            string testName = MethodBase.GetCurrentMethod().Name;

            Console.WriteLine($"\nRunning {testName}");
            Stopwatch sw = Stopwatch.StartNew();

            DiffingConfig DiffingConfig = new DiffingConfig();

            List<IBHoMObject> firstBatch = new List<IBHoMObject>();
            List<IBHoMObject> secondBatch = new List<IBHoMObject>();

            for (int i = 0; i < 3; i++)
            {
                Bar obj = BH.Engine.Base.Create.RandomObject(typeof(Bar)) as Bar;
                TestIdFragment testIdFragment = new TestIdFragment() { Id = i };

                obj.Name = "bar_" + i.ToString();
                obj = obj.AddFragment(testIdFragment) as Bar;


                firstBatch.Add(obj as dynamic);
            }

            for (int i = 0; i < 3; i++)
            {
                Bar obj = BH.Engine.Base.Create.RandomObject(typeof(Bar)) as Bar;
                TestIdFragment testIdFragment = new TestIdFragment() { Id = i };

                obj.Name = "bar_" + i.ToString();
                obj = obj.AddFragment(testIdFragment) as Bar;

                secondBatch.Add(obj as dynamic);
            }

            // This should internally trigger the method "DiffWithHash", which cannot return "modifiedObjects".
            Diff diff = BH.Engine.Diffing.Compute.DiffWithFragmentId(firstBatch, secondBatch, typeof(TestIdFragment), "Id");
            sw.Stop();
            long timespan = sw.ElapsedMilliseconds;

            Debug.Assert(diff.AddedObjects.Count() == 0, "Incorrect number of object identified as new/ToBeCreated.");
            Debug.Assert(diff.ModifiedObjects.Count() == 3, "Incorrect number of object identified as modified/ToBeUpdated.");
            Debug.Assert(diff.RemovedObjects.Count() == 0, "Incorrect number of object identified as old/ToBeDeleted.");
            //var modifiedPropsPerObj = diff.ModifiedPropsPerObject?.FirstOrDefault().Value;
            //Debug.Assert(modifiedPropsPerObj == null, "Incorrect number of changed properties identified by the property-level diffing.");

            Console.WriteLine($"Concluded successfully in {timespan}");
        }

        public static void DiffWithFragmentId_allEqual(bool logging = false)
        {
            string testName = MethodBase.GetCurrentMethod().Name;

            Console.WriteLine($"\nRunning {testName}");
            Stopwatch sw = Stopwatch.StartNew();

            DiffingConfig DiffingConfig = new DiffingConfig();

            List<IBHoMObject> firstBatch = new List<IBHoMObject>();
            List<IBHoMObject> secondBatch = new List<IBHoMObject>();

            for (int i = 0; i < 3; i++)
            {
                Bar obj = BH.Engine.Base.Create.RandomObject(typeof(Bar)) as Bar;
                TestIdFragment testIdFragment = new TestIdFragment() { Id = i };

                obj.Name = "bar_" + i.ToString();
                obj = obj.AddFragment(testIdFragment) as Bar;


                firstBatch.Add(obj as dynamic);
            }


            secondBatch.AddRange(firstBatch);

            // This should internally trigger the method "DiffWithHash", which cannot return "modifiedObjects".
            Diff diff = BH.Engine.Diffing.Compute.DiffWithFragmentId(firstBatch, secondBatch, typeof(TestIdFragment), "Id");
            sw.Stop();
            long timespan = sw.ElapsedMilliseconds;

            Debug.Assert(diff.AddedObjects.Count() == 0, "Incorrect number of object identified as new/ToBeCreated.");
            Debug.Assert(diff.ModifiedObjects.Count() == 0, "Incorrect number of object identified as modified/ToBeUpdated.");
            Debug.Assert(diff.RemovedObjects.Count() == 0, "Incorrect number of object identified as old/ToBeDeleted.");
            Debug.Assert(diff.UnchangedObjects.Count() == 3, "Incorrect number of object identified as unchanged.");

            Console.WriteLine($"Concluded successfully in {timespan}");
        }

        public static void IDiffingTest_DiffWithFragmentId_allDifferentFragments(bool logging = false)
        {
            string testName = MethodBase.GetCurrentMethod().Name;

            Console.WriteLine($"\nRunning {testName}");
            Stopwatch sw = Stopwatch.StartNew();

            DiffingConfig DiffingConfig = new DiffingConfig();

            List<IBHoMObject> firstBatch = new List<IBHoMObject>();
            List<IBHoMObject> secondBatch = new List<IBHoMObject>();

            for (int i = 0; i < 3; i++)
            {
                Bar obj = BH.Engine.Base.Create.RandomObject(typeof(Bar)) as Bar;
                TestIdFragment testIdFragment = new TestIdFragment() { Id = i };
                RandomNumberFragment randomNumberFragment = new RandomNumberFragment();

                obj.Name = "bar_" + i.ToString();
                obj.AddFragment(testIdFragment);
                obj.AddFragment(randomNumberFragment);


                firstBatch.Add(obj as dynamic);
            }

            for (int i = 0; i < 3; i++)
            {
                Bar obj = BH.Engine.Base.Create.RandomObject(typeof(Bar)) as Bar;
                TestIdFragment testIdFragment = new TestIdFragment() { Id = i };
                RandomNumberFragment randomNumberFragment = new RandomNumberFragment();

                obj.Name = "bar_" + i.ToString();
                obj.AddFragment(testIdFragment);
                obj.AddFragment(randomNumberFragment);

                secondBatch.Add(obj as dynamic);
            }

            // This should internally trigger the method "DiffWithHash", which cannot return "modifiedObjects".
            Diff diff = BH.Engine.Diffing.Compute.IDiffing(firstBatch, secondBatch);
            sw.Stop();
            long timespan = sw.ElapsedMilliseconds;

            Debug.Assert(diff.AddedObjects.Count() == 3, "Incorrect number of object identified as new/ToBeCreated.");
            Debug.Assert(diff.ModifiedObjects == null || diff.ModifiedObjects.Count() == 0, "Incorrect number of object identified as modified/ToBeUpdated.");
            Debug.Assert(diff.RemovedObjects.Count() == 3, "Incorrect number of object identified as old/ToBeDeleted.");
            var modifiedPropsPerObj = diff.ModifiedPropsPerObject?.FirstOrDefault().Value;
            Debug.Assert(modifiedPropsPerObj == null, "Incorrect number of changed properties identified by the property-level diffing.");

            Console.WriteLine($"Concluded successfully in {timespan}");
        }

    }
}
