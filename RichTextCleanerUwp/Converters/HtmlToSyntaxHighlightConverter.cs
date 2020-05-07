using RichTextCleaner.Common.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Windows.UI.Xaml.Documents;
using Windows.UI.Xaml.Media;

namespace RichTextCleanerUwp.Converters
{
    /// <summary>
    /// Provides very basic syntax highlighting for HTML source: tags in grey (including attributes), entities in brown, the rest is black (default).
    /// Note that this assumes reasonably clean HTML.
    /// </summary>
    /// <seealso cref="StringToSimpleInlineConverter" />
    public class HtmlToSyntaxHighlightConverter : StringToSimpleInlineConverter
    {
        private static readonly Brush entityForegroundBrush = new SolidColorBrush(Windows.UI.Colors.Brown);
        private static readonly Brush tagForegroundBrush = new SolidColorBrush(Windows.UI.Colors.Gray);
        private static readonly FontFamily nonTextFont = new FontFamily("Consolas");

        protected override IEnumerable<Inline> SyntaxHighlightHtml(string source)
        {
            try
            {
                return SyntaxHighlightHtmlImpl(source);
            }
            catch (Exception ex)
            {
                Logger.Log(LogLevel.Error, nameof(HtmlToSyntaxHighlightConverter), "Error converting to highlight Inlines", ex);
                return Enumerable.Repeat(new Run { Text = source }, 1);
            }
        }

        private IEnumerable<Inline> SyntaxHighlightHtmlImpl(string source)
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

        private Run CreateRun(State state, string text)
        {
            if (string.IsNullOrEmpty(text))
            {
                // this is filtered out later
                return null;
            }

            var run = new Run { Text = text };
            switch (state)
            {
                case State.Entity:
                    run.Foreground = entityForegroundBrush;
                    run.FontSize -= 2.0;
                    run.FontFamily = nonTextFont;
                    run.FontStretch = Windows.UI.Text.FontStretch.ExtraCondensed;
                    break;

                case State.Tag:
                    run.Foreground = tagForegroundBrush;
                    run.FontSize -= 2.0;
                    run.FontFamily = nonTextFont;
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
