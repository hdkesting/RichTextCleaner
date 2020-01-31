using RichTextCleaner.Common;
using RichTextCleanerFW.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

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
            this.MessageLabel.Text = "Checking all links";
            //// this.LinkList.ItemsSource = this.Links;

            var tasks = this.Links.Select(CheckLink).ToList();

            await Task.WhenAll(tasks).ConfigureAwait(true);

            this.MessageLabel.Text = "Done checking";
        }

        private readonly Random rng = new Random();

        private async Task CheckLink(BindableLinkDescription arg)
        {
            await Task.Delay(rng.Next(1000, 2000)).ConfigureAwait(false);
            arg.Result = LinkCheckResult.NotFound;
        }
    }
}
