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
using BH.Adapter.File;
using BH.oM.Dimensional;
using BH.oM.Adapters.File;

namespace Tests
{
    internal static partial class DiffingTests
    {
        public static void HashTest_CostantHash_IdenticalObjs(bool logging = false)
        {
            string testName = MethodBase.GetCurrentMethod().Name;
            Console.WriteLine($"\nRunning {testName}");

            Stopwatch sw = Stopwatch.StartNew();

            // Create one bar
            Node startNode = BH.Engine.Structure.Create.Node(new Point() { X = 0, Y = 0, Z = 0 });
            Node endNode = BH.Engine.Structure.Create.Node(new Point() { X = 0, Y = 0, Z = 1 });
            Bar bar = BH.Engine.Structure.Create.Bar(startNode, endNode);
            bar.Name = "bar";

            bar = BH.Engine.Base.Modify.SetHashFragment(bar);

            // Create another bar identical to the first
            Node startNode2 = BH.Engine.Structure.Create.Node(new Point() { X = 0, Y = 0, Z = 0 });
            Node endNode2 = BH.Engine.Structure.Create.Node(new Point() { X = 0, Y = 0, Z = 1 });
            Bar bar2 = BH.Engine.Structure.Create.Bar(startNode, endNode);
            bar2.Name = "bar";

            bar2 = BH.Engine.Base.Modify.SetHashFragment(bar2);

            if (logging) Logger.Log(new List<object>() { bar, bar2 }, "TwoIdenticalBars", LogOptions.ObjectsAndHashes);

            sw.Stop();
            string hash1 = bar.FindFragment<HashFragment>().Hash;
            string hash2 = bar2.FindFragment<HashFragment>().Hash;
            Debug.Assert(hash1 == hash2);

            long timespan = sw.ElapsedMilliseconds;
            Console.WriteLine($"Concluded successfully in {timespan}");
        }

        public static void HashTest_CostantHash_NumericalPrecision(bool logging = false)
        {
            string testName = MethodBase.GetCurrentMethod().Name;
            Console.WriteLine($"\nRunning {testName}");

            Stopwatch sw = Stopwatch.StartNew();

            // Set numerical precision
            ComparisonConfig cc = new ComparisonConfig() { NumericTolerance = 1E-3 };

            // Create one node
            Node node1 = BH.Engine.Structure.Create.Node(new Point() { X = 0, Y = 0, Z = 0 });
            node1 = BH.Engine.Base.Modify.SetHashFragment(node1, cc);
            string hash1 = node1.FindFragment<HashFragment>().Hash;

            // Create another node with similar coordinates that should be ignored by precision
            Node node2 = BH.Engine.Structure.Create.Node(new Point() { X = 0, Y = 0, Z = 0.0005 });
            node2 = BH.Engine.Base.Modify.SetHashFragment(node2, cc);
            string hash2 = node2.FindFragment<HashFragment>().Hash;

            Debug.Assert(hash1 == hash2);

            // Create another node with similar coordinates that should be considered as different by precision
            Node node3 = BH.Engine.Structure.Create.Node(new Point() { X = 0, Y = 0, Z = 0.005 });
            node3 = BH.Engine.Base.Modify.SetHashFragment(node3, cc);
            string hash3 = node3.FindFragment<HashFragment>().Hash;

            Debug.Assert(hash1 != hash3);

            sw.Stop();
            long timespan = sw.ElapsedMilliseconds;
            Console.WriteLine($"Concluded successfully in {timespan}");
        }

