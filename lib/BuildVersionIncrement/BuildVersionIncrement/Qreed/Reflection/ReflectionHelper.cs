using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;

namespace Qreed.Reflection
{
    /// <summary>
    /// Utility class for the Reflection namespace.
    /// </summary>
    public static class ReflectionHelper
    {
        /// <summary>
        /// Gets an assembly attribute from the given assembly.
        /// </summary>
        /// <typeparam name="T">The attribute.</typeparam>
        /// <param name="assembly">The assembly.</param>
        /// <returns>Returns <c>null</c> if the attribute wasn't found.</returns>
        public static T GetAssemblyAttribute<T>(Assembly assembly)
        {
            return (T)GetAssemblyAttribute(typeof(T), assembly);
        }

        /// <summary>
        /// Gets an assembly attribute from the given assembly.
        /// </summary>
        /// <param name="attributeType">Type of the attribute.</param>
        /// <param name="assembly">The assembly.</param>
        /// <returns>Returns <c>null</c> if the attribute wasn't found.</returns>
        public static object GetAssemblyAttribute(Type attributeType, Assembly assembly)
        {
            if (assembly == null)
                throw (new ArgumentNullException("assembly"));

            object[] attributes = assembly.GetCustomAttributes(attributeType, true);

            if (attributes == null || attributes.Length == 0)
                return null;

            return attributes[0];
        }

        /// <summary>
        /// Creates a list of types that derive from the specified type
        /// </summary>
        /// <param name="asm">The assembly to check.</param>
        /// <param name="baseType">Type of the base.</param>
        /// <param name="includeSelf">if set to <c>true</c> the list will contain the baseType if found.</param>
        /// <param name="includeAbstract">if set to <c>true</c> the list will include abstract types.</param>
        /// <returns>Returns a list of types that derive from the specified type</returns>
        public static List<Type> GetTypesThatDeriveFromType(Assembly asm, Type baseType, bool includeSelf, bool includeAbstract)
        {
            if (asm == null)
                throw (new ArgumentNullException("asm", "No assembly given"));

            Type[] types = asm.GetTypes();

            if (types == null || types.Length == 0)
                throw (new Exception("Failed getting types from assembly: " + asm.FullName));

            List<Type> typeList = new List<Type>();

            foreach (Type t in types)
            {
                if (t == baseType && !includeSelf)
                    continue;

                if (baseType.IsAssignableFrom(t))
                {
                    if (!includeAbstract && t.IsAbstract)
                        continue;

                    typeList.Add(t);
                }
            }

            return typeList;
        }

    }
}
