﻿// Copyright (c) .NET Foundation and Contributors (https://dotnetfoundation.org/ & https://stride3d.net) and Silicon Studio Corp. (https://www.siliconstudio.co.jp)
// Distributed under the MIT license. See the LICENSE.md file in the project root for more information.

using Stride.Core.Quantum;

namespace Stride.Core.Assets.Quantum.Visitors;

/// <summary>
/// An implementation of <see cref="GraphVisitorBase"/> that will stop visiting deeper each time it reaches a node representing an object reference.
/// </summary>
/// <remarks>This visitor requires a <see cref="AssetPropertyGraph"/> to analyze if a node represents an object reference.</remarks>
public class AssetGraphVisitorBase : GraphVisitorBase
{
    protected readonly AssetPropertyGraphDefinition PropertyGraphDefinition;

    /// <summary>
    /// Initializes a new instance of the <see cref="AssetGraphVisitorBase"/> class.
    /// </summary>
    /// <param name="propertyGraphDefinition">The <see cref="AssetPropertyGraphDefinition"/> used to analyze object references.</param>
    public AssetGraphVisitorBase(AssetPropertyGraphDefinition propertyGraphDefinition)
    {
        PropertyGraphDefinition = propertyGraphDefinition ?? throw new ArgumentNullException(nameof(propertyGraphDefinition));
    }

    /// <inheritdoc/>
    protected override bool ShouldVisitMemberTarget(IMemberNode member)
    {
        return base.ShouldVisitMemberTarget(member) && !PropertyGraphDefinition.IsMemberTargetObjectReference(member, member.Retrieve());
    }

    /// <inheritdoc/>
    protected override bool ShouldVisitTargetItem(IObjectNode collectionNode, NodeIndex index)
    {
        return base.ShouldVisitTargetItem(collectionNode, index) && !PropertyGraphDefinition.IsTargetItemObjectReference(collectionNode, index, collectionNode.Retrieve(index));
    }
}
