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
                case LinkCheckResult.NotCheckedYet: return "❓ to do";
                case LinkCheckResult.Ignored: return "- ignored";
                case LinkCheckResult.Ok: return "✔ Ok";
                case LinkCheckResult.NotFound: return "⛔ not found";
                case LinkCheckResult.Error: return "❌ error";
                case LinkCheckResult.Timeout: return "⏲ timeout";
            }

            return res.ToString();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
    }
}
