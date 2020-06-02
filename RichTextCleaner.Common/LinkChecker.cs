using HtmlAgilityPack;
using RichTextCleaner.Common.Support;
using RichTextCleaner.Common.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;

namespace RichTextCleaner.Common
{
    /// <summary>
    /// A component that checks links for existance.
    /// </summary>
    public static class LinkChecker
    {
        private static readonly TimeSpan HttpTimeout = TimeSpan.FromSeconds(20);

        // https://docs.microsoft.com/en-us/dotnet/api/system.net.http.httpclient?view=netframework-4.8
        private static readonly HttpClientHandler clientHandler = new HttpClientHandler { AllowAutoRedirect = true };
        private static readonly HttpClient client = new HttpClient(clientHandler) { Timeout = HttpTimeout };

        static LinkChecker()
        {
            var version = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString(3);
            client.DefaultRequestHeaders.UserAgent.ParseAdd($"Mozilla/5.0 (Windows NT 10.0; Win64; x64) RichTextCleaner/{version}");
        }

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

        /// <summary>
        /// Checks the link for existance and redirects.
        /// </summary>
        /// <param name="link">The link to check.</param>
        /// <param name="cancelToken">The cancellation token.</param>
        /// <returns></returns>
        public static async Task<LinkCheckResult> CheckLink(string link, System.Threading.CancellationToken cancelToken)
        {
            if (string.IsNullOrWhiteSpace(link))
            {
                return new LinkCheckResult(LinkCheckSummary.Ignored);
            }

            if (!link.StartsWith("http://", StringComparison.OrdinalIgnoreCase) && !link.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
            {
                return new LinkCheckResult(LinkCheckSummary.Ignored);
            }

            Uri uri;
            try
            {
                uri = new Uri(link);
            }
            catch (FormatException)
            {
                Logger.Log(LogLevel.Error, nameof(LinkChecker), $"Invalid URL {link}");
                return new LinkCheckResult(LinkCheckSummary.Error);
            }

            if (uri.Host.EndsWith("linkedin.com", StringComparison.OrdinalIgnoreCase) 
                || uri.Host.EndsWith("facebook.com", StringComparison.OrdinalIgnoreCase))
            {
                // these accept only known browsers - just ignore the link (except for a http->https change)
                if (uri.Scheme == "http")
                {
                    return new LinkCheckResult(LinkCheckSummary.SimpleChange, link.Replace("http:", "https:"));
                }

                return new LinkCheckResult(LinkCheckSummary.Ignored);
            }

            var defaultResult = LinkCheckSummary.Ok;

            var link2 = CleanHref(link, LinkQueryCleanLevel.RemoveQuery);
            if (link2 != link)
            {
                uri = new Uri(link2);
                defaultResult = LinkCheckSummary.SimpleChange;
            }

            try
            {
                var response = await client.GetAsync(uri, HttpCompletionOption.ResponseContentRead, cancelToken).ConfigureAwait(false);
                if (response.IsSuccessStatusCode)
                {
                    // ignore any added (or removed) trailing '/'
                    if (string.Equals((response.RequestMessage.RequestUri?.AbsoluteUri ?? link).TrimEnd('/'), link.TrimEnd('/'), StringComparison.OrdinalIgnoreCase))
                    {
                        return new LinkCheckResult(defaultResult, link2, response.StatusCode);
                    }
                    else
                    {
                        // the uris are different, but is it an important difference?
                        if (IsSimpleRedirect(uri, response.RequestMessage.RequestUri))
                        {
                            return new LinkCheckResult(LinkCheckSummary.SimpleChange, response.RequestMessage.RequestUri?.AbsoluteUri, response.StatusCode);
                        }
                        else
                        {
                            return new LinkCheckResult(LinkCheckSummary.Redirected, response.RequestMessage.RequestUri?.AbsoluteUri, response.StatusCode);
                        }
                    }
                }
                else if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    return new LinkCheckResult(LinkCheckSummary.NotFound, null, response.StatusCode);
                }
                else
                {
                    Logger.Log(LogLevel.Warning, nameof(LinkChecker), $"Status {response.StatusCode} checking {link}");
                    return new LinkCheckResult(LinkCheckSummary.Error, null, response.StatusCode);
                }
            }
            catch (HttpRequestException req)
            {
                Logger.Log(LogLevel.Error, nameof(LinkChecker), $"Http exception checking {link}", req);
                return new LinkCheckResult(LinkCheckSummary.Timeout);
            }
            catch (TaskCanceledException tce)
            {
                Logger.Log(LogLevel.Error, nameof(LinkChecker), $"Task cancelled (timeout) checking {link}", tce);
                return new LinkCheckResult(LinkCheckSummary.Timeout);
            }
        }

        /// <summary>
        /// Marks the specified invalid link with square brackets.
        /// </summary>
        /// <param name="htmlSource">The HTML source to update.</param>
        /// <param name="linkHref">The link href to alter.</param>
        /// <returns></returns>
        public static string MarkInvalid(string htmlSource, string linkHref)
        {
            var doc = TextCleaner.CreateHtmlDocument(htmlSource);

            foreach (var node in doc.DocumentNode.SelectNodes("//a[@href='"+linkHref + "']") ?? Enumerable.Empty<HtmlNode>())
            {
                // don't add more than one pair of brackets, but ignore any just outside of this element
                node.InnerHtml = "[" + node.InnerHtml.TrimStart('[').TrimEnd(']') + "]";
            }

            return TextCleaner.GetHtmlSource(doc);
        }

