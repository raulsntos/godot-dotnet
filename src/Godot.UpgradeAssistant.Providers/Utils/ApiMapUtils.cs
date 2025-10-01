using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Godot.Common.CodeAnalysis;
using Microsoft.CodeAnalysis;

namespace Godot.UpgradeAssistant.Providers;

internal static partial class ApiMapUtils
{
    [JsonSourceGenerationOptions(JsonSerializerDefaults.Web,
        ReadCommentHandling = JsonCommentHandling.Skip,
        AllowTrailingCommas = true,
        UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow,
        UseStringEnumConverter = true
    )]
    [JsonSerializable(typeof(ApiMapEntry))]
    [JsonSerializable(typeof(Dictionary<string, ApiMapEntry>))]
    private sealed partial class ApiMapEntryJsonContext : JsonSerializerContext { }

    private sealed class ApiMap
    {
        public string Name { get; }

        public Dictionary<string, ApiMapEntry> Entries { get; }

        public ApiMap(string name, Dictionary<string, ApiMapEntry> entries)
        {
            Name = name;
            Entries = entries;
        }

        public void Deconstruct(out string name, out IReadOnlyDictionary<string, ApiMapEntry> entries)
        {
            name = Name;
            entries = Entries;
        }
    }

    private static IReadOnlyList<ApiMap>? _mappings;

    private static async Task Initialize(CancellationToken cancellationToken = default)
    {
        Debug.Assert(_mappings is null, "API mappings are already initialized.");

        var assembly = typeof(ApiMapUtils).Assembly;
        var resourceNames = assembly.GetManifestResourceNames()
            .Where(name => name.StartsWith("Godot.UpgradeAssistant.Providers.Assets.ApiMap.", StringComparison.Ordinal));

        List<ApiMap> mappings = [];

        foreach (string name in resourceNames)
        {
            using var stream = assembly.GetManifestResourceStream(name);
            Debug.Assert(stream is not null);

            var entries = await JsonSerializer.DeserializeAsync(stream, typeof(Dictionary<string, ApiMapEntry>), ApiMapEntryJsonContext.Default, cancellationToken: cancellationToken).ConfigureAwait(false) as Dictionary<string, ApiMapEntry>;
            Debug.Assert(entries is not null);

            mappings.Add(new ApiMap(name, entries));
        }

        _mappings = mappings;
    }

    private static bool TryGetApiKindFromSymbolType(ISymbol symbol, out ApiMapKind kind)
    {
        (kind, bool ok) = symbol switch
        {
            IFieldSymbol => (ApiMapKind.Field, true),
            IPropertySymbol => (ApiMapKind.Property, true),
            IMethodSymbol => (ApiMapKind.Method, true),
            IEventSymbol => (ApiMapKind.Event, true),
            INamespaceSymbol => (ApiMapKind.Namespace, true),
            ITypeSymbol => (ApiMapKind.Type, true),

            // The found entry does not match the symbol type.
            _ => (default, false),
        };
        return ok;
    }

    /// <summary>
    /// Get an entry from the API map for the specified <paramref name="symbol"/> following redirects.
    /// To avoid follow redirects use
    /// <see cref="GetApiEntryForSymbolExactMatchAsync(ISymbol, SemVer, bool, CancellationToken)"/>.
    /// </summary>
    /// <remarks>
    /// Following redirects is usually preferred because, if an API was renamed multiple
    /// times, it should be replaced with the last name.
    /// </remarks>
    /// <param name="symbol">The symbol to lookup.</param>
    /// <param name="targetGodotVersion">The target Godot version that the entry must apply to.</param>
    /// <param name="isGodotDotNetEnabled">Whether the Godot .NET mappings should be included in the lookup.</param>
    /// <param name="cancellationToken">Optional token to cancel the asynchronous operation.</param>
    /// <returns>The entry found in the API map.</returns>
    public static async ValueTask<ApiMapEntry?> GetApiEntryForSymbolAsync(ISymbol symbol, SemVer targetGodotVersion, bool isGodotDotNetEnabled = false, CancellationToken cancellationToken = default)
    {
        string fullName = symbol.FullQualifiedNameOmitGlobal();
        if (!TryGetApiKindFromSymbolType(symbol, out var kind))
        {
            // Unsupported symbol type, an entry will never be found.
            return null;
        }

        ApiMapEntry? currentEntry = null;
        while (true)
        {
            ApiMapEntry? previousEntry = currentEntry;

            // Check if there's an entry for a symbol with the current fully-qualified name.
            currentEntry = await GetApiEntryForSymbolAsyncCore(fullName, kind, targetGodotVersion, isGodotDotNetEnabled, cancellationToken).ConfigureAwait(false);

            if (currentEntry is null)
            {
                // If the current entry is null, then the previous entry was the last replacement.
                return previousEntry;
            }

            if (currentEntry is not { State: ApiMapState.Replaced })
            {
                // If the current entry is not a replacement, then we reached the last entry.
                return currentEntry;
            }

            // The current entry is a replacement so it must have a value, take it
            // as the new fully-qualified symbol name to check for.
            Debug.Assert(currentEntry.Value is not null);
            fullName = currentEntry.Value;
        }
    }

