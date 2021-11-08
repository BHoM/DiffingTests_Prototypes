using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BH.Engine.Tests
{
    public static class Compute
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
