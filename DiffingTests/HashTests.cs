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
using BH.Engine.Base;
using System.IO;
using Newtonsoft.Json;
using BH.oM.Dimensional;
using BH.Engine.Base.Objects;
using BH.oM.Reflection.Attributes;
using BH.Engine.Diffing.Tests;
using System.Security.AccessControl;
using System.Security.Principal;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using BH.oM.Diffing.Test;

namespace BH.Tests.Diffing
{
    [TestClass]
    public class HashTests
    {
        [TestMethod]
        public void EqualObjects_EqualHash()
        {
            // Create one random object
            TestObject testObject1 = BH.Engine.Diffing.Tests.Create.RandomObject<TestObject>();

            // Create another object identical to the first
            TestObject testObject2 = testObject1.DeepClone();

            string hash1 = testObject1.Hash();
            string hash2 = testObject2.Hash();
            Assert.IsTrue(hash1 == hash2, "Two equal objects must have the same hash.");
        }

        [TestMethod]
        public void EqualObjects_EqualHash_RandomComparisonConfig()
        {
            // Create one random object
            TestObject testObject1 = BH.Engine.Diffing.Tests.Create.RandomObject<TestObject>();

            // Create another object identical to the first
            TestObject testObject2 = testObject1.DeepClone();

            // Create a random comparisonConfig
            ComparisonConfig cc = BH.Engine.Diffing.Tests.Create.RandomObject<ComparisonConfig>();
            cc.PropertiesToConsider = null;
            cc.TypeExceptions = null;

            string hash1 = testObject1.Hash(cc);
            string hash2 = testObject2.Hash(cc);
            Assert.IsTrue(hash1 == hash2, "Two equal objects must have the same hash.");
        }

        [TestMethod]
        public void DifferentObjects_DifferentEnums_DifferentHash()
        {
            ComparisonConfig cc = new ComparisonConfig() { NumericTolerance = 3 };

            // Create one random object. This object has an Enum property which is left "undefined".
            TestObject testObject1 = new TestObject();

            // Create another object identical to the first, but where I assign the enum property.
            TestObject testObject2 = new TestObject();
            testObject2.SomeEnum = TestEnum.TestValue2;

            string hash1 = testObject1.Hash(cc);
            string hash2 = testObject2.Hash(cc);
            Assert.IsTrue(hash1 != hash2, "Two objects with different enum assigned must have different hash.");
        }


        [TestMethod]
        public void HashComparer_AssignHashToFragments()
        {
            List<Bar> bars = Engine.Diffing.Tests.Create.RandomObjects<Bar>(2);

            // Instantiate hashcomparer for nodes. The `true` boolean means it should assign the calculated hashes to objects. 
            HashComparer<Bar> hashComparer_AssignHash = new HashComparer<Bar>(new ComparisonConfig(), true);
            hashComparer_AssignHash.Equals(bars[0], bars[1]); // once Equals() is computed, the Hash of the objects is stored in their Fragments.

            // Check if HashComparer assigned the hashes in the fragments.
            Assert.IsTrue(!string.IsNullOrWhiteSpace(bars[0].FindFragment<HashFragment>()?.Hash));
            Assert.IsTrue(!string.IsNullOrWhiteSpace(bars[1].FindFragment<HashFragment>()?.Hash));
        }

        [TestMethod]
        public void RemoveDuplicatesByHash()
        {
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
            Assert.IsTrue(hashComparer.Equals(node1, node2));

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

            Assert.IsTrue(result.Count == 2, "Incorrect number of duplicates found."); // node1 and node2 must be recognised as the same; hence only 2 unique objects should be in the list.
        }

        [TestMethod]
        public void CustomdataKeysToInclude_DifferentObjects()
        {
            // Create one node with a few properties and some CustomData.
            Node node1 = BH.Engine.Structure.Create.Node(new Point() { X = 0, Y = 0, Z = 0 });
            node1.CustomData.Add("Gnappi", 1);
            node1.CustomData.Add("Zurli", 9999);

            // Create another node with same properties but 1 different CustomData.
            Node node2 = BH.Engine.Structure.Create.Node(new Point() { X = 0, Y = 0, Z = 0 });
            node2.CustomData.Add("Gnappi", 1);
            node2.CustomData.Add("Zurli", 0); // different value

            // Set CustomdataKeysToInclude to consider only changes in the CustomData that remained the same.
            ComparisonConfig cc = new ComparisonConfig() { CustomdataKeysToConsider = { "Gnappi" } };
            Assert.IsTrue(node1.Hash(cc) == node2.Hash(cc), "The two objects should have been seen as equal, despite differences in some of their CustomData.");

            // Set CustomdataKeysToInclude to consider only changes in the CustomData that changed.
            cc = new ComparisonConfig() { CustomdataKeysToConsider = { "Zurli" } };
            Assert.IsTrue(node1.Hash(cc) != node2.Hash(cc), "By considering a specific CustomData that was different between two objects, the two objects should have been seen as different.");

        }

