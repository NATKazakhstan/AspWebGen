using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI.WebControls;
using Nat.Web.Controls.DateTimeControls;
using System.Web.UI;
using Nat.Web.Tools;
using Nat.Web.Controls.Properties;

namespace Nat.Web.Controls.GenerationClasses
{
    public class DateTimeCompareValidatorProperties : CustomValidatorProperties
    {
        public ValidationCompareOperator Operator { get; private set; }
        public DatePicker CompareTo { get; private set; }
        public Page Page { get; private set; }

        /// <summary>
        /// Заголовок поля.
        /// </summary>
        public string FieldHeader { get; private set; }

        /// <summary>
        /// Заголовок поля с которым выполняется сравнение.
        /// </summary>
        public string CompareToHeader { get; private set; }

        public DateTimeCompareValidatorProperties(ValidationCompareOperator Operator, DatePicker CompareTo, Page Page, string FieldHeader, string CompareToHeader)
        {
            ClientValidationFunction = "DateTimeCompareValidator";
            EnableClientScript = true;
            ControlValidate = true;
            ServerValidate = ServerValidate_DateTimeCompareValidator;
            this.Operator = Operator;
            this.CompareTo = CompareTo;
            this.Page = Page;
            this.FieldHeader = FieldHeader;
            this.CompareToHeader = CompareToHeader;
            switch (Operator)
            {
                case ValidationCompareOperator.Equal:
                    break;
                case ValidationCompareOperator.GreaterThan:
                    this.ErrorMessage = string.Format(Resources.EFieldMustBeMoreOfField, FieldHeader, CompareToHeader);
                    break;
                case ValidationCompareOperator.GreaterThanEqual:
                    this.ErrorMessage = string.Format(Resources.EFieldMustBeMoreOrEqualsOfField, FieldHeader, CompareToHeader);
                    break;
                case ValidationCompareOperator.LessThan:
                    this.ErrorMessage = string.Format(Resources.EFieldMustBeLessOfField, FieldHeader, CompareToHeader);
                    break;
                case ValidationCompareOperator.LessThanEqual:
                    this.ErrorMessage = string.Format(Resources.EFieldMustBeLessOrEqualsOfField, FieldHeader, CompareToHeader);
                    break;
                case ValidationCompareOperator.NotEqual:
                    break;
                default:
                    break;
            }
        }

        protected virtual void ServerValidate_DateTimeCompareValidator(object source, ServerValidateEventArgs args)
        {
            if (string.IsNullOrEmpty(args.Value) || CompareTo.Date == null || string.Empty.Equals(CompareTo.Date))
            {
                args.IsValid = !ValidateEmptyText;
                return;
            }

            var controlValue = Convert.ToDateTime(args.Value);
            var compareToValue = Convert.ToDateTime(CompareTo.Date);

            switch (Operator)
            {
                case System.Web.UI.WebControls.ValidationCompareOperator.Equal:
                    args.IsValid = controlValue == compareToValue;
                    break;
                case System.Web.UI.WebControls.ValidationCompareOperator.GreaterThan:
                    args.IsValid = controlValue > compareToValue;
                    break;
                case System.Web.UI.WebControls.ValidationCompareOperator.GreaterThanEqual:
                    args.IsValid = controlValue >= compareToValue;
                    break;
                case System.Web.UI.WebControls.ValidationCompareOperator.LessThan:
                    args.IsValid = controlValue < compareToValue;
                    break;
                case System.Web.UI.WebControls.ValidationCompareOperator.LessThanEqual:
                    args.IsValid = controlValue <= compareToValue;
                    break;
                case System.Web.UI.WebControls.ValidationCompareOperator.NotEqual:
                    args.IsValid = controlValue != compareToValue;
                    break;
                default:
                    break;
            }
        }
            
        public override BaseValidator CreateValidator(string controlToValidate, string validationGroup)
        {
            var validator = base.CreateValidator(controlToValidate, validationGroup);
            validator.ControlToValidate = string.Empty;
            validator.Attributes["controltovalidate"] =
                CompareTo.Parent.NamingContainer
                .FindControl(controlToValidate)
                .FindControl("textBoxID").ClientID;
            return validator;
        }

        public override BaseValidator CreateValidator(string validationGroup)
        {
            var validator = base.CreateValidator(validationGroup);
            var compareTo = ((IClientElementProvider)CompareTo).GetInputElements().GetEnumerator();
            compareTo.MoveNext();
            validator.Attributes["compareTo"] = compareTo.Current;
            validator.Attributes["operator"] = Operator.ToString();
            return validator;
        }
    }
}
