using RichTextCleaner.Common.Support;
using System;
using Windows.UI.Xaml.Data;

namespace RichTextCleanerUwp.Converters
{
    public class LinkResultToDisplayTextConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
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
                case LinkCheckSummary.SimpleChange: return "🔀 Simple redirect";
                case LinkCheckSummary.Updated: return "✔ Updated";
            }

            return res.ToString();
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language) => throw new NotImplementedException();
    }
}
