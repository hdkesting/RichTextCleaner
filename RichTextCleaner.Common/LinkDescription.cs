using System;
using System.Collections.Generic;
using System.Text;

namespace RichTextCleaner.Common
{
    /// <summary>
    /// The description of a link.
    /// </summary>
    public class LinkDescription
    {
        public string LinkText { get; set; }

        public string OriginalLink { get; set; }

        public LinkCheckResult Result { get; set; }

        public string LinkAfterRedirect { get; set; }
    }
}
