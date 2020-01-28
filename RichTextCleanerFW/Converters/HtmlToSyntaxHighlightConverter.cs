using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;

namespace RichTextCleanerFW.Converters
{
    /// <summary>
    /// Provides very basic syntax highlighting for HTML source: tags in grey (including attributes), entities in blue, the rest is black (default).
    /// Note that this assumes reasonably clean HTML.
    /// </summary>
    /// <seealso cref="RichTextCleanerFW.Converters.StringToSimpleInlineConverter" />
    public class HtmlToSyntaxHighlightConverter : StringToSimpleInlineConverter
    {
        protected override IEnumerable<Inline> SyntaxHighlightHtml(string source)
        {
            if (string.IsNullOrWhiteSpace(source))
            {
                yield return new Run();
                yield break;
            }

            var text = new StringBuilder();
            var state = State.Text;
            Run run = new Run();

            foreach (var character in source)
            {
                switch (character)
                {
                    case '<':
                        if (text.Length > 0)
                        {
                            run.Text = text.ToString();
                            yield return run;
                            text.Clear();
                        }

                        // first char of new run
                        text.Append(character);
                        state = State.Tag;
                        run = new Run();
                        run.Foreground = Brushes.Gray;
                        break;

                    case '>' when state == State.Tag:
                    case ';' when state == State.Entity:
                        // include final char of run, which is then non-empty
                        text.Append(character);
                        run.Text = text.ToString();
                        yield return run;
                        text.Clear();

                        state = State.Text;
                        run = new Run();
                        run.FontWeight = FontWeights.Normal;
                        break;

                    case '&' when state == State.Text:
                        if (text.Length > 0)
                        {
                            run.Text = text.ToString();
                            yield return run;
                            text.Clear();
                        }

                        // first char of new run
                        text.Append(character);
                        state = State.Entity;
                        run = new Run();
                        run.Foreground = Brushes.Brown;
                        break;

                    case ' ' when state == State.Entity:
                        // illegal situation, or maybe plain text. Just ignore the "entity" and treat as plain text
                        state = State.Text;
                        text.Append(character);
                        run = new Run();
                        run.FontWeight = FontWeights.Normal;

                        break;

                    default:
                        // any other text, including surrogate pairs, newlines
                        text.Append(character);
                        break;
                }
            }

            // any remaining text
            if (text.Length > 0)
            {
                run.Text = text.ToString();
                yield return run;
                text.Clear();
            }
        }

        private enum State
        {
            Text,
            Tag,
            Entity
        }
    }
}
