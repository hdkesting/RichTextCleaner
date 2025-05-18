
using Microsoft.VisualStudio.TestTools.UnitTesting;

using RichTextCleaner.Common;

namespace RichTextCleaner.Test
{
    [TestClass]
    public class TestOfficeMarkup
    {
        [TestMethod]
        public void RemoveOfficeMarkup()
        {
            var source = @"
<p class=MsoNormal><b><span lang=EN-US style='mso-ansi-language:EN-US'>HU-HU</span><u5:p></u5:p></b><span
lang=EN-US style='mso-ansi-language:EN-US'><o:p></o:p></span></p>

<ul style='margin-top:0cm' type=disc>
 <li class=MsoListParagraph style='margin-left:0cm;mso-list:l0 level1 lfo1'><span
     lang=EN-US style='mso-fareast-font-family:""Times New Roman"";mso-ansi-language:
     EN-US'>text in header should be in Hungarian: Olvassa el a Wolters Kluwer
     legújabb Megfelelőségi szakértői betekintéseit – Cikk, whitepaper,
     kutatás, esettanulmány és podcast.<o:p></o:p></span><u5:p></u5:p></li>
 <li class=MsoListParagraph style='margin-left:0cm;mso-list:l0 level1 lfo1'><span
     lang=EN-US style='mso-fareast-font-family:""Times New Roman"";mso-ansi-language:
     EN-US'>Read More button&nbsp;– see row 107 for local translation <o:p></o:p></span><u5:p></u5:p></li>
 <li class=MsoListParagraph style='color:black;margin-left:0cm;mso-list:l0 level1 lfo1'><span
     lang=EN-US style='mso-fareast-font-family:""Times New Roman"";color:windowtext;
     mso-ansi-language:EN-US'>add dynanic card for expert insights </span><span
     lang=EN-US style='mso-fareast-font-family:""Times New Roman"";mso-ansi-language:
     EN-US'><o:p></o:p></span></li>
</ul>

<u5:p></u5:p><u5:p>        ";

            var html = DocTester.ProcessSource(source, doc => TextCleaner.RemoveOfficeMarkup(doc));

            Assert.IsFalse(html.Contains("<o:p>", System.StringComparison.Ordinal), "Office markup should have been removed, like <o:p>");
            Assert.IsFalse(html.Contains("<u5:p>", System.StringComparison.Ordinal), "Office markup should have been removed, like <u5:p>");

        }
    }
}
