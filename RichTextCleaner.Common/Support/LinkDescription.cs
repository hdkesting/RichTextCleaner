namespace RichTextCleaner.Common.Support
{
    /// <summary>
    /// The description of a link.
    /// </summary>
    public class LinkDescription
    {
        /// <summary>
        /// Gets or sets the text that is linked.
        /// </summary>
        /// <value>
        /// The link text.
        /// </value>
        public string LinkText { get; set; }

        /// <summary>
        /// Gets or sets the original link.
        /// </summary>
        /// <value>
        /// The original link.
        /// </value>
        public string OriginalLink { get; set; }

        /// <summary>
        /// Gets or sets the check result.
        /// </summary>
        /// <value>
        /// The result.
        /// </value>
        public LinkCheckSummary Result { get; set; }

        /// <summary>
        /// Gets or sets the link after redirect (if any).
        /// </summary>
        /// <value>
        /// The link after redirect.
        /// </value>
        public string LinkAfterRedirect { get; set; }
    }
}
