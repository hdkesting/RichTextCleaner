using RichTextCleaner.Common;
using RichTextCleaner.Common.Support;
using RichTextCleanerFW.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace RichTextCleanerFW
{
    /// <summary>
    /// Interaction logic for LinkCheckerWindow.xaml
    /// </summary>
    public partial class LinkCheckerWindow : Window
    {
        public static readonly DependencyProperty LinksProperty = DependencyProperty.Register(nameof(Links), typeof(ObservableCollection<BindableLinkDescription>), typeof(LinkCheckerWindow), new PropertyMetadata(new ObservableCollection<BindableLinkDescription>()));

        public LinkCheckerWindow()
        {
            InitializeComponent();

            this.DataContext = this;
        }

        public event EventHandler<LinkModificationEventArgs> LinkToProcess;

        public ObservableCollection<BindableLinkDescription> Links
        {
            get { return (ObservableCollection<BindableLinkDescription>)this.GetValue(LinksProperty); }
            private set { this.SetValue(LinksProperty, value); }
        }

        internal async Task CheckAllLinks()
        {
            var tasks = new List<Task>();
            
            // gradually start checking all that still need (re-)checking
            foreach (var lnk in this.Links.Where(l => l.Result == LinkCheckSummary.NotCheckedYet))
            {
                await Task.Delay(250).ConfigureAwait(true);
                tasks.Add(CheckLink(lnk));
            }

            do
            {
                this.MessageLabel.Text = $"Checking all links: {tasks.Count(TaskIsDone)} done out of {tasks.Count}";
                await Task.Delay(TimeSpan.FromSeconds(1)).ConfigureAwait(true);
            }
            while (tasks.Any(t => !TaskIsDone(t)));

            this.MessageLabel.Text = "Done checking";

            bool TaskIsDone(Task t) => t.IsCompleted || t.IsFaulted || t.IsCanceled;
        }

        private static async Task CheckLink(BindableLinkDescription arg)
        {
            var res = await LinkChecker.CheckLink(arg.OriginalLink).ConfigureAwait(false);
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
        }

        private void ClickLink(object sender, RoutedEventArgs e)
        {
            var btn = sender as Button;

            var url = btn?.Content as string;

            if (!string.IsNullOrEmpty(url))
            {
                Process.Start(url);
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

            void MarkError(BindableLinkDescription link)
            {
                this.LinkToProcess?.Invoke(this, new LinkModificationEventArgs(link.OriginalLink, LinkModification.MarkInvalid));
                link.Result = LinkCheckSummary.Updated;
                count++;
            }

            void UpdateUrl(BindableLinkDescription link)
            {
                this.LinkToProcess?.Invoke(this, new LinkModificationEventArgs(link.OriginalLink, LinkModification.UpdateSchema, link.LinkAfterRedirect));
                link.Result = LinkCheckSummary.Updated;
                count++;
            }
        }
    }
}
