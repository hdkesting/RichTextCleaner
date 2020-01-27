using System.Collections.Generic;
using System.Text;
using System.Windows.Documents;
using System.Windows.Media;

namespace RichTextCleanerFW.Converters
{
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

                        text.Append(character);
                        state = State.Tag;
                        run = new Run();
                        run.Foreground = Brushes.Gray;
                        break;

                    case '>' when state == State.Tag:
                    case ';' when state == State.Entity:
                        text.Append(character);
                        run.Text = text.ToString();
                        yield return run;
                        text.Clear();

                        state = State.Text;
                        run = new Run();
                        run.FontWeight = System.Windows.FontWeights.Normal;
                        break;

                    case '&' when state == State.Text:
                        if (text.Length > 0)
                        {
                            run.Text = text.ToString();
                            yield return run;
                            text.Clear();
                        }

                        text.Append(character);
                        state = State.Entity;
                        run = new Run();
                        run.Foreground = Brushes.Blue;
                        break;

                    default:
                        text.Append(character);
                        break;
                }
            }

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
