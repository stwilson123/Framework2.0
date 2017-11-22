// ***********************************************************************
// 程序集			: DBORM
// 文件名			: SelectBuilder.cs
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
using DBEntityGenerate.Linq.SqlOperator;
using System.Reflection;
/// <summary>
/// Data 命名空间
/// </summary>
namespace DBORM
{
	/// <summary>
	/// WHERE 构建器
	/// </summary>
	internal class SelectBuilder : ExpressionVisitor
	{
		private StringBuilder __sql;
		private IDatabase __db;
		private bool __wtn;
        private Stack<String> __stackFunExpression;
		/// <summary>
		/// 初始化一个新 <see cref="WhereBuilder" /> 实例。
		/// </summary>
		/// <param name="db">数据库实例</param>
		/// <param name="args">参数</param>
		/// <param name="withTableName">指定是否包含表名</param>
        public SelectBuilder(IDatabase db,  bool withTableName = true)
		{
			__sql = new StringBuilder();
            
			__db = db;
			__wtn = withTableName;
            __stackFunExpression = new Stack<string>();
		}
		 

		/// <summary>
		/// 返回 WHERE 子句内容
		/// </summary>
		/// <returns>WHERE 子句内容</returns>
		public override string ToString()
		{
			return __sql.ToString();
		}
        #region SELECT ALL
        public SelectBuilder Select<T>()
        {
            if (!__sql.IsEmpty_())
                __sql.Remove(0, __sql.Length);
            __sql.Append(string.Format("\"{0}\".*", __db.GetTableName<T>()));

            return this;
        }
        #endregion
        #region Append
        private SelectBuilder Append(Expression expression, string type)
		{
			if(expression != null)
			{
                if (!__sql.IsEmpty_())
					__sql.Append(type);
				base.Visit(expression);
			}

			return this;
		}
        private SelectBuilder Append(Expression property, FunctionOperatorType op, object value,string splitValue)
		{
            var fuctionName = string.Empty;
            switch (op)
            {
                case FunctionOperatorType.Sum:
                    fuctionName = "SUM";
                    break;
                case FunctionOperatorType.Count:
                    fuctionName = "COUNT";
                    break;
            }

			if(property != null && value != null)
			{
				if(!__sql.IsEmpty_())
                    __sql.Append(splitValue);

				__sql.Append(fuctionName+"(");
				__sql.Append(__db.GetTableAndColumnName(property));
				__sql.Append(")");
			}

			return this;
		}
        private SelectBuilder Append(string where, string type)
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
        internal SelectBuilder Append(Expression expression)
		{
            return this.Append(expression, ",");
		}

		/// <summary>添加 WHERE 子句</summary>
		/// <param name="property">字段栏表达式</param>
		/// <param name="op">比较运算符</param>
		/// <param name="value">值</param>
		/// <returns>WHERE 构建器</returns>
        internal SelectBuilder Append(Expression property, FunctionOperatorType op, object value)
		{
			return this.Append(property, op, value, ",");
		}

		/// <summary>添加 WHERE 子句</summary>
		/// <param name="where">WHERE 子句</param>
		/// <returns>WHERE 构建器</returns>
        internal SelectBuilder Append(string select)
		{
            return this.Append(select, ",");
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
			return base.VisitBinary(expression);
		}

		/// <summary>
		/// 处理字段或属性表达式
		/// </summary>
		/// <param name="expression">表达式</param>
		/// <returns>表达式</returns>
        protected override Expression VisitMemberAccess(MemberExpression expression, object memberValue = default(object))
		{
            if (expression.Expression != null && expression.Expression.NodeType == ExpressionType.Parameter)
            {
                 
                if (__sql.Length > 1)
                {
                    var lastChar = __sql[__sql.Length-1];
                    __sql.Append(lastChar == '(' ? this.GetColumnNameByAlias(expression, memberValue) :
                        lastChar != ',' ? "," + this.GetColumnNameByAlias(expression, memberValue) : this.GetColumnNameByAlias(expression, memberValue));
                }
                else
                {
                    __sql.Append(this.GetColumnNameByAlias(expression, memberValue));

                }
            }
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
                case "Sum_":
                    this.ParseFunction(expression, "SUM", memberValue);
					break;
                case "Count_":
                    this.ParseFunction(expression, "COUNT", memberValue);
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
        /// 处理构造函数调用表达式
        /// </summary>
        /// <param name="expression">表达式</param>
        /// <returns>表达式</returns>
        protected override Expression VisitNew(NewExpression expression)
        {
            if (expression.Arguments != null && expression.Arguments.Count > 0)
            {
                for (int i = 0; i < expression.Arguments.Count; i++)
                {
                    base.Visit(expression.Arguments[i], expression.Members[i]);
                }

            }
            return expression;
        }


        /// <summary>
        /// 处理构造函数调用表达式
        /// </summary>
        /// <param name="expression">表达式</param>
        /// <returns>表达式</returns>
        protected override Expression VisitMemberInit(MemberInitExpression expression)
        {
            if (expression.Bindings != null && expression.Bindings.Count > 0)
            {
                for (int i = 0; i < expression.Bindings.Count; i++)
                {
                    if(expression.Bindings[i] is MemberAssignment)
                        base.Visit(((MemberAssignment)expression.Bindings[i]).Expression, expression.Bindings[i].Member);
                }

            }
            return expression;
        }


       
        #endregion

        #region 内部处理
        private string GetColumnName(MemberExpression expression)
		{
			return __wtn ? __db.GetTableAndColumnName((Expression)expression) : __db.GetColumnName((Expression)expression);
		}
        private string GetColumnNameByAlias(MemberExpression expression, object memberValue)
        {
            if (memberValue is PropertyInfo && Utils.GetPropertyInfo(expression).Name != ((PropertyInfo)memberValue).Name)
            {
                string AliasName =  " AS \"" + ((PropertyInfo)memberValue).Name +"\"";
                return __wtn ? __db.GetTableAndColumnName((Expression)expression) + AliasName :
                    __db.GetColumnName((Expression)expression) + AliasName;
            }
            return __wtn ? __db.GetTableAndColumnName((Expression)expression) : __db.GetColumnName((Expression)expression);
        }

		private void ParseToString(MethodCallExpression expression)
		{
            //__sql.AppendFormat("@{0}", this.Params.Count);
            //var value = this.GetRightValue(expression.Object).ToString();
            //this.Params.Add(value);
		}


        private void ParseFunction(MethodCallExpression expression, string type, object methondCallMemberName)
        {
            if (expression.Arguments != null && expression.Arguments.Count > 0)
            {
                __sql.Append(__sql[__sql.Length - 1] != ',' ? 
                    "," + type + "(" : type + "(");
                expression.Arguments.ForEach_(argument => 
                    {
                        base.Visit(argument);

                    });
                __sql.Append(")");
                __sql.Append(methondCallMemberName is PropertyInfo ? 
                    " AS " +  ((PropertyInfo)methondCallMemberName).Name : ""  
                    );

            }
    

        }



		#endregion
	}
}