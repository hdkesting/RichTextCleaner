using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using HtmlAgilityPack;

namespace RichTextCleaner
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void CopyFromClipboard(object sender, RoutedEventArgs e)
        {
            if (Clipboard.ContainsText(TextDataFormat.Html))
            {
                this.TextContent.Text = this.GetHtmlFragment(Clipboard.GetText(TextDataFormat.Html));

            }
            else if (Clipboard.ContainsText(TextDataFormat.Text))
            {
                this.TextContent.Text = Clipboard.GetText(TextDataFormat.Text);
            }
            else if (Clipboard.ContainsText(TextDataFormat.UnicodeText))
            {
                this.TextContent.Text = Clipboard.GetText(TextDataFormat.UnicodeText);
            }
            else
            {
                MessageBox.Show("Couldn't read text from clipboard");
            }
        }

        private string GetHtmlFragment(string html)
        {
            // pasted HTML has some extra details (like where it came from) - remove that
            if (string.IsNullOrWhiteSpace(html))
            {
                return string.Empty;
            }

            int idx = html.IndexOf(ClipboardHelper.StartFragment, StringComparison.Ordinal);
            if (idx >=0)
            {
                html = html.Substring(idx + ClipboardHelper.StartFragment.Length);
            }

            idx = html.IndexOf(ClipboardHelper.EndFragment, StringComparison.Ordinal);
            if (idx >=0)
            {
                html = html.Substring(0, idx);
            }

            return html;
        }

        private void ClearStylingAndCopy(object sender, RoutedEventArgs e)
        {
            string html = this.TextContent.Text;

            var doc = new HtmlDocument();
            doc.LoadHtml(html);
            this.ClearStyling(doc.DocumentNode);

            using(var sw = new StringWriter())
            {
                doc.Save(sw);
                html = sw.ToString();
            }

            ClipboardHelper.CopyToClipboard(html, html);
            this.TextContent.Text = html;
            MessageBox.Show("The cleaned html is on the clipboard, use Ctrl-V to paste");
        }

        private void ClearStyling(HtmlNode node)
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
        }

        private void PlainTextAndCopy(object sender, RoutedEventArgs e)
        {
            string html = this.TextContent.Text;
            StringBuilder sb = new StringBuilder(html.Length);

            var doc = new HtmlDocument();
            doc.LoadHtml(html);
            this.ConvertToPlainText(sb, doc.DocumentNode);

            string text = this.CleanupText(sb.ToString());

            ClipboardHelper.CopyPlainTextToClipboard(text);
            this.TextContent.Text = text;
            MessageBox.Show("The plain text is on the clipboard, use Ctrl-V to paste");
        }

        private string CleanupText(string text)
        {
            var sb = new StringBuilder(text.Length);
            using (var sr = new StringReader(text))
            {
                string? line;
                while ((line = sr.ReadLine()) != null)
                {
#pragma warning disable S2583 // Conditionally executed blocks should be reachable - false positive (can't be null but CAN be whitespace)
                    if (string.IsNullOrWhiteSpace(line))
#pragma warning restore S2583 // Conditionally executed blocks should be reachable
                    {
                        sb.AppendLine();
                    }
                    else
                    {
                        sb.AppendLine(line.TrimEnd());
                    }
                }
            }

            text = sb.ToString();

            text = Regex.Replace(text, @"(\r\n)+", Environment.NewLine, RegexOptions.Multiline);

            return text;
        }

        private void ConvertToPlainText(StringBuilder sb, HtmlNode node)
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
                        case "TABLE":
                            ProcessChildren(sb, node);
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
