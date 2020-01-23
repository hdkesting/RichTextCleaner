using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows.Data;
using System.Windows.Documents;

namespace RichTextCleanerFW.Converters
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

            var run = new Run();
            run.Text = source;
            
            return Enumerable.Repeat((Inline)run, 1);
            // return source;
            // throw new NotImplementedException();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();

        private List<Inline> GetStartupMessage()
        {
            var lines = new List<Inline>();


            return lines;
        }
        /*
                        <!--<Run FontWeight="Bold">Cleaning up HTML fragments.</Run><LineBreak/><LineBreak/>
                <Run>Select and copy (a part of) an HTML page, or HTML source.</Run><LineBreak/>
                <Run>Click the "Paste" button (left) to insert the HTML source, or press Ctrl-V.</Run><LineBreak/>
                <LineBreak/>
                <Run>Click the "Clear styling" button (or Ctrl-C) to remove all "class" and "style" attributes and do some more cleanup.</Run><LineBreak/>
                <Run>This also copies the new text onto the clipboard so you can paste it into a Rich Text editor.</Run><LineBreak/>
                <Run>You can select to keep bold, italic or underline tags (keeping the text inside them) by unchecking the checkbox.</Run><LineBreak/>
                <LineBreak/>
                <Run>OR use the "text only" button to get just the text, without any HTML</Run>-->

          */
    }
}
