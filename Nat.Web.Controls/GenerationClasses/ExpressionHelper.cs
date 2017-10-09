using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Linq.Expressions;

namespace Nat.Web.Controls.GenerationClasses
{
    public static class ExpressionHelper
    {
        public static Expression And(params Expression[] expressions)
        {
            Expression exp = null;
            for (int i = 0; i < expressions.Length; i++)
                if (exp == null) exp = expressions[i];
                else exp = Expression.And(exp, expressions[i]);
            return exp;
        }
    }
}
