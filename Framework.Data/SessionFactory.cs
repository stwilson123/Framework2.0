using System;
using System.Collections.Generic;
using System.Data;
using Framework.Data.Cfg;
using Framework.Data.Interceptor;

namespace Framework.Data
{
    public class SessionFactory : ISessionFactory    
    {
        private  Configuration _configuration;

        public SessionFactory(Configuration configuration)
        {
            _configuration = configuration;
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }

       

        public ISession OpenSession(IDbConnection conn)
        {
            throw new NotImplementedException();
        }

        public ISession OpenSession()
        {
            throw new NotImplementedException();
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