        [TestMethod]
        public void CustomDataToExclude_DifferentObjects()
        {
            // Create one node with a few properties and some CustomData.
            Node node1 = BH.Engine.Structure.Create.Node(new Point() { X = 0, Y = 0, Z = 0 });
            node1.CustomData.Add("Gnappi", 1);
            node1.CustomData.Add("Zurli", 9999);

            // Create another node with same properties but 1 different CustomData.
            Node node2 = BH.Engine.Structure.Create.Node(new Point() { X = 0, Y = 0, Z = 0 });
            node2.CustomData.Add("Gnappi", 1);
            node2.CustomData.Add("Zurli", 0); // different value

            // Set PropertiesToConsider to ignore changes in the CustomData that is different.
            ComparisonConfig cc = new ComparisonConfig() { CustomdataKeysExceptions = { "Zurli" } };
            Assert.IsTrue(node1.Hash(cc) == node2.Hash(cc));
        }

        [TestMethod]
        public void TypeExceptions_DifferentObjects_SeenAsEqual()
        {
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
            Assert.IsTrue(bar1.Hash(cc) == bar2.Hash(cc));
        }

        [TestMethod]
        public void PropertiesToConsider_TopLevelProperty_EqualObjects()
        {
            // Create one testObject.
            TestObject node1 = new TestObject()
            {
                Location = new Point() { X = 0, Y = 0, Z = 0 }
            };

            // Create another testObject with different Location.
            TestObject node2 = new TestObject()
            {
                Location = new Point() { X = 0, Y = 0, Z = 99 }
            };

            // Set PropertiesToConsider to consider only "Location" properties.
            ComparisonConfig cc = new ComparisonConfig() { PropertiesToConsider = { "Location" } };

            Assert.IsTrue(node1.Hash(cc) != node2.Hash(cc), "Objects must be recognised as different via their hash.");

            // Create another node equal to node1 but with the Name changed.
            TestObject node3 = node1.DeepClone();
            node3.Name = "Node3"; // the name is the only thing that distinguishes node3 from node1

            Assert.IsTrue(node1.Hash(cc) == node3.Hash(cc), "Objects must be recognised as the same via their hash."); // although the name is different, they must be recongnised as the same.
        }

        [TestMethod]
        public void PropertiesToConsider_SubProperties_EqualObjects()
        {
            // Create one node
            Node node1 = BH.Engine.Structure.Create.Node(new Point() { X = 0, Y = 0, Z = 0 });

            // Create another node similar to node1 but with the name changed.
            Node node1_diffName = node1.DeepClone();
            node1_diffName.Name = "node1_diffName";

            // Create another node with different coordinates
            Node node2 = BH.Engine.Structure.Create.Node(new Point() { X = 0, Y = 0, Z = 50 });
            node2.Name = "node2";

            // Create another node similar to node2 but with the name changed.
            Node node2_diffName = node2.DeepClone();
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
            Assert.IsTrue(bar1.Hash(cc_onlyEndNodePosition) == bar2.Hash(cc_onlyEndNodePosition));

            // By looking only at EndNode.Position, and StartNode.Position, bars should be the same.
            ComparisonConfig cc_onlyStartNodePosition = new ComparisonConfig() { PropertiesToConsider = { "EndNode.Position", "StartNode.Position" } };
            Assert.IsTrue(bar1.Hash(cc_onlyStartNodePosition) == bar2.Hash(cc_onlyStartNodePosition));

            // By looking only at EndNode.Name, bars should be the different.
            ComparisonConfig cc_onlyEndNodeName = new ComparisonConfig() { PropertiesToConsider = { "EndNode.Name" } };
            Assert.IsTrue(bar1.Hash(cc_onlyEndNodeName) != bar2.Hash(cc_onlyEndNodeName));

            // By looking only at Name, bars should be the different. Note: this only checks the Bar.Name, and will not consider checking pairs of subproperties' names,
            // e.g. it will not look if node1.StartNode.Name is equal or different to node1_diffName.StartNode.Name.
            // In other words, this stops at the topmost matching property.
            ComparisonConfig cc_onlyName = new ComparisonConfig() { PropertiesToConsider = { "Name" } };
            Assert.IsTrue(bar1.Hash(cc_onlyName) != bar2.Hash(cc_onlyName));

            // By default, the bars should be seen as different.
            Assert.IsTrue(bar1.Hash() != bar2.Hash());
        }

