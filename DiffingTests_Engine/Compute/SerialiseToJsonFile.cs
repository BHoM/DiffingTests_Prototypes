using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BH.Engine.Diffing.Tests
{
    public static partial class Compute
    {
        public static bool SerialiseToJsonFile(List<object> obj, string filePath)
        {
            if (obj == null)
                return false;

            if (obj.Count() > 1)
                return SerialiseAndWriteJson(obj, filePath);
            else
                return SerialiseAndWriteJson(obj.FirstOrDefault(), filePath);
        }

        private static bool SerialiseAndWriteJson(object obj, string filePath)
        {
            if (!filePath.IsValidFilePath())
                return false;

            try
            {
                JsonSerializerSettings settings = new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.Auto };
                System.IO.File.WriteAllText(filePath, JsonConvert.SerializeObject(obj, settings));
            }
            catch (Exception e)
            {
                BH.Engine.Reflection.Compute.RecordError($"Error serialising or writing to disk:\n\t{e.Message}");
                return false;
            }

            return true;
        }

    }
}
