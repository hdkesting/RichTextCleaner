using RichTextCleaner.Common.Support;
using System;

namespace RichTextCleanerUwp.Models
{

    /// <summary>
    /// Strongly typed settings.
    /// </summary>
    /// <remarks>
    /// Settings are stored in C:\Users\{user}\AppData\Local\Hans_Kesting\RichTextCleaner.exe...\{version}\user.config
    /// </remarks>
    public class CleanerSettings: ICleanerSettings
    {
        private static readonly Lazy<CleanerSettings> lazyInstance = new Lazy<CleanerSettings>(() => new CleanerSettings());

        /// <summary>
        /// Prevents a default instance of the <see cref="CleanerSettings"/> class from being created.
        /// </summary>
        private CleanerSettings()
        {
            // TODO load/save
        }

        public static CleanerSettings Instance => lazyInstance.Value;

#pragma warning disable CA1822 // Mark members as static
        /// <summary>
        /// Gets or sets a value indicating whether to remove bold elements.
        /// </summary>
        /// <value>
        ///   <c>true</c> if remove bold; otherwise, <c>false</c>.
        /// </value>
        public bool RemoveBold { get; set; } = true;

        /// <summary>
        /// Gets or sets a value indicating whether to remove italic elements.
        /// </summary>
        /// <value>
        ///   <c>true</c> if remove italic; otherwise, <c>false</c>.
        /// </value>
        public bool RemoveItalic { get; set; } = false;

        /// <summary>
        /// Gets or sets a value indicating whether to remove underline elements.
        /// </summary>
        /// <value>
        ///   <c>true</c> if remove underline; otherwise, <c>false</c>.
        /// </value>
        public bool RemoveUnderline { get; set; } = true;

        /// <summary>
        /// Gets all the markup to remove.
        /// </summary>
        /// <value>
        /// The markup to remove.
        /// </value>
        public StyleElements MarkupToRemove =>
            (RemoveBold ? StyleElements.Bold : StyleElements.None) |
            (RemoveItalic ? StyleElements.Italic : StyleElements.None) |
            (RemoveUnderline ? StyleElements.Underline : StyleElements.None);

        /// <summary>
        /// Gets or sets a value indicating whether to add target=_blank to link elements.
        /// </summary>
        /// <value>
        ///   <c>true</c> if add target=_blank; otherwise, <c>false</c>.
        /// </value>
        public bool AddTargetBlank { get; set; } = true;

        /// <summary>
        /// Gets or sets a value indicating how to process quotes.
        /// </summary>
        /// <seealso cref="QuoteProcessing"/>
        /// <value>
        /// The quote process.
        /// </value>
        public QuoteProcessing QuoteProcess { get; set; } = QuoteProcessing.ChangeToSmartQuotes;

        /// <summary>
        /// Gets or sets the query clean level.
        /// </summary>
        /// <value>
        /// The query clean level.
        /// </value>
        public LinkQueryCleanLevel QueryCleanLevel => LinkQueryCleanLevel.RemoveQuery;

        /// <summary>
        /// Gets a value indicating whether to create links from link-like texts.
        /// </summary>
        /// <value>
        /// <c>true</c> if create link from text; otherwise, <c>false</c>.
        /// </value>
        public bool CreateLinkFromText { get; set; } = true;
#pragma warning restore CA1822 // Mark members as static
    }
}
