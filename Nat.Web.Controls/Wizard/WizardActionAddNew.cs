namespace Nat.Web.Controls.Wizard
{
    using Nat.Web.Controls.GenerationClasses;

    public class WizardActionAddNew : WizardAction
    {
        public override void Execute(WizardController wizardController, WizardStep step, MainPageUrlBuilder url)
        {
            var navigator = CreateNavigator(step.ToForm);
            var navigatorInfo = CreateNavigatorInfo(step.ToFormNavigatorInfo);
            if (!ChangeUrlToDestination(wizardController.Navigator, navigator, url))
                return;

            // для строк один к одному проверяем наличие записи
            if (navigatorInfo.IsOneToOne)
            {
                var source = (IDataSourceViewGetName)navigator.CurrentNavigator.DataSource;
                var paramKey = "ref" + navigatorInfo.TableName;

                if (url.QueryParameters.ContainsKey(paramKey))
                {
                    var key = url.QueryParameters[paramKey];
                    if (!string.IsNullOrEmpty(key) && !string.IsNullOrEmpty(source.GetName(key)))
                        return;
                }
            }

            url.UserControl = navigator.CurrentNavigator.TableName + "Edit";
            url.IsNew = true;
            url.IsRead = false;
            NextUrlBuilder = url;
        }
    }
}