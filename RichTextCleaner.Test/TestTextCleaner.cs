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
        public void CleanParagraph_ShouldStayClean()
        {
            var doc = TextCleaner.CreateHtmlDocument("<p>Some paragraph</p>");
            TextCleaner.TrimParagraphs(doc);
            var html = TextCleaner.GetHtmlSource(doc);

            // NB "</p>" gets an added NewLine
            Assert.AreEqual("<p>Some paragraph</p>\r\n", html);
        }

        [TestMethod]
        public void DirtyParagraph_ShouldBeClean()
        {
            var doc = TextCleaner.CreateHtmlDocument(@"<p> Some paragraph
</p>");
            TextCleaner.TrimParagraphs(doc);
            var html = TextCleaner.GetHtmlSource(doc);

            Assert.AreEqual("<p>Some paragraph</p>\r\n", html);
        }

        [TestMethod]
        public void NoParagraph_ShouldStaySame()
        {
            var doc = TextCleaner.CreateHtmlDocument("<span> Some paragraph </span>");
            TextCleaner.TrimParagraphs(doc);
            var html = TextCleaner.GetHtmlSource(doc);

            Assert.AreEqual("<span> Some paragraph </span>", html);
        }


    }
#pragma warning restore CA1707 // Identifiers should not contain underscores
}
