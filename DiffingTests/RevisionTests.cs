/*
 * This file is part of the Buildings and Habitats object Model (BHoM)
 * Copyright (c) 2015 - 2021, the respective contributors. All rights reserved.
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
using BH.Engine.Diffing.Tests;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace BH.Tests.Diffing
{
    [TestClass]
    public class RevisionTests
    {
        // ---------------------------------------------------------- //
        // NOTE: AECDeltas-specific tests                             //
        // These tests check the AECDeltas "Revision" workflow.       //
        // This is now currently still supported but mostly not used. //
        // ---------------------------------------------------------- //

        public void EqualObjects_SameHashInRevisionFragment(bool logging = false)
        {
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

            Assert.IsTrue(bar.FindFragment<RevisionFragment>().Hash == bar2.FindFragment<RevisionFragment>().Hash, "Two equal objects must have the same Hash in the Revision Fragment.");
        }

        public void RevisionFragment_SetAgainOnSameObject(bool logging = false)
        {
            // Create one bar
            Node startNode = BH.Engine.Structure.Create.Node(new Point() { X = 0, Y = 0, Z = 0 });
            Node endNode = BH.Engine.Structure.Create.Node(new Point() { X = 0, Y = 0, Z = 1 });
            Bar bar = BH.Engine.Structure.Create.Bar(startNode, endNode);
            bar.Name = "bar";

            bar = BH.Engine.Diffing.Modify.SetRevisionFragment(bar);

            // Think the object is unchanged and passes through another revision.
            // The following sets its HashFragment again. PreviousHash and currentHash will have to be the same.
            bar = BH.Engine.Diffing.Modify.SetRevisionFragment(bar);

            // Check that the HashFragment's PreviousHash and currentHash are the same
            var hash = bar.FindFragment<RevisionFragment>().Hash;
            var previousHash = bar.FindFragment<RevisionFragment>().PreviousHash;

            Assert.IsTrue(hash == previousHash);
        }

        public void RevisionWorkflow_basic(bool propertyLevelDiffing = true, bool logging = false)
        {
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

            if (logging) BH.Engine.Diffing.Tests.Query.Log(revision_Alessio.Objects, "rev1-hashes", LogOptions.HashesOnly);

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

            // -------------------------------------------------------- //

            // Check delta

            Delta delta = BH.Engine.Diffing.Create.Delta(revision_Alessio, revision_Eduardo, DiffingConfig);

            Assert.IsTrue(delta.Diff.AddedObjects.Count() == 1, "Incorrect number of object identified as new/ToBeCreated.");
            Assert.IsTrue(delta.Diff.ModifiedObjects.Count() == 0, "Incorrect number of object identified as modified/ToBeUpdated.");
            Assert.IsTrue(delta.Diff.UnchangedObjects.Count() == 1, "Incorrect number of object identified as UnchangedObjects.");
            Assert.IsTrue(delta.Diff.RemovedObjects.Count() == 0, "Incorrect number of object identified as RemovedObjects.");
        }

        public void RevisionWorkflow_advanced(bool propertyLevelDiffing = true)
        {
            DiffingConfig DiffingConfig = new DiffingConfig() { EnablePropertyDiffing = propertyLevelDiffing };

            // 1. Suppose Alessio is creating 3 bars in Grasshopper, representing a Portal frame structure.
            // These will be Alessio's "Current" objects.
            List<IBHoMObject> currentObjs_Alessio = new List<IBHoMObject>();

            for (int i = 0; i < 3; i++)
            {
                Bar obj = BH.Engine.Base.Create.RandomObject(typeof(Bar)) as Bar;
                //obj.Fragments = obj.Fragments.Where(fragm => fragm != null).ToList(); // (RandomObject bug workaround: it generates a random number of null fragments)
                obj.Name = "bar_" + i.ToString();
                currentObjs_Alessio.Add(obj as dynamic);
            }

            // 2. Alessio wants these bars to be part of a "Portal frame Stream" that will be tracking the objects for future changes.
            // Alessio creates a first revision
            string comment = "Portal Frame Stream";
            Revision revision_Alessio = BH.Engine.Diffing.Create.Revision(currentObjs_Alessio, Guid.NewGuid(), "", comment, DiffingConfig); // this will add the hash fragments to the objects

            // Alessio can now push the Revision.

            // Logging hashes
            long dateTimeTicks = DateTime.UtcNow.Ticks;
            System.IO.Directory.CreateDirectory(@"C:\temp\BHoM_Tests\Diffing_Engine\");
            string objs_rev1 = JsonConvert.SerializeObject(revision_Alessio.Objects, Formatting.Indented);
            string objs_rev1_hashes = JsonConvert.SerializeObject(revision_Alessio.Objects.Select(obj => ((IBHoMObject)obj).Fragments.OfType<IFragment>()), Formatting.Indented);
            //System.IO.File.WriteAllText($@"C:\temp\BHoM_Tests\Diffing_Engine\Test01_objs-{dateTimeTicks}-rev1.json", objs_rev1);
            System.IO.File.WriteAllText($@"C:\temp\BHoM_Tests\Diffing_Engine\Test01_objs-{dateTimeTicks}-rev1-hashes.json", objs_rev1_hashes);


            // 3. Eduardo is now asked to do some changes to the "Portal frame Project" created by Alessio.
            // On his machine, Eduardo now PULLS the Stream from the external platform to read the existing objects.
            IEnumerable<IBHoMObject> readObjs_Eduardo = revision_Alessio.Objects.Select(obj => BH.Engine.Base.Query.DeepClone(obj) as IBHoMObject).ToList();

            // Eduardo will now work on these objects.
            List<IBHoMObject> currentObjs_Eduardo = readObjs_Eduardo.ToList();

            // 5. Eduardo now modifies one of the bars, deletes another one, and creates a new one. 
            // modifies bar_0
            currentObjs_Eduardo[0].Name = "modifiedBar_0";

            // deletes bar_1
            currentObjs_Eduardo.RemoveAt(1);

            // adds a new bar 
            Bar newBar = BH.Engine.Base.Create.RandomObject(typeof(Bar)) as Bar;
            newBar.Name = "newBar_1";
            currentObjs_Eduardo.Insert(1, newBar as dynamic);

            // Bar_2 will be unchanged at this point.

            // 6. Eduardo updates the Stream Revision.
            Revision revision_Eduardo = BH.Engine.Diffing.Create.Revision(currentObjs_Eduardo, Guid.NewGuid());

            // Eduardo can now push this Stream Revision.

            // Logging hashes
            string objs_rev2 = JsonConvert.SerializeObject(revision_Eduardo.Objects, Formatting.Indented);
            string objs_rev2_hashes = JsonConvert.SerializeObject(revision_Eduardo.Objects.Select(obj => ((IBHoMObject)obj).Fragments.OfType<IFragment>()), Formatting.Indented);
            //System.IO.File.WriteAllText($@"C:\temp\BHoM_Tests\Diffing_Engine\Test01_objs-{dateTimeTicks}-rev2.json", objs_rev2);
            System.IO.File.WriteAllText($@"C:\temp\BHoM_Tests\Diffing_Engine\Test01_objs-{dateTimeTicks}-rev2-hashes.json", objs_rev2_hashes);

            // -------------------------------------------------------- //

            // Eduardo can also manually check the differences.

            Delta delta = BH.Engine.Diffing.Create.Delta(revision_Alessio, revision_Eduardo, DiffingConfig);

            // 7. Now Eduardo can push his new delta object (like step 3).
            // `delta.ToCreate` will have 1 object; `delta2.ToUpdate` 1 object; `delta2.ToDelete` 1 object; `delta2.Unchanged` 2 objects.
            // You can also see which properties have changed for what objects: check `delta2.ModifiedPropsPerObject`.
            Assert.IsTrue(delta.Diff.AddedObjects.Count() == 1, "Incorrect number of object identified as new/ToBeCreated.");
            Assert.IsTrue(delta.Diff.ModifiedObjects.Count() == 1, "Incorrect number of object identified as modified/ToBeUpdated.");
            Assert.IsTrue(delta.Diff.RemovedObjects.Count() == 1, "Incorrect number of object identified as old/ToBeDeleted.");
            var modifiedObjectDifferences = delta.Diff.ModifiedObjectsDifferences.FirstOrDefault().Differences;
            Assert.IsTrue(modifiedObjectDifferences.Count() == 1, "Incorrect number of changed properties identified by the property-level diffing.");
            var identifiedPropertyDifference = modifiedObjectDifferences.FirstOrDefault();
            Assert.IsTrue(identifiedPropertyDifference.DisplayName == "Name", $"The modified property should be `Name`, instead it was `{identifiedPropertyDifference.DisplayName}`.");
            Assert.IsTrue(identifiedPropertyDifference.FullName == typeof(Bar).FullName + ".Name", $"The modified property should be `{typeof(Bar).FullName + ".Name"}`, instead it was `{identifiedPropertyDifference.FullName}`.");
            Assert.IsTrue(identifiedPropertyDifference.PastValue as string == "bar_0", $"The {nameof(PropertyDifference.PastValue)} of the modified property should be `bar_0`, instead it was {identifiedPropertyDifference.PastValue}.");
            Assert.IsTrue(identifiedPropertyDifference.FollowingValue as string == "modifiedBar_0", $"The {nameof(PropertyDifference.FollowingValue)} of the modified property should be `modifiedBar_0`, instead it was {identifiedPropertyDifference.FollowingValue}.");
        }
    }
}
