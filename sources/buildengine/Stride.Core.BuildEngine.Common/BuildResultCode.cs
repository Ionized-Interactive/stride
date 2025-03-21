// Copyright (c) .NET Foundation and Contributors (https://dotnetfoundation.org/ & https://stride3d.net) and Silicon Studio Corp. (https://www.siliconstudio.co.jp)
// Distributed under the MIT license. See the LICENSE.md file in the project root for more information.

namespace Stride.Core.BuildEngine;

public enum BuildResultCode
{
    Successful = 0,
    BuildError = 1,
    CommandLineError = 2,
    Cancelled = 100
}