        [TestMethod]
        public void PropertiesToConsider_InterfaceSubProperties_EqualObjects()
        {
            // Create one testObject.
            TestObject node1 = new TestObject()
            {
                Location = new Point() { X = 0, Y = 0, Z = 0 }
            };

            // Create another testObject with different Location.
            TestObject node2 = new TestObject()
            {
                Location = new Point() { X = 0, Y = 0, Z = 99 }
            };

            // Set PropertiesToConsider to consider only "Location.Z" properties.
            ComparisonConfig cc = new ComparisonConfig() { PropertiesToConsider = { "Location.Z" } };
            Assert.IsTrue(node1.Hash(cc) != node2.Hash(cc), "Objects must be recognised as different via their hash.");

            // Set PropertiesToConsider to consider only "Z" properties.
            cc = new ComparisonConfig() { PropertiesToConsider = { "Z" } };
            Assert.IsTrue(node1.Hash(cc) != node2.Hash(cc), "Objects must be recognised as different via their hash.");

            // Create another node equal to node1 but with the Name changed.
            TestObject node3 = node1.DeepClone();
            node3.Name = "Node3"; // the name is the only thing that distinguishes node3 from node1

            Assert.IsTrue(node1.Hash(cc) == node3.Hash(cc), "Objects must be recognised as the same via their hash."); // although the name is different, they must be recongnised as the same.
        }

        [TestMethod]
        public void PropertiesToConsider_NonExistingPropertyToConsider_Equal()
        {
            Bar bar1 = Engine.Diffing.Tests.Create.RandomObject<Bar>();
            Bar bar2 = Engine.Diffing.Tests.Create.RandomObject<Bar>();

            ComparisonConfig cc = new ComparisonConfig()
            {
                PropertiesToConsider = new List<string>() { "SomeRandomNotExistingPropertyName" }
            };

            Assert.IsTrue(bar1.Hash(cc) == bar2.Hash(cc), "Objects should be seen as equal.");
        }

        [TestMethod]
        public void PropertiesToConsider_FullPropertyNames_EqualObjects()
        {
            // Create one node
            Node node1 = BH.Engine.Structure.Create.Node(new Point() { X = 0, Y = 0, Z = 0 });

            // Create another node similar to node1 but with the name changed.
            Node node1_diffName = node1.DeepClone();
            node1_diffName.Name = "node1_diffName";

            // Create another node with different coordinates
            Node node2 = BH.Engine.Structure.Create.Node(new Point() { X = 0, Y = 0, Z = 50 });
            node2.Name = "node2";

            // Create another node similar to node2 but with the name changed.
            Node node2_diffName = node2.DeepClone();
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
            Assert.IsTrue(bar1.Hash(cc_onlyEndNodePosition) == bar2.Hash(cc_onlyEndNodePosition));

            // By looking only at EndNode.Position, and StartNode.Position, bars should be the same.
            ComparisonConfig cc_onlyStartNodePosition = new ComparisonConfig() { PropertiesToConsider = { "BH.oM.Structure.Elements.Bar.EndNode.Position", "BH.oM.Structure.Elements.Bar.StartNode.Position" } };
            Assert.IsTrue(bar1.Hash(cc_onlyStartNodePosition) == bar2.Hash(cc_onlyStartNodePosition));

            // By looking only at EndNode.Name, bars should be the different.
            ComparisonConfig cc_onlyEndNodeName = new ComparisonConfig() { PropertiesToConsider = { "BH.oM.Structure.Elements.Bar.EndNode.Name" } };
            Assert.IsTrue(bar1.Hash(cc_onlyEndNodeName) != bar2.Hash(cc_onlyEndNodeName));

            // By looking only at Name, bars should be the different. Note: this only checks the Bar.Name, and will not consider checking pairs of subproperties' names,
            // e.g. it will not look if node1.StartNode.Name is equal or different to node1_diffName.StartNode.Name.
            // In other words, this stops at the topmost matching property.
            ComparisonConfig cc_onlyName = new ComparisonConfig() { PropertiesToConsider = { "Name" } };
            Assert.IsTrue(bar1.Hash(cc_onlyName) != bar2.Hash(cc_onlyName));

            // By default, the bars should be seen as different.
            Assert.IsTrue(bar1.Hash() != bar2.Hash());
        }

