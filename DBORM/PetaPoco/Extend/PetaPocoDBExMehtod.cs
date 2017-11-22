using PetaPoco;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using Framework.Util.BaseTypeMethodExpend;
using DBEntityGenerate.Linq;

namespace DBORM.PetaPoco.Extend
{
    public static class PetaPocoDBExMethod
    {
        #region Get
        /// <summary>
        /// 获取实体数据
        /// </summary>
        /// <typeparam name="T">实体类型</typeparam>
        /// <param name="db">数据库实例</param>
        /// <returns>查询构建器</returns>
        public static GetBuilder<T> From<T>(this IDatabase db) where T : new()
        {
            return new GetBuilder<T>(db);
        }


        /// <summary>
        /// 获取实体数据
        /// </summary>
        /// <typeparam name="T">实体类型</typeparam>
        /// <param name="db">数据库实例</param>
        /// <param name="expression">查询条件表达式</param>
        /// <returns>查询构建器</returns>
        public static GetBuilder<T> Get<T>(this IDatabase db, Expression<Func<T, bool>> expression) where T : new()
        {
            return new GetBuilder<T>(db).Where(expression);
        }

        /// <summary>
        /// 获取实体数据
        /// </summary>
        /// <typeparam name="T">实体类型</typeparam>
        /// <param name="db">数据库实例</param>
        /// <param name="args">参数</param>
        /// <returns>查询构建器</returns>
        public static GetBuilder<T> Get<T>(this IDatabase db, List<object> args) where T : new()
        {
            return new GetBuilder<T>(db, args);
        }
        #endregion

        #region Delete
        /// <summary>
        /// 删除
        /// </summary>
        /// <typeparam name="T">实体类型</typeparam>
        /// <param name="db">数据库实例</param>
        /// <param name="expression">删除条件表达式</param>
        /// <returns>受影响的行</returns>
        public static int DeleteByWhereExpression<T>(this IDatabase db, Expression<Func<T, bool>> expression) where T : new()
        {
            return new DeleteBuilder<T>(db).Where(expression).Execute();
        }

        /// <summary>
        /// 删除
        /// </summary>
        /// <typeparam name="T">实体类型</typeparam>
        /// <param name="db">数据库实例</param>
        /// <param name="entity">要删除的实体</param>
        /// <returns>指示是否删除成功</returns>
        public static bool Delete<T>(this IDatabase db, T entity) where T : new()
        {
            return db.Delete(entity) > 0;
        }

        /// <summary>
        /// 删除
        /// </summary>
        /// <typeparam name="T">实体类型</typeparam>
        /// <param name="db">数据库实例</param>
        /// <returns>删除构建器</returns>
        public static DeleteBuilder<T> Delete<T>(this IDatabase db) where T : new()
        {
            return new DeleteBuilder<T>(db);
        }
        #endregion

        #region Set
        /// <summary>
        /// 更新
        /// </summary>
        /// <typeparam name="T">实体类型</typeparam>
        /// <param name="db">数据库实例</param>
        /// <returns>更新构建器</returns>
        public static SetBuilder<T> Update<T>(this IDatabase db) where T : IPropertyChanged,new()
        {
            return new SetBuilder<T>(db);
        }

        /// <summary>
        /// 更新
        /// </summary>
        /// <typeparam name="T">实体类型</typeparam>
        /// <param name="db">数据库实例</param>
        /// <param name="entity">要更新的实体</param>
        /// <returns>更新构建器</returns>
        public static bool Update<T>(this IDatabase db, T entity) where T : new()
        {
            return db.Update(entity) > 0;
        }
        #endregion

        //#region Add
        ///// <summary>
        ///// 添加
        ///// </summary>
        ///// <typeparam name="T">实体类型</typeparam>
        ///// <param name="db">数据库实例</param>
        ///// <param name="entity">要添加的实体</param>
        ///// <returns><c>true</c> if XXXX, <c>false</c> otherwise.</returns>
        //public static bool Add<T>(this Database db, T entity) where T : new()
        //{

        //    var pd = PocoData.ForType(entity.GetType(), db.DefaultMapper);
        //    return db.Insert(pd.TableInfo.TableName, pd.TableInfo.PrimaryKey, false, entity).ToBool_();
        //}

