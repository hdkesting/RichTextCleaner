using RichTextCleaner.Common.Support;
using RichTextCleanerFW.Models;
using System.Windows;

namespace RichTextCleanerFW
{
    /// <summary>
    /// Interaction logic for SettingsWindow.xaml
    /// </summary>
    public partial class SettingsWindow : Window
    {
        public SettingsWindow()
        {
            InitializeComponent();

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
        }

        private void SaveSettings(object sender, RoutedEventArgs e)
        {
            UpdateCleanerSettings();
            this.Close();
        }
    }
}