        [TestMethod]
        public void PropertiesToConsider_PartialPropertyName_DifferentObjects()
        {
            // Create one node
            Node node1 = BH.Engine.Structure.Create.Node(new Point() { X = 0, Y = 0, Z = 0 });

            // Create another node similar to node1 but with the name changed.
            Node node1_diffName = node1.DeepClone();
            node1_diffName.Name = "node1_diffName";

            // Create another node with different coordinates
            Node node2 = BH.Engine.Structure.Create.Node(new Point() { X = 0, Y = 0, Z = 50 });
            node2.Name = "node2";

            // Create another node similar to node2 but with the name changed.
            Node node2_diffName = node2.DeepClone();
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
            Assert.IsTrue(bar1.Hash(cc) != bar2.Hash(cc));

            // Change bar names (the StartNode.Name in bar1/bar2 are still different!)
            bar1.Name = "bar1";
            bar2.Name = "bar1";

            // Bars should still be different as StartNode and Endnodes have different `Name`s
            Assert.IsTrue(bar1.Hash(cc) != bar2.Hash(cc));
        }

        [TestMethod]
        public void PropertiesToConsider_WildCardPrefix_DifferentObjects_SeenAsDifferent()
        {
            // Create one node
            Node node1 = BH.Engine.Structure.Create.Node(new Point() { X = 0, Y = 0, Z = 0 });

            // Create another node with different coordinates
            Node node2 = BH.Engine.Structure.Create.Node(new Point() { X = 0, Y = 0, Z = 50 });
            node2.Name = "node2";

            // Create another node equal to node2 but with the name changed.
            Node node2_diffName = node2.DeepClone();
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

            Assert.IsTrue(bar1.Hash(cc) != bar2.Hash(cc), "Bars should still be different: although the Bar names are the same, their StartNode have different `Name`s.");
        }

        [TestMethod]
        public void PropertiesToConsider_WildCardPrefix_DifferentObjects_SeenAsEqual()
        {
            // Create two parent Bars for the nodes
            Bar bar1 = new Bar();
            Bar bar2 = new Bar();

            bar1.Name = "bar1";
            bar2.Name = "bar1"; // SAME NAMES FOR BARS.

            // Create another node with different coordinates
            Node node1 = BH.Engine.Structure.Create.Node(new Point() { X = 0, Y = 0, Z = 50 });
            node1.Name = "node1";

            // Create another node with different coordinates but with the same name as node2.
            Node node2 = BH.Engine.Structure.Create.Node(new Point() { X = 100, Y = 100, Z = 100 });
            node2.Name = node1.Name; // SAME NAME AS NODE2

            bar1.EndNode = node1;
            bar2.EndNode = node2; // DIFFERENT END NODES, BUT WITH THE SAME NAME.

            // Equivalently, without the wildcard: `Name`
            ComparisonConfig cc = new ComparisonConfig() { PropertiesToConsider = { "Name" } };
            Assert.IsTrue(bar1.Hash(cc) == bar2.Hash(cc), "Bars should be seen as equal: although the End Nodes are different, their names are the same.");

            // Using Wildcard prefix (to capture all possible properties ending in Name).
            cc = new ComparisonConfig() { PropertiesToConsider = { "*.Name" } };
            Assert.IsTrue(bar1.Hash(cc) == bar2.Hash(cc), "Bars should be seen as equal: although the End Nodes are different, their names are the same.");
        }

