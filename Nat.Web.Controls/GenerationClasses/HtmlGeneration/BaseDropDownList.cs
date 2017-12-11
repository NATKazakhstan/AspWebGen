using System;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Web.Compilation;
using System.Web.UI;
using System.Collections;

namespace Nat.Web.Controls.GenerationClasses
{
    using System.Linq;

    [DefaultProperty("SelectedValue")]
    [DefaultBindingProperty("SelectedValue")]
    [ValidationPropertyAttribute("SelectedValue")]
    public class BaseDropDownList : BaseEditControl, IDropDownList, IPostBackDataHandler
    {
        public Action<IList> SetCache { get; set; }
        public Func<IList> GetCache { get; set; }

        public IEnumerable GetData()
        {
            var data = DataSource as IEnumerable;
            if (data != null) return data;

            if (GetCache != null)
            {
                var cache = GetCache();
                if (cache != null) return cache;
            }

            var idataSource = DataSource as IDataSource;
            DataSourceView view = null;
            if (idataSource != null)
                view = idataSource.GetView("Default");
            else if (!string.IsNullOrEmpty(ProjectName) && !string.IsNullOrEmpty(TableName))
            {
                var type = BuildManager.GetType(ProjectName + "." + TableName + "JournalDataSourceView", false, true);
                if (type != null)
                    view = (DataSourceView)Activator.CreateInstance(type);
            }

            if (view != null)
            {
                var baseView = view as IDataSourceView;
                if (baseView != null && SelectedValue != null)
                    baseView.SelectedRowKey = SelectedValue.ToString();

                if (baseView != null && !string.IsNullOrEmpty(SelectMode))
                    data = baseView.GetSelectIRow("mode=" + SelectMode);
                else
                {
                    view.Select(
                        new DataSourceSelectArguments(),
                        delegate(IEnumerable dataSource) { data = dataSource; });
                }

                if (SetCache != null)
                {
                    var result = data.Cast<object>().ToList();
                    SetCache(result);
                    return result;
                }

                return data;
            }

            return null;
        }

        public string NamePropertyName { get; set; }
        public string TitlePropertyName { get; set; }
        public bool AllowValueNotSet { get; set; }
        public string TextOfValueNotSet { get; set; }
        public object SelectedValue { get; set; }
        public string IDPropertyName { get; set; }
        public override string Text { get; set; }
        public object DataSource { get; set; }
        public string ProjectName { get; set; }
        public string TableName { get; set; }
        public string SelectMode { get; set; }

        public override object Value
        {
            get { return SelectedValue; }
            set { SelectedValue = value; }
        }

        protected override void RenderContents(HtmlTextWriter writer)
        {
            HtmlGenerator.RenderDropDownList(writer, this, null);
        }
        
        public override void Render(HtmlTextWriter writer, ExtenderAjaxControl extenderAjaxControl)
        {
            HtmlGenerator.RenderDropDownList(writer, this, extenderAjaxControl);
        }

        public override void InitAjax(Control control, ExtenderAjaxControl extenderAjaxControl)
        {
        }

        public bool LoadPostData(string postDataKey, NameValueCollection postCollection)
        {
            // note: IExplorer отправляет Post с текстом вместо value.
            var value = postCollection[((IRenderComponent)this).UniqueID] ?? postCollection[UniqueID];
            SelectedValue = (value ?? string.Empty).Equals(TextOfValueNotSet, StringComparison.OrdinalIgnoreCase) ? null : value;
            return true;
        }

        public void RaisePostDataChangedEvent()
        {
        }
    }
}
