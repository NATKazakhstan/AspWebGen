/*
 * Created by: Alexey V. Vorochsak
 * Created: 2013.06.11
 * Copyright © JSC NAT Kazakhstan 2013
 */

namespace Nat.Web.Controls.ExtNet.Generation.Filter
{
    using System.Web.UI;
    using Ext.Net;
    using GenerationClasses;

    class FilterControlTextField : FilterControlBase
    {
        internal FieldContainer GetControl(
            FilterHtmlGenerator.Filter filterData, 
            bool isAdditionalFieldSet, 
            bool isHideParentFieldSet,
            out string controlMemberJs)
        {
            var textField = GetTextFieldItem(filterData);
            var fieldContainer = GetFieldContainerFilter(filterData, isAdditionalFieldSet, isHideParentFieldSet);
            fieldContainer.Items.Add(textField);

            if (!string.IsNullOrEmpty(filterData.OnChangedValue))
                textField.Listeners.Change.Handler += filterData.OnChangedValue;

            controlMemberJs = ClientRepositoryJsMember;
            return fieldContainer;
        }

        #region Методы реализации

        private TextField GetTextFieldItem(FilterHtmlGenerator.Filter filterData)
        {
            var textField = new TextField(StandartConfigTextFieldItem())
                { 
                    ID = RegistrationControlToRepository(UniquePrefixes.TextFieldFilterValues, filterData),
                    ClientIDMode = ClientIDMode.Static,
                };
            SetListeners(textField);
            return textField;
        }

        private void SetListeners(TextField textField)
        {
        }

        #endregion

        private TextField.Config StandartConfigTextFieldItem()
        {
            return new TextField.Config
                {
                    Flex = 1,
                    EmptyText = Controls_ExtNet_Resources.EnterValue,
                    AnchorHorizontal = "100%"
                };
        }
    }
}