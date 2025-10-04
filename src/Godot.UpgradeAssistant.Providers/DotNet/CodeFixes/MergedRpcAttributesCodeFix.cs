using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Godot.Common.CodeAnalysis;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Godot.UpgradeAssistant.Providers;

[ExportCodeFixProvider(LanguageNames.CSharp)]
internal sealed class MergedRpcAttributesCodeFix : CodeFixProvider
{
    private static DiagnosticDescriptor Rule =>
        Descriptors.GUA1005_MergedRpcAttributes;

    public override ImmutableArray<string> FixableDiagnosticIds =>
        [Rule.Id];

    public override FixAllProvider? GetFixAllProvider() =>
        WellKnownFixAllProviders.BatchFixer;

    public override async Task RegisterCodeFixesAsync(CodeFixContext context)
    {
        var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);

        var diagnostic = context.Diagnostics.First();
        var diagnosticSpan = diagnostic.Location.SourceSpan;

        // Find the attribute syntax identified by the diagnostic.
        var attributeSyntax = root?
            .FindToken(diagnosticSpan.Start).Parent?
            .AncestorsAndSelf()
            .OfType<AttributeSyntax>()
            .First();

        if (attributeSyntax is null)
        {
            // Can't apply the code fix without syntax.
            return;
        }

        var codeAction = CodeAction.Create(
            title: SR.GUA1005_MergedRpcAttributes_CodeFix,
            equivalenceKey: nameof(MergedRpcAttributesCodeFix),
            createChangedDocument: cancellationToken => ApplyFix(context.Document, attributeSyntax, cancellationToken));

        context.RegisterCodeFix(codeAction, diagnostic);
    }

    private static async Task<Document> ApplyFix(Document document, AttributeSyntax attributeSyntax, CancellationToken cancellationToken = default)
    {
        var semanticModel = await document.GetSemanticModelAsync(cancellationToken).ConfigureAwait(false);

        var symbol = semanticModel
            .GetSymbolInfo(attributeSyntax, cancellationToken)
            .Symbol?.ContainingSymbol as ITypeSymbol;

        var newAttributeSyntax = CreateNewAttributeSyntaxFromOldSymbol(symbol);
        if (newAttributeSyntax is null)
        {
            // Unsupported symbol found, cannot apply a code fix.
            // Return the document unchanged.
            return document;
        }

        var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
        if (root is null)
        {
            // If we couldn't get the syntax root, return the document unchanged.
            // This should be unreachable.
            return document;
        }

        var newRoot = root.ReplaceNode(attributeSyntax, newAttributeSyntax);
        return document.WithSyntaxRoot(newRoot);
    }

    private static AttributeSyntax? CreateNewAttributeSyntaxFromOldSymbol(ITypeSymbol? symbol)
    {
        if (symbol is null)
        {
            return null;
        }

        if (symbol.EqualsType("Godot.RemoteAttribute", "GodotSharp"))
        {
            // [Rpc(MultiplayerApi.RpcMode.AnyPeer)]
            return SyntaxUtils.CreateAttribute("Rpc", [
                SyntaxUtils.CreateMemberAccessAttributeArgument(null, "MultiplayerApi.RpcMode.AnyPeer"),
            ]);
        }
        else if (symbol.EqualsType("Godot.RemoteSyncAttribute", "GodotSharp"))
        {
            // [Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true)]
            return SyntaxUtils.CreateAttribute("Rpc", [
                SyntaxUtils.CreateMemberAccessAttributeArgument(null, "MultiplayerApi.RpcMode.AnyPeer"),
                SyntaxUtils.CreateBooleanAttributeArgument("CallLocal", true),
            ]);
        }
        else if (symbol.EqualsType("Godot.SyncAttribute", "GodotSharp"))
        {
            // [Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true)]
            return SyntaxUtils.CreateAttribute("Rpc", [
                SyntaxUtils.CreateMemberAccessAttributeArgument(null, "MultiplayerApi.RpcMode.AnyPeer"),
                SyntaxUtils.CreateBooleanAttributeArgument("CallLocal", true),
            ]);
        }
        else if (symbol.EqualsType("Godot.SlaveAttribute", "GodotSharp"))
        {
            // [Rpc]
            return SyntaxUtils.CreateAttribute("Rpc");
        }
        else if (symbol.EqualsType("Godot.PuppetAttribute", "GodotSharp"))
        {
            // [Rpc]
            return SyntaxUtils.CreateAttribute("Rpc");
        }
        else if (symbol.EqualsType("Godot.PuppetSyncAttribute", "GodotSharp"))
        {
            // [Rpc(CallLocal = true)]
            return SyntaxUtils.CreateAttribute("Rpc", [
                SyntaxUtils.CreateBooleanAttributeArgument("CallLocal", true),
            ]);
        }
        else
        {
            // Godot.MasterAttribute, Godot.MasterSyncAttribute are unsupported and won't be converted.
            return null;
        }
    }
}
