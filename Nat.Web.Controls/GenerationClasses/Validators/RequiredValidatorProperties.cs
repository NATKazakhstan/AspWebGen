/*
 * Created by: Sergey V. Shpakovskiy
 * Created: 13 ноября 2008 г.
 * Copyright © JSC New Age Technologies 2008
 */


using System.Text;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Nat.Web.Controls.GenerationClasses
{
    public class RequiredValidatorProperties : ValidatorProperties
    {
        public RequiredValidatorProperties()
        {
            ErrorMessage = "*";
            IsValid = true;
        }
     
        public string InitialValue { get; set; }

        /// <summary>
        /// Создание серверного валидатора, для расположения на форме
        /// </summary>
        /// <param name="controlToValidate">ID компоненты для валидации</param>
        /// <param name="validationGroup">группа валидации</param>
        /// <returns>Валидатор</returns>
        public override BaseValidator CreateValidator(string controlToValidate, string validationGroup)
        {
            return new RequiredFieldValidator
            {
                ControlToValidate = controlToValidate,
                Display = ValidatorDisplay.Dynamic,
                EnableViewState = false,
                ValidationGroup = validationGroup,
                ErrorMessage = ErrorMessageInSummary,
                Text = ErrorMessage,
                EnableClientScript = EnableClientScript,
                InitialValue = InitialValue,
                IsValid = true,
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
        public void CreateClientValidator(Page page, StringBuilder sb, string clientID, string controlToValidate, object controlValue)
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
        public void CreateClientValidator(Page page, StringBuilder sb, string clientID, string controlToValidate, object controlValue, ValidatorDisplay display, string validationGroup)
        {
            CreateClientValidator(page, sb, clientID, controlToValidate, display, validationGroup, controlValue);
        }

        protected override void RegisterClientValidator(Page page, clientValidatorInfo validatorInfo, int number)
        {
            base.RegisterClientValidator(page, validatorInfo, number);
            AddExpandoAttribute(page, null, validatorInfo.clientID, "evaluationfunction", "RequiredFieldValidatorEvaluateIsValid", false);
            AddExpandoAttribute(page, null, validatorInfo.clientID, "initialvalue", null);
        }

        public override void Validate()
        {
            IsValid = InternalValidate();
        }

        private bool InternalValidate()
        {
            foreach (var value in ControlValues)
            {
                if (value == null || value.Equals(""))
                    return false;
            }
            return true;            
        }

        public override bool ValidateValue(string value)
        {
            if (string.IsNullOrEmpty(value))
                return false;
            return true;
        }
    }
}