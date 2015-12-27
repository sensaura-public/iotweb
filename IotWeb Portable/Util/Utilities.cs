using System;
using System.Reflection;

namespace IotWeb.Common.Util
{
    public static class Utilities
    {
        /// <summary>
        /// Get the Assembly instance that contains the given type.
        /// </summary>
        /// <param name="t"></param>
        /// <returns></returns>
        public static Assembly GetContainingAssembly(Type t)
        {
            // Type.Assembly is not available in PCLs targets UWP so we have to
            // get the assembly name from the fully qualified class name and then
            // load the assembly directly
            string name = t.AssemblyQualifiedName;
            int index = name.IndexOf(", ");
            if (index >= 0)
                return Assembly.Load(new AssemblyName(name.Substring(index + 2)));
            return null;
        }

        /// <summary>
        /// Get the assembly instance that contains the given type.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static Assembly GetContainingAssembly<T>()
        {
            return GetContainingAssembly(typeof(T));
        }
    }
}
