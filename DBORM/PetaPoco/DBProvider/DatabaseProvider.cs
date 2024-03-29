﻿using PetaPoco;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Text;

namespace PetaPoco.DBProvider
{
    public abstract class DatabaseProvider : IProvider
    {
        /// <summary>
        ///     Gets the DbProviderFactory for this database provider.
        /// </summary>
        /// <returns>The provider factory.</returns>
        public abstract DbProviderFactory GetFactory();

        /// <summary>
        ///     Gets a flag for whether the DB has native support for GUID/UUID.
        /// </summary>
        public virtual bool HasNativeGuidSupport
        {
            get { return false; }
        }

        /// <summary>
        ///     Gets the <seealso cref="IPagingHelper" /> this provider supplies.
        /// </summary>
        public virtual IPagingHelper PagingUtility
        {
            get { return PagingHelper.Instance; }
        }

        /// <summary>
        ///     Escape a tablename into a suitable format for the associated database provider.
        /// </summary>
        /// <param name="tableName">
        ///     The name of the table (as specified by the client program, or as attributes on the associated
        ///     POCO class.
        /// </param>
        /// <returns>The escaped table name</returns>
        public virtual string EscapeTableName(string tableName)
        {
            // Assume table names with "dot" are already escaped
            return tableName.IndexOf('.') >= 0 ? tableName : EscapeSqlIdentifier(tableName);
        }

        /// <summary>
        ///     Escape and arbitary SQL identifier into a format suitable for the associated database provider
        /// </summary>
        /// <param name="sqlIdentifier">The SQL identifier to be escaped</param>
        /// <returns>The escaped identifier</returns>
        public virtual string EscapeSqlIdentifier(string sqlIdentifier)
        {
            return string.Format("[{0}]", sqlIdentifier);
        }

        /// <summary>
        ///     Returns the prefix used to delimit parameters in SQL query strings.
        /// </summary>
        /// <param name="connectionString">The connection string.</param>
        /// <returns>The providers character for prefixing a query parameter.</returns>
        public virtual string GetParameterPrefix(string connectionString)
        {
            return "@";
        }

        /// <summary>
        ///     Converts a supplied C# object value into a value suitable for passing to the database
        /// </summary>
        /// <param name="value">The value to convert</param>
        /// <returns>The converted value</returns>
        public virtual object MapParameterValue(object value)
        {
            if (value is bool)
                return ((bool)value) ? 1 : 0;

            return value;
        }

        /// <summary>
        ///     Called immediately before a command is executed, allowing for modification of the IDbCommand before it's passed to
        ///     the database provider
        /// </summary>
        /// <param name="cmd"></param>
        public virtual void PreExecute(IDbCommand cmd)
        {
        }

        /// <summary>
        ///     Builds an SQL query suitable for performing page based queries to the database
        /// </summary>
        /// <param name="skip">The number of rows that should be skipped by the query</param>
        /// <param name="take">The number of rows that should be retruend by the query</param>
        /// <param name="parts">The original SQL query after being parsed into it's component parts</param>
        /// <param name="args">Arguments to any embedded parameters in the SQL query</param>
        /// <returns>The final SQL query that should be executed.</returns>
        public virtual string BuildPageQuery(long skip, long take, SQLParts parts, ref object[] args)
        {
            var sql = string.Format("{0}\nLIMIT @{1} OFFSET @{2}", parts.Sql, args.Length, args.Length + 1);
            args = args.Concat(new object[] { take, skip }).ToArray();
            return sql;
        }

        /// <summary>
        ///     Returns an SQL Statement that can check for the existence of a row in the database.
        /// </summary>
        /// <returns></returns>
        public virtual string GetExistsSql()
        {
            return "SELECT COUNT(*) FROM {0} WHERE {1}";
        }

        /// <summary>
        ///     Return an SQL expression that can be used to populate the primary key column of an auto-increment column.
        /// </summary>
        /// <param name="tableInfo">Table info describing the table</param>
        /// <returns>An SQL expressions</returns>
        /// <remarks>See the Oracle database type for an example of how this method is used.</remarks>
        public virtual string GetAutoIncrementExpression(TableInfo tableInfo)
        {
            return null;
        }

