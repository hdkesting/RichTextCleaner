using Microsoft.VisualStudio.TestTools.UnitTesting;
using RichTextCleaner.Common;

namespace RichTextCleaner.Test
{
#pragma warning disable CA1707 // Identifiers should not contain underscores
    [TestClass]
    public class TestNonCms
    {
        [TestMethod]
        public void RemoveNonCMSElements_ShouldRemoveScript()
        {
            string source = @"
<body>
    <script>alert('boo')</script>
    <p>Text</p>
</body>";

            var html = DocTester.ProcessSource(source, TextCleaner.RemoveNonCMSElements);

            Assert.IsFalse(html.Contains("<script>", System.StringComparison.Ordinal));
        }

        [TestMethod]
        public void RemoveNonCMSElements_ShouldRemoveNoscript()
        {
            string source = @"
<body>
    <script>alert('boo')</script>
    <noscript>please enable script</noscript>
    <p>Text</p>
</body>";

            var html = DocTester.ProcessSource(source, TextCleaner.RemoveNonCMSElements);

            Assert.IsFalse(html.Contains("<script>", System.StringComparison.Ordinal));
            Assert.IsFalse(html.Contains("<noscript>", System.StringComparison.Ordinal));
            Assert.IsTrue(html.Contains("<p>Text</p>", System.StringComparison.Ordinal));
        }

        [TestMethod]
        public void RemoveNonCMSElements_ShouldNotRemoveIframe()
        {
            string source = @"
<body>
    <iframe>some video content</iframe>
    <p>Text</p>
</body>";
            var html = DocTester.ProcessSource(source, TextCleaner.RemoveNonCMSElements);

            Assert.IsTrue(html.Contains("<iframe>", System.StringComparison.Ordinal));
        }
    }
#pragma warning restore CA1707 // Identifiers should not contain underscores
}
