// <copyright file="ICleanerSettings.cs" company="Hans Kesting">
// Copyright (c) Hans Kesting. All rights reserved.
// </copyright>


namespace RichTextCleaner.Common.Support
{
    public interface ICleanerSettings
    {
        /// <summary>
        /// Gets all the markup to remove.
        /// </summary>
        /// <value>
        /// The markup to remove.
        /// </value>
        StyleElements MarkupToRemove { get; }

        /// <summary>
        /// Gets or sets a value indicating whether to add target=_blank to link elements.
        /// </summary>
        /// <value>
        ///   <c>true</c> if add target=_blank; otherwise, <c>false</c>.
        /// </value>
        bool AddTargetBlank { get; }

        /// <summary>
        /// Gets or sets a value indicating how to process quotes.
        /// </summary>
        /// <seealso cref="QuoteProcessing"/>
        /// <value>
        /// The quote process.
        /// </value>
        QuoteProcessing QuoteProcess { get; }

        /// <summary>
        /// Gets or sets the query clean level.
        /// </summary>
        /// <value>
        /// The query clean level.
        /// </value>
        LinkQueryCleanLevel QueryCleanLevel { get; }

        /// <summary>
        /// Gets a value indicating whether to create links from link-like texts.
        /// </summary>
        /// <value>
        ///   <c>true</c> if create link from text; otherwise, <c>false</c>.
        /// </value>
        bool CreateLinkFromText { get; }
    }
}
