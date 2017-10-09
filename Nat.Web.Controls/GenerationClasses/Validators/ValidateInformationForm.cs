/*
* Created by: Sergey V. Shpakovskiy
* Created: 2013.02.04
* Copyright © JSC NAT Kazakhstan 2013
*/

namespace Nat.Web.Controls.GenerationClasses
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Web.UI;

    public class ValidateInformationForm
    {
        public ValidateInformationForm(Control form)
        {
            Validators = new Dictionary<string, ValidateInformation>();
            Form = form;
        }

        public ValidateInformationForm(Dictionary<string, ValidateInformation> validators)
        {
            Validators = validators;
        }

        public string ValidationGroup { get; set; }

        public Control Form { get; set; }

        protected Dictionary<string, ValidateInformation> Validators { get; private set; }

        public virtual void ApplyValidators(ValidateInformationFormApplyArgsInfo args)
        {
            if (args.Control == null || !Validators.ContainsKey(args.ValidatorCode))
                return;

            var container = Form.FindControl(args.ValidationContainer);

            if (container == null)
                return;

            var info = Validators[args.ValidatorCode];
            var validatorProperties = info.ValidatorProperties;
            foreach (var validatorInfo in validatorProperties)
            {
                var validator = validatorInfo.CreateValidator(args.Control.ID, ValidationGroup);
                container.Controls.Add(validator);
            }
        }

        public virtual bool Validate()
        {
            if (Form != null && Form.Page != null)
            {
                Form.Page.Validate(ValidationGroup);
                return Form.Page.IsValid;
            }

            return false;
        }
        
        public virtual bool IsRequired(string validatorCode)
        {
            if (!Validators.ContainsKey(validatorCode))
                return false;

            return Validators[validatorCode].RequiredValidatorPropertieses.Any(r => r.InitialValue == string.Empty);
        }
    }
}