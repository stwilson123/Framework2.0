using System.Collections.Generic;
using System.IO;
using Framework.Data.Bags;
using Framework.Data.Bags.Serialization;
using Xunit;

namespace Framework.Tests.Data.Bags
{
   public class BagsTests {
        [Fact]
        public void ShouldRemoveMember() {
            dynamic e = new Bag();
            e.Foo = "Bar";
            Assert.NotEmpty(e );
            Assert.Equal(e.Foo,  "Bar");

            e.Foo = null;
            Assert.Empty(e );
        }

        [Fact]
        public void ShouldSupportFactoryInvocation() {
            var e = Bag.New();

            e.Foo = "Bar";
            Assert.Equal(e["Foo"], "Bar");
            Assert.Equal(e.Foo, ("Bar"));
        }

        [Fact]
        public void ShouldAddDynamicProperties() {
            dynamic e = new Bag();
            e.Foo = "Bar";
            Assert.Equal(e["Foo"], ("Bar"));
            Assert.Equal(e.Foo, ("Bar"));
        }

        [Fact]
        public void UnknownPropertiesShouldBeNull() {
            dynamic e = new Bag();
            Assert.Equal((object)e["Foo"], (null));
            Assert.Equal((object)e.Foo, (null));
        }

        [Fact]
        public void ShouldAddDynamicObjects() {
            dynamic e = new Bag();
            e.Address = new Bag();
            
            e.Address.Street = "One Microsoft Way";
            Assert.Equal(e["Address"]["Street"], ("One Microsoft Way"));
            Assert.Equal(e.Address.Street, ("One Microsoft Way"));
        }
       [Fact]
        public void ShouldAddArraysOfAnonymousObject() {
            dynamic e = new Bag();

            e.Foos = new[] { new { Foo1 = "Bar1", Foo2 = "Bar2" } };
            Assert.Equal(e.Foos[0].Foo1, ("Bar1"));
            Assert.Equal(e.Foos[0].Foo2, ("Bar2"));
        }
       [Fact]
        public void ShouldAddAnonymousObject() {
            dynamic e = new Bag();

            e.Foos = new { Foo1 = "Bar1", Foo2 = "Bar2" };
            Assert.Equal(e.Foos.Foo1, ("Bar1"));
            Assert.Equal(e.Foos.Foo2, ("Bar2"));
        }

        [Fact]
        public void ShouldAddArrays() {
            dynamic e = new Bag();
            e.Owners = new[] { "Steve", "Bill" };
            Assert.Equal(e.Owners[0], ("Steve"));
            Assert.Equal(e.Owners[1], ("Bill"));
        }

        [Fact]
        public void ShouldBeEnumerable() {
            dynamic e = new Bag();
            e.Address = new Bag();

            e.Address.Street = "One Microsoft Way";
            e.Foos = new[] { new { Foo1 = "Bar1", Foo2 = "Bar2" } };
            e.Owners = new[] { "Steve", "Bill" };

     
            // IEnumerable
            Assert.Contains(e as IEnumerable<KeyValuePair<string, object>>, x => x.Key == "Address");
            Assert.Contains(e as IEnumerable<KeyValuePair<string, object>>, x => x.Key == "Owners");
            Assert.Contains(e as IEnumerable<KeyValuePair<string, object>>, x => x.Key == "Foos");
        }

        [Fact]
        public void ShouldSerializeAndDeserialize() {
            dynamic e = new Bag();
            
            e.Foo = "Bar";
            
            e.Address = new Bag();
            e.Address.Street = "One Microsoft Way";
            e.Owners = new[] { "Steve", "Bill" };
            e.Foos = new[] { new { Foo1 = "Bar1", Foo2 = "Bar2" } };

            string xml1;

            var serializer = new XmlSettingsSerializer();
            using (var sw = new StringWriter()) {
                serializer.Serialize(sw, e);
                xml1 = sw.ToString();
            }

            dynamic clone;

            using (var sr = new StringReader(xml1)) {
                clone = serializer.Deserialize(sr);
            }

            string xml2;

            using (var sw = new StringWriter()) {
                serializer.Serialize(sw, clone);
                xml2 = sw.ToString();
            }

            Assert.Equal(xml1, (xml2));
        }

        [Fact]
        public void MergeShouldOverwriteExistingProperties() {
            var o1 = Bag.New();
            o1.Foo = "Foo1";
            o1.Bar = "Bar1";

            var o2 = Bag.New();
            o2.Foo = "Foo2";
            o2.Baz = "Baz2";

            var o3 = o1 & o2;

            Assert.Equal(o3.Foo, ("Foo2"));
            Assert.Equal(o3.Bar, ("Bar1"));
            Assert.Equal(o3.Baz, ("Baz2"));
        }

        [Fact]
        public void MergeShouldConcatenateArrays() {
            var o1 = Bag.New();
            o1.Foo = new[] { "a", "b" };

            var o2 = Bag.New();
            o2.Foo = new[] { "c", "d" };

            var o3 = o1 & o2;

            Assert.Equal(o3.Foo[0], ("a"));
            Assert.Equal(o3.Foo[1], ("b"));
            Assert.Equal(o3.Foo[2], ("c"));
            Assert.Equal(o3.Foo[3], ("d"));
        }
    }
}