        [TestMethod]
        public void PropertyExceptions_Wildcard_DifferentObjects_SeenAsEqual()
        {
            // Create one bar
            Node startNode = BH.Engine.Structure.Create.Node(new Point() { X = 0, Y = 0, Z = 0 });
            Node endNode = BH.Engine.Structure.Create.Node(new Point() { X = 0, Y = 0, Z = 1 });
            Bar bar1 = BH.Engine.Structure.Create.Bar(startNode, endNode);
            bar1.Name = "bar1";

            // Create another bar identical to the first
            Node startNode2 = BH.Engine.Structure.Create.Node(new Point() { X = 99, Y = 0, Z = 0 }); // note the X is different from bar1 nodes.
            Node endNode2 = BH.Engine.Structure.Create.Node(new Point() { X = 99, Y = 0, Z = 1 });
            Bar bar2 = BH.Engine.Structure.Create.Bar(startNode, endNode);
            bar2.Name = "bar2";

            // Set PropertiesToConsider to ignore changes in: `Bar.StartNode.X` and `Bar.EndNode.X`; `Name`.
            ComparisonConfig cc = new ComparisonConfig() { PropertyExceptions = { "Bar.*.Position.X", "Name" } };

            Assert.IsTrue(bar1.Hash(cc) == bar2.Hash(cc));
        }

        [TestMethod]
        public void NumericTolerance_DifferentObjects_SeenAsEqual()
        {
            // Set a numerical tolerance (different from the default value).
            ComparisonConfig cc = new ComparisonConfig() { NumericTolerance = 1E-3 };

            // Create one node.
            Node node1 = BH.Engine.Structure.Create.Node(new Point() { X = 0, Y = 0, Z = 0 });
            string hash1 = node1.Hash(cc);

            // Create another node with similar coordinates. The difference should be so minimal that is ignored by the tolerance.
            Node node2 = BH.Engine.Structure.Create.Node(new Point() { X = 0, Y = 0, Z = 0.0005 });
            string hash2 = node2.Hash(cc);

            // Make sure the hashes are the same, thanks to the numeric tolerance.
            Assert.IsTrue(hash1 == hash2);
        }

        [TestMethod]
        public void SignificantFigures_DifferentObjects_SeenAsEqual()
        {
            // Set a numerical SignificantFigures (different from the default value).
            ComparisonConfig cc = new ComparisonConfig() { SignificantFigures = 3 };

            // Create one node.
            Node node1 = BH.Engine.Structure.Create.Node(new Point() { X = 0, Y = 0, Z = 35.06 }); // this should be rounded to 35.1.
            string hash1 = node1.Hash(cc);

            // Create another node with similar coordinates. The difference should be so minimal that is ignored by the SignificantFigures.
            Node node2 = BH.Engine.Structure.Create.Node(new Point() { X = 0, Y = 0, Z = 35.1 });
            string hash2 = node2.Hash(cc);

            // Make sure the hashes are the same, thanks to the numeric tolerance.
            Assert.IsTrue(hash1 == hash2);
        }

        [TestMethod]
        public void NumericTolerance_DifferentObjects_SeenAsDifferent()
        {
            // Set a numerical tolerance (different from the default value).
            ComparisonConfig cc = new ComparisonConfig() { NumericTolerance = 1E-3 };

            // Create one node.
            Node node1 = BH.Engine.Structure.Create.Node(new Point() { X = 0, Y = 0, Z = 0 });
            string hash1 = node1.Hash(cc);

            // Create another node with similar coordinates that should be considered as different by precision
            Node node2 = BH.Engine.Structure.Create.Node(new Point() { X = 0, Y = 0, Z = 0.005 });
            string hash2 = node2.Hash(cc);

            // Make sure the hashes are different, following the numeric tolerance.
            Assert.IsTrue(hash1 != hash2);
        }

        [TestMethod]
        public void SignificantFigures_DifferentObjects_SeenAsDifferent()
        {
            // Set a numerical SignificantFigures (different from the default value).
            ComparisonConfig cc = new ComparisonConfig() { SignificantFigures = 3 };

            // Create one node.
            Node node1 = BH.Engine.Structure.Create.Node(new Point() { X = 0, Y = 0, Z = 35.04 }); // this should be rounded to 35.0.
            string hash1 = node1.Hash(cc);

            // Create another node with similar coordinates. The difference should be so minimal that is ignored by the SignificantFigures.
            Node node2 = BH.Engine.Structure.Create.Node(new Point() { X = 0, Y = 0, Z = 35.12 }); // this should be rounded to 35.1.
            string hash2 = node2.Hash(cc);

            // Make sure the hashes are different, following the numeric tolerance.
            Assert.IsTrue(hash1 != hash2);
        }

