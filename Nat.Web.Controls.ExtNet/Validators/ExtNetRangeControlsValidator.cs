/*
 * Created by: Alexey V. Vorochsak
 * Created: 2013.06.28
 * Copyright © JSC NAT Kazakhstan 2013
 */

namespace Nat.Web.Controls.ExtNet.Validators
{
    using Ext.Net;
    using System.Text;

    /// <summary>
    /// Класс предназначен для валидиции двух Ext.Net контролов,
    /// с помощью которых пользователь указывает диапазон значений.
    /// </summary>
    public class ExtNetRangeControlsValidator
    {
        /// <summary>
        /// Метод устанавливает взаимную валидацию значений Ext.Net контролов,
        /// с помощью которых пользователь указывает диапазон значений.
        /// </summary>
        /// <param name="beginControl">
        /// Контрол ввода начального значения. 
        /// </param>
        /// <param name="endControl">
        /// Контрол ввода конечного значения.
        /// </param>
        public void CompareValidator(AbstractComponent beginControl, AbstractComponent endControl)
        {
            if (IsDateFields(beginControl, endControl))
            {
                SetCompareValidation(beginControl, endControl,
                    ValidatorsJs.RangeValidationTypeDateFields,
                    ValidatorsJs.JsRangeDateFieldsValidationHandler,
                    ValidatorsJs.JsSetEndDateField,
                    ValidatorsJs.JsSetStartDateField);
            }

            if (IsNumberFields(beginControl, endControl))
            {
                SetCompareValidation(beginControl, endControl,
                    ValidatorsJs.RangeValidationTypeNumberFields,
                    ValidatorsJs.JsRangeNumberFieldsValidationHandler,
                    ValidatorsJs.JsSetEndNumberField,
                    ValidatorsJs.JsSetStartNumberField);
            }
        }

        #region Общие локальные методы

        private static void SetCompareValidation(
            AbstractComponent beginDateFieldControl,
            AbstractComponent endDateFieldControl,
            string validationType,
            string jsFunctionValidationHandler,
            string jsCallStartControlSetRangeControl,
            string jsCallEndControlSetRangeControl)
        {
            SetRangeValidationFunction(beginDateFieldControl, jsFunctionValidationHandler);

            SetRangeValidationControl(beginDateFieldControl, validationType,
                jsCallStartControlSetRangeControl, endDateFieldControl.ID);

            SetRangeValidationControl(endDateFieldControl, validationType,
                jsCallEndControlSetRangeControl, beginDateFieldControl.ID);
        }

        private static bool IsDateFields(
           AbstractComponent startControl,
           AbstractComponent endControl)
        {
            return startControl as DateField != null
                   && endControl as DateField != null;
        }

        private static bool IsNumberFields(
            AbstractComponent startControl,
            AbstractComponent endControl)
        {
            return startControl as NumberField != null
                   && endControl as NumberField != null;
        }

        /* todo: рефакторить оба метода, желательно в один метод,
         * на текущий момент падает ошибка в браузере 
         * "Uncaught TypeError: Cannot call method 'on' of undefined"*/

        private static void SetRangeValidationFunction<T>(
            T control,
            string jsValidationFunction)
            where T : AbstractComponent
        {
            var builder = new StringBuilder();
            var dateField = control as DateField;
            var numberField = control as NumberField;
            if (dateField != null)
            {
                dateField.Listeners.BeforeRender.Handler =
                builder.Append(dateField.Listeners.BeforeRender.Handler)
                       .Append(jsValidationFunction).ToString();
            }

            if (numberField != null)
            {
                numberField.Listeners.BeforeRender.Handler =
                builder.Append(numberField.Listeners.BeforeRender.Handler)
                       .Append(jsValidationFunction).ToString();
            }
            //
            //            var fn = new JFunction(jsValidationFunction);
            //            control.AddListener("ready", fn, "window");
        }

        private static void SetRangeValidationControl<T>(
            T controlField,
            string validationType,
            string jsFunction,
            string rangeControlId)
            where T : AbstractComponent
        {
            var control = controlField as TriggerFieldBase;
            if (control == null) return;
            var builder = new StringBuilder();
            control.Vtype = validationType;
            var jsCallFunction = string.Format(jsFunction, rangeControlId);
            var dateField = control as DateField;
            var numberField = control as NumberField;

            if (dateField != null)
            {
                dateField.Listeners.BoxReady.Handler =
                    builder.Append(dateField.Listeners.BoxReady.Handler)
                           .Append(jsCallFunction).ToString();
            }

            if (numberField != null)
            {
                numberField.Listeners.BoxReady.Handler =
                    builder.Append(numberField.Listeners.BoxReady.Handler)
                           .Append(jsCallFunction).ToString();
            }

            //            var fn = new JFunction(string.Format(jsFunction, rangeControlId));
            //            control.AddListener("ready", fn, "window");
        }

        #endregion

    }
}