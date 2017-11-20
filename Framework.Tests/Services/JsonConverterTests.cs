using System;
using Framework.Services;
using Newtonsoft.Json.Linq;
using Xunit;

namespace Framework.Tests.Services
{
    public class JsonConverterTests {
        [Fact]
        public void ShouldConvertPrimitiveTypesToJSon() {
            var converter = new DefaultJsonConverter();

            Assert.Equal(converter.Serialize(12), "12");
            Assert.Equal(converter.Serialize(true), "true");
            Assert.Equal(converter.Serialize(12.345), "12.345");
            Assert.Equal(converter.Serialize("string"), "\"string\"");
            Assert.Equal(converter.Serialize(new DateTime(2013, 1, 31, 0, 0, 0, DateTimeKind.Utc)), "\"2013-01-31T00:00:00Z\"");
        }

        [Fact]
        public void ShouldConvertAnonymousTypeToJSon() {
            dynamic d = JObject.Parse("{number:1000, str:'string', array: [1,2,3,4,5,6]}");

            Assert.Equal((int)d.number, 1000);
            Assert.Equal((int)d["number"], 1000);
            Assert.Equal((int)d.array.Count, 6);
        }

        [Fact]
        public void ShouldConvertWellKnownTypeToJSon() {
            var converter = new DefaultJsonConverter();
            string result = converter.Serialize(new Animal { Age = 12, Name = "Milou" });
            var o = converter.Deserialize<Animal>(result);

            Assert.Equal(o.Age, 12);
            Assert.Equal(o.Name, "Milou");
        }


        public class Animal {
            public int Age { get; set; }
            public string Name { get; set; }
        }
    }
}