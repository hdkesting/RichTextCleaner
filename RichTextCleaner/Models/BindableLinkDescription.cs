using RichTextCleaner.Common.Support;
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace RichTextCleaner.Models
{
    public class BindableLinkDescription : INotifyPropertyChanged
    {
        private LinkCheckSummary result;
        private string? linkAfterRedirect;
        private int httpStatus;
        private bool selectForUpdate;
        private bool selectForInvalidMark;

        public BindableLinkDescription(LinkDescription original)
        {
            if (original is null)
            {
                throw new ArgumentNullException(nameof(original));
            }

            this.LinkText = original.LinkText;
            this.OriginalLink = original.OriginalLink;
            this.Result = original.Result;
            this.LinkAfterRedirect = original.LinkAfterRedirect;
        }

        public string LinkText { get; set; }

        public string OriginalLink { get; set; }

        public int HttpStatus
        {
            get { return this.httpStatus; }
            set { this.SetValue(ref this.httpStatus, value); }
        }

        public LinkCheckSummary Result
        {
            get { return this.result; }
            set
            {
                if (value != this.result)
                {
                    this.result = value;
                    this.NotifyPropertyChanged();
                }
            }
        }

        public string? LinkAfterRedirect
        {
            get { return this.linkAfterRedirect; }
            set { this.SetValue(ref this.linkAfterRedirect, value); }
        }

        public bool SelectForUpdate
        {
            get { return this.selectForUpdate; }
            set { this.SetValue(ref this.selectForUpdate, value); }
        }

        public bool SelectForInvalidMark
        {
            get { return this.selectForInvalidMark; }
            set { this.SetValue(ref this.selectForInvalidMark, value); }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        private void SetValue<T>(ref T propertyValue, T newValue, [CallerMemberName]string? propname = null)
            where T : IEquatable<T>
        {
            if (propertyValue == null)
            {
                if (newValue != null)
                {
                    propertyValue = newValue;
                    this.NotifyPropertyChanged(propname);
                }
            }
            else if (!propertyValue.Equals(newValue))
            {
                propertyValue = newValue;
                this.NotifyPropertyChanged(propname);
            }
        }

        private void NotifyPropertyChanged([CallerMemberName]string? propname = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propname));
        }
    }
}
