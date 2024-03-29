﻿// ***********************************************************************
// 程序集			: DBORM
// 文件名			: DeleteBuilder.cs
// 作者				: 何权洲
// 创建时间			: 2013-08-05
//
// 最后修改者		: 何权洲
// 最后修改时间		: 2013-11-05
// ***********************************************************************

using PetaPoco;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Framework.Util.BaseTypeMethodExpend;
using DBORM.PetaPoco.Extend;

/// <summary>
/// Data 命名空间
/// </summary>
namespace DBORM
{
	/// <summary>
	/// 删除构建器
	/// </summary>
	/// <typeparam name="T">实体类型</typeparam>
	public class DeleteBuilder<T> : SqlBuilder where T : new()
	{
		private IDatabase __db;

		/// <summary>
		/// 初始化一个新 <see cref="DeleteBuilder&lt;T&gt;" /> 实例。
		/// </summary>
		/// <param name="db">数据库实例</param>
		/// <param name="args">参数</param>
		public DeleteBuilder(IDatabase db, List<object> args = null)
		{
			__db = db;
			this.Params = args ?? new List<object>();
		}

		/// <summary>
		/// 获取参数
		/// </summary>
		/// <value>参数集合</value>
		public List<object> Params { get; private set; }

        private WhereBuilder __where;
        #region WHERE
        /// <summary>
        /// 查询条件
        /// </summary>
        /// <param name="expression">表达式</param>
        /// <returns>删除构建器</returns>
        public DeleteBuilder<T> Where(Expression expression)
        {
            if (__where == null)
                __where = new WhereBuilder(__db, this.Params, false);
            __where.Append(expression);

            return this;
        }

        /// <summary>
        /// 查询条件
        /// </summary>
        /// <param name="expression">表达式</param>
        /// <returns>删除构建器</returns>
        public DeleteBuilder<T> Where(Expression<Func<T, bool>> expression)
        {
            return this.Where((Expression)expression);
        }

        /// <summary>
        /// 查询条件
        /// </summary>
        /// <param name="property">字段栏表达式</param>
        /// <param name="op">比较运算符</param>
        /// <param name="value">值</param>
        /// <returns>删除构建器</returns>
        public DeleteBuilder<T> Where(Expression property, QueryOperatorType op, object value)
        {
            if (__where == null)
                __where = new WhereBuilder(__db, this.Params, false);
            __where.Append(property, op, value);

            return this;
        }

        /// <summary>
        /// 查询条件
        /// </summary>
        /// <param name="property">字段栏表达式</param>
        /// <param name="op">比较运算符</param>
        /// <param name="value">值</param>
        /// <returns>删除构建器</returns>
        public DeleteBuilder<T> Where(Expression<Func<T, object>> property, QueryOperatorType op, object value)
        {
            return this.Where((Expression)property, op, value);
        }
        #endregion

        #region WHERE OR
        /// <summary>
        /// 查询条件
        /// </summary>
        /// <param name="expression">表达式</param>
        /// <returns>删除构建器</returns>
        public DeleteBuilder<T> WhereOr(Expression expression)
        {
            if (__where == null)
                __where = new WhereBuilder(__db, this.Params, false);
            __where.AppendOr(expression);

            return this;
        }

        /// <summary>
        /// 查询条件
        /// </summary>
        /// <param name="expression">表达式</param>
        /// <returns>删除构建器</returns>
        public DeleteBuilder<T> WhereOr(Expression<Func<T, bool>> expression)
        {
            return this.WhereOr((Expression)expression);
        }

        /// <summary>
        /// 查询条件
        /// </summary>
        /// <param name="property">字段栏表达式</param>
        /// <param name="op">比较运算符</param>
        /// <param name="value">值</param>
        /// <returns>删除构建器</returns>
        public DeleteBuilder<T> WhereOr(Expression property, QueryOperatorType op, object value)
        {
            if (__where == null)
                __where = new WhereBuilder(__db, this.Params, false);
            __where.AppendOr(property, op, value);

            return this;
        }

        /// <summary>
        /// 查询条件
        /// </summary>
        /// <param name="property">字段栏表达式</param>
        /// <param name="op">比较运算符</param>
        /// <param name="value">值</param>
        /// <returns>删除构建器</returns>
        public DeleteBuilder<T> WhereOr(Expression<Func<T, object>> property, QueryOperatorType op, object value)
        {
            return this.WhereOr((Expression)property, op, value);
        }
        #endregion

        #region 内部操作
        private string GetWhere(bool prefix = true)
        {
            if (__where == null) return string.Empty;
            if (prefix)
                return "WHERE " + __where.ToString();
            return __where.ToString();
        }
        private object[] GetParams()
        {
            return this.Params.ToArray();
        }
        #endregion

        #region 执行
        /// <summary>
        /// 执行操作
        /// </summary>
        /// <returns>受影响的行数</returns>
        public int Execute()
        {
            var sql = GetSql();
            if (string.IsNullOrEmpty(sql)) return 0;
           
            return __db.Execute(sql, this.GetParams());
        }

        /// <summary>
        /// 返回是否执行成功
        /// </summary>
        /// <returns>指示是否执行成功</returns>
        public bool Succeed()
        {
            return this.Execute() > 0;
        }
        #endregion

        public string GetSql()
        {
            if (__where == null) return string.Empty;

            var sql = "DELETE FROM {0} {1}".F(__db.GetTableName<T>(), this.GetWhere());
            return sql;
        }
    }
}