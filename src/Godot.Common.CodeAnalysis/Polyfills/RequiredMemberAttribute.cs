// https://github.com/dotnet/runtime/blob/cc7bf831f02cad241547ebea5c56c82f12a50999/src/libraries/System.Private.CoreLib/src/System/Runtime/CompilerServices/RequiredMemberAttribute.cs

// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.ComponentModel;

namespace System.Runtime.CompilerServices
{
    /// <summary>Specifies that a type has required members or that a member is required.</summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false, Inherited = false)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    internal sealed class RequiredMemberAttribute : Attribute { }
}
