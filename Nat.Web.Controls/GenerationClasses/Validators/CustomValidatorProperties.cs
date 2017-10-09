/*
 * Created by: Sergey V. Shpakovskiy
 * Created: 8 июня 2009 г.
 * Copyright © JSC New Age Technologies 2009
 */

using System.Web.UI.WebControls;

namespace Nat.Web.Controls.GenerationClasses
{
    public class CustomValidatorProperties : ValidatorProperties
    {
        /// <summary>
        /// Метод для выполнения валидации.
        /// </summary>
        public ServerValidateEventHandler ServerValidate { get; set; }

        /// <summary>
        /// Показывает будет ли валидироваться пустой текст.
        /// </summary>
        public bool ValidateEmptyText { get; set; }

        /// <summary>
        /// Наименование функции для валидации на клиенте.
        /// </summary>
        public string ClientValidationFunction { get; set; }

        /// <summary>
        /// Показывает нужно ли валидировать контрол.
        /// Т.е. нужно ли выводить сообщение об ошибке у контрола.
        /// </summary>
        public bool ControlValidate { get; set; }

        public override BaseValidator CreateValidator(string controlToValidate, string validationGroup)
        {
            var validator = CreateValidator(validationGroup);
            if (ControlValidate)
            {
                validator.ControlToValidate = controlToValidate;
                validator.Display = ValidatorDisplay.Dynamic;
            }

            return validator;
        }

        public override BaseValidator CreateValidator(string validationGroup)
        {
            var validator = new CustomValidator
                {
                    Display = ValidatorDisplay.None,
                    EnableViewState = false,
                    ValidationGroup = validationGroup,
                    ErrorMessage = ErrorMessageInSummary,
                    Text = ErrorMessage,
                    EnableClientScript = EnableClientScript,
                    ClientValidationFunction = ClientValidationFunction,
                    ValidateEmptyText = ValidateEmptyText,
                    SetFocusOnError = true,
                };
            validator.ServerValidate += ServerValidate;
            return validator;
        }
    }
}