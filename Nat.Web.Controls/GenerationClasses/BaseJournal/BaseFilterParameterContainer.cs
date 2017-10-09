namespace Nat.Web.Controls.GenerationClasses.BaseJournal
{
    using System;
    using System.Linq.Expressions;

    using Nat.Web.Controls.GenerationClasses.Filter;

    public abstract class BaseFilterParameterContainer<TTable> : BaseFilterParameter<TTable>, IBaseFilterParameterContainer
        where TTable : class
    {
        protected BaseFilterParameterContainer()
        {
            Visible = false;
        }
        
        protected internal override Expression OnFilter(Enum filtertype, FilterItem filterItem, QueryParameters qParams)
        {
            return null;
        }

        public abstract void AddFilterValue(
            BaseFilterParameter<TTable> baseFilterParameter,
            Enum filtertype,
            FilterItem filterItem,
            QueryParameters qParams);

        public abstract Expression GetFilter(QueryParameters qParams);
    }
}