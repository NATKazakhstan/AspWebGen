/*
 * Created by: Alexey V. Vorochsak
 * Created: 2013.07.06
 * Copyright © JSC NAT Kazakhstan 2013
 */

namespace Nat.Web.Controls.ExtNet.Validators
{
    using System.Collections.Generic;
    using System.Text;
    using Ext.Net;

    /// <summary>
    /// Класс предназначен для установки клиентской валидиции
    /// диапазона значений Ext.Net контрола(-ов).
    /// </summary>
    public class RangeValidatorProperties : GenerationClasses.RangeValidatorProperties
    {
        /// <summary>
        /// Метод устанавливает клиентскую валидацию по диапазону
        /// значения(-й) Ext.Net контрола(-ов).
        /// </summary>
        /// <param name="boxReady">
        /// Js обработчик создания контрола.
        /// </param>
        /// <param name="listBoxReadyHandlersValidationGroup">
        /// Js обработчики создания контров валидируемой группы.
        /// </param>
        public void CreateValidator(
            ComponentListener boxReady = null,
            List<ComponentListener> listBoxReadyHandlersValidationGroup = null)
        {
            if (string.IsNullOrEmpty(MaximumValue) && string.IsNullOrEmpty(MinimumValue)) return;

            if (boxReady != null) SetClientRangeValidator(boxReady);

            if (listBoxReadyHandlersValidationGroup != null)
                listBoxReadyHandlersValidationGroup.ForEach(SetClientRangeValidator);
        }

        private void SetClientRangeValidator(ComponentListener boxReady)
        {
            boxReady.Handler = new StringBuilder().Append(boxReady.Handler)
                                                  .Append(ValidatorsJs.SetEnumValidationDataType)
                                                  .Append(ValidatorsJs.JsRangeValidationHandler)
                                                  .Append(ValidatorsJs.JsRangeValidationFunction)
                                                  .Append(ValidatorsJs.JsConvertFunction)
                                                  .AppendFormat(ValidatorsJs.SetRangeValidationType)
                                                  .AppendFormat(ValidatorsJs.SetMinimumValue, MinimumValue)
                                                  .AppendFormat(ValidatorsJs.SetMaximumValue, MaximumValue)
                                                  .AppendFormat(ValidatorsJs.SetDataType, Type)
                                                  .ToString();
        }
    }
}
