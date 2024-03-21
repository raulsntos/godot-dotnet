// https://github.com/dotnet/runtime/blob/cc7bf831f02cad241547ebea5c56c82f12a50999/src/libraries/System.Private.CoreLib/src/System/Collections/Generic/KeyValuePair.cs

// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.ComponentModel;

namespace System.Collections.Generic
{
    internal static class KeyValuePairExtensions
    {
        [EditorBrowsable(EditorBrowsableState.Never)]
        public static void Deconstruct<TKey, TValue>(this KeyValuePair<TKey, TValue> kvp, out TKey key, out TValue value)
        {
            key = kvp.Key;
            value = kvp.Value;
        }
    }
}
