using HtmlAgilityPack;
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
                    lnk.Result = LinkCheckResult.Ignored;
                }

                result.Add(lnk);
            }

            return result;
        }

        public static async Task<(LinkCheckResult result, string newLink)> CheckLink(string link)
        {
            if (string.IsNullOrWhiteSpace(link))
            {
                return (LinkCheckResult.Ignored, link);
            }

            if (!link.StartsWith("http://", StringComparison.OrdinalIgnoreCase) && !link.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
            {
                return (LinkCheckResult.Ignored, link);
            }

            Uri uri;
            try
            {
                uri = new Uri(link);
            }
            catch (FormatException)
            {
                return (LinkCheckResult.Error, null);
            }

            try
            {
                var response = await client.GetAsync(uri, HttpCompletionOption.ResponseHeadersRead).ConfigureAwait(false);
                if (response.IsSuccessStatusCode)
                {
                    if (string.Equals(response.Headers.Location?.AbsoluteUri ?? link, link, StringComparison.OrdinalIgnoreCase))
                    {
                        return (LinkCheckResult.Ok, link);
                    }
                    else
                    {
                        return (LinkCheckResult.Redirected, response.Headers.Location?.AbsoluteUri);
                    }
                }
                else if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    return (LinkCheckResult.NotFound, null);
                }
                else
                {
                    return (LinkCheckResult.Error, null);
                }
            }
            catch (HttpRequestException req)
            {

                return (LinkCheckResult.Timeout, null);
            }
            catch (TaskCanceledException)
            {
                return (LinkCheckResult.Timeout, null);
            }
        }
    }
}