        ///// <summary>
        ///// 添加带自增 ID 的实体
        ///// </summary>
        ///// <typeparam name="T">返回的自增 ID 类型</typeparam>
        ///// <param name="db">数据库实例</param>
        ///// <param name="entity">要添加的实体</param>
        ///// <returns>自增 ID 的值</returns>
        //public static T Add<T>(this Database db, object entity)
        //{
        //    var pd = PocoData.ForType(entity.GetType());
        //    return (T)Convert.ChangeType(db.Insert(pd.TableInfo.TableName, pd.TableInfo.PrimaryKey, true, entity), typeof(T));
        //}
        //#endregion

        //#region 事务
        ///// <summary>
        ///// 在事务范围内处理
        ///// </summary>
        ///// <param name="db">数据库实例</param>
        ///// <param name="action">要处理操作</param>
        ///// <param name="succedAction">The succed action.</param>
        ///// <param name="failedAction">处理失败时的操作</param>
        ///// <returns>提示是否操作成功</returns>
        //public static bool InTransaction(this Database db, Action action, Action succedAction = null, Action failedAction = null)
        //{
        //    var r = false;
        //    db.BeginTransaction();
        //    try
        //    {
        //        action();
        //        db.CompleteTransaction();
        //        r = true;
        //        if(succedAction != null)
        //            succedAction();
        //    }
        //    catch(Exception)
        //    {
        //        db.AbortTransaction();
        //        r = false;
        //        if(failedAction != null)
        //            failedAction();
        //    }

        //    return r;
        //}

        ///// <summary>
        ///// 在事务范围内处理
        ///// </summary>
        ///// <param name="db">数据库实例</param>
        ///// <param name="action">要处理操作</param>
        ///// <param name="succeedAction">处理成功时的操作</param>
        ///// <param name="failedAction">处理失败时的操作</param>
        ///// <returns>提示是否操作成功</returns>
        //public static bool InTransaction(this Database db, Func<bool> action, Action succeedAction = null, Action failedAction = null)
        //{
        //    var r = false;
        //    db.BeginTransaction();
        //    try
        //    {
        //        if(action())
        //        {
        //            db.CompleteTransaction();
        //            r = true;
        //            if(succeedAction != null)
        //                succeedAction();
        //        }
        //        else
        //        {
        //            db.AbortTransaction();
        //            r = false;
        //            if(failedAction != null)
        //                failedAction();
        //        }
        //    }
        //    catch
        //    {
        //        db.AbortTransaction();
        //        r = false;
        //        if(failedAction != null)
        //            failedAction();
        //    }

        //    return r;
        //}
        //#endregion

		#region 获取表名、列名
		/// <summary>
		/// 获取字段列名称
		/// </summary>
		/// <param name="db">数据库实例</param>
		/// <param name="columnName">原始名称</param>
		/// <returns>转码后的字段列名称</returns>
        /// <exception cref="ArgumentException"><paramref name="columnName" /> 为空或 null</exception>
		public static string GetColumnName(this IDatabase db, string columnName)
		{
			if(columnName.IsNullOrEmpty_())
				throw new ArgumentException("columnName is null or empty");

			if(columnName == "*") return columnName;
			if(columnName.Contains('.')) return columnName;
			return db.Provider.EscapeSqlIdentifier(columnName);
		}

		/// <summary>
		/// 获取字段列名称
		/// </summary>
		/// <param name="db">数据库实例</param>
		/// <param name="type">实体类型</param>
		/// <param name="columnName">原始名称</param>
		/// <returns>转码后的字段列名称</returns>
        /// <exception cref="ArgumentException"><paramref name="columnName" /> 为空或 null</exception>
		/// <exception cref="System.NullReferenceException">找不到字段列名</exception>
		public static string GetColumnName(this IDatabase db, Type type, string columnName)
		{
			if(columnName.IsNullOrEmpty_())
				throw new ArgumentException("columnName is null or empty");

			if(columnName.Contains('.')) return columnName;

			var cols = PocoData.ForType(type,db.DefaultMapper).Columns;
			if(!cols.ContainsKey(columnName))
			{
				var c = cols.Values.SingleOrDefault(p => p.PropertyInfo.Name == columnName);
				if(c == null)
                    throw new NullReferenceException(string.Format("IDatabase Entity type:{0} or column name:{1} is null or emtpy", type.Name, columnName));
				columnName = c.ColumnName;
			}

			return GetColumnName(db, columnName);
		}

