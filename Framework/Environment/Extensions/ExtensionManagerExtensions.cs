﻿using System.Collections.Generic;
using System.Linq;
using Framework.Environment.Descriptor.Models;
using Framework.Environment.Extensions.Models;

namespace Framework.Environment.Extensions
{
    public static class ExtensionManagerExtensions {
        public static IEnumerable<FeatureDescriptor> EnabledFeatures(this IExtensionManager extensionManager, ShellDescriptor descriptor) {
            return EnabledFeatures(extensionManager, descriptor.Features);
        }

        public static IEnumerable<FeatureDescriptor> EnabledFeatures(this IExtensionManager extensionManager, IEnumerable<ShellFeature> features) {
            return extensionManager.AvailableFeatures().Where(fd => features.Any(sf => sf.Name == fd.Id));
        }

        public static IEnumerable<FeatureDescriptor> DisabledFeatures(this IExtensionManager extensionManager, ShellDescriptor descriptor) {
            return extensionManager.AvailableFeatures().Where(fd => descriptor.Features.All(sf => sf.Name != fd.Id));
        }
    }
}