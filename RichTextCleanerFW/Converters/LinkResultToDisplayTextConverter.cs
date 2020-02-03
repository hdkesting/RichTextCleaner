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
            var res = (LinkCheckResult)value;

            switch (res)
            {
                case LinkCheckResult.NotCheckedYet: return "❓ Working ⋯";
                case LinkCheckResult.Ignored: return "- Ignored";
                case LinkCheckResult.Ok: return "✔ Ok";
                case LinkCheckResult.NotFound: return "⛔ Not found";
                case LinkCheckResult.Error: return "❌ Error";
                case LinkCheckResult.Timeout: return "⏲ Timeout";
                case LinkCheckResult.Redirected: return "🔀 Redirect";
            }

            return res.ToString();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
    }
}
