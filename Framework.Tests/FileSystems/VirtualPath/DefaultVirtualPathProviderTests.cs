using Framework.FileSystems.VirtualPath;
using Xunit;

namespace Framework.Tests.FileSystems.VirtualPath
{
     public class DefaultVirtualPathProviderTests {
        [Fact]
        public void TryFileExistsTest() {
            StubDefaultVirtualPathProvider defaultVirtualPathProvider = new StubDefaultVirtualPathProvider();

            Assert.Equal(defaultVirtualPathProvider.TryFileExists("~/a.txt"), true);
            Assert.Equal(defaultVirtualPathProvider.TryFileExists("~/../a.txt"), false);
            Assert.Equal(defaultVirtualPathProvider.TryFileExists("~/a/../a.txt"), true);
            Assert.Equal(defaultVirtualPathProvider.TryFileExists("~/a/b/../a.txt"), true);
            Assert.Equal(defaultVirtualPathProvider.TryFileExists("~/a/b/../../a.txt"), true);
            Assert.Equal(defaultVirtualPathProvider.TryFileExists("~/a/b/../../../a.txt"), false);
            Assert.Equal(defaultVirtualPathProvider.TryFileExists("~/a/../../b/c.txt"), false);
        }

        [Fact]
        public void RejectMalformedVirtualPathTests() {
            StubDefaultVirtualPathProvider defaultVirtualPathProvider = new StubDefaultVirtualPathProvider();

            Assert.Equal(defaultVirtualPathProvider.IsMalformedVirtualPath("~/a.txt"), false);
            Assert.Equal(defaultVirtualPathProvider.IsMalformedVirtualPath("/a.txt"), false);

            Assert.Equal(defaultVirtualPathProvider.IsMalformedVirtualPath("~/../a.txt"), true);
            Assert.Equal(defaultVirtualPathProvider.IsMalformedVirtualPath("/../a.txt"), true);

            Assert.Equal(defaultVirtualPathProvider.IsMalformedVirtualPath("~/a/../a.txt"), false);
            Assert.Equal(defaultVirtualPathProvider.IsMalformedVirtualPath("/a/../a.txt"), false);

            Assert.Equal(defaultVirtualPathProvider.IsMalformedVirtualPath("~/a/b/../a.txt"), false);
            Assert.Equal(defaultVirtualPathProvider.IsMalformedVirtualPath("/a/b/../a.txt"), false);

            Assert.Equal(defaultVirtualPathProvider.IsMalformedVirtualPath("~/a/b/../../a.txt"), false);
            Assert.Equal(defaultVirtualPathProvider.IsMalformedVirtualPath("/a/b/../../a.txt"), false);

            Assert.Equal(defaultVirtualPathProvider.IsMalformedVirtualPath("~/a/b/../../../a.txt"), true);
            Assert.Equal(defaultVirtualPathProvider.IsMalformedVirtualPath("/a/b/../../../a.txt"), true);

            Assert.Equal(defaultVirtualPathProvider.IsMalformedVirtualPath("~/a/../../b//.txt"), true);
            Assert.Equal(defaultVirtualPathProvider.IsMalformedVirtualPath("/a/../../b//.txt"), true);
        }
    }

    internal class StubDefaultVirtualPathProvider : DefaultVirtualPathProvider {
        public override bool FileExists(string path) {
            return true;
        }
    }
}