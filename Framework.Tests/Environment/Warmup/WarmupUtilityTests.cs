using System;
using SystemStartUp;
using Xunit;

namespace Framework.Tests.Environment.Warmup
{
    public class WarmupUtilityTests {

        [Fact]
        public void EmptyStringsAreNotAllowed() {
            Assert.Throws<ArgumentException>(() => StartUpUtility.EncodeUrl(""));
            Assert.Throws<ArgumentException>(() => StartUpUtility.EncodeUrl(null));
        }

        [Fact]
        public void EncodedUrlsShouldBeValidFilenames() {
            Assert.Equal(StartUpUtility.EncodeUrl("http://www.microsoft.com"), "http_3A_2F_2Fwww_2Emicrosoft_2Ecom");
            Assert.Equal(StartUpUtility.EncodeUrl("http://www.microsoft.com/foo?bar=baz"), "http_3A_2F_2Fwww_2Emicrosoft_2Ecom_2Ffoo_3Fbar_3Dbaz");
        }

        [Fact]
        public void EncodedUrlsShouldPreserveQueryStrings() {
            Assert.True(StartUpUtility.EncodeUrl("http://www.microsoft.com/foo?bar=baz").IndexOf("bar") > -1);
            Assert.True(StartUpUtility.EncodeUrl("http://www.microsoft.com/foo?bar=baz").IndexOf("baz") > -1);
            Assert.True(StartUpUtility.EncodeUrl("http://www.microsoft.com/foo?bar=baz").IndexOf("foo") > -1);
        }
    }
}