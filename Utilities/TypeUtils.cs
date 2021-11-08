using BH.oM.Base;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Utilities
{
    public static class TypeUtils
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

        public static bool IsBHoMSubtype(this Type t)
        {
            return t.IsClass && (typeof(BHoMObject).IsSubclassOf(t) || typeof(IObject).IsAssignableFrom(t)) && t != typeof(BHoMObject);
        }

        public static Type[] TryGetTypes(this Assembly a)
        {
            try
            {
                return a.GetTypes();
            }
            catch (ReflectionTypeLoadException e )
            {
                Console.WriteLine($"Could not get types for assembly {a.GetName().Name}. Exception:\n {string.Join("\n ", e.LoaderExceptions.Select(le => le.Message).Distinct())}");
            }

            return new Type[] { };
        }
    }
}
