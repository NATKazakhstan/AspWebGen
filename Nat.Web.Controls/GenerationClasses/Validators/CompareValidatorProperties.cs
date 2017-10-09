/*
 * Created by: Sergey V. Shpakovskiy
 * Created: 5 ноября 2008 г.
 * Copyright © JSC New Age Technologies 2008
 */

using System.Web.UI;
using System.Web.UI.WebControls;

namespace Nat.Web.Controls.GenerationClasses
{
    public class CompareValidatorProperties : ValidatorProperties
    {
        public Control ControlToCompare { get; set; }
        public string ValueToCompare { get; set; }
        public ValidationCompareOperator Operator { get; set; }
        public ValidationDataType Type { get; set; }

        public override BaseValidator CreateValidator(string controlToValidate, string validationGroup)
        {
            return new CompareValidator
                       {
                           ControlToValidate = controlToValidate,
                           Display = ValidatorDisplay.Dynamic,
                           EnableViewState = false,
                           ValidationGroup = validationGroup,
                           ErrorMessage = ErrorMessageInSummary,
                           Text = ErrorMessage,
                           ControlToCompare = ControlToCompare == null ? "" : ControlToCompare.ID,
                           ValueToCompare = ValueToCompare,
                           Operator = Operator,
                           Type = Type,
                           EnableClientScript = EnableClientScript,
                           //SetFocusOnError = true,
                       };
        }

    }
}