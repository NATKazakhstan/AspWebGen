using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Web;
using System.Web.UI;
using Nat.ReportManager.QueryGeneration;
using Nat.ReportManager.ReportGeneration;
using Nat.Tools.Filtering;
using Nat.Web.ReportManager.CustomExport;
using Nat.Web.ReportManager.UserControls;
using Nat.Web.Tools;

namespace Nat.Web.ReportManager.ReportGeneration
{
    using Nat.Web.Controls;

    public abstract class CrossJournalReportPlugin : CrossJournalReportPlugin<Control>
    {
        protected CrossJournalReportPlugin(string description) : base(description)
        {
        }

        protected CrossJournalReportPlugin(string description, int position) : base(description, position)
        {
        }
    }

    public abstract class CrossJournalReportPlugin<T> : BaseReportPlugin<T>, IWebReportPlugin, IRedirectReportPlugin where T : Control, new()
    {
        private readonly string _description;
        private readonly int _position;
        private Dictionary<string, object> _constants;

        protected CrossJournalReportPlugin(string description)
        {
            _description = description;
        }

        protected CrossJournalReportPlugin(string description, int position)
        {
            _description = description;
            _position = position;
        }

        public override ViewType ViewType
        {
            get { return ViewType.DefaultView; }
        }

        public override void Clear()
        {
            foreach (DataTable table in Table.DataSet.Tables)
                table.Rows.Clear();
        }

        public override void Fill()
        {
        }

        public override void ShowReportPlugin(IReportManager reportManager)
        {
        }

        public override void InitReport(StringDictionary selectedTexts, StringDictionary selectedFilterTexts, Dictionary<string, DataRow[]> selectedRows) { }

        public override void PostActions() { }

        public override void PreActions() { }

        public override void ShowCustomReport()
        {
        }

        public override List<AdapterToTable> ListAdapterToTable
        {
            get
            {
                return new List<AdapterToTable>(0);
            }
        }

        public override DataTable Table
        {
            get { return null; }
        }

        public override Component TableAdapter
        {
            get { return null; }
        }

        protected virtual string GetAdditionalUrlParameters()
        {
            return string.Empty;
        }

        #region IWebReportPlugin Members

        public override void ReportCompile()
        {
        }

        /// <summary>
        /// Образец условия. Неоднократное заполнение таблиц по списку условий. Условия содержат список значений.
        /// </summary>
        public override List<BaseReportCondition> CreateModelFillConditions()
        {
            return new List<BaseReportCondition>();
        }

        /// <summary>
        /// Неоднократное заполнение таблиц по списку условий. 
        /// Первый список это количество заполнений, вложеный список уловия для заполнения.
        /// </summary>
        public override List<List<BaseReportCondition>> CircleFillConditions { get; set; }

        public override string Description
        {
            get { return _description; }
        }

        public override int Position
        {
            get { return _position; }
        }

        /// <summary>
        /// Права доступа.
        /// </summary>
        /// <returns>Список ролей, которым доступен отчет.</returns>
        public abstract string[] Roles();

        /// <summary>
        /// Роли доступа экспорта
        /// </summary>
        public virtual string[] ExportRoles => new string[0];

        /// <summary>
        /// Выбранное значение по умолчанию. 
        /// К примеру, отчет для персоны и переход на отчеты из персон, 
        /// то поумолчанию считается id персоны
        /// </summary>
        public string DefaultValue { get; set; }

        /// <summary>
        /// Страница на которой генерируется отчет.
        /// </summary>
        public virtual Page Page { get; set; }

        protected virtual bool AddFilterTypeInConstants
        {
            get
            {
                return false;
            }
        }

        public Dictionary<string, object> Constants
        {
            get
            {
                if (_constants == null)
                    return _constants = WebReportManager.CreateConstants(this.Conditions, this.CircleFillConditions, AddFilterTypeInConstants);
                return _constants;
            }
            set { _constants = value; }
        }

        public virtual bool InitSavedValuesInvisibleConditions
        {
            get { return false; }
        }

        public virtual bool AllowSaveValuesConditions
        {
            get { return true; }
        }

        public string ImageUrl
        {
            get { return Themes.IconUrlCrossJournalReport; }
        }

        public override bool SupportRuKz
        {
            get { return true; }
        }

        public abstract string JournalName { get; }
        
        public virtual CustomExportType CustomExportType
        {
            get { return CustomExportType.None; }
        }

        #endregion

        #region IRedirectReportPlugin

        public virtual bool LogViewReport
        {
            get { return true; }
        }

        public virtual string RedirectUrl
        {
            get { return "/MainPage.aspx/data/" + JournalName; }
        }

        public virtual void OpenReport(WebReportManager webReportManager, StorageValues storageValues, string format, string culture, string backPath, string textOfBackPath, string command)
        {
            var guid = ReportManagerControl.SetSession(storageValues, webReportManager);
            HttpContext.Current.Response.Redirect(GetReportUrl(guid, culture));
        }

        public virtual string GetReportUrl(string sessionGuid, string culture)
        {
            if (culture == "ru")
                culture = "ru-ru";
            else if (culture == "kz")
                culture = "kk-kz";

            var redirectUrl = RedirectUrl;
            return string.Format(
                "{0}{1}culture={2}&storageValues={3}&reportPluginName={4}{5}",
                redirectUrl,
                redirectUrl.Contains("?") ? "&" : "?",
                culture,
                sessionGuid,
                GetType().FullName,
                GetAdditionalUrlParameters());
        }

        #endregion
    }
}
