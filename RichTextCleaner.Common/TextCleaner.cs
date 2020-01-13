﻿using HtmlAgilityPack;
using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

[assembly: System.Runtime.CompilerServices.InternalsVisibleTo("RichTextCleaner.Test")]

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
            if (html is null)
            {
                throw new ArgumentNullException(nameof(html));
            }

            var doc = CreateHtmlDocument(html);

            RemoveNonCMSElements(doc);
            ClearStyling(doc.DocumentNode);
            TranslateNodes(doc, clearStyleMarkup);
            RemoveSurroundingTags(doc, "span");
            RemoveEmptySpans(doc);
            ClearParagraphsInBlocks(doc);
            RemoveAnchors(doc);
            TrimParagraphs(doc);

            return GetHtmlSource(doc);
        }

        internal static HtmlDocument CreateHtmlDocument(string html)
        {
            var doc = new HtmlDocument();
            doc.LoadHtml((html ?? String.Empty).Replace("&nbsp;", " "));

            return doc;
        }

        internal static string GetHtmlSource(HtmlDocument document, bool cleanup = true)
        {
            string html;
            using (var sw = new StringWriter())
            {
                document.Save(sw);
                html = sw.ToString();
            }

            if (cleanup)
            {
                html = html
                    .Replace("</p>", "</p>" + Environment.NewLine);
            }

            return html;
        }

        /// <summary>
        /// Remove any whitespace from start and end of paragraphs.
        /// </summary>
        /// <param name="document"></param>
        internal static void TrimParagraphs(HtmlDocument document)
        {
            var paras = document.DocumentNode.SelectNodes("//p") ?? Enumerable.Empty<HtmlNode>();
            foreach (var para in paras.Where(p => p.ChildNodes?.Count > 0))
            {
                var child = para.ChildNodes.First();
                if (child.NodeType == HtmlNodeType.Text)
                {
                    child.InnerHtml = child.InnerHtml.TrimStart();
                }

                child = para.ChildNodes.Last(); // may be the same as first
                if (child.NodeType == HtmlNodeType.Text)
                {
                    child.InnerHtml = child.InnerHtml.TrimEnd();
                }
            }
        }

        /// <summary>
        /// Remove any &lt;a name=..&gt; elements.
        /// </summary>
        /// <param name="document"></param>
        internal static void RemoveAnchors(HtmlDocument document)
        {
            var anchors = document.DocumentNode.SelectNodes("//a[@name]") ?? Enumerable.Empty<HtmlNode>();
            foreach (var anchor in anchors)
            {
                if (anchor.GetAttributeValue("href", null) != null)
                {
                    // both "name" and "href" attributes, so just remove the "name"
                    anchor.Attributes.Remove("name");
                }
                else
                {
                    // just "<a name>", remove from around contents
                    RemoveSurroundingTags(anchor);
                }
            }

            anchors = document.DocumentNode.SelectNodes("//a[@tabindex]") ?? Enumerable.Empty<HtmlNode>();
            foreach (var anchor in anchors)
            {
                anchor.Attributes.Remove("tabindex");
            }
        }

        /// <summary>
        /// If a TD or LI contains a single P element, then remove that P (keeping other markup)
        /// </summary>
        /// <param name="document"></param>
        private static void ClearParagraphsInBlocks(HtmlDocument doc)
        {
            ClearParagraphs(doc, "td");
            ClearParagraphs(doc, "li");

            void ClearParagraphs(HtmlDocument document, string tag)
            {
                var cells = document.DocumentNode.SelectNodes("//" + tag) ?? Enumerable.Empty<HtmlNode>();
                foreach (var cell in cells)
                {
                    // remove empty text nodes from TD (presumably around the P node)
                    foreach (var child in cell.ChildNodes.ToList())
                    {
                        if (child.NodeType == HtmlNodeType.Text && String.IsNullOrWhiteSpace(child.InnerText))
                        {
                            child.Remove();
                        }
                    }

                    // now a single P is really the only node
                    if (cell.ChildNodes.Count == 1 && cell.ChildNodes[0].Name.Equals("p", StringComparison.OrdinalIgnoreCase))
                    {
                        var para = cell.ChildNodes[0];
                        RemoveSurroundingTags(para);
                    }
                }
            }
        }

        /// <summary>
        /// Remove specific tags while keeping their contents.
        /// </summary>
        /// <param name="document"></param>
        /// <param name="tag"></param>
        private static void RemoveSurroundingTags(HtmlDocument document, string tag)
        {
            var spans = document.DocumentNode.SelectNodes("//" + tag) ?? Enumerable.Empty<HtmlNode>();
            foreach (var span in spans)
            {
                RemoveSurroundingTags(span);
            }
        }

        /// <summary>
        /// Remove the element while keeping all contents.
        /// </summary>
        /// <param name="node"></param>
        private static void RemoveSurroundingTags(HtmlNode node)
        {
            // insert the original contents to replace the node
            var content = node.ChildNodes;
            if (content == null || content.Count == 0)
            {
                // completely empty - remove
                node.Remove();
            }
            else if (ContentIsEmptyText(content))
            {
                // space-only node - can be replaced by a space
                if (node.PreviousSibling?.NodeType == HtmlNodeType.Text)
                {
                    // add space to previous text node
                    node.PreviousSibling.InnerHtml += " ";
                    node.Remove();
                }
                else if (node.NextSibling?.NodeType == HtmlNodeType.Text)
                {
                    // add space to next text node
                    node.NextSibling.InnerHtml = " " + node.NextSibling.InnerHtml;
                    node.Remove();
                }
                else
                {
                    // no surrounding text nodes, just replace with a space.
                    var space = HtmlNode.CreateNode(" ");
                    node.ParentNode.ReplaceChild(space, node);
                }
            }
            else
            {
                // non-empty node - replace with all contents
                var parent = node.ParentNode;
                var latest = content.First();
                parent.ReplaceChild(latest, node);

                if (content.Count > 1)
                {
                    foreach (var child in content.Skip(1))
                    {
                        parent.InsertAfter(child, latest);
                        latest = child;
                    }
                }
            }

            bool ContentIsEmptyText(HtmlNodeCollection nodes)
            {
                if (nodes == null || nodes.Count == 0)
                {
                    // empty node, can be removed completely
                    return true;
                }

                if (nodes.Count > 1)
                {
                    // more than one node (assume not all are #text), keep all
                    return false;
                }

                // an &nbsp; was already replaced by a space
                return nodes[0].NodeType == HtmlNodeType.Text && String.IsNullOrWhiteSpace(nodes[0].InnerText);
            }
        }

        /// <summary>
        /// Replace empty SPAN and P elements by a space 
        /// </summary>
        /// <param name="document"></param>
        private static void RemoveEmptySpans(HtmlDocument document)
        {
            // empty span nodes - remove
            var spans = document.DocumentNode.SelectNodes("//span[normalize-space(.) = '']") ?? Enumerable.Empty<HtmlNode>();
            foreach (var span in spans)
            {
                // insert a text-space to replace the span
                var space = HtmlNode.CreateNode(" ");
                span.ParentNode.ReplaceChild(space, span);
            }

            // and empty paragraphs
            spans = document.DocumentNode.SelectNodes("//p[normalize-space(.) = '']") ?? Enumerable.Empty<HtmlNode>();
            foreach (var span in spans)
            {
                // insert a text-space to replace the span
                var space = HtmlNode.CreateNode(" ");
                span.ParentNode.ReplaceChild(space, span);
            }
        }

        /// <summary>
        /// Remove all style and class attributes.
        /// </summary>
        /// <param name="node"></param>
        internal static void ClearStyling(HtmlNode node)
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

                        foreach (var name in node.Attributes
                            .Where(a => a.Name.StartsWith("on", StringComparison.OrdinalIgnoreCase))
                            .Select(a => a.Name)
                            .ToList())
                        {
                            node.Attributes.Remove(name);
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

        /// <summary>
        /// Remove elements that don't belong in a CMS text.
        /// </summary>
        /// <param name="document"></param>
        internal static void RemoveNonCMSElements(HtmlDocument document)
        {
            // RemoveNodes(document.DocumentNode, ".//iframe"); // keep iframe - may contain video
            RemoveNodes(document.DocumentNode, ".//noscript");
            RemoveNodes(document.DocumentNode, ".//script");

            void RemoveNodes(HtmlNode node, string xpath)
            {
                var toremove = node.SelectNodes(xpath) ?? Enumerable.Empty<HtmlNode>();
                foreach (var child in toremove)
                {
                    child.Remove();
                }
            }
        }

        /// <summary>
        /// Replace styling nodes with modern versions, optionally remove them completely.
        /// </summary>
        /// <param name="document"></param>
        /// <param name="clearStyleMarkup">When true, remove the style markup completely.</param>
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

            var sb = new StringBuilder(html.Length);

            var doc = CreateHtmlDocument(html);

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
