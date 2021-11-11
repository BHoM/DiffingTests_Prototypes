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
using BH.oM.Dimensional;
using BH.Engine.Base.Objects;
using BH.oM.Reflection.Attributes;
using BH.Engine.Diffing.Tests;
using System.Security.AccessControl;
using System.Security.Principal;

namespace BH.Tests.Diffing
{
    public static class HashTests
    {
        public static void EqualObjectsHaveSameHash()
        {
            var currentMethod = MethodBase.GetCurrentMethod();
            Console.WriteLine($"\nRunning {currentMethod.DeclaringType.Name}.{currentMethod.Name}");
            Stopwatch sw = Stopwatch.StartNew();

            // Create one bar
            Node startNode = BH.Engine.Structure.Create.Node(new Point() { X = 0, Y = 0, Z = 0 });
            Node endNode = BH.Engine.Structure.Create.Node(new Point() { X = 0, Y = 0, Z = 1 });
            Bar bar = BH.Engine.Structure.Create.Bar(startNode, endNode);
            bar.Name = "bar";

            // Create another bar identical to the first
            Node startNode2 = BH.Engine.Structure.Create.Node(new Point() { X = 0, Y = 0, Z = 0 });
            Node endNode2 = BH.Engine.Structure.Create.Node(new Point() { X = 0, Y = 0, Z = 1 });
            Bar bar2 = BH.Engine.Structure.Create.Bar(startNode, endNode);
            bar2.Name = "bar";

            // Check that the two computed hashes are the same.
            string hash1 = bar.Hash();
            string hash2 = bar2.Hash();
            Debug.Assert(hash1 == hash2);

            sw.Stop();
            long timespan = sw.ElapsedMilliseconds;
            Console.WriteLine($"Concluded successfully in {timespan}");
        }

        public static void NumericTolerance_SameHash()
        {
            var currentMethod = MethodBase.GetCurrentMethod();
            Console.WriteLine($"\nRunning {currentMethod.DeclaringType.Name}.{currentMethod.Name}");
            Stopwatch sw = Stopwatch.StartNew();

            // Set a numerical tolerance (different from the default value).
            ComparisonConfig cc = new ComparisonConfig() { NumericTolerance = 1E-3 };

            // Create one node.
            Node node1 = BH.Engine.Structure.Create.Node(new Point() { X = 0, Y = 0, Z = 0 });
            string hash1 = node1.Hash(cc);

            // Create another node with similar coordinates. The difference should be so minimal that is ignored by the tolerance.
            Node node2 = BH.Engine.Structure.Create.Node(new Point() { X = 0, Y = 0, Z = 0.0005 });
            string hash2 = node2.Hash(cc);

            // Make sure the hashes are the same, thanks to the numeric tolerance.
            Debug.Assert(hash1 == hash2);

            sw.Stop();
            long timespan = sw.ElapsedMilliseconds;
            Console.WriteLine($"Concluded successfully in {timespan}");
        }

        public static void NumericTolerance_DifferentHash()
        {
            var currentMethod = MethodBase.GetCurrentMethod();
            Console.WriteLine($"\nRunning {currentMethod.DeclaringType.Name}.{currentMethod.Name}");
            Stopwatch sw = Stopwatch.StartNew();

            // Set a numerical tolerance (different from the default value).
            ComparisonConfig cc = new ComparisonConfig() { NumericTolerance = 1E-3 };

            // Create one node.
            Node node1 = BH.Engine.Structure.Create.Node(new Point() { X = 0, Y = 0, Z = 0 });
            string hash1 = node1.Hash(cc);

            // Create another node with similar coordinates that should be considered as different by precision
            Node node2 = BH.Engine.Structure.Create.Node(new Point() { X = 0, Y = 0, Z = 0.005 });
            string hash2 = node2.Hash(cc);

            // Make sure the hashes are different, following the numeric tolerance.
            Debug.Assert(hash1 != hash2);

            sw.Stop();
            long timespan = sw.ElapsedMilliseconds;
            Console.WriteLine($"Concluded successfully in {timespan}");
        }

