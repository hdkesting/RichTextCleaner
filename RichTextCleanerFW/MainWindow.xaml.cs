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
            var text = ClipboardHelper.GetTextFromClipboard();
            if (text is null)
            {
                MessageBox.Show("Couldn't read text from clipboard");
            }
            else
            {
                this.TextContent.Text = text;
                await this.SetStatus("Copied text from clipboard.");
            }
        }

        private async void ClearStylingAndCopy(object sender, RoutedEventArgs e)
        {
            string html = this.TextContent.Text;

            html = TextCleaner.ClearStylingFromHtml(html, ClearStyleMarkup.IsChecked.GetValueOrDefault());
            ClipboardHelper.CopyToClipboard(html, html);
            this.TextContent.Text = html;
            await this.SetStatus("The cleaned HTML is on the clipboard, use Ctrl-V to paste.");
        }

        private async void PlainTextAndCopy(object sender, RoutedEventArgs e)
        {
            string html = this.TextContent.Text;

            string text = TextCleaner.HtmlToPlainText(html);

            ClipboardHelper.CopyPlainTextToClipboard(text);
            this.TextContent.Text = text;
            await this.SetStatus("The plain TEXT is on the clipboard, use Ctrl-V to paste.");
        }

        private async Task CopySourceAsText()
        {
            string html = this.TextContent.Text;
            ClipboardHelper.CopyPlainTextToClipboard(html);
            await this.SetStatus("The HTML source is on the clipboard as Text, use Ctrl-V to paste.");
        }

        private async Task SetStatus(string message)
        {
            StatusLabel.Text = "";
            await Task.Delay(200);

            StatusLabel.Text = message;
            StatusLabel.Foreground = Brushes.Black;
            StatusLabel.Background = Brushes.LightYellow;
            await Task.Delay(500);
            StatusLabel.Foreground = StatusForeground;
            StatusLabel.Background = StatusBackground;
        }

        private async void Grid_KeyDown(object sender, KeyEventArgs e)
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

                // "Ctrl-T"- copy text
                case System.Windows.Input.Key.T:
                    this.PlainTextAndCopy(this, new RoutedEventArgs());
                    break;

                case Key.H:
                    await this.CopySourceAsText();
                    break;
            }
        }
    }
}
