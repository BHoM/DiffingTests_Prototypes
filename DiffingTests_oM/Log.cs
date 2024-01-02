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
using System.IO;
using System.Text;

namespace BH.oM.Diffing.Tests
{
    public static class Log
    {
        private static List<string> m_allMessages = new List<string>();
        private static HashSet<string> m_reportedErrors = new HashSet<string>();
        private static HashSet<string> m_reportedWarnings = new HashSet<string>();
        private static HashSet<string> m_reportedNotes = new HashSet<string>();

        public static void RecordError(string error, bool doNotRepeat = false)
        {
            if (doNotRepeat && m_reportedErrors.Contains(error))
                return;

            Console.WriteLine($"ERROR: {error}");
            BH.Engine.Base.Compute.RecordError(error);

            if (doNotRepeat)
                m_reportedErrors.Add(error);

            m_allMessages.Add($"{DateTime.Now}\tERROR: {error}");
        }

        public static void RecordWarning(string warning, bool doNotRepeat = false)
        {
            if (doNotRepeat && m_reportedWarnings.Contains(warning))
                return;

            Console.WriteLine($"Warning: {warning}");
            BH.Engine.Base.Compute.RecordWarning(warning);

            if (doNotRepeat)
                m_reportedWarnings.Add(warning);

            m_allMessages.Add($"{DateTime.Now}\tWarning: {warning}");
        }

        public static void RecordNote(string note, bool doNotRepeat = false)
        {
            if (doNotRepeat && m_reportedNotes.Contains(note))
                return;

            Console.WriteLine(note);
            BH.Engine.Base.Compute.RecordNote(note);

            if (doNotRepeat)
                m_reportedWarnings.Add(note);

            m_allMessages.Add($"{DateTime.Now}\t{note}");
        }

        public static void SaveLogToDisk(string filePath)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(filePath));
            File.WriteAllLines(filePath, m_allMessages);
        }
    }
}

