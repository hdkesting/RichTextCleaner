using RichTextCleaner.Common;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace RichTextCleanerFW
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly Brush StatusForeground;
        private readonly Brush StatusBackground;

        public MainWindow()
        {
            InitializeComponent();

            if (System.Deployment.Application.ApplicationDeployment.IsNetworkDeployed)
            {
                VersionLabel.Content = System.Deployment.Application.ApplicationDeployment.CurrentDeployment.CurrentVersion.ToString();
            }
            else
            {
                VersionLabel.Content = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString();
            }

            StatusForeground = StatusLabel.Foreground;
            StatusBackground = StatusLabel.Background;

        }

        private async void CopyFromClipboard(object sender, RoutedEventArgs e)
        {
            await this.CopyFromClipboard().ConfigureAwait(false);
        }

        private async Task CopyFromClipboard()
        {
            var text = ClipboardHelper.GetTextFromClipboard();
            if (text is null)
            {
                MessageBox.Show("Couldn't read text from clipboard");
            }
            else
            {
                this.TextContent.Text = text;
                await this.SetStatus("Copied text from clipboard.").ConfigureAwait(false);
            }
        }

        private async void ClearStylingAndCopy(object sender, RoutedEventArgs e)
        {
            await this.ClearStylingAndCopy().ConfigureAwait(false);
        }

        private async Task ClearStylingAndCopy()
        {
            string html = this.TextContent.Text;

            html = TextCleaner.ClearStylingFromHtml(html, ClearStyleMarkup.IsChecked.GetValueOrDefault());
            ClipboardHelper.CopyToClipboard(html, html);
            this.TextContent.Text = html;
            await this.SetStatus("The cleaned HTML is on the clipboard, use Ctrl-V to paste.").ConfigureAwait(false);
        }

        private async void PlainTextAndCopy(object sender, RoutedEventArgs e)
        {
            await this.PlainTextAndCopy().ConfigureAwait(false);
        }

        private async Task PlainTextAndCopy()
        {
            string html = this.TextContent.Text;

            string text = TextCleaner.HtmlToPlainText(html);

            ClipboardHelper.CopyPlainTextToClipboard(text);
            this.TextContent.Text = text;
            await this.SetStatus("The plain TEXT is on the clipboard, use Ctrl-V to paste.").ConfigureAwait(false);
        }

        private async Task CopySourceAsText()
        {
            string html = this.TextContent.Text;
            ClipboardHelper.CopyPlainTextToClipboard(html);
            await this.SetStatus("The HTML source is on the clipboard as Text, use Ctrl-V to paste.").ConfigureAwait(false);
        }

        private async Task SetStatus(string message)
        {
            StatusLabel.Text = "";
            await Task.Delay(200).ConfigureAwait(true); // we need to stay on the UI thread

            StatusLabel.Text = message;
            StatusLabel.Foreground = Brushes.Black;
            StatusLabel.Background = Brushes.LightYellow;
            await Task.Delay(500).ConfigureAwait(true);

            StatusLabel.Foreground = StatusForeground;
            StatusLabel.Background = StatusBackground;
        }

        private async void Grid_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                // "Ctrl-V" - paste
                case Key.V:
                    // ignored (don't know how): check for Ctrl
                    await this.CopyFromClipboard().ConfigureAwait(false);
                    break;

                // "Ctrl-C" - copy
                case Key.C:
                    await this.ClearStylingAndCopy().ConfigureAwait(false);
                    break;

                // "Ctrl-T"- copy text
                case Key.T:
                    await this.PlainTextAndCopy().ConfigureAwait(false);
                    break;

                case Key.H:
                    await this.CopySourceAsText().ConfigureAwait(false);
                    break;
            }
        }
    }
}
