/*
 * Created by: Alexey V. Vorochsak
 * Created: 2013.07.06
 * Copyright © JSC NAT Kazakhstan 2013
 */

namespace Nat.Web.Controls.ExtNet.Validators
{
    using System.Text;
    using Ext.Net;
    using System.Collections.Generic;

    /// <summary>
    /// Класс предназначен для установки клиентской валидиции
    /// регулярным выражением значения Ext.Net контрола(-ов).
    /// </summary>
    public class RegexValidatorProperties : GenerationClasses.RegexValidatorProperties
    {
        /// <summary>
        /// Метод устанавливает клиентскую валидацию 
        /// по регулярному выражению значения(-й) Ext.Net контрола(-ов).
        /// </summary>
        /// <param name="listeners">
        /// Js обработчик создания контрола.
        /// </param>
        /// <param name="listBoxReadyHandlersValidationGroup">
        /// Js обработчики создания контров валидируемой группы.
        /// </param>
        public void CreateValidator(
            FieldListeners listeners = null,
            List<FieldListeners> listBoxReadyHandlersValidationGroup = null)
        {
            // Example verify e-mail regular expression 
            // RegularExpression = @"^[A-Za-z0-9](([_\.\-]?[a-zA-Z0-9]+)*)@([A-Za-z0-9]+)(([\.\-]?[a-zA-Z0-9]+)*)\.([A-Za-z]{2,})$";

            if (string.IsNullOrEmpty(RegularExpression)) return;

            if (listeners != null) SetClientRegexValidator(listeners);

            if (listBoxReadyHandlersValidationGroup != null && listBoxReadyHandlersValidationGroup.Count > 0)
                listBoxReadyHandlersValidationGroup.ForEach(SetClientRegexValidator);
        }

        private void SetClientRegexValidator(FieldListeners listeners)
        {
            listeners.Change.Handler = new StringBuilder().Append(listeners.Change.Handler)
                .AppendFormat(ValidatorsJs.SetRegularExpression, RegularExpression, ErrorMessage)
                .Append(ValidatorsJs.JsRegexValidationHandler).ToString();
        }
    }
}
