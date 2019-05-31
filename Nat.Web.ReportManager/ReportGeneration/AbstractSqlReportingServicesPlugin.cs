using System;
using System.Collections.Generic;
using System.Web;
using System.Web.UI;
using Nat.Controls.DataGridViewTools;
using Nat.ReportManager.ReportGeneration;
using Nat.ReportManager.ReportGeneration.SqlReportingServices;
using Nat.Tools.Filtering;
using Nat.Web.ReportManager.CustomExport;
using Nat.Web.ReportManager.UserControls;
using Nat.Web.Tools;

namespace Nat.Web.ReportManager.ReportGeneration
{
    public abstract class AbstractSqlReportingServicesPlugin<T> : BaseSqlReportingServicesPlugin<T>, IWebReportPlugin, IRedirectReportPlugin where T : Control, new()
    {
        private readonly string _description;
        private readonly int _position = 0;
        private Dictionary<string, object> _constants;

        protected AbstractSqlReportingServicesPlugin(string description)
        {
            _description = description;
        }

        protected AbstractSqlReportingServicesPlugin(string description, int position)
        {
            _description = description;
            _position = position;
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
            get { return null; }
        }

        public virtual CustomExportType CustomExportType
        {
            get { return CustomExportType.None; }
        }

        #endregion


        public static string GetReportUrlToWord(Type type, string idrec, bool isKz)
        {
            return GetReportUrl(type, idrec, "WORD", isKz);
        }

        public static string GetReportUrlToExcel(Type type, string idrec, bool isKz)
        {
            return GetReportUrl(type, idrec, "EXCEL", isKz);
        }

        public static string GetReportUrlToPdf(Type type, string idrec, bool isKz)
        {
            return GetReportUrl(type, idrec, "PDF", isKz);
        }

        public static string GetReportUrl(Type type, string idrec, string backText, string backUrl, bool isKz)
        {
            return string.Format(@"{0}?idrec={1}&ClassName={2}&culture={5}&text={3}&backPath={4}",
                                 ReportInitializerSection.GetReportInitializerSection().ReportPageViewer,
                                 idrec, type.FullName, backText, backUrl, isKz ? "kz" : "ru");
        }

        public static string GetReportUrl(Type type, string idrec, string exportFormat, bool isKz)
        {
            return string.Format(@"{0}?idrec={1}&ClassName={2}&culture={4}&rs:format={3}&rs:command=Render",
                                     ReportInitializerSection.GetReportInitializerSection().ReportPageViewer,
                                     idrec, type.FullName, exportFormat, isKz ? "kz" : "ru");
        }

        public bool LogViewReport
        {
            get { return false; }
        }

        public virtual string RedirectUrl
        {
            get { return ReportInitializerSection.GetReportInitializerSection().ReportingServicesPageViewer; }
        }

        public string GetReportUrl(string sessionGuid, string culture)
        {
            throw new NotImplementedException();
        }

        public void OpenReport(WebReportManager webReportManager, StorageValues storageValues, string format, string culture, string backPath, string textOfBackPath, string command)
        {
            var guid = ReportManagerControl.SetSession(storageValues, webReportManager);
            HttpContext.Current.Response.Redirect(
                string.Format(
                    "{7}?reportName={0}&rs:command={4}&rs:format={5}&values={1}&text={2}&culture={6}&backPath={3}",
                    GetType().FullName, guid, textOfBackPath, backPath, command, format, culture, this.RedirectUrl));
        }
    }
}