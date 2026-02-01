using System.Reflection;
using FluentAssertions;

internal static class PrivateSetter
{
    public static void Set<TObj, TValue>(TObj obj, string propertyName, TValue value)
    {
        var prop = typeof(TObj).GetProperty(propertyName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        prop.Should().NotBeNull($"Property '{propertyName}' should exist on {typeof(TObj).Name}");

        var setter = prop!.GetSetMethod(nonPublic: true);
        setter.Should().NotBeNull($"Property '{propertyName}' should have a setter (can be private)");

        setter!.Invoke(obj, new object?[] { value });
    }
}