
using System;
using System.Linq;
using System.Security;
using System.Text;
using Godot.BindingsGeneration.Reflection;
using Godot.Common;

namespace Godot.BindingsGeneration;

internal sealed class XmlDocConverter
{
    private readonly TypeDB _typeDB;

    public XmlDocConverter(TypeDB typeDB)
    {
        _typeDB = typeDB;
    }

    /// <summary>
    /// Convert BBCode documentation to the XMLDoc format used by C#.
    /// </summary>
    /// <param name="bbCode">Original BBCode documentation.</param>
    /// <param name="currentType">Current type to resolve member references that aren't fully qualified.</param>
    /// <returns>Converted XMLDoc string.</returns>
    public string? Convert(string? bbCode, TypeInfo? currentType = null)
    {
        if (string.IsNullOrEmpty(bbCode))
        {
            return null;
        }

        var sb = new StringBuilder();

        sb.AppendLine("<summary>");
        sb.Append("<para>");

        bool inCodeTag = false;
        bool inCodeBlocksTag = false;

        var parser = new BBCodeParser(bbCode);
        while (parser.Read())
        {
            switch (parser.TokenType)
            {
                case BBCodeTokenType.Text:
                    if (inCodeTag)
                    {
                        // Inside a [code]/[codeblock]/[csharp] block, preserve the text exactly as-is.
                        sb.AppendEscapedXml(parser.ValueSpan);
                        break;
                    }

                    if (inCodeBlocksTag)
                    {
                        // Inside a [codeblocks] wrapper but not its active code section (e.g. the
                        // [gdscript] block); ignore the text.
                        continue;
                    }

                    // Normal prose: split into paragraphs.
                    ReadOnlySpan<char> textSpan = parser.ValueSpan;
                    int newLineIndex;
                    while ((newLineIndex = textSpan.IndexOf('\n')) != -1)
                    {
                        sb.AppendEscapedXml(textSpan.Slice(0, newLineIndex));
                        sb.AppendLine("</para>");
                        sb.Append("<para>");
                        textSpan = textSpan.Slice(newLineIndex + 1);
                    }
                    sb.AppendEscapedXml(textSpan);
                    break;

                case BBCodeTokenType.StartTag:
                    var startTag = parser.GetStartTag();

                    // When we're inside a code context, recognized inline tags (e.g. [b], [i], [url])
                    // must be preserved as literal text instead of being emitted as XML elements.
                    // Otherwise an array subscript like 'a[i]' would emit a stray <i> element and
                    // produce malformed XML doc comments (CS1570). Only the tags that manage the code
                    // state fall through to the switch below so the block can still be opened/closed.
                    if (TryHandleTagInCodeContext(sb, startTag.TagName, parser.ValueSpan, inCodeTag, inCodeBlocksTag))
                    {
                        continue;
                    }

                    switch (startTag.TagName)
                    {
                        case "b":
                            sb.Append("<b>");
                            break;

                        case "i":
                            sb.Append("<i>");
                            break;

                        case "u":
                            sb.Append("<u>");
                            break;

                        case "s":
                        case "center":
                        case "color":
                        case "font":
                            // These tags are not supported in XMLDoc, but we need to explicitly handle them
                            // to avoid outputting the raw text. They don't seem to be used in the current
                            // documentation either, but we handle them just in case.
                            break;

                        case "br":
                            sb.AppendLine("</para>");
                            sb.Append("<para>");
                            break;

                        case "kbd":
                        {
                            scoped ReadOnlySpan<char> content = ConsumeToMatchingEndTag(ref parser);
                            sb.Append("<c>");
                            sb.AppendEscapedXml(content);
                            sb.Append("</c>");
                            break;
                        }

                        case "url":
                            scoped ReadOnlySpan<char> linkUrl;
                            scoped ReadOnlySpan<char> linkText;
                            if (startTag.HasAttributes())
                            {
                                linkUrl = startTag.EnumerateAttributes().First();
                                linkText = ConsumeNextTextAndEndTag(ref parser);
                            }
                            else
                            {
                                linkUrl = linkText = ConsumeNextTextAndEndTag(ref parser);
                            }
                            sb.Append("<a href=\"");
                            sb.Append(linkUrl);
                            sb.Append("\">");
                            sb.Append(linkText);
                            sb.Append("</a>");
                            break;

                        case "img":
                            scoped ReadOnlySpan<char> imageUrl = ConsumeNextTextAndEndTag(ref parser);
                            // Not supported. Just append the bbcode.
                            sb.Append("[img]");
                            sb.Append(imageUrl);
                            sb.Append("[/img]");
                            break;

                        case "code":
                        {
                            scoped ReadOnlySpan<char> content = ConsumeToMatchingEndTag(ref parser);
                            switch (content)
                            {
                                case "true":
                                case "false":
                                case "null":
                                    sb.Append("<see langword=\"");
                                    sb.Append(content);
                                    sb.Append("\"/>");
                                    break;

                                case "^\"\"":
                                    // Special case for an empty StringName when using GDScript syntax.
                                    sb.Append("<c>");
                                    sb.AppendEscapedXml("\"\"");
                                    sb.Append("</c>");
                                    break;

                                default:
                                    sb.Append("<c>");
                                    sb.AppendEscapedXml(content);
                                    sb.Append("</c>");
                                    break;
                            }
                            break;
                        }

                        case "codeblock":
                            sb.Append("<code>");
                            inCodeTag = true;
                            break;

                        case "codeblocks":
                            inCodeBlocksTag = true;
                            break;

                        case "csharp":
                            sb.Append("<code>");
                            inCodeTag = true;
                            break;

                        default:
                            // Check if this tag is one of the reference tags.
                            if (TryAppendReference(sb, startTag, currentType))
                            {
                                // Successfully appended a reference for this tag,
                                // so we can move on to the next token.
                                continue;
                            }

                            // We could not find a reference for this tag or it's an unrecognized tag,
                            // just output the raw text.
                            sb.AppendEscapedXml(parser.ValueSpan);
                            break;
                    }
                    break;

                case BBCodeTokenType.EndTag:
                    // We only need to check for the end tags that aren't self-closing.
                    // We also consume some end tags in the StartTag case, so those won't show up here either.
                    var endTag = parser.GetEndTag();

                    // Mirror the StartTag handling: inside a code context, recognized inline end tags
                    // (e.g. [/b], [/i]) must be preserved as literal text instead of being emitted as
                    // XML elements. Only the tags that manage the code state fall through to the switch
                    // below so the code block can still be closed correctly.
                    if (TryHandleTagInCodeContext(sb, endTag.TagName, parser.ValueSpan, inCodeTag, inCodeBlocksTag))
                    {
                        continue;
                    }

                    switch (endTag.TagName)
                    {
                        case "b":
                            sb.Append("</b>");
                            break;

                        case "i":
                            sb.Append("</i>");
                            break;

                        case "u":
                            sb.Append("</u>");
                            break;

                        case "s":
                        case "center":
                        case "color":
                        case "font":
                            // These tags are not supported in XMLDoc, but we need to explicitly handle them
                            // to avoid outputting the raw text. They don't seem to be used in the current
                            // documentation either, but we handle them just in case.
                            break;

                        case "codeblock":
                            sb.Append("</code>");
                            inCodeTag = false;
                            break;

                        case "codeblocks":
                            inCodeBlocksTag = false;
                            if (inCodeTag)
                            {
                                // A [csharp]/[codeblock] block was left open (malformed input); close its
                                // <code> element so we don't emit unbalanced XML.
                                sb.Append("</code>");
                                inCodeTag = false;
                            }
                            break;

                        case "csharp":
                            sb.Append("</code>");
                            inCodeTag = false;
                            break;

                        default:
                            // Unrecognized end tag, just output the raw text.
                            sb.AppendEscapedXml(parser.ValueSpan);
                            break;
                    }
                    break;
            }
        }

        sb.AppendLine("</para>");
        sb.AppendLine("</summary>");
        return sb.ToString();
    }

