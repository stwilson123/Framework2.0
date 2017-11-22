// ***********************************************************************
// 程序集			: DBORM
// 文件名			: GetBuilder.cs
// 作者				: 何权洲
// 创建时间			: 2013-08-05
//
// 最后修改者		: 何权洲
// 最后修改时间		: 2013-11-05
// ***********************************************************************

using PetaPoco;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using Framework.Util.BaseTypeMethodExpend;
using DBORM.PetaPoco.Extend;
/// <summary>
/// Data 命名空间
/// </summary>
namespace DBORM
{
	/// <summary>
	/// 查询构建器基类。内部使用，不能直接初始化此类。
	/// </summary>
	public interface SqlBuilder
	{
	     string GetSql();
	}

	/// <summary>
	/// 查询构建器
	/// </summary>
	/// <typeparam name="T">实体类型</typeparam>
    public class GetBuilder<T> : SqlBuilder where T : new()
    {
		private IDatabase __db;
		private string __table;

		/// <summary>
		/// 初始化一个新 <see cref="GetBuilder&lt;T&gt;" /> 实例。
		/// </summary>
		/// <param name="db">数据库实例</param>
		/// <param name="args">参数</param>
        public GetBuilder(IDatabase db, List<object> args = null)
		{
			__db = db;
            __table = PocoData.ForType(typeof(T), db.DefaultMapper).TableInfo.TableName;  
			this.Params = args ?? new List<object>();
		}

		/// <summary>
		/// 获取参数
		/// </summary>
		/// <value>参数集合</value>
		public List<object> Params { get; private set; }

        #region JOIN

        private List<JoinBuilder> __listJoin = new List<JoinBuilder>();
        /// <summary>
        /// 内连其它实体
        /// </summary>
        /// <typeparam name="TJoin">连接的实体类型</typeparam>
        /// <param name="expression">连接表达式</param>
        /// <returns>查询构建器</returns>
        public GetBuilder<T> InnerJoin<TJoin>(Expression<Func<T, TJoin, bool>> expression) where TJoin : new()
		{
			return this.InternalJoin<TJoin>("INNER JOIN", expression);
		}

		/// <summary>
		/// 内连其它实体
		/// </summary>
		/// <typeparam name="TJoin">连接的实体类型</typeparam>
		/// <typeparam name="TEntity">设置关联的实体类型</typeparam>
		/// <param name="expression">连接表达式</param>
		/// <returns>查询构建器</returns>
        public GetBuilder<T> InnerJoin<TEntity, TJoin>(Expression<Func<TEntity, TJoin, bool>> expression)
			where TJoin : new()
			where TEntity : new()
		{
			return this.InternalJoin<TJoin>("INNER JOIN", expression);
		}

		/// <summary>
		/// 左连其它实体
		/// </summary>
		/// <typeparam name="TJoin">连接的实体类型</typeparam>
		/// <param name="expression">连接表达式</param>
		/// <returns>查询构建器</returns>
		public GetBuilder<T> LeftJoin<TJoin>(Expression<Func<T, TJoin, bool>> expression) where TJoin : new()
		{
			return this.InternalJoin<TJoin>("LEFT JOIN", expression);
		}

		/// <summary>
		/// 左连其它实体
		/// </summary>
		/// <typeparam name="TJoin">连接的实体类型</typeparam>
		/// <typeparam name="TEntity">设置关联的实体类型</typeparam>
		/// <param name="expression">连接表达式</param>
		/// <returns>查询构建器</returns>
        public GetBuilder<T> LeftJoin<TEntity, TJoin>(Expression<Func<TEntity,TJoin, bool>> expression)
			where TJoin : new()
			where TEntity : new()
		{
			return this.InternalJoin<TJoin>("LEFT JOIN", expression);
		}

		/// <summary>
		/// 右连其它实体
		/// </summary>
		/// <typeparam name="TJoin">连接的实体类型</typeparam>
		/// <param name="expression">连接表达式</param>
		/// <returns>查询构建器</returns>
		public GetBuilder<T> RightJoin<TJoin>(Expression<Func<T, TJoin, bool>> expression) where TJoin : new()
		{
			return this.InternalJoin<TJoin>("RIGHT JOIN", expression);
		}

		/// <summary>
		/// 右连其它实体
		/// </summary>
		/// <typeparam name="TJoin">连接的实体类型</typeparam>
		/// <typeparam name="TEntity">设置关联的实体类型</typeparam>
		/// <param name="expression">连接表达式</param>
		/// <returns>查询构建器</returns>
        public GetBuilder<T> RightJoin<TEntity, TJoin>(Expression<Func<TEntity,TJoin, bool>> expression)
			where TJoin : new()
			where TEntity : new()
		{
			return this.InternalJoin<TJoin>("RIGHT JOIN", expression);
		}

		 
		private bool __hasJoin;
		private GetBuilder<T> InternalJoin<TJoin>(string join, Expression expression) where TJoin : new()
		{
			if((expression as LambdaExpression) == null || ((expression as LambdaExpression).Body as BinaryExpression) == null)
				throw new InvalidOperationException();
			var be = (expression as LambdaExpression).Body as BinaryExpression;
             
            //if (!__hasJoin)
            //{
            //    var _join = new JoinBuilder(__db, string.Format(" {0} {1} ON ", join, __db.GetTableName<TJoin>()));
            //    _join.Append(expression);

            //    __listJoin.Add(_join);
            //}
            //else
            //{
            //    var _join = new JoinBuilder(__db, string.Format(" {0} {1} ON ", join, __db.GetTableName<TJoin>()));

            //    _join.Append(expression);

            //    __listJoin.Add(_join);
            //}

            var _join = new JoinBuilder(__db, string.Format(" {0} {1} ON ", join, __db.GetTableName<TJoin>()),this.Params,true);

            _join.Append(expression);

            __listJoin.Add(_join);
            //__join.AppendFormat(" {0} {1} ON {2} {3} {4}",
            //	join, __db.GetTableName<TJoin>(),
            //	__db.GetTableAndColumnName(be.Left),
            //	this.GetOperant(be.NodeType),
            //	__db.GetTableAndColumnName(be.Right));



            __hasJoin = true;
			return this;
		}

		private string GetOperant(ExpressionType expType)
		{
			switch(expType)
			{
				case ExpressionType.Equal:
					return "=";
				case ExpressionType.GreaterThan:
					return ">";
				case ExpressionType.GreaterThanOrEqual:
					return ">=";
				case ExpressionType.LessThan:
					return "<";
				case ExpressionType.LessThanOrEqual:
					return "<=";
				case ExpressionType.NotEqual:
					return "<>";
				default:
                    throw new NotSupportedException(string.Format("ExpressionType:{0} is not supported", expType.ToString()));
			}
		}
		#endregion

		#region WHERE
		private WhereBuilder __where;
		/// <summary>
		/// 查询条件
		/// </summary>
		/// <param name="expression">表达式</param>
		/// <returns>查询构建器</returns>
		public GetBuilder<T> Where(Expression expression)
		{
			if(__where == null)
				__where = new WhereBuilder(__db, this.Params);
			__where.Append(expression);

			return this;
		}

		/// <summary>
		/// 查询条件
		/// </summary>
		/// <param name="property">字段栏表达式</param>
		/// <param name="op">比较运算符</param>
		/// <param name="value">值</param>
		/// <returns>查询构建器</returns>
		public GetBuilder<T> Where(Expression property, QueryOperatorType op, object value)
		{
			if(__where == null)
				__where = new WhereBuilder(__db, this.Params);
			__where.Append(property, op, value);

			return this;
		}

