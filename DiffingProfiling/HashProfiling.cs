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
using System.IO;
using System.Security.Cryptography;
using BH.Engine.Serialiser;
using BH.oM.Diffing;
using System.Diagnostics;
using BH.Engine.Base.Objects;
using System.Collections;
using BH.Engine.Base;

namespace BH.Tests.Diffing
{
    public static partial class HashProfiling
    {
        public static void HashObjects(int totalObjsPerType = 5000, List<Type> types = null, BaseComparisonConfig comparisonConfig = null)
        {
            var asteriskRow = $"\n{string.Join("", Enumerable.Repeat("*", 50))}";

            types = types ?? new List<Type>() { typeof(Bar), typeof(NurbsCurve) };
            string introMessage = $"{asteriskRow}" +
                $"\nHashing {totalObjsPerType} randomly generated object of types:" +
                $"\n{string.Join(", ", types.Select(t => t.Name))}" +
                $"\nfor a total of {totalObjsPerType * types.Count} objects." +
                $"{asteriskRow}";
        
            Console.WriteLine(introMessage);

            comparisonConfig = comparisonConfig ?? new ComparisonConfig();

            Stopwatch totalSw = new Stopwatch();
            totalSw.Start();

            // Generate random objects
            List<IObject> objects = new List<IObject>();
            types.ForEach(t => objects.AddRange(BH.Engine.Diffing.Tests.Create.RandomIObjects(t, totalObjsPerType)));
            totalSw.Stop();
            Console.WriteLine($"\nObject creation time: {totalSw.ElapsedMilliseconds}ms");

            totalSw.Restart();
            objects.ForEach(o => o.Hash());
            totalSw.Stop();
            Console.WriteLine($"\nObject hashing time: {totalSw.ElapsedMilliseconds}ms");
        }
    }
}
