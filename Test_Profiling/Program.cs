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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tests
{
    class Program
    {
        public static void Main(string[] args = null)
        {
            Console.WriteLine("\n/**********************************/");
            Console.WriteLine("\n\t\t\tDIFFING TESTS.");
            Console.WriteLine("Note: the first time tests are run, the computation time might be slower than in reality due to first loading." +
                "\nOptionally, try rerunning the tests when prompted." +
                "\nOtherwise, the subsequent PROFILING will give an accurate performance measure.");
            Console.WriteLine("\n/**********************************/");

            /// ************************************/
            /// Diffing tests
            /// ************************************/

            DiffingTests.HashTest_CostantHash_IdenticalObjs();

            DiffingTests.HashTest_CostantHash_NumericalPrecision();

            DiffingTests.HashTest_HashComparer();

            DiffingTests.HashTest_RemoveDuplicatesByHash();

            DiffingTests.HashTest_CustomDataToConsider_equalObjects();

            DiffingTests.HashTest_CustomDataToConsider_differentObjects();

            DiffingTests.HashTest_CustomDataToExclude_equalObjects();

            DiffingTests.TypeExceptions();

            DiffingTests.HashTest_PropertiesToConsider();

            DiffingTests.HashTest_PropertiesToConsider_subProps();

            DiffingTests.HashTest_PropertiesToConsider_samePropertyNameAtMultipleLevels();

            DiffingTests.HashTest_PropertyExceptions();

            DiffingTests.HashTest_CheckAgainstStoredHash();

            DiffingTests.RevisionTest_CostantHash_IdenticalObjs();

            DiffingTests.RevisionTest_UnchangedObjectsSameHash();

            DiffingTests.RevisionTest_basic();

            DiffingTests.RevisionTest_advanced();

            DiffingTests.IDiffingTest_HashDiffing();

            DiffingTests.DiffWithFragmentId_allDifferent();

            DiffingTests.DiffWithFragmentId_allEqual();

            //RevitDiffing.RevitDiffing_basic();


            /// ************************************/

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

            /// ************************************/
            /// Diffing profiling
            /// ************************************/

            DiffingTests.Profiling();

            /// ************************************/


            Console.WriteLine("Press `Enter` to repeat all / any other key to close.");

            keyInfo = Console.ReadKey(true);
            if (keyInfo.Key == ConsoleKey.Enter)
                Main();

        }
    }
}