    private static bool IsCodeStateTag(ReadOnlySpan<char> tagName) => tagName switch
    {
        // These tags open or close a code context, so they must always be handled (to emit or
        // close the <code> element and toggle the state flags) even while we're inside a code
        // context passing every other tag through as literal text.
        "codeblock" => true,
        "codeblocks" => true,
        "csharp" => true,
        _ => false,
    };

    /// <summary>
    /// Handles a tag encountered while inside a code context. Recognized tags are preserved as
    /// literal text inside a [code]/[codeblock]/[csharp] block, or ignored inside the non-active
    /// section of a [codeblocks] block. Returns <see langword="true"/> if the tag was handled and
    /// the caller should skip its normal processing.
    /// </summary>
    private static bool TryHandleTagInCodeContext(StringBuilder sb, ReadOnlySpan<char> tagName, ReadOnlySpan<char> rawTag, bool inCodeTag, bool inCodeBlocksTag)
    {
        if ((!inCodeTag && !inCodeBlocksTag) || IsCodeStateTag(tagName))
        {
            return false;
        }

        if (inCodeTag)
        {
            // Inside a code block: preserve the tag as literal text.
            sb.AppendEscapedXml(rawTag);
        }
        // else: inside a [codeblocks] wrapper but not its active code section -> ignore.
        return true;
    }

