// Copyright (c) .NET Foundation and Contributors (https://dotnetfoundation.org/ & https://stride3d.net) and Silicon Studio Corp. (https://www.siliconstudio.co.jp)
// Distributed under the MIT license. See the LICENSE.md file in the project root for more information.

using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace Stride.Core.Reflection;

/// <summary>
/// Allows to attach dynamic properties to an object at runtime. Note that in order to use this object at runtime, you need to set to <c>true</c> the <see cref="Enable"/> property.
/// </summary>
public sealed class ShadowObject : Dictionary<ShadowObjectPropertyKey, object>
{
    // Use a conditional weak table in order to attach properties and to
    private static readonly ConditionalWeakTable<object, ShadowObject> Shadows = [];

    private ShadowObject()
    {
    }

    /// <summary>
    /// Gets or sets a boolean to enable or disable shadow object.
    /// </summary>
    /// <remarks>
    /// When disabled, method <see cref="Get"/> or <see cref="GetOrCreate"/>
    /// </remarks>
    public static bool Enable { get; set; }

    /// <summary>
    /// Tries to get the <see cref="ShadowObject"/> instance associated.
    /// </summary>
    /// <param name="instance">The live instance</param>
    /// <param name="shadow">The shadow object</param>
    /// <returns><c>true</c> if the shadow object was found, <c>false</c> otherwise</returns>
    public static bool TryGet(object instance, [MaybeNullWhen(false)] out ShadowObject shadow)
    {
        shadow = null;
        if (!Enable || instance == null) return false;
        return Shadows.TryGetValue(instance, out shadow);
    }

    /// <summary>
    /// Gets the <see cref="ShadowObject"/> instance if it exists or <c>null</c> otherwise.
    /// </summary>
    /// <param name="instance">The live instance.</param>
    /// <returns>The shadow instance or <c>null</c> if none</returns>
    public static ShadowObject? Get(object? instance)
    {
        if (!Enable || instance == null) return null;
        Shadows.TryGetValue(instance, out var shadow);
        return shadow;
    }

    /// <summary>
    /// Gets the <see cref="ShadowObject"/> instance. Creates it if it does not exist.
    /// </summary>
    /// <param name="instance">The live instance.</param>
    /// <returns>The shadow instance</returns>
    [return: NotNullIfNotNull(nameof(instance))]
    public static ShadowObject? GetOrCreate(object? instance)
    {
        if (!Enable)
        {
            throw new InvalidOperationException("ShadowObject is not enabled. You need to enable it in order to use this method. Note also that ShadowObject has a performance cost at runtime");
        }

        if (instance == null) return null;
        return Shadows.GetValue(instance, _ => new ShadowObject());
    }

    /// <summary>
    /// Copies all dynamic properties from an instance to another instance.
    /// </summary>
    /// <param name="fromInstance">The instance to copy the shadow attributes from</param>
    /// <param name="toInstance">The instance to copy the shadow attributes to</param>
    public static void Copy(object fromInstance, object toInstance)
    {
        if (!Enable) return;
#if NET6_0_OR_GREATER
        ArgumentNullException.ThrowIfNull(fromInstance);
        ArgumentNullException.ThrowIfNull(toInstance);
#else
        if (fromInstance is null) throw new ArgumentNullException(nameof(fromInstance));
        if (toInstance is null) throw new ArgumentNullException(nameof(toInstance));
#endif

        Shadows.TryGetValue(fromInstance, out var shadow);

        if (shadow == null)
            return;

        var newShadow = Shadows.GetValue(toInstance, _ => new ShadowObject());
        foreach (var keyValue in shadow.Where(x => x.Key.CopyValueOnClone))
        {
            newShadow.Add(keyValue.Key, keyValue.Value);
        }
    }
}
