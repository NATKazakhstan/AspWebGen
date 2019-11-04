/*
 * Created by: Alexey V. Vorochsak
 * Created: 2013.06.06
 * Copyright © JSC NAT Kazakhstan 2013
 */

using System.Linq;
using System.Web.UI.WebControls;

namespace Nat.Web.Controls.ExtNet.Generation.Filter
{
    using System.Collections.Generic;
    using System.Web;
    using System.Web.UI;
    using Ext.Net;
    using GenerationClasses;

    /// <summary>
    /// Класс генерирует Ext.Net компоненты панели фильтрации.
    /// </summary>
    public static class FilterControlGenerator
    {
        /// <summary>
        /// Метод генерирует Ext.Net компоненты панели фильтрации.
        /// </summary>
        /// <param name="filterPlaceHolder">
        /// Контейнер в который помещается панель фильтрации.
        /// </param>
        /// <param name="filters">
        /// Данные фильтров.
        /// </param>
        /// <param name="defaultValuesJson">
        /// Значения по умолчанию.
        /// </param>
        /// <param name="applyFunctionName">
        /// Наименование JS функции, для применения зачений фильтра к фильтруемому множеству записей.
        /// </param>
        /// <param name="filterPanelCollapsed">
        /// Определяет создавать FilterPanel свернутой или нет.
        /// </param>
        public static void PackingFilter(
            Control filterPlaceHolder,
            IList<FilterHtmlGenerator.Filter> filters,
            string defaultValuesJson,
            string applyFunctionName,
            bool filterPanelCollapsed = false)
        {
            var isReadArgument = (HttpContext.Current.Request.Form["__EVENTARGUMENT"] ?? "").EndsWith("|postback|read");
            if (isReadArgument)
                return;
            if (!filters.Any(f => f.MainGroup)) return;

            var panelFilters = FilterControlPanel.GetFilterPanel(filters, defaultValuesJson, applyFunctionName, filterPanelCollapsed);
            filterPlaceHolder.Controls.Add(panelFilters);
        }
    }
}