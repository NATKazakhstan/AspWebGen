using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Text;
using System.Linq.Expressions;
using Nat.Web.Controls.GenerationClasses.Filter;

namespace Nat.Web.Controls.GenerationClasses.Navigator
{
    public abstract class BaseNavigatorControl : Control
    {
        protected QueryParameters QueryParameters { get; set; }
        public MainPageUrlBuilder Url { get; set; }
        public BaseNavigatorInfo CurrentNavigator
        {
            get { return BaseNavigatorInfo.GetNavigator(NavigatorType); }
        }

        public abstract Type NavigatorType { get; }
        public abstract BaseNavigatorValues BaseNavigatorValues { get; }
        public abstract Expression FilterData(Expression source);
        public abstract string GetNavigatorInfoRowName(Type type, string referenceName, MainPageUrlBuilder urlBuilder);

        public void InitQueryParameters(QueryParameters qParams)
        {
            QueryParameters = qParams;
        }
        
        internal Dictionary<BaseNavigatorInfo, BaseNavigatorCustomControl> NavigatorCustomControls;

        protected void CreateCustomNavigators(BaseNavigatorInfo navigator)
        {
            foreach (var item in navigator.ParentNavigators)
                if (navigator.TableType != item.TableType)
                {
                    if (!string.IsNullOrEmpty(item.CustomNavigatorUserControl))
                    {
                        var control = (BaseNavigatorCustomControl)Page.LoadControl(item.CustomNavigatorUserControl);
                        NavigatorCustomControls[item] = control;
                        Controls.Add(control);
                    }

                    CreateCustomNavigators(item);
                }
        }

        public void AddControl(Control control)
        {
            Controls.Add(control);
        }
    }

