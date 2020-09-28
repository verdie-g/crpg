using System;
using System.Reflection;

namespace Crpg.GameMod.Helpers
{
    internal static class ReflectionHelper
    {
        public static object GetField(object instance, string field) => GetFieldInfo(instance, field)!.GetValue(instance);
        public static void SetField(object instance, string field, object value) => GetFieldInfo(instance, field)!.SetValue(instance, value);
        public static object GetProperty(object instance, string field) => GetPropertyInfo(instance, field)!.GetValue(instance);
        public static void SetProperty(object instance, string field, object value) => GetPropertyInfo(instance, field)!.SetValue(instance, value, null);

        private static FieldInfo? GetFieldInfo(object instance, string field)
        {
            Type? t = instance.GetType();
            while (t != null)
            {
                var f = t.GetField(field, BindingFlags.Instance | BindingFlags.NonPublic);
                if (f != null)
                {
                    return f;
                }

                t = t.BaseType;
            }

            throw new ArgumentException($"Field {field} not found in {instance.GetType()}");
        }

        private static PropertyInfo? GetPropertyInfo(object instance, string prop)
        {
            Type? t = instance.GetType();
            while (t != null)
            {
                var f = t.GetProperty(prop, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                if (f != null)
                {
                    return f.DeclaringType.GetProperty(prop, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                }

                t = t.BaseType;
            }

            throw new ArgumentException($"Property {prop} not found in {instance.GetType()}");
        }
    }
}
