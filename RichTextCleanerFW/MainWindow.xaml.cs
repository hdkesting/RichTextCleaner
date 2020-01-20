using RichTextCleaner.Common;
using RichTextCleanerFW.Logging;
using System;
using System.Diagnostics;
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
                // clickonce deploy: N/A
                VersionLabel.Content = System.Deployment.Application.ApplicationDeployment.CurrentDeployment.CurrentVersion.ToString();
            }
            else
            {
                VersionLabel.Content = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString(3);
            }

            StatusForeground = StatusLabel.Foreground;
            StatusBackground = StatusLabel.Background;

            // set Assembly Version in the Package tab of the properties of project RichTextCleaner.Common
            var libVersion = typeof(TextCleaner).Assembly.GetName().Version;

            // set Assembly Version in AssemblyInfo.cs in this project (below Properties)
            var appVersion = this.GetType().Assembly.GetName().Version;

            if (libVersion != appVersion)
            {
                Logger.Log(LogLevel.Error, "Startup", $"Version mismatch: app={appVersion}, lib={libVersion}.");
                MessageBox.Show("The installation didn't succeed properly. Please run the installer to remove the current installation and then install again.",
                    "Installation error", MessageBoxButton.OK, MessageBoxImage.Error);
                Application.Current.Shutdown(1);
                return;
            }

            var htmllib = typeof(HtmlAgilityPack.HtmlDocument).Assembly.GetName();

            Logger.Log(LogLevel.Information, "Startup", $"Version {appVersion} has started, using {htmllib.Name} version {htmllib.Version}.");
        }

        private async void CopyFromClipboard(object sender, RoutedEventArgs e)
        {
            await this.CopyFromClipboard().ConfigureAwait(false);
        }

        private async Task CopyFromClipboard()
        {
            try
            {
                var text = ClipboardHelper.GetTextFromClipboard();
                if (text is null)
                {
                    Logger.Log(LogLevel.Error, nameof(CopyFromClipboard), "Couldn't read text from clipboard");
                    MessageBox.Show("Couldn't read text from clipboard");
                }
                else
                {
                    this.TextContent.Text = text;
                    Logger.Log(LogLevel.Debug, nameof(CopyFromClipboard), "Copied text from cliboard");
                    await this.SetStatus("Copied text from clipboard.").ConfigureAwait(false);
                }
            }
            catch (Exception ex)
            {
                Logger.Log(LogLevel.Error, nameof(CopyFromClipboard), "Error reading clipboard", ex);
                MessageBox.Show("There was an error reading from the clipboard", "error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void ClearStylingAndCopy(object sender, RoutedEventArgs e)
        {
            await this.ClearStylingAndCopy().ConfigureAwait(false);
        }

        private async Task ClearStylingAndCopy()
        {
            string html = this.TextContent.Text;

            try
            {
                html = TextCleaner.ClearStylingFromHtml(
                    html,
                    ClearBoldMarkup.IsChecked.GetValueOrDefault(),
                    ClearItalicMarkup.IsChecked.GetValueOrDefault(),
                    ClearUnderlineMarkup.IsChecked.GetValueOrDefault(),
                    AddBlankTarget.IsChecked.GetValueOrDefault(),
                    QuoteProcessing.NoChange);
            }
            catch (Exception ex)
            {
                Logger.Log(LogLevel.Error, nameof(ClearStylingAndCopy), "Error cleaning HTML:" + Environment.NewLine + html, ex);
                MessageBox.Show("There was an error cleaning the HTML", "error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            try
            {
                ClipboardHelper.CopyToClipboard(html, html);
                this.TextContent.Text = html;
                Logger.Log(LogLevel.Debug, nameof(ClearStylingAndCopy), "Cleaned HTML and copied to clipboard");
            }
            catch (Exception ex)
            {
                Logger.Log(LogLevel.Error, nameof(ClearStylingAndCopy), "Error writing HTML to clipboard", ex);
                MessageBox.Show("There was an error writing the cleand HTML to the clipboard", "error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            await this.SetStatus("The cleaned HTML is on the clipboard, use Ctrl-V to paste.").ConfigureAwait(false);
        }

        private async void PlainTextAndCopy(object sender, RoutedEventArgs e)
        {
            await this.PlainTextAndCopy().ConfigureAwait(false);
        }

        private async Task PlainTextAndCopy()
        {
            string html = this.TextContent.Text;

            string text;

            try
            {
                text = TextCleaner.HtmlToPlainText(html);
            }
            catch (Exception ex)
            {
                Logger.Log(LogLevel.Error, nameof(PlainTextAndCopy), "Error cleaning HTML to text:" + Environment.NewLine + html, ex);
                MessageBox.Show("There was an error getting text from the HTML", "error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            try
            {
                ClipboardHelper.CopyPlainTextToClipboard(text);
            }
            catch (Exception ex)
            {
                Logger.Log(LogLevel.Error, nameof(PlainTextAndCopy), "Error writing TEXT to clipboard", ex);
                MessageBox.Show("There was an error writing the TEXT to the clipboard", "error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            this.TextContent.Text = text;
            await this.SetStatus("The plain TEXT is on the clipboard, use Ctrl-V to paste.").ConfigureAwait(false);
        }

        private async Task CopySourceAsText()
        {
            string html = this.TextContent.Text;

            try
            {
                ClipboardHelper.CopyPlainTextToClipboard(html);
            }
            catch (Exception ex)
            {
                Logger.Log(LogLevel.Error, nameof(CopySourceAsText), "Error writing source TEXT to clipboard", ex);
                MessageBox.Show("There was an error writing the source TEXT to the clipboard", "error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

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

                case Key.L:
                    Process.Start(Logger.LogFolder);
                    break;
            }
        }
    }
}
