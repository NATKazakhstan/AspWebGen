/*
 * Created by: Alexey V. Vorochsak
 * Created: 2013.06.11
 * Copyright © JSC NAT Kazakhstan 2013
 */

namespace Nat.Web.Controls.ExtNet.Generation.Filter
{
    using System.Linq;

    using Ext.Net;
    using GenerationClasses;

    class FilterControlBoolean : FilterControlBase
    {
        public FieldContainer GetControl(
            FilterHtmlGenerator.Filter filterData,
            bool isAdditionalFieldSet,
            bool isHideParentFieldSet,
            out string controlMemberJs)
        {
            var fieldContainer = GetFieldContainerFilter(filterData, isAdditionalFieldSet,  isHideParentFieldSet);
            controlMemberJs = ClientRepositoryJsMember;
            if (!string.IsNullOrEmpty(filterData.OnChangedValue))
            {
                var box = fieldContainer.Items.OfType<SelectBox>().First();
                box.Listeners.Change.Handler += filterData.OnChangedValue;
            }

            return fieldContainer;
        }

        #region Мутоды реализации группы radioButton'ов

        //        var radioGroupBoolean = GetRadioGroupBoolean(filterData, radioGroupConfig, radioConfig);
        //        fieldContainer.Items.Add(radioGroupBoolean);


        //        private RadioGroup GetRadioGroupBoolean(FilterHtmlGenerator.Filter filterData, RadioGroup.Config radioGroupConfig, Radio.Config radioConfig)
        //        {
        //            var radioGroup = GetRadioGroup(radioGroupConfig ?? StandartRadioGroupConfig());
        //            radioGroup.ID = RegistrationControl(UniquePrefixes.BooleanGroup, filterData);
        //            var radioFieldYes = GetRadioBooleanField(filterData, radioConfig, true);
        //            var radioFieldNo = GetRadioBooleanField(filterData, radioConfig, false);
        //            radioGroup.Items.Add(radioFieldYes);
        //            radioGroup.Items.Add(radioFieldNo);
        //            return radioGroup;
        //        }

        //        private Radio GetRadioBooleanField(FilterHtmlGenerator.Filter filterData, Radio.Config radioConfig, bool isTrueField)
        //        {
        //            var radioField = GetRadio(radioConfig ?? StandartRadioConfig());
        //            radioField.ID = RegistrationControl(isTrueField ? UniquePrefixes.BooleanMemberYes : UniquePrefixes.BooleanMemberNo, filterData);
        //            return radioField;
        //        }

        //        private Radio.Config StandartRadioConfig()
        //        {
        //            return new Radio.Config();
        //        }
        //
        //        private RadioGroup.Config StandartRadioGroupConfig()
        //        {
        //            return new RadioGroup.Config();
        //        }

        #endregion
    }
}