        /// <summary>
        /// Updates the href for simple redirects.
        /// </summary>
        /// <param name="htmlSource">The HTML source.</param>
        /// <param name="linkHref">The link href.</param>
        /// <param name="newHref">The new href.</param>
        /// <returns></returns>
        public static string UpdateHref(string htmlSource, string linkHref, string newHref) 
        {
            var doc = TextCleaner.CreateHtmlDocument(htmlSource);

            foreach (var node in doc.DocumentNode.SelectNodes("//a[@href='"+linkHref + "']") ?? Enumerable.Empty<HtmlNode>())
            {
                node.Attributes["href"].Value = newHref;
            }

            return TextCleaner.GetHtmlSource(doc);
        }

        /// <summary>
        /// Cleans the href value.
        /// </summary>
        /// <param name="original">The original URL.</param>
        /// <param name="cleanLevel">The clean level.</param>
        /// <returns></returns>
        public static string CleanHref(string original, LinkQueryCleanLevel cleanLevel)
        {
            if (string.IsNullOrWhiteSpace(original) || !original.StartsWith("http", StringComparison.Ordinal))
            {
                return original;
            }

            if (!Uri.TryCreate(original, UriKind.Absolute, out Uri uri))
            {
                return original;
            }

            // for example https://nam04.safelinks.protection.outlook.com/?url=http%3A%2F%2Fhealthclarity.wolterskluwer.com ...
            if (uri.Host.EndsWith("safelinks.protection.outlook.com", StringComparison.OrdinalIgnoreCase))
            {
                // I want the "url" parameter (decoded) and can ignore the rest
                var query = original.Substring(original.IndexOf('?') + 1).Replace("&amp;", "&");
                var urlparam = query.Split('&').First(p => p.StartsWith("url=", StringComparison.Ordinal));
                original = System.Net.WebUtility.UrlDecode(urlparam.Substring(4)); // skip the "url="
            }
            else if (uri.Host.Equals("urldefense.com", StringComparison.OrdinalIgnoreCase))
            {
                // assume /v3/__http(s)://fullpath__;security
                var query = uri.PathAndQuery;
                query = query.Substring(query.IndexOf("__", StringComparison.Ordinal) + 2);
                var p = query.IndexOf(";", StringComparison.Ordinal);
                if (p > 0)
                {
                    query = query.Substring(0, p).Trim('_');
                }

                // undo some conversions that I saw in two examples: *20 -> %20, /*/ -> /#/
                query = Regex.Replace(query, "\\*(?=[0-9a-zA-Z]{2})", "%").Replace("*", "#");
                original = query;
            }

            return CleanQueryString(original, cleanLevel);
        }

        /// <summary>
        /// Cleans the query part of the supplied URL.
        /// </summary>
        /// <param name="original">The original URL.</param>
        /// <param name="cleanLevel">The requested clean level.</param>
        /// <returns></returns>
        private static string CleanQueryString(string original, LinkQueryCleanLevel cleanLevel)
        {
            if (cleanLevel == LinkQueryCleanLevel.None || string.IsNullOrWhiteSpace(original) || !original.Contains('?'))
            {
                return original;
            }

            if (cleanLevel == LinkQueryCleanLevel.RemoveQuery)
            {
                return original.Substring(0, original.IndexOf('?'));
            }


            var qry = original.Substring(original.IndexOf('?') + 1);
            bool encoded = qry.Contains("&amp;");
            if (encoded)
            {
                // decode (ignoring any other encodings)
                qry = qry.Replace("&amp;", "&");
            }

            // ignore all "utm_whatever=value" parts
            qry = string.Join("&",
                qry.Split('&').Where(x => !x.StartsWith("utm", StringComparison.OrdinalIgnoreCase)));
            if (string.IsNullOrEmpty(qry))
            {
                return original.Substring(0, original.IndexOf('?'));
            }

            if (encoded)
            {
                // re-encode
                qry = qry.Replace("&", "&amp;");
            }

            return original.Substring(0, original.IndexOf('?')) + "?" + qry;
        }

        private static bool IsSimpleRedirect(Uri oldUri, Uri newUri)
        {
            // assumption: the uris are different (that was already checked)

            if (!string.Equals(oldUri.PathAndQuery.TrimEnd('/'), newUri.PathAndQuery.TrimEnd('/'), StringComparison.Ordinal))
            {
                // it doesn't seem to be the same page (ignoring trailing spaces), judging by the url
                return false;
            }

            if (oldUri.Scheme != newUri.Scheme && oldUri.Scheme != "http" && newUri.Scheme != "https")
            {
                // not a simple http-to-https redirect
                return false;
            }

            if (string.Equals(oldUri.Host, newUri.Host, StringComparison.OrdinalIgnoreCase))
            {
                // same hostname (apart from casing, maybe)
                return true;
            }

            if (string.Equals(oldUri.Host.Replace("www.", string.Empty), newUri.Host.Replace("www.",string.Empty), StringComparison.OrdinalIgnoreCase))
            {
                // same hostname (apart from a www prefix)
                return true;
            }

            return false;
        }
    }
}
