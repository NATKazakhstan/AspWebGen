namespace Nat.Web.Controls.GenerationClasses.BaseJournal
{
    using System;
    using System.Linq;
    using System.Linq.Expressions;

    using Nat.Web.Controls.GenerationClasses.Filter;

    public class BaseFilterParameterFake<TTable, TField> : BaseFilterParameter<TTable, TField>
        where TTable : class
        where TField : struct
    {
        protected internal override Expression OnFilter(Enum filtertype, FilterItem filterItem, QueryParameters qParams)
        {
            return null;
        }

        protected override IQueryable OnFilter(IQueryable query, Enum filtertype, string value1, string value2)
        {
            return query;
        }
    }
}