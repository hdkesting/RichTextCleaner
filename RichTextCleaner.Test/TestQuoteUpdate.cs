using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RichTextCleaner.Common;
using RichTextCleaner.Common.Support;

namespace RichTextCleaner.Test
{
#pragma warning disable CA1707 // Identifiers should not contain underscores
    [TestClass]
    public class TestQuoteUpdate
    {
        [TestMethod]
        public void NoChange_ShouldNotChange()
        {
            var source = "<a target=\"_blank\">&ldquo;some remark&rdquo; said the so-called \"chief\"</a>";

            var html = DocTester.ProcessSource(source, doc => TextCleaner.UpdateQuotes(doc, QuoteProcessing.NoChange));

            Assert.AreEqual(source, html);
        }

        [TestMethod]
        public void ToSimpleQuotes_ShouldChangeSmartQuotes()
        {
            var source = "<a target=\"_blank\">&ldquo;some remark&rdquo; said the so-called \"chief.\"</a>";

            var html = DocTester.ProcessSource(source, doc => TextCleaner.UpdateQuotes(doc, QuoteProcessing.ChangeToSimpleQuotes));

            Assert.AreEqual("<a target=\"_blank\">\"some remark\" said the so-called \"chief.\"</a>", html);
        }

        [TestMethod]
        public void ToSmartQuotes_ShouldChangeSimpleQuotes()
        {
            var source = "<a target=\"_blank\">&ldquo;some remark&rdquo; said the so-called \"chief.\"</a>";

            var html = DocTester.ProcessSource(source, doc => TextCleaner.UpdateQuotes(doc, QuoteProcessing.ChangeToSmartQuotes));

            Assert.AreEqual("<a target=\"_blank\">&ldquo;some remark&rdquo; said the so-called &ldquo;chief.&rdquo;</a>", html);
        }

        [TestMethod]
        public void QuotesInBrackets_ShouldBeConverted()
        {
            var source = "<p>Something ('me') something</p>";
            var html = DocTester.ProcessSource(source, doc => TextCleaner.UpdateQuotes(doc, QuoteProcessing.ChangeToSmartQuotes));

            Assert.AreEqual("<p>Something (&lsquo;me&rsquo;) something</p>", html);
        }

    }
#pragma warning restore CA1707 // Identifiers should not contain underscores
}
