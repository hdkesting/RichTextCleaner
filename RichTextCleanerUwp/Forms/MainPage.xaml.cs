using RichTextCleaner.Common;
using RichTextCleaner.Common.Logging;
using RichTextCleanerUwp.Helpers;
using RichTextCleanerUwp.Models;
using System;
using System.Linq;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.System;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;

namespace RichTextCleanerUwp.Forms
{
    /// <summary>
    /// The main page of the cleaner, containing menu buttons and work area.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public static readonly DependencyProperty SourceValueProperty = DependencyProperty.Register(nameof(SourceValue), typeof(string), typeof(MainPage), null);

        private readonly Brush StatusForegroundHighlight;
        private readonly Brush StatusForeground;

        public MainPage()
        {
            this.InitializeComponent();
            this.DataContext = this;

            var appView = Windows.UI.ViewManagement.ApplicationView.GetForCurrentView();
            appView.Title = "Main";

            var appVersion = GetAppVersion();
            VersionLabel.Text = appVersion;

#if DEBUG
            VersionLabel.Text += " 🐛";
#endif

            StatusForeground = StatusLabel.Foreground;
            StatusForegroundHighlight = new SolidColorBrush(Colors.Black);

            var htmllib = typeof(HtmlAgilityPack.HtmlDocument).Assembly.GetName();
            var cleanerver = typeof(TextCleaner).Assembly.GetName().Version;

            Logger.Log(LogLevel.Information, "MainPage Startup", $"Version {appVersion} has started, using cleaner {cleanerver} and {htmllib.Name} version {htmllib.Version}.");

            // restore source value, required in case of a navigate-back
            this.SourceValue = CleanerSettings.Instance.HtmlSource;
        }

        /// <summary>
        /// Gets or sets the source value to be converted (or that just was converted).
        /// </summary>
        /// <value>
        /// The source value.
        /// </value>
        public string SourceValue
        {
            get { return (string)this.GetValue(SourceValueProperty); }
            private set 
            { 
                this.SetValue(SourceValueProperty, value);
                CleanerSettings.Instance.HtmlSource = value;
            }
        }

        #region Callbacks and eventhandlers

        /// <summary>
        /// Handles the KeyDown event of the Page control. Is linked to rootFrame.KeyDown, to catch *all* key presses.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="KeyRoutedEventArgs"/> instance containing the event data.</param>
        public async void Page_KeyDown(object sender, KeyRoutedEventArgs e)
        {
            Logger.Log(LogLevel.Debug, nameof(MainPage), $"Key pressed {e.Key}");

            await Dispatcher.SwitchToUi();

            switch (e.Key)
            {
                // "Ctrl-V" - paste
                case VirtualKey.V:
                    // ignored (don't know how): check for Ctrl
                    await this.CopyFromClipboardAsync().ConfigureAwait(false);
                    break;

                // "Ctrl-C" - copy
                case VirtualKey.C:
                    await this.ClearStylingAndCopyAsync().ConfigureAwait(false);
                    break;

                // "Ctrl-T"- copy text
                case VirtualKey.T:
                    await this.PlainTextAndCopyAsync().ConfigureAwait(false);
                    break;

                case VirtualKey.H:
                    await this.CopySourceAsText().ConfigureAwait(false);
                    break;

                case VirtualKey.F1:
                case VirtualKey.F:
                    try
                    {
                        await Launcher.LaunchFolderAsync(await Windows.Storage.StorageFolder.GetFolderFromPathAsync(Logger.LogFolder));
                    }
                    catch
                    {
                        // ignore any problem
                    }
                    break;

                case VirtualKey.L:
                    await this.CheckLinksAsync().ConfigureAwait(false);
                    break;

                case VirtualKey.Delete:
                case VirtualKey.Back:
                    this.SourceValue = null;
                    break;
            }
        }

        private async void CopyFromClipboardClick(object sender, RoutedEventArgs e)
        {
            await this.CopyFromClipboardAsync().ConfigureAwait(false);
        }

        private async void ClearStylingAndCopyClick(object sender, RoutedEventArgs e)
        {
            await this.ClearStylingAndCopyAsync().ConfigureAwait(false);
        }

        private async void PlainTextAndCopyClick(object sender, RoutedEventArgs e)
        {
            await this.PlainTextAndCopyAsync().ConfigureAwait(false);
        }

        private async void CheckLinksClick(object sender, RoutedEventArgs e)
        {
            await this.CheckLinksAsync().ConfigureAwait(true);
        }

