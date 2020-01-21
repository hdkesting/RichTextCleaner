namespace RichTextCleaner.Common
{
    /// <summary>
    /// Select how to process quotes in texts.
    /// </summary>
    public enum QuoteProcessing
    {
        /// <summary>
        /// Do not change any quotes.
        /// </summary>
        NoChange,

        /// <summary>
        /// Change smart quotes to simple quotes.
        /// </summary>
        ChangeToSimpleQuotes,

        /// <summary>
        /// Change simple quotes to smart quotes.
        /// </summary>
        ChangeToSmartQuotes
    }
}
