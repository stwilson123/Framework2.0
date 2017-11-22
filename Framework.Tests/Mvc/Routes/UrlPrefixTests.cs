using Framework.Mvc.Routes;
using Xunit;

namespace Framework.Tests.Mvc.Routes
{
     public class UrlPrefixTests {
        [Fact]
        public void RemoveLeadingSegmentsOnlyMatchesFullSegment() {
            var prefix = new UrlPrefix("foo");
            Assert.Equal(prefix.RemoveLeadingSegments("~/foo/bar"), ("~/bar"));
            Assert.Equal(prefix.RemoveLeadingSegments("~/fooo/bar"), ("~/fooo/bar"));
            Assert.Equal(prefix.RemoveLeadingSegments("~/fo/bar"), ("~/fo/bar"));
        }

        [Fact]
        public void RemoveLeadingSegmentsMayContainSlash() {
            var prefix = new UrlPrefix("foo/quux");
            Assert.Equal(prefix.RemoveLeadingSegments("~/foo/quux/bar"), ("~/bar"));
            Assert.Equal(prefix.RemoveLeadingSegments("~/foo/bar"), ("~/foo/bar"));
            Assert.Equal(prefix.RemoveLeadingSegments("~/quux/bar"), ("~/quux/bar"));
        }

        [Fact]
        public void RemoveLeadingSegmentsCanMatchEntireUrl() {
            var prefix = new UrlPrefix("foo");
            Assert.Equal(prefix.RemoveLeadingSegments("~/foo/"), ("~/"));
            Assert.Equal(prefix.RemoveLeadingSegments("~/foo"), ("~/"));
        }

        [Fact]
        public void RemoveLeadingSegmentsIsCaseInsensitive() {
            var prefix = new UrlPrefix("Foo");
            Assert.Equal(prefix.RemoveLeadingSegments("~/foo/bar"), ("~/bar"));
            Assert.Equal(prefix.RemoveLeadingSegments("~/FOO/BAR"), ("~/BAR"));
        }

        [Fact]
        public void RemoveLeadingSegmentsIgnoreLeadingAndTrailingCharactersOnInput() {
            var prefix = new UrlPrefix("foo");
            Assert.Equal(prefix.RemoveLeadingSegments("~/foo/bar"), ("~/bar"));
            var prefix2 = new UrlPrefix("~/foo");
            Assert.Equal(prefix2.RemoveLeadingSegments("~/foo/bar"), ("~/bar"));
            var prefix3 = new UrlPrefix("foo/");
            Assert.Equal(prefix3.RemoveLeadingSegments("~/foo/bar"), ("~/bar"));
        }

        [Fact]
        public void PrependLeadingSegmentsInsertsBeforeNormalVirtualPath() {
            var prefix = new UrlPrefix("foo");
            Assert.Equal(prefix.PrependLeadingSegments("~/bar"), ("~/foo/bar"));
        }

        [Fact]
        public void PrependLeadingSegmentsPreservesNatureOfIncomingPath() {
            var prefix = new UrlPrefix("foo");
            Assert.Equal(prefix.PrependLeadingSegments("~/bar"), ("~/foo/bar"));
            Assert.Equal(prefix.PrependLeadingSegments("/bar"), ("/foo/bar"));
            Assert.Equal(prefix.PrependLeadingSegments("bar"), ("foo/bar"));
        }

        [Fact]
        public void PrependLeadingSegmentsHandlesShortUrlConditionsAppropriately() {
            var prefix = new UrlPrefix("foo");
            Assert.Equal(prefix.PrependLeadingSegments("~/"), ("~/foo/"));
            Assert.Equal(prefix.PrependLeadingSegments("/"), ("/foo/"));
            Assert.Equal(prefix.PrependLeadingSegments("~"), ("~/foo/"));
            Assert.Equal(prefix.PrependLeadingSegments(""), ("foo/"));
        }

    }
}