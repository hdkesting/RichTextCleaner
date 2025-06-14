﻿[assembly: System.Runtime.CompilerServices.InternalsVisibleTo("RichTextCleaner.Test")]

namespace RichTextCleaner.Common;

using HtmlAgilityPack;
using RichTextCleaner.Common.Support;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

/// <summary>
/// The cleaner module.
/// </summary>
public static class TextCleaner
{
    /// <summary>
    /// Standard replacements to do in the source text.
    /// </summary>
    private static readonly Dictionary<string, string> TextReplacements = new()
    {
        { "&nbsp;", " " }, // replace non-breaking space with simple space
        { "™", "&trade;" }, // replace some literal characters with html entities
        { "®", "&reg;" },
        { "©", "&copy;"  },
        { "“", "&ldquo;" },
        { "”", "&rdquo;" },
        { "‘", "&lsquo;" },
        { "’", "&rsquo;" },
        { "\x2013", "&ndash;" },
        { "\x2014", "&mdash;" },
        { "<sup>TM</sup>", "&trade;" },
        { "<sup>&reg;</sup>", "&reg;" }, // includes ®, because that was just replaced
    };

    /// <summary>
    /// The tags to remove while keeping their contents.
    /// </summary>
    private static readonly List<string> TagsToRemove =
    [
        "span",
        "div",
        "header",
        "footer",
        "body",
        "content", // obsolete tag https://developer.mozilla.org/en-US/docs/Web/HTML/Element/content
        "mark",
        "ins",
    ];

    /// <summary>
    /// The non CMS elements to remove including contents.
    /// </summary>
    private static readonly List<string> NonCmsElementsToRemove =
    [
        "noscript",
        "script",
        "style",
        "time",
        "nav",
        "section",
        "article",
        "audio",
        "canvas",
        "del",
    ];

    /// <summary>
    /// Whitelist of permitted attributes on specific elements. If element is not mentioned (as key), then no attributes are allowed.
    /// </summary>
    /// <remarks>
    /// Do not add "rel" to "a", as Sitecore has its own ideas about that.
    /// </remarks>
    private static readonly Dictionary<string, List<string>> AttributeWhitelist = new(StringComparer.OrdinalIgnoreCase)
    {
        { "a", new List<string> { "href", "target", "alt", "title"} },
        { "img", new List<string> { "src", "srcset", "width", "height", "alt", "title" } },
        { "iframe", new List<string> { "src", "width", "height", "title" } },
    };

    /// <summary>
    /// Clears the styling from the HTML.
    /// </summary>
    /// <param name="html">The HTML to clean.</param>
    /// <param name="settings">All the settings.</param>
    /// <returns></returns>
    /// <exception cref="ArgumentNullException">settings must not be null.</exception>
    public static string ClearStylingFromHtml(
        string html,
        ICleanerSettings settings)
    {
        ArgumentNullException.ThrowIfNull(settings);

        var doc = CreateHtmlDocument(html ?? string.Empty);

        RemoveNonCMSElements(doc);
        RemoveOfficeMarkup(doc);
        CreateHeaders(doc);
        ClearStyling(doc);
        TranslateStyleNodes(doc, settings.MarkupToRemove);
        RemoveEmptyElements(doc);

        RemoveUnneededTags(doc);

        ClearParagraphsInBlocks(doc);
        RemoveAnchors(doc);
        if (settings.CreateLinkFromText)
        {
            CreateMissingLinks(doc);
        }

        CombineAndCleanLinks(doc, settings.QueryCleanLevel);
        if (settings.AddTargetBlank)
        {
            AddBlankLinkTargets(doc, settings.AddRelNoOpener);
        }

        TrimParagraphs(doc);

        UpdateQuotes(doc, settings.QuoteProcess);

        return GetHtmlSource(doc);
    }

    internal static HtmlDocument CreateHtmlDocument(string html)
    {
        html = html ?? string.Empty;
        foreach (var kv in TextReplacements)
        {
            html = html.Replace(kv.Key, kv.Value);
        }
        
        int oldLength, newLength;
        do
        {
            oldLength = html.Length;
            html = html.Replace(Environment.NewLine + Environment.NewLine, Environment.NewLine);
            newLength = html.Length;
        }
        while (oldLength != newLength);

        var doc = new HtmlDocument();
        doc.LoadHtml(html);

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
            // single line break after paragraph and table row
            html = html
                .Replace("</p>", "</p>" + Environment.NewLine)
                .Replace("</tr>", "</tr>" + Environment.NewLine);

            // remove double line breaks
            html = Regex.Replace(html, @"\s*[\r\n]+", Environment.NewLine);

            // add line break before and after any header
            html = Regex.Replace(html, "(<h[1-6]>)", Environment.NewLine + "$1");
            html = Regex.Replace(html, "(</h[1-6]>)", "$1" + Environment.NewLine);
        }

