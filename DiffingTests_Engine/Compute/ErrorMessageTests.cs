/*
 * This file is part of the Buildings and Habitats object Model (BHoM)
 * Copyright (c) 2015 - 2022, the respective contributors. All rights reserved.
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

namespace BH.Engine.Diffing.Tests
{
    public static partial class Compute
    {
        public static void ErrorTest1()
        {
            BH.Engine.Reflection.Compute.RecordError("Error");
            BH.Engine.Reflection.Compute.RecordWarning("Warning");
            BH.Engine.Reflection.Compute.RecordNote("Note");
        }

        public static void ErrorTest2()
        {
            BH.Engine.Reflection.Compute.RecordWarning("Warning");
            BH.Engine.Reflection.Compute.RecordError("Error");
            BH.Engine.Reflection.Compute.RecordNote("Note");
        }

        public static void ErrorTest3()
        {
            BH.Engine.Reflection.Compute.RecordNote("Note");
            BH.Engine.Reflection.Compute.RecordError("Error");
            BH.Engine.Reflection.Compute.RecordWarning("Warning");
        }

        public static void ErrorTest4()
        {
            BH.Engine.Reflection.Compute.RecordNote("Note");
            BH.Engine.Reflection.Compute.RecordWarning("Warning");
            BH.Engine.Reflection.Compute.RecordError("Error");
        }

        public static void ErrorTest5()
        {
            BH.Engine.Reflection.Compute.RecordWarning("Warning");
            BH.Engine.Reflection.Compute.RecordNote("Note");
            BH.Engine.Reflection.Compute.RecordError("Error");
        }

        public static void WarningTest1()
        {
            BH.Engine.Reflection.Compute.RecordWarning("Warning");
            BH.Engine.Reflection.Compute.RecordNote("Note");
        }

        public static void WarningTest2()
        {
            BH.Engine.Reflection.Compute.RecordNote("Note");
            BH.Engine.Reflection.Compute.RecordWarning("Warning");
        }

        public static void NoteTest()
        {
            BH.Engine.Reflection.Compute.RecordNote("Note");
        }
    }
}

