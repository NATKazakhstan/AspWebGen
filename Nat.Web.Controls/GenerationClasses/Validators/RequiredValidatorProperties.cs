/*
 * Created by: Sergey V. Shpakovskiy
 * Created: 13 ������ 2008 �.
 * Copyright � JSC New Age Technologies 2008
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
        /// �������� ���������� ����������, ��� ������������ �� �����
        /// </summary>
        /// <param name="controlToValidate">ID ���������� ��� ���������</param>
        /// <param name="validationGroup">������ ���������</param>
        /// <returns>���������</returns>
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
        /// �������� ���������� �� ����������� ID
        /// </summary>
        /// <param name="page">��������</param>
        /// <param name="sb">StringBuilder � ������� ���������� �������� ���������</param>
        /// <param name="clientID">���������� ID ����������</param>
        /// <param name="controlToValidate">���������� ID ������������� ����������</param>
        /// <param name="controlValue">������� �������� ��������</param>
        public void CreateClientValidator(Page page, StringBuilder sb, string clientID, string controlToValidate, object controlValue)
        {
            CreateClientValidator(page, sb, clientID, controlToValidate, controlValue, ValidatorDisplay.Dynamic, "");
        }

        /// <summary>
        /// �������� ���������� �� ����������� ID
        /// </summary>
        /// <param name="page">��������</param>
        /// <param name="sb">StringBuilder � ������� ���������� �������� ���������</param>
        /// <param name="clientID">���������� ID ����������</param>
        /// <param name="controlToValidate">���������� ID ������������� ����������</param>
        /// <param name="controlValue">������� �������� ��������</param>
        /// <param name="display">��������� ������������ ���������</param>
        /// <param name="validationGroup">������ ���������</param>
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