        [TestMethod]
        public void PropertyNumericTolerance_DifferentObjects_IncreasingTolerance()
        {
            ComparisonConfig cc = new ComparisonConfig();

            // Create two objects.
            Node node1 = BH.Engine.Structure.Create.Node(new Point() { X = 0, Y = 0, Z = 0 });
            Node node2 = BH.Engine.Structure.Create.Node(new Point() { X = 0.0003, Y = 0.0003, Z = 0.9 }); // X and Y have very small variation, Z has large variation.

            // With the default tolerance, the objects must be seen as different.
            Assert.IsTrue(node1.Hash(cc) != node2.Hash(cc));

            // Set the global tolerance to be larger than X and Y, but smaller than Z.
            // The two objects must still be seen as different just because of the Z property large variation.
            cc.NumericTolerance = 1e-1;
            Assert.IsTrue(node1.Hash(cc) != node2.Hash(cc));

            // Set the global tolerance to be larger than X, Y and Z.
            // The two objects must be seen as equal.
            cc.NumericTolerance = 1;
            Assert.IsTrue(node1.Hash(cc) == node2.Hash(cc));
        }

        [TestMethod]
        public void SignificantFigures_DifferentObjects_IncreasingFigures()
        {
            ComparisonConfig cc = new ComparisonConfig();

            // Create two objects.
            Node node1 = BH.Engine.Structure.Create.Node(new Point() { X = 0.2, Y = 0.2, Z = 1 });
            Node node2 = BH.Engine.Structure.Create.Node(new Point() { X = 0.169, Y = 0.169, Z = 1.01 });

            // With the default significant figures, the objects must be seen as different.
            Assert.IsTrue(node1.Hash(cc) != node2.Hash(cc));

            // Set the global significant figures so we disregard the variations of X and Y, but not Z. The objects must still be seen as different.
            cc.SignificantFigures = 3;
            Assert.IsTrue(node1.Hash(cc) != node2.Hash(cc));

            // Set the global significant figures so that we disregard all variations.
            // The two objects must be seen as equal.
            cc.SignificantFigures = 1;
            Assert.IsTrue(node1.Hash(cc) == node2.Hash(cc));
        }

        [TestMethod]
        public void PropertyNumericTolerance_DifferentObjects_SeenAsEqual()
        {
            ComparisonConfig cc = new ComparisonConfig();

            // Create two objects.
            Node node1 = BH.Engine.Structure.Create.Node(new Point() { X = 0, Y = 0, Z = 0 });
            Node node2 = BH.Engine.Structure.Create.Node(new Point() { X = 0.0003, Y = 0.0003, Z = 0.9 }); // X and Y have very small variation, Z has large variation.

            // With the default tolerance, the objects must be seen as different.
            Assert.IsTrue(node1.Hash(cc) != node2.Hash(cc));

            // Set the global tolerance to be larger than X and Y, but smaller than Z.
            // The two objects must still be seen as different just because of the Z property large variation.
            cc.NumericTolerance = 1e-1;
            Assert.IsTrue(node1.Hash(cc) != node2.Hash(cc));

            // Set the global tolerance to be larger than X, Y and Z.
            // The two objects must be seen as equal.
            cc.NumericTolerance = 1;
            Assert.IsTrue(node1.Hash(cc) == node2.Hash(cc));

            // Set a custom tolerance for just the variable Z, to be smaller than Z's updated value.
            // The two objects must again be seen as different.
            cc.PropertyNumericTolerances.Add(new NamedNumericTolerance() { Name = "*.Z", Tolerance = 1e-3 });
            Assert.IsTrue(node1.Hash(cc) != node2.Hash(cc));
        }

