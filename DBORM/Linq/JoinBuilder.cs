// ***********************************************************************
// 程序集			: DBORM
// 文件名			: WhereBuilder.cs
// 作者				: 何权洲
// 创建时间			: 2013-08-05
//
// 最后修改者		: 何权洲
// 最后修改时间		: 2013-11-05
// ***********************************************************************

using PetaPoco;
using System;
using System.Collections;
using System.Collections.Generic;
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
	/// WHERE 构建器
	/// </summary>
	internal class JoinBuilder : ExpressionVisitor
	{
		private StringBuilder __sql;
        private string __joinTable;

        private IDatabase __db;
		private bool __wtn;

		/// <summary>
		/// 初始化一个新 <see cref="WhereBuilder" /> 实例。
		/// </summary>
		/// <param name="db">数据库实例</param>
		/// <param name="args">参数</param>
		/// <param name="withTableName">指定是否包含表名</param>
		public JoinBuilder(IDatabase db,string joinTable, List<object> args = null, bool withTableName = true)
		{
			__sql = new StringBuilder();
            __joinTable = joinTable;
            __db = db;
			__wtn = withTableName;
			this.Params = args ?? new List<object>();
		}

		/// <summary>
		/// 获取参数
		/// </summary>
		/// <value>参数集合</value>
		public List<object> Params { get; private set; }

		/// <summary>
		/// 返回 WHERE 子句内容
		/// </summary>
		/// <returns>WHERE 子句内容</returns>
		public override string ToString()
		{
			return __joinTable +  __sql.ToString();
		}

		#region Append
		private JoinBuilder Append(Expression expression, string type)
		{
			if(expression != null)
			{
                if (!__sql.IsEmpty_())
					__sql.Append(type);
				base.Visit(expression);
			}

			return this;
		}
		private JoinBuilder Append(Expression property, QueryOperatorType op, object value, string type)
		{
			if(property != null && value != null)
			{
				if(!__sql.IsEmpty_())
					__sql.Append(type);

				__sql.Append("(");
				__sql.Append(__db.GetColumnName(property));

                if (value == null)
                {

                    __sql.Append(this.GetOperator(op, true));
                    __sql.Append(" NULL ");
                }
                else
                {
                    __sql.Append(this.GetOperator(op, false));
                    __sql.AppendFormat("@{0}", this.Params.Count);
                }
				  
				__sql.Append(")");

				switch(op)
				{
					case QueryOperatorType.Contains:
						value = "%" + value.ToString().Trim('%') + "%";
						break;
					case QueryOperatorType.StartsWith:
						value = value.ToString().Trim('%') + "%";
						break;
					case QueryOperatorType.EndsWith:
						value = "%" + value.ToString().Trim('%');
						break;
				}
				this.Params.Add(value);
			}

			return this;
		}
		private JoinBuilder Append(string where, string type)
		{
			if(!where.IsNullOrEmpty_())
			{
				if(__sql.IsEmpty_())
					__sql.Append(type);
				__sql.Append(where);
			}

			return this;
		}

		/// <summary>添加 WHERE 子句</summary>
		/// <param name="expression">条件表达式</param>
		/// <returns>WHERE 构建器</returns>
		internal JoinBuilder Append(Expression expression)
		{
            return this.Append(expression, " AND ");
		}

		/// <summary>添加 WHERE 子句</summary>
		/// <param name="property">字段栏表达式</param>
		/// <param name="op">比较运算符</param>
		/// <param name="value">值</param>
		/// <returns>WHERE 构建器</returns>
		internal JoinBuilder Append(Expression property, QueryOperatorType op, object value)
		{
			return this.Append(property, op, value, " AND ");
		}

		/// <summary>添加 WHERE 子句</summary>
		/// <param name="where">WHERE 子句</param>
		/// <returns>WHERE 构建器</returns>
		internal JoinBuilder Append(string where)
		{
			return this.Append(where, " AND ");
		}

		/// <summary>添加 WHERE OR 子句</summary>
		/// <param name="expression">条件表达式</param>
		/// <returns>WHERE 构建器</returns>
		internal JoinBuilder AppendOr(Expression expression)
		{
			return this.Append(expression, " OR ");
		}

		/// <summary>添加 WHERE OR 子句</summary>
		/// <param name="property">字段栏表达式</param>
		/// <param name="op">比较运算符</param>
		/// <param name="value">值</param>
		/// <returns>WHERE 构建器</returns>
		internal JoinBuilder AppendOr(Expression property, QueryOperatorType op, object value)
		{
			return this.Append(property, op, value, " OR ");
		}

		/// <summary>添加 WHERE OR 子句</summary>
		/// <param name="where">WHERE 子句</param>
		/// <returns>WHERE 构建器</returns>
		internal JoinBuilder AppendOr(string where)
		{
			return this.Append(where, " OR ");
		}
		#endregion

		#region 重写 ExpressionVisitor
		/// <summary>
		/// 处理二元运算表达式
		/// </summary>
		/// <param name="expression">表达式</param>
		/// <returns>表达式</returns>
		protected override Expression VisitBinary(BinaryExpression expression)
		{
			__sql.Append("(");
            this.Visit(expression.Left);
            if (expression.Right.NodeType == ExpressionType.Constant)
            {
                var value = this.GetRightValue(expression.Right);

                __sql.Append(this.GetOperator(expression.NodeType, value == null));
                if (value == null)
                {
                    __sql.Append("NULL");
                }
                else
                {
                    this.Visit(expression.Right);

                }
            }
            else
            {
             

                __sql.Append(this.GetOperator(expression.NodeType));
                this.Visit(expression.Right);
            }
     
			
			__sql.Append(")");

			return expression;
		}

		/// <summary>
		/// 处理字段或属性表达式
		/// </summary>
		/// <param name="expression">表达式</param>
		/// <returns>表达式</returns>
        protected override Expression VisitMemberAccess(MemberExpression expression, object memberValue = default(object))
		{
			if(expression.Expression != null && expression.Expression.NodeType == ExpressionType.Parameter)
				__sql.Append(this.GetColumnName(expression));
			else
			{
				__sql.AppendFormat("@{0}", this.Params.Count);
				this.Params.Add(this.GetRightValue(expression));
			}

			return expression;
		}

		/// <summary>
		/// 处理常量表达式
		/// </summary>
		/// <param name="expression">表达式</param>
		/// <returns>表达式</returns>
		protected override Expression VisitConstant(ConstantExpression expression)
		{
			__sql.AppendFormat("@{0}", this.Params.Count);
			this.Params.Add(expression.Value);

			return expression;
		}

		/// <summary>
		/// 处理方法调用表达式
		/// </summary>
		/// <param name="expression">表达式</param>
		/// <returns>表达式</returns>
		/// <exception cref="System.NotImplementedException">指定方法未实现</exception>
        protected override Expression VisitMethodCall(MethodCallExpression expression, object memberValue = default(object))
		{
			switch(expression.Method.Name)
			{
				case "Contains":
					this.ParseLike(expression, "Contains");
					break;
				case "StartsWith":
					this.ParseLike(expression, "StartsWith");
					break;
				case "EndsWith":
					this.ParseLike(expression, "EndsWith");
					break;
				case "In_":
					this.ParseIn(expression);
					break;
                case "NotIn_":
                    this.ParseNotIn(expression);
                    break;
				case "ToString":
					this.ParseToString(expression);
					break;
				default:
                    throw new NotImplementedException(string.Format("MethodCallExpression:{0} is not suported", expression.ToString()));
			}

			return expression;
		}

        /// <summary>
		/// 获取查询操作符
		/// </summary>
		/// <param name="expType">表达式树节点类型</param>
		/// <returns>查询操作符</returns>
		/// <exception cref="System.NotSupportedException">指定操作符未实现</exception>
		protected string GetOperator(ExpressionType expType,bool valueIsNull)
        {
            if (valueIsNull)
            {
                switch (expType)
                {
                    case ExpressionType.Equal:
                        return " IS ";

                    case ExpressionType.NotEqual:
                        return " IS NOT ";
                }
            }
          
            return base.GetOperator(expType);
        }

        /// <summary>
        /// 获取查询操作符
        /// </summary>
        /// <param name="expType">表达式树节点类型</param>
        /// <returns>查询操作符</returns>
        /// <exception cref="System.NotSupportedException">指定操作符未实现</exception>
        protected string GetOperator(QueryOperatorType expType, bool valueIsNull)
        {
            if (valueIsNull)
            {
                switch (expType)
                {
                    case QueryOperatorType.Equal:
                        return " IS ";

                    case QueryOperatorType.NotEqual:
                        return " IS NOT ";
                }
            }

            return base.GetOperator(expType);
        }

		#endregion

		#region 内部处理
		private string GetColumnName(MemberExpression expression)
		{
			//var colName = __db.GetColumnName(expression);
			//if(__wtn)
			//	return "{0}.{1}".F(__db.GetTableName(expression.Member.ReflectedType), colName);
			//return colName;
			return __wtn ? __db.GetTableAndColumnName((Expression)expression) : __db.GetColumnName((Expression)expression);
		}
		private void ParseToString(MethodCallExpression expression)
		{
			__sql.AppendFormat("@{0}", this.Params.Count);
			var value = this.GetRightValue(expression.Object).ToString();
			this.Params.Add(value);
		}
		private void ParseIn(MethodCallExpression expression)
		{
			var column = expression.Arguments[0] as MemberExpression;
			var value = this.GetRightValue(expression.Arguments[1]);
			if(value is IEnumerable)
			{
				var val = value as IEnumerable;
				__sql.AppendFormat("({0} IN (@{1}))", this.GetColumnName(column), this.Params.Count);
				this.Params.Add(val);
			}
			else if(value is SqlBuilder)
			{
                var val = value as SqlBuilder;
				__sql.AppendFormat("({0} IN ({1}))", this.GetColumnName(column), val.GetSql());
			}
		}

        private void ParseNotIn(MethodCallExpression expression)
        {
            var column = expression.Arguments[0] as MemberExpression;
            var value = this.GetRightValue(expression.Arguments[1]);
            if (value is IEnumerable)
            {
                var val = value as IEnumerable;
                __sql.AppendFormat("({0} NOT IN (@{1}))", this.GetColumnName(column), this.Params.Count);
                this.Params.Add(val);
            }
            else if (value is SqlBuilder)
            {
                var val = value as SqlBuilder;
                __sql.AppendFormat("({0} NOT IN ({1}))", this.GetColumnName(column), val.GetSql());
            }
        }

		private void ParseLike(MethodCallExpression expression, string type)
		{
			var value = this.GetRightValue(expression.Arguments[0]);
			var column = expression.Object as MemberExpression;
			__sql.AppendFormat("({0} LIKE @{1})", this.GetColumnName(column), this.Params.Count);
			switch(type)
			{
				case "Contains":
					value = "%" + value.ToString().Trim('%') + "%";
					break;
				case "StartsWith":
					value = value.ToString().Trim('%') + "%";
					break;
				case "EndsWith":
					value = "%" + value.ToString().Trim('%');
					break;
			}
			this.Params.Add(value);
		}



		#endregion
	}
}