using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RichTextCleaner.Common;

namespace RichTextCleaner.Test
{
#pragma warning disable CA1707 // Identifiers should not contain underscores
    [TestClass]
    public class TestTextCleaner
    {
        [TestMethod]
        public void RemoveNonCMSElements_ShouldRemoveScript()
        {
            var doc = TextCleaner.CreateHtmlDocument(@"
<body>
    <script>alert('boo')</script>
    <p>Text</p>
</body>");
            TextCleaner.RemoveNonCMSElements(doc);
            var html = TextCleaner.GetHtmlSource(doc);
            Assert.IsFalse(html.Contains("<script>"));
        }

        [TestMethod]
        public void RemoveNonCMSElements_ShouldRemoveNoscript()
        {
            var doc = TextCleaner.CreateHtmlDocument(@"
<body>
    <script>alert('boo')</script>
    <noscript>please enable script</noscript>
    <p>Text</p>
</body>");
            TextCleaner.RemoveNonCMSElements(doc);
            var html = TextCleaner.GetHtmlSource(doc);
            Assert.IsFalse(html.Contains("<script>"));
            Assert.IsFalse(html.Contains("<noscript>"));
            Assert.IsTrue(html.Contains("<p>Text</p>"));
        }

        [TestMethod]
        public void RemoveNonCMSElements_ShouldNotRemoveIframe()
        {
            var doc = TextCleaner.CreateHtmlDocument(@"
<body>
    <iframe>some video content</iframe>
    <p>Text</p>
</body>");
            TextCleaner.RemoveNonCMSElements(doc);
            var html = TextCleaner.GetHtmlSource(doc);
            Assert.IsTrue(html.Contains("<iframe>"));
        }

        [TestMethod]
        public void ClearStyling_ShouldRemoveAttributes()
        {
            var doc = TextCleaner.CreateHtmlDocument(@"
<p class=""something"">Text 1</p>
<p style=""background-color: black"">Text <span class=""super"">2</span></p>
<p onclick=""clickhandler"">Text 3</p>");
            TextCleaner.ClearStyling(doc.DocumentNode);

            var html = TextCleaner.GetHtmlSource(doc, false);
            Assert.AreEqual(@"
<p>Text 1</p>
<p>Text <span>2</span></p>
<p>Text 3</p>", html);

        }

        [TestMethod]
        public void CleanParagraph_ShouldStayClean()
        {
            var doc = TextCleaner.CreateHtmlDocument("<p>Some paragraph</p>");
            TextCleaner.TrimParagraphs(doc);
            var html = TextCleaner.GetHtmlSource(doc, false);

            Assert.AreEqual("<p>Some paragraph</p>", html);
        }

        [TestMethod]
        public void DirtyParagraph_ShouldBeClean()
        {
            var doc = TextCleaner.CreateHtmlDocument(@"<p> Some paragraph
</p>");
            TextCleaner.TrimParagraphs(doc);
            var html = TextCleaner.GetHtmlSource(doc, false);

            Assert.AreEqual("<p>Some paragraph</p>", html);
        }

        [TestMethod]
        public void NoParagraph_ShouldStaySame()
        {
            var doc = TextCleaner.CreateHtmlDocument("<span> Some paragraph </span>");
            TextCleaner.TrimParagraphs(doc);
            var html = TextCleaner.GetHtmlSource(doc);

            Assert.AreEqual("<span> Some paragraph </span>", html);
        }

        [TestMethod]
        public void TabIndex_ShouldBeRemoved()
        {
            var doc = TextCleaner.CreateHtmlDocument("<p> follow <a tabindex=\"0\" href=\"https://twitter.com/wkhealth\">@WKHealth</a> or <a tabindex=\"0\" href=\"https://twitter.com/Wolters_Kluwer\">@Wolters_Kluwer</a> on Twitter</p>");
            TextCleaner.RemoveAnchors(doc);
            var html = TextCleaner.GetHtmlSource(doc);

            Assert.IsFalse(html.Contains("tabindex"));
        }
    }
#pragma warning restore CA1707 // Identifiers should not contain underscores
}
