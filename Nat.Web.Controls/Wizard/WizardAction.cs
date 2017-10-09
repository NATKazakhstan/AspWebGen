namespace Nat.Web.Controls.Wizard
{
    using System;
    using System.Linq;

    using Nat.Web.Controls.GenerationClasses.Navigator;

    public abstract class WizardAction
    {
        public string NextUrl { get; protected set; }
        public MainPageUrlBuilder NextUrlBuilder { get; protected set; }

        public abstract void Execute(WizardController wizardController, WizardStep step, MainPageUrlBuilder url);
        
        protected BaseNavigatorControl CreateNavigator(Type type)
        {
            return (BaseNavigatorControl)Activator.CreateInstance(type);
        }

        protected BaseNavigatorInfo CreateNavigatorInfo(Type type)
        {
            return (BaseNavigatorInfo)Activator.CreateInstance(type);
        }

        protected bool ChangeUrlToDestination(BaseNavigatorControl fromNavigator, BaseNavigatorControl destinationNavigator, MainPageUrlBuilder url)
        {
            if (fromNavigator.CurrentNavigator.TableType == destinationNavigator.CurrentNavigator.TableType)
                return true;

            var childToCurrent = destinationNavigator.CurrentNavigator.ParentNavigators.FirstOrDefault(r => r.TableType == fromNavigator.CurrentNavigator.TableType);
            if (childToCurrent != null)
            {
                ChangeUrlToChild(url, childToCurrent.ReferenceName, fromNavigator.CurrentNavigator.TableName);
                return true;
            }

            var toParent = fromNavigator.CurrentNavigator.ParentNavigators.FirstOrDefault(r => r.TableType == destinationNavigator.CurrentNavigator.TableType);
            if (toParent != null)
            {
                ChangeUrlToParent(url, toParent.ReferenceName, destinationNavigator.CurrentNavigator.TableName);
                return true;
            }

            var parent = fromNavigator.CurrentNavigator.ParentNavigators
                .Select(r => new { toParent = r, toChild = destinationNavigator.CurrentNavigator.ParentNavigators.FirstOrDefault(t => t.TableType == r.TableType) })
                .FirstOrDefault(r => r.toChild != null);
            if (parent != null)
            {
                ChangeUrlToParent(url, parent.toParent.ReferenceName, parent.toParent.TableName);
                ChangeUrlToChild(url, parent.toChild.ReferenceName, parent.toParent.TableName);
                return true;
            }

            var doubleParent = fromNavigator.CurrentNavigator.ParentNavigators
                .SelectMany(r => r.ParentNavigators.Select(c => new { toParent0 = r, toParent1 = c }))
                .Select(
                    r => new
                        {
                            r.toParent0,
                            r.toParent1,
                            toChild = destinationNavigator.CurrentNavigator.ParentNavigators.FirstOrDefault(t => t.TableType == r.toParent1.TableType)
                        })
                .FirstOrDefault(r => r.toChild != null);
            if (doubleParent != null)
            {
                ChangeUrlToParent(url, doubleParent.toParent0.ReferenceName, doubleParent.toParent0.TableName);
                ChangeUrlToParent(url, doubleParent.toParent1.ReferenceName, doubleParent.toParent1.TableName);
                ChangeUrlToChild(url, doubleParent.toChild.ReferenceName, doubleParent.toParent1.TableName);
                return true;
            }

            var tripleParent = fromNavigator.CurrentNavigator.ParentNavigators
                .SelectMany(r => r.ParentNavigators.Select(c => new { toParent0 = r, toParent1 = c }))
                .SelectMany(r => r.toParent1.ParentNavigators.Select(c => new { r.toParent0, r.toParent1, toParent2 = c }))
                .Select(
                    r => new
                        {
                            r.toParent0,
                            r.toParent1,
                            r.toParent2,
                            toChild = destinationNavigator.CurrentNavigator.ParentNavigators.FirstOrDefault(t => t.TableType == r.toParent2.TableType)
                        })
                .FirstOrDefault(r => r.toChild != null);
            if (tripleParent != null)
            {
                ChangeUrlToParent(url, tripleParent.toParent0.ReferenceName, tripleParent.toParent0.TableName);
                ChangeUrlToParent(url, tripleParent.toParent1.ReferenceName, tripleParent.toParent1.TableName);
                ChangeUrlToParent(url, tripleParent.toParent2.ReferenceName, tripleParent.toParent2.TableName);
                ChangeUrlToChild(url, tripleParent.toChild.ReferenceName, tripleParent.toParent2.TableName);
                return true;
            }

            return false;
        }

        public static void ChangeUrlToChild(MainPageUrlBuilder url, string referenceName, string fromTableName)
        {
            referenceName += ".";
            var changeKeys = url.QueryParameters.Where(r => r.Key.Contains(".")).ToList();
            foreach (var parameter in changeKeys)
            {
                url.QueryParameters.Remove(parameter.Key);
                url.QueryParameters[referenceName + parameter.Key] = parameter.Value;
            }

            if (url.QueryParameters.ContainsKey("ref" + fromTableName))
                url.QueryParameters[referenceName + "id"] = url.QueryParameters["ref" + fromTableName];
        }

        public static void ChangeUrlToParent(MainPageUrlBuilder url, string referenceName, string toTableName)
        {
            referenceName += ".";
            var changeKeys = url.QueryParameters.Where(r => r.Key.StartsWith(referenceName)).ToList();
            foreach (var parameter in changeKeys)
            {
                url.QueryParameters.Remove(parameter.Key);
                var newKey = parameter.Key.Remove(0, referenceName.Length);
                if (newKey.Contains("."))
                    url.QueryParameters[newKey] = parameter.Value;
                else
                    url.QueryParameters["ref" + toTableName] = parameter.Value;
            }
        }
    }
}