using SWE3.Demo.Test;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

namespace SWE3.Demo.Extensions
{
    public static class TypeExtensions
    {
        public static string GetNameWithoutGenericInformation(this Type type)
        {
            int index = type.Name.IndexOf('`');
            if (index == -1)
                return type.Name;
            else
                return type.Name.Substring(0, index);
        }

        public static bool IsSimpleType(this Type type)
        {
            return
                type.IsPrimitive ||
                type.IsEnum ||
                type == typeof(string) ||
                type == typeof(decimal) ||
                type == typeof(DateTime) ||
                type == typeof(DateTimeOffset) ||
                type == typeof(TimeSpan) ||
                type == typeof(Guid) || type == typeof(string);
        }

        public static bool IsMapAbleType(this Type type)
        {
            return
                type.IsSimpleType() ||
                typeof(IList).IsAssignableFrom(type) ||
                (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(ICollection<>) && !typeof(IDictionary).IsAssignableFrom(type)) || 
                !type.Namespace.StartsWith("System"); //For custom classes
        }

        public static bool ShouldTypeBeVirtual(this Type type)
        {
            return
                typeof(IEnumerable).IsAssignableFrom(type) &&  !type.IsSimpleType() ||
                !type.Namespace.StartsWith("System") && !type.IsSimpleType(); //For custom classes
        }

        public static bool IsInteger(this Type type)
        {
            return
                type == typeof(Int16) ||
                type == typeof(Int32) ||
                type == typeof(Int64) ||
                type == typeof(UInt16) ||
                type == typeof(UInt32) ||
                type == typeof(UInt64);
        }
    }
}