		/// <summary>
		/// 查询条件
		/// </summary>
		/// <param name="property">字段栏表达式</param>
		/// <param name="op">比较运算符</param>
		/// <param name="value">值</param>
		/// <returns>查询构建器</returns>
		public GetBuilder<T> Where(Expression<Func<T, object>> property, QueryOperatorType op, object value)
		{
			return this.Where((Expression)property, op, value);
		}

		/// <summary>
		/// 查询条件
		/// </summary>
		/// <typeparam name="T1">The type of the 1.</typeparam>
		/// <param name="property">字段栏表达式</param>
		/// <param name="op">比较运算符</param>
		/// <param name="value">值</param>
		/// <returns>查询构建器</returns>
		public GetBuilder<T> Where<T1>(Expression<Func<T1, object>> property, QueryOperatorType op, object value) where T1 : new()
		{
			return this.Where((Expression)property, op, value);
		}

		/// <summary>
		/// 查询条件
		/// </summary>
		/// <param name="expression">表达式</param>
		/// <returns>查询构建器</returns>
		public GetBuilder<T> Where(Expression<Func<T, bool>> expression)
		{
			return this.Where((Expression)expression);
		}

		/// <summary>
		/// 查询条件
		/// </summary>
		/// <typeparam name="T1">实体类型</typeparam>
		/// <param name="expression">表达式</param>
		/// <returns>查询构建器</returns>
		public GetBuilder<T> Where<T1>(Expression<Func<T1, bool>> expression) where T1 : new()
		{
			return this.Where((Expression)expression);
		}

		/// <summary>
		/// 查询条件
		/// </summary>
		/// <typeparam name="T1">实体类型</typeparam>
		/// <param name="expression">表达式</param>
		/// <returns>查询构建器</returns>
		public GetBuilder<T> Where<T1>(Expression<Func<T, T1, bool>> expression) where T1 : new()
		{
			return this.Where((Expression)expression);
		}

		/// <summary>
		/// 查询条件
		/// </summary>
		/// <typeparam name="T1">实体类型 1</typeparam>
		/// <typeparam name="T2">实体类型 2</typeparam>
		/// <param name="expression">表达式</param>
		/// <returns>查询构建器</returns>
		public GetBuilder<T> Where<T1, T2>(Expression<Func<T1, T2, bool>> expression)
			where T1 : new()
			where T2 : new()
		{
			return this.Where((Expression)expression);
		}

        /// <summary>
        /// 查询条件
        /// </summary>
        /// <typeparam name="T1">实体类型 1</typeparam>
        /// <typeparam name="T2">实体类型 2</typeparam>
        /// <param name="expression">表达式</param>
        /// <returns>查询构建器</returns>
        public GetBuilder<T> Where<T1, T2, T3>(Expression<Func<T1, T2,T3, bool>> expression)
            where T1 : new()
            where T2 : new()
            where T3 : new()
        {
            return this.Where((Expression)expression);
        }



		/// <summary>
		/// 查询条件
		/// </summary>
		/// <typeparam name="T1">实体类型 1</typeparam>
		/// <typeparam name="T2">实体类型 2</typeparam>
		/// <param name="expression">表达式</param>
		/// <returns>查询构建器</returns>
		public GetBuilder<T> Where<T1, T2>(Expression<Func<T, T1, T2, bool>> expression)
			where T1 : new()
			where T2 : new()
		{
			return this.Where((Expression)expression);
		}
		#endregion

		#region WHERE OR
		/// <summary>
		/// 查询条件
		/// </summary>
		/// <param name="expression">表达式</param>
		/// <returns>查询构建器</returns>
		public GetBuilder<T> WhereOr(Expression expression)
		{
			if(__where == null)
				__where = new WhereBuilder(__db, this.Params);
			__where.AppendOr(expression);

			return this;
		}

		/// <summary>
		/// 查询条件
		/// </summary>
		/// <param name="property">字段栏表达式</param>
		/// <param name="op">比较运算符</param>
		/// <param name="value">值</param>
		/// <returns>查询构建器</returns>
		public GetBuilder<T> WhereOr(Expression property, QueryOperatorType op, object value)
		{
			if(__where == null)
				__where = new WhereBuilder(__db, this.Params);
			__where.AppendOr(property, op, value);

			return this;
		}

		/// <summary>
		/// 查询条件
		/// </summary>
		/// <param name="property">字段栏表达式</param>
		/// <param name="op">比较运算符</param>
		/// <param name="value">值</param>
		/// <returns>查询构建器</returns>
		public GetBuilder<T> WhereOr(Expression<Func<T, object>> property, QueryOperatorType op, object value)
		{
			return this.WhereOr((Expression)property, op, value);
		}

		/// <summary>
		/// 查询条件
		/// </summary>
		/// <typeparam name="T1">The type of the 1.</typeparam>
		/// <param name="property">字段栏表达式</param>
		/// <param name="op">比较运算符</param>
		/// <param name="value">值</param>
		/// <returns>查询构建器</returns>
		public GetBuilder<T> WhereOr<T1>(Expression<Func<T1, object>> property, QueryOperatorType op, object value) where T1 : new()
		{
			return this.WhereOr((Expression)property, op, value);
		}

		/// <summary>
		/// 查询条件
		/// </summary>
		/// <param name="expression">表达式</param>
		/// <returns>查询构建器</returns>
		public GetBuilder<T> WhereOr(Expression<Func<T, bool>> expression)
		{
			return this.WhereOr((Expression)expression);
		}

		/// <summary>
		/// 查询条件
		/// </summary>
		/// <typeparam name="T1">实体类型</typeparam>
		/// <param name="expression">表达式</param>
		/// <returns>查询构建器</returns>
		public GetBuilder<T> WhereOr<T1>(Expression<Func<T1, bool>> expression) where T1 : new()
		{
			return this.WhereOr((Expression)expression);
		}

		/// <summary>
		/// 查询条件
		/// </summary>
		/// <typeparam name="T1">实体类型</typeparam>
		/// <param name="expression">表达式</param>
		/// <returns>查询构建器</returns>
		public GetBuilder<T> WhereOr<T1>(Expression<Func<T, T1, bool>> expression) where T1 : new()
		{
			return this.WhereOr((Expression)expression);
		}

		/// <summary>
		/// 查询条件
		/// </summary>
		/// <typeparam name="T1">实体类型 1</typeparam>
		/// <typeparam name="T2">实体类型 2</typeparam>
		/// <param name="expression">表达式</param>
		/// <returns>查询构建器</returns>
		public GetBuilder<T> WhereOr<T1, T2>(Expression<Func<T1, T2, bool>> expression)
			where T1 : new()
			where T2 : new()
		{
			return this.WhereOr((Expression)expression);
		}

		/// <summary>
		/// 查询条件
		/// </summary>
		/// <typeparam name="T1">实体类型 1</typeparam>
		/// <typeparam name="T2">实体类型 2</typeparam>
		/// <param name="expression">表达式</param>
		/// <returns>查询构建器</returns>
		public GetBuilder<T> WhereOr<T1, T2>(Expression<Func<T, T1, T2, bool>> expression)
			where T1 : new()
			where T2 : new()
		{
			return this.WhereOr((Expression)expression);
		}
		#endregion

