using System.Web;
using Autofac;
using Framework.Environment;
using Framework.Mvc;
using Framework.Tests.Stub;
using Framework.Tests.Utility.Container;
using Xunit;

namespace Framework.Tests.Environment
{
    public class DefaultWorkContextAccessorTests : ContainerTestBase {

        HttpContextBase _httpContextCurrent;

        public override void Init() {
            _httpContextCurrent = null;
            base.Init();
        }

        protected override void Register(ContainerBuilder builder) {
            builder.RegisterModule(new MvcModule());
            builder.RegisterModule(new WorkContextModule());
            builder.RegisterType<WorkContextAccessor>().As<IWorkContextAccessor>();
            builder.RegisterAutoMocking();
        }

        protected override void Resolve(ILifetimeScope container) {
            container.Mock<IHttpContextAccessor>()
                .Setup(x => x.Current())
                .Returns(() => _httpContextCurrent);
        }

        [Fact]
        public void ScopeIsCreatedAndCanBeRetrievedFromHttpContextBase() {
            var accessor = _container.Resolve<IWorkContextAccessor>();
            var httpContext = new StubHttpContext();
            
            var workContextScope = accessor.CreateWorkContextScope(httpContext);
            Assert.NotNull(workContextScope.WorkContext);

            var workContext = accessor.GetContext(httpContext);
            Assert.Same(workContext, workContextScope.WorkContext);
        }

        [Fact]
        public void DifferentHttpContextWillHoldDifferentWorkContext() {
            var accessor = _container.Resolve<IWorkContextAccessor>();
            var httpContext1 = new StubHttpContext();
            var workContextScope1 = accessor.CreateWorkContextScope(httpContext1);
            var workContext1 = accessor.GetContext(httpContext1);

            var httpContext2 = new StubHttpContext();
            var workContextScope2 = accessor.CreateWorkContextScope(httpContext2);
            var workContext2 = accessor.GetContext(httpContext2);

            Assert.NotNull(workContext1);
            Assert.Same(workContext1, workContextScope1.WorkContext);
            Assert.NotNull(workContext2);
            Assert.Same(workContext2, workContextScope2.WorkContext);
            Assert.NotSame(workContext1, workContext2);
        }

        [Fact]
        public void ContextIsNullAfterDisposingScope() {
            var accessor = _container.Resolve<IWorkContextAccessor>();
            var httpContext = new StubHttpContext();

            Assert.Null(accessor.GetContext(httpContext));

            var scope = accessor.CreateWorkContextScope(httpContext);
            Assert.NotNull(accessor.GetContext(httpContext));
            
            scope.Dispose();
            Assert.Null(accessor.GetContext(httpContext));
        }

        [Fact]
        public void DifferentChildScopesWillNotCollideInTheSameHttpContext() {
            var shell1 = _container.BeginLifetimeScope();
            var accessor1 = shell1.Resolve<IWorkContextAccessor>();

            var shell2 = _container.BeginLifetimeScope();
            var accessor2 = shell2.Resolve<IWorkContextAccessor>();

            var httpContext = new StubHttpContext();

            Assert.Null(accessor1.GetContext(httpContext));
            Assert.Null(accessor2.GetContext(httpContext));

            var scope1 = accessor1.CreateWorkContextScope(httpContext);
            Assert.NotNull(accessor1.GetContext(httpContext));
            Assert.Null(accessor2.GetContext(httpContext));

            var scope2 = accessor2.CreateWorkContextScope(httpContext);
            Assert.NotNull(accessor1.GetContext(httpContext));
            Assert.NotNull(accessor2.GetContext(httpContext));

            scope1.Dispose();
            Assert.Null(accessor1.GetContext(httpContext));
            Assert.NotNull(accessor2.GetContext(httpContext));

            scope2.Dispose();
            Assert.Null(accessor1.GetContext(httpContext));
            Assert.Null(accessor2.GetContext(httpContext));
        }


        [Fact]
        public void FunctionsByDefaultAgainstAmbientHttpContext() {
            var accessor = _container.Resolve<IWorkContextAccessor>();

            var explicitHttpContext = new StubHttpContext();
            var ambientHttpContext = new StubHttpContext();

            _httpContextCurrent = ambientHttpContext;

            Assert.Null(accessor.GetContext());
            Assert.Null(accessor.GetContext(ambientHttpContext));
            Assert.Null(accessor.GetContext(explicitHttpContext));

            var scope = accessor.CreateWorkContextScope();
            Assert.NotNull(accessor.GetContext());
            Assert.NotNull(accessor.GetContext(ambientHttpContext));
            Assert.Null(accessor.GetContext(explicitHttpContext));
            Assert.Same(accessor.GetContext(), accessor.GetContext(ambientHttpContext));

            _httpContextCurrent = explicitHttpContext;
            Assert.Null(accessor.GetContext());

            _httpContextCurrent = ambientHttpContext;
            Assert.NotNull(accessor.GetContext());

            scope.Dispose();
            Assert.Null(accessor.GetContext());
        }


        [Fact]
        public void StillFunctionsWithoutAmbientHttpContext() {
            var accessor = _container.Resolve<IWorkContextAccessor>();

            Assert.Null(accessor.GetContext());

            var scope = accessor.CreateWorkContextScope();
            Assert.NotNull(accessor.GetContext());

            scope.Dispose();
            Assert.Null(accessor.GetContext());
        }

        [Fact]
        public void DifferentChildScopesWillNotCollideWithoutAmbientHttpContext() {
            var shell1 = _container.BeginLifetimeScope();
            var accessor1 = shell1.Resolve<IWorkContextAccessor>();

            var shell2 = _container.BeginLifetimeScope();
            var accessor2 = shell2.Resolve<IWorkContextAccessor>();

            Assert.Null(accessor1.GetContext());
            Assert.Null(accessor2.GetContext());

            var scope1 = accessor1.CreateWorkContextScope();
            Assert.NotNull(accessor1.GetContext());
            Assert.Null(accessor2.GetContext());

            var scope2 = accessor2.CreateWorkContextScope();
            Assert.NotNull(accessor1.GetContext());
            Assert.NotNull(accessor2.GetContext());

            scope1.Dispose();
            Assert.Null(accessor1.GetContext());
            Assert.NotNull(accessor2.GetContext());

            scope2.Dispose();
            Assert.Null(accessor1.GetContext());
            Assert.Null(accessor2.GetContext());
        }

    }
}