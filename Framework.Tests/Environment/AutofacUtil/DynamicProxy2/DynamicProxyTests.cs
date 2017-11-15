using System;
using Autofac;
using Autofac.Core;
using Autofac.Features.Metadata;
using Castle.DynamicProxy;
using Framework.AutofacExtend.DynamicProxy2;
using Xunit;

namespace Framework.Tests.Environment.AutofacUtil.DynamicProxy2
{
     public class DynamicProxyTests {
        [Fact]
        public void ContextAddedToMetadataWhenRegistered() {
            var context = new DynamicProxyContext();

            var builder = new ContainerBuilder();
            builder.RegisterType<SimpleComponent>().EnableDynamicProxy(context);

            var container = builder.Build();

            var meta = container.Resolve<Meta<SimpleComponent>>();
            Assert.Contains(meta.Metadata,t => t.Key.Equals("Framework.AutofacExtend.DynamicProxy2.DynamicProxyContext.ProxyContextKey"));
            Assert.Same(meta.Metadata["Framework.AutofacExtend.DynamicProxy2.DynamicProxyContext.ProxyContextKey"], context);
        }

        [Fact]
        public void ProxyContextReturnsTrueIfTypeHasBeenProxied() {
            var context = new DynamicProxyContext();

            Type proxyType;
            Assert.False(context.TryGetProxy(typeof(SimpleComponent), out proxyType));
            Assert.False(context.TryGetProxy(typeof(SimpleComponent), out proxyType));
            Assert.Null(proxyType);

            context.AddProxy(typeof(SimpleComponent));
            Assert.Equal(context.TryGetProxy(typeof(SimpleComponent), out proxyType), true);
            Assert.Equal(context.TryGetProxy(typeof(SimpleComponent), out proxyType), true);
            Assert.NotNull(proxyType);
        }

        [Fact]
        public void AddProxyCanBeCalledMoreThanOnce() {
            var context = new DynamicProxyContext();
            context.AddProxy(typeof(SimpleComponent));

            Type proxyType;
            Assert.True(context.TryGetProxy(typeof(SimpleComponent), out proxyType));
            Assert.NotNull(proxyType);

            Type proxyType2;
            context.AddProxy(typeof(SimpleComponent));
            Assert.True(context.TryGetProxy(typeof(SimpleComponent), out proxyType2));

            Assert.Same(proxyType2, proxyType);
        }

        [Fact]
        public void InterceptorAddedToContextFromModules() {
            var context = new DynamicProxyContext();

            var builder = new ContainerBuilder();
            builder.RegisterType<SimpleComponent>().EnableDynamicProxy(context);
            builder.RegisterModule(new SimpleInterceptorModule());

            builder.Build();

            Type proxyType;
            Assert.True(context.TryGetProxy(typeof(SimpleComponent), out proxyType));
            Assert.NotNull(proxyType);
        }

        [Fact]
        public void ResolvedObjectIsSubclass() {
            var context = new DynamicProxyContext();

            var builder = new ContainerBuilder();
            builder.RegisterType<SimpleComponent>().EnableDynamicProxy(context);
            builder.RegisterModule(new SimpleInterceptorModule());

            var container = builder.Build();

            var simpleComponent = container.Resolve<SimpleComponent>();
            Assert.True(simpleComponent is SimpleComponent);
            Assert.False(simpleComponent.GetType() ==  typeof(SimpleComponent));
        }

        [Fact]
        public void InterceptorCatchesMethodCallOnlyFromContainerWithInterceptor() {
            var context = new DynamicProxyContext();

            var builder1 = new ContainerBuilder();
            builder1.RegisterModule(new SimpleInterceptorModule());
            builder1.RegisterType<SimpleComponent>().EnableDynamicProxy(context);
  
            var container1 = builder1.Build();

            var simple1 = container1.Resolve<SimpleComponent>();

            var builder2 = new ContainerBuilder();
            builder2.RegisterType<SimpleComponent>().EnableDynamicProxy(context);
            var container2 = builder2.Build();

            var simple2 = container2.Resolve<SimpleComponent>();

            Assert.Equal(simple2.SimpleMethod(), "default return value");
            Assert.Equal(simple1.SimpleMethod(), "different return value");
        }
    }

    public class SimpleComponent {
        public virtual string SimpleMethod() {
            return "default return value";
        }
    }

    public class SimpleInterceptorModule : Module {
        protected override void Load(ContainerBuilder builder) {
            builder.RegisterType<SimpleInterceptor>() ;

            base.Load(builder);
        }

        protected override void AttachToComponentRegistration(
            IComponentRegistry componentRegistry,
            IComponentRegistration registration) {

            if (DynamicProxyContext.From(registration) != null)
                registration.InterceptedBy<SimpleInterceptor>();
        }
    }

    public class SimpleInterceptor : IInterceptor {
        public void Intercept(IInvocation invocation) {
            if (invocation.Method.Name == "SimpleMethod") {
                invocation.ReturnValue = "different return value";
            }
            else {
                invocation.Proceed();
            }
        }
    }
}