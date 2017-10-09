using System;
using System.ComponentModel;
using System.Data;
using System.Reflection;
using System.Web.UI;
using System.Web.UI.WebControls;
using Nat.Tools;
using Nat.Tools.Data;
using Nat.Web.Tools;

namespace Nat.Web.Controls
{
    [DisplayName("Table")]
    [Description("TableDataSource")]
    public class TableDataSource : ObjectDataSource, ITableSource
    {
        private const string DefaultViewName = "DefaultView";

        private static readonly FieldInfo _viewFieldInfo =
            typeof (ObjectDataSource).GetField("_view", BindingFlags.NonPublic | BindingFlags.Instance);

        private readonly TableDataSourceView _view;

        public TableDataSource()
        {
            _view = new TableDataSourceView(this, DefaultViewName, base.Context);
            _view.FillType = TableDataSourceFillType.ParametersNotChanged;
            _viewFieldInfo.SetValue(this, _view);
            if (IsTrackingViewState)
                ((IStateManager) _view).TrackViewState();
        }

        /// <summary>
        /// Тип заполнения данными.
        /// </summary>
        [Browsable(true)]
        [Category("Behavior")]
        [DefaultValue(TableDataSourceFillType.ParametersNotChanged)]
        public TableDataSourceFillType FillType
        {
            get { return _view.FillType; }
            set { _view.FillType = value; }
        }

        [IDReferenceProperty(typeof (SessionWorkerControl))]
        [Category("Behavior")]
        [DefaultValue("")]
        public string SessionWorkerControl
        {
            get { return _view.SessionWorkerControl; }
            set { _view.SessionWorkerControl = value; }
        }

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public SessionWorker SessionWorker
        {
            get { return _view.SessionWorker; }
            set { _view.SessionWorker = value; }
        }

        [DefaultValue("NotUse")]
        public new string UpdateMethod
        {
            get { return base.UpdateMethod; }
            set { _view.UpdateMethod = value; }
        }

        [DefaultValue("NotUse")]
        public new string DeleteMethod
        {
            get { return base.DeleteMethod; }
            set { _view.DeleteMethod = value; }
        }

        [DefaultValue("NotUse")]
        public new string InsertMethod
        {
            get { return base.InsertMethod; }
            set { _view.InsertMethod = value; }
        }

        [Browsable(false)]
        [DefaultValue("{0}")]
        public new string OldValuesParameterFormatString
        {
            get { return _view.OldValuesParameterFormatString; }
            set { _view.OldValuesParameterFormatString = value; }
        }

        [Browsable(false)]
        [DefaultValue(-1)]
        public int CurrentIndex
        {
            get { return _view.CurrentIndex; }
            set { _view.CurrentIndex = value; }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Browsable(false)]
        public DataRowView Current
        {
            get
            {
                if (DesignMode) return _view.CurrentDesign;
                return _view.Current;
            }
        }

        [DefaultValue(true)]
        [Category("Behavior")]
        public bool SetFilterByCustomConditions
        {
            get { return _view.SetFilterByCustomConditions; }
            set { _view.SetFilterByCustomConditions = value; }
        }

        [Category("Behavior")]
        [DefaultValue(false)]
        public bool ShowHistoricalData
        {
            get { return _view.ShowHistoricalData; }
            set { _view.ShowHistoricalData = value; }
        }

        [Category("HistoricalData")]
        [DefaultValue("dateEnd")]
        public string EndDateField
        {
            get { return _view.EndDateField; }
            set { _view.EndDateField = value; }
        }

        [Category("HistoricalData")]
        [DefaultValue("dateStart")]
        public string StartDateField
        {
            get { return _view.StartDateField; }
            set { _view.StartDateField = value; }
        }

        [Category("HistoricalData")]
        [DefaultValue(null)]
        public DateTime? HistolicalPoint
        {
            get { return _view.HistolicalPoint; }
            set { _view.HistolicalPoint = value; }
        }

        [Category("HistoricalData")]
        [DefaultValue(false)]
        [Description("Загружать все данные таблицы включая ушедшие в историю данные, не действует, при EnablePaging")]
        public bool LoadAllHistoricalData
        {
            get { return _view.LoadAllHistoricalData; }
            set { _view.LoadAllHistoricalData = value; }
        }

        [DefaultValue("id")]
        public string DefaultOrderBy
        {
            get { return _view.DefaultOrderBy; }
            set { _view.DefaultOrderBy = value; }
        }

        public new bool EnablePaging
        {
            get { return _view.EnablePaging; }
            set { _view.EnablePaging = value; }
        }