        return html;
    }

    internal static void UpdateQuotes(HtmlDocument document, QuoteProcessing quoteProcessing)
    {
        switch (quoteProcessing)
        {
            case QuoteProcessing.ChangeToSimpleQuotes:
                UpdateQuotesToSimple(document);
                break;

            case QuoteProcessing.ChangeToSmartQuotes:
                UpdateQuotesToSmart(document);
                break;
        }
    }

    /// <summary>
    /// Remove all nodes with a namespace, like &lt;o:p&gt; and &lt;u5:p&gt;.
    /// </summary>
    /// <param name="document"></param>
    internal static void RemoveOfficeMarkup(HtmlDocument document)
    {
        foreach (var elt in document.DocumentNode.DescendantsAndSelf()?.ToList() ?? Enumerable.Empty<HtmlNode>())
        {
            if (elt.NodeType == HtmlNodeType.Element && elt.Name.Contains(':'))
            {
                RemoveSurroundingTags(elt);
            }
        }
    }

    /// <summary>
    /// Find P elements containing a single STRONG, convert to H2.
    /// </summary>
    /// <remarks>
    /// This will not see cases where the "header" ends on (or starts with) a BR or other whitespace. Or when it consists of two consecutive STRONGs.
    /// It will ignore paragraphs of 200 or longer as that is probably not a title (but maybe an abstract/intro).
    /// </remarks>
    /// <param name="document"></param>
    internal static void CreateHeaders(HtmlDocument document)
    {
        var paras = document.DocumentNode.SelectNodes("//p") ?? Enumerable.Empty<HtmlNode>();
        foreach (var par in paras.Where(p => p.InnerText.Length < 200))
        {
            var brNodes = new List<HtmlNode>();
            var strongNodes = new List<HtmlNode>();
            bool canBeHeader = false;

            // check just a single level deep
            foreach (var child in par.ChildNodes)
            {
                if (child.NodeType == HtmlNodeType.Text)
                {
                    if (!String.IsNullOrWhiteSpace(child.InnerText))
                    {
                        // plain text in P - so not a header
                        canBeHeader = false;
                        break;
                    }
                }
                else if (child.NodeType == HtmlNodeType.Element)
                {
                    if (child.Name == "br")
                    {
                        brNodes.Add(child);
                    }
                    else if (child.Name == "strong" || child.Name == "b")
                    {
                        canBeHeader = true;
                        strongNodes.Add(child);
                    }
                    else
                    {
                        // other node, so not a header
                        canBeHeader = false;
                        break;
                    }
                }
            }

            if (canBeHeader)
            {
                // remove BRs
                foreach (var nd in brNodes)
                {
                    par.RemoveChild(nd);
                }

                // remove B/STRONG from around their content
                foreach (var nd in strongNodes)
                {
                    RemoveSurroundingTags(nd);
                }

                // make this P into a H2
                par.Name = "h2";

                TrimElement(par);
            }
        }
    }

    private static void UpdateQuotesToSimple(HtmlDocument document)
    {
        foreach (var textNode in document.DocumentNode.SelectNodes("//text()") ?? Enumerable.Empty<HtmlNode>())
        {
            textNode.InnerHtml = textNode.InnerHtml
                .Replace("&ldquo;", "\"")
                .Replace("&rdquo;", "\"")
                .Replace("&lsquo;", "'")
                .Replace("&rsquo;", "'");
        }
    }

    private static void UpdateQuotesToSmart(HtmlDocument document)
    {
        // note that quoted text may end on a comma or period.
        foreach (var textNode in document.DocumentNode.SelectNodes("//text()") ?? Enumerable.Empty<HtmlNode>())
        {
            textNode.InnerHtml = Regex.Replace(textNode.InnerHtml, @"(^|(?<=(\s|\()))""", "&ldquo;", RegexOptions.None);
            textNode.InnerHtml = Regex.Replace(textNode.InnerHtml, @"""((?=(\s|\)))|$)", "&rdquo;", RegexOptions.None);
            textNode.InnerHtml = Regex.Replace(textNode.InnerHtml, @"(^|(?<=(\s|\()))'", "&lsquo;", RegexOptions.None);
            textNode.InnerHtml = Regex.Replace(textNode.InnerHtml, @"'((?=(\s|\)))|$)", "&rsquo;", RegexOptions.None);
        }
    }

    private static void RemoveUnneededTags(HtmlDocument document)
    {
        foreach (var tag in TagsToRemove)
        {
            RemoveSurroundingTags(document, tag);
        }
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
            TrimElement(para);
        }

        // remove any trailing <br/>
        var brnode = (document.DocumentNode.SelectNodes("//br") ?? Enumerable.Empty<HtmlNode>()).LastOrDefault();
        if (brnode != null && brnode.NextSibling is null)
        {
            brnode.ParentNode.RemoveChild(brnode);
        }
    }

    private static void TrimElement(HtmlNode node)
    {
        if (node is null || (node.ChildNodes?.Count ?? 0) == 0)
        {
            return;
        }

        // remove leading whitespace
        bool trimmed;
        do
        {
            var child = node.ChildNodes.FirstOrDefault();
            if (child == null)
            {
                break;
            }

            trimmed = false;
            if (child.NodeType == HtmlNodeType.Text)
            {
                if (string.IsNullOrWhiteSpace(child.InnerHtml))
                {
                    child.Remove();
                    trimmed = true;
                }
                else
                {
                    // remove any leading whitespace
                    var htmlTrim = child.InnerHtml.TrimStart();
                    if (htmlTrim != child.InnerHtml)
                    {
                        child.InnerHtml = htmlTrim;
                        trimmed = true;
                    }
                }
            }
            else if (child.NodeType == HtmlNodeType.Element && child.Name == "br")
            {
                // remove an initial <br>
                child.Remove();
                trimmed = true;
            }
        }
        while (trimmed);

        // remove trailing whitespace
        do
        {
            var child = node.ChildNodes.LastOrDefault(); // may be the same as first
            if (child == null)
            {
                break;
            }

            trimmed = false;
            if (child.NodeType == HtmlNodeType.Text)
            {
                if (string.IsNullOrWhiteSpace(child.InnerHtml))
                {
                    child.Remove();
                    trimmed = true;
                }
                else
                {
                    // remove any trailing whitespace
                    var htmlTrim = child.InnerHtml.TrimEnd();
                    if (htmlTrim != child.InnerHtml)
                    {
                        child.InnerHtml = htmlTrim;
                        trimmed = true;
                    }
                }
            }
            else if (child.NodeType == HtmlNodeType.Element && child.Name == "br")
            {
                // remove an final <br>
                child.Remove();
                trimmed = true;
            }
        }
        while (trimmed);
    }

    /// <summary>
    /// Remove any &lt;a&gt; elements without attributes.
    /// </summary>
    /// <remarks>
    /// The "name" didn't make it through the whitelist, so left an &lt;a&gt; without attributes.
    /// </remarks>
    /// <param name="document"></param>
    internal static void RemoveAnchors(HtmlDocument document)
    {
        var anchors = document.DocumentNode.SelectNodes("//a[not(@href)]") ?? Enumerable.Empty<HtmlNode>();
        foreach (var anchor in anchors)
        {
            // used to be just "<a name>", remove from around contents
            RemoveSurroundingTags(anchor);
        }
    }

    /// <summary>
    /// Find link-like texts and make real links.
    /// </summary>
    /// <param name="document"></param>
    private static void CreateMissingLinks(HtmlDocument document)
    {
        // search for text nodes directly under a <p>
        var plainTexts = document.DocumentNode.SelectNodes("(//p|//li|//td)/text()") ?? Enumerable.Empty<HtmlNode>();

        foreach (var textnode in plainTexts)
        {
            // start with http(s):// or www or 2 "words", then at least one more "word". TLD must be letters-only
            textnode.InnerHtml = Regex.Replace(
                textnode.InnerHtml, 
                @"((https?://[a-z0-9]{2,})|www|[a-zA-Z0-9-]{2,})(\.[a-zA-Z0-9-]{2,})+(\.[a-zA-Z]{2,10})(/[a-zA-Z0-9.-]+)*", 
                new MatchEvaluator(WebLinkCreator));

            // start with a "+" and then numbers and spaces - that is a telephone number
            textnode.InnerHtml = Regex.Replace(
                textnode.InnerHtml,
                @"\+[1-9][ -]?([0-9][ -]?){6,}[0-9]",
                new MatchEvaluator(PhoneLinkCreator));
        }

        string WebLinkCreator(Match m) => m.Value.StartsWith("http", StringComparison.OrdinalIgnoreCase) 
            ? $"<a href=\"{m.Value}\">{m.Value}</a>"
            : $"<a href=\"http://{m.Value}\">{m.Value}</a>";

        string PhoneLinkCreator(Match m) => $"<a href=\"tel:{m.Value}\">{m.Value}</a>";
    }

    private static void CombineAndCleanLinks(HtmlDocument document, LinkQueryCleanLevel queryCleanLevel)
    {
        CleanLinks(document, queryCleanLevel);
        CombineLinks(document);
        RemoveEmptyLinks(document);
        RemoveLeadingAndTrailingSpacesFromLinks(document);
        CleanLinkContent(document);
    }

    private static void CleanLinks(HtmlDocument document, LinkQueryCleanLevel queryCleanLevel)
    {
        var links = document.DocumentNode.SelectNodes("//a[@href]") ?? Enumerable.Empty<HtmlNode>();
        foreach (var link in links)
        {
            var href = link.Attributes["href"].Value;
            href = LinkChecker.CleanHref(href, queryCleanLevel);
            link.Attributes["href"].Value = href;
        }
    }

    /// <summary>
    /// If a text in a link seems to be a URL, then remove "http(s)://" and any trailing "/".
    /// </summary>
    /// <param name="document">The document.</param>
    private static void CleanLinkContent(HtmlDocument document)
    {
        var links = document.DocumentNode.SelectNodes("//a") ?? Enumerable.Empty<HtmlNode>();
        foreach (var link in links)
        {
            if (link.ChildNodes.Count == 1 && link.ChildNodes[0].NodeType == HtmlNodeType.Text)
            {
                var text = link.InnerHtml;
                if (text.StartsWith("http", StringComparison.Ordinal))
                {
                    text = Regex.Replace(text, "^https?://", string.Empty).TrimEnd('/');
                    link.InnerHtml = text;
                }
            }
        }
    }

    internal static void CombineLinks(HtmlDocument document)
    {
        var links = document.DocumentNode.SelectNodes("//a") ?? Enumerable.Empty<HtmlNode>();

        foreach (var link in links.Skip(1).ToList())
        {
            var previousSibling = link.PreviousSibling;
            if (previousSibling?.NodeType == HtmlNodeType.Element
                && previousSibling.Name == "a"
                && previousSibling.GetAttributeValue("href", string.Empty) == link.GetAttributeValue("href", string.Empty))
            {
                // this link points to the same destination as its immediate predecessor. Join contents to that, clearing this (to be removed next)
                foreach (var child in (link.ChildNodes ?? Enumerable.Empty<HtmlNode>()).ToList())
                {
                    link.ChildNodes.Remove(child);
                    previousSibling.ChildNodes.Append(child); // this *copies* the node
                }

                link.Remove();
            }
        }
    }

    internal static void RemoveEmptyLinks(HtmlDocument document)
    {
        // remove empty links
        var links = document.DocumentNode.SelectNodes("//a[normalize-space(.) = '']") ?? Enumerable.Empty<HtmlNode>();

        // use .ToList() to make sure the original list cannot be modified
        foreach (var link in links.ToList())
        {
            RemoveSurroundingTags(link);
        }
    }

    internal static void RemoveLeadingAndTrailingSpacesFromLinks(HtmlDocument document)
    {
        // move leading and trailing spaces and commas/periods outside of link
        var links = document.DocumentNode.SelectNodes("//a") ?? Enumerable.Empty<HtmlNode>();
        foreach (var link in links.Where(l => l.ChildNodes?.Count > 0).ToList())
        {
            var firstchild = link.ChildNodes[0];
            if (firstchild.NodeType == HtmlNodeType.Text)
            {
                var match = Regex.Match(firstchild.InnerHtml, "^[ .,]+", RegexOptions.None);
                if (match.Success)
                {
                    var txt = match.Value;
                    var prev = link.PreviousSibling;
                    firstchild.InnerHtml = firstchild.InnerHtml.Substring(txt.Length);
                    if (prev?.NodeType == HtmlNodeType.Text)
                    {
#pragma warning disable S1643 // Strings should not be concatenated using '+' in a loop
                        prev.InnerHtml = prev.InnerHtml + txt;
#pragma warning restore S1643 // Strings should not be concatenated using '+' in a loop
                    }
                    else
                    {
                        link.ParentNode.InsertBefore(HtmlNode.CreateNode(txt), link);
                    }
                }
            }

            var lastchild = link.ChildNodes.Last();
            if (lastchild.NodeType == HtmlNodeType.Text)
            {
                var match = Regex.Match(lastchild.InnerHtml, "[ .,]+$", RegexOptions.None);
                if (match.Success)
                {
                    var txt = match.Value;
                    var next = link.NextSibling;
                    lastchild.InnerHtml = lastchild.InnerHtml.Substring(0, lastchild.InnerHtml.Length - txt.Length);
                    if (next?.NodeType == HtmlNodeType.Text)
                    {
                        next.InnerHtml = txt + next.InnerHtml;
                    }
                    else
                    {
                        link.ParentNode.InsertAfter(HtmlNode.CreateNode(txt), link);
                    }
                }
            }
        }
    }

    /// <summary>Add "target=_blank" to http links.</summary>
    /// <param name="document">The document to process.</param>
    /// <param name="addRelNoOpener">A value indicating whether "rel=noopener" should be added as well.</param>
    internal static void AddBlankLinkTargets(HtmlDocument document, bool addRelNoOpener)
    {
        var links = document.DocumentNode.SelectNodes("//a") ?? Enumerable.Empty<HtmlNode>();

        foreach (var link in links.Where(l => l.GetAttributeValue("href", string.Empty).StartsWith("http", StringComparison.Ordinal)))
        {
            if (link.Attributes["target"] == null)
            {
                link.Attributes.Add("target", "_blank");
            }

            if (addRelNoOpener)
            {
                // https://developer.mozilla.org/en-US/docs/Web/HTML/Link_types/noopener
                if (link.Attributes["rel"] == null)
                {
                    link.Attributes.Add("rel", "noopener");
                }
                else
                {
                    var attr = link.Attributes["rel"];

                    if (!attr.Value.Contains("noopener"))
                    {
                        attr.Value += " noopener";
                    }
                }
            }
        }
    }

    /// <summary>
    /// If a TD or LI contains a single P element, then remove that P (keeping other markup)
    /// </summary>
    /// <param name="doc">The HtmlDocument.</param>
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

                // now a single P is really the only node, or it is a P followed by a UL or OL
                if (FirstNodeIsParagraph(cell) && SecondNodeIsEmptyOrList(cell))
                {
                    var para = cell.ChildNodes[0];
                    RemoveSurroundingTags(para);
                }
            }
        }

        bool FirstNodeIsParagraph(HtmlNode node) =>
            node.ChildNodes.Count >= 1 && node.ChildNodes[0].Name.Equals("p", StringComparison.OrdinalIgnoreCase);

        bool SecondNodeIsEmptyOrList(HtmlNode node) => 
            node.ChildNodes.Count == 1
            || (node.ChildNodes.Count == 2
                && (node.ChildNodes[1].Name.Equals("ul", StringComparison.OrdinalIgnoreCase) 
                   || node.ChildNodes[1].Name.Equals("ol", StringComparison.OrdinalIgnoreCase)));
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
            // space-only node - can be replaced by a single space
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
    /// Remove several empty elements 
    /// </summary>
    /// <param name="document"></param>
    internal static void RemoveEmptyElements(HtmlDocument document)
    {
        // empty span nodes - remove
        ReplaceEmptyElement("span", " ");

        // and empty paragraphs
        ReplaceEmptyElement("p", " ");

        RemoveEmptyElements("strong");
        RemoveEmptyElements("em");
        RemoveEmptyElements("a");

        void ReplaceEmptyElement(string elementname, string replacement)
        {
            var elements = document.DocumentNode.SelectNodes("//" + elementname) ?? Enumerable.Empty<HtmlNode>();
            foreach (var element in elements.Where(e => !e.HasChildNodes && string.IsNullOrWhiteSpace(e.InnerText)))
            {
                // insert a text-element to replace the span
                var space = HtmlNode.CreateNode(replacement);
                element.ParentNode.ReplaceChild(space, element);
            }
        }
        void RemoveEmptyElements(string elementname)
        {
            var elements = document.DocumentNode.SelectNodes("//" + elementname) ?? Enumerable.Empty<HtmlNode>();
            foreach (var element in elements.Where(e => !e.HasChildNodes && string.IsNullOrWhiteSpace(e.InnerText)))
            {
                element.ParentNode.RemoveChild(element);
            }
        }
    }

    /// <summary>
    /// Remove all style and class attributes.
    /// </summary>
    /// <param name="document">The document.</param>
    internal static void ClearStyling(HtmlDocument document) => ClearStyling(document?.DocumentNode);

    /// <summary>
    /// Remove all style and class attributes.
    /// </summary>
    /// <param name="node"></param>
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
                    if (AttributeWhitelist.TryGetValue(node.Name, out List<string> whitelist))
                    {
                        var toremove = node.Attributes.Where(a => !whitelist.Contains(a.Name, StringComparer.OrdinalIgnoreCase))
                            .Select(a => a.Name)
                            .ToList();

                        foreach (var name in toremove)
                        {
                            node.Attributes.Remove(name);
                        }
                    }
                    else
                    {
                        // no attributes whitelisted, so remove all
                        node.Attributes.RemoveAll();
                    }

                    ProcessChildren(node);
                    break;
            }
        }

        static void ProcessChildren(HtmlNode parent)
        {
            if (parent.ChildNodes != null && parent.ChildNodes.Count != 0)
            {
                foreach (var child in parent.ChildNodes)
                {
                    ClearStyling(child);
                }
            }
        }
    }

    /// <summary>
    /// Remove elements that don't belong in a CMS text, including contents.
    /// </summary>
    /// <param name="document"></param>
    internal static void RemoveNonCMSElements(HtmlDocument document)
    {
        // remove all comments
        foreach (var cmt in document.DocumentNode.SelectNodes("//comment()") ?? Enumerable.Empty<HtmlNode>())
        {
            cmt.ParentNode.RemoveChild(cmt);
        }

        // keep iframe - may contain video
        foreach (var elt in NonCmsElementsToRemove)
        {
            var xpath = ".//" + elt;
            var toremove = document.DocumentNode.SelectNodes(xpath) ?? Enumerable.Empty<HtmlNode>();
            foreach (var child in toremove)
            {
                child.Remove();
            }
        }
    }

    /// <summary>
    /// Replace styling nodes with modern versions, optionally remove them completely.
    /// </summary>
    /// <param name="document">The document.</param>
    /// <param name="markupToRemove">The style markup to remove.</param>
    private static void TranslateStyleNodes(HtmlDocument document, StyleElements markupToRemove)
    {
        // normalize "b" and "i" to "strong" and "em"
        Replace(document.DocumentNode, "b", "strong");
        Replace(document.DocumentNode, "i", "em");

        // remove any "font" tag
        Replace(document.DocumentNode, "font", "span");

        if (markupToRemove.HasFlag(StyleElements.Bold))
        {
            // remove all "strong" tags (which includes the recently renamed "b")
            RemoveSurroundingTags(document, "strong");
        }

        if (markupToRemove.HasFlag(StyleElements.Italic))
        {
            // remove all "em" tags (which includes the recently renamed "i")
            RemoveSurroundingTags(document, "em");
        }

        if (markupToRemove.HasFlag(StyleElements.Underline))
        {
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

    /// <summary>
    /// Convert the html text to plain text.
    /// </summary>
    /// <param name="html"></param>
    /// <returns></returns>
    public static string HtmlToPlainText(string html)
    {
        if (html is null)
        {
            throw new ArgumentNullException(nameof(html));
        }

        var sb = new StringBuilder(html.Length);

        var doc = CreateHtmlDocument(html);

        ConvertToPlainText(sb, doc.DocumentNode);

        return CleanupText(sb.ToString());
    }

    private static string CleanupText(string text)
    {
        // remove trailing spaces from lines, add extra line break
        text = Regex.Replace(text, @"(\s*\r\n){3,}", Environment.NewLine + Environment.NewLine, RegexOptions.Multiline);

        return text.Trim();
    }

    private static void ConvertToPlainText(StringBuilder sb, HtmlNode node)
    {
        if (node != null)
        {
            if (node.NodeType == HtmlNodeType.Text)
            {
                if (string.IsNullOrWhiteSpace(node.InnerText))
                {
                    sb.Append(' ');
                }
                else
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

        static void ProcessChildren(StringBuilder sb1, HtmlNode node1)
        {
            foreach (var child in node1.ChildNodes)
            {
                ConvertToPlainText(sb1, child);
            }
        }
    }
}
