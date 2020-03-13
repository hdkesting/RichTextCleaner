using RichTextCleaner.Common.Support;

namespace RichTextCleanerFW.Models
{

    /// <summary>
    /// Strongly typed settings.
    /// </summary>
    /// <remarks>
    /// Settings are stored in C:\Users\{user}\AppData\Local\Hans_Kesting\RichTextCleaner.exe...\{version}\user.config
    /// </remarks>
    public static class CleanerSettings
    {
        /// <summary>
        /// Gets or sets a value indicating whether to remove bold elements.
        /// </summary>
        /// <value>
        ///   <c>true</c> if remove bold; otherwise, <c>false</c>.
        /// </value>
        public static bool RemoveBold
        {
            get { return Properties.Settings.Default.RemoveBold; }
            set
            {
                Properties.Settings.Default.RemoveBold = value;
                Properties.Settings.Default.Save();
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether to remove italic elements.
        /// </summary>
        /// <value>
        ///   <c>true</c> if remove italic; otherwise, <c>false</c>.
        /// </value>
        public static bool RemoveItalic
        {
            get { return Properties.Settings.Default.RemoveItalic; }
            set
            {
                Properties.Settings.Default.RemoveItalic = value;
                Properties.Settings.Default.Save();
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether to remove underline elements.
        /// </summary>
        /// <value>
        ///   <c>true</c> if remove underline; otherwise, <c>false</c>.
        /// </value>
        public static bool RemoveUnderline
        {
            get { return Properties.Settings.Default.RemoveUnderline; }
            set
            {
                Properties.Settings.Default.RemoveUnderline = value;
                Properties.Settings.Default.Save();
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether to add target=_blank to link elements.
        /// </summary>
        /// <value>
        ///   <c>true</c> if add target=_blank; otherwise, <c>false</c>.
        /// </value>
        public static bool AddTargetBlank
        {
            get { return Properties.Settings.Default.AddTargetBlank; }
            set
            {
                Properties.Settings.Default.AddTargetBlank = value;
                Properties.Settings.Default.Save();
            }
        }

        /// <summary>
        /// Gets or sets a value indicating how to process quotes.
        /// </summary>
        /// <seealso cref="QuoteProcessing"/>
        /// <value>
        /// The quote process.
        /// </value>
        public static QuoteProcessing QuoteProcess
        {
            get { return (QuoteProcessing)Properties.Settings.Default.QuoteProcess; }
            set
            {
                Properties.Settings.Default.QuoteProcess = (int)value;
                Properties.Settings.Default.Save();
            }
        }

        /// <summary>
        /// Gets or sets the query clean level.
        /// </summary>
        /// <value>
        /// The query clean level.
        /// </value>
        public static LinkQueryCleanLevel QueryCleanLevel
        {
            get { return (LinkQueryCleanLevel)Properties.Settings.Default.QueryCleanLevel; }
            set
            {
                Properties.Settings.Default.QueryCleanLevel = (int)value;
                Properties.Settings.Default.Save();
            }
        }
    }
}
