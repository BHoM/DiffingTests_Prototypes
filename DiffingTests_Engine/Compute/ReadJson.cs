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
        public static object ReadJson(string filePath)
        {
            if (!filePath.IsValidFilePath())
                return false;

            try
            {
                JsonSerializerSettings settings = new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.Auto };
                using (StreamReader file = File.OpenText(filePath))
                    return JsonConvert.DeserializeObject(file.ReadToEnd(), settings);
            }
            catch (Exception e)
            {
                BH.Engine.Reflection.Compute.RecordError($"Error deserialising or reading from disk:\n\t{e.Message}");
                return null;
            }
        }
    }
}
