using System.Collections.Generic;
using System.Text;
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

            foreach (var character in source)
            {
                switch (character)
                {
                    case '<':
                        yield return CreateRun(state, text.ToString());

                        // first char of new run
                        text.Clear();
                        text.Append(character);
                        state = State.Tag;
                        break;

                    case '>' when state == State.Tag:
                    case ';' when state == State.Entity:
                        // include final char of run, which is then non-empty
                        text.Append(character);
                        yield return CreateRun(state, text.ToString());
                       
                        text.Clear();
                        state = State.Text;
                        break;

                    case '&' when state == State.Text:
                        yield return CreateRun(state, text.ToString());

                        // first char of new run
                        text.Clear();
                        text.Append(character);
                        state = State.Entity;
                        break;

                    case ' ' when state == State.Entity:
                        // illegal situation, or maybe plain text. Just ignore the "entity" and treat as plain text
                        state = State.Text;
                        text.Append(character);

                        break;

                    default:
                        // any other text, including surrogate pairs, newlines
                        text.Append(character);
                        break;
                }
            }

            // any remaining text, which cannot be an entity (because not closed with ;)
            if (state == State.Entity)
            {
                state = State.Text;
            }

            yield return CreateRun(state, text.ToString());
        }

        private static Run CreateRun(State state, string text)
        {
            if (string.IsNullOrEmpty(text))
            {
                // this is filtered out later
                return null;
            }

            var run = new Run(text);
            switch (state)
            {
                case State.Entity:
                    run.Foreground = Brushes.Brown;
                    run.FontSize -= 1.0;
                    break;

                case State.Tag:
                    run.Foreground = Brushes.Gray;
                    break;

                default:
                    // for Text, just stay default
                    break;
            }

            return run;
        }

        private enum State
        {
            Text,
            Tag,
            Entity
        }
    }
}
