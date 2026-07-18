using Godot.BindingsGeneration.Reflection;

namespace Godot.BindingsGeneration.Tests;

public class XmlDocConverterTests
{
    [Theory]
    [InlineData("[b]Bold[/b]", "<b>Bold</b>")]
    [InlineData("[i]Italic[/i]", "<i>Italic</i>")]
    [InlineData("[u]Underline[/u]", "<u>Underline</u>")]
    [InlineData("[url=https://example.com]Link[/url]", "<a href=\"https://example.com\">Link</a>")]
    public void SimpleTags(string input, string expected)
    {
        // The converter wraps the output in a <summary> and <para> tags for each line.
        expected = $"""
            <summary>
            <para>{expected}</para>
            </summary>

            """;

        var xmlDocConverter = new XmlDocConverter(new TypeDB());
        string? actual = xmlDocConverter.Convert(input);
        Assert.Equal(expected, actual, ignoreLineEndingDifferences: true);
    }

    [Fact]
    public void CodeblockTagPreservesWhiteSpace()
    {
        var xmlDocConverter = new XmlDocConverter(new TypeDB());

        string? actual = xmlDocConverter.Convert("""
            [codeblock]
            static void Main(string[] args)
            {
                Console.WriteLine("This line is indented with spaces.");


            	Console.WriteLine("This line is indented with a tab.");
            }
            [/codeblock]
            """);

        string expected = """
            <summary>
            <para><code>
            static void Main(string[] args)
            {
                Console.WriteLine(&quot;This line is indented with spaces.&quot;);


            	Console.WriteLine(&quot;This line is indented with a tab.&quot;);
            }
            </code></para>
            </summary>

            """;

        Assert.Equal(expected, actual, ignoreLineEndingDifferences: true);
    }

    [Fact]
    public void CodeblockTagPreservesNestedBBCodeAndXml()
    {
        var xmlDocConverter = new XmlDocConverter(new TypeDB());

        string? actual = xmlDocConverter.Convert("""
            [codeblock]
            [GodotClass]
            public partial class MyNode : Node
            {
                [BindMethod]
                public GodotArray<int> MyMethod()
                {
                    return [1, 2, 3];
                }
            }
            [/codeblock]
            """);

        string expected = """
            <summary>
            <para><code>
            [GodotClass]
            public partial class MyNode : Node
            {
                [BindMethod]
                public GodotArray&lt;int&gt; MyMethod()
                {
                    return [1, 2, 3];
                }
            }
            </code></para>
            </summary>

            """;

        Assert.Equal(expected, actual, ignoreLineEndingDifferences: true);
    }

    [Fact]
    public void ParamReferenceWithValidParameterNameEmitsParamref()
    {
        var xmlDocConverter = new XmlDocConverter(new TypeDB());

        // The parameter 'some_arg' converts to 'someArg' which is a valid parameter,
        // so it should be emitted as a '<paramref>'.
        string? actual = xmlDocConverter.Convert("[param some_arg]", parameters: [new ParameterInfo("someArg", KnownTypes.SystemInt32)]);

        string expected = """
            <summary>
            <para><paramref name="someArg"/></para>
            </summary>

            """;

        Assert.Equal(expected, actual, ignoreLineEndingDifferences: true);
    }

    [Fact]
    public void ParamReferenceWithUnknownParameterNameEmitsCode()
    {
        var xmlDocConverter = new XmlDocConverter(new TypeDB());

        // The parameter 'some_arg' converts to 'someArg' which is NOT in the valid set,
        // so it should fall back to '<c>' instead of emitting an invalid '<paramref>'.
        string? actual = xmlDocConverter.Convert("[param some_arg]", parameters: []);

        string expected = """
            <summary>
            <para><c>someArg</c></para>
            </summary>

            """;

        Assert.Equal(expected, actual, ignoreLineEndingDifferences: true);
    }

    [Fact]
    public void ParamReferenceWithoutValidParameterNamesEmitsCode()
    {
        var xmlDocConverter = new XmlDocConverter(new TypeDB());

        // Signals (C# events) declare no parameters and pass no valid set, so the
        // '[param]' reference should collapse to '<c>' rather than an invalid '<paramref>'.
        string? actual = xmlDocConverter.Convert("[param some_arg]", parameters: null);

        string expected = """
            <summary>
            <para><c>someArg</c></para>
            </summary>

            """;

        Assert.Equal(expected, actual, ignoreLineEndingDifferences: true);
    }

