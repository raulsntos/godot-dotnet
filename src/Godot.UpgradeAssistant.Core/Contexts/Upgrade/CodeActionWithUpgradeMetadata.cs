using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;

namespace Godot.UpgradeAssistant;

internal static class CodeActionWithUpgradeMetadataExtensions
{
    /// <summary>
    /// Create a wrapper for <paramref name="action"/> that includes the
    /// specified <paramref name="metadata"/>.
    /// </summary>
    /// <param name="action">
    /// Original <see cref="CodeAction"/> that will be wrapped.
    /// </param>
    /// <param name="metadata">
    /// The <see cref="CodeActionUpgradeMetadata"/> to include in the wrapper.
    /// </param>
    /// <returns>Code action with included metadata for the upgrade assistant.</returns>
    public static CodeAction WithUpgradeMetadata(this CodeAction action, CodeActionUpgradeMetadata metadata)
    {
        return new CodeActionWithUpgradeMetadata(action, metadata);
    }

    internal static bool TryDeconstruct(this CodeAction action, out CodeAction originalCodeAction, [NotNullWhen(true)] out CodeActionUpgradeMetadata? metadata)
    {
        if (action is CodeActionWithUpgradeMetadata wrapper)
        {
            wrapper.Deconstruct(out originalCodeAction, out metadata);
            return true;
        }

        (originalCodeAction, metadata) = (action, null);
        return false;
    }

    /// <summary>
    /// Wrapper for <see cref="CodeAction"/> that can be used to register code actions
    /// with included <see cref="CodeActionUpgradeMetadata"/>.
    /// </summary>
    /// <remarks>
    /// This is just a hacky way to workaround lack of Roslyn APIs to add "metadata" or
    /// additional information to <see cref="CodeAction"/> and must not be exposed.
    /// </remarks>
    private class CodeActionWithUpgradeMetadata : CodeAction
    {
        /// <summary>
        /// The original <see cref="CodeAction"/> that should actually be registered.
        /// </summary>
        private readonly CodeAction _originalCodeAction;

        private readonly CodeActionUpgradeMetadata _metadata;

        public override string Title => _originalCodeAction.Title;

        public override string? EquivalenceKey => _originalCodeAction.EquivalenceKey;

        public CodeActionWithUpgradeMetadata(CodeAction action, CodeActionUpgradeMetadata metadata)
        {
            _originalCodeAction = action;
            _metadata = metadata;
        }

        public void Deconstruct(out CodeAction originalCodeAction, out CodeActionUpgradeMetadata metadata)
        {
            originalCodeAction = _originalCodeAction;
            metadata = _metadata;
        }

        protected override Task<Solution?> GetChangedSolutionAsync(CancellationToken cancellationToken)
        {
            // This CodeAction must never be used directly, use the wrapped OriginalCodeAction.
            throw new UnreachableException();
        }
    }
}
