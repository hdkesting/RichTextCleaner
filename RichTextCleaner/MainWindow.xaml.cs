using System.Windows;

namespace RichTextCleaner
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void CopyFromClipboard(object sender, RoutedEventArgs e)
        {
            var text = ClipboardHelper.GetTextFromClipboard();
            if (text is null)
            {
                MessageBox.Show("Couldn't read text from clipboard");
            }
            else
            {
                this.TextContent.Text = text;
            }
        }

        private void ClearStylingAndCopy(object sender, RoutedEventArgs e)
        {
            string html = this.TextContent.Text;

            html = TextCleaner.ClearStylingFromHtml(html);

            ClipboardHelper.CopyToClipboard(html, html);
            this.TextContent.Text = html;
            MessageBox.Show("The cleaned html is on the clipboard, use Ctrl-V to paste");
        }

        private void PlainTextAndCopy(object sender, RoutedEventArgs e)
        {
            string html = this.TextContent.Text;

            string text = TextCleaner.HtmlToPlainText(html);

            ClipboardHelper.CopyPlainTextToClipboard(text);
            this.TextContent.Text = text;
            MessageBox.Show("The plain text is on the clipboard, use Ctrl-V to paste");
        }


        private void Grid_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            switch (e.Key)
            {
                // "Ctrl-V" - paste
                case System.Windows.Input.Key.V:
                    // ignored (don't know how): check for Ctrl
                    this.CopyFromClipboard(this, new RoutedEventArgs());

                    break;

                // "Ctrl-C" - copy
                case System.Windows.Input.Key.C:
                    this.ClearStylingAndCopy(this, new RoutedEventArgs());
                    break;
            }
        }
    }
}
