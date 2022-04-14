using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace SWE3.Demo.ExpressionVisitors
{
    public class SelectExpressionVisitor : ExpressionVisitor
    {
        public List<String> Members { get; set; } = new List<string>();

        protected override Expression VisitMember(MemberExpression node)
        {
            Expression expr = base.VisitMember(node);
            this.Members.Add(node.Member.Name);
            return expr;
        }
    }
}