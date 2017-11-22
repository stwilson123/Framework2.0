using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DBORM
{
    public static class Clone
    {
        /// <summary>克隆对象</summary>
        /// <typeparam name="T">对象类型</typeparam>
        /// <param name="obj">要克隆的对象</param>
        /// <returns>新对象</returns>
        public static T Clone_<T>(this T obj)
        {
            if (obj == null) return default(T);

            var type = obj.GetType();
            if (type.IsValueType)
                return obj;

            var id = obj as IDictionary;
            if (id != null)
                return (T)CloneDict(id);

            var il = obj as IList;
            if (il != null)
                return (T)CloneList(il);

            var ic = obj as ICloneable;
            if (ic != null)
                return (T)ic.Clone();

            var newObj = FastReflection.FastInvoke<T>();
            var ls = FastReflection.FastGetAccessors(type);
            foreach (var a in ls.Values)
            {
                if (!a.CanRade || !a.CanWrite)
                    continue;

                a.SetValue(newObj, Clone_(a.GetValue(obj)));
            }
            return newObj;
        }


        private static IList CloneList(IList list)
        {
            if (list == null) return null;

            IList ls = list.GetType().FastInvoke(list.Count) as IList;
            if (ls == null) return null;

            foreach (var item in list)
                ls.Add(Clone_(item));
            return ls;
        }

        private static IDictionary CloneDict(IDictionary list)
        {
            if (list == null) return null;

            IDictionary ls = list.GetType().FastInvoke(list.Count) as IDictionary;
            if (ls == null) return null;

            foreach (DictionaryEntry item in list)
                ls.Add(Clone_(item.Key), Clone_(item.Value));
            return ls;
        }
    }
}
