using System;
using System.Data;
using System.Linq.Expressions;
using System.Web.Http.Filters;
using PetaPoco;

namespace Framework.Data
{
    public interface ISession
    {
      
      
        
        /// <summary>
        /// End the <c>ISession</c> by disconnecting from the ADO.NET connection and cleaning up.
        /// </summary>
        /// <remarks>
        /// It is not strictly necessary to <c>Close()</c> the <c>ISession</c> but you must
        /// at least <c>Disconnect()</c> it.
        /// </remarks>
        /// <returns>The connection provided by the application or <see langword="null" /></returns>
        IDbConnection Close();
        
        /// <summary>Gets the ADO.NET connection.</summary>
        /// <remarks>
        /// Applications are responsible for calling commit/rollback upon the connection before
        /// closing the <c>ISession</c>.
        /// </remarks>
        IDbConnection Connection { get; }
        
        
        /// <summary>
        /// Begin a unit of work and return the associated <c>ITransaction</c> object.
        /// </summary>
        /// <remarks>
        /// If a new underlying transaction is required, begin the transaction. Otherwise
        /// continue the new work in the context of the existing underlying transaction.
        /// The class of the returned <see cref="T:NHibernate.ITransaction" /> object is determined by
        /// the property <c>transaction_factory</c>
        /// </remarks>
        /// <returns>A transaction instance</returns>
        ITransaction BeginTransaction();

        /// <summary>
        /// Begin a transaction with the specified <c>isolationLevel</c>
        /// </summary>
        /// <param name="isolationLevel">Isolation level for the new transaction</param>
        /// <returns>A transaction instance having the specified isolation level</returns>
        ITransaction BeginTransaction(IsolationLevel isolationLevel);

        /// <summary>
        /// Get the current Unit of Work and return the associated <c>ITransaction</c> object.
        /// </summary>
        ITransaction Transaction { get; }

    }
}