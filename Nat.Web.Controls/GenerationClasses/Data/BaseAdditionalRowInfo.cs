using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.Linq;
using System.Linq.Expressions;

namespace Nat.Web.Controls.GenerationClasses.Data
{
    public abstract class BaseAdditionalRowInfo
    {
        public abstract Expression GetExpression(DataContext db, Expression item, params Expression[] items);
    }
}
