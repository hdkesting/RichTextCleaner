using RichTextCleaner.Common;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace RichTextCleanerFW.Models
{
    public class BindableLinkDescription : INotifyPropertyChanged
    {
        private LinkCheckResult result;
        private string linkAfterRedirect;

        public BindableLinkDescription(LinkDescription original)
        {
            if (original is null)
            {
                throw new ArgumentNullException(nameof(original));
            }

            this.LinkText = original.LinkText;
            this.OriginalLink = original.OriginalLink;
            this.Result = original.Result;
            this.LinkAfterRedirect = original.LinkAfterRedirect ?? original.OriginalLink;
        }

        public string LinkText { get; set; }

        public string OriginalLink { get; set; }

        public LinkCheckResult Result
        {
            get { return this.result; }
            set
            {
                if (value != this.result)
                {
                    this.result = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public string LinkAfterRedirect
        {
            get { return this.linkAfterRedirect; }
            set { 
                if (value != this.linkAfterRedirect)
                {
                    this.linkAfterRedirect = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged([CallerMemberName]string propname = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propname));
        }
    }
}
