/*
* Created by: Sergey V. Shpakovskiy
* Created: 2013.02.06
* Copyright © JSC NAT Kazakhstan 2013
*/

namespace Nat.Web.Controls.ExtNet.Validators
{
    using System.Web.UI;
    using System.Web.UI.WebControls;

    using Ext.Net;

    public class ValidateInformationForm : GenerationClasses.ValidateInformationForm
    {
        public ValidateInformationForm(Control form)
            : base(form)
        {
        }

        public override bool Validate()
        {
            var validate = base.Validate();
            if (!validate && Form != null && Form.Page != null)
            {
                var validators = Form.Page.GetValidators(ValidationGroup);
                foreach (BaseValidator validator in validators)
                {
                    if (validator.IsValid || !validator.Enabled)
                        continue;

                    var control = Form.FindControl(validator.ControlToValidate);
                    if (control == null)
                    {
                        // todo: thing
                        continue;
                    }

                    var field = control as Field;
                    if (field != null)
                        field.MarkInvalid(validator.ErrorMessage);
                }
            }

            return validate;
        }
    }
}
