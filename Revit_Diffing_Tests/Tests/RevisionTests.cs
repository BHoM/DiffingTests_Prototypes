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
using System.IO;
using Newtonsoft.Json;
using BH.Engine.Base;

namespace Tests
{
    internal static partial class DiffingTests
    {
        public static void RevisionTest_CostantHash_IdenticalObjs(bool logging = false)
        {
            string testName = MethodBase.GetCurrentMethod().Name;
            Console.WriteLine($"\nRunning {testName}");

            Stopwatch sw = Stopwatch.StartNew();

            // Create one bar
            Node startNode = BH.Engine.Structure.Create.Node(new Point() { X = 0, Y = 0, Z = 0 });
            Node endNode = BH.Engine.Structure.Create.Node(new Point() { X = 0, Y = 0, Z = 1 });
            Bar bar = BH.Engine.Structure.Create.Bar(startNode, endNode);
            bar.Name = "bar";

            bar = BH.Engine.Diffing.Modify.SetRevisionFragment(bar);

            // Create another bar identical to the first
            Node startNode2 = BH.Engine.Structure.Create.Node(new Point() { X = 0, Y = 0, Z = 0 });
            Node endNode2 = BH.Engine.Structure.Create.Node(new Point() { X = 0, Y = 0, Z = 1 });
            Bar bar2 = BH.Engine.Structure.Create.Bar(startNode, endNode);
            bar2.Name = "bar";

            bar2 = BH.Engine.Diffing.Modify.SetRevisionFragment(bar2);

            if (logging) Logger.Log(new List<object>() { bar, bar2 }, "TwoIdenticalBars", LogOptions.ObjectsAndHashes);

            sw.Stop();
            Debug.Assert(bar.FindFragment<RevisionFragment>().Hash == bar2.FindFragment<RevisionFragment>().Hash);

            long timespan = sw.ElapsedMilliseconds;
            Console.WriteLine($"Concluded successfully in {timespan}");
        }

        public static void RevisionTest_UnchangedObjectsSameHash(bool logging = false)
        {
            string testName = MethodBase.GetCurrentMethod().Name;
            Console.WriteLine($"\nRunning {testName}");

            Stopwatch sw = Stopwatch.StartNew();


            // Create one bar
            Node startNode = BH.Engine.Structure.Create.Node(new Point() { X = 0, Y = 0, Z = 0 });
            Node endNode = BH.Engine.Structure.Create.Node(new Point() { X = 0, Y = 0, Z = 1 });
            Bar bar = BH.Engine.Structure.Create.Bar(startNode, endNode);
            bar.Name = "bar";

            bar = BH.Engine.Diffing.Modify.SetRevisionFragment(bar);

            // Think the object is unchanged and passes through another revision.
            // The following sets its HashFragment again. PreviousHash and currentHash will have to be the same.
            bar = BH.Engine.Diffing.Modify.SetRevisionFragment(bar);

            sw.Stop();

            // Check that the HashFragment's PreviousHash and currentHash are the same:
            var hash = bar.FindFragment<RevisionFragment>().Hash;
            var previousHash = bar.FindFragment<RevisionFragment>().PreviousHash;

            Debug.Assert(hash == previousHash);

            long timespan = sw.ElapsedMilliseconds;
            Console.WriteLine($"Concluded successfully in {timespan}");
        }

        public static void RevisionTest_basic(bool propertyLevelDiffing = true, bool logging = false)
        {
            string testName = MethodBase.GetCurrentMethod().Name;

            Console.WriteLine($"\nRunning {testName}");
            Stopwatch sw = Stopwatch.StartNew();

            DiffingConfig DiffingConfig = new DiffingConfig() { EnablePropertyDiffing = propertyLevelDiffing, IncludeUnchangedObjects = true };

            // First object set
            List<IBHoMObject> currentObjs_Alessio = new List<IBHoMObject>();

            // Create one bar
            Node startNode = BH.Engine.Structure.Create.Node(new Point() { X = 0, Y = 0, Z = 0 });
            Node endNode = BH.Engine.Structure.Create.Node(new Point() { X = 0, Y = 0, Z = 1 });
            Bar bar = BH.Engine.Structure.Create.Bar(startNode, endNode);

            bar.Name = "bar";
            currentObjs_Alessio.Add(bar as dynamic);

            // First revision
            Revision revision_Alessio = BH.Engine.Diffing.Create.Revision(currentObjs_Alessio, Guid.NewGuid(), "", "", DiffingConfig); // this will add the hash fragments to the objects

            if (logging) Logger.Log(revision_Alessio.Objects, "rev1-hashes", LogOptions.HashesOnly);

            // Prepare second revision

            // Add a clone of the first bar (will be "unchanged object")
            List<IBHoMObject> currentObjs_Eduardo = revision_Alessio.Objects.Select(obj => BH.Engine.Base.Query.DeepClone(obj) as IBHoMObject).ToList();

            // Add a new bar 
            Bar newBar = BH.Engine.Base.Create.RandomObject(typeof(Bar)) as Bar;
            newBar.Name = "newBar";
            currentObjs_Eduardo.Insert(1, newBar as dynamic);

            // The first Bar will be unchanged at this point.

            // Second revision
            Revision revision_Eduardo = BH.Engine.Diffing.Create.Revision(currentObjs_Eduardo, Guid.NewGuid());

            if (logging) Logger.Log(revision_Alessio.Objects, "rev2-hashes", LogOptions.HashesOnly);


            // -------------------------------------------------------- //

            // Check delta

            Delta delta = BH.Engine.Diffing.Create.Delta(revision_Alessio, revision_Eduardo, DiffingConfig);

            sw.Stop();

            Debug.Assert(delta.Diff.AddedObjects.Count() == 1, "Incorrect number of object identified as new/ToBeCreated.");
            Debug.Assert(delta.Diff.ModifiedObjects.Count() == 0, "Incorrect number of object identified as modified/ToBeUpdated.");
            Debug.Assert(delta.Diff.UnchangedObjects.Count() == 1, "Incorrect number of object identified as UnchangedObjects.");
            Debug.Assert(delta.Diff.RemovedObjects.Count() == 0, "Incorrect number of object identified as RemovedObjects.");

            long timespan = sw.ElapsedMilliseconds;
            Console.WriteLine($"Concluded successfully in {timespan}");
        }
    }
}
