using HtmlAgilityPack;
using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace RichTextCleaner.Common
{
    public static class TextCleaner
    {
        /// <summary>
        /// Clears the styling from the HTML.
        /// </summary>
        /// <param name="html">The HTML to clean.</param>
        /// <param name="clearStyleMarkup">if set to <c>true</c> remove all bold and italic tags.</param>
        /// <returns></returns>
        public static string ClearStylingFromHtml(string html, bool clearStyleMarkup)
        {
            var doc = new HtmlDocument();
            doc.LoadHtml(html.Replace("&nbsp;", " "));
            RemoveNonCMSElements(doc);
            ClearStyling(doc.DocumentNode);
            TranslateNodes(doc, clearStyleMarkup);
            RemoveEmptySpans(doc);
            RemoveSurroundingTags(doc, "span");

            using (var sw = new StringWriter())
            {
                doc.Save(sw);
                html = sw.ToString();
            }

            html = html.Replace("</p>", "</p>" + System.Environment.NewLine);

            return html;
        }

        private static void RemoveSurroundingTags(HtmlDocument document, string tag)
        {
            var spans = document.DocumentNode.SelectNodes("//" + tag);
            if (spans != null)
            {
                foreach (var span in spans.ToList())
                {
                    // insert the original contents to replace the span
                    var content = HtmlNode.CreateNode(span.InnerHtml);
                    span.ParentNode.ReplaceChild(content, span);
                }
            }

        }

        private static void RemoveEmptySpans(HtmlDocument document)
        {
            // empty span nodes - remove
            var spans = document.DocumentNode.SelectNodes("//span[normalize-space(.) = '']");
            if (spans != null)
            {
                foreach (var span in spans)
                {
                    // insert a text-space to replace the span
                    var space = HtmlNode.CreateNode(" ");
                    span.ParentNode.ReplaceChild(space, span);
                }
            }

            // and empty paragraphs
            spans = document.DocumentNode.SelectNodes("//p[normalize-space(.) = '']");
            if (spans != null)
            {
                foreach (var span in spans)
                {
                    // insert a text-space to replace the span
                    var space = HtmlNode.CreateNode(" ");
                    span.ParentNode.ReplaceChild(space, span);
                }
            }

            // empty texts - replace with single newline
            var empties = document.DocumentNode.SelectNodes("//text()[normalize-space(.) = '']");
            if (empties != null)
            {
                foreach (var empty in empties)
                {
                    empty.InnerHtml = Environment.NewLine;
                }
            }
        }

        private static void ClearStyling(HtmlNode node)
        {
            if (node != null)
            {
                switch (node.NodeType)
                {
                    case HtmlNodeType.Document:
                        ProcessChildren(node);
                        break;

                    case HtmlNodeType.Element:
                        if (node.Attributes.Contains("style"))
                        {
                            node.Attributes.Remove("style");
                        }

                        if (node.Attributes.Contains("class"))
                        {
                            node.Attributes.Remove("class");
                        }
                        ProcessChildren(node);
                        break;
                }
            }

            void ProcessChildren(HtmlNode parent)
            {
                if (parent.ChildNodes != null && parent.ChildNodes.Any())
                {
                    foreach (var child in parent.ChildNodes)
                    {
                        ClearStyling(child);
                    }
                }
            }
        }

        private static void RemoveNonCMSElements(HtmlDocument document)
        {
            RemoveNodes(document.DocumentNode, ".//iframe");
            RemoveNodes(document.DocumentNode, ".//noscript");
            RemoveNodes(document.DocumentNode, ".//script");

            void RemoveNodes(HtmlNode node, string xpath)
            {
                var toremove = node.SelectNodes(xpath);
                if (toremove != null)
                {
                    foreach (var child in toremove)
                    {
                        child.Remove();
                    }
                }
            }
        }

        private static void TranslateNodes(HtmlDocument document, bool clearStyleMarkup)
        {
            // normalize "b" and "i" to "strong" and "em"
            Replace(document.DocumentNode, "b", "strong");
            Replace(document.DocumentNode, "i", "em");

            // remove any "font" tag
            Replace(document.DocumentNode, "font", "span");

            if (clearStyleMarkup)
            {
                // remove all "stong" and "em" tags (which includes the recently renamed "b" and "i")
                RemoveSurroundingTags(document, "strong");
                RemoveSurroundingTags(document, "em");
                RemoveSurroundingTags(document, "u");
            }

            void Replace(HtmlNode node, string oldtag, string newtag)
            {
                if (node.Name.Equals(oldtag, StringComparison.OrdinalIgnoreCase))
                {
                    node.Name = newtag;
                }

                ProcessChildren(node, oldtag, newtag);
            }

            void ProcessChildren(HtmlNode parent, string oldtag, string newtag)
            {
                if (parent.ChildNodes != null && parent.ChildNodes.Any())
                {
                    foreach (var child in parent.ChildNodes)
                    {
                        Replace(child, oldtag, newtag);
                    }
                }
            }

        }

        public static string HtmlToPlainText(string html)
        {
            if (html is null)
            {
                throw new ArgumentNullException(nameof(html));
            }

            StringBuilder sb = new StringBuilder(html.Length);

            var doc = new HtmlDocument();
            doc.LoadHtml(html);
            ConvertToPlainText(sb, doc.DocumentNode);

            string text = CleanupText(sb.ToString());
            return text;
        }

        private static string CleanupText(string text)
        {
            text = Regex.Replace(text, @"(\s*\r\n){3,}", Environment.NewLine + Environment.NewLine, RegexOptions.Multiline);

            return text;
        }

        private static void ConvertToPlainText(StringBuilder sb, HtmlNode node)
        {
            if (node != null)
            {
                if (node.NodeType == HtmlNodeType.Text)
                {
                    if (!string.IsNullOrWhiteSpace(node.InnerText))
                    {
                        sb.Append(System.Net.WebUtility.HtmlDecode(node.InnerText));
                    }
                }
                else if (node.NodeType == HtmlNodeType.Element)
                {
                    switch (node.Name.ToUpperInvariant())
                    {
                        case "BR":
                            // no content, but do break
                            sb.AppendLine();
                            break;

                        case "H1":
                        case "H2":
                        case "H3":
                        case "H4":
                        case "H5":
                        case "H6":
                        case "P":
                            sb.AppendLine(System.Net.WebUtility.HtmlDecode(node.InnerText));
                            break;

                        case "DIV":
                            sb.AppendLine();
                            ProcessChildren(sb, node);
                            sb.AppendLine();
                            break;

                        case "TABLE":
                            sb.AppendLine();
                            sb.AppendLine(new string('-', 10));
                            ProcessChildren(sb, node);
                            sb.AppendLine(new string('-', 10));
                            sb.AppendLine();
                            break;

                        case "TR":
                            ProcessChildren(sb, node);
                            sb.AppendLine(" |");
                            break;

                        case "TD":
                        case "TH":
                            sb.Append(" | ").Append(System.Net.WebUtility.HtmlDecode(node.InnerText).Trim());
                            break;

                        default:
                            ProcessChildren(sb, node); // which may include Text nodes!
                            break;
                    }
                }
                else if (node.NodeType == HtmlNodeType.Document)
                {
                    ProcessChildren(sb, node);
                }
            }

            void ProcessChildren(StringBuilder sb1, HtmlNode node1)
            {
                foreach (var child in node1.ChildNodes)
                {
                    ConvertToPlainText(sb1, child);
                }
            }
        }

    }
}
