using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;

namespace Framework.Util.BaseTypeMethodExpend
{
    public partial class Utils
    {
        #region GetPropertyInfo
        /// <summary>获取属性信息</summary>
        /// <typeparam name="T">实体类型</typeparam>
        /// <param name="expression">属性表达式</param>
        /// <returns>属性信息</returns>
        public static PropertyInfo GetPropertyInfo<T>(Expression<Func<T, object>> expression)
        {

            return GetPropertyInfo((Expression)expression);
        }

        /// <summary>获取属性信息</summary>
        /// <typeparam name="T">实体类型</typeparam>
        /// <typeparam name="V">属性值类型</typeparam>
        /// <param name="expression">属性表达式</param>
        /// <returns>属性信息</returns>
        public static PropertyInfo GetPropertyInfo<T, V>(Expression<Func<T, V>> expression)
        {
            return GetPropertyInfo((Expression)expression);
        }

        /// <summary>获取属性信息</summary>
        /// <param name="expression">属性表达式</param>
        /// <returns>属性信息</returns>
        /// <exception cref="System.InvalidOperationException"></exception>
        public static PropertyInfo GetPropertyInfo(Expression expression)
        {
            var body = expression;
            var le = body as LambdaExpression;
            if (le != null)
                body = le.Body;
            var ue = body as UnaryExpression;
            if (ue != null)
                body = ue.Operand;
            var me = body as MemberExpression;
            if (me != null)
            {
                var et = me.Expression.Type;
                var mm = me.Member;
                if (mm.ReflectedType == et)
                    return mm as PropertyInfo;
                return et.GetProperty(mm.Name);
            }

            throw new InvalidOperationException();
        }

        /// <summary>获取属性信息</summary>
        /// <typeparam name="T">实体类型</typeparam>
        /// <param name="expression">属性表达式</param>
        /// <returns>属性信息</returns>
        /// <exception cref="System.InvalidOperationException"></exception>
        public static PropertyInfo GetPropertyInfo<T>(Expression expression)
        {
            return GetPropertyInfo(expression);
        }
        #endregion

        #region GetPropertyName
        /// <summary>获取属性名称</summary>
        /// <typeparam name="T">实体类型</typeparam>
        /// <param name="expression">属性表达式</param>
        /// <returns>属性名称</returns>
        public static string GetPropertyName<T>(Expression<Func<T, object>> expression)
        {
            return GetPropertyName((Expression)expression);
        }

        /// <summary>获取属性名称</summary>
        /// <typeparam name="T">实体类型</typeparam>
        /// <typeparam name="V">属性值类型</typeparam>
        /// <param name="expression">属性表达式</param>
        /// <returns>属性名称</returns>
        public static string GetPropertyName<T, V>(Expression<Func<T, V>> expression)
        {
            return GetPropertyName((Expression)expression);
        }

        /// <summary>获取属性名称</summary>
        /// <param name="expression">属性表达式</param>
        /// <returns>属性名称</returns>
        public static string GetPropertyName(Expression expression)
        {
            var body = expression;
            var le = body as LambdaExpression;
            if (le != null)
                body = le.Body;
            var ue = body as UnaryExpression;
            if (ue != null)
                body = ue.Operand;
            var me = body as MemberExpression;
            if (me != null)
                return me.Member.Name;

            throw new InvalidOperationException();
        }

        /// <summary>获取属性名称</summary>
        /// <typeparam name="T">实体类型</typeparam>
        /// <param name="expression">属性表达式</param>
        /// <returns>属性名称</returns>
        public static Expression[] GetPropertyNameByObject(Expression expression)
        {
          
            var body = (Expression)expression;
            var le = body as LambdaExpression;
            if (le != null)
                body = le.Body;
            var ne = body as NewExpression;

            if (ne != null)

                return ne.Arguments.ToArray();
           
            throw new InvalidOperationException();
        }
       

        /// <summary>获取属性名称</summary>
        /// <typeparam name="T">实体类型</typeparam>
        /// <param name="expression">属性表达式</param>
        /// <returns>属性名称</returns>
        public static string GetPropertyName<T>(Expression expression)
        {
            return GetPropertyName(expression);
        }


        ///// <summary>获取属性名称</summary>
        ///// <param name="expression">属性表达式</param>
        ///// <returns>属性名称</returns>
        //public static string[] GetPropertyNameByObject(Expression expression)
        //{
        //    var body = expression;
        //    var le = body as LambdaExpression;
        //    if (le != null)
        //        body = le.Body;
        //    var ue = body as UnaryExpression;
        //    if (ue != null)
        //        body = ue.Operand;
        //    var me = body as MemberExpression;
        //    if (me != null)
        //        return me.Member.Name;