		/// <summary>
		/// 获取表名称
		/// </summary>
		/// <typeparam name="T">实体类型</typeparam>
		/// <param name="db">数据库实例</param>
		/// <param name="columnName">原字段列名称</param>
		/// <returns>转码后的字段列名称</returns>
		public static string GetColumnName<T>(this IDatabase db, string columnName)
		{
			return GetColumnName(db, typeof(T), columnName);
		}

		/// <summary>
		/// 获取表名称
		/// </summary>
		/// <typeparam name="T">实体类型</typeparam>
		/// <param name="db">数据库实例</param>
		/// <param name="entity">实体</param>
		/// <param name="columnName">原字段列名称</param>
		/// <returns>转码后的字段列名称</returns>
		public static string GetColumnName<T>(this IDatabase db, T entity, string columnName)
		{
			return GetColumnName(db, typeof(T), columnName);
		}

		/// <summary>
		/// 获取字段列名称
		/// </summary>
		/// <param name="db">数据库实例</param>
		/// <param name="expression">字段表达式</param>
		/// <returns>转码后的字段列名称</returns>
		public static string GetColumnName(this IDatabase db, Expression expression)
		{
			return GetColumnName(db, Utils.GetPropertyName(expression));
		}

		/// <summary>
		/// 获取字段列名称
		/// </summary>
		/// <typeparam name="T">实体类型</typeparam>
		/// <param name="db">数据库实例</param>
		/// <param name="expression">字段表达式</param>
		/// <returns>转码后的字段列名称</returns>
		public static string GetColumnName<T>(this IDatabase db, Expression expression)
		{
			return GetColumnName(db, typeof(T), Utils.GetPropertyName(expression));
		}

		/// <summary>
		/// 获取字段列名称
		/// </summary>
		/// <typeparam name="T">实体类型</typeparam>
		/// <param name="db">数据库实例</param>
		/// <param name="expression">字段表达式</param>
		/// <returns>转码后的字段列名称</returns>
		public static string GetColumnName<T>(this IDatabase db, Expression<Func<T, object>> expression)
		{
			return GetColumnName(db, typeof(T), Utils.GetPropertyName(expression));
		}

		/// <summary>
		/// 获取表名称
		/// </summary>
		/// <param name="db">数据库实例</param>
		/// <param name="tableName">原始表名</param>
		/// <returns>转码后的表名</returns>
        /// <exception cref="ArgumentException"><paramref name="tableName" /> 为空或 null</exception>
		public static string GetTableName(this IDatabase db, string tableName)
		{
			if(tableName.IsNullOrEmpty_())
				throw new ArgumentException("tableName is null or empty");

			return db.Provider.EscapeTableName(tableName);
		}

		/// <summary>
		/// 获取表名称
		/// </summary>
		/// <param name="db">数据库实例</param>
		/// <param name="type">实体类型</param>
		/// <returns>转码后的表名</returns>
		public static string GetTableName(this IDatabase db, Type type)
		{
			return GetTableName(db, PocoData.ForType(type,db.DefaultMapper).TableInfo.TableName);
		}

		/// <summary>
		/// 获取表名称
		/// </summary>
		/// <typeparam name="T">实体类型</typeparam>
		/// <param name="db">数据库实例</param>
		/// <returns>转码后的表名</returns>
		public static string GetTableName<T>(this IDatabase db)
		{
			return GetTableName(db, typeof(T));
		}

		/// <summary>
		/// 获取表名称
		/// </summary>
		/// <typeparam name="T">实体类型</typeparam>
		/// <param name="db">数据库实例</param>
		/// <param name="entity">实体</param>
		/// <returns>转码后的表名</returns>
		public static string GetTableName<T>(this IDatabase db, T entity)
		{
			return GetTableName(db, typeof(T));
		}

		/// <summary>
		/// 按字段表达式获取表名称
		/// </summary>
		/// <param name="db">数据库实例</param>
		/// <param name="expression">字段表达式</param>
		/// <returns>转码后的表名</returns>
		public static string GetTableName(this IDatabase db, Expression expression)
		{
            return GetTableName(db, Utils.GetPropertyInfo(expression).ReflectedType);
		}

		/// <summary>
		/// 按字段表达式获取表名称
		/// </summary>
		/// <typeparam name="T">实体类型</typeparam>
		/// <param name="db">数据库实例</param>
		/// <param name="expression">字段表达式</param>
		/// <returns>转码后的表名</returns>
		public static string GetTableName<T>(this IDatabase db, Expression expression)
		{
			return GetTableName(db, typeof(T));
		}

