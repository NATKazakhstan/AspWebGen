namespace Nat.Web.WorkFlow
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Web.UI;

    using Nat.Web.Controls.GenerationClasses;

    public delegate void WorkFlowValidationHandler(ValidateInformationForm validationInfo);

    public class WorkFlowValidation<TControlInfo>
        where TControlInfo : BaseControlInfo
    {
        public WorkFlowValidation()
        {
        }

        public WorkFlowValidation(WorkFlowValidationHandler initializeValidationFunction)
        {
            InitializeValidationFunction = initializeValidationFunction;
        }

        protected Dictionary<Control, ValidateInformation> ValidateInformation { get; set; }

        public TControlInfo Info { get; set; }

        public virtual WorkFlowItem BaseWorkFlowItem { get; set; }

        public WorkFlowValidationHandler InitializeValidationFunction { get; set; }

        public virtual void InitializeValidations(ValidateInformationForm validationInfo)
        {
            if (InitializeValidationFunction == null)
                throw new Exception("Property 'InitializeValidationFunction' is null and method 'InitializeValidations' is not overrided");

            InitializeValidationFunction(validationInfo);
        }

        public virtual void InitializeValidations(Dictionary<Control, ValidateInformation> validationInfo)
        {
            if (InitializeValidationFunction == null)
                throw new Exception("Property 'InitializeValidationFunction' is null and method 'InitializeValidations' is not overrided");

            InitializeValidationFunction(new ValidateInformationForm(validationInfo.ToDictionary(r => r.Key.ID, r => r.Value)));
        }
    }
}