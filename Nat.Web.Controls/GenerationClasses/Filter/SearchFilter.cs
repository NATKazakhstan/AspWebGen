namespace Nat.Web.Controls.GenerationClasses.Filter
{
    using System.Collections.Generic;

    public class SearchFilter
    {
        private readonly IList<FilterHtmlGenerator.Filter> filters;

        public SearchFilter(IList<FilterHtmlGenerator.Filter> filters)
        {
            this.filters = filters;
        }

        public FilterHtmlGenerator.Filter Filter { get; private set; }

        public int FilterIndex { get; private set; }

        public IList<FilterHtmlGenerator.Filter> FiltersCollection { get; private set; }

        public bool Find(string filterName)
        {
            Filter = null;
            FiltersCollection = null;
            FilterIndex = -1;
            return Find(filters, filterName);
        }

        protected bool Find(IList<FilterHtmlGenerator.Filter> items, string filterName)
        {
            for (int index = 0; index < items.Count; index++)
            {
                var filter = items[index];
                if (filter.FilterName == filterName)
                {
                    this.Filter = filter;
                    this.FiltersCollection = items;
                    this.FilterIndex = index;
                    return true;
                }

                if (this.Find(filter.Children, filterName))
                    return true;
            }

            return false;
        }
    }
}