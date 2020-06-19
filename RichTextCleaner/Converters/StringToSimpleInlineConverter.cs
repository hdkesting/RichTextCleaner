using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows.Data;
using System.Windows.Documents;

namespace RichTextCleaner.Converters
{
    public class StringToSimpleInlineConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string source = (value as string) ?? string.Empty;

            if (string.IsNullOrWhiteSpace(source))
            {
                return GetStartupMessage();
            }

            // don't emit null-runs
            return SyntaxHighlightHtml(source).Where(x => !(x is null)).ToList();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();

        protected virtual IEnumerable<Inline?> SyntaxHighlightHtml(string source)
        {
            var run = new Run
            {
                Text = source
            };

            return Enumerable.Repeat((Inline)run, 1);
        }

        private static List<Inline> GetStartupMessage()
        {
            var lines = new List<Inline>();
            var run = new Run("Cleaning up HTML fragments.")
            {
                FontWeight = System.Windows.FontWeights.Bold
            };
            lines.Add(run);

            run = new Run(@"

(1) Select and copy (a part of) an HTML page, or HTML source.");
            lines.Add(run);

            run = new Run(@"

(2) Click the ""Paste"" button (left) to insert the HTML source, or press Ctrl-V.

(3) Click the ""Clear styling"" button (or Ctrl-C) to remove all ""class"" and ""style"" attributes and do some more cleanup.
This also copies the new text onto the clipboard so you can paste it into a Rich Text editor.

You can select to remove bold, italic and/or underline tags (keeping the text inside them) by checking the checkbox.

");
            lines.Add(run);
            run = new Run("OR: ")
            {
                FontWeight = System.Windows.FontWeights.Bold
            };
            lines.Add(run);
            run = new Run("(3) Use the \"text only\" button to get just the text, without any HTML.");
            lines.Add(run);

            run = new Run(@"

(4) Paste this into the Rich Text editor (either design or html tab).");
            lines.Add(run);

            return lines;
        }
    }
}
