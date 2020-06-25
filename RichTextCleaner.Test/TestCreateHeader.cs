using System;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using RichTextCleaner.Common;

namespace RichTextCleaner.Test
{
    [TestClass]
    public class TestCreateHeader
    {
#pragma warning disable CA1707 // Identifiers should not contain underscores
        [TestMethod]
        public void TestHeaderCreation_NoHeader()
        {
            string source = "<p>Some paragraph</p>";
            var html = DocTester.ProcessSource(source, TextCleaner.CreateHeaders);

            Assert.AreEqual("<p>Some paragraph</p>", html);
        }

        [TestMethod]
        public void TestHeaderCreation_SimpleBHeader()
        {
            string source = "<p><B>Some paragraph</B></p>";
            var html = DocTester.ProcessSource(source, TextCleaner.CreateHeaders);

            Assert.AreEqual("<h2>Some paragraph</h2>", html);
        }

        [TestMethod]
        public void TestHeaderCreation_SimpleStrongHeader()
        {
            string source = "<p><strong>Some paragraph</strong></p>";
            var html = DocTester.ProcessSource(source, TextCleaner.CreateHeaders);

            Assert.AreEqual("<h2>Some paragraph</h2>", html);
        }

        [TestMethod]
        public void TestHeaderCreation_SimpleStrongHeaderWithEm()
        {
            string source = "<p><strong>Some <em>paragraph</em></strong></p>";
            var html = DocTester.ProcessSource(source, TextCleaner.CreateHeaders);

            Assert.AreEqual("<h2>Some <em>paragraph</em></h2>", html);
        }

        [TestMethod]
        public void TestHeaderCreation_StrongHeaderWithWhitespace()
        {
            string source = "<p>&nbsp;<strong>Some paragraph</strong> </p>";
            var html = DocTester.ProcessSource(source, TextCleaner.CreateHeaders);

            Assert.AreEqual("<h2>Some paragraph</h2>", html);
        }

        [TestMethod]
        public void TestHeaderCreation_DoubleBHeader()
        {
            string source = "<p><B>Some</B><strong> paragraph</strong></p>";
            var html = DocTester.ProcessSource(source, TextCleaner.CreateHeaders);

            Assert.AreEqual("<h2>Some paragraph</h2>", html);
        }

        [TestMethod]
        public void TestHeaderCreation_DoubleBHeaderWithSpace()
        {
            string source = "<p><B>Some</B>&nbsp;<strong>paragraph</strong></p>";
            var html = DocTester.ProcessSource(source, TextCleaner.CreateHeaders);

            Assert.AreEqual("<h2>Some paragraph</h2>", html);
        }

#pragma warning restore CA1707 // Identifiers should not contain underscores
    }
}
