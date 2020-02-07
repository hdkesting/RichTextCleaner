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
                this.MessageLabel.Text = $"Checking all links: {tasks.Count(t => t.IsCompleted || t.IsFaulted || t.IsCanceled)}/{tasks.Count}";
                await Task.Delay(TimeSpan.FromSeconds(1)).ConfigureAwait(true);
            }
            while (tasks.Any(t => !t.IsCompleted && !t.IsFaulted && !t.IsCanceled));

            this.MessageLabel.Text = "Done checking";
        }

        private static async Task CheckLink(BindableLinkDescription arg)
        {
            var res = await LinkChecker.CheckLink(arg.OriginalLink).ConfigureAwait(false);
            arg.HttpStatus = (int)res.HttpStatusCode;
            arg.LinkAfterRedirect = res.NewLink;
            arg.Result = res.Summary;
            if (res.Summary == LinkCheckSummary.SimpleChange 
                || res.Summary == LinkCheckSummary.Error 
                || res.Summary == LinkCheckSummary.NotFound)
            {
                arg.SelectForUpdate = true;
            }
        }

        private void ClickLink(object sender, RoutedEventArgs e)
        {
            var btn = sender as Button;

            if (btn == null)
            {
                return;
            }

            var url = btn.Content as string;

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
            foreach (var lnk in this.Links.Where(l => l.SelectForUpdate))
            {
                switch (lnk.Result)
                {
                    case LinkCheckSummary.NotFound:
                    case LinkCheckSummary.Error:
                        this.LinkToProcess?.Invoke(this, new LinkModificationEventArgs(lnk.OriginalLink, LinkModification.MarkInvalid));
                        lnk.Result = LinkCheckSummary.Updated;
                        lnk.SelectForUpdate = false;
                        count++;
                        break;

                    case LinkCheckSummary.Redirected:
                    case LinkCheckSummary.SimpleChange:
                        this.LinkToProcess?.Invoke(this, new LinkModificationEventArgs(lnk.OriginalLink, LinkModification.UpdateSchema, lnk.LinkAfterRedirect));
                        lnk.Result = LinkCheckSummary.Updated;
                        lnk.SelectForUpdate = false;
                        count++;
                        break;
                }
            }

            this.MessageLabel.Text = $"{count} link(s) updated.";
        }
    }
}
