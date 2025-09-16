using Microsoft.CodeAnalysis;

namespace Godot.Analyzers;

internal static class Descriptors
{
    // TODO: These documentation pages don't exist yet.
    private const string VersionDocsUrl = "https://docs.godotengine.org/en/latest";
    private static string FormatHelpLink(string diagnosticId) => $"{VersionDocsUrl}/tutorials/scripting/c_sharp/diagnostics/{diagnosticId}.html";

    /// <summary>
    /// Rule categories as defined in https://learn.microsoft.com/en-us/dotnet/fundamentals/code-analysis/categories.
    /// We only define the categories we actually use.
    /// </summary>
    private static class Category
    {
        /// <summary>
        /// Usage rules help developers use APIs correctly.
        /// </summary>
        public const string Usage = nameof(Usage);

        /// <summary>
        /// Performance rules help developers avoid performance pitfalls.
        /// </summary>
        public const string Performance = nameof(Performance);
    }

    #region 00XX: Reserved for general rules about the Godot .NET integration.

    public static readonly DiagnosticDescriptor GODOT0001_GodotAttributeHasNoEffectOutsideGodotClass = new(
        id: "GODOT0001",
        title: SR.GODOT0001_GodotAttributeHasNoEffectOutsideGodotClass_Title,
        messageFormat: SR.GODOT0001_GodotAttributeHasNoEffectOutsideGodotClass_MessageFormat,
        description: SR.GODOT0001_GodotAttributeHasNoEffectOutsideGodotClass_Description,
        category: Category.Usage,
        defaultSeverity: DiagnosticSeverity.Info,
        isEnabledByDefault: true,
        helpLinkUri: FormatHelpLink("GODOT0001"));

    public static readonly DiagnosticDescriptor GODOT0002_BoundMembersMustHaveUniqueNames = new(
        id: "GODOT0002",
        title: SR.GODOT0002_BoundMembersMustHaveUniqueNames_Title,
        messageFormat: SR.GODOT0002_BoundMembersMustHaveUniqueNames_MessageFormat,
        description: SR.GODOT0002_BoundMembersMustHaveUniqueNames_Description,
        category: Category.Usage,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        helpLinkUri: FormatHelpLink("GODOT0002"));

    public static readonly DiagnosticDescriptor GODOT0003_MarhsallingRequiresCopying = new(
        id: "GODOT0003",
        title: SR.GODOT0003_MarhsallingRequiresCopying_Title,
        messageFormat: SR.GODOT0003_MarhsallingRequiresCopying_MessageFormat,
        description: SR.GODOT0003_MarhsallingRequiresCopying_Description,
        category: Category.Performance,
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        helpLinkUri: FormatHelpLink("GODOT0003"));

    public static readonly DiagnosticDescriptor GODOT0004_GodotClassWithEditorCallbacksShouldBeTool = new(
        id: "GODOT0004",
        title: SR.GODOT0004_GodotClassWithEditorCallbacksShouldBeTool_Title,
        messageFormat: SR.GODOT0004_GodotClassWithEditorCallbacksShouldBeTool_MessageFormat,
        description: SR.GODOT0004_GodotClassWithEditorCallbacksShouldBeTool_Description,
        category: Category.Usage,
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        helpLinkUri: FormatHelpLink("GODOT0004"));

    #endregion

    #region 01XX: Rules about the [GodotClass] attribute.

    public static readonly DiagnosticDescriptor GODOT0101_GodotClassMustDeriveFromGodotObject = new(
        id: "GODOT0101",
        title: SR.GODOT0101_GodotClassMustDeriveFromGodotObject_Title,
        messageFormat: SR.GODOT0101_GodotClassMustDeriveFromGodotObject_MessageFormat,
        description: SR.GODOT0101_GodotClassMustDeriveFromGodotObject_Description,
        category: Category.Usage,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        helpLinkUri: FormatHelpLink("GODOT0101"));

    public static readonly DiagnosticDescriptor GODOT0102_GodotClassMustNotBeGeneric = new(
        id: "GODOT0102",
        title: SR.GODOT0102_GodotClassMustNotBeGeneric_Title,
        messageFormat: SR.GODOT0102_GodotClassMustNotBeGeneric_MessageFormat,
        description: SR.GODOT0102_GodotClassMustNotBeGeneric_Description,
        category: Category.Usage,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        helpLinkUri: FormatHelpLink("GODOT0102"));