		#region SELECT
        private SelectBuilder __select;
        /// <summary>
        /// 查询条件
        /// </summary>
        /// <param name="expression">表达式</param>
        /// <returns>查询构建器</returns>
        public GetBuilder<T> SelectCode(Expression expression)
        {
            if (__select == null)
                __select = new SelectBuilder(__db);
            __select.Append(expression);

            return this;
        }
		/// <summary>
		/// 获取所有字段
		/// </summary>
		/// <returns>查询构建器</returns>
		public GetBuilder<T> SelectAll()
		{
            __select.Select<T>();

			return this;
		}

        ///// <summary>
        ///// 添加要获取的字段
        ///// </summary>
        ///// <param name="expression">表达式</param>
        ///// <returns>查询构建器</returns>
        //public GetBuilder<T> Select(params Expression<Func<T, object>>[] expression)
        //{
        //    //expression.ForEach_(
        //    //    e => this.Select(expression)
        //    //    );
        //    this.Select(expression);
        //    return this;
        //}

        /// <summary>
        /// 添加要获取的字段
        /// </summary>
        /// <param name="expression">表达式</param>
        /// <returns>查询构建器</returns>
        public GetBuilder<T> Select(Expression<Func<T, object>> expression)
        {
            //expression.(
            //    e => this.Select(__db.GetTableAndColumnName(e))
            //    );
            //__db.GetTableAndColumnNameByObject(expression).ForEach_(
            //    colname => this.Select(colname)
            //    );
            //;
            this.SelectCode(expression);
            return this;
        }

        /// <summary>
        /// 添加要获取的字段
        /// </summary>
        /// <param name="expression">表达式</param>
        /// <returns>查询构建器</returns>
        public GetBuilder<T> Select<TEntity>(Expression<Func<T,TEntity, object>> expression)
        {
            //expression.(
            //    e => this.Select(__db.GetTableAndColumnName(e))
            //    );
            //__db.GetTableAndColumnNameByObject(expression).ForEach_(
            //    colname => this.Select(colname)
            //    );
            this.SelectCode(expression);
            return this;
        }

        /// <summary>
        /// 添加要获取的字段
        /// </summary>
        /// <param name="expression">表达式</param>
        /// <returns>查询构建器</returns>
        public GetBuilder<T> Select<TEntity,TEntity1>(Expression<Func<T, TEntity,TEntity1, object>> expression)
        {
            //expression.(
            //    e => this.Select(__db.GetTableAndColumnName(e))
            //    );
            //__db.GetTableAndColumnNameByObject(expression).ForEach_(
            //    colname => this.Select(colname)
            //    );
            this.SelectCode(expression);
            return this;
        }

        /// <summary>
        /// 添加要获取的字段
        /// </summary>
        /// <param name="expression">表达式</param>
        /// <returns>查询构建器</returns>
        public GetBuilder<T> Select<TEntity, TEntity1, TEntity2>(Expression<Func<T, TEntity, TEntity1, TEntity2,object>> expression)
        {
            //expression.(
            //    e => this.Select(__db.GetTableAndColumnName(e))
            //    );
            //__db.GetTableAndColumnNameByObject(expression).ForEach_(
            //    colname => this.Select(colname)
            //    );
            this.SelectCode(expression);

            return this;
        }

        /// <summary>
        /// 添加要获取的字段
        /// </summary>
        /// <param name="expression">表达式</param>
        /// <returns>查询构建器</returns>
        public GetBuilder<T> Select<TEntity, TEntity1, TEntity2, TEntity3>(Expression<Func<T, TEntity, TEntity1, TEntity2, TEntity3,object>> expression)
        {
            //expression.(
            //    e => this.Select(__db.GetTableAndColumnName(e))
            //    );
            //__db.GetTableAndColumnNameByObject(expression).ForEach_(
            //    colname => this.Select(colname)
            //    );
            this.SelectCode(expression);

            return this;
        }
        /// <summary>
        /// 添加要获取的字段
        /// </summary>
        /// <param name="expression">表达式</param>
        /// <returns>查询构建器</returns>
        public GetBuilder<T> Select<TEntity, TEntity1, TEntity2, TEntity3, TEntity4>(Expression<Func<T, TEntity, TEntity1, TEntity2, TEntity3, TEntity4, object>> expression)
        {
            //expression.(
            //    e => this.Select(__db.GetTableAndColumnName(e))
            //    );
            //__db.GetTableAndColumnNameByObject(expression).ForEach_(
            //    colname => this.Select(colname)
            //    );
            this.SelectCode(expression);

            return this;
        }

        /// <summary>
        /// 添加要获取的字段
        /// </summary>
        /// <param name="expression">表达式</param>
        /// <returns>查询构建器</returns>
        public GetBuilder<T> Select<TEntity, TEntity1, TEntity2, TEntity3, TEntity4, TEntity5>(Expression<Func<T, TEntity, TEntity1, TEntity2, TEntity3, TEntity4, TEntity5, object>> expression)
        {
            //expression.(
            //    e => this.Select(__db.GetTableAndColumnName(e))
            //    );
            //__db.GetTableAndColumnNameByObject(expression).ForEach_(
            //    colname => this.Select(colname)
            //    );
            this.SelectCode(expression);

            return this;
        }

        /// <summary>
        /// 添加要获取的字段
        /// </summary>
        /// <param name="expression">表达式</param>
        /// <returns>查询构建器</returns>
        public GetBuilder<T> Select<TEntity, TEntity1, TEntity2, TEntity3, TEntity4, TEntity5, TEntity6>(Expression<Func<T, TEntity, TEntity1, TEntity2, TEntity3, TEntity4, TEntity5, TEntity6, object>> expression)
        {
            //expression.(
            //    e => this.Select(__db.GetTableAndColumnName(e))
            //    );
            //__db.GetTableAndColumnNameByObject(expression).ForEach_(
            //    colname => this.Select(colname)
            //    );
            this.SelectCode(expression);

            return this;
        }


        /// <summary>
        /// 添加要获取的字段
        /// </summary>
        /// <param name="expression">表达式</param>
        /// <returns>查询构建器</returns>
        public GetBuilder<T> Select<TEntity, TEntity1, TEntity2, TEntity3, TEntity4, TEntity5, TEntity6, TEntity7>(Expression<Func<T, TEntity, TEntity1, TEntity2, TEntity3, TEntity4, TEntity5, TEntity6, TEntity7, object>> expression)
        {
            //expression.(
            //    e => this.Select(__db.GetTableAndColumnName(e))
            //    );
            //__db.GetTableAndColumnNameByObject(expression).ForEach_(
            //    colname => this.Select(colname)
            //    );
            this.SelectCode(expression);

            return this;
        }

        /// <summary>
        /// 添加要获取的字段
        /// </summary>
        /// <param name="expression">表达式</param>
        /// <returns>查询构建器</returns>
        public GetBuilder<T> Select<TEntity, TEntity1, TEntity2, TEntity3, TEntity4, TEntity5, TEntity6, TEntity7, TEntity8>(Expression<Func<T, TEntity, TEntity1, TEntity2, TEntity3, TEntity4, TEntity5, TEntity6, TEntity7, TEntity8, object>> expression)
        {
            //expression.(
            //    e => this.Select(__db.GetTableAndColumnName(e))
            //    );
            //__db.GetTableAndColumnNameByObject(expression).ForEach_(
            //    colname => this.Select(colname)
            //    );
            this.SelectCode(expression);

            return this;


        }