    private ref struct Reference
    {
        public ReadOnlySpan<char> Value;
        public bool Found;

        /// <summary>
        /// Creates a reference for the matching C# type or member that was found
        /// for the given engine type or member name. The reference will be included
        /// with <c>&lt;see cref="..."&gt;</c> tags.
        /// </summary>
        public static Reference From(ReadOnlySpan<char> value) => new Reference(value, true);

        /// <summary>
        /// Creates a reference for an engine type or member name that was not found in the C# bindings,
        /// even though the BBCode tag is recognized. This can happen if the engine type or member
        /// is not exposed in the C# bindings, the documentation has a typo, or the mappings are missing
        /// some manually implemented APIs. The reference will be included with <c>&lt;c&gt;</c> tags.
        /// </summary>
        public static Reference Unknown(ReadOnlySpan<char> value) => new Reference(value, false);

        /// <summary>
        /// Creates a reference for an unrecognized or unsupported BBCode tag. In this case we can't
        /// even provide a reference to the engine type or member name, so the documentation will have
        /// to include the raw text of the BBCode tag instead.
        /// </summary>
        public static Reference Unrecognized() => new Reference(default, false);

        private Reference(ReadOnlySpan<char> value, bool found)
        {
            Value = value;
            Found = found;
        }
    }

    private bool TryAppendReference(StringBuilder sb, BBCodeStartTag startTag, TypeInfo? currentType)
    {
        // HARDCODED: Special case for '@GlobalScope' and '@GDScript' references.
        if (!startTag.HasAttributes() && startTag.TagName.StartsWith('@'))
        {
            string linkName = startTag.TagName.Slice(1).ToString().ToLowerInvariant();
            sb.Append("<a href=\"https://docs.godotengine.org/en/stable/classes/class_%40");
            sb.Append(linkName);
            sb.Append(".html\">");
            sb.Append(startTag.TagName);
            sb.Append("</a>");
            return true;
        }

        if (startTag.TagName.SequenceEqual("param") && startTag.HasAttributes())
        {
            // We'll just assume the parameter corresponds to a parameter on the current method
            // and the name will match after converting to follow C# naming conventions.
            string parameterName = startTag.EnumerateAttributes().First().ToString();
            parameterName = NamingUtils.SnakeToCamelCase(parameterName);

            sb.Append("<paramref name=\"");
            sb.Append(parameterName);
            sb.Append("\"/>");
            return true;
        }

        if (startTag.TagName.SequenceEqual("method") && startTag.HasAttributes())
        {
            ReadOnlySpan<char> methodName = startTag.EnumerateAttributes().First();
            switch (methodName)
            {
                case "@GlobalScope.typeof":
                case "typeof":
                    // Special case for the typeof operator.
                    sb.Append("<see langword=\"typeof\"/>");
                    return true;

                case "Object._init":
                case "_init" when currentType == KnownTypes.GodotObject:
                    // Special case for the _init method on Object,
                    // which is equivalent to the constructor in C#.
                    sb.Append("<see cref=\"global::Godot.GodotObject.GodotObject()\"/>");
                    return true;
            }
        }

        Reference reference = startTag.TagName switch
        {
            // Constructors and operators are currently only used in documentation for built-in types,
            // but we don't generate C# bindings for these types, so these tags don't need to be supported.
            // If these tags were ever used in documentation for types that do have C# bindings, we can add support.
            "constructor" => GetUnknownReference(startTag),
            "operator" => GetUnknownReference(startTag),

            "method" => FindMemberReference(startTag, currentType, _typeDB),

            "member" => FindMemberReference(startTag, currentType, _typeDB),

            "signal" => FindMemberReference(startTag, currentType, _typeDB),

            "enum" => FindMemberReference(startTag, currentType, _typeDB),

            "constant" => FindMemberReference(startTag, currentType, _typeDB),

            "annotation" => FindAnnotationReference(startTag),

            // C# bindings don't generate theme items, so there's nothing to reference.
            "theme_item" => GetUnknownReference(startTag),

            _ when !startTag.HasAttributes() => FindTypeReference(startTag.TagName, _typeDB),

            // Unrecognized tag.
            _ => Reference.Unrecognized(),
        };

        if (reference.Value.IsEmpty)
        {
            // Could not find a valid reference for this tag.
            return false;
        }

        if (!reference.Found)
        {
            // The reference did not have a matching C# type or member,
            // so we'll just output the engine type or member name as-is.
            sb.Append("<c>");
            sb.Append(reference.Value);
            sb.Append("</c>");
            return true;
        }

        sb.Append("<see cref=\"");
        sb.Append(reference.Value);
        sb.Append("\"/>");
        return true;

        static Reference FindMemberReference(BBCodeStartTag startTag, TypeInfo? currentType, TypeDB typeDB)
        {
            ReadOnlySpan<char> engineMemberName = startTag.EnumerateAttributes().First();

            // Try the global member mappings first, in case there is a hardcoded special case.
            if (typeDB.TryGetGlobalMemberMapping(engineMemberName, out string? memberName))
            {
                return Reference.From(memberName);
            }

            // Check if the member name if qualified by a type.
            ReadOnlySpan<char> originalMemberName = engineMemberName;
            int typeSeparatorIndex = engineMemberName.IndexOf('.');
            if (typeSeparatorIndex != -1)
            {
                ReadOnlySpan<char> engineTypeName = engineMemberName[..typeSeparatorIndex];
                engineMemberName = engineMemberName[(typeSeparatorIndex + 1)..];

                if (!typeDB.TryGetTypeFromEngineName(engineTypeName, out var type))
                {
                    // No type found for this engine type name, so we can't reference it.
                    return Reference.Unknown(originalMemberName);
                }

                // This member is qualified by a type, so we'll use that type
                // instead of the current one to resolve the member.
                currentType = type;
            }

            if (currentType is null || !typeDB.TryGetMemberMapping(currentType, engineMemberName, out memberName))
            {
                // No member found for this engine member name, so we can't reference it.
                return Reference.Unknown(originalMemberName);
            }

            return Reference.From(memberName);
        }

        static Reference FindAnnotationReference(BBCodeStartTag startTag)
        {
            // These reference GDScript annotations which often don't have an equivalent in C#,
            // but for those that do we can maintain a hardcoded mapping here.
            ReadOnlySpan<char> annotationName = startTag.EnumerateAttributes().First();
            return annotationName switch
            {
                "@GDScript.@export" => Reference.From("global::Godot.BindPropertyAttribute"),
                "@GDScript.@rpc" => Reference.From("global::Godot.RpcAttribute"),

                // Unrecognized annotation or no equivalent exists in C#.
                _ => Reference.Unknown(annotationName),
            };
        }

        static Reference FindTypeReference(ReadOnlySpan<char> engineTypeName, TypeDB typeDB)
        {
            if (!typeDB.TryGetTypeFromEngineName(engineTypeName, out var type))
            {
                // No type found for this engine type name, so we can't reference it.
                // We return 'Unrecognized' instead of 'Unknown' here because type tags are
                // just the name of the type, so we can't differentiate between an unrecognized
                // tag and a recognized tag that doesn't have a matching C# type.
                return Reference.Unrecognized();
            }

            return Reference.From(type.FullNameWithGlobal);
        }

        static Reference GetUnknownReference(BBCodeStartTag startTag)
        {
            ReadOnlySpan<char> content = startTag.EnumerateAttributes().First();
            return Reference.Unknown(content);
        }
    }