        public static void HashTest_HashComparer(bool logging = false)
        {
            string testName = MethodBase.GetCurrentMethod().Name;
            Console.WriteLine($"\nRunning {testName}");

            Stopwatch sw = Stopwatch.StartNew();

            // Set numerical precision
            ComparisonConfig cc = new ComparisonConfig() { NumericTolerance = 1E-3 };

            // Instantiate hashcomparer for nodes. The `true` boolean means it should assign the calculated hashes to objects. 
            HashComparer<Node> hashComparer = new HashComparer<Node>(cc, true);

            // Create one node
            Node node1 = BH.Engine.Structure.Create.Node(new Point() { X = 0, Y = 0, Z = 0 });
            node1 = BH.Engine.Base.Modify.SetHashFragment(node1, cc);

            // Create another node with similar coordinates that should be ignored by precision
            Node node2 = BH.Engine.Structure.Create.Node(new Point() { X = 0, Y = 0, Z = 0.0005 });
            node2 = BH.Engine.Base.Modify.SetHashFragment(node2, cc);

            Debug.Assert(hashComparer.Equals(node1, node2));

            // Create another node with similar coordinates that should be considered as different by precision
            Node node3 = BH.Engine.Structure.Create.Node(new Point() { X = 0, Y = 0, Z = 0.005 });

            // Instantiate another hashcomparer for nodes. The `false` boolean means it should NOT assign the calculated hashes to objects. 
            HashComparer<Node> hashComparer_notAssign = new HashComparer<Node>(cc, false);

            Debug.Assert(!hashComparer_notAssign.Equals(node1, node3));

            // Check if HashComparer assigned the hashes in the fragments.
            Debug.Assert(!string.IsNullOrWhiteSpace(node1.FindFragment<HashFragment>()?.Hash));
            Debug.Assert(!string.IsNullOrWhiteSpace(node2.FindFragment<HashFragment>()?.Hash));
            Debug.Assert(string.IsNullOrWhiteSpace(node3.FindFragment<HashFragment>()?.Hash));

            sw.Stop();
            long timespan = sw.ElapsedMilliseconds;
            Console.WriteLine($"Concluded successfully in {timespan}");
        }

        public static void HashTest_RemoveDuplicatesByHash(bool logging = false)
        {
            string testName = MethodBase.GetCurrentMethod().Name;
            Console.WriteLine($"\nRunning {testName}");

            Stopwatch sw = Stopwatch.StartNew();

            // Set numerical precision
            ComparisonConfig cc = new ComparisonConfig() { NumericTolerance = 1E-3 };

            // Instantiate hashcomparer for nodes. The `true` boolean means it should assign the calculated hashes to objects. 
            HashComparer<Node> hashComparer = new HashComparer<Node>(cc, true);

            // Create one node
            Node node1 = BH.Engine.Structure.Create.Node(new Point() { X = 0, Y = 0, Z = 0 });
            node1 = BH.Engine.Base.Modify.SetHashFragment(node1, cc);

            // Create another node with similar coordinates that should be ignored by precision
            Node node2 = BH.Engine.Structure.Create.Node(new Point() { X = 0, Y = 0, Z = 0.0005 });
            node2 = BH.Engine.Base.Modify.SetHashFragment(node2, cc);

            Debug.Assert(hashComparer.Equals(node1, node2));

            // Create another node with similar coordinates that should be considered as different by precision
            Node node3 = BH.Engine.Structure.Create.Node(new Point() { X = 0, Y = 0, Z = 0.005 });

            List<Node> allNodes = new List<Node>();

            int n = 3;
            for (int i = 0; i < n; i++)
            {
                // Add n copies of the nodes in the list.
                allNodes.Add(node1);
                allNodes.Add(node2);
                allNodes.Add(node3);
            }

            var result = BH.Engine.Diffing.Modify.RemoveDuplicatesByHash(allNodes).ToList();

            Debug.Assert(result.Count == 2); // node1 and node3 must be recongnised as the same; hence only 2 unique objects should be in the list.

            sw.Stop();
            long timespan = sw.ElapsedMilliseconds;
            Console.WriteLine($"Concluded successfully in {timespan}");
        }

