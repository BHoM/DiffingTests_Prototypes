/*
 * This file is part of the Buildings and Habitats object Model (BHoM)
 * Copyright (c) 2015 - 2024, the respective contributors. All rights reserved.
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
            /// ***************************************************************************/
            ///                                PROFILING
            /// Performance profiling.
            /// ***************************************************************************/

            var asteriskRow = $"\n{string.Join("", Enumerable.Repeat("*", 50))}";

            try
            {
                HashProfiling.HashObjects();
            }
            catch (Exception e)
            {
                Console.WriteLine($"\n\t\tERROR:\n\t\t{e.ToString()}");
            }

            Console.WriteLine(asteriskRow);
            Console.WriteLine("Press `Enter` to repeat HashProfiling / any other key to continue.");
            Console.WriteLine(asteriskRow);

            var keyInfo = Console.ReadKey(true);
            if (keyInfo.Key == ConsoleKey.Enter)
                Main();

            try
            {
                DiffingProfiling.GeneralProfiling();
            } 
            catch (Exception e)
            {
                Console.WriteLine($"\n\t\tERROR:\n\t\t{e.ToString()}");
            }

            Console.WriteLine(asteriskRow);
            Console.WriteLine("Press `Enter` to repeat all / any other key to close.");
            Console.WriteLine(asteriskRow);

            keyInfo = Console.ReadKey(true);
            if (keyInfo.Key == ConsoleKey.Enter)
                Main();
        }
    }
}



