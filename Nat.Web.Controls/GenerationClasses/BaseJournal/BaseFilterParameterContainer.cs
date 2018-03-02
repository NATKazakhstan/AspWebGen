namespace Nat.Web.Controls.GenerationClasses.BaseJournal
{
    using System;
    using System.Collections.Generic;
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

        public override void ClearState()
        {
            base.ClearState();

            AppliedFilters.Clear();
        }

        public abstract void AddFilterValue(
            BaseFilterParameter<TTable> baseFilterParameter,
            Enum filtertype,
            FilterItem filterItem,
            QueryParameters qParams);

        public abstract Expression GetFilter(QueryParameters qParams);

        protected List<BaseFilterParameter> AppliedFilters { get; } = new List<BaseFilterParameter>();

        public virtual IEnumerable<BaseFilterParameter> GetAppliedFilters()
        {
            return AppliedFilters;
        }
    }
}