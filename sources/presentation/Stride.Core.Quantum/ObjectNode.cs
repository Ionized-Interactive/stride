// Copyright (c) .NET Foundation and Contributors (https://dotnetfoundation.org/ & https://stride3d.net) and Silicon Studio Corp. (https://www.siliconstudio.co.jp)
// Distributed under the MIT license. See the LICENSE.md file in the project root for more information.

using Stride.Core.Collections;
using Stride.Core.Extensions;
using Stride.Core.Reflection;
using Stride.Core.Quantum.References;
using System.Diagnostics.CodeAnalysis;

namespace Stride.Core.Quantum;

/// <summary>
/// An implementation of <see cref="IGraphNode"/> that gives access to an object or a boxed struct.
/// </summary>
/// <remarks>This content is not serialized by default.</remarks>
public class ObjectNode : GraphNodeBase, IInitializingObjectNode, IGraphNodeInternal
{
    private readonly HybridDictionary<string, IMemberNode> childrenMap = [];
    private object value;

    public ObjectNode(INodeBuilder nodeBuilder, object value, Guid guid, ITypeDescriptor descriptor, IReference? reference)
        : base(nodeBuilder.SafeArgument().NodeContainer, guid, descriptor)
    {
        if (reference is ObjectReference)
            throw new ArgumentException($"An {nameof(ObjectNode)} cannot contain an {nameof(ObjectReference)}");
        this.value = value;
        ItemReferences = reference as ReferenceEnumerable;
    }

    /// <inheritdoc/>
    public IMemberNode this[string name] => childrenMap[name];

    /// <inheritdoc/>
    public IReadOnlyCollection<IMemberNode> Members => (IReadOnlyCollection<IMemberNode>)childrenMap.Values;

    /// <inheritdoc/>
    public IEnumerable<NodeIndex>? Indices => GetIndices();

    /// <inheritdoc/>
    [MemberNotNullWhen(true, nameof(Indices))]
    public bool IsEnumerable => Descriptor is CollectionDescriptor || Descriptor is DictionaryDescriptor || Descriptor is ArrayDescriptor;

    /// <inheritdoc/>
    [MemberNotNullWhen(true, nameof(ItemReferences))]
    public override bool IsReference => ItemReferences is not null;

    /// <inheritdoc/>
    public ReferenceEnumerable? ItemReferences { get; }

    /// <inheritdoc/>
    protected sealed override object Value => value;

    /// <inheritdoc/>
    public event EventHandler<INodeChangeEventArgs>? PrepareChange;

    /// <inheritdoc/>
    public event EventHandler<INodeChangeEventArgs>? FinalizeChange;

    /// <inheritdoc/>
    public event EventHandler<ItemChangeEventArgs>? ItemChanging;

    /// <inheritdoc/>
    public event EventHandler<ItemChangeEventArgs>? ItemChanged;

    /// <inheritdoc/>
    public IMemberNode? TryGetChild(string name)
    {
        childrenMap.TryGetValue(name, out var child);
        return child;
    }

    /// <inheritdoc/>
    public IObjectNode? IndexedTarget(NodeIndex index)
    {
        if (index == NodeIndex.Empty) throw new ArgumentException("index cannot be Index.Empty when invoking this method.", nameof(index));
        if (ItemReferences == null) throw new InvalidOperationException("The node does not contain enumerable references.");
        return ItemReferences[index].TargetNode;
    }

    /// <inheritdoc/>
    public void Update(object? newValue, NodeIndex index)
    {
        Update(newValue, index, true);
    }

    /// <inheritdoc/>
    public void Add(object newItem)
    {
        if (Descriptor is CollectionDescriptor collectionDescriptor)
        {
            NodeIndex index = NodeIndex.Empty;
            switch (collectionDescriptor.Category)
            {
                case DescriptorCategory.List:
                    index = new NodeIndex(collectionDescriptor.GetCollectionCount(value));
                    break;
                case DescriptorCategory.Set:
                    index = new NodeIndex(newItem);
                    break;
            }
            var args = new ItemChangeEventArgs(this, index, ContentChangeType.CollectionAdd, null, newItem);
            NotifyItemChanging(args);
            collectionDescriptor.Add(value, newItem);
            UpdateReferences();
            NotifyItemChanged(args);
        }
        else
        {
            throw new NotSupportedException("Unable to set the node value, the collection is unsupported");
        }
    }

