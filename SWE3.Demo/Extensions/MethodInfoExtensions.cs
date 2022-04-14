using System;
using System.Reflection;

namespace SWE3.Demo.Extensions
{
    internal static class MethodInfoExtensions
    {
        public static String GetPropertyName(this MethodInfo type)
        {
            return type.Name.Substring(4, type.Name.Length - 4);
        }
    }
}