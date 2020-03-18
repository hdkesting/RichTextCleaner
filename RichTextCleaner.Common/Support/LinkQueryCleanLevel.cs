namespace RichTextCleaner.Common.Support
{
    /// <summary>
    /// The level at which to clean the query part of a URL.
    /// </summary>
    public enum LinkQueryCleanLevel
    {
        /// <summary>
        /// Perform no query string cleaning.
        /// </summary>
        None,

        /// <summary>
        /// Remove only parameters starting with "utm".
        /// </summary>
        RemoveUtmParams,

        /// <summary>
        /// Remove the full query
        /// </summary>
        RemoveQuery
    }
}
