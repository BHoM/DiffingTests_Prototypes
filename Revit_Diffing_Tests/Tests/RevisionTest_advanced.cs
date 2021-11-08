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

namespace Tests
{
    internal static partial class DiffingTests
    {
        public static void RevisionTest_advanced(bool propertyLevelDiffing = true)
        {
            string testName = MethodBase.GetCurrentMethod().Name;

            Console.WriteLine($"\nRunning {testName}");
            Stopwatch sw = Stopwatch.StartNew();

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
            Revision revision_Alessio = Create.Revision(currentObjs_Alessio, Guid.NewGuid(), "", comment, DiffingConfig); // this will add the hash fragments to the objects

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
            Revision revision_Eduardo = Create.Revision(currentObjs_Eduardo, Guid.NewGuid());

            // Eduardo can now push this Stream Revision.

            // Logging hashes
            string objs_rev2 = JsonConvert.SerializeObject(revision_Eduardo.Objects, Formatting.Indented);
            string objs_rev2_hashes = JsonConvert.SerializeObject(revision_Eduardo.Objects.Select(obj => ((IBHoMObject)obj).Fragments.OfType<IFragment>()), Formatting.Indented);
            //System.IO.File.WriteAllText($@"C:\temp\BHoM_Tests\Diffing_Engine\Test01_objs-{dateTimeTicks}-rev2.json", objs_rev2);
            System.IO.File.WriteAllText($@"C:\temp\BHoM_Tests\Diffing_Engine\Test01_objs-{dateTimeTicks}-rev2-hashes.json", objs_rev2_hashes);

            // -------------------------------------------------------- //

            // Eduardo can also manually check the differences.

            Delta delta = BH.Engine.Diffing.Create.Delta(revision_Alessio, revision_Eduardo, DiffingConfig);

            sw.Stop();

            // 7. Now Eduardo can push his new delta object (like step 3).
            // `delta.ToCreate` will have 1 object; `delta2.ToUpdate` 1 object; `delta2.ToDelete` 1 object; `delta2.Unchanged` 2 objects.
            // You can also see which properties have changed for what objects: check `delta2.ModifiedPropsPerObject`.
            Debug.Assert(delta.Diff.AddedObjects.Count() == 1, "Incorrect number of object identified as new/ToBeCreated.");
            Debug.Assert(delta.Diff.ModifiedObjects.Count() == 1, "Incorrect number of object identified as modified/ToBeUpdated.");
            Debug.Assert(delta.Diff.RemovedObjects.Count() == 1, "Incorrect number of object identified as old/ToBeDeleted.");
            var modifiedPropsPerObj = delta.Diff.ModifiedPropsPerObject?.First().Value;
            Debug.Assert(modifiedPropsPerObj?.Count == 1, "Incorrect number of changed properties identified by the property-level diffing.");
            Debug.Assert(modifiedPropsPerObj?.First().Key == "Name", "Error in property-level diffing");
            Debug.Assert(modifiedPropsPerObj?.First().Value.Item1 as string == "modifiedBar_0", "Error in property-level diffing");

            long timespan = sw.ElapsedMilliseconds;
            Console.WriteLine($"Concluded successfully in {timespan}");
        }

    }
}
