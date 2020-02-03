using System.Net;

namespace RichTextCleaner.Common
{
    public class LinkCheckResult
    {
        public LinkCheckSummary Summary { get; private set; }

        public string NewLink { get; private set; }

        public HttpStatusCode HttpStatusCode { get; private set; }

        public LinkCheckResult(LinkCheckSummary summary, string link)
        {
            this.Summary = summary;
            this.NewLink = link;
        }

        public LinkCheckResult(LinkCheckSummary summary, string link, HttpStatusCode httpStatus)
        {
            this.Summary = summary;
            this.NewLink = link;
            this.HttpStatusCode = httpStatus;
        }
    }
}