        [TestMethod]
        public void PropertySignificantFigures_DifferentObjects_SeenAsEqual()
        {
            ComparisonConfig cc = new ComparisonConfig();

            // Create two objects.
            Node node1 = BH.Engine.Structure.Create.Node(new Point() { X = 0.2, Y = 0.2, Z = 1 });
            Node node2 = BH.Engine.Structure.Create.Node(new Point() { X = 0.169, Y = 0.169, Z = 1.01 });

            // With the default significant figures, the objects must be seen as different.
            Assert.IsTrue(node1.Hash(cc) != node2.Hash(cc));

            // Set the global significant figures to be larger than X and Y, but smaller than Z.
            // The two objects must still be seen as different just because of the Z property large variation.
            cc.SignificantFigures = 3;
            Assert.IsTrue(node1.Hash(cc) != node2.Hash(cc));

            // Set the global significant figures so the two objects must be seen as equal.
            cc.SignificantFigures = 1;
            Assert.IsTrue(node1.Hash(cc) == node2.Hash(cc));

            // Set a custom significant figures for just the variable Z, to be smaller than Z's updated value.
            // The two objects must again be seen as different, because this overcomes the global significant figures settings.
            cc.PropertySignificantFigures.Add(new NamedSignificantFigures() { Name = "*.Z", SignificantFigures = 3 });
            Assert.IsTrue(node1.Hash(cc) != node2.Hash(cc));
        }

        [TestMethod]
        public void SignificantFiguresAndNumericTolerance_DifferentObjects_SeenAsEqual()
        {
            // Tests together the Numeric Tolerance and Significant Figures settings in the same ComparisonConfig.
            // When set together, the number returned will be the most approximate (least precise).

            ComparisonConfig cc = new ComparisonConfig();

            // Create two objects.
            Node controlNode;
            Node node = BH.Engine.Structure.Create.Node(new Point() { X = 0.169, Y = 0.169, Z = 1.01 });

            // Set Numerical tolerance.
            cc.NumericTolerance = 1e-2; // This will round as: X = 0.17, Y = 0.17, Z = 1.01.
            controlNode = BH.Engine.Structure.Create.Node(new Point() { X = 0.17, Y = 0.17, Z = 1.01 });

            Assert.IsTrue(controlNode.Hash(cc) == node.Hash(cc));

            // Also set SignificantFigures in the same ComparisonConfig.
            cc.SignificantFigures = 1; // This will round as: X = 0.2, Y = 0.2, Z = 1.
            controlNode = BH.Engine.Structure.Create.Node(new Point() { X = 0.2, Y = 0.2, Z = 1 });

            Assert.IsTrue(controlNode.Hash(cc) == node.Hash(cc));
        }

        [TestMethod]
        [DataRow(false)] // Setting this to true updates the serialized object.
        public void SerialisedObject_RandomObject_HashDidNotChange(bool resetSerialisedObject = false)
        {
            string filePath = Path.GetFullPath(Path.Combine(System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location), @"..\..\..\..\Datasets\211117_HashTest_SerialisedObject_RandomObject.json"));

            Assert.IsTrue(filePath.IsValidFilePath(), $"Check that the filepath for the serialised object is valid and that the file can be found on disk: {filePath}.");

            Bar bar = null;
            ComparisonConfig cc = new ComparisonConfig();

            // Set newtonsoft serialization settings to handle automatically any type.
            JsonSerializerSettings settings = new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.Auto };

            if (resetSerialisedObject)
            {
                bar = BH.Engine.Base.Create.RandomObject(typeof(Bar)) as Bar;
                string generatedObjectHash = bar.Hash(cc);

                // Store the computed Hash of the object into a HashFragment.
                // This way, when the object is read, we can re-compute the Hash and check it against this stored version.
                // (The HashFragment is always ignored when computing a Hash.)
                bar = bar.SetHashFragment(generatedObjectHash);

                System.IO.File.WriteAllText(filePath, JsonConvert.SerializeObject(bar, settings));
            }

            if (!resetSerialisedObject)
            {
                // deserialize JSON directly from a file
                using (StreamReader file = File.OpenText(filePath))
                    bar = JsonConvert.DeserializeObject<Bar>(file.ReadToEnd(), settings);
            }

            // Compute the hash of the object that was serialised.
            // (Any existing HashFragment is always ignored when computing the Hash)
            string currentHash = bar.Hash(cc);

            // The object was serialised with a Hash stored in its HashFragment.
            // Get it so we can compare it with the currentHash.
            string hashStoredInSerialisedObject = bar.FindFragment<HashFragment>().Hash;

            Assert.IsTrue(hashStoredInSerialisedObject == currentHash, "The hash for the same Bar object has changed.");
        }
    }
}
