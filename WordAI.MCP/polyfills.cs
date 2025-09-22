namespace System.Runtime.CompilerServices
{
    public sealed class IsExternalInit { }
    [AttributeUsage(AttributeTargets.Method)]
    public sealed class ModuleInitializerAttribute : Attribute { }
    [AttributeUsage(AttributeTargets.Parameter)]
    public sealed class CallerArgumentExpressionAttribute : Attribute
    {
        public CallerArgumentExpressionAttribute(string paramName) { }
    }
}
namespace System.Diagnostics.CodeAnalysis
{
    [AttributeUsage(AttributeTargets.Parameter)]
    public sealed class NotNullWhenAttribute : Attribute { public NotNullWhenAttribute(bool r) { } }
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public sealed class MaybeNullAttribute : Attribute { }
    [AttributeUsage(AttributeTargets.Method)]
    public sealed class DoesNotReturnAttribute : Attribute { }
    // add others only as you need them
}
