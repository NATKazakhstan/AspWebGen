using System;
using System.Linq;
using System.Web.UI;

using Nat.Web.Controls.Data;
using Nat.Web.Tools.Initialization;

namespace Nat.Web.Controls.GenerationClasses.Filter
{
    using Nat.Web.Tools.Security;

    public partial class SavedUserFilterValues : UserControl
    {
        public string TableName { get; set; }
        public string FilterControlName { get; set; }
        public bool ValuesInFilterIsObjects { get; set; }

        public void FillFilterList()
        {
            var db = GetDB();
            var data = db.SYS_GetUserFilters(TableName, GetSid()).
                GetResult<SYS_GetUserFiltersResult>();
            ddlFilter.DataSource = data;
            ddlFilter.Items.Clear();
            ddlFilter.DataBind();
        }

        #region event handlers

        protected void saveSettings_Click(object sender, EventArgs e)
        {
            Page.Validate("FilterName");
            if (!Page.IsValid) return;
            var value = GetSelectedValue();
            var db = GetDB();
            var result = db.SYS_SetUserFilters(value, TableName, GetSid(), tbName.Text, hvSettings.Value).First();
            FillFilterList();
            ddlFilter.SelectedValue = result.refUserFilterValues;
        }

        protected void deleteSettings_Click(object sender, ImageClickEventArgs e)
        {
            var value = GetSelectedValue();
            if (value != null)
            {
                var db = GetDB();
                db.SYS_DeleteUserFilter(value);
            }
            FillFilterList();
        }

        protected void setAsDefultFilter_Click(object sender, ImageClickEventArgs e)
        {
            Page.Validate("ddlFilterName");
            if (!Page.IsValid) return;
            var value = GetSelectedValue();
            var db = GetDB();
            db.SYS_SetDefaultUserFilter(value, TableName, GetSid());
        }

        #endregion

        #region override

        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);
            saveSettingsIB.ImageUrl = Themes.IconUrlSaveSettings;
            deleteSettings.ImageUrl = Themes.IconUrlDelete;
            setAsDefultFilter.ImageUrl = Themes.IconUrlDefaultFilter;
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            if (!IsPostBack)
            {
                FillFilterList();
            }
        }

        protected override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);
            if (ddlFilter.Items.Count <= 1)
                FillFilterList();
            var value = GetSelectedValue();
            if (value != null && ddlFilter.SelectedValue == null)
            {
                ddlFilter.SelectedValue = value;
                tbName.Text = ddlFilter.SelectedText;
            }
            if (ddlFilter.SelectedValue == null)
                deleteSettings.Style["display"] = "none";
        }

        #endregion

        private long? GetSelectedValue()
        {
            if (ValuesInFilterIsObjects)
            {
                var filterValues = MainPageUrlBuilder.Current.GetFilterItemsDic(TableName);
                if (!string.IsNullOrEmpty(Page.Request.Form[ddlFilter.UniqueID]))
                    return Convert.ToInt64(Page.Request.Form[ddlFilter.UniqueID]);
                if (filterValues != null && filterValues.ContainsKey("__refUserFilterValues")
                    && filterValues["__refUserFilterValues"].Count > 0
                    && !string.IsNullOrEmpty(filterValues["__refUserFilterValues"][0].Value1))
                {
                    return Convert.ToInt64(filterValues["__refUserFilterValues"][0].Value1);
                }
                return null;
            }
            else
            {
                var filterValues = MainPageUrlBuilder.Current.GetFilterDic(TableName);
                if (!string.IsNullOrEmpty(Page.Request.Form[ddlFilter.UniqueID]))
                    return Convert.ToInt64(Page.Request.Form[ddlFilter.UniqueID]);
                if (filterValues != null && filterValues.ContainsKey("__refUserFilterValues")
                    && !string.IsNullOrEmpty(filterValues["__refUserFilterValues"][0]))
                {
                    return Convert.ToInt64(filterValues["__refUserFilterValues"][0]);
                }
                return null;
            }
        }
        
        public string GetDefaultFilter()
        {
            var db = GetDB();
            return db.SYS_GetDefaultUserFilter(TableName, GetSid()).Select(r => r.FilterValues).FirstOrDefault();
        }

        private static DBFilterValuesDataContext GetDB()
        {
            if (WebSpecificInstances.DbFactory == null)
                WebInitializer.Initialize();
            return new DBFilterValuesDataContext(WebSpecificInstances.DbFactory.CreateConnection());
        }

        private static string GetSid()
        {
            return User.GetSID();
        }
    }
}