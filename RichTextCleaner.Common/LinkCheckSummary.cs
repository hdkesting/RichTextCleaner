namespace RichTextCleaner.Common
{
    /// <summary>
    /// The result of checking a particular link.
    /// </summary>
    public enum LinkCheckSummary
    {
        /// <summary>
        /// The link is not checked yet.
        /// </summary>
        NotCheckedYet,

        /// <summary>
        /// The link checks out fine.
        /// </summary>
        Ok,

        /// <summary>
        /// The link is ignored (is not a URL or not external).
        /// </summary>
        Ignored,

        /// <summary>
        /// The link gets redirected.
        /// </summary>
        Redirected,

        /// <summary>
        /// The schema changed (from http to https, presumably).
        /// </summary>
        SchemaChange,

        /// <summary>
        /// The link is not found (returns a 404 status).
        /// </summary>
        NotFound,

        /// <summary>
        /// The link results in an error (4xx, 5xx)
        /// </summary>
        Error,

        /// <summary>
        /// The link results in a timeout.
        /// </summary>
        Timeout,

        /// <summary>
        /// The link is updated.
        /// </summary>
        Updated
    }
}
