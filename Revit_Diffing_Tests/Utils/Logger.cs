using BH.oM.Base;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Collections;
using BH.oM.Diffing;

namespace Tests
{
    static class Logger
    {
        public static void Log(object objectsToLog, string fileSuffix, LogOptions logOpt = LogOptions.HashesOnly)
        {
            // Get call stack and calling method name
            StackTrace stackTrace = new StackTrace();
            string testName = stackTrace.GetFrame(1).GetMethod().Name;

            long dateTimeTicks = DateTime.UtcNow.Ticks;
            System.IO.Directory.CreateDirectory(@"C:\temp\BHoM_Tests\Diffing_Engine\");


            // Wrap single object in list to handle it in the same way
            IList allObjs = new List<object>();
            if (!(objectsToLog is IEnumerable) && !(objectsToLog is string))
                allObjs = new List<object>() { objectsToLog };
            else
                allObjs = (IList)objectsToLog;

            string objsToLog_serialised = JsonConvert.SerializeObject(allObjs, Formatting.Indented);

            if (logOpt == LogOptions.ObjectsOnly || logOpt == LogOptions.ObjectsAndHashes)
            {
                // Log objects
                System.IO.File.WriteAllText($@"C:\temp\BHoM_Tests\Diffing_Engine\{testName}_objs-{dateTimeTicks}-{fileSuffix}.json", objsToLog_serialised);
            }

            if (logOpt == LogOptions.HashesOnly || logOpt == LogOptions.ObjectsAndHashes)
            {
                // Log hashes
                var bhoMObjects = allObjs.OfType<IBHoMObject>();
                var objsWithFragments = allObjs.OfType<IBHoMObject>().Where(obj => obj.Fragments.Contains(typeof(HashFragment)));

                if (objsWithFragments.Count() > allObjs.OfType<IBHoMObject>().Count())
                    throw new ArgumentOutOfRangeException("Some object have too many HashFragments assigned.");

                // Extract HashFragments
                List<HashFragment> HashFragments = allObjs.OfType<IBHoMObject>().SelectMany(obj => obj.Fragments.OfType<HashFragment>()).ToList();

                if (HashFragments.Count > 0 && HashFragments.Count == allObjs.OfType<IBHoMObject>().Count())
                {
                    // All input BHoMObjects already had HashFragments
                    string objs_hashes = JsonConvert.SerializeObject(HashFragments, Formatting.Indented);
                    System.IO.File.WriteAllText($@"C:\temp\BHoM_Tests\Diffing_Engine\{testName}_objs-{dateTimeTicks}-{fileSuffix}-hashes.json", objs_hashes);

                    return;
                }
                else
                {
                    throw new ArgumentException("Not all input objects have HashFragment assigned.");
                }

            }
        }
    }

    public enum LogOptions
    {
        HashesOnly = 1,
        ObjectsOnly = 2,
        ObjectsAndHashes = 3
    }
}