using RichTextCleaner.Common;
using System;
using System.Globalization;
using System.Windows.Data;

namespace RichTextCleanerFW.Converters
{
    public class LinkResultToDisplayTextConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var res = (LinkCheckSummary)value;

            switch (res)
            {
                case LinkCheckSummary.NotCheckedYet: return "❓ Working ⋯";
                case LinkCheckSummary.Ignored: return "- Ignored";
                case LinkCheckSummary.Ok: return "✔ Ok";
                case LinkCheckSummary.NotFound: return "⛔ Not found";
                case LinkCheckSummary.Error: return "❌ Error";
                case LinkCheckSummary.Timeout: return "⏲ Timeout";
                case LinkCheckSummary.Redirected: return "🔀 Redirect";
                case LinkCheckSummary.SchemaChange: return "🔀 Schema";
                case LinkCheckSummary.Updated: return "✔ Updated";
            }

            return res.ToString();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
    }
}