        /// <summary>
        /// 添加要获取的字段
        /// </summary>
        /// <param name="expression">表达式</param>
        /// <returns>查询构建器</returns>
        public GetBuilder<T> Select<TEntity, TEntity1, TEntity2, TEntity3, TEntity4, TEntity5, TEntity6, TEntity7, TEntity8, TEntity9>(Expression<Func<T, TEntity, TEntity1, TEntity2, TEntity3, TEntity4, TEntity5, TEntity6, TEntity7, TEntity8, TEntity9, object>> expression)
        {
            //expression.(
            //    e => this.Select(__db.GetTableAndColumnName(e))
            //    );
            //__db.GetTableAndColumnNameByObject(expression).ForEach_(
            //    colname => this.Select(colname)
            //    );
            this.SelectCode(expression);

            return this;


        }
        ///// <summary>
        ///// 添加要获取的字段
        ///// </summary>
        ///// <typeparam name="TEntity">实体类型</typeparam>
        ///// <param name="expression">表达式</param>
        ///// <returns>查询构建器</returns>
        //private GetBuilder<T> Select<TEntity>(params Expression<Func<TEntity, object>>[] expression) where TEntity : new()
        //{
        //    //expression.ForEach_(
        //    //    e => this.Select(__db.GetTableAndColumnName(e))
        //    //    );
        //    expression.ForEach_(
        //        e => this.Select(e)
        //        );

        //    return this;
        //}

        /// <summary>
        /// 添加要获取的字段
        /// </summary>
        /// <typeparam name="TEntity">实体类型</typeparam>
        /// <param name="expression">表达式</param>
        /// <returns>查询构建器</returns>
        public GetBuilder<T> Select<TEntity>(Expression<Func<TEntity, object>> expression) 
		{
            //expression.ForEach_(
            //    e => this.Select(__db.GetTableAndColumnName(e))
            //    );
            //__db.GetTableAndColumnNameByObject(expression).ForEach_(
            // colname => this.Select(colname)
            // );
            this.SelectCode(expression);

			return this;
		}

        /// <summary>
        /// 添加要获取的字段
        /// </summary>
        /// <param name="expression">表达式</param>
        /// <returns>查询构建器</returns>
        public GetBuilder<T> Select<TEntity>(Expression<Func<T, object>> expression, Expression<Func<TEntity, object>> expression1)
        {
            this.Select(expression);
            this.Select(expression1);
          
            //__db.GetTableAndColumnNameByObject(expression).ForEach_(
            //    colname => this.Select(colname)
            //    );
            //__db.GetTableAndColumnNameByObject(expression1).ForEach_(
            // colname => this.Select(colname)
            // );

            return this;
        }
        /// <summary>
        /// 添加要获取的字段
        /// </summary>
        /// <param name="expression">表达式</param>
        /// <returns>查询构建器</returns>
        public GetBuilder<T> Select<TEntity, TEntity1>(Expression<Func<T, object>> expression, Expression<Func<TEntity, object>> expressionTEntity,
            Expression<Func<TEntity1, object>> expressionTEntity1)
        {
            //__db.GetTableAndColumnNameByObject(expression).ForEach_(
            //    colname => this.Select(colname)
            //    );
            //__db.GetTableAndColumnNameByObject(expressionTEntity).ForEach_(
            //    colname => this.Select(colname)
            //    );
            //__db.GetTableAndColumnNameByObject(expressionTEntity1).ForEach_(
            //   colname => this.Select(colname)
            //   );


            this.Select(expression);
            this.Select(expressionTEntity);
            this.Select(expressionTEntity1);

            return this;
        }

        /// <summary>
        /// 添加要获取的字段
        /// </summary>
        /// <param name="expression">表达式</param>
        /// <returns>查询构建器</returns>
        public GetBuilder<T> Select<TEntity, TEntity1, TEntity2>(Expression<Func<T, object>> expression, Expression<Func<TEntity, object>> expressionTEntity,
            Expression<Func<TEntity1, object>> expressionTEntity1, Expression<Func<TEntity2, object>> expressionTEntity2)
        {
            //__db.GetTableAndColumnNameByObject(expression).ForEach_(
            //    colname => this.Select(colname)
            //    );
            //__db.GetTableAndColumnNameByObject(expressionTEntity).ForEach_(
            //    colname => this.Select(colname)
            //    );
            //__db.GetTableAndColumnNameByObject(expressionTEntity1).ForEach_(
            //   colname => this.Select(colname)
            //   );
            //__db.GetTableAndColumnNameByObject(expressionTEntity2).ForEach_(
            //colname => this.Select(colname)
            //);

            this.Select(expression);
            this.Select(expressionTEntity);
            this.Select(expressionTEntity1);
            this.Select(expressionTEntity2);

            return this;
        }

        /// <summary>
        /// 添加要获取的字段
        /// </summary>
        /// <param name="expression">表达式</param>
        /// <returns>查询构建器</returns>
        public GetBuilder<T> Select<TEntity, TEntity1, TEntity2, TEntity3>(Expression<Func<T, object>> expression,Expression<Func<TEntity, object>> expressionTEntity, 
            Expression<Func<TEntity1, object>> expressionTEntity1,Expression<Func<TEntity2, object>> expressionTEntity2 ,Expression<Func<TEntity3, object>> expressionTEntity3)
        {
           // __db.GetTableAndColumnNameByObject(expression).ForEach_(
           //     colname => this.Select(colname)
           //     );
           // __db.GetTableAndColumnNameByObject(expressionTEntity).ForEach_(
           //     colname => this.Select(colname)
           //     );
           // __db.GetTableAndColumnNameByObject(expressionTEntity1).ForEach_(
           //    colname => this.Select(colname)
           //    );
           // __db.GetTableAndColumnNameByObject(expressionTEntity2).ForEach_(
           // colname => this.Select(colname)
           // );
           // __db.GetTableAndColumnNameByObject(expressionTEntity3).ForEach_(
           //colname => this.Select(colname)
           //);


            this.Select(expression);
            this.Select(expressionTEntity);
            this.Select(expressionTEntity1);
            this.Select(expressionTEntity2);
            this.Select(expressionTEntity3);
            
            return this;
        }

        /// <summary>
        /// 添加要获取的字段
        /// </summary>
        /// <param name="expression">表达式</param>
        /// <returns>查询构建器</returns>
        public GetBuilder<T> Select<TEntity, TEntity1, TEntity2, TEntity3,TEntity4>(Expression<Func<T, object>> expression, Expression<Func<TEntity, object>> expressionTEntity,
            Expression<Func<TEntity1, object>> expressionTEntity1, Expression<Func<TEntity2, object>> expressionTEntity2, Expression<Func<TEntity3, object>> expressionTEntity3, Expression<Func<TEntity4, object>> expressionTEntity4)
        {
            // __db.GetTableAndColumnNameByObject(expression).ForEach_(
            //     colname => this.Select(colname)
            //     );
            // __db.GetTableAndColumnNameByObject(expressionTEntity).ForEach_(
            //     colname => this.Select(colname)
            //     );
            // __db.GetTableAndColumnNameByObject(expressionTEntity1).ForEach_(
            //    colname => this.Select(colname)
            //    );
            // __db.GetTableAndColumnNameByObject(expressionTEntity2).ForEach_(
            // colname => this.Select(colname)
            // );
            // __db.GetTableAndColumnNameByObject(expressionTEntity3).ForEach_(
            //colname => this.Select(colname)
            //);


            this.Select(expression);
            this.Select(expressionTEntity);
            this.Select(expressionTEntity1);
            this.Select(expressionTEntity2);
            this.Select(expressionTEntity3);
            this.Select(expressionTEntity4);

            return this;
        }

