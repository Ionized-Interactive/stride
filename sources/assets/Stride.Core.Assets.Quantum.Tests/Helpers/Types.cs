﻿// Copyright (c) .NET Foundation and Contributors (https://dotnetfoundation.org/ & https://stride3d.net) and Silicon Studio Corp. (https://www.siliconstudio.co.jp)
// Distributed under the MIT license. See the LICENSE.md file in the project root for more information.

using System.ComponentModel;
using Stride.Core.Annotations;
using Stride.Core.Diagnostics;
using Stride.Core.Extensions;
using Stride.Core.Quantum;

namespace Stride.Core.Assets.Quantum.Tests.Helpers;

public static class Types
{
    public const string FileExtension = ".sdtest";

    [DataContract]
    public abstract class MyAssetBase : Asset;

    [DataContract]
    [AssetDescription(FileExtension)]
    public class MyAsset1 : MyAssetBase
    {
        public string MyString { get; set; }
    }

    [DataContract]
    [AssetDescription(FileExtension)]
    public class MyAsset2 : MyAssetBase
    {
        public List<string> MyStrings { get; set; } = [];
        public StructWithList Struct = new() { MyStrings = [] };
    }

    [DataContract]
    [AssetDescription(FileExtension)]
    public class MyAsset3 : MyAssetBase
    {
        public Dictionary<string, string> MyDictionary { get; set; } = [];
    }

    [DataContract]
    [AssetDescription(FileExtension)]
    public class MyAsset4 : MyAssetBase
    {
        public List<SomeObject> MyObjects { get; set; } = [];
    }

    [DataContract]
    [AssetDescription(FileExtension)]
    public class MyAsset5 : MyAssetBase
    {
        public List<IMyInterface> MyInterfaces { get; set; } = [];
        public IMyInterface MyInterface { get; set; }
    }

    [DataContract]
    [AssetDescription(FileExtension)]
    public class MyAsset6 : MyAssetBase
    {
        public Dictionary<string, IMyInterface> MyDictionary { get; set; } = [];
    }

    [DataContract]
    [AssetDescription(FileExtension)]
    public class MyAsset7 : MyAssetBase
    {
        public MyAsset2 MyAsset2 { get; set; }
        public MyAsset3 MyAsset3 { get; set; }
        public MyAsset4 MyAsset4 { get; set; }
    }

    [DataContract]
    [AssetDescription(FileExtension)]
    public class MyAsset8 : MyAssetBase
    {
        [NonIdentifiableCollectionItems]
        public List<SomeObject> MyObjects { get; set; } = [];
    }

    [DataContract]
    [AssetDescription(FileExtension)]
    public class MyAsset9 : MyAssetBase
    {
        public SomeObject MyObject { get; set; }
    }

    [DataContract]
    [AssetDescription(FileExtension)]
    public class MyAsset10 : MyAssetBase
    {
        [DefaultValue(true)]
        public bool MyBool { get; set; } = true;
    }

    [DataContract]
    public class MyReferenceable : IIdentifiable
    {
        public MyReferenceable() { Id = Guid.NewGuid(); }
        public string Value { get; set; }
        [NonOverridable]
        public Guid Id { get; set; }
        public override string ToString() => $"[{Id}] {Value}";
    }

    [DataContract]
    [AssetDescription(FileExtension)]
    public class MyAssetWithRef : MyAssetBase
    {
        public MyReferenceable MyObject1 { get; set; }

        public MyReferenceable MyObject2 { get; set; }

        [DefaultValue(null)]
        public MyReferenceable MyObject3 { get; set; }

        public List<MyReferenceable> MyObjects { get; set; } = [];

        [NonIdentifiableCollectionItems]
        public List<MyReferenceable> MyNonIdObjects { get; set; } = [];
    }

    [DataContract]
    [AssetDescription(FileExtension)]
    public class MyAssetWithRef2 : MyAssetBase
    {
        public MyReferenceable NonReference { get; set; }

        public MyReferenceable Reference { get; set; }

        public List<MyReferenceable> References { get; set; } = [];

        public static int MemberCount => 4 + 3; // 4 (number of members in Asset) + 3 (number of members in Types.MyAssetWithRef2)
    }

    [DataContract]
    [AssetDescription(FileExtension)]
    public class MyAssetWithStructWithPrimitives : MyAssetBase
    {
        public StructWithPrimitives StructValue { get; set; }
    }

    [DataContract]
    public struct StructWithList
    {
        public List<string> MyStrings { get; set; }
    }

    [DataContract]
    [DataStyle(DataStyle.Compact)]
    public struct StructWithPrimitives : IEquatable<StructWithPrimitives>
    {
        [DefaultValue(0)]
        public int Value1 { get; set; }
        [DefaultValue(0)]
        public int Value2 { get; set; }

        public override readonly bool Equals(object obj) => obj is StructWithPrimitives value && Equals(value);
        public readonly bool Equals(StructWithPrimitives other) => Value1 == other.Value1 && Value2 == other.Value2;

        public override readonly int GetHashCode() => HashCode.Combine(Value1, Value2);

        public override readonly string ToString() => $"(Value1: {Value1}, Value2: {Value2})";
    }

    public interface IMyInterface
    {
        string Value { get; set; }
    }

    [DataContract]
    public class SomeObject : IMyInterface
    {
        public string Value { get; set; }
    }

    [DataContract]
    public class SomeObject2 : IMyInterface
    {
        public string Value { get; set; }
        public int Number { get; set; }
    }

    [AssetPropertyGraphDefinition(typeof(MyAssetWithRef))]
    public class AssetWithRefPropertyGraphDefinition : AssetPropertyGraphDefinition
    {
        public static Func<IGraphNode, NodeIndex, bool> IsObjectReferenceFunc { get; set; }

