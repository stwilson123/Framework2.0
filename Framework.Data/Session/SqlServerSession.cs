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
            throw new System.NotImplementedException();
        }

        public List<T> Get<T>(List<object> ids)
        {
            throw new System.NotImplementedException();
        }

        public IDbConnection Close()
        {
            return this.Close();
        }

        public ITransaction BeginTransaction()
        {
           return this.BeginTransaction();
        }

        public ITransaction BeginTransaction(IsolationLevel isolationLevel)
        {
            return this.BeginTransaction(isolationLevel);

        }

        public ITransaction Transaction {
            get { return this.Transaction; }
        }
    }
}