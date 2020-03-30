using RichTextCleaner.Common;
using RichTextCleaner.Common.Logging;
using RichTextCleanerUwp.Forms;
using RichTextCleanerUwp.Helpers;
using RichTextCleanerUwp.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.System;
using Windows.UI;
using Windows.UI.Core;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace RichTextCleanerUwp
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public static readonly DependencyProperty SourceValueProperty = DependencyProperty.Register(nameof(SourceValue), typeof(string), typeof(MainPage), null);

        private readonly Brush StatusForegroundHighlight;
        private readonly Brush StatusForeground;
        private LinkCheckerWindow checker;

        public MainPage()
        {
            this.InitializeComponent();
            this.DataContext = this;

            VersionLabel.Text = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version?.ToString(3);

#if DEBUG
            VersionLabel.Text += " 🐛";
#endif

            StatusForeground = StatusLabel.Foreground;
            StatusForegroundHighlight = new SolidColorBrush(Colors.Black);

            // set Assembly Version in the Package tab of the properties of project RichTextCleaner.Common
            var libVersion = typeof(TextCleaner).Assembly.GetName().Version;

            // set Assembly Version in AssemblyInfo.cs in this project (below Properties)
            var appVersion = this.GetType().Assembly.GetName().Version;

            if (libVersion != appVersion)
            {
                Logger.Log(LogLevel.Error, "Startup", $"Version mismatch: app={appVersion}, lib={libVersion}.");
                var mbox = new MessageDialog("The installation didn't succeed properly. Please run the installer to remove the current installation and then install again.");
                mbox.Commands.Add(new UICommand("Close app", CloseApp));
                return;
            }


            var htmllib = typeof(HtmlAgilityPack.HtmlDocument).Assembly.GetName();

            Logger.Log(LogLevel.Information, "Startup", $"Version {appVersion} has started, using {htmllib.Name} version {htmllib.Version}.");

            // this.Closing += this.MainWindow_Closing;
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

        #region Callbacks and eventhandlers

        private async void Page_KeyDown(object sender, KeyRoutedEventArgs e)
        {
            switch (e.Key)
            {
                // "Ctrl-V" - paste
                case VirtualKey.V:
                    // ignored (don't know how): check for Ctrl
                    await this.CopyFromClipboard().ConfigureAwait(false);
                    break;

                // "Ctrl-C" - copy
                case VirtualKey.C:
                    await this.ClearStylingAndCopy().ConfigureAwait(false);
                    break;

                // "Ctrl-T"- copy text
                case VirtualKey.T:
                    await this.PlainTextAndCopy().ConfigureAwait(false);
                    break;

                case VirtualKey.H:
                    await this.CopySourceAsText().ConfigureAwait(false);
                    break;

                case VirtualKey.F1:
                case VirtualKey.F:
                    Process.Start(Logger.LogFolder);
                    break;

                case VirtualKey.L:
                    await CheckLinks().ConfigureAwait(false);
                    break;

                case VirtualKey.Delete:
                case VirtualKey.Back:
                    this.SourceValue = null;
                    break;
            }
        }

        private async void CopyFromClipboard(object sender, RoutedEventArgs e)
        {
            await this.CopyFromClipboard().ConfigureAwait(false);
        }

        private async void ClearStylingAndCopy(object sender, RoutedEventArgs e)
        {
            await this.ClearStylingAndCopy().ConfigureAwait(false);
        }
        private async void PlainTextAndCopy(object sender, RoutedEventArgs e)
        {
            await this.PlainTextAndCopy().ConfigureAwait(false);
        }

        private async void CheckLinks(object sender, RoutedEventArgs e)
        {
            await CheckLinks().ConfigureAwait(true);
        }

        private void OpenSettingsWindow(object sender, RoutedEventArgs e)
        {
            //var settingsWindow = new SettingsWindow();
            //settingsWindow.ShowDialog();
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
            Debug.WriteLine("Checker was closed");
            if (this.checker != null)
            {
                //this.checker.Closed -= Checker_Closed;
                //this.checker.LinkToProcess -= Checker_LinkToProcess;
                //this.checker.Dispose();
            }

            this.checker = null;
        }

        private void CloseApp(IUICommand command) 
        {
            Application.Current.Exit();
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

        private async Task CopyFromClipboard()
        {
#pragma warning disable CA1031 // Do not catch general exception types
            try
            {
                var text = await ClipboardHelper.GetTextFromClipboardAsync();
                if (text is null)
                {
                    Logger.Log(LogLevel.Error, nameof(CopyFromClipboard), "Couldn't read text from clipboard");
                    await this.SetStatusAsync("Couldn't read text from clipboard");
                }
                else
                {
                    this.SourceValue = text;
                    Logger.Log(LogLevel.Debug, nameof(CopyFromClipboard), "Copied text from cliboard");
                    await this.SetStatusAsync("Copied text from clipboard.").ConfigureAwait(false);
                }
            }
            catch (Exception ex)
            {
                Logger.Log(LogLevel.Error, nameof(CopyFromClipboard), "Error reading clipboard", ex);
                await this.SetStatusAsync("There was an error reading from the clipboard");
            }
#pragma warning restore CA1031 // Do not catch general exception types
        }

        private async Task ClearStylingAndCopy()
        {
            string html = this.SourceValue;

#pragma warning disable CA1031 // Do not catch general exception types
            try
            {
                html = TextCleaner.ClearStylingFromHtml(
                    html,
                    CleanerSettings.Instance);
            }
            catch (Exception ex)
            {
                Logger.Log(LogLevel.Error, nameof(ClearStylingAndCopy), "Error cleaning HTML:" + Environment.NewLine + html, ex);
                await this.SetStatusAsync("There was an error cleaning the HTML");
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
                await this.SetStatusAsync("There was an error writing the cleand HTML to the clipboard");
                return;
            }
#pragma warning restore CA1031 // Do not catch general exception types

            await this.SetStatusAsync("The cleaned HTML is on the clipboard, use Ctrl-V to paste.").ConfigureAwait(false);
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
                await this.SetStatusAsync("There was an error getting text from the HTML");
                return;
            }

            try
            {
                ClipboardHelper.CopyPlainTextToClipboard(text);
            }
            catch (Exception ex)
            {
                Logger.Log(LogLevel.Error, nameof(PlainTextAndCopy), "Error writing TEXT to clipboard", ex);
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

        private async Task CheckLinks()
        {
            /*
            this.checker?.Links.Clear();
            var links = LinkChecker.FindLinks(this.SourceValue);
            if (!links.Any())
            {
                MessageBox.Show("No links found.");
                return;
            }

            OpenLinkCheckerWindow();
            this.checker!.Links.Clear();

            foreach (var lnk in links)
            {
                checker.Links.Add(new BindableLinkDescription(lnk));
            }

            checker.Focus();

            await checker.CheckAllLinks().ConfigureAwait(false);
            */
            await Task.CompletedTask;
        }

        private void OpenLinkCheckerWindow()
        {
            if (this.checker == null)
            {
                //this.checker = new LinkCheckerWindow();
                //this.checker.LinkToProcess += this.Checker_LinkToProcess;
                //this.checker.Closed += this.Checker_Closed;
            }

            //this.checker.Show();
        }
    }
}
