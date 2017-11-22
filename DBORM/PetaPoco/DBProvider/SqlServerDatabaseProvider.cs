using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace PetaPoco.DBProvider
{
    public class SqlServerDatabaseProvider : DatabaseProvider
    {
        public override DbProviderFactory GetFactory()
        {
            return GetFactory("System.Data.SqlClient.SqlClientFactory, System.Data, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089");
        }

        private static readonly Regex simpleRegexOrderBy = new Regex(@"\bORDER\s+BY\s+", RegexOptions.RightToLeft | RegexOptions.IgnoreCase | RegexOptions.Multiline | RegexOptions.Singleline | RegexOptions.Compiled);

        public override string BuildPageQuery(long skip, long take, SQLParts parts, ref object[] args)
        {
            var helper = (PagingHelper)PagingUtility;
            // when the query does not contain an "order by", it is very slow
            if (simpleRegexOrderBy.IsMatch(parts.SqlSelectRemoved))
            {
                parts.SqlSelectRemoved = helper.RegexOrderBy.Replace(parts.SqlSelectRemoved, "", 1);
            }
            if (helper.RegexDistinct.IsMatch(parts.SqlSelectRemoved))
            {
                parts.SqlSelectRemoved = "peta_inner.* FROM (SELECT " + parts.SqlSelectRemoved + ") peta_inner";
            }
            var sqlPage = string.Format("SELECT * FROM (SELECT ROW_NUMBER() OVER ({0}) peta_rn, {1}) peta_paged WHERE peta_rn > @{2} AND peta_rn <= @{3}", parts.SqlOrderBy ?? "ORDER BY (SELECT NULL)", parts.SqlSelectRemoved, args.Length, args.Length + 1);
            args = args.Concat(new object[] { skip, skip + take }).ToArray();
            return sqlPage;
        }

        public override object ExecuteInsert(Database db, System.Data.IDbCommand cmd, string primaryKeyName)
        {
            return db.ExecuteScalarHelper(cmd);
        }

        public override string GetExistsSql()
        {
            return "IF EXISTS (SELECT 1 FROM {0} WHERE {1}) SELECT 1 ELSE SELECT 0";
        }

        public override string GetInsertOutputClause(string primaryKeyName)
        {
            return String.Format(" OUTPUT INSERTED.[{0}]", primaryKeyName);
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