        public static void HashTest_CustomDataToConsider_equalObjects(bool logging = false)
        {
            string testName = MethodBase.GetCurrentMethod().Name;
            Console.WriteLine($"\nRunning {testName}");

            Stopwatch sw = Stopwatch.StartNew();

            // Set PropertiesToConsider
            ComparisonConfig cc = new ComparisonConfig() { CustomdataKeysToInclude = { "Gnappi" } };

            // Instantiate hashcomparer for nodes. The `true` boolean means it should assign the calculated hashes to objects. 
            HashComparer<Node> hashComparer = new HashComparer<Node>(cc, true);

            // Create one node
            Node node1 = BH.Engine.Structure.Create.Node(new Point() { X = 0, Y = 0, Z = 0 });
            node1.CustomData.Add("Gnappi", 1);
            node1.CustomData.Add("Zurli", 9999);

            // Create another node with similar coordinates that should be considered as different
            Node node2 = BH.Engine.Structure.Create.Node(new Point() { X = 0, Y = 0, Z = 0 });
            node2.CustomData.Add("Gnappi", 1);
            node2.CustomData.Add("Zurli", 0);

            Debug.Assert(hashComparer.Equals(node1, node2));

            sw.Stop();
            long timespan = sw.ElapsedMilliseconds;
            Console.WriteLine($"Concluded successfully in {timespan}");
        }

        public static void HashTest_CustomDataToConsider_differentObjects(bool logging = false)
        {
            string testName = MethodBase.GetCurrentMethod().Name;
            Console.WriteLine($"\nRunning {testName}");

            Stopwatch sw = Stopwatch.StartNew();

            // Set PropertiesToConsider
            ComparisonConfig cc = new ComparisonConfig() { CustomdataKeysToInclude = { "Zurli" } };

            // Instantiate hashcomparer for nodes. The `true` boolean means it should assign the calculated hashes to objects. 
            HashComparer<Node> hashComparer = new HashComparer<Node>(cc, true);

            // Create one node
            Node node1 = BH.Engine.Structure.Create.Node(new Point() { X = 0, Y = 0, Z = 0 });
            node1.CustomData.Add("Gnappi", 1);
            node1.CustomData.Add("Zurli", 9999);

            // Create another node with similar coordinates that should be considered as different
            Node node2 = BH.Engine.Structure.Create.Node(new Point() { X = 0, Y = 0, Z = 0 });
            node2.CustomData.Add("Gnappi", 1);
            node2.CustomData.Add("Zurli", 0);

            Debug.Assert(!hashComparer.Equals(node1, node2));

            sw.Stop();
            long timespan = sw.ElapsedMilliseconds;
            Console.WriteLine($"Concluded successfully in {timespan}");
        }

        public static void HashTest_CustomDataToExclude_equalObjects(bool logging = false)
        {
            string testName = MethodBase.GetCurrentMethod().Name;
            Console.WriteLine($"\nRunning {testName}");

            Stopwatch sw = Stopwatch.StartNew();

            // Set PropertiesToConsider
            ComparisonConfig cc = new ComparisonConfig() { CustomdataKeysExceptions = { "Zurli" } };

            // Instantiate hashcomparer for nodes. The `true` boolean means it should assign the calculated hashes to objects. 
            HashComparer<Node> hashComparer = new HashComparer<Node>(cc, true);

            // Create one node
            Node node1 = BH.Engine.Structure.Create.Node(new Point() { X = 0, Y = 0, Z = 0 });
            node1.CustomData.Add("Gnappi", 1);
            node1.CustomData.Add("Zurli", 9999);

            // Create another node with similar coordinates that should be considered as different
            Node node2 = BH.Engine.Structure.Create.Node(new Point() { X = 0, Y = 0, Z = 0 });
            node2.CustomData.Add("Gnappi", 1);
            node2.CustomData.Add("Zurli", 0);

            Debug.Assert(hashComparer.Equals(node1, node2));

            sw.Stop();
            long timespan = sw.ElapsedMilliseconds;
            Console.WriteLine($"Concluded successfully in {timespan}");
        }

