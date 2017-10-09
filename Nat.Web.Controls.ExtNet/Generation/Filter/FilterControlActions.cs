/*
 * Created by: Alexey V. Vorochsak
 * Created: 2013.06.12
 * Copyright © JSC NAT Kazakhstan 2013
 */

namespace Nat.Web.Controls.ExtNet.Generation.Filter
{
    using System.Web.UI.WebControls;
    using Ext.Net;

    class FilterControlActions : FilterControlBase
    {
        internal static void SetActionsControls(string applyFunctionName, Ext.Net.Panel panelFilters, bool hasAdditionalFilters)
        {
            var applyButton = new Ext.Net.Button(StandartConfigApplyButton());
            var clearButton = new Ext.Net.Button(StandartConfigClearButton());
            SetListenersApplyButton(applyButton, applyFunctionName);
            SetListenersClearButton(clearButton);
            panelFilters.Buttons.Add(applyButton);
            panelFilters.Buttons.Add(clearButton);

            if (!hasAdditionalFilters) return;
            var additonalFiltersButton = new Ext.Net.Button(StandartConfigAdditonalFiltersButton());
            SetListenersAdditonalFiltersButton(additonalFiltersButton);
            panelFilters.Buttons.Add(additonalFiltersButton);
        }

        private static void SetListenersAdditonalFiltersButton(Ext.Net.Button additonalFiltersButton)
        {
            additonalFiltersButton.Listeners.Click.Handler = FilterJs.JsCallToogleVisibleAdditionalFilters;
        }

        #region Методы реализации

        private static void SetListenersClearButton(Ext.Net.Button clearButton)
        {
            clearButton.Listeners.Click.Handler = FilterJs.JsCallCleanupFilter;
        }

        private static void SetListenersApplyButton(Ext.Net.Button applyButton, string applyFunctionName)
        {
            applyButton.Listeners.Click.Handler = FilterJs.GetApplyFilterHandler(applyFunctionName);
            applyButton.Listeners.BoxReady.Handler = FilterJs.ClickApplyButtonByEnterPress;
        }

        #endregion

        #region Стандартная конфигурация

        private static Ext.Net.Button.Config StandartConfigApplyButton()
        {
            return new Ext.Net.Button.Config
                {
                    Text = Controls_ExtNet_Resources.Apply,
                    MarginSpec = "5 5 5 15",
                    Width = Unit.Pixel(100),
                    Icon = Icon.ApplicationGo
                };
        }

        private static Ext.Net.Button.Config StandartConfigClearButton()
        {
            return new Ext.Net.Button.Config
                {
                    Text = Controls_ExtNet_Resources.Clear,
                    Margin = 5,
                    Width = Unit.Pixel(100),
                    Icon = Icon.ApplicationOsxDelete,
                };
        }
        private static Ext.Net.Button.Config StandartConfigAdditonalFiltersButton()
        {
            return new Ext.Net.Button.Config
                {
                    Margin = 5,
                    MaxWidth = 22,
                    ToolTip = Controls_ExtNet_Resources.ToogleVisibleAdditionalFilters,
                    Icon = Icon.ApplicationViewDetail,
                };
        }

        #endregion

        #region Код реализации фильтрации по пользовательским фильтрам

        //       var userFilters = GetUserFilters(selectBoxUserFiltersConfig);

        //        private SelectBox GetUserFilters(SelectBox.Config config)
        //        {
        //            var selectBox = GetSelectBox(config ?? StandartConfigUserFiltersSelectBox());
        //            // todo: здесь получать Store c данными пользовательских фильтров по текущему пользователю.
        //            var addUserFilterTrigger = GetFieldTriggerAddUserFilter();
        //            var removeUserFilterTrigger = GetFieldTriggerRemoveUserFilter();
        //            selectBox.Triggers.Add(addUserFilterTrigger);
        //            selectBox.Triggers.Add(removeUserFilterTrigger);
        //            return selectBox;
        //        }

        //        private FieldTrigger GetFieldTriggerRemoveUserFilter()
        //        {
        //            return GetFieldTrigger(StandartConfigFieldTriggerRemoveUserFilter());
        //        }
        //
        //        private FieldTrigger GetFieldTriggerAddUserFilter()
        //        {
        //            return GetFieldTrigger(StandartConfigFieldTriggerAddUserFilter());
        //        }

        //        private FieldTrigger.Config StandartConfigFieldTriggerAddUserFilter()
        //        {
        //            return new FieldTrigger.Config
        //            {
        //                Icon = TriggerIcon.SimpleAdd,
        //                Qtip = "Сохранить фильтр"
        //            };
        //        }
        //
        //        private FieldTrigger.Config StandartConfigFieldTriggerRemoveUserFilter()
        //        {
        //            return new FieldTrigger.Config
        //            {
        //                Icon = TriggerIcon.SimpleDelete,
        //                Qtip = "Удалить фильтр"
        //            };
        //        }

        //         private SelectBox.Config StandartConfigUserFiltersSelectBox()
        //        {
        //            return new SelectBox.Config
        //            {
        //                Width = Unit.Pixel(500),
        //                MarginSpec = "0 0 0 50",
        //                Editable = true,
        //                Flex = 1,
        //                Grow = true,
        //                ValueField = "Code",
        //                DisplayField = "Name",
        //                EmptyText = "Выберите значение ...",
        //            };
        //        }

        #endregion
    }
}