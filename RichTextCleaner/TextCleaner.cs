using HtmlAgilityPack;
using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace RichTextCleaner
{
    public static class TextCleaner
    {
        public static string ClearStylingFromHtml(string html)
        {
            var doc = new HtmlDocument();
            doc.LoadHtml(html);
            RemoveNonCMSElements(doc.DocumentNode);
            ClearStyling(doc.DocumentNode);

            using (var sw = new StringWriter())
            {
                doc.Save(sw);
                html = sw.ToString();
            }

            return html;

            void RemoveNonCMSElements(HtmlNode document)
            {
                RemoveNodes(document, ".//iframe");
                RemoveNodes(document, ".//noscript");
                RemoveNodes(document, ".//script");
            }

            void RemoveNodes(HtmlNode document, string xpath)
            {
                var toremove = document.SelectNodes(xpath);
                if (toremove != null)
                {
                    foreach (var node in toremove)
                    {
                        node.Remove();
                    }
                }
            }

            void ClearStyling(HtmlNode node)
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
            }

            void ProcessChildren(HtmlNode node)
            {
                if (node.ChildNodes != null && node.ChildNodes.Any())
                {
                    foreach (var child in node.ChildNodes)
                    {
                        ClearStyling(child);
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

            void ProcessChildren(StringBuilder sb, HtmlNode node)
            {
                foreach (var child in node.ChildNodes)
                {
                    ConvertToPlainText(sb, child);
                }
            }
        }

    }
}
