using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace DBEntityGenerate.Linq.SqlOperator
{
    public enum FunctionOperatorType
    {
        /// <summary>等于（累计数量）</summary>
        [Description("累计数量")]
        Sum,
        /// <summary>等于（统计数量）</summary>
        [Description("统计数量")]
        Count,
    }
}