        /// <summary>
        ///     Returns an SQL expression that can be used to specify the return value of auto incremented columns.
        /// </summary>
        /// <param name="primaryKeyName">The primary key of the row being inserted.</param>
        /// <returns>An expression describing how to return the new primary key value</returns>
        /// <remarks>See the SQLServer database provider for an example of how this method is used.</remarks>
        public virtual string GetInsertOutputClause(string primaryKeyName)
        {
            return string.Empty;
        }

        /// <summary>
        ///     Performs an Insert operation
        /// </summary>
        /// <param name="database">The calling Database object</param>
        /// <param name="cmd">The insert command to be executed</param>
        /// <param name="primaryKeyName">The primary key of the table being inserted into</param>
        /// <returns>The ID of the newly inserted record</returns>
        public virtual object ExecuteInsert(Database database, IDbCommand cmd, string primaryKeyName)
        {
            cmd.CommandText += ";\nSELECT @@IDENTITY AS NewID;";
            return database.ExecuteScalarHelper(cmd);
        }

        /// <summary>
        ///     Returns the .net standard conforming DbProviderFactory.
        /// </summary>
        /// <param name="assemblyQualifiedName">The assembly qualified name of the provider factory.</param>
        /// <returns>The db provider factory.</returns>
        /// <exception cref="ArgumentException">Thrown when <paramref name="assemblyQualifiedName" /> does not match a type.</exception>
        protected DbProviderFactory GetFactory(string assemblyQualifiedName)
        {
            var ft = Type.GetType(assemblyQualifiedName);

            if (ft == null)
                throw new ArgumentException("Could not load the " + GetType().Name + " DbProviderFactory.");

            return (DbProviderFactory)ft.GetField("Instance").GetValue(null);
        }

        /// <summary>
        ///     Look at the type and provider name being used and instantiate a suitable DatabaseType instance.
        /// </summary>
        /// <param name="type">The type name.</param>
        /// <param name="allowDefault">A flag that when set allows the default <see cref="SqlServerDatabaseProvider"/> to be returned if not match is found.</param>
        /// <param name="connectionString">The connection string.</param>
        /// <returns>The database provider.</returns>
        internal static IProvider Resolve(Type type, bool allowDefault, string connectionString)
        {
            var typeName = type.Name;

            // Try using type name first (more reliable)
            if (typeName.StartsWith("MySql"))
                return Singleton<MySqlDatabaseProvider>.Instance;
            if (typeName.StartsWith("MariaDb"))
                return Singleton<MariaDbDatabaseProvider>.Instance;
            if (typeName.StartsWith("SqlCe"))
                return Singleton<SqlServerCEDatabaseProviders>.Instance;
            if (typeName.StartsWith("Npgsql") || typeName.StartsWith("PgSql"))
                return Singleton<PostgreSQLDatabaseProvider>.Instance;
            if (typeName.StartsWith("Oracle"))
                return Singleton<OracleDatabaseProvider>.Instance;
            if (typeName.StartsWith("SQLite"))
                return Singleton<SQLiteDatabaseProvider>.Instance;
            if (typeName.Equals("SqlConnection") || typeName.Equals("SqlClientFactory"))
                return Singleton<SqlServerDatabaseProvider>.Instance;
            if (typeName.StartsWith("FbConnection") || typeName.EndsWith("FirebirdClientFactory"))
                return Singleton<FirebirdDbDatabaseProvider>.Instance;
            if (typeName.IndexOf("OleDb", StringComparison.InvariantCultureIgnoreCase) >= 0
                && (connectionString.IndexOf("Jet.OLEDB", StringComparison.InvariantCultureIgnoreCase) > 0 || connectionString.IndexOf("ACE.OLEDB", StringComparison.InvariantCultureIgnoreCase) > 0))
            {
                return Singleton<MsAccessDbDatabaseProvider>.Instance;
            }
            if (!allowDefault)
                throw new ArgumentException("Could not match `" + type.FullName + "` to a provider.", "type");

            // Assume SQL Server
            return Singleton<SqlServerDatabaseProvider>.Instance;
        }

