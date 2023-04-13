using BH.oM.Diffing;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BH.Test.Engine.Diffing
{
    public static partial class Query
    {
        public static string ToText(this ObjectDifferences objectDifferences) 
        {
            if (objectDifferences == null)
                return "";

            return Newtonsoft.Json.JsonConvert.SerializeObject(
                objectDifferences.Differences
                    .Select(d => $"{d.FullName} went from `{d.PastValue}` to `{d.FollowingValue}`. Description: {d.Description}"), Formatting.Indented);
        }

        public static string ToText(this IEnumerable<ObjectDifferences> objectDifferencesList)
        {
            if (objectDifferencesList == null)
                return "";

            return string.Join(",\n\n", objectDifferencesList.Select(d => d.ToText()));
        }
    }
}
