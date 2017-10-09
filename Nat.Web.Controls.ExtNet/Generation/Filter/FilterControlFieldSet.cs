/*
 * Created by: Alexey V. Vorochsak
 * Created: 2013.07.04
 * Copyright © JSC NAT Kazakhstan 2013
 */

namespace Nat.Web.Controls.ExtNet.Generation.Filter
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Ext.Net;
    using GenerationClasses;

    class FilterControlFieldSet:FilterControlBase
    {
        internal FieldSet GetFieldSetFilters(
            IList<FilterHtmlGenerator.Filter> filters, 
            IList<string> clientRepository)
        {
            return GetFilledFieldSet(filters.Where(f => f.Visible), clientRepository, true);
        }

        private FieldSet GetFilledFieldSet(
            IEnumerable<FilterHtmlGenerator.Filter> filters, 
            IList<string> clientRepositoryJs,
            bool isMainFilterFieldSet,
            bool? isHideParentFieldSet = null,
            string headerGroup = null)
        {
            var enumerable = filters as IList<FilterHtmlGenerator.Filter> ?? filters.ToList();
            var isAdditionalFieldSet = IsAdditionalFieldSet(enumerable);
            var fieldSet = new FieldSet(StandartConfigFiledSet(headerGroup, isAdditionalFieldSet, isAdditionalFieldSet));
            if (isMainFilterFieldSet)
            {
                fieldSet.ID = "MainFilterFieldSet";
                fieldSet.ClientIDMode = System.Web.UI.ClientIDMode.Static;
            }

            filters = SortFilters(enumerable);

            foreach (var filter in filters)
            {
                string controlMemberJs;
                var control = GetFilterControlsByType(filter, clientRepositoryJs, isAdditionalFieldSet, isHideParentFieldSet, out controlMemberJs);
                if (control == null) continue;
                if (!filter.MainGroup)
                    control.Hidden = true;
                fieldSet.Items.Add(control);
                
                if (!string.IsNullOrEmpty(controlMemberJs))
                {
                    clientRepositoryJs.Add(controlMemberJs);
                }
            }

            return fieldSet;
        }

        private bool IsAdditionalFieldSet(IEnumerable<FilterHtmlGenerator.Filter> filters)
        {
            var list = new List<FilterHtmlGenerator.Filter>();
            GetListFilters(filters, list);
            return list.All(f => !f.MainGroup);
        }

        private void GetListFilters(IEnumerable<FilterHtmlGenerator.Filter> filters, List<FilterHtmlGenerator.Filter> listFilters)
        {
            foreach (var filter in filters.Where(f => f.Visible))
            {
                if (filter.Type != FilterHtmlGenerator.FilterType.Group)
                listFilters.Add(filter);

                var childs = filter.Children.ToList();

                if (childs.Any())
                {
                    GetListFilters(childs, listFilters);
                }
            }
        }

        private AbstractComponent GetFilterControlsByType(
            FilterHtmlGenerator.Filter filterData, 
            IList<string> clientRepositoryJs, 
            bool isAdditionalFieldSet,
            bool? isHideParentFieldSet,
            out string controlMemberJs)
        {
            var hideParentGroupFieldSet = isHideParentFieldSet.HasValue && isHideParentFieldSet.Value;
            switch (filterData.Type)
            {
                case FilterHtmlGenerator.FilterType.Group:
                    {
                        //todo: не протестированно -> не найден прецедент.
                        controlMemberJs = null;
                        return new FilterControlFieldSet().GetFilledFieldSet(
                            SortFilters(filterData.Children.Where(f => f.Visible)), clientRepositoryJs, false, hideParentGroupFieldSet || IsHideParentFieldSet(filterData), filterData.Header);
                    }
                case FilterHtmlGenerator.FilterType.Reference:
                    {
                        return new FilterControlComboBox().GetControl(filterData, isAdditionalFieldSet, hideParentGroupFieldSet, out controlMemberJs);
                    }
                case FilterHtmlGenerator.FilterType.Numeric:
                    {
                        if (filterData.IsDateTime)
                            return new FilterControlDateField().GetControl(filterData, isAdditionalFieldSet, hideParentGroupFieldSet, out controlMemberJs);

                        return new FilterControlNumberField().GetControl(filterData, isAdditionalFieldSet, hideParentGroupFieldSet, out controlMemberJs);
                    }
                case FilterHtmlGenerator.FilterType.Boolean:
                    {
                        return new FilterControlBoolean().GetControl(filterData, isAdditionalFieldSet, hideParentGroupFieldSet, out controlMemberJs);
                    }
                case FilterHtmlGenerator.FilterType.Text:
                case FilterHtmlGenerator.FilterType.FullTextSearch:
                    {
                        return new FilterControlTextField().GetControl(filterData, isAdditionalFieldSet, hideParentGroupFieldSet, out controlMemberJs);
                    }
            }

            controlMemberJs = String.Empty;
            return null;
        }

        private bool IsHideParentFieldSet(FilterHtmlGenerator.Filter filterData)
        {
            var childs = filterData.Children.Where(f => f.Visible);
            var enumerable = childs as IList<FilterHtmlGenerator.Filter> ?? childs.ToList();
            
            if (enumerable.Any() && enumerable.All(s => s.Type == FilterHtmlGenerator.FilterType.Group))
            {
                return IsAdditionalFieldSet(enumerable);
            }

            return false;
        }

        private static IEnumerable<FilterHtmlGenerator.Filter> SortFilters(
            IEnumerable<FilterHtmlGenerator.Filter> filters)
        {
            /*var sortFiltersList = new List<FilterHtmlGenerator.Filter>();
            var listFilters = filters as IList<FilterHtmlGenerator.Filter> ?? filters.ToList();
            sortFiltersList.AddRange(listFilters.Where(f => f.Type == FilterHtmlGenerator.FilterType.Reference && f.Lookup));
            sortFiltersList.AddRange(listFilters.Where(f => f.Type == FilterHtmlGenerator.FilterType.Reference && !f.Lookup));
            sortFiltersList.AddRange(listFilters.Where(f => f.Type == FilterHtmlGenerator.FilterType.Text));
            sortFiltersList.AddRange(listFilters.Where(f => f.Type == FilterHtmlGenerator.FilterType.Numeric));
            sortFiltersList.AddRange(listFilters.Where(f => f.Type == FilterHtmlGenerator.FilterType.Boolean));
            sortFiltersList.AddRange(listFilters.Where(f => f.Type == FilterHtmlGenerator.FilterType.Group));
            return sortFiltersList;*/
            return filters;
        }

        private static FieldSet.Config StandartConfigFiledSet(string headerGroup, bool hidden, bool collapsed)
        {
            return new FieldSet.Config
                {
                    Margin = 5,
                    PaddingSpec = "0 0 8 0",
                    Collapsible = true,
                    //Collapsed = collapsed,
                    Hidden = hidden,
                    Title = headerGroup,
                    AutoRender = true
                };
        }
    }
}