        public static void TypeExceptions(bool logging = false)
        {
            string testName = MethodBase.GetCurrentMethod().Name;
            Console.WriteLine($"\nRunning {testName}");

            Stopwatch sw = Stopwatch.StartNew();

            // Create one node
            Node node1 = BH.Engine.Structure.Create.Node(new Point() { X = 0, Y = 0, Z = 0 });

            // Create another node with different coordinates
            Node node2 = BH.Engine.Structure.Create.Node(new Point() { X = 0, Y = 0, Z = 50 });
            node2.Name = "node2";

            // Create another node with different coordinates
            Node node3 = BH.Engine.Structure.Create.Node(new Point() { X = 0, Y = 0, Z = 100 });

            // Create two parent Bars for the nodes
            Bar bar1 = new Bar();
            Bar bar2 = new Bar();

            bar1.StartNode = node1;
            bar1.EndNode = node2;

            bar2.StartNode = node1;
            bar2.EndNode = node3; // EndNode is different for the two bars.

            // By ignoring the IElement1D types, we ignore any difference in Nodes. Bars should be seen as equal.
            ComparisonConfig cc = new ComparisonConfig() { TypeExceptions = { typeof(IElement1D)} };
            HashComparer<Bar> hashComparer_bars_onlyEndNodePosition = new HashComparer<Bar>(cc);
            Debug.Assert(hashComparer_bars_onlyEndNodePosition.Equals(bar1, bar2));

            sw.Stop();
            long timespan = sw.ElapsedMilliseconds;
            Console.WriteLine($"Concluded successfully in {timespan}");
        }


            public static void HashTest_PropertiesToConsider(bool logging = false)
        {
            string testName = MethodBase.GetCurrentMethod().Name;
            Console.WriteLine($"\nRunning {testName}");

            Stopwatch sw = Stopwatch.StartNew();

            // Set PropertiesToConsider
            ComparisonConfig cc = new ComparisonConfig() { PropertiesToConsider = { "Position" } };

            // Instantiate hashcomparer for nodes. The `true` boolean means it should assign the calculated hashes to objects. 
            HashComparer<Node> hashComparer = new HashComparer<Node>(cc, true);

            // Create one node
            Node node1 = BH.Engine.Structure.Create.Node(new Point() { X = 0, Y = 0, Z = 0 });
            node1 = BH.Engine.Base.Modify.SetHashFragment(node1, cc);

            // Create another node with similar coordinates that should be considered as different
            Node node2 = BH.Engine.Structure.Create.Node(new Point() { X = 0, Y = 0, Z = 50 });

            Debug.Assert(!hashComparer.Equals(node1, node2)); // node1 and node2 must be recongnised as different

            // Create another node similar to node1 but with the name changed.
            Node node3 = BH.Engine.Structure.Create.Node(new Point() { X = 0, Y = 0, Z = 0 });
            node3.Name = "Node3"; // the name is the only thing that distinguishes node3 from node1

            Debug.Assert(hashComparer.Equals(node1, node3)); // although the name is different, they must be recongnised as the same.

            sw.Stop();
            long timespan = sw.ElapsedMilliseconds;
            Console.WriteLine($"Concluded successfully in {timespan}");
        }

