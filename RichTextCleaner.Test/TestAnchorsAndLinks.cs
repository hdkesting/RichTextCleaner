using Microsoft.VisualStudio.TestTools.UnitTesting;
using RichTextCleaner.Common;

namespace RichTextCleaner.Test
{
#pragma warning disable CA1707 // Identifiers should not contain underscores
    [TestClass]
    public class TestAnchorsAndLinks
    {
        [TestMethod]
        public void TabIndex_ShouldBeRemoved()
        {
            var source = "<p> follow <a>@WKHealth</a> or <a name='target'>@Wolters_Kluwer</a> on Twitter</p>";

            // removes an A without HREF
            var html = DocTester.ProcessSource(source, TextCleaner.RemoveAnchors);

            Assert.IsFalse(html.Contains("<a"));
        }

        [TestMethod]
        public void ConsecutiveLinks_ShouldCombine()
        {
            var source = "<p><a href=\"http://www.example.com/investment-compliance/solutions/gainskeeper.aspx\">GainsKeeper</a><a href=\"http://www.example.com/investment-compliance/solutions/gainskeeper.aspx\"><sup>&reg;</sup></a></p>";

            var html = DocTester.ProcessSource(source, TextCleaner.CombineLinks);
            // SUP is removed from around (R)
            Assert.AreEqual("<p><a href=\"http://www.example.com/investment-compliance/solutions/gainskeeper.aspx\">GainsKeeper&reg;</a></p>", html);
        }

        [TestMethod]
        public void NonConsecutiveLinks_ShouldNotCombine()
        {
            var source = "<a href=\"nu.nl\">x</a><a href=\"http://www.example.com/investment-compliance/solutions/gainskeeper.aspx\">GainsKeeper</a><a href=\"http://www.example.com/investment-compliance/solutions/gainskeeper.aspx\"><sup>&reg;</sup></a>";

            var html = DocTester.ProcessSource(source, TextCleaner.CombineLinks);

            // SUP is removed from around &reg;
            Assert.AreEqual("<a href=\"nu.nl\">x</a><a href=\"http://www.example.com/investment-compliance/solutions/gainskeeper.aspx\">GainsKeeper&reg;</a>", html);
        }

        [TestMethod]
        public void MultipleLinks_ShouldCombine()
        {
            var source = "<a href=\"a.yy\">a</a><a href=\"a.yy\">b</a><a href=\"a.yy\">c</a><a href=\"a.yy\">d</a><a href=\"b.yy\">e</a>";

            var html = DocTester.ProcessSource(source, TextCleaner.CombineLinks);

            Assert.AreEqual("<a href=\"a.yy\">abcd</a><a href=\"b.yy\">e</a>", html);
        }

        [TestMethod]
        public void MultipleLinks_ShouldCombine2()
        {
            var source = "<a href=\"a.yy\">a</a><a href=\"a.yy\">b</a><a href=\"a.yy\">c</a><a href=\"a.yy\">d</a><a href=\"b.yy\">e</a> <a href=\"b.yy\">f</a>";

            var html = DocTester.ProcessSource(source, TextCleaner.CombineLinks);

            Assert.AreEqual("<a href=\"a.yy\">abcd</a><a href=\"b.yy\">e</a> <a href=\"b.yy\">f</a>", html);
        }

        [TestMethod]
        public void NonConsecutiveLink_ShouldNotCombine()
        {
            var source = "<a href=\"a.yy\">A</a> <a href=\"a.yy\">B</a>";

            var html = DocTester.ProcessSource(source, TextCleaner.CombineLinks);

            Assert.AreEqual(source, html);
        }

        [TestMethod]
        public void LinkAroundSpace_ShouldBeRemoved()
        {
            var source = "<span><a href=\"www.example.com\"> </a></span>";

            var html = DocTester.ProcessSource(source, TextCleaner.RemoveEmptyLinks);

            Assert.AreEqual("<span> </span>", html);
        }

        [TestMethod]
        public void LinkWithLeadingSpaces_ShouldBeCleaned()
        {
            var source = "<span>bla<a href=\"www.example.com\">, bla</a></span>";

            var html = DocTester.ProcessSource(source, TextCleaner.RemoveLeadingAndTrailingSpacesFromLinks);

            Assert.AreEqual("<span>bla, <a href=\"www.example.com\">bla</a></span>", html);
        }

