using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Web.Hosting;
using System.Web.Http;
using System.Web.Http.Dispatcher;
using System.Web.Mvc;
using Autofac;
using Autofac.Core;
using Framework.AutofacExtend.Config;
using Framework.Caching;
using Framework.Environment.Assemblys;
using Framework.Environment.Configuration;
using Framework.Environment.Descriptor;
using Framework.Environment.Extensions;
using Framework.Environment.Extensions.Compilers;
using Framework.Environment.Extensions.Folders;
using Framework.Environment.Extensions.Loaders;
using Framework.Environment.ShellBuilder;
using Framework.Environment.State;
using Framework.Events;
using Framework.FileSystems.AppData;
using Framework.FileSystems.Dependencies;
using Framework.FileSystems.LockFile;
using Framework.FileSystems.VirtualPath;
using Framework.FileSystems.WebSite;
using Framework.Logging;
using Framework.Mvc;
using Framework.Mvc.DataAnnotations;
using Framework.Mvc.Filters;
using Framework.Mvc.ViewEngines;
using Framework.Mvc.ViewEngines.Razor;
using Framework.Services;
using Framework.UI.Resources;
using Framework.WebApi;
using Framework.WebApi.Filters;
using Orchard.Environment.Extensions.Loaders;

namespace Framework.Environment
{
    public static class SystemStarter
    {
        public static IContainer CreateHostContainer(Action<ContainerBuilder> registrations)
        {
            ExtensionLocations extensionLocations = new ExtensionLocations();
            
            var builder = new ContainerBuilder();
            builder.RegisterModule(new CollectionOrderModule());
            builder.RegisterModule(new LoggingModule());
            builder.RegisterModule(new EventsModule());
            builder.RegisterModule(new CacheModule());
            
            // a single default host implementation is needed for bootstrapping a web app domain
            builder.RegisterType<DefaultOrchardEventBus>().As<IEventBus>().SingleInstance();
            builder.RegisterType<DefaultCacheHolder>().As<ICacheHolder>().SingleInstance();
            builder.RegisterType<DefaultCacheContextAccessor>().As<ICacheContextAccessor>().SingleInstance();
            builder.RegisterType<DefaultParallelCacheContext>().As<IParallelCacheContext>().SingleInstance();
           // builder.RegisterType<DefaultAsyncTokenProvider>().As<IAsyncTokenProvider>().SingleInstance();
            builder.RegisterType<DefaultHostEnvironment>().As<IHostEnvironment>().SingleInstance();    
            builder.RegisterType<DefaultHostLocalRestart>().As<IHostLocalRestart>().Named<IEventHandler>(typeof(IShellSettingsManagerEventHandler).Name).SingleInstance();
            builder.RegisterType<DefaultBuildManager>().As<IBuildManager>().SingleInstance();
            builder.RegisterType<DynamicModuleVirtualPathProvider>().As<ICustomVirtualPathProvider>().SingleInstance();
            builder.RegisterType<AppDataFolderRoot>().As<IAppDataFolderRoot>().SingleInstance();
            builder.RegisterType<DefaultExtensionCompiler>().As<IExtensionCompiler>().SingleInstance();
            builder.RegisterType<DefaultRazorCompilationEvents>().As<IRazorCompilationEvents>().SingleInstance();
            builder.RegisterType<DefaultProjectFileParser>().As<IProjectFileParser>().SingleInstance();
            builder.RegisterType<DefaultAssemblyLoader>().As<IAssemblyLoader>().SingleInstance();
            builder.RegisterType<AppDomainAssemblyNameResolver>().As<IAssemblyNameResolver>().SingleInstance();
            builder.RegisterType<GacAssemblyNameResolver>().As<IAssemblyNameResolver>().SingleInstance();
            builder.RegisterType<SystemFrameworkAssemblyNameResolver>().As<IAssemblyNameResolver>().SingleInstance();
            builder.RegisterType<HttpContextAccessor>().As<IHttpContextAccessor>().InstancePerDependency();
           // builder.RegisterType<ViewsBackgroundCompilation>().As<IViewsBackgroundCompilation>().SingleInstance();
            //builder.RegisterType<DefaultExceptionPolicy>().As<IExceptionPolicy>().SingleInstance();
            builder.RegisterType<DefaultCriticalErrorProvider>().As<ICriticalErrorProvider>().SingleInstance();
            builder.RegisterType<ResourceFileHashProvider>().As<IResourceFileHashProvider>().SingleInstance();
            //builder.RegisterType<RazorTemplateCache>().As<IRazorTemplateProvider>().SingleInstance();

            RegisterVolatileProvider<WebSiteFolder, IWebSiteFolder>(builder);
            RegisterVolatileProvider<AppDataFolder, IAppDataFolder>(builder);  
            RegisterVolatileProvider<DefaultLockFileManager, ILockFileManager>(builder);
            RegisterVolatileProvider<Clock, IClock>(builder);
            RegisterVolatileProvider<DefaultDependenciesFolder, IDependenciesFolder>(builder);
            RegisterVolatileProvider<DefaultAssemblyProbingFolder, IAssemblyProbingFolder>(builder);
            RegisterVolatileProvider<DefaultVirtualPathMonitor, IVirtualPathMonitor>(builder);
            RegisterVolatileProvider<DefaultVirtualPathProvider, IVirtualPathProvider>(builder);

            
            builder.RegisterType<DefaultSystemHost>().As<ISystemHost>().As<IEventHandler>()
                .Named<IEventHandler>(typeof(IShellSettingsManagerEventHandler).Name)
                .Named<IEventHandler>(typeof(IShellDescriptorManagerEventHandler).Name)
                .SingleInstance();
             {
                builder.RegisterType<ShellSettingsManager>().As<IShellSettingsManager>().SingleInstance();

                builder.RegisterType<ShellContextFactory>().As<IShellContextFactory>().SingleInstance();
                {
                    builder.RegisterType<ShellDescriptorCache>().As<IShellDescriptorCache>().SingleInstance();

                    builder.RegisterType<CompositionStrategy>().As<ICompositionStrategy>().SingleInstance();
                    {
                        builder.RegisterType<ShellContainerRegistrations>().As<IShellContainerRegistrations>().SingleInstance();
                        builder.RegisterType<ExtensionLoaderCoordinator>().As<IExtensionLoaderCoordinator>().SingleInstance();
                        builder.RegisterType<ExtensionMonitoringCoordinator>().As<IExtensionMonitoringCoordinator>().SingleInstance();
                        builder.RegisterType<ExtensionManager>().As<IExtensionManager>().SingleInstance();
                        {
                            builder.RegisterType<ExtensionHarvester>().As<IExtensionHarvester>().SingleInstance();
                        builder.RegisterType<ModuleFolders>().As<IExtensionFolders>().SingleInstance()
                                  .WithParameter(new NamedParameter("paths", extensionLocations.ModuleLocations));
                            builder.RegisterType<CoreModuleFolders>().As<IExtensionFolders>().SingleInstance()
                                .WithParameter(new NamedParameter("paths", extensionLocations.CoreLocations));
                            builder.RegisterType<ThemeFolders>().As<IExtensionFolders>().SingleInstance()
                                .WithParameter(new NamedParameter("paths", extensionLocations.ThemeLocations));
 
                             builder.RegisterType<CoreExtensionLoader>().As<IExtensionLoader>().SingleInstance();
                            builder.RegisterType<ReferencedExtensionLoader>().As<IExtensionLoader>().SingleInstance();
                             builder.RegisterType<PrecompiledExtensionLoader>().As<IExtensionLoader>().SingleInstance();
                            builder.RegisterType<DynamicExtensionLoader>().As<IExtensionLoader>().SingleInstance();
                            builder.RegisterType<RawThemeExtensionLoader>().As<IExtensionLoader>().SingleInstance();
                       }
                    }
 
                    builder.RegisterType<ShellContainerFactory>().As<IShellContainerFactory>().SingleInstance();
                }
//
                builder.RegisterType<DefaultProcessingEngine>().As<IProcessingEngine>().SingleInstance();
            }
            builder.RegisterType<RunningShellTable>().As<IRunningShellTable>().SingleInstance();
            builder.RegisterType<DefaultSystemShell>().As<ISystemShell>().InstancePerMatchingLifetimeScope("shell");
           // builder.RegisterType<SessionConfigurationCache>().As<ISessionConfigurationCache>().InstancePerMatchingLifetimeScope("shell");

            registrations(builder);

            var autofacSection = ConfigurationManager.GetSection(ConfigurationSettingsReaderConstants.DefaultSectionName);
            if (autofacSection != null)
                // builder.RegisterModule(new ConfigurationSettingsReader());
                builder.RegisterModule(ConfigurationSettingsReaderFactory.CreateConfigurationSettingsReader());
            var optionalHostConfig = HostingEnvironment.MapPath("~/Config/Host.config");
            if (File.Exists(optionalHostConfig))
//                builder.RegisterModule(new ConfigurationSettingsReader(ConfigurationSettingsReaderConstants.DefaultSectionName, optionalHostConfig));
                builder.RegisterModule(ConfigurationSettingsReaderFactory.CreateConfigurationSettingsReader(optionalHostConfig));

            var optionalComponentsConfig = HostingEnvironment.MapPath("~/Config/HostComponents.config");
            if (File.Exists(optionalComponentsConfig))
                builder.RegisterModule(new HostComponentsConfigModule(optionalComponentsConfig));

            var container = builder.Build();

            //
            // Register Virtual Path Providers
            //
            if (HostingEnvironment.IsHosted) {
                foreach (var vpp in container.Resolve<IEnumerable<ICustomVirtualPathProvider>>()) {
                    HostingEnvironment.RegisterVirtualPathProvider(vpp.Instance);
                }
            }
//
            ControllerBuilder.Current.SetControllerFactory(new SystemControllerFactory());
            FilterProviders.Providers.Add(new SystemFilterProvider());
//
            GlobalConfiguration.Configuration.Services.Replace(typeof(IHttpControllerSelector), new DefaultSystemWebApiHttpControllerSelector(GlobalConfiguration.Configuration));
            GlobalConfiguration.Configuration.Services.Replace(typeof(System.Web.Http.Dispatcher.IHttpControllerActivator), new DefaultSystemWebApiHttpControllerActivator(GlobalConfiguration.Configuration));
            GlobalConfiguration.Configuration.DependencyResolver = new AutofacWebApiDependencyResolver(container);
//
            GlobalConfiguration.Configuration.Filters.Add(new SystemApiActionFilterDispatcher());
            GlobalConfiguration.Configuration.Filters.Add(new SystemApiExceptionFilterDispatcher());
            GlobalConfiguration.Configuration.Filters.Add(new SystemApiAuthorizationFilterDispatcher());

            ViewEngines.Engines.Clear();
            ViewEngines.Engines.Add(new ThemeAwareViewEngineShim());
//
            var hostContainer = new DefaultSystemHostContainer(container);
            //MvcServiceLocator.SetCurrent(hostContainer);
            SystemHostContainerRegistry.RegisterHostContainer(hostContainer);
//
//            // Register localized data annotations
            ModelValidatorProviders.Providers.Clear();
            ModelValidatorProviders.Providers.Add(new LocalizedModelValidatorProvider());
            
            
            
            return container;
        }
        
        private static void RegisterVolatileProvider<TRegister, TService>(ContainerBuilder builder) where TService : IVolatileProvider {
            builder.RegisterType<TRegister>()
                .As<TService>()
                .As<IVolatileProvider>()
                .SingleInstance();
        }

        public static ISystemHost CreateHost(Action<ContainerBuilder> registrations) {
            var container = CreateHostContainer(registrations);
            return container.Resolve<ISystemHost>();
        }
    }
 
}