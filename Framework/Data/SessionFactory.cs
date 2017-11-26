using System;
using System.Collections.Generic;
using System.Data;
using Autofac.Features.Metadata;
using Framework.Data.Cfg;
using Framework.Data.Interceptor;
using Framework.Data.Providers;

namespace Framework.Data
{
   public  delegate ISession CreateDbSession(string connectionString);
    public class SessionFactory : ISessionFactory    
    {
        private readonly IEnumerable<Meta<CreateDbSession>> _sessions;
        private readonly Configuration _configuartion;


        public SessionFactory(IEnumerable<Meta<CreateDbSession>> sessions,Configuration  configuartion)
        {
            _sessions = sessions;
            _configuartion = configuartion;
        }
 

        public void Dispose()
        {
            //throw new NotImplementedException();
        }


        public ISession OpenSession()
        {
            foreach (var sessionMeta in _sessions) {
                object name;
                if (!sessionMeta.Metadata.TryGetValue("ProviderName", out name)) {
                    continue;
                }
                if (string.Equals(Convert.ToString(name), _configuartion.ProviderName, StringComparison.OrdinalIgnoreCase)) {
                    return sessionMeta.Value(_configuartion.ConnectString);
                }
            }
            return null;
        }

        public void Close()
        {
            throw new NotImplementedException();
        }

        public void Evict(Type persistentClass)
        {
            throw new NotImplementedException();
        }

        public void Evict(Type persistentClass, object id)
        {
            throw new NotImplementedException();
        }

        public void EvictEntity(string entityName)
        {
            throw new NotImplementedException();
        }

        public void EvictEntity(string entityName, object id)
        {
            throw new NotImplementedException();
        }

        public void EvictCollection(string roleName)
        {
            throw new NotImplementedException();
        }

        public void EvictCollection(string roleName, object id)
        {
            throw new NotImplementedException();
        }

        public void EvictQueries()
        {
            throw new NotImplementedException();
        }

        public void EvictQueries(string cacheRegion)
        {
            throw new NotImplementedException();
        }

        public ISession GetCurrentSession()
        {
            throw new NotImplementedException();
        }

        public bool IsClosed { get; }
        public ICollection<string> DefinedFilterNames { get; }

        public ISession OpenSession(IDbConnection conn, IDbInterceptor sessionLocalInterceptor)
        {
            throw new NotImplementedException();
        }

        public ISession OpenSession(IDbInterceptor sessionLocalInterceptor)
        {
            throw new NotImplementedException();
        }
    }
}