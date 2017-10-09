using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Linq.Expressions;

namespace Nat.Web.Controls.GenerationClasses.Data
{
    public abstract class BaseTableParameter
    {
        public abstract Expression GetExpression(bool isKz, Expression item);
        public abstract Type GetResultDataType();
    }
}
