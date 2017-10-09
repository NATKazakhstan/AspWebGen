/*
 * Created by: Alexey V. Vorochsak
 * Created: 2013.06.11
 * Copyright © JSC NAT Kazakhstan 2013
 */

namespace Nat.Web.Controls.ExtNet.Generation.Filter
{
    using System;
    using System.Web.UI;
    using System.Web.UI.WebControls;
    using Ext.Net;
    using GenerationClasses;

    using Nat.Web.Controls.ExtNet.Properties;

    using Validators;

    class FilterControlDateField : FilterControlBase
    {
        public FieldContainer GetControl(
            FilterHtmlGenerator.Filter filterData, 
            bool isAdditionalFieldSet,
            bool isHideParentFieldSet,
            out string controlMemberJs)
        {
            const bool isRangeControl = true;
            var dateFieldBegin = GetDateFiledItem(filterData, !isRangeControl);
            var dateFieldEnd = GetDateFiledItem(filterData, isRangeControl);
            var fieldContainer = GetFieldContainerFilter(filterData, isAdditionalFieldSet, isHideParentFieldSet, isRangeControl);
            SetListeners(filterData, dateFieldBegin, !isRangeControl);
            SetListeners(filterData, dateFieldEnd, isRangeControl);
            new ExtNetRangeControlsValidator().CompareValidator(dateFieldBegin, dateFieldEnd);
            fieldContainer.Items.Add(dateFieldBegin);
            fieldContainer.Items.Add(dateFieldEnd);
            controlMemberJs = ClientRepositoryJsMember;
            return fieldContainer;
        }


        #region Методы реализации

        private DateField GetDateFiledItem(FilterHtmlGenerator.Filter filterData, bool isRangeControl)
        {
            var config = StandartConfigDateField(isRangeControl);
            var maxDate = (DateTime?)filterData.MaxValue;
            if (maxDate != null)
                config.MaxDate = maxDate.Value;

            var dateField = new DateField(config)
                {
                    ID = RegistrationControlToRepository(isRangeControl
                                                 ? UniquePrefixes.EndDateField
                                                 : UniquePrefixes.BeginDateField,
                                             filterData,
                                             isRangeControl),
                    ClientIDMode = ClientIDMode.Static,
                };

            dateField.CustomConfig.Add(new ConfigItem("rememberMaxValue", maxDate?.ToShortDateString() ?? "", ParameterMode.Value));
            dateField.CustomConfig.Add(new ConfigItem("maxText", Resources.SMaxDateText, ParameterMode.Value));
            dateField.CustomConfig.Add(new ConfigItem("minText", Resources.SMinDateText, ParameterMode.Value));

            return dateField;
        }

        private void SetListeners(FilterHtmlGenerator.Filter filterData, DateField dateField, bool isRangeControl)
        {
            if (!isRangeControl && !string.IsNullOrEmpty(filterData.OnChangedValue))
                dateField.Listeners.Change.Handler += filterData.OnChangedValue;
        }

        #endregion

        private DateField.Config StandartConfigDateField(bool isRangeControl)
        {
            return new DateField.Config
                {
                    Width = Unit.Pixel(195),
                    MarginSpec = !isRangeControl ? "0 0 0 0" : "0 0 0 5",
                    Format = "dd.MM.yyyy",
                    EmptyText = Controls_ExtNet_Resources.SelectDate,
                    Vtype = "daterange",
                    EnableKeyEvents = true,
                };
        }
    }
}