namespace Nat.Web.Controls.Wizard
{
    public class WizardActionOpenJournal : WizardAction
    {
        public override void Execute(WizardController wizardController, WizardStep step, MainPageUrlBuilder url)
        {
            var navigator = CreateNavigator(step.ToForm);
            if (!ChangeUrlToDestination(wizardController.Navigator, navigator, url))
                return;

            url.UserControl = navigator.CurrentNavigator.TableName + "Journal";
            url.IsNew = false;
            url.IsRead = false;
            NextUrlBuilder = url;
        }
    }
}