        /// <summary>
        ///     Look at the type and provider name being used and instantiate a suitable DatabaseType instance.
        /// </summary>
        /// <param name="providerName">The provider name.</param>
        /// <param name="allowDefault">A flag that when set allows the default <see cref="SqlServerDatabaseProvider"/> to be returned if not match is found.</param>
        /// <param name="connectionString">The connection string.</param>
        /// <returns>The database type.</returns>
        internal static IProvider Resolve(string providerName, bool allowDefault, string connectionString)
        {
            // Try again with provider name
            if (providerName.IndexOf("MySql", StringComparison.InvariantCultureIgnoreCase) >= 0)
                return Singleton<MySqlDatabaseProvider>.Instance;
            if (providerName.IndexOf("MariaDb", StringComparison.InvariantCultureIgnoreCase) >= 0)
                return Singleton<MariaDbDatabaseProvider>.Instance;
            if (providerName.IndexOf("SqlServerCe", StringComparison.InvariantCultureIgnoreCase) >= 0 ||
                providerName.IndexOf("SqlCeConnection", StringComparison.InvariantCultureIgnoreCase) >= 0)
                return Singleton<SqlServerCEDatabaseProviders>.Instance;
            if (providerName.IndexOf("Npgsql", StringComparison.InvariantCultureIgnoreCase) >= 0
                || providerName.IndexOf("pgsql", StringComparison.InvariantCultureIgnoreCase) >= 0)
                return Singleton<PostgreSQLDatabaseProvider>.Instance;
            if (providerName.IndexOf("Oracle", StringComparison.InvariantCultureIgnoreCase) >= 0)
                return Singleton<OracleDatabaseProvider>.Instance;
            if (providerName.IndexOf("SQLite", StringComparison.InvariantCultureIgnoreCase) >= 0)
                return Singleton<SQLiteDatabaseProvider>.Instance;
            if (providerName.IndexOf("Firebird", StringComparison.InvariantCultureIgnoreCase) >= 0 ||
                providerName.IndexOf("FbConnection", StringComparison.InvariantCultureIgnoreCase) >= 0)
                return Singleton<FirebirdDbDatabaseProvider>.Instance;
            if (providerName.IndexOf("OleDb", StringComparison.InvariantCultureIgnoreCase) >= 0
                && (connectionString.IndexOf("Jet.OLEDB", StringComparison.InvariantCultureIgnoreCase) > 0 || connectionString.IndexOf("ACE.OLEDB", StringComparison.InvariantCultureIgnoreCase) > 0))
            {
                return Singleton<MsAccessDbDatabaseProvider>.Instance;
            }
            if (providerName.IndexOf("SqlServer", StringComparison.InvariantCultureIgnoreCase) >= 0 ||
                providerName.IndexOf("System.Data.SqlClient", StringComparison.InvariantCultureIgnoreCase) >= 0)
                return Singleton<SqlServerDatabaseProvider>.Instance;

            if (!allowDefault)
                throw new ArgumentException("Could not match `" + providerName + "` to a provider.", "providerName");

            // Assume SQL Server
            return Singleton<SqlServerDatabaseProvider>.Instance;
        }

        /// <summary>
        ///     Unwraps a wrapped <see cref="DbProviderFactory"/>.
        /// </summary>
        /// <param name="factory">The factory to unwrap.</param>
        /// <returns>The unwrapped factory or the original factory if no wrapping occurred.</returns>
        internal static DbProviderFactory Unwrap(DbProviderFactory factory)
        {
            var sp = factory as IServiceProvider;

            if (sp == null)
                return factory;

            var unwrapped = sp.GetService(factory.GetType()) as DbProviderFactory;
            return unwrapped == null ? factory : Unwrap(unwrapped);
        }


        #region 扩展
        /// <summary>
        /// 生成 SQL TOP 查询语句
        /// </summary>
        /// <param name="take">要获取记录数</param>
        /// <param name="dist">指定是否返回非重复记录</param>
        /// <param name="selectColumns">要获取的字段名列表</param>
        /// <param name="tableName">表名</param>
        /// <param name="joins">联合子句</param>
        /// <param name="where">条件子句</param>
        /// <param name="orderby">排序子句</param>
        /// <param name="args">SQL 查询用的参数</param>
        /// <returns>最终可以执行的 SQL 查询语句</returns>
        public virtual string BuildTopSql(int take, bool dist, string selectColumns, string tableName, string joins, string where, string orderby, List<object> args)
        {
            var sql = string.Format("SELECT {0}{1} FROM {2}{3} {4}{5} LIMIT @{6}",

                    dist ? "DISTINCT " : string.Empty,
                    selectColumns,
                    tableName,
                    joins,
                    where,
                    orderby,
                    args.Count
                );
            args.Add(take);
            return sql;
        }
        #endregion
    }
}
