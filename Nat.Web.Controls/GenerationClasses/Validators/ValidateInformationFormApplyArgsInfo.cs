/*
* Created by: Sergey V. Shpakovskiy
* Created: 2013.02.06
* Copyright © JSC NAT Kazakhstan 2013
*/

namespace Nat.Web.Controls.GenerationClasses
{
    using System.Web.UI;

    public class ValidateInformationFormApplyArgsInfo
    {
        public string ValidatorCode { get; set; }
        
        public Control Control { get; set; }
        
        public bool ReadOnly { get; set; }
        
        public string ValidationContainer { get; set; }
    }
}