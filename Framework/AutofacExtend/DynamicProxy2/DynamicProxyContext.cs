using System;
using System.Collections.Generic;
using System.Linq;
using Autofac;
using Autofac.Builder;
using Autofac.Core;
using Castle.DynamicProxy;

namespace Framework.AutofacExtend.DynamicProxy2
{
    public class DynamicProxyContext {
        const string ProxyContextKey = "Framework.AutofacExtend.DynamicProxy2.DynamicProxyContext.ProxyContextKey";
                                       
        const string InterceptorServicesKey = "Framework.AutofacExtend.DynamicProxy2.DynamicProxyContext.InterceptorServicesKey";

        readonly IProxyBuilder _proxyBuilder = new DefaultProxyBuilder();
        readonly IDictionary<Type, Type> _cache = new Dictionary<Type, Type>();

        /// <summary>
        /// Static method to resolve the context for a component registration. The context is set
        /// by using the registration builder extension method EnableDynamicProxy(context).
        /// </summary>
        public static DynamicProxyContext From(IComponentRegistration registration) {
            object value;
            if (registration.Metadata.TryGetValue(ProxyContextKey, out value))
                return value as DynamicProxyContext;
            return null;
        }

        /// <summary>
        /// Static method to resolve the context for a component registration. The context is set
        /// by using the registration builder extension method EnableDynamicProxy(context).
        /// </summary>
        public static DynamicProxyContext From(RegistrationData registrationData)
        {
            object value;
            if (registrationData.Metadata.TryGetValue(ProxyContextKey, out value))
                return value as DynamicProxyContext;
            return null;
        }
        /// <summary>
        /// Called indirectly from the EnableDynamicProxy extension method.
        /// Modifies a registration to support dynamic interception if needed, and act as a normal type otherwise.
        /// </summary>
        public void EnableDynamicProxy<TLimit, TConcreteReflectionActivatorData, TRegistrationStyle>(
            IRegistrationBuilder<TLimit, TConcreteReflectionActivatorData, TRegistrationStyle> registrationBuilder)
            where TConcreteReflectionActivatorData : ConcreteReflectionActivatorData {

            // associate this context. used later by static DynamicProxyContext.From() method.
            registrationBuilder.WithMetadata(ProxyContextKey, this);

            // put a shim in place. this will return constructors for the proxy class if it interceptors have been added.
            registrationBuilder.ActivatorData.ConstructorFinder = new ConstructorFinderWrapper(
                registrationBuilder.ActivatorData.ConstructorFinder, this);

            // add a proxy in advance,because ConstructorFinder event in autofac 4.0 later is fire before the autofac module AttachToComponentRegistration event.
            // but is not in autofac 4.0 before.
            // may be cause performance problems.
            AddProxy(registrationBuilder.ActivatorData.ImplementationType);

            
            // when component is being resolved, this even handler will place the array of appropriate interceptors as the first argument
            registrationBuilder.OnPreparing(e => {
                object value;
             
                if (e.Component.Metadata.TryGetValue(InterceptorServicesKey, out value)) {
                    var interceptorServices = (IEnumerable<Service>)value;
                    var interceptors = interceptorServices.Select(service => e.Context.ResolveService(service)).Cast<IInterceptor>().ToArray();
                    var parameter = new PositionalParameter(0, interceptors);
                    e.Parameters = new[] { parameter }.Concat(e.Parameters).ToArray();
                }
            });

           
        }

        /// <summary>
        /// Called indirectly from the InterceptedBy extension method.
        /// Adds services to the componenent's list of interceptors, activating the need for dynamic proxy
        /// </summary>
        public void AddInterceptorService(IComponentRegistration registration, Service service) {
            AddProxy(registration.Activator.LimitType);

            var interceptorServices = Enumerable.Empty<Service>();
            object value;
            if (registration.Metadata.TryGetValue(InterceptorServicesKey, out value)) {
                interceptorServices = (IEnumerable<Service>)value;
            }

            registration.Metadata[InterceptorServicesKey] = interceptorServices.Concat(new[] { service }).Distinct().ToArray();
        }


        ///// <summary>
        ///// Called indirectly from the InterceptedBy extension method.
        ///// Adds services to the componenent's list of interceptors, activating the need for dynamic proxy
        ///// </summary>
        //public void AddInterceptorService<TLimit>(RegistrationData registrationData, params Service[] interceptorServices)
        //{
        //    AddProxy(typeof(TLimit));

        //    var interceptorServicesConcat = Enumerable.Empty<Service>();
        //    object value;
        //    if (registrationData.Metadata.TryGetValue(InterceptorServicesKey, out value))
        //    {
        //        interceptorServicesConcat = (IEnumerable<Service>)value;
        //    }

        //    registrationData.Metadata[InterceptorServicesKey] = interceptorServicesConcat.Concat(interceptorServices ).Distinct().ToArray();


        //}


        /// <summary>
        /// Ensures that a proxy has been generated for the particular type in this context
        /// </summary>
        public void AddProxy(Type type) {
            Type proxyType;
            if (_cache.TryGetValue(type, out proxyType))
                return;

            lock (_cache) {
                if (_cache.TryGetValue(type, out proxyType))
                    return;

                _cache[type] = _proxyBuilder.CreateClassProxyType(type, new Type[0], ProxyGenerationOptions.Default);
            }
        }

        /// <summary>
        /// Determines if a proxy has been generated for the given type, and returns it.
        /// </summary>
        public bool TryGetProxy(Type type, out Type proxyType) {
            return _cache.TryGetValue(type, out proxyType);
        }

    }
}