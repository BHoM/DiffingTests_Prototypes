using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Utilities
{
    public static class CollectionUtilities
    {
        public static void ConcatenateDictionaryValues<K, L>(this Dictionary<K, List<L>> dict1, Dictionary<K, List<L>> dict2, bool distinct = false)
        {
            foreach (var kv2 in dict2)
            {
                List<L> v1 = null;
                if (dict1.TryGetValue(kv2.Key, out v1))
                {
                    if (v1 != null)
                        dict1[kv2.Key].AddRange(kv2.Value);
                }
                else
                    dict1[kv2.Key] = kv2.Value;
            }

            if (distinct)
            {
                Dictionary<K, List<L>> distinctResult = new Dictionary<K, List<L>>();
                foreach (var kv1 in dict1)
                {
                    distinctResult[kv1.Key] = kv1.Value.Distinct().ToList();
                }

                dict1 = distinctResult;
            }
        }
    }
}
