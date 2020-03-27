using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;

namespace RichTextCleaner.Converters
{
    public static class TextBlockExtensions
    {
        // https://stackoverflow.com/a/47599775/121309

        public static readonly DependencyProperty BindableInlinesProperty =
            DependencyProperty.RegisterAttached(
                "BindableInlines", 
                typeof(IEnumerable<Inline>), 
                typeof(TextBlockExtensions), 
                new PropertyMetadata(null, OnBindableInlinesChanged));

        public static IEnumerable<Inline> GetBindableInlines(DependencyObject target)
        {
            return target?.GetValue(BindableInlinesProperty) as IEnumerable<Inline> ?? Enumerable.Empty<Inline>();
        }

        public static void SetBindableInlines(DependencyObject target, IEnumerable<Inline> value)
        {
            target?.SetValue(BindableInlinesProperty, value);
        }

        private static void OnBindableInlinesChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var target = d as TextBlock;

            if (target != null)
            {
                target.Inlines.Clear();
                target.Inlines.AddRange((System.Collections.IEnumerable)e.NewValue);
            }
        }
    }
}
