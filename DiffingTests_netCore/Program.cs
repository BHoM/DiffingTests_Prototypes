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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BH.Tests.Diffing
{
    class Program
    {
        public static void Main(string[] args = null)
        {
            Console.WriteLine("\n/**********************************/");
            Console.WriteLine("\n\t\t\tDIFFING TESTS.");
            Console.WriteLine("Note: the first time tests are run, the computation time might be slower due to first assembly loading." +
                "\nOptionally, try rerunning the tests when prompted." +
                "\nOtherwise, the subsequent PROFILING will give an accurate performance measure.");
            Console.WriteLine("\n/**********************************/");

            /// ***************************************************************************/
            ///                                 HASH TESTS
            /// Tests on the Hash computation.
            /// ***************************************************************************/

            HashTests.EqualObjectsHaveSameHash();

            HashTests.NumericTolerance_SameHash();

            HashTests.NumericTolerance_DifferentHash();

            HashTests.HashComparer_AssignHashToFragments();

            HashTests.RemoveDuplicatesByHash();

            HashTests.CustomDataToConsider_EqualObjects();

            HashTests.CustomDataToConsider_DifferentObjects();

            HashTests.CustomDataToExclude_EqualObjects();

            HashTests.TypeExceptions();

            HashTests.PropertiesToConsider_TopLevelProperty_EqualObjects();

            HashTests.PropertiesToConsider_SubProperties_EqualObjects();

            HashTests.PropertiesToConsider_FullPropertyNames_EqualObjects();

            HashTests.PropertiesToConsider_PartialPropertyName_Unsupported();

            HashTests.PropertiesToConsider_WildCards_Unsupported();

            HashTests.PropertyExceptions_EqualObjects();

            HashTests.CheckAgainstSerialisedObject();


            /// ***************************************************************************/
            ///                              DIFFING TESTS
            /// Tests on the Diffing methods.
            /// ***************************************************************************/

            DiffingTests.IDiffing_HashDiffing();

            DiffingTests.DiffWithFragmentId_allModifiedObjects();

            DiffingTests.DiffWithFragmentId_allUnchangedObjects();

            DiffingTests.ObjectDifferences_DifferentBars();

            DiffingTests.DifferentProperties_DifferentBars();

            DiffingTests.ObjectDifferences_PropertiesToInclude_FullName_Equals();

            DiffingTests.ObjectDifferences_PropertiesToInclude_PartialName_Equals();

            DiffingTests.ObjectDifferences_PropertiesToInclude_WildCardPrefix_Equals();

            DiffingTests.ObjectDifferences_PropertiesToInclude_WildCardMiddle_Equals();

            DiffingTests.ObjectDifferences_PropertiesExceptions_Equals();

            //RevitDiffing.RevitDiffing_basic();

            /// ***************************************************************************/
            ///                              REVISION TESTS
            /// Tests related to the revision workflow (part of AECDeltas; not really used)
            /// ***************************************************************************/

            RevisionTests.CostantHash_IdenticalObjs();

            RevisionTests.UnchangedObjectsSameHash();

            RevisionTests.RevisionWorkflow_basic();

            RevisionTests.RevisionWorkflow_advanced();

            /// ***************************************************************************/

            Console.WriteLine("\n/**********************************/");
            string userInputRequiredMessage = "\nPress `SpaceBar` to repeat tests. `Enter` to continue on Profiling. `Esc` to exit.";
            Console.WriteLine(userInputRequiredMessage);
            Console.WriteLine("\n/**********************************/");

            ConsoleKeyInfo keyInfo = Console.ReadKey(true);
            if (keyInfo.Key == ConsoleKey.Spacebar)
                Main();
            else if (keyInfo.Key == ConsoleKey.Escape)
                Console.WriteLine(userInputRequiredMessage);
            else if (keyInfo.Key == ConsoleKey.Escape)
                return;

            /// ***************************************************************************/
            ///                                PROFILING
            /// Performance profiling.
            /// ***************************************************************************/

            Profiling.Diffing_GeneralProfiling();

            Console.WriteLine("Press `Enter` to repeat all / any other key to close.");

            keyInfo = Console.ReadKey(true);
            if (keyInfo.Key == ConsoleKey.Enter)
                Main();

        }
    }
}