		/// <summary>
		/// 按字段表达式获取表名称
		/// </summary>
		/// <typeparam name="T">实体类型</typeparam>
		/// <param name="db">数据库实例</param>
		/// <param name="expression">字段表达式</param>
		/// <returns>转码后的表名</returns>
		public static string GetTableName<T>(this IDatabase db, Expression<Func<T, object>> expression)
		{
			return GetTableName(db, typeof(T));
		}

		/// <summary>
		/// 获取表名称和字段列名称
		/// </summary>
		/// <param name="db">数据库实例</param>
		/// <param name="tableName">表名称</param>
		/// <param name="columnName">字段列名称</param>
		/// <returns>组合后的表名称和字段列名称</returns>
        /// <exception cref="ArgumentException"><paramref name="tableName" /> 或 <paramref name="columnName" /> 为空或 null</exception>
		public static string GetTableAndColumnName(this IDatabase db, string tableName, string columnName)
		{
			if(tableName.IsNullOrEmpty_())
				throw new ArgumentException("tableName is null or empty");
			if(columnName.IsNullOrEmpty_())
                throw new ArgumentException("columnName is null or empty");

			if(columnName.Contains('.')) return columnName;
			return GetTableName(db, tableName) + "." + GetColumnName(db, columnName);
		}

		/// <summary>
		/// 获取表名称和字段列名称
		/// </summary>
		/// <param name="db">数据库实例</param>
		/// <param name="type">实体类型</param>
		/// <returns>组合后的表名称和字段列名称</returns>
		public static string GetTableAndColumnName(this IDatabase db, Type type)
		{
			return GetTableAndColumnName(db, GetTableName(db, type), "*");
		}

		/// <summary>
		/// 获取表名称和字段列名称
		/// </summary>
		/// <typeparam name="T">实体类型</typeparam>
		/// <param name="db">数据库实例</param>
		/// <returns>组合后的表名称和字段列名称</returns>
		public static string GetTableAndColumnName<T>(this IDatabase db)
		{
			return GetTableAndColumnName(db, GetTableName(db, typeof(T)), "*");
		}

		/// <summary>
		/// 获取表名称和字段列名称
		/// </summary>
		/// <typeparam name="T">实体类型</typeparam>
		/// <param name="db">数据库实例</param>
		/// <param name="entity">实体</param>
		/// <returns>组合后的表名称和字段列名称</returns>
		public static string GetTableAndColumnName<T>(this IDatabase db, T entity)
		{
			return GetTableAndColumnName(db, GetTableName(db, typeof(T)), "*");
		}

		/// <summary>
		/// 获取表名称和字段列名称
		/// </summary>
		/// <param name="db">数据库实例</param>
		/// <param name="type">实体类型</param>
		/// <param name="columnName">原字段列名称</param>
		/// <returns>组合后的表名称和字段列名称</returns>
        /// <exception cref="ArgumentException"><paramref name="columnName" /> 为空或 null</exception>
		/// <exception cref="System.NullReferenceException">找不到字段列名</exception>
		public static string GetTableAndColumnName(this IDatabase db, Type type, string columnName)
		{
			if(columnName.IsNullOrEmpty_())
				throw new ArgumentException("columnName is null or emtpy");

			if(columnName.Contains('.')) return columnName;

			var pd = PocoData.ForType(type,db.DefaultMapper);
			var cols = pd.Columns;
			if(!cols.ContainsKey(columnName))
			{
				var c = cols.Values.SingleOrDefault(p => p.PropertyInfo.Name == columnName);
				if(c == null)
					throw new NullReferenceException(string.Format("IDatabase Entity type:{0} or column name:{1} is null or emtpy",type.Name, columnName));
				columnName = c.ColumnName;
			}

			return GetTableAndColumnName(db, pd.TableInfo.TableName, columnName);
		}

		/// <summary>
		/// 获取表名称和字段列名称
		/// </summary>
		/// <typeparam name="T">实体类型</typeparam>
		/// <param name="db">数据库实例</param>
		/// <param name="columnName">原字段列名称</param>
		/// <returns>组合后的表名称和字段列名称</returns>
		public static string GetTableAndColumnName<T>(this IDatabase db, string columnName)
		{
			return GetTableAndColumnName(db, typeof(T), columnName);
		}

