using RichTextCleaner.Common.Support;
using System;
using System.IO;
using Windows.Storage;

namespace RichTextCleanerUwp.Models
{

    /// <summary>
    /// Strongly typed settings.
    /// </summary>
    /// <remarks>
    /// Settings are stored in C:\Users\{user}\AppData\Local\Hans_Kesting\RichTextCleaner.exe...\{version}\user.config
    /// </remarks>
    public class CleanerSettings: ICleanerSettings
    {
        private static readonly Lazy<CleanerSettings> lazyInstance = new Lazy<CleanerSettings>(() => new CleanerSettings());
        private readonly Windows.Storage.ApplicationDataContainer localSettings;

        private const string RemoveBoldKey = "RemoveBold";
        private const string RemoveItalicKey = "RemoveItalic";
        private const string RemoveUnderlineKey = "RemoveUnderline";
        private const string AddTargetBlankKey = "AddTargetBlank";
        private const string QuoteProcessKey = "QuoteProcess";
        private const string QueryCleanLevelKey = "QueryCleanLevel";
        private const string CreateLinkFromTextKey = "CreateLinkFromText";

        private string htmlSource;
        private readonly string htmlSourcePath;

        /// <summary>
        /// Prevents a default instance of the <see cref="CleanerSettings"/> class from being created.
        /// </summary>
        private CleanerSettings()
        {
            localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;
            EnsureDefaultValues(localSettings);

            // The name of each setting can be 255 characters in length at most.
            // Each setting can be up to 8K bytes in size 
            // and each composite setting can be up to 64K bytes in size.
            // https://docs.microsoft.com/en-us/uwp/api/windows.storage.applicationdata.localsettings?view=winrt-18362#remarks

            this.htmlSourcePath = Path.Combine(ApplicationData.Current.LocalFolder.Path, "source.html");
        }


        public static CleanerSettings Instance => lazyInstance.Value;

        /// <summary>
        /// Gets or sets a value indicating whether to remove bold elements.
        /// </summary>
        /// <value>
        ///   <c>true</c> if remove bold; otherwise, <c>false</c>.
        /// </value>
        public bool RemoveBold
        {
            get
            {
                return (bool)this.localSettings.Values[RemoveBoldKey];
            }
            set
            {
                this.localSettings.Values[RemoveBoldKey] = value;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether to remove italic elements.
        /// </summary>
        /// <value>
        ///   <c>true</c> if remove italic; otherwise, <c>false</c>.
        /// </value>
        public bool RemoveItalic
        {
            get
            {
                return (bool)this.localSettings.Values[RemoveItalicKey];
            }
            set
            {
                this.localSettings.Values[RemoveItalicKey] = value;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether to remove underline elements.
        /// </summary>
        /// <value>
        ///   <c>true</c> if remove underline; otherwise, <c>false</c>.
        /// </value>
        public bool RemoveUnderline
        {
            get
            {
                return (bool)this.localSettings.Values[RemoveUnderlineKey];
            }
            set
            {
                this.localSettings.Values[RemoveUnderlineKey] = value;
            }
        }

        /// <summary>
        /// Gets all the markup to remove.
        /// </summary>
        /// <value>
        /// The markup to remove.
        /// </value>
        public StyleElements MarkupToRemove =>
            (RemoveBold ? StyleElements.Bold : StyleElements.None) |
            (RemoveItalic ? StyleElements.Italic : StyleElements.None) |
            (RemoveUnderline ? StyleElements.Underline : StyleElements.None);

        /// <summary>
        /// Gets or sets a value indicating whether to add target=_blank to link elements.
        /// </summary>
        /// <value>
        ///   <c>true</c> if add target=_blank; otherwise, <c>false</c>.
        /// </value>
        public bool AddTargetBlank
        {
            get
            {
                return (bool)this.localSettings.Values[AddTargetBlankKey];
            }
            set
            {
                this.localSettings.Values[AddTargetBlankKey] = value;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating how to process quotes.
        /// </summary>
        /// <seealso cref="QuoteProcessing"/>
        /// <value>
        /// The quote process.
        /// </value>
        public QuoteProcessing QuoteProcess
        {
            get
            {
                return (QuoteProcessing)(int)this.localSettings.Values[QuoteProcessKey];
            }
            set
            {
                this.localSettings.Values[QuoteProcessKey] = (int)value;
            }
        }

        /// <summary>
        /// Gets or sets the query clean level.
        /// </summary>
        /// <value>
        /// The query clean level.
        /// </value>
        public LinkQueryCleanLevel QueryCleanLevel => LinkQueryCleanLevel.RemoveQuery;

        /// <summary>
        /// Gets a value indicating whether to create links from link-like texts.
        /// </summary>
        /// <value>
        /// <c>true</c> if create link from text; otherwise, <c>false</c>.
        /// </value>
        public bool CreateLinkFromText
        {
            get
            {
                return (bool)this.localSettings.Values[CreateLinkFromTextKey];
            }
            set
            {
                this.localSettings.Values[CreateLinkFromTextKey] = value;
            }
        }

        /// <summary>Gets a value indicating whether to add "rel=noopener" to external links.</summary>
        /// <value>
        ///   <c>true</c> if "rel=noopener" should be added; otherwise, <c>false</c>.</value>
        public bool AddRelNoOpener => true;

        /// <summary>
        /// Gets or sets the HTML source currently being processed.
        /// </summary>
        /// <remarks>
        /// Store in separate file because the local settings are limited in size.
        /// </remarks>
        /// <value>
        /// The HTML source.
        /// </value>
        public string HtmlSource
        {
            get
            {
                if (this.htmlSource is null)
                {
                    if (File.Exists(this.htmlSourcePath))
                    {
                        this.htmlSource = File.ReadAllText(this.htmlSourcePath);
                    }
                    else
                    {
                        // signal that I've tried the file already
                        this.htmlSource = string.Empty;
                    }
                }

                return this.htmlSource;
            }

            set
            {
                if (string.IsNullOrWhiteSpace(value))
                {
                    this.htmlSource = string.Empty;
                    if (File.Exists(this.htmlSourcePath))
                    {
                        File.Delete(this.htmlSourcePath);
                    }
                }
                else
                {
                    File.WriteAllText(this.htmlSourcePath, value);
                    this.htmlSource = value;
                }
            }
        }


        /// <summary>
        /// Makes sure that at least the default values exist.
        /// </summary>
        /// <param name="localSettings">The local settings.</param>
        private static void EnsureDefaultValues(ApplicationDataContainer localSettings)
        {
            EnsureSetting(RemoveBoldKey, true);
            EnsureSetting(RemoveItalicKey, false);
            EnsureSetting(RemoveUnderlineKey, true);
            EnsureSetting(AddTargetBlankKey, true);
            EnsureSetting(QuoteProcessKey, (int)QuoteProcessing.ChangeToSmartQuotes);
            EnsureSetting(QueryCleanLevelKey, (int)LinkQueryCleanLevel.RemoveQuery);
            EnsureSetting(CreateLinkFromTextKey, true);

            void EnsureSetting(string key, object value)
            {
                if (!localSettings.Values.ContainsKey(key))
                {
                    localSettings.Values[key] = value;
                }
            }
        }
    }
}