        /// <summary>
        /// 添加要获取的字段
        /// </summary>
        /// <param name="expression">表达式</param>
        /// <returns>查询构建器</returns>
        public GetBuilder<T> Select<TEntity, TEntity1, TEntity2, TEntity3, TEntity4,TEntity5>(Expression<Func<T, object>> expression, Expression<Func<TEntity, object>> expressionTEntity,
            Expression<Func<TEntity1, object>> expressionTEntity1, Expression<Func<TEntity2, object>> expressionTEntity2, Expression<Func<TEntity3, object>> expressionTEntity3, Expression<Func<TEntity4, object>> expressionTEntity4,
             Expression<Func<TEntity5, object>> expressionTEntity5)
        {
            // __db.GetTableAndColumnNameByObject(expression).ForEach_(
            //     colname => this.Select(colname)
            //     );
            // __db.GetTableAndColumnNameByObject(expressionTEntity).ForEach_(
            //     colname => this.Select(colname)
            //     );
            // __db.GetTableAndColumnNameByObject(expressionTEntity1).ForEach_(
            //    colname => this.Select(colname)
            //    );
            // __db.GetTableAndColumnNameByObject(expressionTEntity2).ForEach_(
            // colname => this.Select(colname)
            // );
            // __db.GetTableAndColumnNameByObject(expressionTEntity3).ForEach_(
            //colname => this.Select(colname)
            //);


            this.Select(expression);
            this.Select(expressionTEntity);
            this.Select(expressionTEntity1);
            this.Select(expressionTEntity2);
            this.Select(expressionTEntity3);
            this.Select(expressionTEntity4);
            this.Select(expressionTEntity5);

            return this;
        }


        /// <summary>
        /// 添加要获取的字段
        /// </summary>
        /// <param name="expression">表达式</param>
        /// <returns>查询构建器</returns>
        public GetBuilder<T> Select<TEntity, TEntity1, TEntity2, TEntity3, TEntity4, TEntity5, TEntity6>(Expression<Func<T, object>> expression, Expression<Func<TEntity, object>> expressionTEntity,
            Expression<Func<TEntity1, object>> expressionTEntity1, Expression<Func<TEntity2, object>> expressionTEntity2, Expression<Func<TEntity3, object>> expressionTEntity3, Expression<Func<TEntity4, object>> expressionTEntity4,
             Expression<Func<TEntity5, object>> expressionTEntity5, Expression<Func<TEntity6, object>> expressionTEntity6)
        {
            // __db.GetTableAndColumnNameByObject(expression).ForEach_(
            //     colname => this.Select(colname)
            //     );
            // __db.GetTableAndColumnNameByObject(expressionTEntity).ForEach_(
            //     colname => this.Select(colname)
            //     );
            // __db.GetTableAndColumnNameByObject(expressionTEntity1).ForEach_(
            //    colname => this.Select(colname)
            //    );
            // __db.GetTableAndColumnNameByObject(expressionTEntity2).ForEach_(
            // colname => this.Select(colname)
            // );
            // __db.GetTableAndColumnNameByObject(expressionTEntity3).ForEach_(
            //colname => this.Select(colname)
            //);


            this.Select(expression);
            this.Select(expressionTEntity);
            this.Select(expressionTEntity1);
            this.Select(expressionTEntity2);
            this.Select(expressionTEntity3);
            this.Select(expressionTEntity4);
            this.Select(expressionTEntity5);
            this.Select(expressionTEntity6);

            return this;
        }


        /// <summary>
        /// 添加要获取的字段
        /// </summary>
        /// <param name="expression">表达式</param>
        /// <returns>查询构建器</returns>
        public GetBuilder<T> Select<TEntity, TEntity1, TEntity2, TEntity3, TEntity4, TEntity5, TEntity6, TEntity7>(Expression<Func<T, object>> expression, Expression<Func<TEntity, object>> expressionTEntity,
            Expression<Func<TEntity1, object>> expressionTEntity1, Expression<Func<TEntity2, object>> expressionTEntity2, Expression<Func<TEntity3, object>> expressionTEntity3, Expression<Func<TEntity4, object>> expressionTEntity4,
             Expression<Func<TEntity5, object>> expressionTEntity5, Expression<Func<TEntity6, object>> expressionTEntity6, Expression<Func<TEntity7, object>> expressionTEntity7)
        {
            // __db.GetTableAndColumnNameByObject(expression).ForEach_(
            //     colname => this.Select(colname)
            //     );
            // __db.GetTableAndColumnNameByObject(expressionTEntity).ForEach_(
            //     colname => this.Select(colname)
            //     );
            // __db.GetTableAndColumnNameByObject(expressionTEntity1).ForEach_(
            //    colname => this.Select(colname)
            //    );
            // __db.GetTableAndColumnNameByObject(expressionTEntity2).ForEach_(
            // colname => this.Select(colname)
            // );
            // __db.GetTableAndColumnNameByObject(expressionTEntity3).ForEach_(
            //colname => this.Select(colname)
            //);


            this.Select(expression);
            this.Select(expressionTEntity);
            this.Select(expressionTEntity1);
            this.Select(expressionTEntity2);
            this.Select(expressionTEntity3);
            this.Select(expressionTEntity4);
            this.Select(expressionTEntity5);
            this.Select(expressionTEntity6);
            this.Select(expressionTEntity7);

            return this;
        }


        /// <summary>
        /// 添加要获取的字段
        /// </summary>
        /// <param name="expression">表达式</param>
        /// <returns>查询构建器</returns>
        public GetBuilder<T> Select<TEntity, TEntity1, TEntity2, TEntity3, TEntity4, TEntity5, TEntity6, TEntity7, TEntity8>(Expression<Func<T, object>> expression, Expression<Func<TEntity, object>> expressionTEntity,
            Expression<Func<TEntity1, object>> expressionTEntity1, Expression<Func<TEntity2, object>> expressionTEntity2, Expression<Func<TEntity3, object>> expressionTEntity3, Expression<Func<TEntity4, object>> expressionTEntity4,
             Expression<Func<TEntity5, object>> expressionTEntity5, Expression<Func<TEntity6, object>> expressionTEntity6, Expression<Func<TEntity7, object>> expressionTEntity7, Expression<Func<TEntity8, object>> expressionTEntity8)
        {
            // __db.GetTableAndColumnNameByObject(expression).ForEach_(
            //     colname => this.Select(colname)
            //     );
            // __db.GetTableAndColumnNameByObject(expressionTEntity).ForEach_(
            //     colname => this.Select(colname)
            //     );
            // __db.GetTableAndColumnNameByObject(expressionTEntity1).ForEach_(
            //    colname => this.Select(colname)
            //    );
            // __db.GetTableAndColumnNameByObject(expressionTEntity2).ForEach_(
            // colname => this.Select(colname)
            // );
            // __db.GetTableAndColumnNameByObject(expressionTEntity3).ForEach_(
            //colname => this.Select(colname)
            //);


            this.Select(expression);
            this.Select(expressionTEntity);
            this.Select(expressionTEntity1);
            this.Select(expressionTEntity2);
            this.Select(expressionTEntity3);
            this.Select(expressionTEntity4);
            this.Select(expressionTEntity5);
            this.Select(expressionTEntity6);
            this.Select(expressionTEntity7);
            this.Select(expressionTEntity8);


            return this;
        }

