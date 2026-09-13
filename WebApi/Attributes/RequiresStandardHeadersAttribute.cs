namespace WebApi.Attributes;

/// <summary>
/// Marks an endpoint as requiring the standard cross-cutting headers.
/// </summary>
[AttributeUsage(AttributeTargets.Method)]
public sealed class RequiresStandardHeadersAttribute : Attribute
{
}