        [TestMethod]
        public void LinkWithTrailingSpaces_ShouldBeCleaned()
        {
            var source = "<span>bla<a href=\"www.example.com\"> bla, </a>bla</span>";

            var html = DocTester.ProcessSource(source, TextCleaner.RemoveLeadingAndTrailingSpacesFromLinks);

            Assert.AreEqual("<span>bla <a href=\"www.example.com\">bla</a>, bla</span>", html);
        }

        [TestMethod]
        public void NestedLinkWithTrailingSpaces_ShouldBeCleaned()
        {
            var source = "<p style=\"\">[<a href=\"http://www.example.com/a\" style=\"\">Some example</a>] bla bla [<a href=\"http://www.example.com/b\" style=\"\">name® bla].</a></p>";

            var html = DocTester.ProcessSource(source, TextCleaner.RemoveLeadingAndTrailingSpacesFromLinks);

            // actual ® is replaced by &reg;
            Assert.AreEqual("<p style=\"\">[<a href=\"http://www.example.com/a\" style=\"\">Some example</a>] bla bla [<a href=\"http://www.example.com/b\" style=\"\">name&reg; bla]</a>.</p>", html);

        }

        [TestMethod]
        public void LinksToRemote_ShouldGetTarget()
        {
            var source = "<a href=\"https://www.example.com\">link</a>";

            var html = DocTester.ProcessSource(source, doc => TextCleaner.AddBlankLinkTargets(doc, false));

            Assert.AreEqual("<a href=\"https://www.example.com\" target=\"_blank\">link</a>", html);
        }

        [TestMethod]
        public void LinksToRemote_ShouldGetTargetAndNoOpener()
        {
            var source = "<a href=\"https://www.example.com\">link</a>";

            var html = DocTester.ProcessSource(source, doc => TextCleaner.AddBlankLinkTargets(doc, true));

            Assert.AreEqual("<a href=\"https://www.example.com\" target=\"_blank\" rel=\"noopener\">link</a>", html);
        }

        [TestMethod]
        public void LinksToRemoteWithRel_ShouldGetTargetAndNoOpener()
        {
            var source = "<a href=\"https://www.example.com\" rel=\"noopener\">link</a>";

            var html = DocTester.ProcessSource(source, doc => TextCleaner.AddBlankLinkTargets(doc, true));

            Assert.AreEqual("<a href=\"https://www.example.com\" rel=\"noopener\" target=\"_blank\">link</a>", html);
        }

        [TestMethod]
        public void LinksToRemoteWithRel2_ShouldGetTargetAndNoOpener()
        {
            var source = "<a href=\"https://www.example.com\" rel=\"noreferrer\">link</a>";

            var html = DocTester.ProcessSource(source, doc => TextCleaner.AddBlankLinkTargets(doc, true));

            Assert.AreEqual("<a href=\"https://www.example.com\" rel=\"noreferrer noopener\" target=\"_blank\">link</a>", html);
        }

        [TestMethod]
        public void LinksToRemoteWithTarget_ShouldNotChangeTarget()
        {
            var source = "<a href=\"https://www.example.com\" target=\"_self\">link</a>";

            var html = DocTester.ProcessSource(source, doc => TextCleaner.AddBlankLinkTargets(doc, false));

            Assert.AreEqual("<a href=\"https://www.example.com\" target=\"_self\">link</a>", html);
        }

        [TestMethod]
        public void LinksToRemoteWithTarget_ShouldNotChangeTargetButAddOpener()
        {
            var source = "<a href=\"https://www.example.com\" target=\"_self\">link</a>";

            var html = DocTester.ProcessSource(source, doc => TextCleaner.AddBlankLinkTargets(doc, true));

            Assert.AreEqual("<a href=\"https://www.example.com\" target=\"_self\" rel=\"noopener\">link</a>", html);
        }

        [TestMethod]
        public void LinksToLocal_ShouldNotGetTarget()
        {
            var source = "<a href=\"/default.html\">link</a>";

            var html = DocTester.ProcessSource(source, doc => TextCleaner.AddBlankLinkTargets(doc, false));

            Assert.AreEqual("<a href=\"/default.html\">link</a>", html);
        }

        [TestMethod]
        public void LinksToLocal_ShouldNotGetTargetOrOpener()
        {
            var source = "<a href=\"/default.html\">link</a>";

            var html = DocTester.ProcessSource(source, doc => TextCleaner.AddBlankLinkTargets(doc, true));

            Assert.AreEqual("<a href=\"/default.html\">link</a>", html);
        }

    }
#pragma warning restore CA1707 // Identifiers should not contain underscores
}
