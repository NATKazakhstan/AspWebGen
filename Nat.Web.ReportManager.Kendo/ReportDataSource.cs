using System;
using System.Collections;
using System.Linq.Expressions;
using System.Web;
using System.Web.Mvc;
using System.Web.Mvc.Html;
using System.Web.Routing;
using Nat.Controls.DataGridViewTools;

namespace Nat.Web.ReportManager.Kendo
{
    public abstract class ReportDataSource : IEnumerable
    {
        public IEnumerable Data { get; set; }
        public abstract IEnumerable GetStaticData(ColumnFilterStorage storage);
        public abstract string GetTemplateValue(string name);
        public IEnumerator GetEnumerator()
        {
            return Data?.GetEnumerator() ?? new string[0].GetEnumerator();
        }
        public abstract string[] GetTexts(ColumnFilterStorage storage);
    }

    public class ReportDataSource<TModelView, TField> : ReportDataSource
        where TModelView : class
    {
        private readonly Expression<Func<TModelView, TField>> _field;
        private readonly Func<ColumnFilterStorage, string[]> _getTexts;
        public TModelView Model { get; }
        public ModelMetadata ModelMetaData { get; }
        HtmlHelper<TModelView> htmlHelper;

        public ReportDataSource(Expression<Func<TModelView, TField>> field, TModelView model, Func<ColumnFilterStorage, string[]> getTexts)
        {
            _field = field;
            _getTexts = getTexts;
            Model = model;
            var member = field.Body as MemberExpression ?? (MemberExpression) ((UnaryExpression) field.Body).Operand;
            ModelMetaData = ModelMetadataProviders.Current.GetMetadataForProperty(
                () => Model, typeof(TModelView), member.Member.Name);

            //new ViewDataDictionary<TModelView>(Model).ModelMetadata = ModelMetaData;
        }

        public override string[] GetTexts(ColumnFilterStorage storage)
        {
            return _getTexts?.Invoke(storage);
        }

        private HtmlHelper<TModelView> GetHtmlHelper()
        {
            if (htmlHelper == null)
            {
                var viewData = new ViewDataDictionary(Model) {ModelMetadata = ModelMetaData};
                var httpContext = new HttpContextWrapper(HttpContext.Current);
                var viewContext = new ViewContext
                {
                    HttpContext = httpContext,
                    ViewData = viewData,
                    RouteData = new RouteData()
                };
                viewContext.RouteData.Values["action"] = "Index";
                viewContext.RouteData.Values["controller"] = "Manager";
                viewContext.RouteData.Values["area"] = "Reports";
                viewContext.TempData = new TempDataDictionary();
                htmlHelper = new HtmlHelper<TModelView>(viewContext, new TempContainer(viewData));
            }

            return htmlHelper;
        }

        public override IEnumerable GetStaticData(ColumnFilterStorage storage)
        {
            return null;
        }

        public override string GetTemplateValue(string name)
        {
            return GetHtmlHelper().EditorFor(_field, null, name).ToHtmlString();
        }

        private class TempContainer : IViewDataContainer
        {
            public TempContainer(ViewDataDictionary viewData)
            {
                ViewData = viewData;
            }

            public ViewDataDictionary ViewData { get; set; }
        }
    }
}