    #endregion

    #region 02XX: Rules about the [BindConstructor] attribute.
    #endregion

    #region 03XX: Rules about the [BindConstant] attribute.

    public static readonly DiagnosticDescriptor GODOT0301_ConstantTypeIsNotSupported = new(
        id: "GODOT0301",
        title: SR.GODOT0301_ConstantTypeIsNotSupported_Title,
        messageFormat: SR.GODOT0301_ConstantTypeIsNotSupported_MessageFormat,
        description: SR.GODOT0301_ConstantTypeIsNotSupported_Description,
        category: Category.Usage,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        helpLinkUri: FormatHelpLink("GODOT0301"));

    public static readonly DiagnosticDescriptor GODOT0302_ConstantMustBeConst = new(
        id: "GODOT0302",
        title: SR.GODOT0302_ConstantMustBeConst_Title,
        messageFormat: SR.GODOT0302_ConstantMustBeConst_MessageFormat,
        description: SR.GODOT0302_ConstantMustBeConst_Description,
        category: Category.Usage,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        helpLinkUri: FormatHelpLink("GODOT0302"));

    #endregion

    #region 04XX: Rules about the [BindEnum] attribute.
    #endregion

    #region 05XX: Rules about the [BindProperty] attribute.

    public static readonly DiagnosticDescriptor GODOT0501_PropertyTypeIsNotSupported = new(
        id: "GODOT0501",
        title: SR.GODOT0501_PropertyTypeIsNotSupported_Title,
        messageFormat: SR.GODOT0501_PropertyTypeIsNotSupported_MessageFormat,
        description: SR.GODOT0501_PropertyTypeIsNotSupported_Description,
        category: Category.Usage,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        helpLinkUri: FormatHelpLink("GODOT0501"));

    public static readonly DiagnosticDescriptor GODOT0502_PropertyMustNotBeStatic = new(
        id: "GODOT0502",
        title: SR.GODOT0502_PropertyMustNotBeStatic_Title,
        messageFormat: SR.GODOT0502_PropertyMustNotBeStatic_MessageFormat,
        description: SR.GODOT0502_PropertyMustNotBeStatic_Description,
        category: Category.Usage,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        helpLinkUri: FormatHelpLink("GODOT0502"));

    public static readonly DiagnosticDescriptor GODOT0503_PropertyMustNotBeConst = new(
        id: "GODOT0503",
        title: SR.GODOT0503_PropertyMustNotBeConst_Title,
        messageFormat: SR.GODOT0503_PropertyMustNotBeConst_MessageFormat,
        description: SR.GODOT0503_PropertyMustNotBeConst_Description,
        category: Category.Usage,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        helpLinkUri: FormatHelpLink("GODOT0503"));

    public static readonly DiagnosticDescriptor GODOT0504_PropertyMustNotBeReadOnly = new(
        id: "GODOT0504",
        title: SR.GODOT0504_PropertyMustNotBeReadOnly_Title,
        messageFormat: SR.GODOT0504_PropertyMustNotBeReadOnly_MessageFormat,
        description: SR.GODOT0504_PropertyMustNotBeReadOnly_Description,
        category: Category.Usage,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        helpLinkUri: FormatHelpLink("GODOT0504"));

    public static readonly DiagnosticDescriptor GODOT0505_PropertyMustHaveGetter = new(
        id: "GODOT0505",
        title: SR.GODOT0505_PropertyMustHaveGetterAndSetter_Title,
        messageFormat: SR.GODOT0505_PropertyMustHaveGetter_MessageFormat,
        description: SR.GODOT0505_PropertyMustHaveGetterAndSetter_Description,
        category: Category.Usage,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        helpLinkUri: FormatHelpLink("GODOT0505"));

    public static readonly DiagnosticDescriptor GODOT0505_PropertyMustHaveSetter = new(
        id: "GODOT0505",
        title: SR.GODOT0505_PropertyMustHaveGetterAndSetter_Title,
        messageFormat: SR.GODOT0505_PropertyMustHaveSetter_MessageFormat,
        description: SR.GODOT0505_PropertyMustHaveGetterAndSetter_Description,
        category: Category.Usage,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        helpLinkUri: FormatHelpLink("GODOT0505"));

