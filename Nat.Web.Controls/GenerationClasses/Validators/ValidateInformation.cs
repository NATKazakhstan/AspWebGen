/*
 * Created by: Sergey V. Shpakovskiy
 * Created: 8 ноября 2008 г.
 * Copyright © JSC New Age Technologies 2008
 */

namespace Nat.Web.Controls.GenerationClasses
{
    using System.Collections.Generic;
    using System.Linq;

    public class ValidateInformation
    {
        public ValidateInformation()
        {
            RegexValidatorPropertieses = new List<RegexValidatorProperties>(1);
            CompareValidatorPropertieses = new List<CompareValidatorProperties>(1);
            RangeValidatorPropertieses = new List<RangeValidatorProperties>(1);
            RequiredValidatorPropertieses = new List<RequiredValidatorProperties>(1);
            CustomValidatorProperties = new List<CustomValidatorProperties>(1);
        }

        public List<RegexValidatorProperties> RegexValidatorPropertieses { get; private set; }

        public List<CompareValidatorProperties> CompareValidatorPropertieses { get; private set; }

        public List<RangeValidatorProperties> RangeValidatorPropertieses { get; private set; }

        public List<RequiredValidatorProperties> RequiredValidatorPropertieses { get; private set; }

        public List<CustomValidatorProperties> CustomValidatorProperties { get; private set; }

        public IEnumerable<ValidatorProperties> ValidatorProperties
        {
            get
            {
                return RegexValidatorPropertieses.Cast<ValidatorProperties>()
                    .Union(CompareValidatorPropertieses.Cast<ValidatorProperties>())
                    .Union(RangeValidatorPropertieses.Cast<ValidatorProperties>())
                    .Union(CustomValidatorProperties.Cast<ValidatorProperties>())
                    .Union(RequiredValidatorPropertieses.Cast<ValidatorProperties>());
            }
        }

        public void AddRequiredValidator()
        {
            if (RequiredValidatorPropertieses.Count == 0)
                RequiredValidatorPropertieses.Add(new RequiredValidatorProperties());
        }

        public void AddRequiredValidator(string errorMessageInSummary)
        {
            if (RequiredValidatorPropertieses.Count == 0)
                RequiredValidatorPropertieses.Add(new RequiredValidatorProperties { ErrorMessageInSummary = errorMessageInSummary });
        }

        public void Join(ValidateInformation validateInformation)
        {
            RegexValidatorPropertieses.AddRange(validateInformation.RegexValidatorPropertieses);
            CompareValidatorPropertieses.AddRange(validateInformation.CompareValidatorPropertieses);
            RangeValidatorPropertieses.AddRange(validateInformation.RangeValidatorPropertieses);
            CustomValidatorProperties.AddRange(validateInformation.CustomValidatorProperties);
            RequiredValidatorPropertieses.AddRange(
                validateInformation.RequiredValidatorPropertieses.Where(
                    r => !RequiredValidatorPropertieses.Any(rv => rv.InitialValue == r.InitialValue)));
        }
    }
}