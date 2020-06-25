using HtmlAgilityPack;
using RichTextCleaner.Common;
using System;

namespace RichTextCleaner.Test
{
    internal static class DocTester
    {
        /// <summary>
        /// Processes the source using the supplied processor and returns the result.
        /// </summary>
        /// <param name="source">The (html) source.</param>
        /// <param name="processor">The processor method.</param>
        /// <returns>The resulting HTML.</returns>
        public static string ProcessSource(string source, Action<HtmlDocument> processor)
        {
            var doc = TextCleaner.CreateHtmlDocument(source);
            processor(doc);
            var html = TextCleaner.GetHtmlSource(doc, false);
            return html;
        }
    }
}
