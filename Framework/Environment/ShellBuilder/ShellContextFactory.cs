using System.Linq;
using Autofac;
using Framework.Environment.Configuration;
using Framework.Environment.Descriptor;
using Framework.Environment.Descriptor.Models;
using Framework.Logging;

namespace Framework.Environment.ShellBuilder
{
     public class ShellContextFactory : IShellContextFactory {
         private readonly IShellDescriptorCache _shellDescriptorCache;
         private readonly ICompositionStrategy _compositionStrategy;
         private readonly IShellContainerFactory _shellContainerFactory;
 
         public ShellContextFactory(
             IShellDescriptorCache shellDescriptorCache,
             ICompositionStrategy compositionStrategy,
             IShellContainerFactory shellContainerFactory) {
             _shellDescriptorCache = shellDescriptorCache;
             _compositionStrategy = compositionStrategy;
             _shellContainerFactory = shellContainerFactory;
             Logger = NullLogger.Instance;
         }
 
         public ILogger Logger { get; set; }
 
         public ShellContext CreateShellContext(ShellSettings settings) {           
 
             Logger.Debug("Creating shell context for tenant {0}", settings.Name);
 
             var knownDescriptor = _shellDescriptorCache.Fetch(settings.Name);
             if (knownDescriptor == null) {
                 Logger.Information("No descriptor cached. Starting with minimum components.");
                 knownDescriptor = MinimumShellDescriptor();
             }
 
             var blueprint = _compositionStrategy.Compose(settings, knownDescriptor);
             var shellScope = _shellContainerFactory.CreateContainer(settings, blueprint);
 
             ShellDescriptor currentDescriptor;
             using (var standaloneEnvironment = shellScope.CreateWorkContextScope()) {
                 var shellDescriptorManager = standaloneEnvironment.Resolve<IShellDescriptorManager>();
                 currentDescriptor = shellDescriptorManager.GetShellDescriptor();
             }
 
             if (currentDescriptor != null && knownDescriptor.SerialNumber != currentDescriptor.SerialNumber) {
                 Logger.Information("Newer descriptor obtained. Rebuilding shell container.");
 
                 _shellDescriptorCache.Store(settings.Name, currentDescriptor);
                 blueprint = _compositionStrategy.Compose(settings, currentDescriptor);
                 shellScope.Dispose();
                 shellScope = _shellContainerFactory.CreateContainer(settings, blueprint);
             }
 
             return new ShellContext {
                 Settings = settings,
                 Descriptor = currentDescriptor,
                 Blueprint = blueprint,
                 LifetimeScope = shellScope,
                 Shell = shellScope.Resolve<ISystemShell>(),
             };
         }
 
         private static ShellDescriptor MinimumShellDescriptor() {
             return new ShellDescriptor {
                 SerialNumber = -1,
                 Features = new[] {
                     new ShellFeature {Name = "System.Framework"},
                     new ShellFeature {Name = "Settings"},
                 },
                 Parameters = Enumerable.Empty<ShellParameter>(),
             };
         }
 
         public ShellContext CreateSetupContext(ShellSettings settings) {
             Logger.Debug("No shell settings available. Creating shell context for setup");
 
             var descriptor = new ShellDescriptor {
                 SerialNumber = -1,
                 Features = new[] {
                     new ShellFeature { Name = "System.Setup" },
                     new ShellFeature { Name = "Shapes" },
                     new ShellFeature { Name = "System.Resources" }
                 },
             };
 
             var blueprint = _compositionStrategy.Compose(settings, descriptor);
             var shellScope = _shellContainerFactory.CreateContainer(settings, blueprint);
 
             return new ShellContext {
                 Settings = settings,
                 Descriptor = descriptor,
                 Blueprint = blueprint,
                 LifetimeScope = shellScope,
                 Shell = shellScope.Resolve<ISystemShell>(),
             };
         }
 
         public ShellContext CreateDescribedContext(ShellSettings settings, ShellDescriptor shellDescriptor) {
             Logger.Debug("Creating described context for tenant {0}", settings.Name);
 
             var blueprint = _compositionStrategy.Compose(settings, shellDescriptor);
             var shellScope = _shellContainerFactory.CreateContainer(settings, blueprint);
 
             return new ShellContext
             {
                 Settings = settings,
                 Descriptor = shellDescriptor,
                 Blueprint = blueprint,
                 LifetimeScope = shellScope,
                 Shell = shellScope.Resolve<ISystemShell>(),
             };
         }
     }
}