        /// <summary>
        /// 添加要获取的字段
        /// </summary>
        /// <param name="expression">表达式</param>
        /// <returns>查询构建器</returns>
        public GetBuilder<T> Select<TEntity, TEntity1, TEntity2, TEntity3, TEntity4, TEntity5, TEntity6, TEntity7, TEntity8, TEntity9>(Expression<Func<T, object>> expression, Expression<Func<TEntity, object>> expressionTEntity,
            Expression<Func<TEntity1, object>> expressionTEntity1, Expression<Func<TEntity2, object>> expressionTEntity2, Expression<Func<TEntity3, object>> expressionTEntity3, Expression<Func<TEntity4, object>> expressionTEntity4,
             Expression<Func<TEntity5, object>> expressionTEntity5, Expression<Func<TEntity6, object>> expressionTEntity6, Expression<Func<TEntity7, object>> expressionTEntity7, Expression<Func<TEntity8, object>> expressionTEntity8,
             Expression<Func<TEntity9, object>> expressionTEntity9)
        {
            // __db.GetTableAndColumnNameByObject(expression).ForEach_(
            //     colname => this.Select(colname)
            //     );
            // __db.GetTableAndColumnNameByObject(expressionTEntity).ForEach_(
            //     colname => this.Select(colname)
            //     );
            // __db.GetTableAndColumnNameByObject(expressionTEntity1).ForEach_(
            //    colname => this.Select(colname)
            //    );
            // __db.GetTableAndColumnNameByObject(expressionTEntity2).ForEach_(
            // colname => this.Select(colname)
            // );
            // __db.GetTableAndColumnNameByObject(expressionTEntity3).ForEach_(
            //colname => this.Select(colname)
            //);


            this.Select(expression);
            this.Select(expressionTEntity);
            this.Select(expressionTEntity1);
            this.Select(expressionTEntity2);
            this.Select(expressionTEntity3);
            this.Select(expressionTEntity4);
            this.Select(expressionTEntity5);
            this.Select(expressionTEntity6);
            this.Select(expressionTEntity7);
            this.Select(expressionTEntity8);
            this.Select(expressionTEntity9);


            return this;
        }
        #endregion

        #region FROM
        private GetBuilder<T> __from;
		/// <summary>
		/// 从指定查询构建器里查询数据，如“FROM (SQL 语句)”
		/// </summary>
		/// <param name="from">查询构建器</param>
		/// <returns>查询构建器</returns>
		public GetBuilder<T> From(GetBuilder<T> from)
		{
			__from = from;
			return this;
		}
		#endregion

		#region LIMIT
		private int __skip;
		/// <summary>
		/// 跳过第几条记录
		/// </summary>
		/// <param name="count">跳过记录数量</param>
		/// <returns>查询构建器</returns>
		public GetBuilder<T> Skip(int count)
		{
			__skip = count;
			return this;
		}

		private int __take;
		/// <summary>
		/// 取连续记录数
		/// </summary>
		/// <param name="count">获取记录数</param>
		/// <returns>查询构建器</returns>
		public GetBuilder<T> Take(int count)
		{
			__take = count;
			return this;
		}
		#endregion

		#region ORDER BY
        private StringBuilder __orderby;

        /// <summary>
        /// 添加排序字段
        /// </summary>
        /// <param name="expression">表达式只支持单个字段</param>
        /// <returns>查询构建器</returns>
        public GetBuilder<T> OrderBy(Expression<Func<T, object>> expression)
        {
            return this.OrderByCode(false, expression);
        }




        /// <summary>
        /// 添加排序字段
        /// </summary>
        /// <param name="expression">表达式只支持单个字段</param>
        /// <returns>查询构建器</returns>
        public GetBuilder<T> OrderBy(params Expression[] expression)
		{
			return this.OrderBy(false, expression);
		}

		/// <summary>
		/// 添加排序字段
		/// </summary>
		/// <param name="expression">表达式只支持单个字段</param>
		/// <returns>查询构建器</returns>
		public GetBuilder<T> OrderBy(params Expression<Func<T, object>>[] expression)
		{
			return this.OrderBy(false, expression);
		}

        /// <summary>
        /// 添加排序字段
        /// </summary>
        /// <typeparam name="TEntity">实体类型</typeparam>
        /// <param name="expression">表达式只支持单个字段</param>
        /// <returns>查询构建器</returns>
        public GetBuilder<T> OrderBy<TEntity>(params Expression<Func<TEntity, object>>[] expression) where TEntity : new()
		{
			return this.OrderBy(false, expression);
		}

		/// <summary>
		/// 添加降序排序字段
		/// </summary>
		/// <param name="expression">表达式</param>
		/// <returns>查询构建器</returns>
		public GetBuilder<T> OrderByDescending(params Expression[] expression)
		{
			return this.OrderBy(true, expression);
		}

        /// <summary>
        /// 添加降序排序字段
        /// </summary>
        /// <param name="expression">表达式只支持单个字段</param>
        /// <returns>查询构建器</returns>
        public GetBuilder<T> OrderByDescending(params Expression<Func<T, object>>[] expression)
		{
			return this.OrderBy(true, expression);
		}

        /// <summary>
        /// 添加降序排序字段
        /// </summary>
        /// <typeparam name="TEntity">实体类型</typeparam>
        /// <param name="expression">表达式只支持单个字段</param>
        /// <returns>查询构建器</returns>
        public GetBuilder<T> OrderByDescending<TEntity>(params Expression<Func<TEntity, object>>[] expression) where TEntity : new()
		{
			return this.OrderByCode(true, expression);
		}

		private GetBuilder<T> OrderByCode(bool desc, params Expression[] expression)
		{
            if (expression == null || expression.Any(ex => ex == null))
                return this;
			if(__orderby.IsNull_()) __orderby = new StringBuilder(" ORDER BY ");
            expression.ForEach_(e =>
            {
                if (e.NodeType == ExpressionType.Lambda)
                {
                    var LambdaE = (LambdaExpression)e;
                    if (LambdaE.Body.NodeType == ExpressionType.New)
                    {
                        var LambdaN = LambdaE.Body as NewExpression;

                        if (__orderby.Length > 10 && LambdaN.Arguments.Any())
                            __orderby.Append(", ");

                        foreach (var argumemt in LambdaN.Arguments)
                        {
                            __orderby.Append(__db.GetTableAndColumnName(argumemt));

                        }
                        if (LambdaN.Arguments.Any() && desc)
                            __orderby.Append(" DESC");
                    }
                    else
                    {
                        if (__orderby.Length > 10)
                            __orderby.Append(", ");
                        __orderby.Append(__db.GetTableAndColumnName(e));
                        if (desc)
                            __orderby.Append(" DESC");

                    }

                }


            });
            return this;
		}

        private GetBuilder<T> OrderBy(bool desc, params Expression[] expression)
        {
            if (expression == null)
                return this;
            if (__orderby.IsNull_()) __orderby = new StringBuilder(" ORDER BY ");
            expression.ForEach_(e =>
            {
                if (__orderby.Length > 10)
                    __orderby.Append(", ");
                __orderby.Append(__db.GetTableAndColumnName(e));
                if (desc)
                    __orderby.Append(" DESC");
            });
            return this;
        }
        #endregion

        #region GROUP BY
        private StringBuilder __groupBy;
     
        /// <summary>
        /// 添加GroupBy字段
        /// </summary>
        /// <param name="expression">表达式</param>
        /// <returns>查询构建器</returns>
        public GetBuilder<T> GroupBy(params Expression<Func<T, object>>[] expression)
        {
            return this.GroupByCode(expression);
        }

