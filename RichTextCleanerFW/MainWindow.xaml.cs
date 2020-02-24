using RichTextCleaner.Common;
using RichTextCleaner.Common.Support;
using RichTextCleanerFW.Common.Logging;
using RichTextCleanerFW.Models;
using System;
using System.Diagnostics;
using System.Linq;
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
        public static readonly DependencyProperty SourceValueProperty = DependencyProperty.Register(nameof(SourceValue), typeof(string), typeof(MainWindow));

        private readonly Brush StatusForeground;
        private readonly Brush StatusBackground;
        private LinkCheckerWindow checker;

        public MainWindow()
        {
            InitializeComponent();
            this.DataContext = this;

            if (System.Deployment.Application.ApplicationDeployment.IsNetworkDeployed)
            {
                // clickonce deploy: N/A
                VersionLabel.Content = System.Deployment.Application.ApplicationDeployment.CurrentDeployment.CurrentVersion.ToString();
            }
            else
            {
                VersionLabel.Content = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString(3);
            }

#if DEBUG
            VersionLabel.Content += " 🐛";
#endif

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

            var settings = Properties.Settings.Default;

            this.ClearBoldMarkup.IsChecked = settings.RemoveBold;
            this.ClearItalicMarkup.IsChecked = settings.RemoveItalic;
            this.ClearUnderlineMarkup.IsChecked = settings.RemoveUnderline;
            this.AddBlankTarget.IsChecked = settings.AddTargetBlank;
            this.ChangeToFancyQuotes.IsChecked = (QuoteProcessing)settings.QuoteProcess != QuoteProcessing.NoChange;

            var htmllib = typeof(HtmlAgilityPack.HtmlDocument).Assembly.GetName();

            Logger.Log(LogLevel.Information, "Startup", $"Version {appVersion} has started, using {htmllib.Name} version {htmllib.Version}.");

            this.Closing += this.MainWindow_Closing;
        }

        private void MainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            this.checker?.Close();
        }

        /// <summary>
        /// Gets or sets the source value to be converted (or just converted).
        /// </summary>
        /// <value>
        /// The source value.
        /// </value>
        public string SourceValue
        {
            get { return (string)this.GetValue(SourceValueProperty); }
            private set { this.SetValue(SourceValueProperty, value); }
        }

        private async void CopyFromClipboard(object sender, RoutedEventArgs e)
        {
            await this.CopyFromClipboard().ConfigureAwait(false);
        }

        private async Task CopyFromClipboard()
        {
#pragma warning disable CA1031 // Do not catch general exception types
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
                    this.SourceValue = text;
                    Logger.Log(LogLevel.Debug, nameof(CopyFromClipboard), "Copied text from cliboard");
                    await this.SetStatus("Copied text from clipboard.").ConfigureAwait(false);
                }
            }
            catch (Exception ex)
            {
                Logger.Log(LogLevel.Error, nameof(CopyFromClipboard), "Error reading clipboard", ex);
                MessageBox.Show("There was an error reading from the clipboard", "error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
#pragma warning restore CA1031 // Do not catch general exception types
        }

        private async void ClearStylingAndCopy(object sender, RoutedEventArgs e)
        {
            await this.ClearStylingAndCopy().ConfigureAwait(false);
        }

        private async Task ClearStylingAndCopy()
        {
            string html = this.SourceValue;

#pragma warning disable CA1031 // Do not catch general exception types
            try
            {
                html = TextCleaner.ClearStylingFromHtml(
                    html,
                    GetStyleSetting(),
                    AddBlankTarget.IsChecked.GetValueOrDefault(),
                    GetQuoteSetting());
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
                this.SourceValue = html;
                Logger.Log(LogLevel.Debug, nameof(ClearStylingAndCopy), "Cleaned HTML and copied to clipboard");
            }
            catch (Exception ex)
            {
                Logger.Log(LogLevel.Error, nameof(ClearStylingAndCopy), "Error writing HTML to clipboard", ex);
                MessageBox.Show("There was an error writing the cleand HTML to the clipboard", "error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
#pragma warning restore CA1031 // Do not catch general exception types

            await this.SetStatus("The cleaned HTML is on the clipboard, use Ctrl-V to paste.").ConfigureAwait(false);

            StyleElements GetStyleSetting()
            {
                // settings are stored in C:\Users\<user>\AppData\Local\Hans_Kesting\RichTextCleaner.exe...\<version>\user.config
                var settings = Properties.Settings.Default;
                var styles = StyleElements.None;
                if (ClearBoldMarkup.IsChecked.GetValueOrDefault())
                {
                    styles |= StyleElements.Bold;
                    settings.RemoveBold = true;
                }
                else
                {
                    settings.RemoveBold = false;
                }

                if (ClearItalicMarkup.IsChecked.GetValueOrDefault())
                {
                    styles |= StyleElements.Italic;
                    settings.RemoveItalic = true;
                }
                else
                {
                    settings.RemoveItalic = false;
                }

                if (ClearUnderlineMarkup.IsChecked.GetValueOrDefault())
                {
                    styles |= StyleElements.Underline;
                    settings.RemoveUnderline = true;
                }
                else
                {
                    settings.RemoveUnderline = false;
                }

                settings.AddTargetBlank = AddBlankTarget.IsChecked.GetValueOrDefault();
                settings.Save();

                return styles;
            }

            QuoteProcessing GetQuoteSetting() {
                QuoteProcessing qp;
                if (this.ChangeToFancyQuotes.IsChecked.GetValueOrDefault())
                {
                    qp = QuoteProcessing.ChangeToSmartQuotes;
                }
                else
                {
                    qp = QuoteProcessing.NoChange;
                }

                Properties.Settings.Default.QuoteProcess = (int)qp;
                Properties.Settings.Default.Save();

                return qp;
            }
        }

        private async void PlainTextAndCopy(object sender, RoutedEventArgs e)
        {
            await this.PlainTextAndCopy().ConfigureAwait(false);
        }

        private async Task PlainTextAndCopy()
        {
            string html = this.SourceValue;

            string text;

#pragma warning disable CA1031 // Do not catch general exception types
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
#pragma warning restore CA1031 // Do not catch general exception types

            this.SourceValue = text;
            await this.SetStatus("The plain TEXT is on the clipboard, use Ctrl-V to paste.").ConfigureAwait(false);
        }

        private async Task CopySourceAsText()
        {
            string html = this.SourceValue;

#pragma warning disable CA1031 // Do not catch general exception types
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
#pragma warning restore CA1031 // Do not catch general exception types

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

                case Key.F1:
                case Key.F:
                    Process.Start(Logger.LogFolder);
                    break;

                case Key.L:
                    await CheckLinks().ConfigureAwait(false);
                    break;

                case Key.Delete:
                case Key.Back:
                    this.SourceValue = null;
                    break;
            }
        }

        private void OpenLinkCheckerWindow()
        {
            if (this.checker == null)
            {
                this.checker = new LinkCheckerWindow();
                this.checker.LinkToProcess += this.Checker_LinkToProcess;
                this.checker.Closed += this.Checker_Closed;
            }

            this.checker.Show();
        }

        private void Checker_LinkToProcess(object sender, LinkModificationEventArgs e)
        {
            switch (e.Modification)
            {
                case LinkModification.MarkInvalid:
                    this.SourceValue = LinkChecker.MarkInvalid(this.SourceValue, e.LinkHref);
                    break;

                case LinkModification.UpdateSchema:
                    this.SourceValue = LinkChecker.UpdateHref(this.SourceValue, e.LinkHref, e.NewHref);
                    break;
            }
        }

        private void Checker_Closed(object sender, EventArgs e)
        {
            this.checker.Closed -= Checker_Closed;
            this.checker.LinkToProcess -= Checker_LinkToProcess;
            this.checker = null;
        }

        private async void CheckLinks(object sender, RoutedEventArgs e)
        {
            await CheckLinks().ConfigureAwait(true);
        }

        private async Task CheckLinks()
        {
            checker?.Links.Clear();
            var links = LinkChecker.FindLinks(this.SourceValue);
            if (!links.Any())
            {
                MessageBox.Show("No links found.");
                return;
            }

            OpenLinkCheckerWindow();
            checker.Links.Clear();

            foreach (var lnk in links)
            {
                checker.Links.Add(new BindableLinkDescription(lnk));
            }

            checker.Focus();

            await checker.CheckAllLinks().ConfigureAwait(false);
        }
    }
}