    public static readonly DiagnosticDescriptor GODOT0506_PropertyMustNotBeIndexer = new(
        id: "GODOT0506",
        title: SR.GODOT0506_PropertyMustNotBeIndexer_Title,
        messageFormat: SR.GODOT0506_PropertyMustNotBeIndexer_MessageFormat,
        description: SR.GODOT0506_PropertyMustNotBeIndexer_Description,
        category: Category.Usage,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        helpLinkUri: FormatHelpLink("GODOT0506"));

    #endregion

    #region 06XX: Rules about the [BindMethod] attribute.

    public static readonly DiagnosticDescriptor GODOT0601_MethodParameterTypeIsNotSupported = new(
        id: "GODOT0601",
        title: SR.GODOT0601_MethodParameterTypeIsNotSupported_Title,
        messageFormat: SR.GODOT0601_MethodParameterTypeIsNotSupported_MessageFormat,
        description: SR.GODOT0601_MethodParameterTypeIsNotSupported_Description,
        category: Category.Usage,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        helpLinkUri: FormatHelpLink("GODOT0601"));

    public static readonly DiagnosticDescriptor GODOT0601_MethodReturnTypeIsNotSupported = new(
        id: "GODOT0601",
        title: SR.GODOT0601_MethodReturnTypeIsNotSupported_Title,
        messageFormat: SR.GODOT0601_MethodReturnTypeIsNotSupported_MessageFormat,
        description: SR.GODOT0601_MethodReturnTypeIsNotSupported_Description,
        category: Category.Usage,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        helpLinkUri: FormatHelpLink("GODOT0601"));

    #endregion

    #region 07XX: Rules about the [Signal] attribute.

    public static readonly DiagnosticDescriptor GODOT0701_SignalParameterTypeIsNotSupported = new(
        id: "GODOT0701",
        title: SR.GODOT0701_SignalParameterTypeIsNotSupported_Title,
        messageFormat: SR.GODOT0701_SignalParameterTypeIsNotSupported_MessageFormat,
        description: SR.GODOT0701_SignalParameterTypeIsNotSupported_Description,
        category: Category.Usage,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        helpLinkUri: FormatHelpLink("GODOT0701"));

    public static readonly DiagnosticDescriptor GODOT0702_SignalShouldBeVoidReturnType = new(
        id: "GODOT0702",
        title: SR.GODOT0702_SignalShouldBeVoidReturnType_Title,
        messageFormat: SR.GODOT0702_SignalShouldBeVoidReturnType_MessageFormat,
        description: SR.GODOT0702_SignalShouldBeVoidReturnType_Description,
        category: Category.Usage,
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        helpLinkUri: FormatHelpLink("GODOT0702"));

    public static readonly DiagnosticDescriptor GODOT0704_SignalDelegateMissingSuffix = new(
        id: "GODOT0704",
        title: SR.GODOT0704_SignalDelegateMissingSuffix_Title,
        messageFormat: SR.GODOT0704_SignalDelegateMissingSuffix_MessageFormat,
        description: SR.GODOT0704_SignalDelegateMissingSuffix_Description,
        category: Category.Usage,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        helpLinkUri: FormatHelpLink("GODOT0704"));

    #endregion

    #region 08XXX: Rules about the [MustBeVariant] attribute.

    public static readonly DiagnosticDescriptor GODOT0801_GenericTypeArgumentMustBeVariant = new(
        id: "GODOT0801",
        title: SR.GODOT0801_GenericTypeArgumentMustBeVariant_Title,
        messageFormat: SR.GODOT0801_GenericTypeArgumentMustBeVariant_MessageFormat,
        description: SR.GODOT0801_GenericTypeArgumentMustBeVariant_Description,
        category: Category.Usage,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        helpLinkUri: FormatHelpLink("GODOT0801"));

    public static readonly DiagnosticDescriptor GODOT0802_GenericTypeParameterMustBeVariantAnnotated = new(
        id: "GODOT0802",
        title: SR.GODOT0802_GenericTypeParameterMustBeVariantAnnotated_Title,
        messageFormat: SR.GODOT0802_GenericTypeParameterMustBeVariantAnnotated_MessageFormat,
        description: SR.GODOT0802_GenericTypeParameterMustBeVariantAnnotated_Description,
        category: Category.Usage,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        helpLinkUri: FormatHelpLink("GODOT0802"));

    #endregion
}
