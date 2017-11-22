using System.Data;
using PetaPoco;

namespace Framework.Data.Session
{
    public partial class SqlServerSession : Database, ISession
    {
        public SqlServerSession()
            : base("sqlserverConn")
        {
            CommonConstruct();
        }
        
        partial void CommonConstruct();
 
        public IDbConnection Close()
        {
            throw new System.NotImplementedException();
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