    private static ReadOnlySpan<char> ConsumeNextTextAndEndTag(ref BBCodeParser parser)
    {
        var startTag = parser.GetStartTag();

        if (!parser.Read())
        {
            throw new InvalidOperationException($"Expected content after start tag '{startTag.TagName}'.");
        }

        ReadOnlySpan<char> content = parser.ValueSpan;

        if (!parser.Read())
        {
            throw new InvalidOperationException($"Expected end tag after content '{content}' for start tag '{startTag.TagName}'.");
        }

        var endTag = parser.GetEndTag();
        if (!endTag.TagName.SequenceEqual(startTag.TagName))
        {
            throw new InvalidOperationException($"Expected end tag for '{startTag.TagName}' but got '{endTag.TagName}'.");
        }

        return content;
    }

    private static ReadOnlySpan<char> ConsumeToMatchingEndTag(ref BBCodeParser parser)
    {
        var startTag = parser.GetStartTag();

        if (!parser.ReadToEndTag())
        {
            throw new InvalidOperationException($"Expected content after start tag '{startTag.TagName}'.");
        }

        ReadOnlySpan<char> content = parser.ValueSpan;

        if (!parser.Read())
        {
            throw new InvalidOperationException($"Expected end tag after content '{content}' for start tag '{startTag.TagName}'.");
        }

        var endTag = parser.GetEndTag();
        if (!endTag.TagName.SequenceEqual(startTag.TagName))
        {
            throw new InvalidOperationException($"Expected end tag for '{startTag.TagName}' but got '{endTag.TagName}'.");
        }

        return content;
    }
}

file static class StringBuilderExtensions
{
    extension(StringBuilder sb)
    {
        public void AppendEscapedXml(ReadOnlySpan<char> text)
        {
            sb.Append(SecurityElement.Escape(text.ToString()));
        }
    }
}
