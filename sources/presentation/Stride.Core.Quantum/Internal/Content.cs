// Copyright (c) .NET Foundation and Contributors (https://dotnetfoundation.org/ & https://stride3d.net) and Silicon Studio Corp. (https://www.siliconstudio.co.jp)
// Distributed under the MIT license. See the LICENSE.md file in the project root for more information.

using Stride.Core.Reflection;

namespace Stride.Core.Quantum;

/// <summary>
/// A helper class containing methods to manipulate contents.
/// </summary>
internal static class Content
{
    /// <summary>
    /// Retrieves the value itself or the value of one of its item, depending on the given <see cref="NodeIndex"/>.
    /// </summary>
    /// <param name="value">The value on which this method applies.</param>
    /// <param name="index">The index of the item to retrieve. If <see cref="NodeIndex.Empty"/> is passed, this method will return the value itself.</param>
    /// <param name="descriptor">The descriptor of the type of <paramref name="value"/>.</param>
    /// <returns>The value itself or the value of one of its item.</returns>
    public static object? Retrieve(object value, NodeIndex index, ITypeDescriptor? descriptor)
    {
        if (!index.TryGetValue(out var indexValue))
            return value;

        ArgumentNullException.ThrowIfNull(value);

        if (descriptor is CollectionDescriptor collectionDescriptor)
        {
            return collectionDescriptor.GetValue(value, indexValue);
        }
        else if (descriptor is DictionaryDescriptor dictionaryDescriptor)
        {
            return dictionaryDescriptor.GetValue(value, indexValue);
        }
        else if (descriptor is ArrayDescriptor arrayDescriptor)
        {
            return arrayDescriptor.GetValue(value, (int)indexValue);
        }

        // Try with the concrete type descriptor
        var objectDescriptor = TypeDescriptorFactory.Default.Find(value.GetType());
        if (objectDescriptor != descriptor)
        {
            return Retrieve(value, index, objectDescriptor);
        }

        throw new NotSupportedException("Unable to retrieve the value at the given index, this collection is unsupported");
    }
}