        public static void HashTest_PropertiesToConsider_subProps(bool logging = false)
        {
            string testName = MethodBase.GetCurrentMethod().Name;
            Console.WriteLine($"\nRunning {testName}");

            Stopwatch sw = Stopwatch.StartNew();

            // Create one node
            Node node1 = BH.Engine.Structure.Create.Node(new Point() { X = 0, Y = 0, Z = 0 });

            // Create another node similar to node1 but with the name changed.
            Node node1_diffName = BH.Engine.Structure.Create.Node(new Point() { X = 0, Y = 0, Z = 0 });
            node1_diffName.Name = "node1_diffName";

            // Create another node with different coordinates
            Node node2 = BH.Engine.Structure.Create.Node(new Point() { X = 0, Y = 0, Z = 50 });
            node2.Name = "node2";

            // Create another node similar to node2 but with the name changed.
            Node node2_diffName = BH.Engine.Structure.Create.Node(new Point() { X = 0, Y = 0, Z = 50 });
            node2_diffName.Name = "node2_diffName";

            // // - Checks on Bars
            // Create two parent Bars for the nodes
            Bar bar1 = new Bar();
            Bar bar2 = new Bar();

            bar1.Name = "bar1";
            bar1.StartNode = node1;
            bar1.EndNode = node2;

            bar2.Name = "bar2";
            bar2.StartNode = node1_diffName;
            bar2.EndNode = node2_diffName;

            // By looking only at EndNode.Position, bars should be the same.
            ComparisonConfig cc1 = new ComparisonConfig() { PropertiesToConsider = { "EndNode.Position" } };
            HashComparer<Bar> hashComparer_bars_onlyEndNodePosition = new HashComparer<Bar>(cc1);
            Debug.Assert(hashComparer_bars_onlyEndNodePosition.Equals(bar1, bar2));

            // By looking only at EndNode.Position, and StartNode.Position, bars should be the same.
            ComparisonConfig cc2 = new ComparisonConfig() { PropertiesToConsider = { "EndNode.Position", "StartNode.Position" } };
            HashComparer<Bar> hashComparer_bars_onlyStartNodePosition = new HashComparer<Bar>(cc2);
            Debug.Assert(hashComparer_bars_onlyStartNodePosition.Equals(bar1, bar2));

            // By looking only at EndNode.Name, bars should be the different.
            ComparisonConfig cc3 = new ComparisonConfig() { PropertiesToConsider = { "EndNode.Name" } };
            var c = new HashComparer<Bar>(cc3);
            Debug.Assert(!c.Equals(bar1, bar2));

            // By looking only at Name, bars should be the different. Note: this only checks the Bar.Name, and will not consider checking pairs of subproperties' names,
            // e.g. it will not look if node1.StartNode.Name is equal or different to node1_diffName.StartNode.Name.
            // In other words, this stops at the topmost matching property.
            ComparisonConfig cc4 = new ComparisonConfig() { PropertiesToConsider = { "Name" } };
            c = new HashComparer<Bar>(cc4);
            Debug.Assert(!c.Equals(bar1, bar2));

            HashComparer<Bar> hashComparer_bars = new HashComparer<Bar>();
            Debug.Assert(!hashComparer_bars.Equals(bar1, bar2)); // by default, the bars should be seen as different.

            sw.Stop();
            long timespan = sw.ElapsedMilliseconds;
            Console.WriteLine($"Concluded successfully in {timespan}");
        }

        public static void HashTest_PropertiesToConsider_samePropertyNameAtMultipleLevels(bool logging = false)
        {
            string testName = MethodBase.GetCurrentMethod().Name;
            Console.WriteLine($"\nRunning {testName}");

            Stopwatch sw = Stopwatch.StartNew();

            // Create one node
            Node node1 = BH.Engine.Structure.Create.Node(new Point() { X = 0, Y = 0, Z = 0 });

            // Create another node similar to node1 but with the name changed.
            Node node1_diffName = BH.Engine.Structure.Create.Node(new Point() { X = 0, Y = 0, Z = 0 });
            node1_diffName.Name = "node1_diffName";

            // Create another node with different coordinates
            Node node2 = BH.Engine.Structure.Create.Node(new Point() { X = 0, Y = 0, Z = 50 });
            node2.Name = "node2";

            // Create another node similar to node2 but with the name changed.
            Node node2_diffName = BH.Engine.Structure.Create.Node(new Point() { X = 0, Y = 0, Z = 50 });
            node2_diffName.Name = "node2_diffName";

            // // - Checks on Bars
            // Create two parent Bars for the nodes
            Bar bar1 = new Bar();
            Bar bar2 = new Bar();

            bar1.Name = "bar1";
            bar1.StartNode = node1;
            bar1.EndNode = node2;

            bar2.Name = "bar2"; // DIFFERENT NAMES FOR BARS
            bar2.StartNode = node1_diffName; // DIFFERENT NAMES FOR STARTNODES
            bar2.EndNode = node2_diffName; // DIFFERENT NAMES FOR ENDNODES

            // By looking only at Name, bars should be the different. Note: this only checks the Bar.Name, and will not consider checking pairs of subproperties' names,
            // e.g. it will not look if node1.StartNode.Name is equal or different to node1_diffName.StartNode.Name.
            // In other words, this stops at the topmost matching property.
            ComparisonConfig cc = new ComparisonConfig() { PropertiesToConsider = { "Name" } };
            var c = new HashComparer<Bar>(cc);
            Debug.Assert(!c.Equals(bar1, bar2));

            // Change bar names to make them equal
            bar1.Name = "bar1";
            bar2.Name = "bar1";

            cc = new ComparisonConfig() { PropertiesToConsider = { "*.Name" } };
            c = new HashComparer<Bar>(cc);

            // Bars should still be different as StartNode and Endnodes have different `Name`s
            Debug.Assert(!c.Equals(bar1, bar2));

            sw.Stop();
            long timespan = sw.ElapsedMilliseconds;
            Console.WriteLine($"Concluded successfully in {timespan}");
        }

