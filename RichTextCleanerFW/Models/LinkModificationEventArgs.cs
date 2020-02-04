using System;

namespace RichTextCleanerFW.Models
{
    public class LinkModificationEventArgs: EventArgs
    {
        public LinkModificationEventArgs(string linkHref, LinkModification modification, string newHref = null)
        {
            this.LinkHref = linkHref;
            this.Modification = modification;
            this.NewHref = newHref;
        }

        public string LinkHref { get; }

        public LinkModification Modification { get; }
        public string NewHref { get; }
    }

    public enum LinkModification
    {
        UpdateSchema,

        MarkInvalid
    }
}
