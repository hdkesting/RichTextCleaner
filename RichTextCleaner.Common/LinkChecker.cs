using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RichTextCleaner.Common
{
    public static class LinkChecker
    {
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
                lnk.LinkText = link.InnerText;
                lnk.OriginalLink = link.GetAttributeValue("href", string.Empty);
                if (!lnk.OriginalLink.StartsWith("http", StringComparison.OrdinalIgnoreCase))
                {
                    lnk.Result = LinkCheckResult.Ignored;
                }

                result.Add(lnk);
            }

            return result;
        }
    }
}
