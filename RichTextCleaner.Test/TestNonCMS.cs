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

            Assert.IsFalse(html.Contains("<script>"));
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

            Assert.IsFalse(html.Contains("<script>"));
            Assert.IsFalse(html.Contains("<noscript>"));
            Assert.IsTrue(html.Contains("<p>Text</p>"));
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

            Assert.IsTrue(html.Contains("<iframe>"));
        }
    }
#pragma warning restore CA1707 // Identifiers should not contain underscores
}
