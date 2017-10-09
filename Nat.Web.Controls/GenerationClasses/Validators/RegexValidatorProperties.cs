/*
 * Created by: Sergey V. Shpakovskiy
 * Created: 5 ноября 2008 г.
 * Copyright © JSC New Age Technologies 2008
 */

using System.Text;
using System.Text.RegularExpressions;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Nat.Web.Controls.GenerationClasses
{
    public class RegexValidatorProperties : ValidatorProperties
    {

        public string RegularExpression { get; set; }

        public override BaseValidator CreateValidator(string controlToValidate, string validationGroup)
        {
            return new RegularExpressionValidator
                       {
                           ControlToValidate = controlToValidate,
                           Display = ValidatorDisplay.Dynamic,
                           EnableViewState = false,
                           ValidationGroup = validationGroup,
                           ErrorMessage = ErrorMessageInSummary,
                           Text = ErrorMessage,
                           ValidationExpression = RegularExpression,
                           EnableClientScript = EnableClientScript,
                           //SetFocusOnError = true,
                       };
        }

        /// <summary>
        /// Создание валидатора по клиентскому ID
        /// </summary>
        /// <param name="page">Страница</param>
        /// <param name="sb">StringBuilder в который необходимо добавить валидатор</param>
        /// <param name="clientID">Клиентский ID валидатора</param>
        /// <param name="controlToValidate">Клиентский ID валидируемого компонента</param>
        /// <param name="controlValue">Текущее значение контрола</param>
        public void CreateClientValidator(Page page, StringBuilder sb, string clientID, string controlToValidate, string controlValue)
        {
            CreateClientValidator(page, sb, clientID, controlToValidate, controlValue, ValidatorDisplay.Dynamic, "");
        }

        /// <summary>
        /// Создание валидатора по клиентскому ID
        /// </summary>
        /// <param name="page">Страница</param>
        /// <param name="sb">StringBuilder в который необходимо добавить валидатор</param>
        /// <param name="clientID">Клиентский ID валидатора</param>
        /// <param name="controlToValidate">Клиентский ID валидируемого компонента</param>
        /// <param name="controlValue">Текущее значение контрола</param>
        /// <param name="display">Настройка срабатывания валидации</param>
        /// <param name="validationGroup">Группа валидации</param>
        public void CreateClientValidator(Page page, StringBuilder sb, string clientID, string controlToValidate, string controlValue, ValidatorDisplay display, string validationGroup)
        {
            base.CreateClientValidator(page, sb, clientID, controlToValidate, display, validationGroup, controlValue);
        }

        protected override void RegisterClientValidator(Page page, clientValidatorInfo validatorInfo, int number)
        {
            base.RegisterClientValidator(page, validatorInfo, number);
            AddExpandoAttribute(page, null, validatorInfo.clientID, "evaluationfunction", "RegularExpressionValidatorEvaluateIsValid", false);
            if (!string.IsNullOrEmpty(RegularExpression))
                AddExpandoAttribute(page, null, validatorInfo.clientID, "validationexpression", RegularExpression);
        }


        public override void Validate()
        {
            IsValid = InternalValidate();
        }

        private bool InternalValidate()
        {
            Regex regex = null;
            if (!string.IsNullOrEmpty(RegularExpression))
                regex = new Regex(RegularExpression);
            foreach (var value in ControlValues)
            {
                if (regex == null || value == null)
                    continue;
                var input = value.ToString();
                var match = regex.Match(input);
                if (!match.Success || match.Index != 0 || match.Length != input.Length)
                    return false;
            }
            return true;
        }

        public override bool ValidateValue(string value)
        {
            if (string.IsNullOrEmpty(RegularExpression) || string.IsNullOrEmpty(value))
                return true;
            var regex = new Regex(RegularExpression);
            var match = regex.Match(value);
            if (!match.Success || match.Index != 0 || match.Length != value.Length)
                return false;
            return true;
        }
    }
}