        public static void HashTest_PropertyExceptions(bool logging = false)
        {
            string testName = MethodBase.GetCurrentMethod().Name;
            Console.WriteLine($"\nRunning {testName}");

            Stopwatch sw = Stopwatch.StartNew();

            // Set PropertiesToConsider
            ComparisonConfig cc = new ComparisonConfig() { PropertyExceptions = { "Bar.*.Position.X", "Name" } }; // Ignore changes in: Bar.StartNode.X and Bar.EndNode.X; Name.

            // Create one bar
            Node startNode = BH.Engine.Structure.Create.Node(new Point() { X = 0, Y = 0, Z = 0 });
            Node endNode = BH.Engine.Structure.Create.Node(new Point() { X = 0, Y = 0, Z = 1 });
            Bar bar = BH.Engine.Structure.Create.Bar(startNode, endNode);
            bar.Name = "bar1";

            bar = BH.Engine.Base.Modify.SetHashFragment(bar, cc);

            // Create another bar identical to the first
            Node startNode2 = BH.Engine.Structure.Create.Node(new Point() { X = 99, Y = 0, Z = 0 }); // note the X is different from bar1 nodes.
            Node endNode2 = BH.Engine.Structure.Create.Node(new Point() { X = 99, Y = 0, Z = 1 });
            Bar bar2 = BH.Engine.Structure.Create.Bar(startNode, endNode);
            bar2.Name = "bar2";

            bar2 = BH.Engine.Base.Modify.SetHashFragment(bar2, cc);

            sw.Stop();
            string hash1 = bar.FindFragment<HashFragment>().Hash;
            string hash2 = bar2.FindFragment<HashFragment>().Hash;
            Debug.Assert(hash1 == hash2);

            sw.Stop();
            long timespan = sw.ElapsedMilliseconds;
            Console.WriteLine($"Concluded successfully in {timespan}");
        }

        public static void HashTest_CheckAgainstStoredHash(bool logging = false)
        {
            string testName = MethodBase.GetCurrentMethod().Name;
            Console.WriteLine($"\nRunning {testName}");

            Stopwatch sw = Stopwatch.StartNew();

            // Set PropertiesToConsider
            ComparisonConfig cc = new ComparisonConfig() { PropertyExceptions = { "Bar.*.Position.X", "Name" } }; // Ignore changes in: Bar.StartNode.X and Bar.EndNode.X; Name.

            // Create one bar
            Node startNode = BH.Engine.Structure.Create.Node(new Point() { X = 0, Y = 0, Z = 0 });
            Node endNode = BH.Engine.Structure.Create.Node(new Point() { X = 0, Y = 0, Z = 1 });
            Bar bar = BH.Engine.Structure.Create.Bar(startNode, endNode);
            bar.Name = "bar1";

            // Store the HashFragment in the bar
            bar = BH.Engine.Base.Modify.SetHashFragment(bar, cc);

            // Write the bar to file. This should be done only once.
            string filePath = Path.GetFullPath(Path.Combine(System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location), @"..\..\..\Test Datasets\HashTest_CheckAgainstStoredHash-Bar.json"));
            FileAdapter fa = new FileAdapter(filePath);
            if (false) // set this to true only when you want to update the existing file.
                fa.Push(new List<object>() { bar }, "", BH.oM.Adapter.PushType.DeleteThenCreate);

            // Pull from file.
            var pullResult = fa.Pull(new FileContentRequest() { File = filePath }).OfType<IBHoMObject>().FirstOrDefault();

            string hashBefore = pullResult.FindFragment<HashFragment>().Hash;
            string hashNow = bar.FindFragment<HashFragment>().Hash;

            Debug.Assert(hashBefore == hashNow, "The hash for the same Bar object has changed");

            sw.Stop();
            long timespan = sw.ElapsedMilliseconds;
            Console.WriteLine($"Concluded successfully in {timespan}");
        }

    }
}
