using System.Net;

namespace RichTextCleaner.Common.Support
{
    /// <summary>
    /// The result of checking a link.
    /// </summary>
    public class LinkCheckResult
    {
        /// <summary>
        /// Gets the summary of the result.
        /// </summary>
        /// <value>
        /// The summary.
        /// </value>
        public LinkCheckSummary Summary { get; private set; }

        /// <summary>
        /// Gets the link that is redirected to.
        /// </summary>
        /// <value>
        /// The new link.
        /// </value>
        public string NewLink { get; private set; }

        /// <summary>
        /// Gets the HTTP status code of the check.
        /// </summary>
        /// <value>
        /// The HTTP status code.
        /// </value>
        public HttpStatusCode HttpStatusCode { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="LinkCheckResult"/> class.
        /// </summary>
        /// <param name="summary">The summary.</param>
        /// <param name="link">The link.</param>
        public LinkCheckResult(LinkCheckSummary summary, string link = null)
        {
            this.Summary = summary;
            this.NewLink = link;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LinkCheckResult"/> class.
        /// </summary>
        /// <param name="summary">The summary.</param>
        /// <param name="link">The link.</param>
        /// <param name="httpStatus">The HTTP status.</param>
        public LinkCheckResult(LinkCheckSummary summary, string link, HttpStatusCode httpStatus)
        {
            this.Summary = summary;
            this.NewLink = link;
            this.HttpStatusCode = httpStatus;
        }
    }
}
