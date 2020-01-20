using System;

namespace RichTextCleaner.Common
{
    /// <summary>
    /// Style elements to process. Can be combined.
    /// </summary>
    [Flags]
    public enum StyleElements
    {
        /// <summary>
        /// No style elements to process.
        /// </summary>
        None = 0,

        /// <summary>
        /// Process "bold" elements (&lt;b&gt; and &lt;strong&gt;)
        /// </summary>
        Bold = 1,

        /// <summary>
        /// Process "italic" elements (&lt;i&gt; and &lt;em&gt;)
        /// </summary>
        Italic = 2,

        /// <summary>
        /// Process "underline" elements (&lt;u&gt;)
        /// </summary>
        Underline = 4,

        /// <summary>
        /// Process all style elements.
        /// </summary>
        All = Bold | Italic | Underline,
    }
}
