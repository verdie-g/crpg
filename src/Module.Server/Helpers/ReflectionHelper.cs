using System.Reflection;

namespace Crpg.Module.Helpers;

/// <summary>
/// Provides helper methods for reflection to use only when is no other choice.
/// </summary>
internal static class ReflectionHelper
{
    public static object GetField(object instance, string field) => GetFieldInfo(instance, field).GetValue(instance);
    public static void SetField(object instance, string field, object value) => GetFieldInfo(instance, field).SetValue(instance, value);
    public static object GetProperty(object instance, string prop) => GetPropertyInfo(instance, prop)!.GetValue(instance);
    public static void SetProperty(object instance, string prop, object value) => GetPropertyInfo(instance, prop)!.SetValue(instance, value, null);

    public static object InvokeMethod(object instance, string method, object[] parameters)
    {
        return instance
            .GetType()
            .GetMethod(method, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)!
            .Invoke(instance, parameters);
    }

    public static void RaiseEvent(object instance, string evt, object[] parameters)
    {
        var raiseMethod = GetEventInfo(instance, evt).GetRaiseMethod();
        raiseMethod?.Invoke(instance, parameters);
    }

    private static FieldInfo GetFieldInfo(object instance, string field)
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
                return f.DeclaringType!.GetProperty(prop, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            }

            t = t.BaseType;
        }

        throw new ArgumentException($"Property {prop} not found in {instance.GetType()}");
    }

    private static EventInfo GetEventInfo(object instance, string evt)
    {
        var e = instance.GetType().GetEvent(evt);
        if (e == null)
        {
            throw new ArgumentException($"Event {evt} not found in {instance.GetType()}");
        }

        return e;
    }
}
