using System;

namespace Godot.UpgradeAssistant.Providers;

internal sealed class ApiMapEntry
{
    private bool _isReadOnly;

    private ApiDescriptor? _keyDescriptor;

    private ApiDescriptor? _valueDescriptor;

    private string? _key;

    private string? _value;

    private ApiMapKind _kind = ApiMapKind.Type;

    private ApiMapState _state = ApiMapState.Replaced;

    private string? _version;

    private bool _isStatic;

    private bool _isAsync;

    private bool _isExtension;

    private bool _needsManualUpgrade;

    private bool _needsTodoInComment;

    private string? _messageId;

    private string[]? _messageParams;

    private string? _documentationUrl;

    /// <summary>
    /// API descriptor for <see cref="Key"/>.
    /// </summary>
    public ApiDescriptor KeyDescriptor =>
        _keyDescriptor ??= ApiDescriptor.CreateFromFullName(Key, Kind);

    /// <summary>
    /// API descriptor for <see cref="Value"/>.
    /// </summary>
    public ApiDescriptor? ValueDescriptor =>
        _valueDescriptor ??= ApiDescriptor.CreateFromFullName(Value, Kind);

    /// <summary>
    /// Original API that was changed.
    /// </summary>
    public string Key
    {
        // We assume key is not null because it should be assigned the first time
        // this entry is retrieved, before it the property is ever acccessed.
        get => _key!;
        set
        {
            if (_key == value)
            {
                // Ignore if the new value is the same.
                // This avoids the read-only exception and it's fine because there are no changes.
                return;
            }

            ThrowIfReadOnly();
            _key = value;
        }
    }

    /// <summary>
    /// New API that replaces the old one, empty if <see cref="State"/> is not <see cref="ApiMapState.Replaced"/>.
    /// </summary>
    public string? Value
    {
        get => _value;
        set
        {
            ThrowIfReadOnly();
            _value = value;
        }
    }

    /// <summary>
    /// Indicates the kind of API that was changed.
    /// </summary>
    public ApiMapKind Kind
    {
        get => _kind;
        set
        {
            ThrowIfReadOnly();
            _kind = value;
        }
    }

    /// <summary>
    /// Indicates the state of the API, i.e.: how the API has changed.
    /// </summary>
    public ApiMapState State
    {
        get => _state;
        set
        {
            ThrowIfReadOnly();
            _state = value;
        }
    }

    /// <summary>
    /// Version of Godot that introduced the rename.
    /// </summary>
    public string? Version
    {
        get => _version;
        set
        {
            ThrowIfReadOnly();
            _version = value;
        }
    }

    /// <summary>
    /// Indicates whether the API is static.
    /// </summary>
    public bool IsStatic
    {
        get => _isStatic;
        set
        {
            ThrowIfReadOnly();
            _isStatic = value;
        }
    }

    /// <summary>
    /// Indicates whether the API is async.
    /// </summary>
    public bool IsAsync
    {
        get => _isAsync;
        set
        {
            ThrowIfReadOnly();
            _isAsync = value;
        }
    }

    /// <summary>
    /// Indicates whether the API is an extension method.
    /// Only valid when <see cref="Kind"/> is <see cref="ApiMapKind.Method"/>.
    /// </summary>
    public bool IsExtension
    {
        get => _isExtension;
        set
        {
            ThrowIfReadOnly();
            _isExtension = value;
        }
    }

    /// <summary>
    /// Indicates that the API needs to be upgraded manually, the tool is unable to automate it.
    /// </summary>
    public bool NeedsManualUpgrade
    {
        get => _needsManualUpgrade;
        set
        {
            ThrowIfReadOnly();
            _needsManualUpgrade = value;
        }
    }

    /// <summary>
    /// If <see langword="true"/>, a <c>TODO(GUA):</c> prefix is added to the comment message.
    /// </summary>
    public bool NeedsTodoInComment
    {
        get => _needsTodoInComment;
        set
        {
            ThrowIfReadOnly();
            _needsTodoInComment = value;
        }
    }

    /// <summary>
    /// The resource ID to use for the comment message.
    /// If not provided, a default message will be used instead.
    /// </summary>
    /// <remarks>
    /// This should only be used when a custom message is required.
    /// Make sure the message exists in the <c>Strings.resx</c> file contained in this project.
    /// </remarks>
    public string? MessageId
    {
        get => _messageId;
        set
        {
            ThrowIfReadOnly();
            _messageId = value;
        }
    }

    /// <summary>
    /// The parameters to be passed into the string format for the custom comment message
    /// that is used if <see cref="MessageId"/> was provided.
    /// </summary>
    public string[]? MessageParams
    {
        get => _messageParams;
        set
        {
            ThrowIfReadOnly();
            _messageParams = value;
        }
    }

    /// <summary>
    /// Link to a documentation page that explains the API change.
    /// The documentation page may list alternative APIs that can be used instead, or steps
    /// that the user can take to finish the upgrade manually.
    /// </summary>
    public string? DocumentationUrl
    {
        get => _documentationUrl;
        set
        {
            ThrowIfReadOnly();
            _documentationUrl = value;
        }
    }

    public string? GetComment()
    {
        if (!TryGetCustomComment(out string? comment))
        {
            comment = GetDefaultComment();
        }

        if (NeedsTodoInComment)
        {
            comment = $"TODO(GUA): {comment}";
        }

        return comment;
    }

    /// <summary>
    /// Get a custom comment to use when <see cref="MessageId"/> is provided.
    /// </summary>
    private bool TryGetCustomComment(out string? customComment)
    {
        if (!string.IsNullOrEmpty(MessageId))
        {
            string? messageFormat = SR.GetResourceString(MessageId);
            if (!string.IsNullOrEmpty(messageFormat))
            {
                if (MessageParams is null or { Length: 0 })
                {
                    customComment = messageFormat;
                    return true;
                }

                customComment = string.Format(SR.Culture, messageFormat, MessageParams);
                return true;
            }
        }

        customComment = null;
        return false;
    }

    /// <summary>
    /// Get a default comment to use when <see cref="MessageId"/> is not provided.
    /// </summary>
    private string? GetDefaultComment()
    {
        switch (State)
        {
            case ApiMapState.NotImplemented:
            {
                if (!string.IsNullOrEmpty(DocumentationUrl))
                {
                    return SR.FormatApiNotImplementedCommentWithDocumentationUrl(Key, DocumentationUrl);
                }
                else
                {
                    return SR.FormatApiNotImplementedComment(Key);
                }
            }

            case ApiMapState.Removed:
            {
                if (!string.IsNullOrEmpty(DocumentationUrl))
                {
                    return SR.FormatApiRemovedCommentWithDocumentationUrl(Key, DocumentationUrl);
                }
                else
                {
                    return SR.FormatApiRemovedComment(Key);
                }
            }

            case ApiMapState.Replaced:
            {
                if (!string.IsNullOrEmpty(DocumentationUrl))
                {
                    return SR.FormatApiReplacedCommentWithDocumentationUrl(Key, Value, DocumentationUrl);
                }
                else
                {
                    return SR.FormatApiReplacedComment(Key, Value);
                }
            }
        }

        return null;
    }

    public void MakeReadOnly()
    {
        _isReadOnly = true;
    }

    private void ThrowIfReadOnly()
    {
        if (_isReadOnly)
        {
            throw new InvalidOperationException(SR.InvalidOperation_ApiMapEntryIsReadOnly);
        }
    }
}
