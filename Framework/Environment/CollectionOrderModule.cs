﻿using System;
using System.Collections;
using Autofac.Core;

namespace Framework.Environment
{
    internal class CollectionOrderModule : IModule {
        public void Configure(IComponentRegistry componentRegistry) {
            componentRegistry.Registered += (sender, registered) => {
                // only bother watching enumerable resolves
                var limitType = registered.ComponentRegistration.Activator.LimitType;
                if (typeof(IEnumerable).IsAssignableFrom(limitType)) {
                    registered.ComponentRegistration.Activated += (sender2, activated) => {
                        // Autofac's IEnumerable feature returns an Array
                        if (activated.Instance is Array) {
                            // System needs FIFO, not FILO, component order
                            Array.Reverse((Array)activated.Instance);
                        }
                    };
                }
            };
        }
    }
}