        [DefaultValue("")]
        [Category("Behavior")]
        public string SortExpression
        {
            get { return _view.SortExpression; }
            set { _view.SortExpression = value; }
        }

        /// <summary>
        /// Режим возврата данных.
        /// </summary>
        [DefaultValue(TableDataSourceDataMode.All)]
        [Category("Behavior")]
        public TableDataSourceDataMode DataMode
        {
            get { return _view.DataMode; }
            set { _view.DataMode = value; }
        }

        [DefaultValue(true)]
        [Category("Behavior")]
        public bool ClearBeforeFill
        {
            get { return _view.ClearBeforeFill; }
            set { _view.ClearBeforeFill = value; }
        }


        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public TableDataSourceView View
        {
            get { return _view; }
        }

        /// <summary>
        /// Имя метода для получения устаревших данных.
        /// </summary>
        [DefaultValue("GetData")]
        [Description("Имя метода для получения устаревших данных")]
        public string HistoricalSelectMethod
        {
            get { return _view.HistoricalSelectMethod; }
            set { _view.HistoricalSelectMethod = value; }
        }

        /// <summary>
        /// Ключевое поле таблицы.
        /// </summary>
        [DefaultValue("id")]
        [Description("Ключевое поле таблицы")]
        public string HistoricalKey
        {
            get { return _view.HistoricalKey; }
            set { _view.HistoricalKey = value; }
        }

        /// <summary>
        /// Значения ключей для получения устаревших данных.
        /// </summary>
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public object[] HistoricalValues
        {
            get { return _view.HistoricalValues; }
            set { _view.HistoricalValues = value; }
        }

        /// <summary>
        /// Количество данных прогружаемых по истории.
        /// </summary>
        [Description("Количество данных прогружаемых по истории")]
        [Category("HistoricalData")]
        [DefaultValue(1)]
        public int HistoricalCountKeys
        {
            get { return _view.HistoricalCountKeys; }
            set { _view.HistoricalCountKeys = value; }
        }

        #region ITableSource Members

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public DataTable Table
        {
            get { return _view.Table; }
        }

        #endregion

        public event EventHandler<CurrentChangeEventArgs> CurrentChanged
        {
            add { _view.CurrentChanged += value; }
            remove { _view.CurrentChanged -= value; }
        }

        public event EventHandler<CurrentChangingEventArgs> CurrentChanging
        {
            add { _view.CurrentChanging += value; }
            remove { _view.CurrentChanging -= value; }
        }

        /// <summary>
        /// Добавить строку в данные.
        /// </summary>
        /// <returns>Возвращается добавленная строка, если не добавлена null.</returns>
        /// <remarks>Новую строку добавить нельзя пока не завершена/отменена работа с уже добавленой строкой.</remarks>
        public DataRow AddNewRow()
        {
            return _view.AddNewRow();
        }

        public bool SetCurrent(DataRow row)
        {
            return _view.SetCurrent(row);
        }

        /// <summary>
        /// Комит изменений.
        /// </summary>
        /// <returns></returns>
        public int UpdateData()
        {
            return _view.UpdateData();
        }

        /// <summary>
        /// Заполнение данных не смотря на запрет от FillType
        /// </summary>
        public void Select(bool anyWay)
        {
            _view.Select(anyWay, DataSourceSelectArguments.Empty);
        }

        public event TableDataSourceUpdatedDataEventHandler UpdatedData
        {
            add { _view.UpdatedData += value; }
            remove { _view.UpdatedData -= value; }
        }

        public event TableDataSourceUpdatingDataEventHandler UpdatingData
        {
            add { _view.UpdatingData += value; }
            remove { _view.UpdatingData -= value; }
        }

        public event TableDataSourceClearingDataEventHandler ClearingData
        {
            add { _view.ClearingData += value; }
            remove { _view.ClearingData -= value; }
        }

        public event TableDataSourceClearedDataEventHandler ClearedData
        {
            add { _view.ClearedData += value; }
            remove { _view.ClearedData -= value; }
        }

        public event TableDataSourceErrorEventHandler Error
        {
            add { _view.Error += value; }
            remove { _view.Error -= value; }
        }

        public event TableDataSourceAddedRowEventHandler AddedRow
        {
            add { _view.AddedRow += value; }
            remove { _view.AddedRow -= value; }
        }

        public event TableDataSourceAddingRowEventHandler AddingRow
        {
            add { _view.AddingRow += value; }
            remove { _view.AddingRow -= value; }
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            _view.FillType = FillType;
        }

        protected override void OnUnload(EventArgs e)
        {
            base.OnUnload(e);
            _view.Dispose();
        }
    }
}