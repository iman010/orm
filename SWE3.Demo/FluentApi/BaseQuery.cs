using SWE3.Demo.ExpressionVisitors;
using SWE3.Demo.Extensions;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;

namespace SWE3.Demo.FluentApi
{
    public struct QueryPart
    {
        public string Name;
        public string TableNameInDB;
        public Type type;
    }
}

