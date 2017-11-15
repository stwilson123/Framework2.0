using System;
using Autofac.Builder;
using Autofac.Core;
using Autofac.Features.Scanning;
using System.Collections.Generic;
using System.Linq;

namespace Framework.AutofacExtend.DynamicProxy2
{
    public static class DynamicProxyExtensions {

        public static IRegistrationBuilder<TLimit, TConcreteReflectionActivatorData, TRegistrationStyle> EnableDynamicProxy<TLimit, TConcreteReflectionActivatorData, TRegistrationStyle>(
            this IRegistrationBuilder<TLimit, TConcreteReflectionActivatorData, TRegistrationStyle> rb,
            DynamicProxyContext dynamicProxyContext)
            where TConcreteReflectionActivatorData : ConcreteReflectionActivatorData {

            dynamicProxyContext.EnableDynamicProxy(rb);

            return rb;
        }

        public static IRegistrationBuilder<TLimit, ScanningActivatorData, TRegistrationStyle> EnableDynamicProxy<TLimit, TRegistrationStyle>(
            this IRegistrationBuilder<TLimit, ScanningActivatorData, TRegistrationStyle> rb,
            DynamicProxyContext dynamicProxyContext) {

            rb.ActivatorData.ConfigurationActions.Add((t, rb2) => rb2.EnableDynamicProxy(dynamicProxyContext));
            return rb;
        }

        public static void InterceptedBy<TService>(this IComponentRegistration cr) {
            var dynamicProxyContext = DynamicProxyContext.From(cr);
            if (dynamicProxyContext == null)
                throw new ApplicationException(string.Format("Component {0} was not registered with EnableDynamicProxy", cr.Activator.LimitType));

            dynamicProxyContext.AddInterceptorService(cr, new TypedService(typeof(TService)));
        }

        //public static IRegistrationBuilder<TLimit, TActivatorData, TStyle> InterceptedBy<TLimit, TActivatorData, TStyle>(this IRegistrationBuilder<TLimit, TActivatorData, TStyle> builder, params Type[] interceptorServiceTypes)
        //{
        //    if (interceptorServiceTypes == null || ((IEnumerable<Type>)interceptorServiceTypes).Any<Type>((Func<Type, bool>)(t => t == (Type)null)))
        //        throw new ArgumentNullException(nameof(interceptorServiceTypes));


        //    return builder.InterceptedBy<TLimit, TActivatorData, TStyle>((Service[])((IEnumerable<Type>)interceptorServiceTypes).Select<Type, TypedService>((Func<Type, TypedService>)(t => new TypedService((Type)t))).ToArray<TypedService>());
        //}

        //public static IRegistrationBuilder<TLimit, TActivatorData, TStyle> InterceptedBy<TLimit, TActivatorData, TStyle>(this IRegistrationBuilder<TLimit, TActivatorData, TStyle> builder, params Service[] interceptorServices)
        //{
          

        //    if (builder == null)
        //        throw new ArgumentNullException(nameof(builder));
        //    if (interceptorServices == null || ((IEnumerable<Service>)interceptorServices).Any<Service>((Func<Service, bool>)(s => s == (Service)null)))
        //        throw new ArgumentNullException(nameof(interceptorServices));
        //    var dynamicProxyContext = DynamicProxyContext.From(builder.RegistrationData);
        //    if (dynamicProxyContext == null)
        //        throw new ApplicationException(string.Format("Component {0} was not registered with EnableDynamicProxy", typeof(TLimit)));

        //    dynamicProxyContext.AddInterceptorService<TLimit>(builder.RegistrationData, interceptorServices);
        //    //object obj;
        //    //if (((IDictionary<string, object>)builder.RegistrationData.Metadata).TryGetValue("Autofac.Extras.DynamicProxy2.RegistrationExtensions.InterceptorsPropertyName", out obj))
        //    //    ((IDictionary<string, object>)builder.RegistrationData.Metadata)["Autofac.Extras.DynamicProxy2.RegistrationExtensions.InterceptorsPropertyName"] = (object)((IEnumerable<Service>)obj).Concat<Service>((IEnumerable<Service>)interceptorServices).Distinct<Service>();
        //    //else
        //    //    ((IDictionary<string, object>)builder.RegistrationData.Metadata).Add("Autofac.Extras.DynamicProxy2.RegistrationExtensions.InterceptorsPropertyName", (object)interceptorServices);
        //    return builder;
        //}
    }
}