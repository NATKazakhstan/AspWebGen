namespace Nat.Web.Controls.GenerationClasses.BaseJournal
{
    using System;
    using System.Data.Linq;
    using System.Linq.Expressions;

    using Nat.Web.Controls.GenerationClasses.Filter;

    public class CustomBooleanFilterParameter<TDataContext, TTable> : BaseFilterParameter<TTable>
        where TTable : class
        where TDataContext : DataContext
    {
        private readonly Expression<Func<TDataContext, TTable, bool>> checkExpression;

        public CustomBooleanFilterParameter(Expression<Func<TDataContext, TTable, bool>> checkExpression)
        {
            Type = FilterHtmlGenerator.FilterType.Boolean;
            Mandatory = true;
            IsJournalFilter = true;

            this.checkExpression = checkExpression;
        }

        public override Expression OValueExpression
        {
            get { return checkExpression; }
        }

        protected override void EqualsExpression(string strValue, Type tableType)
        {
            var param = Expression.Parameter(tableType, "bFilter");
            Expression filter = Expression.Invoke(OValueExpression, QueryParameters.GetDBExpression<TDataContext>(), param);
            if (QueryParameters != null) QueryParameters.AddUniqueParameter(filter);
            SetWhereExpression(filter, param);
        }

        protected override void NotEqualsExpression(string strValue, Type tableType)
        {
            var param = Expression.Parameter(tableType, "bFilter");
            Expression filter = Expression.Invoke(OValueExpression, QueryParameters.GetDBExpression<TDataContext>(), param);
            filter = Expression.Not(filter);
            if (QueryParameters != null) QueryParameters.AddUniqueParameter(filter);
            SetWhereExpression(filter, param);
        }
    }
}