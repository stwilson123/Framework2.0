using System.IO;
using System.Runtime.Serialization;
using System.Xml;
using Autofac;
using Framework.Environment.Descriptor;
using Framework.Environment.Descriptor.Models;
using Framework.FileSystems.AppData;
using Framework.Tests.Stub;
using Xunit;

namespace Framework.Tests.Environment.Blueprint
{
    public class DefaultShellDescriptorCacheTests
    {
        private IContainer _container;
        private IAppDataFolder _appDataFolder;


        public void Init()
        {
            var clock = new StubClock();
            _appDataFolder = new StubAppDataFolder(clock);

            var builder = new ContainerBuilder();
            builder.RegisterInstance(_appDataFolder).As<IAppDataFolder>();
            builder.RegisterType<ShellDescriptorCache>().As<IShellDescriptorCache>();
            _container = builder.Build();
        }

        public DefaultShellDescriptorCacheTests()
        {
            Init();
        }

        [Fact]
        public void FetchReturnsNullForCacheMiss()
        {
            var service = _container.Resolve<IShellDescriptorCache>();
            var descriptor = service.Fetch("No such shell");
            Assert.Null(descriptor);
        }

        [Fact]
        public void StoreCanBeCalledMoreThanOnceOnTheSameName()
        {
            var service = _container.Resolve<IShellDescriptorCache>();
            var descriptor = new ShellDescriptor {SerialNumber = 6655321};
            service.Store("Hello", descriptor);
            service.Store("Hello", descriptor);
            var result = service.Fetch("Hello");
            Assert.NotNull(result);
            Assert.Equal(result.SerialNumber, 6655321);
        }

        [Fact]
        public void SecondCallUpdatesData()
        {
            var service = _container.Resolve<IShellDescriptorCache>();
            var descriptor1 = new ShellDescriptor {SerialNumber = 6655321};
            service.Store("Hello", descriptor1);
            var descriptor2 = new ShellDescriptor {SerialNumber = 42};
            service.Store("Hello", descriptor2);
            var result = service.Fetch("Hello");
            Assert.NotNull(result);
            Assert.Equal(result.SerialNumber, 42);
        }

        [Fact]
        public void AllDataWillRoundTrip()
        {
            var service = _container.Resolve<IShellDescriptorCache>();

            var descriptor = new ShellDescriptor
            {
                SerialNumber = 6655321,
                Features = new[]
                {
                    new ShellFeature {Name = "f2"},
                    new ShellFeature {Name = "f4"}
                },
                Parameters = new[]
                {
                    new ShellParameter {Component = "p1", Name = "p2", Value = "p3"},
                    new ShellParameter {Component = "p4", Name = "p5", Value = "p6"}
                }
            };
            var descriptorInfo = descriptor.ToDataString();

            service.Store("Hello", descriptor);
            var result = service.Fetch("Hello");
            var resultInfo = result.ToDataString();

            Assert.Contains("6655321", descriptorInfo );
            Assert.Contains("6655321", resultInfo);
            Assert.Contains(resultInfo, descriptorInfo);
        }
    }

    internal static class DataContractExtensions
    {
        public static string ToDataString<T>(this T obj)
        {
            var serializer = new DataContractSerializer(typeof(ShellDescriptor));
            var writer = new StringWriter();
            using (var xmlWriter = XmlWriter.Create(writer))
            {
                serializer.WriteObject(xmlWriter, obj);
            }
            return writer.ToString();
        }
    }
}