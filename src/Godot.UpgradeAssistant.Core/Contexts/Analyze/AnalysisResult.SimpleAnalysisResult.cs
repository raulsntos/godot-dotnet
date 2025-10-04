using System;

namespace Godot.UpgradeAssistant;

partial class AnalysisResult
{
    private class SimpleAnalysisResult : AnalysisResult
    {
        public object?[]? MessageArgs { get; init; }

        protected override string? GetMessageCore(IFormatProvider? formatProvider)
        {
            if (MessageFormat is null)
            {
                return null;
            }

            string localizedMessageFormat = MessageFormat.ToString(formatProvider);
            if (MessageArgs is null or { Length: 0 })
            {
                return localizedMessageFormat;
            }

            return string.Format(formatProvider, localizedMessageFormat, MessageArgs);
        }
    }
}
