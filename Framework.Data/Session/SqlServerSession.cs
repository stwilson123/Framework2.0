using System.Collections.Generic;
using System.Data;
using PetaPoco;

namespace Framework.Data.Session
{
    public partial class SqlServerSession : Database, ISession
    {
        public SqlServerSession(string connectString)
            : base(connectString,"SqlServer")
        {
            CommonConstruct();
        }
        
        public static string ProviderName {
            get { return "SqlServer"; }
        }
        
        partial void CommonConstruct();

        public object Insert(object poco)
        {
            return base.Insert(poco);
        }
        
        public IList<object> Insert(IList<object> poco)
        {
            throw new System.NotImplementedException();
        }

        public bool Delete(object id)
        {
            throw new System.NotImplementedException();
        }

        public bool Delete(List<object> ids)
        {
            throw new System.NotImplementedException();
        }

        public bool Update(object poco)
        {
            throw new System.NotImplementedException();
        }

        public T Get<T>(object id)
        {
            return base.SingleOrDefault<T>(id);
        }

        public List<T> Get<T>(List<object> ids)
        {
            throw new System.NotImplementedException();
        }

        public IDbConnection Close()
        {
            base.CloseSharedConnection();
            return base.Connection;
        }

        public ITransaction BeginTransaction()
        {
            base.BeginTransaction();
            return base.GetTransaction();
        }

        public ITransaction BeginTransaction(IsolationLevel isolationLevel)
        {
            base.IsolationLevel = isolationLevel;
            base.BeginTransaction();
            return base.GetTransaction();

        }

        public ITransaction Transaction {
            get { return this.Transaction; }
        }
    }
}