		/// <summary>
		/// 获取表名称和字段列名称
		/// </summary>
		/// <typeparam name="T">实体类型</typeparam>
		/// <param name="db">数据库实例</param>
		/// <param name="entity">实体</param>
		/// <param name="columnName">原字段列名称</param>
		/// <returns>组合后的表名称和字段列名称</returns>
		public static string GetTableAndColumnName<T>(this IDatabase db, T entity, string columnName)
		{
			return GetTableAndColumnName(db, typeof(T), columnName);
		}

		/// <summary>
		/// 获取表名称和字段列名称
		/// </summary>
		/// <param name="db">数据库实例</param>
		/// <param name="expression">字段表达式</param>
		/// <returns>组合后的表名称和字段列名称</returns>
		public static string GetTableAndColumnName(this IDatabase db, Expression expression)
		{
			var pi = Utils.GetPropertyInfo(expression);
			return GetTableAndColumnName(db, pi.ReflectedType, pi.Name);
		}

		/// <summary>
		/// 获取表名称和字段列名称
		/// </summary>
		/// <typeparam name="T">实体类型</typeparam>
		/// <param name="db">数据库实例</param>
		/// <param name="expression">字段表达式</param>
		/// <returns>组合后的表名称和字段列名称</returns>
		public static string GetTableAndColumnName<T>(this IDatabase db, Expression expression)
		{
			return GetTableAndColumnName(db, typeof(T), Utils.GetPropertyName(expression));
		}

		/// <summary>
		/// 获取表名称和字段列名称
		/// </summary>
		/// <typeparam name="T">实体类型</typeparam>
		/// <param name="db">数据库实例</param>
		/// <param name="expression">字段表达式</param>
		/// <returns>组合后的表名称和字段列名称</returns>
		public static string GetTableAndColumnName<T>(this IDatabase db, Expression<Func<T, object>> expression)
		{
			return GetTableAndColumnName(db, typeof(T), Utils.GetPropertyName(expression));
		}

        /// <summary>
        /// 获取表名称和字段列名称
        /// </summary>
        /// <typeparam name="T">实体类型</typeparam>
        /// <param name="db">数据库实例</param>
        /// <param name="expression">字段表达式</param>
        /// <returns>组合后的表名称和字段列名称</returns>
        public static string[] GetTableAndColumnNameByObject(this IDatabase db, Expression expression)
        {
            List<string> listColumnName = new List<string>();
            var propertyMemberExpression = Utils.GetPropertyNameByObject(expression);
            propertyMemberExpression.ForEach_(p => listColumnName.Add(db.GetTableAndColumnName((Expression)p)));
            return listColumnName.ToArray();
        }

        /// <summary>
        /// 获取表名称和字段列名称
        /// </summary>
        /// <typeparam name="T">实体类型</typeparam>
        /// <param name="db">数据库实例</param>
        /// <param name="expression">字段表达式</param>
        /// <returns>组合后的表名称和字段列名称</returns>
        public static string[] GetTableAndColumnNameByObject<T>(this IDatabase db, Expression<Func<T, object>> expression)
        {

            return GetTableAndColumnNameByObject(db, (Expression)expression);
        }

        /// <summary>
        /// 获取表名称和字段列名称
        /// </summary>
        /// <typeparam name="T">实体类型</typeparam>
        /// <param name="db">数据库实例</param>
        /// <param name="expression">字段表达式</param>
        /// <returns>组合后的表名称和字段列名称</returns>
        public static string[] GetTableAndColumnNameByObject<T,T1>(this IDatabase db, Expression<Func<T,T1, object>> expression)
        {

            return GetTableAndColumnNameByObject(db, (Expression)expression);


        }


		#endregion

		/// <summary>
		/// 调用 SQL IN 函数
		/// </summary>
		/// <typeparam name="T">字段栏数据库类型</typeparam>
		/// <typeparam name="Table">数据库表数据库类型</typeparam>
		/// <param name="field">字段栏</param>
		/// <param name="gb">查询构建器</param>
		/// <returns>表示此SQL 函数的返回值类型。只能在 SQL 构建器中调用，当直接调用此 SQL 函数时将引发 <see cref="NotSupportedException"/> 异常。</returns>
		/// <exception cref="NotSupportedException">只能在 SQL 构建器中调用，当直接调用此 SQL 函数时将引发此异常。</exception>
		public static bool In_<T, Table>(this T field, GetBuilder<Table> gb)
			where T : struct
			where Table : class, new()
		{
			throw new NotSupportedException("不能直接调用此方法，只能在 SQL 构建器中调用。");
		}


    }
}
