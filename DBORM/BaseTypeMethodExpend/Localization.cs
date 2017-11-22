using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Framework.Util.BaseTypeMethodExpend
{
    public static partial class Utils
    {

        #region GetLocalization
        /// <summary>获取本地化资源</summary>
        /// <param name="e">要获取的枚举值</param>
        /// <param name="res">所在的本地化资源</param>
        /// <param name="info">区域性信息</param>
        /// <returns>本地化资源</returns>
        public static string GetLocalization_(this Enum e, System.Resources.ResourceManager res, System.Globalization.CultureInfo info = null)
        {
            var type = e.GetType();
            var name = type.Name + "_" + Enum.GetName(type, e);
            return GetLocalization_(name, res, info);
        }

        /// <summary>获取本地化资源</summary>
        /// <param name="name">要获取的资源名称</param>
        /// <param name="res">所在的本地化资源</param>
        /// <param name="info">区域性信息</param>
        /// <returns>本地化资源</returns>
        public static string GetLocalization_(string name, System.Resources.ResourceManager res, System.Globalization.CultureInfo info = null)
        {
            if (name.IsNullOrEmpty_()) return string.Empty;
            return res.GetString(name, info);
        }
        #endregion
    }
}
