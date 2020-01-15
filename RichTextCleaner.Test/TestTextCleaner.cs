using Microsoft.VisualStudio.TestTools.UnitTesting;
using RichTextCleaner.Common;

namespace RichTextCleaner.Test
{
#pragma warning disable CA1707 // Identifiers should not contain underscores
    [TestClass]
    public class TestTextCleaner
    {
        [TestMethod]
        public void ClearStyling_ShouldRemoveAttributes()
        {
            var source = @"
<p class=""something"">Text 1</p>
<p style=""background-color: black"">Text <span class=""super"">2</span></p>
<p onclick=""clickhandler"">Text 3</p>";

            var html = DocTester.ProcessSource(source, TextCleaner.ClearStyling);

            Assert.AreEqual(@"
<p>Text 1</p>
<p>Text <span>2</span></p>
<p>Text 3</p>", html);
        }
    }
#pragma warning restore CA1707 // Identifiers should not contain underscores
}
