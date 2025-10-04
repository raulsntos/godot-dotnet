using Microsoft.CodeAnalysis;

namespace Godot.UpgradeAssistant.Providers;

internal static class Descriptors
{
    private const string Category = "GodotUpgradeAssistant";

    #region 0XXX: IAnalysisProvider and IUpgradeProvider

    public static readonly DiagnosticDescriptor GUA0001_DotNetProjectTfm = new(
        id: "GUA0001",
        title: SR.GUA0001_DotNetProjectTfm_Title,
        messageFormat: SR.GUA0001_DotNetProjectTfm_MessageFormat,
        description: SR.GUA0001_DotNetProjectTfm_Description,
        category: Category,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    public static readonly DiagnosticDescriptor GUA0001_DotNetProjectTfm_Platform = new(
        id: "GUA0001",
        title: SR.GUA0001_DotNetProjectTfm_Title,
        messageFormat: SR.GUA0001_DotNetProjectTfm_Platform_MessageFormat,
        description: SR.GUA0001_DotNetProjectTfm_Description,
        category: Category,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    public static readonly DiagnosticDescriptor GUA0002_DotNetProjectSdk = new(
        id: "GUA0002",
        title: SR.GUA0002_DotNetProjectSdk_Title,
        messageFormat: SR.GUA0002_DotNetProjectSdk_MessageFormat,
        description: SR.GUA0002_DotNetProjectSdk_Description,
        category: Category,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    public static readonly DiagnosticDescriptor GUA0003_DotNetProjectReferencesTfm = new(
        id: "GUA0003",
        title: SR.GUA0003_DotNetProjectReferencesTfm_Title,
        messageFormat: SR.GUA0003_DotNetProjectReferencesTfm_MessageFormat,
        description: SR.GUA0003_DotNetProjectReferencesTfm_Description,
        category: Category,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    public static readonly DiagnosticDescriptor GUA0004_DotNetProjectReferencesOldBindings = new(
        id: "GUA0004",
        title: SR.GUA0004_DotNetProjectReferencesOldBindings_Title,
        messageFormat: SR.GUA0004_DotNetProjectReferencesOldBindings_MessageFormat,
        description: SR.GUA0004_DotNetProjectReferencesOldBindings_Description,
        category: Category,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    public static readonly DiagnosticDescriptor GUA0005_DotNetSolutionConfiguration = new(
        id: "GUA0005",
        title: SR.GUA0005_DotNetSolutionConfiguration_Title,
        messageFormat: SR.GUA0005_DotNetSolutionConfiguration_MessageFormat,
        description: SR.GUA0005_DotNetSolutionConfiguration_Description,
        category: Category,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    public static readonly DiagnosticDescriptor GUA0006_DotNetProjectEnablePreview = new(
        id: "GUA0006",
        title: SR.GUA0006_DotNetProjectEnablePreview_Title,
        messageFormat: SR.GUA0006_DotNetProjectEnablePreview_MessageFormat,
        description: SR.GUA0006_DotNetProjectEnablePreview_Description,
        category: Category,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    #endregion

    #region 1XXX: DiagnosticAnalyzer and CodeFixProvider

    public static readonly DiagnosticDescriptor GUA1001_AddPartialKeywordToGodotClasses = new(
        id: "GUA1001",
        title: SR.GUA1001_AddPartialKeywordToGodotClasses_Title,
        messageFormat: SR.GUA1001_AddPartialKeywordToGodotClasses_MessageFormat,
        description: SR.GUA1001_AddPartialKeywordToGodotClasses_Description,
        category: Category,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    public static readonly DiagnosticDescriptor GUA1002_NotImplementedSymbol = new(
        id: "GUA1002",
        title: SR.GUA1002_NotImplementedSymbol_Title,
        messageFormat: SR.GUA1002_NotImplementedSymbol_MessageFormat,
        description: SR.GUA1002_NotImplementedSymbol_Description,
        category: Category,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    public static readonly DiagnosticDescriptor GUA1003_RemovedSymbol = new(
        id: "GUA1003",
        title: SR.GUA1003_RemovedSymbol_Title,
        messageFormat: SR.GUA1003_RemovedSymbol_MessageFormat,
        description: SR.GUA1003_RemovedSymbol_Description,
        category: Category,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    public static readonly DiagnosticDescriptor GUA1004_ReplacedSymbol = new(
        id: "GUA1004",
        title: SR.GUA1004_ReplacedSymbol_Title,
        messageFormat: SR.GUA1004_ReplacedSymbol_MessageFormat,
        description: SR.GUA1004_ReplacedSymbol_Description,
        category: Category,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    public static readonly DiagnosticDescriptor GUA1005_MergedRpcAttributes = new(
        id: "GUA1005",
        title: SR.GUA1005_MergedRpcAttributes_Title,
        messageFormat: SR.GUA1005_MergedRpcAttributes_MessageFormat,
        description: SR.GUA1005_MergedRpcAttributes_Description,
        category: Category,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    public static readonly DiagnosticDescriptor GUA1006_AddGodotClassAttribute = new(
        id: "GUA1006",
        title: SR.GUA1006_AddGodotClassAttribute_Title,
        messageFormat: SR.GUA1006_AddGodotClassAttribute_MessageFormat,
        description: SR.GUA1006_AddGodotClassAttribute_Description,
        category: Category,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    public static readonly DiagnosticDescriptor GUA1007_AddBindPropertyAttribute = new(
        id: "GUA1007",
        title: SR.GUA1007_AddBindPropertyAttribute_Title,
        messageFormat: SR.GUA1007_AddBindPropertyAttribute_MessageFormat,
        description: SR.GUA1007_AddBindPropertyAttribute_Description,
        category: Category,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    public static readonly DiagnosticDescriptor GUA1008_AddBindMethodAttribute = new(
        id: "GUA1008",
        title: SR.GUA1008_AddBindMethodAttribute_Title,
        messageFormat: SR.GUA1008_AddBindMethodAttribute_MessageFormat,
        description: SR.GUA1008_AddBindMethodAttribute_Description,
        category: Category,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    public static readonly DiagnosticDescriptor GUA1009_VirtualMethodAccessibility = new(
        id: "GUA1009",
        title: SR.GUA1009_VirtualMethodAccessibility_Title,
        messageFormat: SR.GUA1009_VirtualMethodAccessibility_MessageFormat,
        description: SR.GUA1009_VirtualMethodAccessibility_Description,
        category: Category,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    public static readonly DiagnosticDescriptor GUA1010_DeltaParameterType = new(
        id: "GUA1010",
        title: SR.GUA1010_DeltaParameterType_Title,
        messageFormat: SR.GUA1010_DeltaParameterType_MessageFormat,
        description: SR.GUA1010_DeltaParameterType_Description,
        category: Category,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    public static readonly DiagnosticDescriptor GUA1011_SystemArrayToPackedArray = new(
        id: "GUA1011",
        title: SR.GUA1011_SystemArrayToPackedArray_Title,
        messageFormat: SR.GUA1011_SystemArrayToPackedArray_MessageFormat,
        description: SR.GUA1011_SystemArrayToPackedArray_Description,
        category: Category,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    #endregion
}
