using RichTextCleaner.Common.Support;

namespace RichTextCleaner.Test
{
    internal class DefaultSettings : ICleanerSettings
    {
        public StyleElements MarkupToRemove { get; set; } = StyleElements.Bold | StyleElements.Underline;

        public bool AddTargetBlank { get; set; } = true;

        public QuoteProcessing QuoteProcess => QuoteProcessing.ChangeToSmartQuotes;

        public LinkQueryCleanLevel QueryCleanLevel => LinkQueryCleanLevel.RemoveTrackingParams;

        public bool CreateLinkFromText { get; set; } = true;

        public bool AddRelNoOpener { get; set; } = true;
    }
}