    /// <inheritdoc/>
    public void Add(object newItem, NodeIndex itemIndex)
    {
        if (Descriptor is CollectionDescriptor collectionDescriptor)
        {
            var index = collectionDescriptor.Category == DescriptorCategory.Collection
                ? NodeIndex.Empty
                : itemIndex;
            var args = new ItemChangeEventArgs(this, index, ContentChangeType.CollectionAdd, null, newItem);
            NotifyItemChanging(args);
            if (!collectionDescriptor.HasInsert || collectionDescriptor.GetCollectionCount(value) == itemIndex.Int)
            {
                collectionDescriptor.Add(value, newItem);
            }
            else
            {
                collectionDescriptor.Insert(value, itemIndex.Int, newItem);
            }
            UpdateReferences();
            NotifyItemChanged(args);
        }
        else if (Descriptor is DictionaryDescriptor dictionaryDescriptor)
        {
            var args = new ItemChangeEventArgs(this, itemIndex, ContentChangeType.CollectionAdd, null, newItem);
            NotifyItemChanging(args);
            dictionaryDescriptor.AddToDictionary(value, itemIndex.Value, newItem);
            UpdateReferences();
            NotifyItemChanged(args);
        }
        else
        {
            throw new NotSupportedException("Unable to set the node value, the collection is unsupported");
        }
    }

    /// <inheritdoc/>
    public void Remove(object item, NodeIndex itemIndex)
    {
        if (!itemIndex.TryGetValue(out var itemIndexValue))
            throw new ArgumentException("index cannot be empty.", nameof(itemIndex));

        var args = new ItemChangeEventArgs(this, itemIndex, ContentChangeType.CollectionRemove, item, null);
        NotifyItemChanging(args);
        if (Descriptor is CollectionDescriptor collectionDescriptor)
        {
            if (collectionDescriptor.HasRemoveAt)
            {
                collectionDescriptor.RemoveAt(value, itemIndex.Int);
            }
            else
            {
                collectionDescriptor.Remove(value, item);
            }
        }
        else if (Descriptor is DictionaryDescriptor dictionaryDescriptor)
        {
            dictionaryDescriptor.Remove(value, itemIndexValue);
        }
        else
        {
            throw new NotSupportedException("Unable to set the node value, the collection is unsupported");
        }

        UpdateReferences();
        NotifyItemChanged(args);
    }

    /// <inheritdoc/>
    protected internal override void UpdateFromMember(object newValue, NodeIndex index)
    {
        if (index == NodeIndex.Empty)
        {
            throw new InvalidOperationException("An ObjectNode value cannot be modified after it has been constructed");
        }
        Update(newValue, index, true);
    }

    protected void SetValue(object newValue)
    {
        value = newValue;
    }

    protected void NotifyItemChanging(ItemChangeEventArgs args)
    {
        PrepareChange?.Invoke(this, args);
        ItemChanging?.Invoke(this, args);
    }

    protected void NotifyItemChanged(ItemChangeEventArgs args)
    {
        ItemChanged?.Invoke(this, args);
        FinalizeChange?.Invoke(this, args);
    }

    private void Update(object? newValue, NodeIndex index, bool sendNotification)
    {
        if (!index.TryGetValue(out var indexValue))
            throw new ArgumentException("index cannot be empty.", nameof(index));

        if (Descriptor is SetDescriptor setDescriptor)
        {
            if (setDescriptor.Contains(Value, newValue))
            {
                return;
            }
        }

        var oldValue = Retrieve(index);
        ItemChangeEventArgs? itemArgs = null;
        if (sendNotification)
        {
            itemArgs = new ItemChangeEventArgs(this, index, ContentChangeType.CollectionUpdate, oldValue, newValue);
            NotifyItemChanging(itemArgs);
        }
        if (Descriptor is CollectionDescriptor collectionDescriptor)
        {
            collectionDescriptor.SetValue(Value, indexValue, ConvertValue(newValue, collectionDescriptor.ElementType));
        }
        else if (Descriptor is DictionaryDescriptor dictionaryDescriptor)
        {
            dictionaryDescriptor.SetValue(Value, indexValue, ConvertValue(newValue, dictionaryDescriptor.ValueType));
        }
        else if (Descriptor is ArrayDescriptor arrayDescriptor)
        {
            arrayDescriptor.SetValue(Value, (int)indexValue, ConvertValue(newValue, arrayDescriptor.ElementType));
        }
        else
        {
            throw new NotSupportedException("Unable to set the node value, the collection is unsupported");
        }

        UpdateReferences();
        if (sendNotification)
        {
            NotifyItemChanged(itemArgs);
        }
    }

    private void UpdateReferences()
    {
        NodeContainer?.UpdateReferences(this);
    }

    private IEnumerable<NodeIndex>? GetIndices()
    {
        var enumRef = ItemReferences;
        if (enumRef != null)
            return enumRef.Indices;

        return GetIndices(this);
    }

    public override string ToString()
    {
        return $"{{Node: Object {Type.Name} = [{Value}]}}";
    }

    /// <inheritdoc/>
    void IInitializingObjectNode.AddMember(IMemberNode member, bool allowIfReference)
    {
        if (IsSealed)
            throw new InvalidOperationException("Unable to add a child to a GraphNode that has been sealed");

        // ReSharper disable once HeuristicUnreachableCode - this code is reachable only at the specific moment we call this method!
        if (ItemReferences != null && !allowIfReference)
            throw new InvalidOperationException("A GraphNode cannot have children when its content hold a reference.");

        childrenMap.Add(member.Name, (MemberNode)member);
    }
}
