namespace Nat.Web.Controls.Wizard
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using Nat.Web.Controls.GenerationClasses;
    using Nat.Web.Controls.GenerationClasses.Navigator;

    public abstract class WizardController
    {
        protected Dictionary<Type, List<WizardStep>> WizardSteps { get; private set; }
        
        public BaseNavigatorControl Navigator { get; private set; }

        public AdditionalButtons Buttons { get; set; }

        public string NewID { get; set; }

        public static TWizardController GetWizard<TWizardController>(BaseNavigatorControl navigator)
            where TWizardController : WizardController, new()
        {
            var wizard = new TWizardController
                {
                    Navigator = navigator
                };
            wizard.Initialize();
            return wizard;
        }

        public static void RunNextButtons<TWizardController>(BaseNavigatorControl navigator, AdditionalButtons buttons)
            where TWizardController : WizardController, new()
        {
            var wizard = GetWizard<TWizardController>(navigator);
            wizard.Buttons = buttons;
            wizard.Run(WizardOnAction.NextButton);
        }

        public static void Run<TWizardController>(BaseNavigatorControl navigator, WizardOnAction action)
            where TWizardController : WizardController, new()
        {
            var wizard = GetWizard<TWizardController>(navigator);
            wizard.Run(action);
        }

        public static string GetHeader<TWizardController>(BaseNavigatorControl navigator, WizardOnAction action)
            where TWizardController : WizardController, new()
        {
            var wizard = GetWizard<TWizardController>(navigator);
            return wizard.GetHeader(action);
        }

        private void Initialize()
        {
            WizardSteps = new Dictionary<Type, List<WizardStep>>();
            InitializeWizard();
        }

        protected abstract void InitializeWizard();

        protected void Add(WizardStep step)
        {
            if (!WizardSteps.ContainsKey(step.OnForm))
                WizardSteps[step.OnForm] = new List<WizardStep>();
            WizardSteps[step.OnForm].Add(step);
        }

        public void Run(WizardOnAction action)
        {
            if (!WizardSteps.ContainsKey(Navigator.GetType()))
                return;

            foreach (var step in WizardSteps[Navigator.GetType()].Where(step => step.OnAction.Contains(action)))
                step.Run(this, action);
        }

        public string GetHeader(WizardOnAction action)
        {
            if (!WizardSteps.ContainsKey(Navigator.GetType()))
                return null;

            return WizardSteps[Navigator.GetType()]
                .Where(step => step.OnAction.Contains(action))
                .Where(r => r.EnabledHandler == null || r.EnabledHandler(this, action))
                .Select(step => step.Header)
                .FirstOrDefault();
        }
    }
}