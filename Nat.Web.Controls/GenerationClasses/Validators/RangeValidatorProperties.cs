/*
 * Created by: Sergey V. Shpakovskiy
 * Created: 5 ноября 2008 г.
 * Copyright © JSC New Age Technologies 2008
 */

using System.Web.UI.WebControls;

namespace Nat.Web.Controls.GenerationClasses
{
    public class RangeValidatorProperties : ValidatorProperties
    {
        public string MinimumValue { get; set; }
        public string MaximumValue { get; set; }
        public ValidationDataType Type { get; set; }

        public override BaseValidator CreateValidator(string controlToValidate, string validationGroup)
        {
            return new RangeValidator
                       {
                           ControlToValidate = controlToValidate,
                           Display = ValidatorDisplay.Dynamic,
                           EnableViewState = false,
                           ValidationGroup = validationGroup,
                           ErrorMessage = ErrorMessageInSummary,
                           Text = ErrorMessage,
                           MinimumValue = MinimumValue,
                           MaximumValue = MaximumValue,
                           Type = Type,
                           EnableClientScript = EnableClientScript,
                           //SetFocusOnError = true,
                       };
        }
    }
}