        private void OpenSettingsWindowClick(object sender, RoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(SettingsPage), null);
        }

        private async Task SetStatusAsync(string message)
        {
            StatusLabel.Text = "";
            await Task.Delay(200).ConfigureAwait(true); // we need to stay on the UI thread

            StatusLabel.Text = message;
            StatusLabel.Foreground = StatusForegroundHighlight;
            
            await Task.Delay(500).ConfigureAwait(true);

            StatusLabel.Foreground = StatusForeground;
        }

        #endregion

        private async Task CopyFromClipboardAsync()
        {
            Logger.Log(LogLevel.Debug, nameof(MainPage), "Start copying text from clipboard");

#pragma warning disable CA1031 // Do not catch general exception types
            try
            {
                var text = await ClipboardHelper.GetTextFromClipboardAsync();
                if (text is null)
                {
                    Logger.Log(LogLevel.Error, nameof(CopyFromClipboardAsync), "Couldn't read text from clipboard");
                    await this.SetStatusAsync("Couldn't read text from clipboard");
                }
                else
                {
                    this.SourceValue = text;
                    Logger.Log(LogLevel.Debug, nameof(CopyFromClipboardAsync), $"Copied text from clipboard ({text.Length} chars)");
                    await this.SetStatusAsync("Copied text from clipboard.").ConfigureAwait(false);
                }
            }
            catch (Exception ex)
            {
                Logger.Log(LogLevel.Error, nameof(CopyFromClipboardAsync), "Error reading clipboard", ex);
                await this.SetStatusAsync("There was an error reading from the clipboard");
            }
#pragma warning restore CA1031 // Do not catch general exception types
            Logger.Log(LogLevel.Debug, nameof(MainPage), "Done copying text from clipboard");
        }

        private async Task ClearStylingAndCopyAsync()
        {
            Logger.Log(LogLevel.Debug, nameof(MainPage), "Start clearing styling");
            string html = this.SourceValue;
            Logger.Log(LogLevel.Debug, nameof(MainPage), $"HTML size before processing: {html?.Length ?? 0}");

#pragma warning disable CA1031 // Do not catch general exception types
            try
            {
                html = TextCleaner.ClearStylingFromHtml(
                    html,
                    CleanerSettings.Instance);
            }
            catch (Exception ex)
            {
                Logger.Log(LogLevel.Error, nameof(ClearStylingAndCopyAsync), "Error cleaning HTML:" + Environment.NewLine + html, ex);
                await this.SetStatusAsync("There was an error cleaning the HTML");
                return;
            }

            Logger.Log(LogLevel.Debug, nameof(MainPage), $"HTML size after processing: {html?.Length ?? 0}");

            try
            {
                ClipboardHelper.CopyToClipboard(html, html);
                this.SourceValue = html;
                Logger.Log(LogLevel.Debug, nameof(ClearStylingAndCopyAsync), "Cleaned HTML and copied to clipboard");
            }
            catch (Exception ex)
            {
                Logger.Log(LogLevel.Error, nameof(ClearStylingAndCopyAsync), "Error writing HTML to clipboard", ex);
                await this.SetStatusAsync("There was an error writing the cleand HTML to the clipboard");
                return;
            }
#pragma warning restore CA1031 // Do not catch general exception types

            await this.SetStatusAsync("The cleaned HTML is on the clipboard, use Ctrl-V to paste.").ConfigureAwait(false);
            Logger.Log(LogLevel.Debug, nameof(MainPage), "Done clearing styling");
        }

        private async Task PlainTextAndCopyAsync()
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
                Logger.Log(LogLevel.Error, nameof(PlainTextAndCopyAsync), "Error cleaning HTML to text:" + Environment.NewLine + html, ex);
                await this.SetStatusAsync("There was an error getting text from the HTML");
                return;
            }

            try
            {
                ClipboardHelper.CopyPlainTextToClipboard(text);
            }
            catch (Exception ex)
            {
                Logger.Log(LogLevel.Error, nameof(PlainTextAndCopyAsync), "Error writing TEXT to clipboard", ex);
                await this.SetStatusAsync("There was an error writing the TEXT to the clipboard");
                return;
            }
#pragma warning restore CA1031 // Do not catch general exception types

            this.SourceValue = text;
            await this.SetStatusAsync("The plain TEXT is on the clipboard, use Ctrl-V to paste.").ConfigureAwait(false);
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
                await this.SetStatusAsync("There was an error writing the source TEXT to the clipboard");
                return;
            }
#pragma warning restore CA1031 // Do not catch general exception types

            await this.SetStatusAsync("The HTML source is on the clipboard as Text, use Ctrl-V to paste.").ConfigureAwait(false);
        }

        private async Task CheckLinksAsync()
        {
            var links = LinkChecker.FindLinks(this.SourceValue);
            if (!links.Any())
            {
                await this.SetStatusAsync("No links found to check.").ConfigureAwait(false);
                return;
            }

            this.Frame.Navigate(typeof(LinkCheckerWindow));

            await Task.CompletedTask;
        }

        private static string GetAppVersion()
        {
            Package package = Package.Current;
            PackageId packageId = package.Id;
            PackageVersion version = packageId.Version;
            return $"{version.Major}.{version.Minor}.{version.Build}";
        }
    }
}
