/*
 * Created by: Alexey V. Vorochsak
 * Created: 2013.06.28
 * Copyright © JSC NAT Kazakhstan 2013
 */

using System.Collections.Generic;

namespace Nat.Web.Controls.ExtNet.Validators
{
    using Ext.Net;
    using System.Text;

    /// <summary>
    /// Класс предназначен для установки клиентской валидиции
    /// сравнения Ext.Net контрола(-ов).
    /// </summary>
    public class CompareValidatorProperties : GenerationClasses.CompareValidatorProperties
    {
        /// <summary>
        /// Контрол ввода конечного значения.
        /// </summary>
        public new AbstractComponent ControlToCompare { get; set; }

        /// <summary>
        /// Метод устанавливает клиентскую валидацию
        /// сравнения значения(-й) Ext.Net контрола(-ов).
        /// </summary>
        /// <param name="boxReady">
        /// Js обработчик создания контрола.
        /// </param>
        /// <param name="listBoxReadyHandlersValidationGroup">
        /// Валидируемая группа.
        /// </param>
        public void CreateValidator(ComponentListener boxReady = null, List<ComponentListener> listBoxReadyHandlersValidationGroup = null)
        {
            if (string.IsNullOrEmpty(ValueToCompare) && ControlToCompare == null) return;

            if (boxReady != null) { SetClientCompareValidator(boxReady); }
            
            if (listBoxReadyHandlersValidationGroup != null && listBoxReadyHandlersValidationGroup.Count > 0)
            { listBoxReadyHandlersValidationGroup.ForEach(SetClientCompareValidator); }
        }

        private void SetClientCompareValidator(ComponentListener boxReady)
        {
            boxReady.Handler = new StringBuilder().Append(boxReady.Handler)
                                                  .Append(ValidatorsJs.SetEnumValidationCompareOperator)
                                                  .Append(ValidatorsJs.SetEnumValidationDataType)
                                                  .Append(ValidatorsJs.JsCompareValidationHandler)
                                                  .Append(ValidatorsJs.JsConvertFunction)
                                                  .AppendFormat(ValidatorsJs.SetClientTypeValidation)
                                                  .AppendFormat(ValidatorsJs.SetControlToCompare, ControlToCompare.ID)
                                                  .AppendFormat(ValidatorsJs.SetValueToCompare, ValueToCompare)
                                                  .AppendFormat(ValidatorsJs.SetCompareOperator, Operator)
                                                  .AppendFormat(ValidatorsJs.SetDataType, Type)
                                                  .ToString();
        }
    }
}