    public abstract class BaseNavigatorControl<TNavigatorValues> : BaseNavigatorControl
        where TNavigatorValues : BaseNavigatorValues, new()
    {
        bool _navigatorAdded;
        bool _brAdded;
        TNavigatorValues _values;
        string _logInformation;
        private string _renderedNavigator;

        public event EventHandler ValuesInitialized;

        public override BaseNavigatorValues BaseNavigatorValues
        {
            get { return Values; }
        }

        public TNavigatorValues Values 
        {
            get
            {
                if(_values == null)
                {
                    //сделано чтобы если лежит один и тот же контрол то обход был один раз.
                    if (Parent != null)
                        _values = (TNavigatorValues)HttpContext.Current.Items[typeof(TNavigatorValues).FullName];
                    if (_values == null)
                    {
                        _values = CreateNavigatorValues();
                        InitializeNavigatorValues(_values);
                        if (Parent != null)
                            HttpContext.Current.Items[typeof(TNavigatorValues).FullName] = _values;
                        OnValuesInitialized();
                    }
                }
                return _values;
            }
        }

        public string LogInformation
        {
            get
            {
                if (_logInformation == null)
                {
                    _logInformation = (string)HttpContext.Current.Items[typeof(TNavigatorValues).FullName + ".LogInformation"];
                    if (_logInformation == null)
                    {
                        _logInformation = InitializeLogInformation();
                        HttpContext.Current.Items[typeof(TNavigatorValues).FullName + ".LogInformation"] = _logInformation;
                    }
                }

                return _logInformation;
            }
        }

        protected virtual void OnValuesInitialized()
        {
            if (ValuesInitialized != null)
                ValuesInitialized(this, EventArgs.Empty);
        }

        protected virtual void InitializeNavigatorValues(TNavigatorValues values)
        {
            RecurciveInitializeNavigatorValues(values, CurrentNavigator, "", true);
            //foreach (var item in CurrentNavigator.ParentNavigators)
            //    RecurciveInitializeNavigatorValues(values, item, item.ReferenceName);
        }

        protected virtual void RecurciveInitializeNavigatorValues(TNavigatorValues values, BaseNavigatorInfo navigator, string referenceKey, bool allowGetTableKey)
        {
            var value = navigator.GetKeyValue(Url ?? MainPageUrlBuilder.Current, referenceKey, allowGetTableKey);
            if (value != null) values[navigator.TableType] = value;
            foreach (var item in navigator.ParentNavigators)
                if (navigator.TableType != item.TableType)
                    RecurciveInitializeNavigatorValues(values, item, (string.IsNullOrEmpty(referenceKey) ? "" : referenceKey + ".") + item.ReferenceName, false);
        }

        protected virtual TNavigatorValues CreateNavigatorValues()
        {
            return new TNavigatorValues();
        }

        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);
            NavigatorCustomControls = new Dictionary<BaseNavigatorInfo, BaseNavigatorCustomControl>();
            CreateCustomNavigators(CurrentNavigator);
        }

        protected override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);
            if (NavigatorCustomControls.Count > 0)
                _renderedNavigator = Render();
        }

        protected override void Render(HtmlTextWriter writer)
        {
            if (DesignMode || !Visible) return;

            if (!string.IsNullOrEmpty(_renderedNavigator))
                writer.Write(_renderedNavigator);
            else
            {
                var url = Url ?? MainPageUrlBuilder.Current;
                var urlClone = url.Clone(string.Empty, false, string.Empty, false);
                var navigator = CurrentNavigator;
                foreach (var item in navigator.ParentNavigators)
                    if (navigator.TableType != item.TableType)
                    {
                        var parentUrl = GetUrlForParentNavigator(navigator, urlClone);
                        Render(writer, item, url, parentUrl, item.ReferenceName);
                    }

                if (_navigatorAdded)
                    writer.Write("<hr/>");
            }
        }
        
        public string Render()
        {
            using (var stream = new StringWriter())
            using (var writer = new HtmlTextWriter(stream))
            {
                Render(writer);
                return stream.ToString();
            }
        }

        private void Render(HtmlTextWriter writer, BaseNavigatorInfo navigator, MainPageUrlBuilder url, MainPageUrlBuilder urlClone, string referenceKey)
        {
            if (!navigator.GetVisibleToParent(url.QueryParameters, referenceKey, Values)) return;

            foreach (var item in navigator.ParentNavigators)
                if (navigator.TableType != item.TableType)
                {
                    var parentUrl = GetUrlForParentNavigator(navigator, urlClone);
                    var referenceKeyForRender = string.IsNullOrEmpty(referenceKey)
                                                    ? item.ReferenceName
                                                    : referenceKey + "." + item.ReferenceName;
                    Render(writer, item, url, parentUrl, referenceKeyForRender);
                }

            if (_navigatorAdded && !_brAdded)
            {
                writer.WriteBreak();
                _brAdded = true;
            }

            if (navigator.GetNavigatorToParent(writer, url, urlClone, referenceKey, this))
            {
                _brAdded = false;
                _navigatorAdded = true;
            }
        }

        private static MainPageUrlBuilder GetUrlForParentNavigator(BaseNavigatorInfo navigator, MainPageUrlBuilder url)
        {
            var parentUrl = url.Clone();
            if (parentUrl.QueryParameters.ContainsKey("ref" + navigator.TableName))
                parentUrl.QueryParameters.Remove("ref" + navigator.TableName);
            if (parentUrl.QueryParameters.ContainsKey("ref" + navigator.TableName + "History"))
                parentUrl.QueryParameters.Remove("ref" + navigator.TableName + "History");
            if (parentUrl.QueryParameters.ContainsKey("ref" + navigator.TableName + "Parent"))
                parentUrl.QueryParameters.Remove("ref" + navigator.TableName + "Parent");
            return parentUrl;
        }

        public MainPageUrlBuilder GetUrlForTableType(Type type)
        {
            var url = Url ?? MainPageUrlBuilder.Current;
            var urlClone = url.Clone(string.Empty, false, string.Empty, false);
            var navigator = CurrentNavigator;
            foreach (var item in navigator.ParentNavigators)
                if (navigator.TableType != item.TableType)
                {
                    var parentUrl = GetUrlForParentNavigator(navigator, urlClone);
                    var resultUrl = GetUrlForTableType(type, item, url, parentUrl, item.ReferenceName);
                    if (resultUrl != null) return resultUrl;
                }

            return null;
        }

        private MainPageUrlBuilder GetUrlForTableType(Type type, BaseNavigatorInfo navigator, MainPageUrlBuilder url, MainPageUrlBuilder urlClone, string referenceKey)
        {
            if (!navigator.GetVisibleToParent(url.QueryParameters, referenceKey, Values)) return null;
            if (navigator.TableType == type) return navigator.GetUrlToParentJournal(url, urlClone, referenceKey, Values);
            foreach (var item in navigator.ParentNavigators)
                if (navigator.TableType != item.TableType)
                {
                    var parentUrl = GetUrlForParentNavigator(navigator, urlClone);
                    var resultUrl = GetUrlForTableType(type, item, url, parentUrl, referenceKey + "." + item.ReferenceName);
                    if (resultUrl != null) return resultUrl;
                }

            return null;
        }

        protected virtual string InitializeLogInformation()
        {
            var sb = new StringBuilder();
            var url = Url ?? MainPageUrlBuilder.Current;
            foreach (var item in CurrentNavigator.ParentNavigators)
                RecurciveInitializeLogInformation(sb, item, item.ReferenceName, url, false);
            return sb.ToString();
        }

        protected virtual void RecurciveInitializeLogInformation(StringBuilder sb, BaseNavigatorInfo navigator, string referenceKey, MainPageUrlBuilder url, bool allowGetTableKey)
        {
            if (navigator.RenderLogInformation(sb, url.QueryParameters, referenceKey, allowGetTableKey))
                sb.Append("; ");
            foreach (var item in navigator.ParentNavigators)
                if (navigator.TableType != item.TableType)
                    RecurciveInitializeLogInformation(sb, item, (string.IsNullOrEmpty(referenceKey) ? string.Empty : referenceKey + ".") + item.ReferenceName, url, false);
        }

        public IQueryable FilterData(IQueryable source)
        {
            var param = Expression.Parameter(CurrentNavigator.TableType, "c");
            Expression filter = RecurciveFilterData(param, param, string.Empty, CurrentNavigator, string.Empty);
            if (filter == null) return source;
            Expression pred = Expression.Lambda(filter, param);
            Expression expr = Expression.Call(typeof(Queryable), "Where", new[] { CurrentNavigator.TableType }, source.Expression, pred);
            return source.Provider.CreateQuery(expr);
        }

        public override Expression FilterData(Expression source)
        {
            var param = Expression.Parameter(CurrentNavigator.TableType, "c");
            Expression filter = RecurciveFilterData(param, param, string.Empty, CurrentNavigator, string.Empty);
            if (filter == null) return source;
            Expression pred = Expression.Lambda(filter, param);
            return Expression.Call(typeof(Queryable), "Where", new[] { CurrentNavigator.TableType }, source, pred);
        }

        protected virtual Expression RecurciveFilterData(ParameterExpression param, Expression parentExp, string propertyName, BaseNavigatorInfo navigator, string referenceKey)
        {
            Expression exp = null;
            Expression filter = null;
            navigator.InitQueryParameters(QueryParameters);
            if (navigator != CurrentNavigator)
            {
                var url = Url ?? MainPageUrlBuilder.Current;
                if (!navigator.GetFilterByParent(url.QueryParameters, referenceKey, Values))
                    return filter;

                exp = string.IsNullOrEmpty(propertyName) ? parentExp : Expression.Property(parentExp, propertyName); 
                filter = navigator.FilterData(url, exp, referenceKey);
                if (filter != null && navigator.FilterDataSeterEnough)
                    return filter;
            }

            if (exp == null)
                exp = string.IsNullOrEmpty(propertyName) ? parentExp : Expression.Property(parentExp, propertyName); 
            foreach (var item in navigator.ParentNavigators)
            {
                if (navigator.TableType != item.TableType)
                {
                    var innerFilter = RecurciveFilterData(
                        param,
                        exp,
                        item.ReferenceName,
                        item,
                        (string.IsNullOrEmpty(referenceKey) ? string.Empty : referenceKey + ".") + item.ReferenceName);
                    if (innerFilter != null)
                        filter = filter == null ? innerFilter : Expression.And(innerFilter, filter);
                }
            }

            return filter;
        }

        public override string GetNavigatorInfoRowName(Type tableType, string referenceName, MainPageUrlBuilder urlBuilder)
        {
            string result;
            if (GetNavigatorInfoRowNameByParents(CurrentNavigator, tableType, referenceName, urlBuilder, out result))
                return result;
            if (GetNavigatorInfoRowNameByChilds(CurrentNavigator, tableType, referenceName, urlBuilder, out result))
                return result;
            return null;
        }

        private static bool GetNavigatorInfoRowNameByChilds(BaseNavigatorInfo navigatorInfo, Type tableType, string referenceName, MainPageUrlBuilder urlBuilder, out string result)
        {
            result = null;
            if (navigatorInfo.TableType == tableType)
            {
                var value = navigatorInfo.GetKeyValue(urlBuilder.QueryParameters, referenceName, false);
                if (string.IsNullOrEmpty(value)) return false;
                result = navigatorInfo.GetRowName(value);
                return true;
            }

            foreach (var childNavigator in navigatorInfo.ChildsNavigators)
            {
                if (childNavigator.TableType == navigatorInfo.TableType) continue;
                if (GetNavigatorInfoRowNameByChilds(childNavigator, tableType, referenceName, urlBuilder, out result))
                    return true;
            }

            return false;
        }

        private static bool GetNavigatorInfoRowNameByParents(BaseNavigatorInfo navigatorInfo, Type tableType, string referenceName, MainPageUrlBuilder urlBuilder, out string result)
        {
            result = null;
            if (navigatorInfo.TableType == tableType)
            {
                var value = navigatorInfo.GetKeyValue(urlBuilder.QueryParameters, referenceName, false);
                if (string.IsNullOrEmpty(value))
                    return false;
                result = navigatorInfo.GetRowName(value);
                return true;
            }

            foreach (var parentNavigator in navigatorInfo.ParentNavigators)
            {
                if (parentNavigator.TableType == navigatorInfo.TableType)
                    continue;
                if (GetNavigatorInfoRowNameByParents(parentNavigator, tableType, referenceName, urlBuilder, out result))
                    return true;
            }

            return false;
        }

        public void Redirect(bool anyWay, params Type[] types)
        {
            var url = Url ?? MainPageUrlBuilder.Current;
            var urlClone = url.Clone("", false, "", false);
            var navigator = CurrentNavigator;
            foreach (var item in navigator.ParentNavigators)
                if (navigator.TableType != item.TableType)
                {
                    var parentUrl = GetUrlForParentNavigator(navigator, urlClone);
                    Redirect(types, anyWay, item, url, parentUrl, item.ReferenceName);
                }
        }

        public void Redirect(Type type)
        {
            Redirect(true, type);
        }

        private void Redirect(IEnumerable<Type> types, bool anyWay, BaseNavigatorInfo navigator, MainPageUrlBuilder url, MainPageUrlBuilder urlClone, string referenceKey)
        {
            if (!navigator.GetVisibleToParent(url.QueryParameters, referenceKey, Values)) return;

            if (types.Contains(navigator.TableType))
            {
                var value = navigator.GetKeyValue(url.QueryParameters, referenceKey, false);
                if (value != null || anyWay)
                {
                    navigator.MoveQueryParametersToUp(urlClone, referenceKey);
                    HttpContext.Current.Response.Redirect(navigator.GetJournalUrl(urlClone));
                }
            }

            foreach (var item in navigator.ParentNavigators)
                if (navigator.TableType != item.TableType)
                {
                    var parentUrl = GetUrlForParentNavigator(navigator, urlClone);
                    Redirect(types, anyWay, item, url, parentUrl, referenceKey + "." + item.ReferenceName);
                }
        }
    }
}