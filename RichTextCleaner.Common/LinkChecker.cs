using HtmlAgilityPack;
using RichTextCleanerFW.Common.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;

namespace RichTextCleaner.Common
{
    public static class LinkChecker
    {
        private static readonly TimeSpan HttpTimeout = TimeSpan.FromSeconds(20);

        // https://docs.microsoft.com/en-us/dotnet/api/system.net.http.httpclient?view=netframework-4.8
        private static readonly HttpClientHandler clientHandler = new HttpClientHandler { AllowAutoRedirect = true };
        private static readonly HttpClient client = new HttpClient(clientHandler) { Timeout = HttpTimeout };

        /// <summary>
        /// Finds the links in the supplied HTML source.
        /// </summary>
        /// <param name="html">The HTML.</param>
        /// <returns></returns>
        public static List<LinkDescription> FindLinks(string html)
        {
            var doc = TextCleaner.CreateHtmlDocument(html);

            var links = doc.DocumentNode.SelectNodes("//a[@href]") ?? Enumerable.Empty<HtmlNode>();

            var result = new List<LinkDescription>();
            foreach (var link in links)
            {
                var lnk = new LinkDescription();
                lnk.LinkText = HttpUtility.HtmlDecode(link.InnerText);
                lnk.OriginalLink = link.GetAttributeValue("href", string.Empty);
                if (!lnk.OriginalLink.StartsWith("http", StringComparison.OrdinalIgnoreCase))
                {
                    // neither "http" nor "https"
                    lnk.Result = LinkCheckSummary.Ignored;
                }

                result.Add(lnk);
            }

            return result;
        }

        public static async Task<(LinkCheckSummary result, string newLink)> CheckLink(string link)
        {
            if (string.IsNullOrWhiteSpace(link))
            {
                return (LinkCheckSummary.Ignored, link);
            }

            if (!link.StartsWith("http://", StringComparison.OrdinalIgnoreCase) && !link.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
            {
                return (LinkCheckSummary.Ignored, link);
            }

            Uri uri;
            try
            {
                uri = new Uri(link);
            }
            catch (FormatException)
            {
                Logger.Log(LogLevel.Error, nameof(LinkChecker), $"Invalid URL {link}");
                return (LinkCheckSummary.Error, null);
            }

            try
            {
                var response = await client.GetAsync(uri, HttpCompletionOption.ResponseHeadersRead).ConfigureAwait(false);
                if (response.IsSuccessStatusCode)
                {
                    if (string.Equals(response.Headers.Location?.AbsoluteUri ?? link, link, StringComparison.OrdinalIgnoreCase))
                    {
                        return (LinkCheckSummary.Ok, link);
                    }
                    else
                    {
                        return (LinkCheckSummary.Redirected, response.Headers.Location?.AbsoluteUri);
                    }
                }
                else if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    return (LinkCheckSummary.NotFound, null);
                }
                else
                {
                    Logger.Log(LogLevel.Warning, nameof(LinkChecker), $"Status {response.StatusCode} checking {link}");
                    return (LinkCheckSummary.Error, null);
                }
            }
            catch (HttpRequestException req)
            {
                Logger.Log(LogLevel.Error, nameof(LinkChecker), $"Http exception checking {link}", req);
                return (LinkCheckSummary.Timeout, null);
            }
            catch (TaskCanceledException tce)
            {
                Logger.Log(LogLevel.Error, nameof(LinkChecker), $"Task cancelled (timeout) checking {link}", tce);
                return (LinkCheckSummary.Timeout, null);
            }
        }
    }
}
