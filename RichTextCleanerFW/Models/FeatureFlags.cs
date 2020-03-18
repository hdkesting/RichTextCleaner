namespace RichTextCleanerFW.Models
{
    internal static class FeatureFlags
    {
        public static bool CleanUrlQuery
#if DEBUG
            => true;
#else
            => false;
#endif
    }
}