        public static void HashComparer_AssignHashToFragments()
        {
            var currentMethod = MethodBase.GetCurrentMethod();
            Console.WriteLine($"\nRunning {currentMethod.DeclaringType.Name}.{currentMethod.Name}");
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

            // Make sure the hashComparer sees node1 and node2 as equal.
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

        public static void RemoveDuplicatesByHash()
        {
            var currentMethod = MethodBase.GetCurrentMethod();
            Console.WriteLine($"\nRunning {currentMethod.DeclaringType.Name}.{currentMethod.Name}");
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

            // Make sure the hashComparer sees node1 and node2 as equal.
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

            Debug.Assert(result.Count == 2); // node1 and node2 must be recognised as the same; hence only 2 unique objects should be in the list.

            sw.Stop();
            long timespan = sw.ElapsedMilliseconds;
            Console.WriteLine($"Concluded successfully in {timespan}");
        }

        public static void CustomDataToConsider_EqualObjects()
        {
            var currentMethod = MethodBase.GetCurrentMethod();
            Console.WriteLine($"\nRunning {currentMethod.DeclaringType.Name}.{currentMethod.Name}");
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

        public static void CustomDataToConsider_DifferentObjects()
        {
            var currentMethod = MethodBase.GetCurrentMethod();
            Console.WriteLine($"\nRunning {currentMethod.DeclaringType.Name}.{currentMethod.Name}");
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

        public static void CustomDataToExclude_EqualObjects()
        {
            var currentMethod = MethodBase.GetCurrentMethod();
            Console.WriteLine($"\nRunning {currentMethod.DeclaringType.Name}.{currentMethod.Name}");
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

        public static void TypeExceptions()
        {
            var currentMethod = MethodBase.GetCurrentMethod();
            Console.WriteLine($"\nRunning {currentMethod.DeclaringType.Name}.{currentMethod.Name}");
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
            ComparisonConfig cc = new ComparisonConfig() { TypeExceptions = { typeof(IElement1D) } };
            HashComparer<Bar> hashComparer_bars_onlyEndNodePosition = new HashComparer<Bar>(cc);
            Debug.Assert(hashComparer_bars_onlyEndNodePosition.Equals(bar1, bar2));

            sw.Stop();
            long timespan = sw.ElapsedMilliseconds;
            Console.WriteLine($"Concluded successfully in {timespan}");
        }

        public static void PropertiesToConsider_TopLevelProperty_EqualObjects()
        {
            var currentMethod = MethodBase.GetCurrentMethod();
            Console.WriteLine($"\nRunning {currentMethod.DeclaringType.Name}.{currentMethod.Name}");
            Stopwatch sw = Stopwatch.StartNew();

            // Set PropertiesToConsider
            ComparisonConfig cc = new ComparisonConfig() { PropertiesToConsider = { "Position" } };

            // Create one node
            Node node1 = BH.Engine.Structure.Create.Node(new Point() { X = 0, Y = 0, Z = 0 });
            string hash1 = node1.Hash(cc);

            // Create another node with similar coordinates that should be considered as different via its `Position` property alone.
            Node node2 = BH.Engine.Structure.Create.Node(new Point() { X = 0, Y = 0, Z = 50 });
            string hash2 = node2.Hash(cc);

            Debug.Assert(hash1 != hash2); // node1 and node2 must be recongnised as different

            // Create another node similar to node1 but with the name changed.
            Node node3 = BH.Engine.Structure.Create.Node(new Point() { X = 0, Y = 0, Z = 0 });
            node3.Name = "Node3"; // the name is the only thing that distinguishes node3 from node1
            string hash3 = node3.Hash(cc);

            Debug.Assert(hash1 == hash3); // although the name is different, they must be recongnised as the same.

            sw.Stop();
            long timespan = sw.ElapsedMilliseconds;
            Console.WriteLine($"Concluded successfully in {timespan}");
        }

        public static void PropertiesToConsider_SubProperties_EqualObjects()
        {
            var currentMethod = MethodBase.GetCurrentMethod();
            Console.WriteLine($"\nRunning {currentMethod.DeclaringType.Name}.{currentMethod.Name}");
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
            ComparisonConfig cc_onlyEndNodePosition = new ComparisonConfig() { PropertiesToConsider = { "EndNode.Position" } };
            Debug.Assert(bar1.Hash(cc_onlyEndNodePosition) == bar2.Hash(cc_onlyEndNodePosition));

            // By looking only at EndNode.Position, and StartNode.Position, bars should be the same.
            ComparisonConfig cc_onlyStartNodePosition = new ComparisonConfig() { PropertiesToConsider = { "EndNode.Position", "StartNode.Position" } };
            Debug.Assert(bar1.Hash(cc_onlyStartNodePosition) == bar2.Hash(cc_onlyStartNodePosition));

            // By looking only at EndNode.Name, bars should be the different.
            ComparisonConfig cc_onlyEndNodeName = new ComparisonConfig() { PropertiesToConsider = { "EndNode.Name" } };
            Debug.Assert(bar1.Hash(cc_onlyEndNodeName) != bar2.Hash(cc_onlyEndNodeName));

            // By looking only at Name, bars should be the different. Note: this only checks the Bar.Name, and will not consider checking pairs of subproperties' names,
            // e.g. it will not look if node1.StartNode.Name is equal or different to node1_diffName.StartNode.Name.
            // In other words, this stops at the topmost matching property.
            ComparisonConfig cc_onlyName = new ComparisonConfig() { PropertiesToConsider = { "Name" } };
            Debug.Assert(bar1.Hash(cc_onlyName) != bar2.Hash(cc_onlyName));

            // By default, the bars should be seen as different.
            Debug.Assert(bar1.Hash() != bar2.Hash());

            sw.Stop();
            long timespan = sw.ElapsedMilliseconds;
            Console.WriteLine($"Concluded successfully in {timespan}");
        }

        public static void PropertiesToConsider_FullPropertyNames_EqualObjects()
        {
            var currentMethod = MethodBase.GetCurrentMethod();
            Console.WriteLine($"\nRunning {currentMethod.DeclaringType.Name}.{currentMethod.Name}");
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
            ComparisonConfig cc_onlyEndNodePosition = new ComparisonConfig() { PropertiesToConsider = { "BH.oM.Structure.Elements.Bar.EndNode.Position" } };
            Debug.Assert(bar1.Hash(cc_onlyEndNodePosition) == bar2.Hash(cc_onlyEndNodePosition));

            // By looking only at EndNode.Position, and StartNode.Position, bars should be the same.
            ComparisonConfig cc_onlyStartNodePosition = new ComparisonConfig() { PropertiesToConsider = { "BH.oM.Structure.Elements.Bar.EndNode.Position", "SBH.oM.Structure.Elements.Bar.StartNode.Position" } };
            Debug.Assert(bar1.Hash(cc_onlyStartNodePosition) == bar2.Hash(cc_onlyStartNodePosition));

            // By looking only at EndNode.Name, bars should be the different.
            ComparisonConfig cc_onlyEndNodeName = new ComparisonConfig() { PropertiesToConsider = { "BH.oM.Structure.Elements.Bar.EndNode.Name" } };
            Debug.Assert(bar1.Hash(cc_onlyEndNodeName) != bar2.Hash(cc_onlyEndNodeName));

            // By looking only at Name, bars should be the different. Note: this only checks the Bar.Name, and will not consider checking pairs of subproperties' names,
            // e.g. it will not look if node1.StartNode.Name is equal or different to node1_diffName.StartNode.Name.
            // In other words, this stops at the topmost matching property.
            ComparisonConfig cc_onlyName = new ComparisonConfig() { PropertiesToConsider = { "Name" } };
            Debug.Assert(bar1.Hash(cc_onlyName) != bar2.Hash(cc_onlyName));

            // By default, the bars should be seen as different.
            Debug.Assert(bar1.Hash() != bar2.Hash());

            sw.Stop();
            long timespan = sw.ElapsedMilliseconds;
            Console.WriteLine($"Concluded successfully in {timespan}");
        }

        [Obsolete("This test is similar to a wildcard prefix test. Only Full Names are accepted for PropertiesToConsider at the moment. Method kept for reference.")]
        public static void PropertiesToConsider_PartialPropertyName_DifferentObjects()
        {
            var currentMethod = MethodBase.GetCurrentMethod();
            Console.WriteLine($"\nRunning {currentMethod.DeclaringType.Name}.{currentMethod.Name}");
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
            Bar bar2 = bar1.DeepClone();

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
            Debug.Assert(bar1.Hash(cc) != bar2.Hash(cc));

            // Change bar names (the StartNode.Name in bar1/bar2 are still different!)
            bar1.Name = "bar1";
            bar2.Name = "bar1";

            // Bars should still be different as StartNode and Endnodes have different `Name`s
            Debug.Assert(bar1.Hash(cc) != bar2.Hash(cc));

            sw.Stop();
            long timespan = sw.ElapsedMilliseconds;
            Console.WriteLine($"Concluded successfully in {timespan}");
        }

        public static void PropertiesToConsider_PartialPropertyName_Unsupported()
        {
            var currentMethod = MethodBase.GetCurrentMethod();
            Console.WriteLine($"\nRunning {currentMethod.DeclaringType.Name}.{currentMethod.Name}");
            Stopwatch sw = Stopwatch.StartNew();

            Bar bar1 = new Bar();

            // Non full property names are not supported.
            ComparisonConfig cc = new ComparisonConfig() { PropertiesToConsider = { "Name" } };
            Debug.Assert(bar1.Hash(cc) == ""); // the Hash must be returned empty.

            cc = new ComparisonConfig() { PropertiesToConsider = { "StartNode.Name" } };
            Debug.Assert(bar1.Hash(cc) == ""); // the Hash must be returned empty.

            sw.Stop();
            long timespan = sw.ElapsedMilliseconds;
            Console.WriteLine($"Concluded successfully in {timespan}");
        }

        [Obsolete("Wildcards support is too tricky to implement consistently when computing the Hash, so it was deprecated. Method kept for reference.")]
        public static void PropertiesToConsider_WildCardPrefix_Different()
        {
            var currentMethod = MethodBase.GetCurrentMethod();
            Console.WriteLine($"\nRunning {currentMethod.DeclaringType.Name}.{currentMethod.Name}");
            Stopwatch sw = Stopwatch.StartNew();

            // Create one node
            Node node1 = BH.Engine.Structure.Create.Node(new Point() { X = 0, Y = 0, Z = 0 });

            // Create another node with different coordinates
            Node node2 = BH.Engine.Structure.Create.Node(new Point() { X = 0, Y = 0, Z = 50 });
            node2.Name = "node2";

            // Create another node equal to node2 but with the name changed.
            Node node2_diffName = BH.Engine.Structure.Create.Node(new Point() { X = 0, Y = 0, Z = 50 });
            node2_diffName.Name = "node2_diffName";

            // Create two parent Bars for the nodes
            Bar bar1 = new Bar();
            Bar bar2 = new Bar();

            bar1.Name = "bar1";
            bar2.Name = "bar1"; // SAME NAMES FOR BARS.

            bar1.StartNode = node1;
            bar2.StartNode = node2; // SAME START NODES.

            bar1.EndNode = node2;
            bar2.EndNode = node2_diffName; // DIFFERENT NAMES FOR END NODES.

            // Using Wildcard prefix (to capture all possible properties ending in Name).
            ComparisonConfig cc = new ComparisonConfig() { PropertiesToConsider = { "*.Name" } };

            // Bars should still be different: although the Bar names are the same, their StartNode have different `Name`s
            Debug.Assert(bar1.Hash(cc) != bar2.Hash(cc));

            sw.Stop();
            long timespan = sw.ElapsedMilliseconds;
            Console.WriteLine($"Concluded successfully in {timespan}");
        }

        [Obsolete("Wildcards support is too tricky to implement consistently when computing the Hash, so it was deprecated. Method kept for reference.")]
        public static void PropertiesToConsider_WildCardPrefix_Equals()
        {
            var currentMethod = MethodBase.GetCurrentMethod();
            Console.WriteLine($"\nRunning {currentMethod.DeclaringType.Name}.{currentMethod.Name}");
            Stopwatch sw = Stopwatch.StartNew();

            // Create one node
            Node node1 = BH.Engine.Structure.Create.Node(new Point() { X = 0, Y = 0, Z = 0 });

            // Create another node with different coordinates
            Node node2 = BH.Engine.Structure.Create.Node(new Point() { X = 0, Y = 0, Z = 50 });
            node2.Name = "node2";

            // Create another node with different coordinates but with the same name as node2.
            Node node3 = BH.Engine.Structure.Create.Node(new Point() { X = 100, Y = 100, Z = 100 });
            node3.Name = node2.Name; // SAME NAME AS NODE2, BUT DIFFERENT COORDINATES.

            // Create two parent Bars for the nodes
            Bar bar1 = new Bar();
            Bar bar2 = new Bar();

            bar1.Name = "bar1";
            bar2.Name = "bar1"; // SAME NAMES FOR BARS.

            bar1.StartNode = node1;
            bar2.StartNode = node2; // SAME START NODES.

            bar1.EndNode = node2;
            bar2.EndNode = node3; // DIFFERENT END NODES, BUT WITH THE SAME NAME.

            // Using Wildcard prefix (to capture all possible properties ending in Name).
            ComparisonConfig cc = new ComparisonConfig() { PropertiesToConsider = { "*.Name" } };

            // Bars should be seen as equal: although the End Nodes are different, their names are the same.
            Debug.Assert(bar1.Hash(cc) == bar2.Hash(cc));

            // Equivalently, without the wildcard: `Name`
            cc = new ComparisonConfig() { PropertiesToConsider = { "Name" } };

            // Bars should be seen as equal: although the End Nodes are different, their names are the same.
            Debug.Assert(bar1.Hash(cc) == bar2.Hash(cc));

            sw.Stop();
            long timespan = sw.ElapsedMilliseconds;
            Console.WriteLine($"Concluded successfully in {timespan}");
        }

        public static void PropertiesToConsider_WildCards_Unsupported()
        {
            var currentMethod = MethodBase.GetCurrentMethod();
            Console.WriteLine($"\nRunning {currentMethod.DeclaringType.Name}.{currentMethod.Name}");
            Stopwatch sw = Stopwatch.StartNew();

            Bar bar1 = new Bar();

            // Wildcards in PropertiesToConsider for Hash computation are not supported. An Reflection Error is recorded and an empty string should be returned by Hash().
            ComparisonConfig cc = new ComparisonConfig() { PropertiesToConsider = { "*.Name" } };
            Debug.Assert(bar1.Hash(cc) == "");

            cc = new ComparisonConfig() { PropertiesToConsider = { "BH.oM.*.Name" } };
            Debug.Assert(bar1.Hash(cc) == "");

            sw.Stop();
            long timespan = sw.ElapsedMilliseconds;
            Console.WriteLine($"Concluded successfully in {timespan}");
        }

        public static void PropertyExceptions_EqualObjects()
        {
            var currentMethod = MethodBase.GetCurrentMethod();
            Console.WriteLine($"\nRunning {currentMethod.DeclaringType.Name}.{currentMethod.Name}");
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

        public static void CheckAgainstSerialisedObject(bool resetSerialisedObject = false)
        {
            var currentMethod = MethodBase.GetCurrentMethod();
            Console.WriteLine($"\nRunning {currentMethod.DeclaringType.Name}.{currentMethod.Name}");
            Stopwatch sw = Stopwatch.StartNew();


            string filePath = Path.GetFullPath(Path.Combine(System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location), @"..\..\..\..\Datasets\HashTest_CheckAgainstStoredHash-Bar.json"));

            Bar bar = null;
            ComparisonConfig cc = new ComparisonConfig();
            JsonSerializerSettings settings = new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.Auto };

            if (resetSerialisedObject)
            {
                bar = BH.Engine.Base.Create.RandomObject(typeof(Bar)) as Bar;
                string generatedObjectHash = bar.Hash(cc); // this stores the Hash on the object Fragments too.
                bar = bar.SetHashFragment(generatedObjectHash);

                System.IO.File.WriteAllText(filePath, JsonConvert.SerializeObject(bar, settings));
            }

            if (bar == null)
            {
                // deserialize JSON directly from a file
                using (StreamReader file = File.OpenText(filePath))
                    bar = JsonConvert.DeserializeObject<Bar>(file.ReadToEnd(), settings);
            }

            string hashStoredInSerialisedObject = bar.FindFragment<HashFragment>().Hash;
            string hashNow = bar.Hash(cc);

            Debug.Assert(hashStoredInSerialisedObject == hashNow, "The hash for the same Bar object has changed");

            sw.Stop();
            long timespan = sw.ElapsedMilliseconds;
            Console.WriteLine($"Concluded successfully in {timespan}");
        }
    }
}
