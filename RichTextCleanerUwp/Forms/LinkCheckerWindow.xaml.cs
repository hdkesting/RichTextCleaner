using RichTextCleaner.Common;
using RichTextCleaner.Common.Logging;
using RichTextCleaner.Common.Support;
using RichTextCleanerUwp.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Windows.System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace RichTextCleanerUwp.Forms
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class LinkCheckerWindow : Page
    {
        public static readonly DependencyProperty LinksProperty = DependencyProperty.Register(nameof(Links), typeof(ObservableCollection<BindableLinkDescription>), typeof(LinkCheckerWindow), new PropertyMetadata(new ObservableCollection<BindableLinkDescription>()));
        private CancellationTokenSource tokenSource = new CancellationTokenSource();

        public LinkCheckerWindow()
        {
            this.InitializeComponent();
            this.DataContext = this;

            var links = LinkChecker.FindLinks(CleanerSettings.Instance.HtmlSource);
            this.Links.Clear();
            foreach (var lnk in links)
            {
                this.Links.Add(new BindableLinkDescription(lnk));
            }
        }

        private ObservableCollection<BindableLinkDescription> Links
        {
            get { return (ObservableCollection<BindableLinkDescription>)this.GetValue(LinksProperty); }
            set { this.SetValue(LinksProperty, value); }
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            Task.Run(() => Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, async () => await this.CheckAllLinks()));
        }

        protected override void OnNavigatingFrom(NavigatingCancelEventArgs e)
        {
            tokenSource.Cancel();
            base.OnNavigatingFrom(e);
        }

        private async Task CheckAllLinks()
        {
            var tasks = new List<Task>();

            var cancelToken = tokenSource.Token;

            try
            {
                // gradually start checking all that still need (re-)checking
                foreach (var lnk in this.Links.Where(l => l.Result == LinkCheckSummary.NotCheckedYet))
                {
                    await Task.Delay(250).ConfigureAwait(true);
                    tasks.Add(CheckLink(lnk, cancelToken));
                }
            }
            catch (Exception ex)
            {
                Logger.Log(LogLevel.Error, nameof(CheckAllLinks), "Error starting link checking", ex);
            }

            do
            {
                this.MessageLabel.Text = $"Checking all links: still working on {tasks.Count(TaskIsBusy)} out of {tasks.Count}";
                await Task.Delay(TimeSpan.FromSeconds(1)).ConfigureAwait(true);
            }
            while (tasks.Any(TaskIsBusy));

            this.MessageLabel.Text = "Done checking";

            bool TaskIsBusy(Task t) => !(t.IsCompleted || t.IsFaulted || t.IsCanceled);
        }

        private async Task CheckLink(BindableLinkDescription arg, CancellationToken cancelToken)
        {
            try
            {
                var res = await LinkChecker.CheckLink(arg.OriginalLink, cancelToken).ConfigureAwait(false);

                // access the binding on the UI thread
                await this.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                {
                    arg.HttpStatus = (int)res.HttpStatusCode;
                    arg.LinkAfterRedirect = res.NewLink;
                    arg.Result = res.Summary;
                    if (res.Summary == LinkCheckSummary.SimpleChange)
                    {
                        arg.SelectForUpdate = true;
                    }
                    else if (res.Summary == LinkCheckSummary.Error
                        || res.Summary == LinkCheckSummary.NotFound)
                    {
                        arg.SelectForInvalidMark = true;
                    }
                });
            }
            catch (Exception ex)
            {
                Logger.Log(LogLevel.Error, nameof(CheckLink), "Error checking link: " + arg?.OriginalLink, ex);
            }
        }

        private async void ClickLink(object sender, RoutedEventArgs e)
        {
            var btn = sender as HyperlinkButton;

            var url = btn?.Content as string;

            if (!string.IsNullOrEmpty(url))
            {
                await Launcher.LaunchUriAsync(new Uri(url));
            }
        }

        private async void RescanList(object sender, RoutedEventArgs e)
        {
            // reset just the timeouts
            foreach (var lnk in this.Links.Where(l => l.Result == LinkCheckSummary.Timeout))
            {
                lnk.Result = LinkCheckSummary.NotCheckedYet;
            }

            // and rescan (maybe the server is awake now?)
            await this.CheckAllLinks().ConfigureAwait(false);
        }

        private void UpdateSource(object sender, RoutedEventArgs e)
        {
            int count = 0;

            var html = CleanerSettings.Instance.HtmlSource;

            // process any checked link, but "invalid" beats "update".
            foreach (var lnk in this.Links.Where(l => l.SelectForUpdate || l.SelectForInvalidMark))
            {
                if (lnk.SelectForInvalidMark)
                {
                    MarkError(lnk);
                }
                else if (lnk.SelectForUpdate && !string.IsNullOrEmpty(lnk.LinkAfterRedirect))
                {
                    UpdateUrl(lnk);
                }

                lnk.SelectForInvalidMark = false;
                lnk.SelectForUpdate = false;
            }

            this.MessageLabel.Text = $"{count} link(s) updated.";
            CleanerSettings.Instance.HtmlSource = html;

            void MarkError(BindableLinkDescription link)
            {
                html = LinkChecker.MarkInvalid(html, link.OriginalLink);
                link.Result = LinkCheckSummary.Updated;
                count++;
            }

            void UpdateUrl(BindableLinkDescription link)
            {
                html = LinkChecker.UpdateHref(html, link.OriginalLink, link.LinkAfterRedirect);
                link.Result = LinkCheckSummary.Updated;
                count++;
            }
        }

        private void BackToMain(object sender, RoutedEventArgs e)
        {
            this.Frame.GoBack();
        }
    }
}
