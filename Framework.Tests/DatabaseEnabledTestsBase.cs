using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Autofac;
using Framework.Data;
using Framework.Environment.Configuration;
using Framework.Services;
using Framework.Tests.Stub;

namespace Framework.Tests
{
//     public abstract class DatabaseEnabledTestsBase {
//
//        protected IContainer _container;
//
//        protected ISession _session;
//        protected string _databaseFilePath;
//        protected ISessionFactory _sessionFactory;
//        protected StubClock _clock;
//        protected ShellSettings _shellSettings;
//
//       
//        public void InitFixture() {
//        }
//
//        
//        public void TearDownFixture() {
//            File.Delete(_databaseFilePath);
//        }
//
//     
//        public virtual void Init() {
//            _databaseFilePath = Path.GetTempFileName();
//            _sessionFactory = DataUtility.CreateSessionFactory(_databaseFilePath, DatabaseTypes.ToArray());
//            _session = _sessionFactory.OpenSession();
//            _clock = new StubClock();
//
//            var builder = new ContainerBuilder();
//            //builder.RegisterModule(new ImplicitCollectionSupportModule());
//            builder.RegisterType<InfosetHandler>().As<IContentHandler>();
//            builder.RegisterInstance(new StubLocator(_session)).As<ISessionLocator>();
//            builder.RegisterInstance(_clock).As<IClock>();
//            builder.RegisterGeneric(typeof(Repository<>)).As(typeof(IRepository<>)).InstancePerLifetimeScope();
//            builder.RegisterInstance(_shellSettings = new ShellSettings { Name = ShellSettings.DefaultName, DataProvider = "SqlCe" });
//            builder.RegisterType<TestTransactionManager>().As<ITransactionManager>().InstancePerLifetimeScope();
//            builder.Register(context => _sessionFactory.OpenSession()).As<ISession>().InstancePerLifetimeScope();
//
//            Register(builder);
//            _container = builder.Build();
//        }
//
//       
//        public void Cleanup() {
//            if(_container != null)
//                _container.Dispose();
//        }
//
//        public abstract void Register(ContainerBuilder builder);
//
//        protected virtual IEnumerable<Type> DatabaseTypes {
//            get {
//                return Enumerable.Empty<Type>();
//            }
//        }
//
//        protected void ClearSession() {
//            Trace.WriteLine("Flush and clear session");
//            _session.Flush();
//            _session.Clear();
//            Trace.WriteLine("Flushed and cleared session");
//        }
//    }
}