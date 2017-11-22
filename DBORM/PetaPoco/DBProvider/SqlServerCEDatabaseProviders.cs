using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;

namespace PetaPoco.DBProvider
{
    public class SqlServerCEDatabaseProviders : DatabaseProvider
    {
        public override DbProviderFactory GetFactory()
        {
            return GetFactory("System.Data.SqlServerCe.SqlCeProviderFactory, System.Data.SqlServerCe, Culture=neutral, PublicKeyToken=89845dcd8080cc91");
        }

        public override string BuildPageQuery(long skip, long take, SQLParts parts, ref object[] args)
        {
            if (string.IsNullOrEmpty(parts.SqlOrderBy))
                parts.Sql += " ORDER BY ABS(1)";
            var sqlPage = string.Format("{0}\nOFFSET @{1} ROWS FETCH NEXT @{2} ROWS ONLY", parts.Sql, args.Length, args.Length + 1);
            args = args.Concat(new object[] { skip, take }).ToArray();
            return sqlPage;
        }

        public override object ExecuteInsert(Database db, System.Data.IDbCommand cmd, string primaryKeyName)
        {
            db.ExecuteNonQueryHelper(cmd);
            return db.ExecuteScalar<object>("SELECT @@@IDENTITY AS NewID;");
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
        public override string BuildTopSql(int take, bool dist, string selectColumns, string tableName, string joins, string where, string orderby, List<object> args)
        {
            var sql = string.Format("SELECT {0}TOP (@{1}) {2} FROM {3}{4} {5}{6}",
                 
                    dist ? "DISTINCT " : string.Empty,
                    args.Count,
                    selectColumns,
                    tableName,
                    joins,
                    where,
                    orderby
                );
            args.Add(take);
            return sql;
        }
        #endregion
    }
}
