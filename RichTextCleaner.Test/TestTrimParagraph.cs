using Microsoft.VisualStudio.TestTools.UnitTesting;
using RichTextCleaner.Common;

namespace RichTextCleaner.Test
{
#pragma warning disable CA1707 // Identifiers should not contain underscores
    [TestClass]
    public class TestTrimParagraph
    {
        [TestMethod]
        public void CleanParagraph_ShouldStayClean()
        {
            string source = "<p>Some paragraph</p>";
            var html = DocTester.ProcessSource(source, TextCleaner.TrimParagraphs);

            Assert.AreEqual("<p>Some paragraph</p>", html);
        }

        [TestMethod]
        public void DirtyParagraph_ShouldBeClean()
        {
            string source = @"<p> Some paragraph
</p>";
            var html = DocTester.ProcessSource(source, TextCleaner.TrimParagraphs);

            Assert.AreEqual("<p>Some paragraph</p>", html);
        }

        [TestMethod]
        public void ParagraphWithLeadingBreaks_ShouldBeClean()
        {
            string source = @"<p><br/>
<br/>
Some paragraph
</p>";
            var html = DocTester.ProcessSource(source, TextCleaner.TrimParagraphs);

            Assert.AreEqual("<p>Some paragraph</p>", html);
        }

        [TestMethod]
        public void ParagraphWithTrailingBreaks_ShouldBeClean()
        {
            string source = @"<p>
Some paragraph<br/>
<br/>
</p>";
            var html = DocTester.ProcessSource(source, TextCleaner.TrimParagraphs);

            Assert.AreEqual("<p>Some paragraph</p>", html);
        }

        [TestMethod]
        public void NoParagraph_ShouldStaySame()
        {
            var source = "<span> Some paragraph </span>";
            var html = DocTester.ProcessSource(source, TextCleaner.TrimParagraphs);

            Assert.AreEqual(source, html);
        }
    }
#pragma warning restore CA1707 // Identifiers should not contain underscores
}
