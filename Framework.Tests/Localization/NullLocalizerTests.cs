using Framework.Localization;
using Xunit;

namespace Framework.Tests.Localization
{
    public class NullLocalizerTests {
        [Fact]
        public void StringsShouldPassThrough() {
            var result = NullLocalizer.Instance("hello world");
            Assert.Equal(result.ToString(), "hello world");
        }

        [Fact]
        public void StringsShouldFormatIfArgumentsArePassedIn() {
            var result = NullLocalizer.Instance("hello {0} world", "!");
            Assert.Equal(result.ToString(), "hello ! world");
        }

        [Fact]
        public void StringsShouldNotFormatWithoutAnyArguments() {
            var result = NullLocalizer.Instance("hello {0} world");
            Assert.Equal(result.ToString(),  "hello {0} world");
        }
    }
}