        public override bool IsMemberTargetObjectReference(IMemberNode member, object value)
        {
            return IsObjectReferenceFunc?.Invoke(member, NodeIndex.Empty) ?? base.IsMemberTargetObjectReference(member, value);
        }

        public override bool IsTargetItemObjectReference(IObjectNode collection, NodeIndex itemIndex, object value)
        {
            return IsObjectReferenceFunc?.Invoke(collection, itemIndex) ?? base.IsTargetItemObjectReference(collection, itemIndex, value);
        }
    }

    [AssetPropertyGraphDefinition(typeof(MyAssetWithRef2))]
    public class AssetWithRefPropertyGraph2 : AssetPropertyGraphDefinition
    {
        public override bool IsMemberTargetObjectReference(IMemberNode member, object value)
        {
            return member.Name == nameof(MyAssetWithRef2.Reference);
        }

        public override bool IsTargetItemObjectReference(IObjectNode collection, NodeIndex itemIndex, object value)
        {
            return collection.Retrieve() is List<MyReferenceable>;
        }
    }

    // TODO: we don't want to have to do this to detect children!
    [DataContract]
    public class ChildrenList : List<MyPart>;

    [DataContract("MyPart")]
    public class MyPart : IIdentifiable
    {
        [NonOverridable]
        public Guid Id { get; set; }
        [DefaultValue(null)] public string Name { get; set; }
        [DefaultValue(null)] public MyPart Parent { get; set; }
        [DefaultValue(null)] public MyPart MyReference { get; set; }
        [DefaultValue(null)] public List<MyPart> MyReferences { get; set; }
        [DefaultValue(null)] public SomeObject Object { get; set; }
        [NonIdentifiableCollectionItems] public List<MyPart> Children { get; } = [];
        public void AddChild([NotNull] MyPart child) { Children.Add(child); child.Parent = this; }
        public override string ToString() => $"{Name} [{Id}]";
    }

    [DataContract("MyPartDesign")]
    public class MyPartDesign : IAssetPartDesign<MyPart>
    {
        [DefaultValue(null)]
        public BasePart Base { get; set; }
        IIdentifiable IAssetPartDesign.Part => Part;
        // ReSharper disable once NotNullMemberIsNotInitialized
        public MyPart Part { get; set; }
        public override string ToString() => $"Design: {Part.Name} [{Part.Id}]";
    }

    [DataContract("MyAssetHierarchy")]
    [AssetDescription(FileExtension)]
    public class MyAssetHierarchy : AssetCompositeHierarchy<MyPartDesign, MyPart>
    {
        public override MyPart GetParent(MyPart part) => part.Parent;
        public override int IndexOf(MyPart part) => GetParent(part)?.Children.IndexOf(part) ?? Hierarchy.RootParts.IndexOf(part);
        public override MyPart GetChild(MyPart part, int index) => part.Children[index];
        public override int GetChildCount(MyPart part) => part.Children.Count;
        public override IEnumerable<MyPart> EnumerateChildParts(MyPart part, bool isRecursive) => isRecursive ? part.Children.DepthFirst(t => t.Children) : part.Children;
        public AssetCompositeHierarchyData<MyPartDesign, MyPart> CreatePartInstances()
        {
            var instance = (MyAssetHierarchy)CreateDerivedAsset("", out _);
            return instance.Hierarchy;
        }
    }

    [AssetPropertyGraph(typeof(MyAssetHierarchy))]
    // ReSharper disable once ClassNeverInstantiated.Local
    public class MyAssetHierarchyPropertyGraph : AssetCompositeHierarchyPropertyGraph<MyPartDesign, MyPart>
    {
        public MyAssetHierarchyPropertyGraph(AssetPropertyGraphContainer container, AssetItem assetItem, ILogger logger) : base(container, assetItem, logger) { }
        public override bool IsChildPartReference(IGraphNode node, NodeIndex index) => node.Type == typeof(ChildrenList);
        protected override void AddChildPartToParentPart(MyPart parentPart, MyPart childPart, int index)
        {
            Container.NodeContainer.GetNode(parentPart)[nameof(MyPart.Children)].Target.Add(childPart, new NodeIndex(index));
            Container.NodeContainer.GetNode(childPart)[nameof(MyPart.Parent)].Update(parentPart);
        }

        protected override void RemoveChildPartFromParentPart(MyPart parentPart, MyPart childPart)
        {
            Container.NodeContainer.GetNode(parentPart)[nameof(MyPart.Children)].Target.Remove(childPart, new NodeIndex(parentPart.Children.IndexOf(childPart)));
            Container.NodeContainer.GetNode(childPart)[nameof(MyPart.Parent)].Update(null);
        }

        protected override Guid GetIdFromChildPart(object part) => ((MyPart)part).Id;
        protected override IEnumerable<IGraphNode> RetrieveChildPartNodes(MyPart part)
        {
            yield return Container.NodeContainer.GetNode(part.Children);
        }
    }

    [AssetPropertyGraph(typeof(MyAssetBase))]
    public class MyAssetBasePropertyGraph : AssetPropertyGraph
    {
        private readonly Dictionary<IGraphNode, IGraphNode> customBases = [];

        public MyAssetBasePropertyGraph(AssetPropertyGraphContainer container, AssetItem assetItem, ILogger logger)
            : base(container, assetItem, logger)
        {
        }

        public void RegisterCustomBaseLink(IGraphNode node, IGraphNode baseNode)
        {
            customBases.Add(node, baseNode);
        }

        public override IGraphNode FindTarget(IGraphNode sourceNode, IGraphNode target)
        {
            return customBases.TryGetValue(sourceNode, out var baseNode) ? baseNode : base.FindTarget(sourceNode, target);
        }
    }
}
