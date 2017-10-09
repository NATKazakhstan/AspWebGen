/*
 * Created by: Sergey V. Shpakovskiy
 * Created: 8 ���� 2009 �.
 * Copyright � JSC New Age Technologies 2009
 */

using System.Web.UI.WebControls;

namespace Nat.Web.Controls.GenerationClasses
{
    public class CustomValidatorProperties : ValidatorProperties
    {
        /// <summary>
        /// ����� ��� ���������� ���������.
        /// </summary>
        public ServerValidateEventHandler ServerValidate { get; set; }

        /// <summary>
        /// ���������� ����� �� �������������� ������ �����.
        /// </summary>
        public bool ValidateEmptyText { get; set; }

        /// <summary>
        /// ������������ ������� ��� ��������� �� �������.
        /// </summary>
        public string ClientValidationFunction { get; set; }

        /// <summary>
        /// ���������� ����� �� ������������ �������.
        /// �.�. ����� �� �������� ��������� �� ������ � ��������.
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