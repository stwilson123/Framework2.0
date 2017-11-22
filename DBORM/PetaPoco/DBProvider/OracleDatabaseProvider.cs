using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;

namespace  PetaPoco.DBProvider
{


    public class OracleDatabaseProvider : DatabaseProvider
    {

        private static readonly Regex simpleRegexOrderBy = new Regex(@"\bORDER\s+BY\s+", RegexOptions.RightToLeft | RegexOptions.IgnoreCase | RegexOptions.Multiline | RegexOptions.Singleline | RegexOptions.Compiled);

        public override string GetParameterPrefix(string connectionString)
        {
            return ":";
        }

        public override void PreExecute(IDbCommand cmd)
        {
            cmd.GetType().GetProperty("BindByName").SetValue(cmd, true, null);
            cmd.GetType().GetProperty("InitialLONGFetchSize").SetValue(cmd, -1, null);
        }

        public override string BuildPageQuery(long skip, long take, SQLParts parts, ref object[] args)
        {
            if (parts.SqlSelectRemoved.StartsWith("*"))
                throw new Exception("Query must alias '*' when performing a paged query.\neg. select t.* from table t order by t.id");


            var helper = (PagingHelper)PagingUtility;
            // when the query does not contain an "order by", it is very slow
            if (simpleRegexOrderBy.IsMatch(parts.SqlSelectRemoved))
            {
                parts.SqlSelectRemoved = helper.RegexOrderBy.Replace(parts.SqlSelectRemoved, "", 1);
            }
            //if (helper.RegexDistinct.IsMatch(parts.SqlSelectRemoved))
            //{
            //    parts.SqlSelectRemoved = "peta_inner.* FROM (SELECT " + parts.SqlSelectRemoved + ") peta_inner";
            //}
            var sqlPage = string.Empty;
            if (helper.RegexDistinct.IsMatch(parts.SqlSelectRemoved))
            {
                string SqlSelectRemoved = parts.SqlSelectRemoved.Substring(parts.SqlSelectRemoved.IndexOf("DISTINCT", StringComparison.OrdinalIgnoreCase) + "DISTINCT".Length);
                sqlPage = string.Format("SELECT * FROM (SELECT DISTINCT ROW_NUMBER() OVER ({0}) peta_rn,{1} ) peta_paged WHERE peta_rn > @{2} AND peta_rn <= @{3}", parts.SqlOrderBy ?? "ORDER BY (SELECT NULL)", SqlSelectRemoved, args.Length, args.Length + 1);
            }
            else
            {
                sqlPage = string.Format("SELECT * FROM (SELECT ROW_NUMBER() OVER ({0}) peta_rn, {1}) peta_paged WHERE peta_rn > @{2} AND peta_rn <= @{3}", parts.SqlOrderBy ?? "ORDER BY (NULL)", parts.SqlSelectRemoved, args.Length, args.Length + 1);
            }
            args = args.Concat(new object[] { skip, skip + take }).ToArray();
            return sqlPage;
             
            
            //Same deal as SQL Server
             //return Singleton<SqlServerDatabaseProvider>.Instance.BuildPageQuery(skip, take, parts, ref args);
        }

        public override DbProviderFactory GetFactory()
        {
            return GetFactory("Oracle.ManagedDataAccess.Client.OracleClientFactory, Oracle.ManagedDataAccess, Culture=neutral, PublicKeyToken=89b483f429c47342");
        }

        public override string EscapeSqlIdentifier(string sqlIdentifier)
        {
            return string.Format("\"{0}\"", sqlIdentifier);
        }

        public override string GetAutoIncrementExpression(TableInfo ti)
        {
            if (!string.IsNullOrEmpty(ti.SequenceName))
                return string.Format("{0}.nextval", ti.SequenceName);

            return null;
        }

        public override object ExecuteInsert(Database db, IDbCommand cmd, string primaryKeyName)
        {
            if (primaryKeyName != null)
            {
                cmd.CommandText += string.Format(" returning {0} into :newid", EscapeSqlIdentifier(primaryKeyName));
                var param = cmd.CreateParameter();
                param.ParameterName = ":newid";
                param.Value = DBNull.Value;
                param.Direction = ParameterDirection.ReturnValue;
                param.DbType = DbType.Int64;
                cmd.Parameters.Add(param);
                db.ExecuteNonQueryHelper(cmd);
                return param.Value;
            }
            else
            {
                db.ExecuteNonQueryHelper(cmd);
                return -1;
            }
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
        public override string BuildTopSql(int take, bool dist, string selectColumns, string tableName, string joins, string where, string orderby, System.Collections.Generic.List<object> args)
        {
            var sql = string.Format("SELECT {0} FROM (SELECT {1}{0} FROM {2}{3} {4}{5}) WHERE ROWNUM <= @{6} ORDER BY ROWNUM ASC",
                
                    selectColumns,
                    dist ? "DISTINCT " : string.Empty,
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
