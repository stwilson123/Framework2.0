using System.Collections.Generic;
using System.Text;
using Xunit;

namespace Framework.Tests.Localization
{
//     public class LocalizationStreamParserTests {
//
//        [Fact]
//        public void ShouldTrimLeadingQuotes() {
//            var parser = new LocalizationStreamParser();
//
//            var text = new StringBuilder();
//            text.AppendLine("#: ~/Themes/MyTheme/Views/MyView.cshtml");
//            text.AppendLine("msgctxt \"~/Themes/MyTheme/Views/MyView.cshtml\"");
//            text.AppendLine("msgid \"\\\"{0}\\\" Foo\"");
//            text.AppendLine("msgstr \"\\\"{0}\\\" Foo\"");
//
//            var translations = new Dictionary<string, string>();
//            parser.ParseLocalizationStream(text.ToString(), translations, false);
//
//            Assert.Equal("\"{0}\" Foo", translations["~/themes/mytheme/views/myview.cshtml|\"{0}\" foo"]);
//        }
//
//        [Fact]
//        public void ShouldTrimTrailingQuotes() {
//            var parser = new LocalizationStreamParser();
//
//            var text = new StringBuilder();
//            text.AppendLine("#: ~/Themes/MyTheme/Views/MyView.cshtml");
//            text.AppendLine("msgctxt \"~/Themes/MyTheme/Views/MyView.cshtml\"");
//            text.AppendLine("msgid \"Foo \\\"{0}\\\"\"");
//            text.AppendLine("msgstr \"Foo \\\"{0}\\\"\"");
//
//            var translations = new Dictionary<string, string>();
//            parser.ParseLocalizationStream(text.ToString(), translations, false);
//
//            Assert.Equal("Foo \"{0}\"", translations["~/themes/mytheme/views/myview.cshtml|foo \"{0}\""]);
//        }
//
//        [Fact]
//        public void ShouldHandleUnclosedQuote() {
//            var parser = new LocalizationStreamParser();
//
//            var text = new StringBuilder();
//            text.AppendLine("#: ~/Themes/MyTheme/Views/MyView.cshtml");
//            text.AppendLine("msgctxt \"");
//            text.AppendLine("msgid \"Foo \\\"{0}\\\"\"");
//            text.AppendLine("msgstr \"Foo \\\"{0}\\\"\"");
//
//            var translations = new Dictionary<string, string>();
//            parser.ParseLocalizationStream(text.ToString(), translations, false);
//
//            Assert.Equal("Foo \"{0}\"", translations["|foo \"{0}\""]);
//        }
//    }
}