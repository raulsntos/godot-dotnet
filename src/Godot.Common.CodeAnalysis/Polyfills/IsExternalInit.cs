// https://github.com/dotnet/runtime/blob/cc7bf831f02cad241547ebea5c56c82f12a50999/src/libraries/System.Private.CoreLib/src/System/Runtime/CompilerServices/IsExternalInit.cs

// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.ComponentModel;

namespace System.Runtime.CompilerServices
{
    /// <summary>
    /// Reserved to be used by the compiler for tracking metadata.
    /// This class should not be used by developers in source code.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    internal static class IsExternalInit { }
}
