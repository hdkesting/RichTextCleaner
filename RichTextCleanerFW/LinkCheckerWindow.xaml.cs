using RichTextCleaner.Common;
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
        public LinkCheckerWindow()
        {
            InitializeComponent();
        }

        public ObservableCollection<LinkDescription> Links { get; } = new ObservableCollection<LinkDescription>();

        internal Task CheckAllLinks()
        {
            this.LinkList.ItemsSource = this.Links;

            return Task.CompletedTask;
        }
    }
}