        /// <summary>
        /// 添加GroupBy字段
        /// </summary>
        /// <typeparam name="TEntity">实体类型</typeparam>
        /// <param name="expression">表达式</param>
        /// <returns>查询构建器</returns>
        public GetBuilder<T> GroupBy<TEntity>(params Expression<Func<TEntity, object>>[] expression) where TEntity : new()
        {
            return this.GroupByCode(expression);
        }


        private GetBuilder<T> GroupByCode(Expression[] expression)
        {
            if (__groupBy.IsNull_()) __groupBy = new StringBuilder(" GROUP BY ");
            expression.ForEach_(e =>
            {
                if (__groupBy.Length > 10)
                    __groupBy.Append(", ");
                __groupBy.Append(__db.GetTableAndColumnName(e));
            });
            return this;
        }

        #endregion
		#region DISTINCT
		private bool __dist;
		/// <summary>
		/// 返回非重复记录
		/// </summary>
		/// <returns>查询构建器</returns>
		public GetBuilder<T> Distinct()
		{
			__dist = true;
			return this;
		}
		#endregion

		#region Map
		private Delegate __mapCallback;
		private Type[] __mapType;
		private bool __hasMap;
		private GetBuilder<T> InternalMap(Type[] type, Delegate cb)
		{
			__mapType = type;
			__mapCallback = cb;
			__hasMap = true;
			return this;
		}

        /// <summary>
        /// 多实体关联映射
        /// </summary>
        /// <typeparam name="T1">实体类型 1</typeparam>
        /// <param name="cb">实体间的关联转换器</param>
        /// <returns>查询构建器</returns>
        public GetBuilder<T> Map<T1>(Func<T,T1, T> cb)
            where T1 : new()
        {
            return InternalMap(
                new Type[] {
                    typeof(T),
					typeof(T1),
				},
                cb);
        }

		/// <summary>
		/// 多实体关联映射
		/// </summary>
		/// <typeparam name="T1">实体类型 1</typeparam>
		/// <typeparam name="T2">实体类型 2</typeparam>
		/// <param name="cb">实体间的关联转换器</param>
		/// <returns>查询构建器</returns>
		public GetBuilder<T> Map<T1, T2>(Func<T, T1, T2, T> cb)
			where T1 : new()
			where T2 : new()
		{
			return InternalMap(
				new Type[] {
                    typeof(T),
					typeof(T1),
					typeof(T2)
				},
				cb);
		}

		/// <summary>
		/// 多实体关联映射
		/// </summary>
		/// <typeparam name="T1">实体类型 1</typeparam>
		/// <typeparam name="T2">实体类型 2</typeparam>
		/// <typeparam name="T3">实体类型 3</typeparam>
		/// <param name="cb">实体间的关联转换器</param>
		/// <returns>查询构建器</returns>
        public GetBuilder<T> Map<T1, T2, T3>(Func<T, T1, T2, T3, T> cb)
			where T1 : new()
			where T2 : new()
			where T3 : new()
		{
			return InternalMap(
				new Type[] {
                     typeof(T),
					typeof(T1),
					typeof(T2),
					typeof(T3)
				},
				cb);
		}

		/// <summary>
		/// 多实体关联映射
		/// </summary>
		/// <typeparam name="T1">实体类型 1</typeparam>
		/// <typeparam name="T2">实体类型 2</typeparam>
		/// <typeparam name="T3">实体类型 3</typeparam>
		/// <typeparam name="T4">实体类型 4</typeparam>
		/// <param name="cb">实体间的关联转换器</param>
		/// <returns>查询构建器</returns>
        public GetBuilder<T> Map<T1, T2, T3, T4>(Func<T, T1, T2, T3, T4, T> cb)
			where T1 : new()
			where T2 : new()
			where T3 : new()
			where T4 : new()
		{
			return InternalMap(
				new Type[] {
                      typeof(T),
					typeof(T1),
					typeof(T2),
					typeof(T3),
					typeof(T4)
				},
				cb);
		}

		/// <summary>
		/// 多实体关联映射
		/// </summary>
		/// <typeparam name="T1">实体类型 1</typeparam>
		/// <typeparam name="T2">实体类型 2</typeparam>
		/// <typeparam name="T3">实体类型 3</typeparam>
		/// <typeparam name="T4">实体类型 4</typeparam>
		/// <typeparam name="T5">实体类型 5</typeparam>
		/// <param name="cb">实体间的关联转换器</param>
		/// <returns>查询构建器</returns>
		public GetBuilder<T> Map<T1, T2, T3, T4, T5>(Func<T,T1, T2, T3, T4, T5, T> cb)
			where T1 : new()
			where T2 : new()
			where T3 : new()
			where T4 : new()
			where T5 : new()
		{
			return InternalMap(
				new Type[] {
                      typeof(T),
					typeof(T1),
					typeof(T2),
					typeof(T3),
					typeof(T4),
					typeof(T5)
				},
				cb);
		}
		#endregion

		#region 内部操作
		private string GetSelect()
		{
			if(__select.IsNull_()) return "*";
			return __select.ToString();
		}
		private string GetFrom()
		{
			if(__from.IsNull_()) return __table;
			return "({0}) {1}".F(__from.GetSql(), __table);
		}
		private string GetJoin()
		{
			if(__listJoin == null || !__listJoin.Any()) return string.Empty;
			return string.Join("", __listJoin.Select(t => t.ToString()));
		}
		private string GetWhere(bool prefix = true)
		{
			if(__where.IsNull_()) return string.Empty;
			if(prefix)
				return "WHERE " + __where.ToString();
			return __where.ToString();
		}
		public object[] GetParams()
		{
			return this.Params.ToArray();
		}
		private string GetOrderBy()
		{
			if(__orderby.IsNull_()) return string.Empty;
			return __orderby.ToString();
		}

        private string GetGroupBy()
        {
            if (__groupBy.IsNull_()) return string.Empty;
            return __groupBy.ToString();
        }
		#endregion

		#region 生成 SQL 语句
		public string GetSql()
		{
			if(__take > 0)
			{
				if(__skip > 0)
					return this.GetSqlPaged();
				return this.GetSqlTake();
			}
			var dist = __dist ? "DISTINCT " : string.Empty;
            return "SELECT {0}{1} FROM {2}{3} {4}{5}{6}".F(dist, this.GetSelect(), this.GetFrom(), this.GetJoin(), this.GetWhere(), this.GetGroupBy(), this.GetOrderBy());
		}

       
        private string GetSqlTake()
        {
            return __db.Provider.BuildTopSql(__take, __dist, this.GetSelect(), this.GetFrom(), this.GetJoin(), this.GetWhere(), this.GetOrderBy(), this.Params);
        }
        private string GetSqlPaged()
        {
            return string.Empty; // TODO　__db.Provider.BuildPageQuery(__skip, __take, __dist, this.GetSelect(), this.GetFrom(), this.GetJoin(), this.GetWhere(), this.GetOrderBy(), this.Params);
        }
		#endregion

		#region 执行
		#region Count
		/// <summary>
		/// 获取符合相关条件的记录数量
		/// </summary>
		/// <returns>返回符合相关条件的记录数量</returns>
		public int Count()
		{
			return this.Count<int>();
		}
		/// <summary>
		/// 获取符合相关条件的记录数量
		/// </summary>
		/// <typeparam name="TResult">返回的数据类型</typeparam>
		/// <returns>以指定的数据类型返回符合相关条件的记录数量</returns>
		public TResult Count<TResult>()
		{
			if(__dist)
			{
				return __db.ExecuteScalar<TResult>("SELECT COUNT(*) FROM (SELECT DISTINCT {0} FROM {1}{2} {3})"
					.F(this.GetSelect(), this.GetFrom(), this.GetJoin(), this.GetWhere()), this.GetParams());
			}
			return __db.ExecuteScalar<TResult>("SELECT COUNT(*) FROM {0}{1} {2}"
				.F(this.GetFrom(), this.GetJoin(), this.GetWhere()), this.GetParams());
		}
		#endregion