        //    throw new InvalidOperationException();
        //}
        #endregion
    }


    public static class CollectionHelper
    {
        public enum OrderType
        {
            DESC,
            ASC
        }
        public static bool IsNullOrEmpty(IList iCollection)
        {

            return iCollection == null || iCollection.Count == 0;
        }


        public static IList<IList> SplitCollection(IList ListSource, int ListCapacity)
        {

            IList<IList> listSplit = new List<IList>();
            if (CollectionHelper.IsNullOrEmpty(ListSource))
                return listSplit;

            for (int i = 0; i < ListSource.Count / ListCapacity + 1 && i < ListSource.Count; i++)
            {
                //var listTemp = ListSource.Skip(i * ListCapacity).TakeWhile((num, index) => index < ListCapacity).ToList();
                var listTemp = new ArrayList();
                for (int j = i * ListCapacity; j < i * ListCapacity + ListCapacity && j < ListSource.Count; j++)
                {
                    listTemp.Add(ListSource[j]);
                }

                if (listTemp != null && listTemp.Count > 0)
                    listSplit.Add(listTemp);
            }

            return listSplit;
        }

        //public static List<Dictionary<TKey, TValue>> SplitDic<TKey, TValue>(Dictionary<TKey, TValue> dicSource, int capacity)
        //{
        //    List<Dictionary<TKey, TValue>> listSplit = new List<Dictionary<TKey, TValue>>();
        //    if (CollectionHelper.IsNullOrEmpty(dicSource))
        //        return listSplit;

        //    for (int i = 0; i < dicSource.Count / capacity + 1 && i < dicSource.Count; i++)
        //    {
        //        var dicTemp = dicSource.Skip(i * capacity).TakeWhile((pair, index) => index < capacity);
        //        if (dicTemp.Any())
        //        {
        //            listSplit.Add(dicTemp.ToDictionary(pair=>pair.Key,pair=>pair.Value));
        //        }

        //    }
        //    return listSplit;
        //}

        private static IQueryable<TEntity> OrderBy<TEntity>(this IQueryable<TEntity> source, OrderType orderType, string propertyStr) where TEntity : class
        {
            ParameterExpression param = Expression.Parameter(typeof(TEntity), "c");
            PropertyInfo property = typeof(TEntity).GetProperty(propertyStr);
            Expression propertyAccessExpression = Expression.MakeMemberAccess(param, property);
            LambdaExpression le = Expression.Lambda(propertyAccessExpression, param);
            Type type = typeof(TEntity);
            string strOrderby = orderType == OrderType.ASC ? "OrderBy" : "OrderByDescending";
            MethodCallExpression resultExp = Expression.Call(typeof(Queryable), strOrderby, new Type[] { type, property.PropertyType }, source.Expression, Expression.Quote(le));
            return source.Provider.CreateQuery<TEntity>(resultExp);
        }

        public static IQueryable<TEntity> OrderBy<TEntity>(this IQueryable<TEntity> source, string propertyStr) where TEntity : class
        {
            return OrderBy<TEntity>(source, OrderType.ASC, propertyStr);
        }

        public static IQueryable<TEntity> OrderByDescending<TEntity>(this IQueryable<TEntity> source, string propertyStr) where TEntity : class
        {
            return OrderBy<TEntity>(source, OrderType.DESC, propertyStr);
        }

        //public static List<List<T>> GetListCollection(List<T> listICollection)  
        //{
        //    int listNum = 1;
        //    List<List<T>> listGroup = new List<List<T>>();
        //    int i = 0;
        //    for (i = 0; i < listICollection.Count; i += listNum)
        //    {
        //        listGroup.Add(listICollection.Skip<T>(i * listNum).Take<T>(listNum).ToList());
        //    }
        //    listGroup.Add(listICollection.Skip<T>(i * listNum).ToList());
        //    return listGroup;

        //}
        public static T Clone<T>(T RealObject)
        {
            using (Stream objectStream = new MemoryStream())
            {
                //利用 System.Runtime.Serialization序列化与反序列化完成引用对象的复制
                IFormatter formatter = new BinaryFormatter();
                formatter.Serialize(objectStream, RealObject);
                objectStream.Seek(0, SeekOrigin.Begin);
                return (T)formatter.Deserialize(objectStream);
            }
        }
    }
}
