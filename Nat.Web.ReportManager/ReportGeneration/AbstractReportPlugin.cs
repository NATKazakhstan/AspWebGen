using System;
using System.Collections.Generic;
using System.Drawing;
using System.Web.UI;
using Nat.Controls.DataGridViewTools;
using Nat.ReportManager.ReportGeneration;
using Nat.ReportManager.ReportGeneration.StimulSoft;
using Nat.Tools.Filtering;
using Nat.Web.ReportManager.CustomExport;

namespace Nat.Web.ReportManager.ReportGeneration
{
    using Nat.ReportManager.QueryGeneration;

    public abstract class AbstractReportPlugin<T> : BaseStimulsoftReportPlugin<T>, IWebReportPlugin where T : Control, new()
    {
        private readonly string _description;
        private readonly int _position = 0;
        private List<List<BaseReportCondition>> _circleFillConditions;
        private Dictionary<string, object> _constants;
        private string _defaultValue;
        private Page _page;

        protected AbstractReportPlugin(string description)
        {
            _description = description;
        }

        protected AbstractReportPlugin(string description, int position)
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
        public override List<List<BaseReportCondition>> CircleFillConditions
        {
            get { return _circleFillConditions; }
            set { _circleFillConditions = value; }
        }

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
        public string DefaultValue
        {
            get { return _defaultValue; }
            set { _defaultValue = value; }
        }

        /// <summary>
        /// Страница на которой генерируется отчет.
        /// </summary>
        public virtual Page Page
        {
            get { return _page; }
            set { _page = value; }
        }

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

        public static string GetReportUrl(Type pluginType, string idrec, string backText, string backUrl, bool export, bool isKz)
        {
            if (export)
                return string.Format(@"{0}?expword=1&idrec={1}&ClassName={2}&text={3}&culture={5}&backPath={4}",
                                     ReportInitializerSection.GetReportInitializerSection().ReportPageViewer,
                                     idrec, pluginType.FullName, backText, backUrl, isKz ? "kz" : "ru");

            return string.Format(@"{0}?idrec={1}&ClassName={2}&text={3}&culture={5}&backPath={4}",
                                 ReportInitializerSection.GetReportInitializerSection().ReportPageViewer,
                                 idrec, pluginType.FullName, backText, backUrl, isKz ? "kz" : "ru");
        }

        protected BaseReportCondition GetCultureParameterIsKz(bool emptyCondition)
        {
            return new ReportCondition(CultureParameter, ColumnFilterType.Equal, CultureParameter, typeof(bool), InitializedKzCulture, null)
                {
                    Visible = false,
                    EmptyCondition = emptyCondition
                };
        }
    }
}