		#region Exist
		/// <summary>
		/// 获取是否存在相关条件的记录
		/// </summary>
		/// <returns>返回是否存在相关条件的记录</returns>
		public bool Exist()
		{
			return __db.Exists<T>(GetWhere(false), this.GetParams());
		}
		#endregion

		#region Sum
		/// <summary>
		/// 获取指定列的和
		/// </summary>
		/// <param name="expression">列表达式</param>
		/// <returns>符合相关条件的和</returns>
		public int Sum(Expression<Func<T, object>> expression)
		{
			return this.Sum<int>(expression);
		}
		/// <summary>
		/// 获取指定字段的和
		/// </summary>
		/// <typeparam name="TResult">返回的数据类型</typeparam>
		/// <param name="expression">表达式</param>
		/// <returns>以指定的数据类型返回符合相关条件的和</returns>
		public TResult Sum<TResult>(Expression<Func<T, object>> expression)
		{
			return __db.ExecuteScalar<TResult>("SELECT SUM({0}) FROM {1}{2} {3}".F(__db.GetTableAndColumnName(expression), this.GetFrom(), this.GetJoin(), this.GetWhere()), this.GetParams());
		}
		/// <summary>
		/// 获取指定字段的和
		/// </summary>
		/// <typeparam name="TEntity">实体类型</typeparam>
		/// <param name="expression">表达式</param>
		/// <returns>符合相关条件的和</returns>
		public int Sum<TEntity>(Expression<Func<TEntity, object>> expression)
		{
			return this.Sum<TEntity, int>(expression);
		}
		/// <summary>
		/// 获取指定字段的和
		/// </summary>
		/// <typeparam name="TEntity">实体类型</typeparam>
		/// <typeparam name="TResult">返回的数据类型</typeparam>
		/// <param name="expression">表达式</param>
		/// <returns>以指定的数据类型返回符合相关条件的和</returns>
		public TResult Sum<TEntity, TResult>(Expression<Func<TEntity, object>> expression)
		{
			return __db.ExecuteScalar<TResult>("SELECT SUM({0}) FROM {1}{2} {3}".F(__db.GetTableAndColumnName(expression), this.GetFrom(), this.GetJoin(), this.GetWhere()), this.GetParams());
		}
		#endregion

		#region Max
		/// <summary>
		/// 获取指定列的最大值
		/// </summary>
		/// <param name="expression">列表达式</param>
		/// <returns>符合相关条件的最大值</returns>
		public int Max(Expression<Func<T, object>> expression)
		{
			return this.Max<int>(expression);
		}
		/// <summary>
		/// 获取指定字段的最大值
		/// </summary>
		/// <typeparam name="TResult">返回的数据类型</typeparam>
		/// <param name="expression">表达式</param>
		/// <returns>以指定的数据类型返回符合相关条件的最大值</returns>
		public TResult Max<TResult>(Expression<Func<T, object>> expression)
		{
			return __db.ExecuteScalar<TResult>("SELECT MAX({0}) FROM {1}{2} {3}".F(__db.GetColumnName(expression), this.GetFrom(), this.GetJoin(), this.GetWhere()), this.GetParams());
		}
		/// <summary>
		/// 获取指定字段的最大值
		/// </summary>
		/// <typeparam name="TEntity">实体类型</typeparam>
		/// <param name="expression">表达式</param>
		/// <returns>符合相关条件的最大值</returns>
		public int Max<TEntity>(Expression<Func<TEntity, object>> expression)
		{
			return this.Max<TEntity, int>(expression);
		}
		/// <summary>
		/// 获取指定字段的最大值
		/// </summary>
		/// <typeparam name="TEntity">实体类型</typeparam>
		/// <typeparam name="TResult">返回的数据类型</typeparam>
		/// <param name="expression">表达式</param>
		/// <returns>以指定的数据类型返回符合相关条件的最大值</returns>
		public TResult Max<TEntity, TResult>(Expression<Func<TEntity, object>> expression)
		{
			return __db.ExecuteScalar<TResult>("SELECT MAX({0}) FROM {1}{2} {3}".F(__db.GetColumnName(expression), this.GetFrom(), this.GetJoin(), this.GetWhere()), this.GetParams());
		}
		#endregion

		#region Min
		/// <summary>
		/// 获取指定字段的最小值
		/// </summary>
		/// <param name="expression">表达式</param>
		/// <returns>符合相关条件的最小值</returns>
		public int Min(Expression<Func<T, object>> expression)
		{
			return this.Min<int>(expression);
		}
		/// <summary>
		/// 获取指定字段的最小值
		/// </summary>
		/// <typeparam name="TResult">返回的数据类型</typeparam>
		/// <param name="expression">表达式</param>
		/// <returns>以指定的数据类型返回符合相关条件的最小值</returns>
		public TResult Min<TResult>(Expression<Func<T, object>> expression)
		{
			return __db.ExecuteScalar<TResult>("SELECT MIN({0}) FROM {1}{2} {3}".F(__db.GetColumnName(expression), this.GetFrom(), this.GetJoin(), this.GetWhere()), this.GetParams());
		}
		/// <summary>
		/// 获取指定字段的最小值
		/// </summary>
		/// <typeparam name="TEntity">实体类型</typeparam>
		/// <param name="expression">表达式</param>
		/// <returns>符合相关条件的最小值</returns>
		public int Min<TEntity>(Expression<Func<TEntity, object>> expression)
		{
			return this.Min<TEntity, int>(expression);
		}
		/// <summary>
		/// 获取指定字段的最小值
		/// </summary>
		/// <typeparam name="TEntity">实体类型</typeparam>
		/// <typeparam name="TResult">返回的数据类型</typeparam>
		/// <param name="expression">表达式</param>
		/// <returns>以指定的数据类型返回符合相关条件的最小值</returns>
		public TResult Min<TEntity, TResult>(Expression<Func<TEntity, object>> expression)
		{
			return __db.ExecuteScalar<TResult>("SELECT MIN({0}) FROM {1}{2} {3}".F(__db.GetColumnName(expression), this.GetFrom(), this.GetJoin(), this.GetWhere()), this.GetParams());
		}
		#endregion

		/// <summary>
		/// 获取列表
		/// </summary>
		/// <returns>符合相关条件的实体列表</returns>
		public List<T> ToList()
		{
			if(__hasMap)
				return __db.Query<T>(__mapType, __mapCallback, this.GetSql(), this.GetParams()).ToList();
			return __db.Query<T>(this.GetSql(), this.GetParams()).ToList();
		}

		/// <summary>
		/// 获取满足条件的第一项或默认内容
		/// </summary>
		/// <returns>符合相关条件的单个实体</returns>
		public T FirstOrDefault()
		{
			if(__hasMap)
				return __db.Query<T>(__mapType, __mapCallback, this.GetSql(), this.GetParams()).FirstOrDefault();
			__take = 1;
			return __db.Query<T>(this.GetSql(), this.GetParams()).FirstOrDefault();
		}

       
		#endregion


        
    }
}