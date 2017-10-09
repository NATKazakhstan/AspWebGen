using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Web;

namespace Nat.Web.Controls.GenerationClasses.BaseJournal
{
    public class JournalFilterParameter<TTable> : BaseFilterParameter<TTable>
        where TTable : class
    {
        public JournalFilterParameter(Expression<Func<TTable, bool>> where, bool applyForHeader)
            : base(where)
        {
            ApplyForHeader = applyForHeader;
        }

        public bool ApplyForHeader { get; set; }

        public override Expression OValueExpression
        {
            get { return null; }
        }
    }

    public class JournalFilterParameter<TTable, TField> : JournalFilterParameter<TTable>
        where TTable : class
        where TField : struct
    {
        protected JournalFilterParameter(Expression<Func<TTable, bool>> where, bool applyForHeader) : base(where, applyForHeader)
        {
        }

        public JournalFilterParameter(Expression<Func<TTable, TField?>> valueExpression)
            : base(null, false)
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

    public class JournalFilterParameter<TTable, TField, THeaderTable> : JournalFilterParameter<TTable, TField>
        where TTable : class
        where TField : struct
        where THeaderTable : class
    {
        protected Expression<Func<THeaderTable, IQueryable<TTable>>> GetTableDataByHeader { get; set; }

        public JournalFilterParameter(Expression<Func<TTable, TField?>> valueExpression, Expression<Func<THeaderTable, IQueryable<TTable>>> getTableDataByHeader)
            : base(null, true)
        {
            ValueExpression = valueExpression;
            GetTableDataByHeader = getTableDataByHeader;
        }
    }
}
