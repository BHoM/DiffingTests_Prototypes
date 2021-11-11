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
        public static Type GetInnermostType<T>(T obj)
        {
            Type type = typeof(T);

            if (type == typeof(System.Type) && obj is Type && obj != null)
                type = obj as Type;

            if (type.IsGenericType)
            {
                var genericArgs = type.GetGenericArguments();
                if (genericArgs.Length == 1)
                    return GetInnermostType(genericArgs[0]);
            }
            else
            {
                if (obj is IEnumerable)
                {
                    return GetInnermostType(
                        obj.GetType()
                        .GetInterfaces()
                        .Where(t => t.IsGenericType
                            && t.GetGenericTypeDefinition() == typeof(IEnumerable<>))
                        .Select(t => t.GetGenericArguments()[0]).FirstOrDefault());
                }
            }

            return type;
        }
    }
}