    [Fact]
    public void References()
    {
        var typeDB = new TypeDB();
        var xmlDocConverter = new XmlDocConverter(typeDB);

        var currentType = new TypeInfo("SomeType", "SomeNamespace");
        typeDB.RegisterTypeName("SomeType", currentType);

        var someMethod = new MethodInfo("SomeMethod");
        currentType.DeclaredMethods.Add(someMethod);
        typeDB.RegisterMemberMapping(currentType, "some_method", someMethod);

        var someProperty = new PropertyInfo("SomeProperty", currentType);
        currentType.DeclaredProperties.Add(someProperty);
        typeDB.RegisterMemberMapping(currentType, "some_property", someProperty);

        var someConstant = new FieldInfo("SomeConstant", currentType);
        currentType.DeclaredFields.Add(someConstant);
        typeDB.RegisterMemberMapping(currentType, "SOME_CONSTANT", someConstant);

        var someEnum = new EnumInfo("SomeEnum");
        someEnum.DeclaredFields.Add(new FieldInfo("SomeEnumMember", KnownTypes.SystemInt32)
        {
            IsLiteral = true,
            DefaultValue = "0",
        });
        currentType.NestedTypes.Add(someEnum);
        typeDB.RegisterMemberMapping(currentType, "SomeEnum", someEnum);
        typeDB.RegisterMemberMapping(someEnum, "SOME_ENUM_MEMBER", $"{currentType.FullNameWithGlobal}.{someEnum.Name}.SomeEnumMember");
        typeDB.RegisterMemberMapping(currentType, "SOME_ENUM_MEMBER", $"{currentType.FullNameWithGlobal}.{someEnum.Name}.SomeEnumMember");

        var someSignal = new EventInfo("SomeSignal", new TypeInfo("SignalEventHandler"));
        currentType.DeclaredEvents.Add(someSignal);
        typeDB.RegisterMemberMapping(currentType, "some_signal", someSignal);

        var anotherType = new TypeInfo("AnotherType", "SomeNamespace");
        typeDB.RegisterTypeName("AnotherType", anotherType);

        var someOtherMethod = new MethodInfo("SomeOtherMethod");
        anotherType.DeclaredMethods.Add(someOtherMethod);
        typeDB.RegisterMemberMapping(anotherType, "some_other_method", someOtherMethod);

        var someOtherProperty = new PropertyInfo("SomeOtherProperty", anotherType);
        anotherType.DeclaredProperties.Add(someOtherProperty);
        typeDB.RegisterMemberMapping(anotherType, "some_other_property", someOtherProperty);

        var someOtherConstant = new FieldInfo("SomeOtherConstant", anotherType);
        anotherType.DeclaredFields.Add(someOtherConstant);
        typeDB.RegisterMemberMapping(anotherType, "SOME_OTHER_CONSTANT", someOtherConstant);

        var someOtherEnum = new EnumInfo("SomeOtherEnum");
        someOtherEnum.DeclaredFields.Add(new FieldInfo("SomeOtherEnumMember", KnownTypes.SystemInt32)
        {
            IsLiteral = true,
            DefaultValue = "0",
        });
        anotherType.NestedTypes.Add(someOtherEnum);
        typeDB.RegisterMemberMapping(anotherType, "SomeOtherEnum", someOtherEnum);
        typeDB.RegisterMemberMapping(someOtherEnum, "SOME_OTHER_ENUM_MEMBER", $"{anotherType.FullNameWithGlobal}.{someOtherEnum.Name}.SomeOtherEnumMember");
        typeDB.RegisterMemberMapping(anotherType, "SOME_OTHER_ENUM_MEMBER", $"{anotherType.FullNameWithGlobal}.{someOtherEnum.Name}.SomeOtherEnumMember");

        var someOtherSignal = new EventInfo("SomeOtherSignal", new TypeInfo("SignalEventHandler"));
        anotherType.DeclaredEvents.Add(someOtherSignal);
        typeDB.RegisterMemberMapping(anotherType, "some_other_signal", someOtherSignal);

        typeDB.RegisterGlobalMemberMapping("some_global_method", "global::SomeNamespace.GD.SomeGlobalMethod");
        typeDB.RegisterGlobalMemberMapping("some_global_property", "global::SomeNamespace.GD.SomeGlobalProperty");
        typeDB.RegisterGlobalMemberMapping("SOME_GLOBAL_CONSTANT", "global::SomeNamespace.GD.SomeGlobalConstant");
        typeDB.RegisterGlobalMemberMapping("SomeGlobalEnum", "global::SomeNamespace.SomeGlobalEnum");
        typeDB.RegisterGlobalMemberMapping("SOME_GLOBAL_ENUM_MEMBER", "global::SomeNamespace.SomeGlobalEnum.SomeGlobalEnumMember");
        typeDB.RegisterGlobalMemberMapping("some_global_signal", "global::SomeNamespace.GD.SomeGlobalSignal");

        string? actual = xmlDocConverter.Convert("""
            [method some_method] [method AnotherType.some_other_method]
            [member some_property] [method AnotherType.some_other_property]
            [constant SOME_CONSTANT] [constant AnotherType.SOME_OTHER_CONSTANT]
            [constant SOME_ENUM_MEMBER] [constant AnotherType.SOME_OTHER_ENUM_MEMBER]
            [enum SomeEnum] [enum AnotherType.SomeOtherEnum]
            [signal some_signal] [signal AnotherType.some_other_signal]

            [method some_global_method] [method @GlobalScope.some_global_method]
            [member some_global_property] [method @GlobalScope.some_global_property]
            [constant SOME_GLOBAL_CONSTANT] [constant @GlobalScope.SOME_GLOBAL_CONSTANT]
            [constant SOME_GLOBAL_ENUM_MEMBER] [constant @GlobalScope.SOME_GLOBAL_ENUM_MEMBER]
            [enum SomeGlobalEnum] [enum @GlobalScope.SomeGlobalEnum]
            [signal some_global_signal] [signal @GlobalScope.some_global_signal]
            """, currentType);

        string expected = """
            <summary>
            <para><see cref="global::SomeNamespace.SomeType.SomeMethod"/> <see cref="global::SomeNamespace.AnotherType.SomeOtherMethod"/></para>
            <para><see cref="global::SomeNamespace.SomeType.SomeProperty"/> <see cref="global::SomeNamespace.AnotherType.SomeOtherProperty"/></para>
            <para><see cref="global::SomeNamespace.SomeType.SomeConstant"/> <see cref="global::SomeNamespace.AnotherType.SomeOtherConstant"/></para>
            <para><see cref="global::SomeNamespace.SomeType.SomeEnum.SomeEnumMember"/> <see cref="global::SomeNamespace.AnotherType.SomeOtherEnum.SomeOtherEnumMember"/></para>
            <para><see cref="global::SomeNamespace.SomeType.SomeEnum"/> <see cref="global::SomeNamespace.AnotherType.SomeOtherEnum"/></para>
            <para><see cref="global::SomeNamespace.SomeType.SomeSignal"/> <see cref="global::SomeNamespace.AnotherType.SomeOtherSignal"/></para>
            <para></para>
            <para><see cref="global::SomeNamespace.GD.SomeGlobalMethod"/> <see cref="global::SomeNamespace.GD.SomeGlobalMethod"/></para>
            <para><see cref="global::SomeNamespace.GD.SomeGlobalProperty"/> <see cref="global::SomeNamespace.GD.SomeGlobalProperty"/></para>
            <para><see cref="global::SomeNamespace.GD.SomeGlobalConstant"/> <see cref="global::SomeNamespace.GD.SomeGlobalConstant"/></para>
            <para><see cref="global::SomeNamespace.SomeGlobalEnum.SomeGlobalEnumMember"/> <see cref="global::SomeNamespace.SomeGlobalEnum.SomeGlobalEnumMember"/></para>
            <para><see cref="global::SomeNamespace.SomeGlobalEnum"/> <see cref="global::SomeNamespace.SomeGlobalEnum"/></para>
            <para><see cref="global::SomeNamespace.GD.SomeGlobalSignal"/> <see cref="global::SomeNamespace.GD.SomeGlobalSignal"/></para>
            </summary>

            """;

        Assert.Equal(expected, actual, ignoreLineEndingDifferences: true);
    }
}
