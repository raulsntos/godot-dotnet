using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Godot.Collections;
using Godot.EditorIntegration.CodeEditors;
using Godot.EditorIntegration.Internals;
using Godot.EditorIntegration.Utils;
using Godot.Common;
using Godot.Common.CodeAnalysis;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Godot.EditorIntegration;

[GodotClass]
internal sealed partial class DotNetEditorExtensionSourceCodePlugin : EditorExtensionSourceCodePlugin, IDisposable
{
    private struct ParsedTemplateFile
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string Content { get; set; }
    }

    private readonly Dictionary<string, ParsedTemplateFile> _parsedTemplates = [];

    public DotNetEditorExtensionSourceCodePlugin()
    {
        EditorInternal.RegisterDotNetSourceCodePlugin(this);
    }

    protected override string _GetSourcePath(StringName className)
    {
        try
        {
            return GetSourcePathCoreAsync(className.ToString()).Result;
        }
        catch (Exception e)
        {
            GD.PrintErr($"Error getting source code for {className}: {e}");
            return "";
        }

        static async Task<string> GetSourcePathCoreAsync(string className)
        {
            using var workspace = await DotNetWorkspace.OpenAsync(EditorPath.ProjectCSProjPath).ConfigureAwait(false);
            var symbolCandidates = workspace.FindTypeSymbols(className);

            // There can't be multiple symbols with the same name registered in Godot,
            // so if we found more than one symbol, we are in an invalid state and we
            // can just return an empty string.
            var symbol = symbolCandidates.SingleOrDefault();
            if (symbol is null)
            {
                return "";
            }

            var classDeclarationSyntax = symbol.GetInSourceDeclarationSyntax<ClassDeclarationSyntax>();
            if (classDeclarationSyntax is null)
            {
                return "";
            }

            return classDeclarationSyntax.SyntaxTree.FilePath;
        }
    }

    protected override bool _OverridesExternalEditor()
    {
        var editorSettings = EditorInterface.Singleton.GetEditorSettings();
        return editorSettings.GetSetting(EditorSettingNames.ExternalEditor).As<CodeEditorId>() != CodeEditorId.None;
    }

    protected override Error _OpenInExternalEditor(string sourcePath, int line, int col)
    {
        return DotNetEditorPlugin.Singleton.CodeEditorManager.OpenInCurrentEditor(sourcePath, line, col);
    }

    protected override void _ConfigureSelectPathDialog(int pathIndex, EditorFileDialog dialog)
    {
        dialog.SetFilters([".cs"]);
    }

    protected override string _AdjustPath(int pathIndex, string className, string basePath, string oldPath)
    {
        if (!string.IsNullOrEmpty(oldPath))
        {
            // If the old path is not empty, the user selected a specific path, so take it as the base path.
            basePath = Path.GetDirectoryName(ProjectSettings.Singleton.GlobalizePath(oldPath));
            basePath = ProjectSettings.Singleton.LocalizePath(basePath);
        }

        // The AdjustScriptNameCasing API also looks at the editor setting,
        // in case the user overrides the language preference.
        // Default preference is 'Auto' to match the casing of the class name provided by the user.
        var preferredNameCasing = ScriptLanguage.ScriptNameCasing.Auto;
        string fileName = AdjustScriptNameCasing(className, preferredNameCasing);

        return Path.Combine(basePath, $"{fileName}.cs");
    }

    protected override PackedStringArray _GetAvailableTemplates(string baseClassName)
    {
        baseClassName = NamingUtils.PascalToPascalCase(baseClassName);
        return [.. Templates.GetTemplateIds(baseClassName)];
    }

    protected override string _GetTemplateDisplayName(string templateId)
    {
        return Templates.GetTemplateDisplayName(templateId);
    }

    protected override string _GetTemplateDescription(string templateId)
    {
        return Templates.GetTemplateDescription(templateId);
    }

    protected override GodotArray<GodotDictionary> _GetTemplateOptions(string templateId)
    {
        return [];
    }

    protected override bool _CanHandleTemplateFile(string templatePath)
    {
        return TryParseTemplateFile(templatePath, out _);
    }

    protected override string _GetTemplateFileDisplayName(string templatePath)
    {
        if (TryParseTemplateFile(templatePath, out var parsedTemplate))
        {
            return parsedTemplate.Name;
        }

        return "";
    }

    protected override string _GetTemplateFileDescription(string templatePath)
    {
        if (TryParseTemplateFile(templatePath, out var parsedTemplate))
        {
            return parsedTemplate.Description;
        }

        return "";
    }

    private static bool ValidateCreateClassSourcePaths(PackedStringArray paths, [NotNullWhen(true)] out string? path)
    {
        path = null;
        if (paths.Count != 1)
        {
            GD.PrintErr($"Expected only one path for the C# file, but got {paths.Count}.");
            return false;
        }

        path = paths[0];
        if (!path.EndsWith(".cs", StringComparison.OrdinalIgnoreCase))
        {
            GD.PrintErr($"Expected a .cs file, but got '{path}'.");
            return false;
        }

        return true;
    }

    private static bool CreateProjectSolutionIfNeeded()
    {
        if (!File.Exists(EditorPath.ProjectCSProjPath))
        {
            return DotNetEditorPlugin.Singleton.CreateProjectSolution();
        }

        return true;
    }

    protected override Error _CreateClassSource(string className, string baseClassName, PackedStringArray paths)
    {
        // When no template is specified, use the default template that works for all classes.
        return _CreateClassSourceFromTemplateId(className, baseClassName, paths, Templates.DefaultTemplateId, []);
    }

    protected override Error _CreateClassSourceFromTemplateId(string className, string baseClassName, PackedStringArray paths, string templateId, GodotDictionary options)
    {
        if (!ValidateCreateClassSourcePaths(paths, out string? filePath))
        {
            return Error.InvalidParameter;
        }

        filePath = ProjectSettings.Singleton.GlobalizePath(filePath);

        if (!CreateProjectSolutionIfNeeded())
        {
            GD.PrintErr($"Failed to create C# project.");
            return Error.Unavailable;
        }

        if (!Templates.ContainsTemplate(templateId))
        {

            GD.PrintErr($"Unknown template ID: {templateId}");
            return Error.InvalidParameter;
        }

        baseClassName = NamingUtils.PascalToPascalCase(baseClassName);
        if (baseClassName == className)
        {
            baseClassName = $"Godot.{baseClassName}";
        }

        using var file = File.Create(filePath);
        using var writer = new StreamWriter(file);
        try
        {
            writer.Write(Templates.GetTemplateContent(templateId, className, baseClassName));
        }
        catch (Exception e)
        {
            GD.PrintErr($"Failed to create class source file: {e}");
            return Error.CantCreate;
        }

        return Error.Ok;
    }

    protected override Error _CreateClassSourceFromTemplateFile(string className, string baseClassName, PackedStringArray paths, string templateFilePath)
    {
        if (!ValidateCreateClassSourcePaths(paths, out string? filePath))
        {
            return Error.InvalidParameter;
        }

        filePath = ProjectSettings.Singleton.GlobalizePath(filePath);

        // This will attempt to retrieve the parsed template from the cache, if the template has been parsed before.
        // Otherwise, it will parse it and cache it for the next time.
        if (!TryParseTemplateFile(templateFilePath, out var parsedTemplate))
        {
            GD.PrintErr($"Failed to parse template file: {templateFilePath}");
            return Error.InvalidParameter;
        }

        if (!CreateProjectSolutionIfNeeded())
        {
            GD.PrintErr($"Failed to create C# project.");
            return Error.Unavailable;
        }

        var editorSettings = EditorInterface.Singleton.GetEditorSettings();

        string indentation = "\t";
        bool useSpaceIndentation = editorSettings.GetSetting("text_editor/behavior/indent/type").AsBool();
        if (useSpaceIndentation)
        {
            int indentSize = editorSettings.GetSetting("text_editor/behavior/indent/size").AsInt32();
            indentation = new string(' ', indentSize);
        }

        string classNameStr = className.ToString().Replace(" ", "_");

        string baseClassNameStr = NamingUtils.PascalToPascalCase(baseClassName.ToString());
        if (baseClassNameStr == classNameStr)
        {
            baseClassNameStr = $"Godot.{baseClassNameStr}";
        }

        string processedTemplate = parsedTemplate.Content
            .Replace("_BINDINGS_NAMESPACE_", "Godot")
            .Replace("_BASE_", baseClassNameStr)
            .Replace("_CLASS_", classNameStr)
            .Replace("_TS_", indentation);

        using var file = File.Create(filePath);
        using var writer = new StreamWriter(file);
        try
        {
            writer.Write(processedTemplate);
        }
        catch (Exception e)
        {
            GD.PrintErr($"Failed to create class source file: {e}");
            return Error.CantCreate;
        }

        return Error.Ok;
    }

    private static bool ClassNameOnlyContainsLowercaseAscii(ReadOnlySpan<char> className)
    {
        foreach (char c in className)
        {
            if (!IsLowercaseAscii(c))
            {
                return false;
            }
        }

        return true;

        static bool IsLowercaseAscii(char c) => c >= 'a' && c <= 'z';
    }

    private static bool IsReservedWord(string value)
    {
        return value switch
        {
            // Reserved keywords.
            "abstract" or "as" or "base" or "bool" or "break" or "byte" or "case" or "catch" or
            "char" or "checked" or "class" or "const" or "continue" or "decimal" or "default" or
            "delegate" or "do" or "double" or "else" or "enum" or "event" or "explicit" or
            "extern" or "false" or "finally" or "fixed" or "float" or "for" or "foreach" or
            "goto" or "if" or "implicit" or "in" or "int" or "interface" or "internal" or "is" or
            "lock" or "long" or "namespace" or "new" or "null" or "object" or "operator" or
            "out" or "override" or "params" or "private" or "protected" or "public" or
            "readonly" or "ref" or "return" or "sbyte" or "sealed" or "short" or "sizeof" or
            "stackalloc" or "static" or "string" or "struct" or "switch" or "this" or "throw" or
            "true" or "try" or "typeof" or "uint" or "ulong" or "unchecked" or "unsafe" or
            "ushort" or "using" or "virtual" or "void" or "volatile" or "while" or

            // Contextual keywords. Not reserved words, but I guess we should include
            // them because this seems to be used only for syntax highlighting.
            "add" or "alias" or "ascending" or "async" or "await" or "by" or "descending" or
            "dynamic" or "equals" or "from" or "get" or "global" or "group" or "into" or
            "join" or "let" or "nameof" or "on" or "orderby" or "partial" or "remove" or
            "select" or "set" or "value" or "var" or "when" or "where" or "yield"

            => true,

            _ => false,
        };
    }

    protected override void _ValidateClassName(ValidationContext validationContext, string className)
    {
        if (!SyntaxFacts.IsValidIdentifier(className))
        {
            validationContext.AddValidation(ValidationContext.ValidationSeverity.Error, SR.DotNetEditorExtensionSourceCodePlugin_ClassNameMustBeAValidIdentifier);
            return;
        }

        if (IsReservedWord(className))
        {
            validationContext.AddValidation(ValidationContext.ValidationSeverity.Error, SR.DotNetEditorExtensionSourceCodePlugin_ClassNameCannotBeAReservedWord);
            return;
        }

        // Check for diagnostic CS8981.
        if (ClassNameOnlyContainsLowercaseAscii(className))
        {
            validationContext.AddValidation(ValidationContext.ValidationSeverity.Warning, SR.DotNetEditorExtensionSourceCodePlugin_ClassNameOnlyContainsLowercaseAsciiCharacters);
        }
    }

    protected override void _ValidatePath(ValidationContext validationContext, int pathIndex, string path)
    {
        if (!path.EndsWith(".cs", StringComparison.OrdinalIgnoreCase))
        {
            validationContext.AddValidation(ValidationContext.ValidationSeverity.Error, SR.DotNetEditorExtensionSourceCodePlugin_PathMustEndWithCSharpExtension);
        }
    }

    protected override void _ValidateTemplateOption(ValidationContext validationContext, string templateId, string optionName, Variant value) { }

    protected override Error _AddMethodFunc(StringName className, string methodName, PackedStringArray args)
    {
        try
        {
            AddMethodFuncCoreAsync(className.ToString(), methodName, args).Wait();
            return Error.Ok;
        }
        catch (Exception e)
        {
            GD.PrintErr($"Error adding method '{methodName}' to class '{className}': {e}");
            return Error.Unavailable;
        }

        static async Task AddMethodFuncCoreAsync(string className, string methodName, PackedStringArray args, CancellationToken cancellationToken = default)
        {
            using var workspace = await DotNetWorkspace.OpenAsync(EditorPath.ProjectCSProjPath, cancellationToken).ConfigureAwait(false);

            var symbolCandidates = workspace.FindTypeSymbols(className);

            // There can't be multiple symbols with the same name registered in Godot,
            // so if we found more than one symbol, we are in an invalid state and we
            // can just return an empty string.
            var symbol = symbolCandidates.SingleOrDefault();
            if (symbol is null)
            {
                throw new InvalidOperationException($"Could not find type symbol for class '{className}'.");
            }

            var classDeclarationSyntax = symbol.GetInSourceDeclarationSyntax<ClassDeclarationSyntax>();
            if (classDeclarationSyntax is null)
            {
                throw new InvalidOperationException($"Could not find class declaration syntax for class '{className}'.");
            }

            var document = workspace.GetDocumentForSyntax(classDeclarationSyntax);
            if (document is null)
            {
                throw new InvalidOperationException($"Could not find document for class '{className}'.");
            }

            var newDocument = await AddMethodDeclaration(document, classDeclarationSyntax, methodName, args, cancellationToken).ConfigureAwait(false);
            if (newDocument == document)
            {
                // No changes were made.
                return;
            }

            if (!workspace.TryApplyChanges(newDocument.Project.Solution))
            {
                throw new InvalidOperationException($"Could not apply changes to add method '{methodName}' to class '{className}'.");
            }
        }

        static async Task<Document> AddMethodDeclaration(Document document, ClassDeclarationSyntax classDeclarationSyntax, string methodName, PackedStringArray args, CancellationToken cancellationToken = default)
        {
            var newSyntaxNode = classDeclarationSyntax
                .AddMembers(CreateMethodDeclaration(methodName, args));

            var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
            if (root is null)
            {
                throw new InvalidOperationException("Could not get syntax root.");
            }

            var newRoot = root.ReplaceNode(classDeclarationSyntax, newSyntaxNode);
            return document.WithSyntaxRoot(newRoot);
        }

        static MethodDeclarationSyntax CreateMethodDeclaration(string methodName, PackedStringArray args)
        {
            var returnType = SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.VoidKeyword));

            List<ParameterSyntax> parameters = [];
            foreach (string arg in args)
            {
                string name = arg.Split(':')[0].Trim();
                string type = arg.Split(':')[1].Trim();
                type = NamingUtils.PascalToPascalCase(type);

                var parameterName = SyntaxFactory.Identifier(name);
                var parameterType = SyntaxFactory.ParseTypeName(type);

                parameters.Add(SyntaxFactory.Parameter(parameterName).WithType(parameterType));
            }

            var parameterList = SyntaxFactory.ParameterList(SyntaxFactory.SeparatedList(parameters));

            var commentTrivia = SyntaxFactory.Comment($"// {SR.DotNetEditorExtensionSourceCodePlugin_ReplaceWithMethodBody}");

            var body = SyntaxFactory.Block()
                .WithCloseBraceToken(
                    SyntaxFactory.Token(
                        SyntaxFactory.TriviaList(commentTrivia),
                        SyntaxKind.CloseBraceToken,
                        SyntaxFactory.TriviaList()
                    )
                );

            var attributeSyntax = SyntaxFactory.Attribute(SyntaxFactory.IdentifierName("BindMethod"));
            var attributeListSyntax = SyntaxFactory.AttributeList(SyntaxFactory.SingletonSeparatedList(attributeSyntax));

            return SyntaxFactory.MethodDeclaration(returnType, methodName)
                .AddAttributeLists(attributeListSyntax)
                .AddModifiers(SyntaxFactory.Token(SyntaxKind.PrivateKeyword))
                .WithParameterList(parameterList)
                .WithBody(body)
                .NormalizeWhitespace(elasticTrivia: true)
                .WithLeadingTrivia(SyntaxFactory.TriviaList(SyntaxFactory.ElasticCarriageReturnLineFeed))
                .WithTrailingTrivia(SyntaxFactory.TriviaList(SyntaxFactory.ElasticCarriageReturnLineFeed));
        }
    }

    private bool TryParseTemplateFile(string templateFilePath, [MaybeNullWhen(false)] out ParsedTemplateFile parsedTemplateFile)
    {
        parsedTemplateFile = default;

        if (!templateFilePath.EndsWith(".cs", StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        // Check if the template has already been parsed before and is cached.
        if (_parsedTemplates.TryGetValue(templateFilePath, out parsedTemplateFile))
        {
            return true;
        }


        using FileAccess templateFile = FileAccess.Open(templateFilePath, FileAccess.ModeFlags.Read);
        if (templateFile is null)
        {
#if DEBUG
            GD.PrintErr($"Failed to open template file '{templateFilePath}'.");
#endif
            return false;
        }

        var content = new StringBuilder();

        // Parse file for meta-information and content.
        const string MetaPrefix = "// meta-";
        int spaceIndentSize = 4;
        while (!templateFile.EofReached())
        {
            string line = templateFile.GetLine();
            if (line.StartsWith(MetaPrefix, StringComparison.Ordinal))
            {
                // Store meta information.
                line = line.Substring(MetaPrefix.Length);
                if (line.StartsWith("name:", StringComparison.Ordinal))
                {
                    parsedTemplateFile.Name = line["name:".Length..].Trim();
                }
                else if (line.StartsWith("description:", StringComparison.Ordinal))
                {
                    parsedTemplateFile.Description = line["description:".Length..].Trim();
                }
                else if (line.StartsWith("space-indent:", StringComparison.Ordinal))
                {
                    string indentValue = line["space-indent:".Length..].Trim();
                    if (int.TryParse(indentValue, out int indentSize))
                    {
                        if (indentSize >= 0)
                        {
                            spaceIndentSize = indentSize;
                        }
                        else
                        {
                            GD.PushWarning($"Template meta-space-indent needs to be a non-negative integer value. Found {indentValue}.");
                        }
                    }
                    else
                    {
                        GD.PushWarning($"Template meta-space-indent needs to be a valid integer value. Found '{indentValue}'.");
                    }
                }
            }
            else
            {
                // Replace indentation.
                int i = 0;
                int spaceCount = 0;
                for (; i < line.Length; i++)
                {
                    if (line[i] == '\t')
                    {
                        if (spaceCount > 0)
                        {
                            content.Append(new string(' ', spaceCount));
                            spaceCount = 0;
                        }
                        content.Append("_TS_");
                    }
                    else if (line[i] == ' ')
                    {
                        spaceCount++;
                        if (spaceCount == spaceIndentSize)
                        {
                            content.Append("_TS_");
                            spaceCount = 0;
                        }
                    }
                    else
                    {
                        break;
                    }
                }
                if (spaceCount > 0)
                {
                    content.Append(new string(' ', spaceCount));
                }
                content.AppendLine(line[i..]);
            }
        }

        parsedTemplateFile.Content = content.ToString();

        // Cache parsed template so we don't have to do it again for the same file path.
        _parsedTemplates[templateFilePath] = parsedTemplateFile;

        return true;
    }

    protected override void Dispose(bool disposing)
    {
        EditorInternal.UnregisterDotNetSourceCodePlugin(this);
        base.Dispose(disposing);
    }
}
