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
    using Validators;

    class FilterControlNumberField : FilterControlBase
    {
        public FieldContainer GetControl(
            FilterHtmlGenerator.Filter filterData,
            bool isAdditionalFieldSet,
            bool isHideParentFieldSet,
            out string controlMemberJs)
        {
            const bool isRangeControl = true;
            var numberFiledBegin = GetNumberFieldItem(filterData, !isRangeControl);
            var numberFiledEnd = GetNumberFieldItem(filterData, isRangeControl);
            var fieldContainer = GetFieldContainerFilter(filterData, isAdditionalFieldSet,  isHideParentFieldSet, isRangeControl);
            SetListeners(filterData, numberFiledBegin, !isRangeControl);
            SetListeners(filterData, numberFiledEnd, isRangeControl);
            new ExtNetRangeControlsValidator().CompareValidator(numberFiledBegin, numberFiledEnd);
            fieldContainer.Items.Add(numberFiledBegin);
            fieldContainer.Items.Add(numberFiledEnd);
            controlMemberJs = ClientRepositoryJsMember;
            return fieldContainer;
        }

        #region Методы реализации

        private NumberField GetNumberFieldItem(FilterHtmlGenerator.Filter filterData, bool isRangeControl)
        {
            var config = StandartConfigNumberField(isRangeControl);
            var maxValue = filterData.MaxValue == null ? (double?)null : Convert.ToDouble(filterData.MaxValue);
            if (maxValue != null)
                config.MaxValue = maxValue.Value;

            var numberField = new NumberField(config)
                {
                    ID = RegistrationControlToRepository(
                        isRangeControl
                            ? UniquePrefixes.EndNumberField
                            : UniquePrefixes.BeginNumberField,
                        filterData,
                        isRangeControl),
                    ClientIDMode = ClientIDMode.Static,
                    DecimalPrecision = filterData.DecimalPrecision
                };
            numberField.CustomConfig.Add(new ConfigItem("rememberMaxValue", maxValue?.ToString() ?? "", ParameterMode.Value));
            return numberField;
        }

        private void SetListeners(FilterHtmlGenerator.Filter filterData, NumberField numberField, bool isRangeControl)
        {
            if (!isRangeControl && !string.IsNullOrEmpty(filterData.OnChangedValue))
                numberField.Listeners.Change.Handler += filterData.OnChangedValue;
        }

        #endregion

        private NumberField.Config StandartConfigNumberField(bool isRangeControl)
        {
            return new NumberField.Config
                {
                    Width = Unit.Pixel(195),
                    EmptyText = Controls_ExtNet_Resources.SelectValue,
                    MarginSpec = !isRangeControl ? "0 0 0 0" : "0 0 0 5",
                    AllowBlank = !isRangeControl,
                    MinValue = 0
                };
        }
    }
}