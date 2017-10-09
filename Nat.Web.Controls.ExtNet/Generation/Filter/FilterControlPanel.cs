/*
 * Created by: Alexey V. Vorochsak
 * Created: 2013.06.06
 * Copyright © JSC NAT Kazakhstan 2013
 */

using System.Linq;
using System.Web.UI;

[assembly: WebResource("Nat.Web.Controls.ExtNet.Generation.Filter.FilterJs.js", "text/javascript")]

namespace Nat.Web.Controls.ExtNet.Generation.Filter
{
    using System.Collections.Generic;
    using Ext.Net;
    using Panel = Ext.Net.Panel;
    using GenerationClasses;

    class FilterControlPanel : FilterControlBase
    {
        private static bool FilterPanelCollapsed { get; set; }

        public static Panel GetFilterPanel(
            IList<FilterHtmlGenerator.Filter> filters, 
            string defaultValuesJson, 
            string applyFunctionName,
            bool filterPanelCollapsed)
        {
            FilterPanelCollapsed = filterPanelCollapsed;
            IList<string> clientRepository = new List<string>();
            var panelFilters = GetPanelFilters();
            clientRepository = AddFieldSet(filters, clientRepository, panelFilters);
            FilterControlActions.SetActionsControls(applyFunctionName, panelFilters, HasAdditionalFilters(filters));
            SetListeners(defaultValuesJson, clientRepository, panelFilters);
            panelFilters.PreRender += (sender, args) =>
                {
                    panelFilters.Page.ClientScript.RegisterClientScriptResource(
                        typeof(FilterControlPanel),
                        "Nat.Web.Controls.ExtNet.Generation.Filter.FilterJs.js");
                };
            return panelFilters;
        }

        private static bool HasAdditionalFilters(IList<FilterHtmlGenerator.Filter> filters)
        {
            if (filters.Any(f => !f.MainGroup))
            {
                return true;
            }

            foreach (var filter in filters)
            {
                var childs = filter.Children.Where(f => f.Visible).ToList();
                if (childs.Any())
                {
                    if (HasAdditionalFilters(childs))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        #region Методы реализации

        private static IList<string> AddFieldSet(
            IList<FilterHtmlGenerator.Filter> filters,
            IList<string> clientRepository,
            AbstractContainer panelFilters)
        {
            var mainFiltersFieldSet = new FilterControlFieldSet()
                .GetFieldSetFilters(filters, clientRepository);
            
            if (mainFiltersFieldSet != null)
            {
                panelFilters.Items.Add(mainFiltersFieldSet);
                mainFiltersFieldSet.Listeners.BoxReady.Handler = FilterJs.JsCallRegistrateFilterOperationControl;
            }

            return clientRepository;
        }

        private static void SetListeners(
            string defaultValuesJson,
            IEnumerable<string> clientRepository,
            Panel panelFilters)
        {
            var jsClientRepository = string.Join(FilterJs.MemberJsSeparator, clientRepository);
            panelFilters.Listeners.Render.Handler = FilterJs.GetFilterRepository(jsClientRepository,
                                                                                   panelFilters.ID,
                                                                                   defaultValuesJson);
            /*
             *             panelFilters.Listeners.Render.Handler = FilterJs.GetFilterRepository(jsClientRepository,
                                                                                   panelFilters.ID,
                                                                                   defaultValuesJson);
             */
        }

        private static Panel GetPanelFilters()
        {
            var panel = new Panel(StandartConfigPanel()) {ID = UniquePrefixes.FiltersPanel.ToString()};
            return panel;
        }

        #endregion

        private static Panel.Config StandartConfigPanel()
        {
            return new Panel.Config
            {
                Collapsible = true,
                Collapsed = FilterPanelCollapsed,
                Title = Controls_ExtNet_Resources.Filter,
                MaxHeight = 400,
                AutoScroll = true,
                TitleCollapse = true,
                ButtonAlign = Alignment.Right
            };
        }
    }
}