using RichTextCleaner.Common.Support;
using RichTextCleanerUwp.Models;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace RichTextCleanerUwp.Forms
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class SettingsPage : Page
    {
        public SettingsPage()
        {
            this.InitializeComponent();

            var appView = Windows.UI.ViewManagement.ApplicationView.GetForCurrentView();
            appView.Title = "Settings";

            this.ClearBoldMarkup.IsChecked = CleanerSettings.Instance.RemoveBold;
            this.ClearItalicMarkup.IsChecked = CleanerSettings.Instance.RemoveItalic;
            this.ClearUnderlineMarkup.IsChecked = CleanerSettings.Instance.RemoveUnderline;
            this.AddBlankTarget.IsChecked = CleanerSettings.Instance.AddTargetBlank;
            this.ChangeToFancyQuotes.IsChecked = CleanerSettings.Instance.QuoteProcess != QuoteProcessing.NoChange;
        }

        private void UpdateCleanerSettings()
        {
            var cs = CleanerSettings.Instance;
            cs.RemoveBold = ClearBoldMarkup.IsChecked.GetValueOrDefault();

            cs.RemoveItalic = ClearItalicMarkup.IsChecked.GetValueOrDefault();

            cs.RemoveUnderline = ClearUnderlineMarkup.IsChecked.GetValueOrDefault();

            cs.AddTargetBlank = AddBlankTarget.IsChecked.GetValueOrDefault();

            cs.QuoteProcess = this.ChangeToFancyQuotes.IsChecked.GetValueOrDefault()
                ? QuoteProcessing.ChangeToSmartQuotes
                : QuoteProcessing.NoChange;

            // no separate "save" command required
        }

        private void SaveSettings(object sender, RoutedEventArgs e)
        {
            UpdateCleanerSettings();
            this.Frame.GoBack();
        }

        private void BackWithoutSaving(object sender, RoutedEventArgs e)
        {
            this.Frame.GoBack();
        }
    }
}
