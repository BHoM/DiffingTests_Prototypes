using BH.oM.Base;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace BH.Engine.Diffing.Tests
{
    public static partial class Query
    {
        public static void Dump(this object obj)
        {
            Console.WriteLine(obj.ToString());
        }
    }
}
