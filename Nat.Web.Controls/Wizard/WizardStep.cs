namespace Nat.Web.Controls.Wizard
{
    using System;
    using System.Collections.Generic;
    using System.Web;

    using Nat.Web.Controls.GenerationClasses.Navigator;
    using Nat.Web.Controls.Properties;

    public abstract class WizardStep
    {
        protected WizardStep(Type onForm, WizardOnAction onAction, Type toAction, Type toForm, Type toFormNavigatorInfo)
            : this(onForm, new []{onAction}, toAction, toForm, toFormNavigatorInfo)
        {
        }

        protected WizardStep(Type onForm, IEnumerable<WizardOnAction> onAction, Type toAction, Type toForm, Type toFormNavigatorInfo)
        {
            ToFormNavigatorInfo = toFormNavigatorInfo;
            ToForm = toForm;
            OnForm = onForm;
            OnAction = new List<WizardOnAction>(onAction);
            ToAction = toAction;
        }

        public string Header { get; set; }
        public string ButtonImage { get; set; }
        public Action<WizardController, WizardOnAction, MainPageUrlBuilder> PrepareUrl { get; set; }
        public Func<WizardController, WizardOnAction, bool> EnabledHandler { get; set; }

        public Type OnForm { get; private set; }
        public List<WizardOnAction> OnAction { get; private set; }
        public Type ToAction { get; private set; }
        public Type ToForm { get; private set; }
        public Type ToFormNavigatorInfo { get; private set; }

        public abstract void Run(WizardController wizardController, WizardOnAction onAction);
    }

    public class WizardStep<TFromNavigatorControl, TWizardAction, TNextNavigatorControl, TNextNavigatorInfo> : WizardStep
        where TNextNavigatorControl : BaseNavigatorControl
        where TNextNavigatorInfo : BaseNavigatorInfo
        where TFromNavigatorControl : BaseNavigatorControl
        where TWizardAction : WizardAction, new()
    {
        public WizardStep(params WizardOnAction[] onAction)
            : base(typeof(TFromNavigatorControl), onAction, typeof(TWizardAction), typeof(TNextNavigatorControl), typeof(TNextNavigatorInfo))
        {
        }

        public override void Run(WizardController wizardController, WizardOnAction onAction)
        {
            var action = new TWizardAction();
            if (EnabledHandler != null && !EnabledHandler(wizardController, onAction))
                return;

            var urlBuilder = MainPageUrlBuilder.Current.Clone();
            if (onAction == WizardOnAction.AfterAdded && !string.IsNullOrEmpty(wizardController.NewID))
                urlBuilder.QueryParameters["ref" + wizardController.Navigator.CurrentNavigator.TableName] = wizardController.NewID;
            if (PrepareUrl != null)
                PrepareUrl(wizardController, onAction, urlBuilder);

            action.Execute(wizardController, this, urlBuilder);
            if (action.NextUrlBuilder != null)
            {
                if (onAction == WizardOnAction.NextButton)
                    AddButtons(wizardController, action.NextUrlBuilder);
                else
                    HttpContext.Current.Response.Redirect(action.NextUrlBuilder.CreateUrl());
            }
            else if (!string.IsNullOrEmpty(action.NextUrl))
            {
                if (onAction == WizardOnAction.NextButton)
                    AddButtons(wizardController, action.NextUrl);
                else
                    HttpContext.Current.Response.Redirect(action.NextUrl);
            }
        }

        protected virtual void AddButtons(WizardController wizardController, MainPageUrlBuilder url)
        {
            AddButtons(wizardController, url.CreateUrl());
        }

        protected virtual void AddButtons(WizardController wizardController, string url)
        {
            if (string.IsNullOrEmpty(Header) || wizardController.Buttons == null)
                return;

            AddButton(wizardController, url, Header, ButtonImage ?? Themes.IconUrlArrowRightMin, Resources.SWizardNext);
        }

        protected virtual void AddButton(WizardController wizardController, string url, string header, string imageUrl, string tooltip)
        {
            var text = string.Format("{0} <img src=\"{1}\" style=\"border:0;position:relative;top:1px\" />", header, imageUrl);
            wizardController.Buttons.AddHyperLink(url, text, tooltip);
        }
    }
}