using RichTextCleaner.Common.Support;
using System;

namespace RichTextCleaner
{
    internal class CleanerSettings : ICleanerSettings
    {
        private static readonly Lazy<CleanerSettings> lazyInstance = new Lazy<CleanerSettings>(() => new CleanerSettings());

        /// <summary>
        /// Prevents a default instance of the <see cref="CleanerSettings"/> class from being created.
        /// </summary>
        private CleanerSettings()
        {
        }

        public static readonly CleanerSettings Default = lazyInstance.Value;

        public StyleElements MarkupToRemove => StyleElements.All;

        public bool AddTargetBlank => true;

        public QuoteProcessing QuoteProcess => QuoteProcessing.ChangeToSmartQuotes;

        public LinkQueryCleanLevel QueryCleanLevel => LinkQueryCleanLevel.RemoveTrackingParams;

        public bool CreateLinkFromText => true;
    }
}