    /// <summary>
    /// Get an entry from the API map for the specified <paramref name="symbol"/> without following redirects.
    /// To follow redirects use
    /// <see cref="GetApiEntryForSymbolAsync(ISymbol, SemVer, bool, CancellationToken)"/>.
    /// </summary>
    /// <remarks>
    /// Following redirects is usually preferred because, if an API was renamed multiple
    /// times, it should be replaced with the last name.
    /// </remarks>
    /// <param name="symbol">The symbol to lookup.</param>
    /// <param name="targetGodotVersion">The target Godot version that the entry must apply to.</param>
    /// <param name="isGodotDotNetEnabled">Whether the Godot .NET mappings should be included in the lookup.</param>
    /// <param name="cancellationToken">Optional token to cancel the asynchronous operation.</param>
    /// <returns>The entry found in the API map.</returns>
    public static async ValueTask<ApiMapEntry?> GetApiEntryForSymbolExactMatchAsync(ISymbol symbol, SemVer targetGodotVersion, bool isGodotDotNetEnabled = false, CancellationToken cancellationToken = default)
    {
        string fullName = symbol.FullQualifiedNameOmitGlobal();
        if (!TryGetApiKindFromSymbolType(symbol, out var kind))
        {
            // Unsupported symbol type, an entry will never be found.
            return null;
        }

        return await GetApiEntryForSymbolAsyncCore(fullName, kind, targetGodotVersion, isGodotDotNetEnabled, cancellationToken).ConfigureAwait(false);
    }

    public static async ValueTask<ApiMapEntry?> GetApiEntryForSymbolAsyncCore(string fullName, ApiMapKind kind, SemVer targetGodotVersion, bool isGodotDotNetEnabled = false, CancellationToken cancellationToken = default)
    {
        if (_mappings is null)
        {
            await Initialize(cancellationToken).ConfigureAwait(false);
        }
        if (_mappings is null)
        {
            throw new InvalidOperationException(SR.InvalidOperation_ApiMappingsNotAvailable);
        }

        SemVer version = default;
        ApiMapEntry? entry = null;

        foreach (var (name, entries) in _mappings)
        {
            if (name.Contains(".GodotDotNet.", StringComparison.Ordinal) && !isGodotDotNetEnabled)
            {
                // Not targeting the Godot .NET packages, so skip this group of entries.
                continue;
            }

            if (!entries.TryGetValue(fullName, out var currentEntry))
            {
                // The given symbol has no mapping in this group of entries.
                continue;
            }

            SemVer currentEntryVersion;
            if (string.IsNullOrEmpty(currentEntry.Version))
            {
                // If a first version is not specified, use a really high value
                // so this entry is always picked.
                currentEntryVersion = new SemVer(42, 42, 42);
            }
            else
            {
                currentEntryVersion = SemVer.Parse(currentEntry.Version);
                if (currentEntryVersion > targetGodotVersion)
                {
                    // The mapping in this group does not apply to the target version.
                    continue;
                }
            }

            if (entry is null)
            {
                // This is the first entry we find for the given symbol, so let's take it.
                entry = currentEntry;
                version = currentEntryVersion;
                continue;
            }

            // We already had an entry for the given symbol, compare them to see which one we should keep.
            if (currentEntryVersion > version)
            {
                // The first version that this entry applies to is newer than the one we had, so let's take it.
                entry = currentEntry;
            }
        }

        if (entry is null)
        {
            // The given symbol has no entry, which means it was likely not replaced/removed.
            return null;
        }

        // The key is not part of the JSON file, so we have to set it here.
        // We also mark the entry as read-only to ensure it doesn't change after the first read.
        entry.Key = fullName;
        entry.MakeReadOnly();

        if (entry.State == ApiMapState.Replaced && entry.Key == entry.Value)
        {
            // This is a special case where an entry is added as if a symbol was replaced by itself
            // to override a previous NotImplemented entry. Since there was no actual replacement,
            // we avoid returning an entry so we don't report a diagnostic for this symbol.
            return null;
        }

        // Only return the entry if it matches the kind; otherwise, assume it wasn't found.
        return entry.Kind == kind ? entry : null;
    }
}
