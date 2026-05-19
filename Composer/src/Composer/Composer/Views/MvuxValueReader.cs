using System;
using System.Collections.Concurrent;
using System.Reflection;

namespace Composer.Views;

/// <summary>
/// MVUX wraps record-typed <c>IState&lt;T&gt;</c> properties in a generated
/// <c>BindableTViewModel</c> proxy so XAML can do per-field two-way binding.
/// When code-behind reads the property via reflection on the bindable
/// proxy's <c>DataContext</c>, the result is the wrapper type — not the
/// record. A direct <c>as T</c> cast then silently fails and the canvas
/// falls back to its hardcoded default, hiding any AI-seeded or
/// runtime-updated content.
///
/// This helper unwraps: it tries the direct cast, then probes the wrapper
/// for common accessor names exposed by MVUX V4/V5 generators (Model,
/// Value, Current, Source, _model, _state).
/// </summary>
internal static class MvuxValueReader
{
    private static readonly ConcurrentDictionary<(Type proxy, Type target), MemberInfo?> _unwrapMember = new();

    /// <summary>Cache for the first-level property lookup on the DataContext
    /// type. The MVUX bindable wrapper has a fixed set of properties; reading
    /// them via reflection on every PropertyChanged was a measurable hot path
    /// (audit 2026-05-12 §9). Sentinel value: null PropertyInfo means
    /// "property does not exist on this type". Keyed by (proxy type, name).</summary>
    private static readonly ConcurrentDictionary<(Type proxy, string name), PropertyInfo?> _propertyLookup = new();

    /// <summary>Cache for method-info lookups invoked on the DataContext —
    /// e.g., <c>GetPreviewValues(LayerKind)</c>. Same rationale as the
    /// property cache: avoid <c>Type.GetMethod()</c> per call. Keyed by
    /// (proxy type, method name); assumes no overload collisions for the
    /// methods we use through this helper.</summary>
    private static readonly ConcurrentDictionary<(Type proxy, string name), MethodInfo?> _methodLookup = new();

    public static T? Read<T>(object? dataContext, string propertyName) where T : class
    {
        if (dataContext is null) return null;

        var proxyType = dataContext.GetType();
        var prop = _propertyLookup.GetOrAdd(
            (proxyType, propertyName),
            static k => k.proxy.GetProperty(k.name));
        if (prop is null) return null;

        var raw = prop.GetValue(dataContext);
        return Unwrap<T>(raw);
    }

    /// <summary>Untyped variant — same caching as <see cref="Read{T}"/> but
    /// returns the raw wrapper value without trying to unwrap a typed
    /// generic. Useful when the caller does its own unwrap (e.g., for
    /// value-typed feeds like <c>IFeed&lt;double&gt;</c>).</summary>
    public static object? ReadRaw(object? dataContext, string propertyName)
    {
        if (dataContext is null) return null;
        var proxyType = dataContext.GetType();
        var prop = _propertyLookup.GetOrAdd(
            (proxyType, propertyName),
            static k => k.proxy.GetProperty(k.name));
        return prop?.GetValue(dataContext);
    }

    /// <summary>Invoke a method on the DataContext with cached
    /// <see cref="MethodInfo"/>. Swallows reflection exceptions and
    /// returns null on failure (matching the call-site contract used by
    /// canvas Render methods).</summary>
    public static object? InvokeMethod(object? target, string methodName, params object?[] args)
    {
        if (target is null) return null;
        var method = _methodLookup.GetOrAdd(
            (target.GetType(), methodName),
            static k => k.proxy.GetMethod(k.name));
        if (method is null) return null;
        try { return method.Invoke(target, args); }
        catch { return null; }
    }

    public static T? Unwrap<T>(object? value) where T : class
    {
        if (value is null) return null;
        if (value is T direct) return direct;

        var key = (value.GetType(), typeof(T));
        var member = _unwrapMember.GetOrAdd(key, FindUnwrapMember<T>);
        if (member is null) return null;

        return ReadMember(member, value) as T;
    }

    private static MemberInfo? FindUnwrapMember<T>((Type proxy, Type target) key) where T : class
    {
        var t = key.proxy;
        const BindingFlags Flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

        // Try common MVUX wrapper accessors by name first.
        foreach (var name in new[] { "Model", "Value", "Current", "Source", "_model", "_state", "_value" })
        {
            var prop = t.GetProperty(name, Flags);
            if (prop is { } p && typeof(T).IsAssignableFrom(p.PropertyType)) return p;
            var field = t.GetField(name, Flags);
            if (field is { } f && typeof(T).IsAssignableFrom(f.FieldType)) return f;
        }

        // Fall back: any public/private property or field whose declared type
        // matches the target. The wrapper holds a reference to the underlying
        // record somewhere — find the first one assignable to T.
        foreach (var prop in t.GetProperties(Flags))
            if (typeof(T).IsAssignableFrom(prop.PropertyType)) return prop;
        foreach (var field in t.GetFields(Flags))
            if (typeof(T).IsAssignableFrom(field.FieldType)) return field;

        return null;
    }

    private static object? ReadMember(MemberInfo member, object target) => member switch
    {
        PropertyInfo p => p.GetValue(target),
        FieldInfo f    => f.GetValue(target),
        _              => null,
    };
}
