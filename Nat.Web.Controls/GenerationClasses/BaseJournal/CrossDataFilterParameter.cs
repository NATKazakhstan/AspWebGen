using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Web;

namespace Nat.Web.Controls.GenerationClasses.BaseJournal
{
    public class CrossDataFilterParameter<TTable> : BaseFilterParameter<TTable>
        where TTable : class
    {
        public override Expression OValueExpression
        {
            get { return null; }
        }
    }

    public class CrossDataFilterParameter<TTable, TField> : CrossDataFilterParameter<TTable>
        where TTable : class
        where TField : struct
    {
        public CrossDataFilterParameter()
        {
        }

        public CrossDataFilterParameter(Expression<Func<TTable, TField?>> valueExpression)
        {
            ValueExpression = valueExpression;
        }

        public Expression<Func<TTable, TField?>> ValueExpression { get; set; }

        public override Expression OValueExpression
        {
            get { return ValueExpression; }
        }

        protected override Type FieldType
        {
            get
            {
                return typeof(TField);
            }
        }
    }
}
