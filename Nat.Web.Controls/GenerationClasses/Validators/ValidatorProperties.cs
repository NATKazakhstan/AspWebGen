/*
 * Created by: Sergey V. Shpakovskiy
 * Created: 5 ноября 2008 г.
 * Copyright © JSC New Age Technologies 2008
 */

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Nat.Web.Controls.GenerationClasses
{
    using System.Linq;

    [DefaultProperty("ErrorMessageInSummary")]
    public abstract class ValidatorProperties : IValidator
    {
        protected ValidatorProperties()
        {
            EnableClientScript = true;
        }

        private bool isAddedToPage;
        private bool _isRegistered;
        private readonly IList<object> controlValues = new List<object>();
        private readonly Dictionary<string, clientValidatorInfo> clientValidatorInfos = new Dictionary<string, clientValidatorInfo>();

        public bool IsValid { get; set; }
        public string ErrorMessage { get; set; }
        public string ErrorMessageInSummary { get; set; }
        public bool EnableClientScript { get; set; }

        public abstract BaseValidator CreateValidator(string controlToValidate, string validationGroup);
        public virtual BaseValidator CreateValidator(string validationGroup)
        {
            throw new NotSupportedException("Валидатор не подерживает создание без указания контрола");
        }

        public virtual void Validate()
        {
            IsValid = true;
        }

        public virtual bool ValidateValue(string value)
        {
            return true;
        }

        public virtual void CreateClientValidator(Page page, StringBuilder sb, string clientID, string controlToValidate, ValidatorDisplay display, string validationGroup, object controlValue)
        {
            controlValues.Add(controlValue);
            var isValid = ValidateValue((controlValue ?? "").ToString());
            clientValidatorInfos[clientID] =
                new clientValidatorInfo
                    {
                        clientID = clientID,
                        controlToValidate = controlToValidate,
                        display = display,
                        isValid = isValid,
                        validationGroup = validationGroup,
                    };

            sb.Append("&nbsp;<span id=\"");
            sb.Append(clientID);
            sb.Append("\" style=\"color:Red;");
            if (isValid) sb.Append("display:none;");
            sb.Append("\">");
            sb.Append(ErrorMessage);
            sb.Append("</span>");
            if (!isAddedToPage)
            {
                page.Validators.Add(this);
                isAddedToPage = true;
            }
        }

        public virtual void RegisterClientValidator(Page page)
        {
            if (_isRegistered) return;
            _isRegistered = true;
            var i = 0;
            foreach (var clientValidatorInfo in clientValidatorInfos.Values)
            {
                RegisterClientValidator(page, clientValidatorInfo, i + 1);
                RegisterValidatorCommonScript(page);
                RegisterValidatorDeclaration(page, clientValidatorInfo.clientID);
                i++;
            }
        }

        protected virtual void RegisterClientValidator(Page page, clientValidatorInfo validatorInfo, int number)
        {
            AddExpandoAttribute(page, null, validatorInfo.clientID, "controltovalidate", validatorInfo.controlToValidate);
            AddExpandoAttribute(page, null, validatorInfo.clientID, "focusOnError", "t", false);
            AddExpandoAttribute(page, null, validatorInfo.clientID, "errormessage", ErrorMessageInSummary + number.ToString(" (#0)"));
            if (validatorInfo.display != ValidatorDisplay.Static)
                AddExpandoAttribute(page, null, validatorInfo.clientID, "display", PropertyConverter.EnumToString(typeof(ValidatorDisplay), validatorInfo.display), false);
            if (!validatorInfo.isValid)
                AddExpandoAttribute(page, null, validatorInfo.clientID, "isvalid", "False", false);
            if (validatorInfo.validationGroup.Length > 0)
                AddExpandoAttribute(page, null, validatorInfo.clientID, "validationGroup", validatorInfo.validationGroup);
        }

        public void ClearClientValidators()
        {
            controlValues.Clear();
            clientValidatorInfos.Clear();
        }

        internal static void AddExpandoAttribute(Page page, HtmlTextWriter writer, string controlId, string attributeName, string attributeValue)
        {
            AddExpandoAttribute(page, writer, controlId, attributeName, attributeValue, true);
        }

        internal static void AddExpandoAttribute(Page page, HtmlTextWriter writer, string controlId, string attributeName, string attributeValue, bool encode)
        {
            if (writer != null)
            {
                writer.AddAttribute(attributeName, attributeValue, encode);
            }
            else
            {
                page.ClientScript.RegisterExpandoAttribute(controlId, attributeName, attributeValue, encode);
            }
        }

        protected static void RegisterValidatorCommonScript(Page page)
        {
            if (!page.ClientScript.IsClientScriptBlockRegistered(typeof(BaseValidator), "ValidatorIncludeScript"))
            {
                page.ClientScript.RegisterClientScriptResource(typeof(BaseValidator), "WebUIValidation.js");
                page.ClientScript.RegisterStartupScript(typeof(BaseValidator), "ValidatorIncludeScript", "\r\nvar Page_ValidationActive = false;\r\nif (typeof(ValidatorOnLoad) == \"function\") {\r\n    ValidatorOnLoad();\r\n}\r\n\r\nfunction ValidatorOnSubmit() {\r\n    if (Page_ValidationActive) {\r\n        return ValidatorCommonOnSubmit();\r\n    }\r\n    else {\r\n        return true;\r\n    }\r\n}\r\n        ", true);
                page.ClientScript.RegisterOnSubmitStatement(typeof(BaseValidator), "ValidatorOnSubmit", "if (typeof(ValidatorOnSubmit) == \"function\" && ValidatorOnSubmit() == false) return false;");
            }
        }

        protected virtual void RegisterValidatorDeclaration(Page page, string clientID)
        {
            string arrayValue = "document.getElementById(\"" + clientID + "\")";
            page.ClientScript.RegisterArrayDeclaration("Page_Validators", arrayValue);
        }

        protected IList<object> ControlValues
        {
            get { return controlValues; }
        }

        protected struct clientValidatorInfo
        {
            public string clientID;
            public string controlToValidate;
            public ValidatorDisplay display;
            public string validationGroup;
            public bool isValid;
        }
    }
}