using RichTextCleaner.Common;
using RichTextCleanerFW.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace RichTextCleanerFW
{
    /// <summary>
    /// Interaction logic for LinkCheckerWindow.xaml
    /// </summary>
    public partial class LinkCheckerWindow : Window
    {
        public static readonly DependencyProperty LinksProperty = DependencyProperty.Register(nameof(Links), typeof(List<BindableLinkDescription>), typeof(LinkCheckerWindow), new PropertyMetadata(new List<BindableLinkDescription>()));

        public LinkCheckerWindow()
        {
            InitializeComponent();

            this.DataContext = this;
        }

        public List<BindableLinkDescription> Links
        {
            get { return (List<BindableLinkDescription>)this.GetValue(LinksProperty); }
            private set { this.SetValue(LinksProperty, value); }
        }

        internal async Task CheckAllLinks()
        {
            var tasks = new List<Task>();
            
            foreach (var lnk in this.Links)
            {
                await Task.Delay(250).ConfigureAwait(true);
                tasks.Add(CheckLink(lnk));
            }

            do
            {
                this.MessageLabel.Text = $"Checking all links: {tasks.Count(t => !t.IsCompleted && !t.IsFaulted && !t.IsCanceled)}/{tasks.Count}";
                await Task.Delay(TimeSpan.FromSeconds(1)).ConfigureAwait(true);
            }
            while (tasks.Any(t => !t.IsCompleted && !t.IsFaulted && !t.IsCanceled));

            this.MessageLabel.Text = "Done checking";
        }

        private static async Task CheckLink(BindableLinkDescription arg)
        {
            (arg.Result, arg.LinkAfterRedirect) = await LinkChecker.CheckLink(arg.OriginalLink).ConfigureAwait(false);
        }
    }
}
