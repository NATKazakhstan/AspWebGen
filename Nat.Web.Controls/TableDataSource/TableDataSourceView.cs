#define NULL_REFERENCE

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Data;
using System.Data.Common;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.Serialization.Formatters.Binary;
using System.Web;
using System.Web.Compilation;
using System.Web.UI;
using System.Web.UI.WebControls;
using Nat.Tools;
using Nat.Tools.Data;
using Nat.Tools.Data.Transactions;
using Nat.Tools.Filtering;
using Nat.Tools.QueryGeneration;
using Nat.Tools.Specific;
using Nat.Web.Tools;
using Nat.Web.Tools.Initialization;

namespace Nat.Web.Controls
{
    public class TableDataSourceView : ObjectDataSourceView, IDisposable, ICurrentProvider
    {
        #region reflection

        private static readonly MethodInfo reflectionBuildObjectValue =
            typeof (ObjectDataSourceView).GetMethod("BuildObjectValue",
                                                    BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);

        #endregion

        #region поля

        private readonly HttpContext _context;
        private readonly TableDataSource _owner;
        private readonly QueryConditionList customConditions = new QueryConditionList();
        private DataSourceSelectArguments _arguments;
        private bool _clearBeforeFill = true;
        private int _countRows;
        private string _defaultOrderBy = "id";
        private bool _enablePaging;
        private DataSourceSelectExtArguments _extArguments;
        private string _historicalKey = "id";
        private string _historicalSelectMethod = "GetData";
        private object[] _historicalValues = new object[1];
        private bool _setFilterByCustomConditions = true;
        private bool _showHistoricalData;
        private string _sortExpression = "";
        private DataTable _table;
        private string _tableName = "";
        private DataView _view;
        private bool beginLoad;
        private int currentIndex = -1;
        private object[] currentKeys;
        private TableDataSourceDataMode dataMode = TableDataSourceDataMode.All;
        private bool doSelect = true;
        private object[] editingItem;
        private string endDateField = "dateEnd";
        private bool fillData;
        private TableDataSourceFillType fillType = TableDataSourceFillType.ParametersNotChanged;
        private string filter;
        private bool generateSessionWorker;
        private DateTime? histolicalPoint;
        private IDictionary inputParameters;
        private int startFromIndex;
        private string lastFilter;
        private bool isEditRow;
        private bool isExecuteSelect;
        private bool loadAllHistoricalData;
        private bool loadState;
        private bool needResetPosition;
        private object[] newKeys;
        private DataRowState rowState;
        private SessionWorker sessionWorker;
        private string sessionWorkerControl = "";
        private string sort;
        private string startDateField = "dateStart";

        #endregion

        public TableDataSourceView(TableDataSource owner, string name, HttpContext context)
            : base(owner, name, context)
        {
            _owner = owner;
            _context = context;
            base.OldValuesParameterFormatString = "{0}";
            base.UpdateMethod = "NotUse";
            base.DeleteMethod = "NotUse";
            base.InsertMethod = "NotUse";
        }


        //public TableDataSourceView(SessionWorker sessionWorker, bool showHistoricalData, bool loadAllHistoricalData, string endDateField, string startDateField, DateTime? histolicalPoint, QueryConditionList customConditions, string selectMethod, string typeName, QueryConditionList selectParameters, Dictionary<String, String> filterParameters, String filterExpression, Boolean setFilterByCustomConditions)
        //    : base(new ObjectDataSource(), "", null)
        //{
        //    this.sessionWorker = sessionWorker;
        //    this.showHistoricalData = showHistoricalData;
        //    this.loadAllHistoricalData = loadAllHistoricalData;
        //    this.endDateField = endDateField;
        //    this.startDateField = startDateField;
        //    this.histolicalPoint = histolicalPoint;
        //    this.customConditions = customConditions;
        //    this.SelectMethod = selectMethod;
        //    this.TypeName = typeName;
        //    foreach (QueryCondition parameter in selectParameters)
        //    {
        //        SelectParameters.Add(parameter.ColumnName, (parameter.Param1.Value ?? "").ToString());
        //    }
        //    foreach(KeyValuePair<string, string> pair in filterParameters)
        //    {
        //        Parameter parameter =  new Parameter(pair.Key);
        //        parameter.DefaultValue = pair.Value;
        //        FilterParameters.Add(parameter);
        //    }
        //    FilterExpression = filterExpression;
        //    SetFilterByCustomConditions = setFilterByCustomConditions;
        //}

        public TableDataSourceView(SessionWorker sessionWorker, bool showHistoricalData,
                                   bool loadAllHistoricalData, string endDateField,
                                   string startDateField, DateTime? histolicalPoint,
                                   QueryConditionList customConditions, string selectMethod,
                                   string typeName, List<Triplet> selectParameters,
                                   Dictionary<String, String> filterParameters,
                                   String filterExpression, Boolean setFilterByCustomConditions)
            : base(new ObjectDataSource(), "", null)
        {
            this.sessionWorker = sessionWorker;
            _showHistoricalData = showHistoricalData;
            this.loadAllHistoricalData = loadAllHistoricalData;
            this.endDateField = endDateField;
            this.startDateField = startDateField;
            this.histolicalPoint = histolicalPoint;
            this.customConditions = customConditions;
            SelectMethod = selectMethod;
            TypeName = typeName;

            foreach (Triplet selectParameter in selectParameters)
                SelectParameters.Add((String) selectParameter.First, (TypeCode) selectParameter.Second,
                                     (String) selectParameter.Third);


            foreach (var pair in filterParameters)
            {
                var parameter = new Parameter(pair.Key) {DefaultValue = pair.Value};
                FilterParameters.Add(parameter);
            }
            FilterExpression = filterExpression;
            SetFilterByCustomConditions = setFilterByCustomConditions;
        }

        #region IDisposable Members

        public void Dispose()
        {
            if (_view != null)
                _view.ListChanged -= View_OnListChanged;
        }

        #endregion

        #region public

        #region properties

        /// <summary>
        /// Тип заполнения.
        /// </summary>
        public TableDataSourceFillType FillType
        {
            get { return fillType; }
            set { fillType = value; }
        }

        public string SortExpression
        {
            get { return _sortExpression; }
            set { _sortExpression = value; }
        }

        public DataTable Table
        {
            get
            {
                EnsureDataTableCreated();
                return _table;
            }
            protected set
            {
                if (SessionWorker != null && value != null && (_table = GetDataTable(value.TableName)) != null)
                {
                    EnsureSetProperies();
                    TableName = value.TableName;
                    var clearing =
                        new TableDataSourceClearingDataEventArgs(_table, value);
                    bool stopedEnforceConstraint = false;
                    Exception exception = null;
                    try
                    {
                        if (_clearBeforeFill)
                        {
                            OnClearingData(clearing);
                            if (clearing.StopEnforceConstraints && _table.DataSet != null)
                            {
                                _table.DataSet.EnforceConstraints = false;
                                stopedEnforceConstraint = true;
                            }
                            if (!clearing.Cancel)
                            {
                                if (!clearing.CancelClearChild) clearing.ClearAllChildKeyConstraintTables();
                                _table.Clear();
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        exception = e;
                    }
                    var args = new TableDataSourceClearedDataEventArgs(exception, _table);
                    OnClearedData(args);
                    if (exception != null && args.ThrowException) throw exception;
                    _table.Merge(value);
                    if (stopedEnforceConstraint && clearing.StopEnforceConstraints && _table.DataSet != null)
                        _table.DataSet.EnforceConstraints = true;
                    ResetCurrentPosition();
                    return;
                }
                _table = value;
                ResetCurrentPosition();
            }
        }

        public DataView DataView
        {
            get
            {
                EnsureDataViewCreated();
                return _view;
            }
        }

        private string TableName
        {
            get
            {
                if (string.IsNullOrEmpty(_tableName))
                {
                    Type tTableAdapter = GetType(TypeName);
                    Type tTable = TableAdapterTools.GetAdapterTableType(tTableAdapter);
                    _tableName = ((DataTable) Activator.CreateInstance(tTable)).TableName;
                }
                return _tableName;
            }
            set { _tableName = value; }
        }

        [IDReferenceProperty(typeof (SessionWorkerControl))]
        [Category("Behavior")]
        [DefaultValue("")]
        public string SessionWorkerControl
        {
            get { return sessionWorkerControl; }
            set
            {
                sessionWorkerControl = value;
                generateSessionWorker = true;
            }
        }

        public SessionWorker SessionWorker
        {
            get
            {
                if (!generateSessionWorker) return sessionWorker;
                generateSessionWorker = false;
                Control control = FindSessionWorkerControl(_owner.Parent);
                var swc = control as SessionWorkerControl;
                if (swc != null) return sessionWorker = swc.SessionWorker;
                return sessionWorker;
            }
            set { sessionWorker = value; }
        }

        public bool SetFilterByCustomConditions
        {
            get { return _setFilterByCustomConditions; }
            set { _setFilterByCustomConditions = value; }
        }

        /// <summary>
        /// Текущая строка новая.
        /// </summary>
        public bool HasNewRow
        {
            get { return newKeys != null; }
        }

        /// <summary>
        /// Текущая строка редактируется.
        /// </summary>
        public bool IsEditRow
        {
            get { return isEditRow; }
        }

        /// <summary>
        /// Текущий индекс.
        /// </summary>
        public virtual int CurrentIndex
        {
            get { return currentIndex; }
            set
            {
                if (currentIndex != value)
                {
                    EnsureSetProperies();
                    if (HasNewRow) throw new HasNewRowException("Позиция не может меняться, при HasNewRow");
                    if (IsEditRow) EndEdit();
                    if (value < -1) throw new IndexOutOfRangeException("CurrentIndex can not be less -1");
                    if (value >= DataView.Count)
                    {
                        throw new IndexOutOfRangeException("CurrentIndex can not be more " +
                                                           (DataView.Count - 1));
                    }

                    var args = new CurrentChangingEventArgs(null, null, value, currentIndex);
                    OnCurrentChanging(args);
                    if (!args.Cancel)
                    {
                        if (value < 0) currentKeys = null;
                        else
                            currentKeys = GetKeys(DataView[value].Row);
                        int oldIndex = currentIndex;
                        currentIndex = value;
                        OnCurrentChanged(new CurrentChangeEventArgs(null, null, value, oldIndex));
                    }
                }
            }
        }

        /// <summary>
        /// Текущая строка.
        /// </summary>
        public DataRowView Current
        {
            get
            {
                if (CurrentIndex < 0) return null;
//                Debug.Assert(CurrentIndex < DataView.Count, "CurrentIndex >= DataView.Count");
                return DataView[CurrentIndex];
            }
        }

        internal DataRowView CurrentDesign
        {
            get
            {
                if (_table == null)
                {
                    Type tTableAdapter = GetType(TypeName);
                    Type tTable = TableAdapterTools.GetAdapterTableType(tTableAdapter);
                    _table = (DataTable) Activator.CreateInstance(tTable);
                }
                if (_table.DefaultView.Count < 1)
                    _table.DefaultView.AddNew();
                return _table.DefaultView[0];
            }
        }

        /// <summary>
        /// Режим возврата данных.
        /// </summary>
        public TableDataSourceDataMode DataMode
        {
            get { return dataMode; }
            set { dataMode = value; }
        }

        public bool ShowHistoricalData
        {
            get { return _showHistoricalData; }
            set { _showHistoricalData = value; }
        }

        public bool LoadAllHistoricalData
        {
            get { return loadAllHistoricalData; }
            set { loadAllHistoricalData = value; }
        }

        public string EndDateField
        {
            get { return endDateField; }
            set { endDateField = value; }
        }

        public string StartDateField
        {
            get { return startDateField; }
            set { startDateField = value; }
        }

        public DateTime? HistolicalPoint
        {
            get { return histolicalPoint; }
            set { histolicalPoint = value; }
        }

        /// <summary>
        /// Имя метода для получения устаревших данных.
        /// </summary>
        public string HistoricalSelectMethod
        {
            get { return _historicalSelectMethod; }
            set { _historicalSelectMethod = value; }
        }

        /// <summary>
        /// Ключевое поле таблицы.
        /// </summary>
        public string HistoricalKey
        {
            get { return _historicalKey; }
            set { _historicalKey = value; }
        }

        /// <summary>
        /// Значения ключей для получения устаревших данных.
        /// </summary>
        public object[] HistoricalValues
        {
            get { return _historicalValues; }
            set { _historicalValues = value; }
        }

        /// <summary>
        /// Количество данных прогружаемых по истории.
        /// </summary>
        public int HistoricalCountKeys
        {
            get { return HistoricalValues == null ? 0 : HistoricalValues.Length; }
            set
            {
                if (value != HistoricalCountKeys)
                {
                    HistoricalValues = new object[value];
                }
            }
        }

        /// <summary>
        /// Собственные условия.
        /// </summary>
        /// <remarks>Свойство не серелизуется.</remarks>
        public QueryConditionList CustomConditions
        {
            get { return customConditions; }
        }

        [DefaultValue("id")]
        public string DefaultOrderBy
        {
            get { return _defaultOrderBy; }
            set { _defaultOrderBy = value; }
        }

        public new bool EnablePaging
        {
            get { return _enablePaging; }
            set { _enablePaging = value; }
        }

        public bool ClearBeforeFill
        {
            get { return _clearBeforeFill; }
            set { _clearBeforeFill = value; }
        }

        object ICurrentProvider.Current
        {
            get { return Current; }
        }

        bool ICurrentProvider.SupportCurrentChanging
        {
            get { return true; }
        }

        private void EnsureDataViewCreated()
        {
            EnsureDataTableCreated();
            if (_view == null || _view.Table != Table)
            {
                if (_view != null)
                {
                    _view.ListChanged -= View_OnListChanged;
                    sort = _view.Sort;
                    filter = _view.RowFilter;
                }
                _view = new DataView(Table);
                _view.ListChanged += View_OnListChanged;
                EnsureSetProperies();
                ResetCurrentPosition();
            }
        }

        private void EnsureDataTableCreated()
        {
            if (_table == null && SessionWorker != null && !string.IsNullOrEmpty(TableName))
            {
                _table = GetDataTable(TableName);
                if(_table != null)
                    EnsureSetProperies();
            }
        }

        public void ResetCurrentKeys()
        {
            if (Current != null) currentKeys = GetKeys(Current.Row);
            else currentKeys = null;
        }

        #endregion

        #region methods

        /// <summary>
        /// Выставить текущей строчку.
        /// </summary>
        /// <param name="row">Строка.</param>
        /// <returns>Выставленный индекс.</returns>
        public int SetCurrentRow(DataRow row)
        {
            int index = FindHelper.Find(row, DataView, CurrentIndex);
            CurrentIndex = index;
            return index;
        }

        /// <summary>
        /// Добавить новую строку в данные.
        /// </summary>
        /// <returns>Возвращается добавленная строка, если не добавлена null.</returns>
        /// <remarks>Новую строку добавить нельзя пока не завершена/отменена работа с уже добавленой строкой.</remarks>
        public DataRow AddNewRow()
        {
            if (HasNewRow)
                throw new HasNewRowException("Новая строка уже добавлена, необходимо завершить редактирование");
            if (IsEditRow) EndEdit();
            if (Table != null)
            {
                DataRow row = Table.NewRow();
                try
                {
                    SetValues(row, InsertParameters.GetValues(_context, _owner));
                    var args = new TableDataSourceAddingRowEventArgs(row);
                    OnAddingNewRow(args);
                    if (args.Cancel) return null;
                    Table.Rows.Add(row);
                    row.EndEdit();
                    rowState = DataRowState.Added;
                    editingItem = (object[]) deepCopy(row.ItemArray);
                }
                catch (Exception e)
                {
                    newKeys = null;
                    if (row.RowState != DataRowState.Detached)
                        Table.Rows.Remove(row);
                    TableDataSourceErrorEventArgs eventArgs;
                    eventArgs = new TableDataSourceErrorEventArgs(e, EventArgs.Empty, false);
                    OnError(eventArgs);
                    if (eventArgs.ThrowException) throw;
                    return null;
                }
                int index = FindHelper.Find(row, DataView, CurrentIndex);
                CurrentIndex = index;
                if (CurrentIndex != index)
                {
                    Table.Rows.Remove(row);
                    return null;
                }
                newKeys = GetKeys(row);
                OnAddedNewRow(new TableDataSourceAddedRowEventArgs(row));
                newKeys = GetKeys(row);
                currentKeys = newKeys;
                ResetCurrentPosition();
                return row;
            }
            return null;
        }

        /// <summary>
        /// Комит изменений.
        /// </summary>
        /// <returns></returns>
        public int UpdateData()
        {
            var args = new TableDataSourceUpdatingDataEventArgs(Table);
            OnUpdatingData(args);
            if (args.Cancel) return 0;
            EndEdit();
            DataRow curRow = null;
            if (Current != null) curRow = Current.Row;

            TransactionManager executeTransaction;
            DbConnection connection = null;
            DbTransaction transaction = null;
            Exception exception = null;
            int rowsCount = 0;
            try
            {
                connection = SpecificInstances.DbFactory.CreateConnection();
                connection.Open();
                transaction = connection.BeginTransaction();
                executeTransaction = new TransactionManager(transaction);
                executeTransaction.Add(new TransactionItem(Table));
                rowsCount = executeTransaction.UpdateData();
                executeTransaction.Commit();
                transaction.Commit();
                if (curRow != null && curRow.RowState != DataRowState.Detached &&
                    curRow.RowState != DataRowState.Deleted)
                    currentKeys = GetKeys(curRow);
                else currentKeys = null;
                ResetCurrentPosition();
            }
            catch (Exception e)
            {
                if (transaction != null) transaction.Rollback();
                if (e is TargetInvocationException) exception = e.InnerException;
                else exception = e;
            }
            finally
            {
                if (connection != null) connection.Close();
            }
            var resArgs = new TableDataSourceUpdatedDataEventArgs(rowsCount, exception);
            OnUpdatedData(resArgs);
            if (exception != null && resArgs.ThrowException) throw exception;
            return rowsCount;
        }

        /// <summary>
        /// Редактировать текущую строку.
        /// </summary>
        public void BeginEdit()
        {
            if (CurrentIndex < 0) throw new Exception("CurrentIndex должен быть больше -1");
            if (IsEditRow) throw new Exception("Строка уже редактируется.");
            rowState = Current.Row.RowState;
            isEditRow = true;

            editingItem = (object[]) deepCopy(Current.Row.ItemArray);
            Current.Row.AcceptChanges();
        }

        /// <summary>
        /// Редактировать текущую строку.
        /// </summary>
        public void SetRowAsNew()
        {
            if (CurrentIndex < 0) throw new Exception("CurrentIndex должен быть больше -1");
            if (IsEditRow) throw new Exception("Строка уже редактируется.");
            rowState = Current.Row.RowState;
            newKeys = GetKeys(Current.Row);

            editingItem = (object[]) deepCopy(Current.Row.ItemArray);
            Current.Row.AcceptChanges();
        }

        /// <summary>
        /// Завершить редактирование текущей строки.
        /// </summary>
        public void EndEdit()
        {
            if (IsEditRow || HasNewRow)
            {
                isEditRow = false;
                newKeys = null;
                Current.Row.AcceptChanges();

                // Журнализация
                var baseSPPage = _owner.Parent.Page as BaseSPPage;
                if (baseSPPage != null)
                    baseSPPage.LogMonitor.RowChanged(_table, Current.Row.Table.Columns, editingItem,
                                                     Current.Row.ItemArray);

                editingItem = null;
                if (rowState == DataRowState.Added) Current.Row.SetAdded();
                else Current.Row.SetModified();
            }
        }

        /// <summary>
        /// Отмена редактирования текущей строки.
        /// </summary>
        public void CancelEdit()
        {
            if (IsEditRow)
            {
                isEditRow = false;
                DataRow row = Current.Row;
                row.BeginEdit();
                for (int i = 0; i < Table.Columns.Count; i++)
                    if (!Table.Columns[i].ReadOnly) row[i] = editingItem[i];
                editingItem = null;
                row.EndEdit();
                row.AcceptChanges();
                if (rowState == DataRowState.Added) row.SetAdded();
                else if (rowState == DataRowState.Modified) row.SetModified();
                ResetCurrentPosition();
            }
            if (newKeys != null)
            {
                DataRow row = Table.Rows.Find(newKeys);
                Table.Rows.Remove(row);
                newKeys = null;
                ResetCurrentPosition(CurrentIndex);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="anyWay">если true, то запрос к базе будет несмотря на FillType, иначе зависит от FillType</param>
        /// <param name="arguments">A System.Web.UI.DataSourceSelectArguments used to request operations on the data beyond basic data retrieval.</param>
        public IEnumerable Select(bool anyWay, DataSourceSelectArguments arguments)
        {
            fillData = anyWay;
            _arguments = arguments;
            try
            {
                return Select(arguments);
            }
            finally
            {
                _arguments = null;
                fillData = false;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="anyWay">если true, то запрос к базе будет несмотря на FillType, иначе зависит от FillType</param>
        /// <param name="arguments">A System.Web.UI.DataSourceSelectArguments used to request operations on the data beyond basic data retrieval.</param>
        /// <param name="extArguments">дополнительный аргумент для выборки</param>
        public IEnumerable Select(bool anyWay, DataSourceSelectArguments arguments,
                                  DataSourceSelectExtArguments extArguments)
        {
            fillData = anyWay;
            _arguments = arguments;
            try
            {
                _extArguments = extArguments;
                return Select(arguments);
            }
            finally
            {
                _extArguments = null;
                _arguments = null;
                fillData = false;
            }
        }

        public override void Select(DataSourceSelectArguments arguments, DataSourceViewSelectCallback callback)
        {
            _arguments = arguments;
            try
            {
                base.Select(arguments, callback);
            }
            finally
            {
                _arguments = null;
            }
        }

        public string GetFilterString()
        {
            /*
             * есть мысль сделать поддержку not, но тут что-то переполнение стека возникает.
            string compare = "=";
            if(rowFilter.EndsWith("not"))
            {
                compare = "<>";
                if (rowFilter.Length > 3)
                    rowFilter.Substring(0, rowFilter.Length - 3);
                else
                    rowFilter = "";
            }
            foreach (DictionaryEntry entry in dicFilters)
            {
                if (string.IsNullOrEmpty(rowFilter))
                    rowFilter = string.Format("({0}{2}{1})", entry.Key, entry.Value, compare);
                else
                    rowFilter += string.Format(" AND ({0}{2}{1})", entry.Key, entry.Value, compare);
            }

             */
            string rowFilter = FilterExpression;
            IOrderedDictionary dicFilters = FilterParameters.GetValues(_context, _owner);
            foreach (DictionaryEntry entry in dicFilters)
            {
                if (string.IsNullOrEmpty(rowFilter))
                    rowFilter = string.Format("({0}={1})", entry.Key, entry.Value);
                else
                    rowFilter += string.Format(" AND ({0}={1})", entry.Key, entry.Value);
            }
            if (ShowHistoricalData)
            {
                string historicalFilter = HistoricalData.GetHistoricalFilter(endDateField, startDateField,
                                                                             histolicalPoint);
                if (string.IsNullOrEmpty(rowFilter))
                    rowFilter = string.Format("{0}", historicalFilter);
                else
                    rowFilter += string.Format(" AND {0}", historicalFilter);
            }
            if (customConditions != null && customConditions.Count > 0 && SetFilterByCustomConditions)
            {
                string filterString = customConditions.NonSqlFilter;
                if (!string.IsNullOrEmpty(filterString))
                {
                    if (string.IsNullOrEmpty(rowFilter))
                        rowFilter = filterString;
                    else
                        rowFilter += " and " + filterString;
                }
            }
            if (ShowHistoricalData && HistoricalCountKeys > 0 && !string.IsNullOrEmpty(HistoricalKey))
            {
                string historicalOverFilter = "";
                foreach (object value in HistoricalValues)
                {
                    if (value != null && value != DBNull.Value && (value as string) != "")
                        historicalOverFilter += string.Format(" OR {0} = '{1}'", HistoricalKey, value);
                }
                if (!string.IsNullOrEmpty(historicalOverFilter))
                {
                    if (!string.IsNullOrEmpty(rowFilter))
                        rowFilter += string.Format("{0}", historicalOverFilter);
                    else
                        rowFilter = string.Format("{0}", historicalOverFilter.Substring(4));
                }
            }
            return rowFilter;
        }

        public void BeginLoad()
        {
            if (beginLoad) throw new Exception("BeginLoad уже вызван");
            beginLoad = true;
        }

        public void EndLoad()
        {
            if (!beginLoad) throw new Exception("не вызван BeginLoad");
            beginLoad = false;
            if (needResetPosition) ResetCurrentPosition();
        }

        public Dictionary<String, String> GetFilterParameters()
        {
            IOrderedDictionary dicOldValues = new OrderedDictionary(StringComparer.OrdinalIgnoreCase);
            MergeDictionaries(FilterParameters, dicOldValues, dicOldValues);

            var parameters = new Dictionary<string, string>();
            foreach (Parameter parameter in FilterParameters)
                parameters.Add(parameter.Name, (String) dicOldValues[parameter.Name]);
            return parameters;
        }


        //public QueryConditionList GetSelectParameters()
        //{
        //    IOrderedDictionary dic = new OrderedDictionary(StringComparer.OrdinalIgnoreCase);
        //    MergeDictionaries(SelectParameters, SelectParameters.GetValues(_context, _owner), dic);
        //    QueryConditionList list = new QueryConditionList();
        //    foreach(DictionaryEntry entry in dic)
        //    {
        //        QueryCondition condition = new QueryCondition((string)entry.Key, ColumnFilterType.Equal, entry.Value, null);
        //        condition.EmptyCondition = true;
        //        list.Add(condition);
        //    }

        //    return list;
        //}        

        public List<Triplet> GetSelectParameters()
        {
            var selectParametersList = new List<Triplet>();
            IOrderedDictionary dic = new OrderedDictionary(StringComparer.OrdinalIgnoreCase);
            MergeDictionaries(SelectParameters, SelectParameters.GetValues(_context, _owner), dic);
            foreach (Parameter parameter in SelectParameters)
                selectParametersList.Add(new Triplet(parameter.Name, parameter.Type,
                                                     Convert.ToString(dic[parameter.Name])));
            return selectParametersList;
        }

        public void MarkAsNewRow()
        {
            newKeys = GetKeys(Current.Row);
        }

        #endregion

        #endregion

        #region private

        private static object deepCopy(object obj)
        {
            var bf = new BinaryFormatter();
            using (Stream stream = new MemoryStream())
            {
                bf.Serialize(stream, obj);
                stream.Flush();
                stream.Seek(0, SeekOrigin.Begin);
                return bf.Deserialize(stream);
            }
        }

        private Control FindSessionWorkerControl(Control control)
        {
            if (control == null) return null;
            Control findControl = control.FindControl(sessionWorkerControl);
            if (findControl != null) return findControl;
            return FindSessionWorkerControl(control.Parent);
        }

        private DataTable GetDataTable(string name)
        {
            var ds = SessionWorker.Object as DataSet;
            DataTable dataTable;
            if (ds != null) dataTable = ds.Tables[name];
            else dataTable = (DataTable) sessionWorker.Object;
            return dataTable;
        }

        private object[] GetKeys(DataRow row)
        {
            var objects = new object[Table.PrimaryKey.Length];
            for (int i = 0; i < Table.PrimaryKey.Length; i++)
                objects[i] = row[Table.PrimaryKey[i]];
            return objects;
        }

        private void EnsureSetProperies()
        {
            EnsureDataViewCreated();
            if (_table != null && loadState)
            {
                DataView.Sort = sort;
                sort = "";
                DataView.RowFilter = filter;
                filter = "";
                ResetCurrentPosition();
                loadState = false;
            }
        }

        private void View_OnListChanged(object sender, ListChangedEventArgs e)
        {
            if (e.ListChangedType == ListChangedType.ItemMoved)
            {
                ResetCurrentPosition();
            }
        }

        #endregion

        #region Перекрытые свойства

        public override bool CanInsert
        {
            get { return true; }
        }

        public override bool CanDelete
        {
            get { return true; }
        }

        public override bool CanUpdate
        {
            get { return true; }
        }

#pragma warning disable ValueParameterNotUsed
        [DefaultValue("{0}")]
        public new string OldValuesParameterFormatString
        {
            get { return "{0}"; }
            set { base.OldValuesParameterFormatString = "{0}"; }
        }

        [DefaultValue("NotUse")]
        public new string UpdateMethod
        {
            get { return base.UpdateMethod; }
            set { base.UpdateMethod = "NotUse"; }
        }

        [DefaultValue("NotUse")]
        public new string DeleteMethod
        {
            get { return base.DeleteMethod; }
            set { base.DeleteMethod = "NotUse"; }
        }

        [DefaultValue("NotUse")]
        public new string InsertMethod
        {
            get { return base.InsertMethod; }
            set { base.InsertMethod = "NotUse"; }
        }
#pragma warning restore ValueParameterNotUsed

        #endregion

        #region override, virtual methods

        #region on event methods

        protected override void OnObjectCreating(ObjectDataSourceEventArgs e)
        {
//            if (!isExecuteSelect) e.ObjectInstance = Table.NewRow();

            base.OnObjectCreating(e);

            if (e.ObjectInstance == null)
            {
                e.ObjectInstance = CreateTableAdapter();
            }
        }

        protected virtual object CreateTableAdapter()
        {
            WebInitializer.Initialize();
            QueryConditionList listConditions = new QueryConditionList();
            QueryGenerator queryGenerator = CreateQueryGenerator(listConditions);
            return queryGenerator.GetTableAdapter(listConditions);
        }

        protected virtual QueryGenerator CreateQueryGenerator(QueryConditionList listConditions)
        {
            WebInitializer.Initialize();
            Component tableAdapter;

            // создания адаптреа для историчности
            if (ShowHistoricalData)
            {
                if (Table != null && (!Table.Columns.Contains(EndDateField) || !Table.Columns.Contains(StartDateField)))
                {
                    throw new Exception(
                        string.Format("Таблица '{0}' не содержит поля '{1}'(EndDateField) или '{2}'(StartDateField)",
                                      Table.TableName, EndDateField, StartDateField));
                }
                QueryConditionList condition = HistoricalData.GetHistoricalCondition(EndDateField, StartDateField, HistolicalPoint);
                if (EnablePaging || !LoadAllHistoricalData)
                    listConditions.AddRange(condition);
            }

            Type typeTableAdapter = BuildManager.GetType(TypeName, true, true);
            tableAdapter = (Component)Activator.CreateInstance(typeTableAdapter);
            int commandIndex = TableAdapterTools.GetSelectCommandIndex(typeTableAdapter, SelectMethod);
            if (customConditions.Count > 0) listConditions.AddRange(customConditions);
            return CreateQueryGenerator(tableAdapter, commandIndex);
        }

        protected virtual QueryGenerator CreateQueryGenerator(Component adapter, int commandIndex)
        {
            WebInitializer.Initialize();
            var queryGenerator = new QueryGenerator(adapter, commandIndex);
            if (_extArguments != null && _extArguments.AllowSelectTop)
                queryGenerator.TopCount = _extArguments.TopCount;
            if (EnablePaging)
            {
                queryGenerator.AllowPaging = true;
                queryGenerator.OrderByDefault = DefaultOrderBy;
                queryGenerator.OrderBy = _arguments.SortExpression;
                queryGenerator.MaximumRows = _arguments.MaximumRows;
                queryGenerator.StartRowIndex = _arguments.StartRowIndex;
            }
            return queryGenerator;
        }

        protected override void OnSelecting(ObjectDataSourceSelectingEventArgs e)
        {
            if (e.ExecutingSelectCount)
            {
                base.OnSelecting(e);
                return;
            }
            if (fillData)
                doSelect = true;
            else
            {
                switch (fillType)
                {
                    case (TableDataSourceFillType.ParametersNotChanged):
                        doSelect = true;

                        #region Проверка изменения параметров

                        if (inputParameters != null && startFromIndex == e.Arguments.StartRowIndex && CustomConditions.Count == 0)
                        {
                            if (e.InputParameters.Count != inputParameters.Count || inputParameters.Count == 0)
                                doSelect = false;
                            else
                            {
                                doSelect = false;
                                foreach (DictionaryEntry entry in inputParameters)
                                {
                                    if (!e.InputParameters.Contains(entry.Key)) continue;

                                    object obj1 = e.InputParameters[entry.Key];
                                    object obj2 = inputParameters[entry.Key];
                                    if (obj1 == obj2) continue;
                                    if (obj1 != null && obj1.Equals(obj2)) continue;
                                    doSelect = true;
                                    break;
                                }
                            }
                        }

                        #endregion

                        break;
                    case (TableDataSourceFillType.Always):
                        doSelect = true;
                        break;
                    case (TableDataSourceFillType.Never):
                        doSelect = false;
                        break;
                    default:
                        throw new ArgumentException(string.Format("Неизвестный тип заполнения ({0})", fillType));
                }
            }
            if (EnablePaging && (IsEditRow || HasNewRow || !doSelect))
            {
                doSelect = false;
                e.Arguments.TotalRowCount = _countRows;
            }
            e.Cancel = !doSelect;
            if (doSelect)
            {
                inputParameters = e.InputParameters;
                startFromIndex = e.Arguments.StartRowIndex;
                lastFilter = GetFilterString();
            }

            base.OnSelecting(e);
        }

        protected override void OnSelected(ObjectDataSourceStatusEventArgs e)
        {
//            if (e.Exception == null)
//            {
//                DataTable returnTable;
//                returnTable = e.ReturnValue as DataTable;
//                if (returnTable == null)
//                {
//                    var returnView = e.ReturnValue as DataView;
//                    if (returnView != null) returnTable = returnView.Table;
//                }
//                if (returnTable != null) Table = returnTable;
//            }
            base.OnSelected(e);
            if (e.Exception != null)
            {
                TableDataSourceErrorEventArgs eventArgs;
                eventArgs = new TableDataSourceErrorEventArgs(e.Exception, e, true, !e.ExceptionHandled);
                OnError(eventArgs);
                e.ExceptionHandled = !eventArgs.ThrowException;
            }
        }

        protected virtual void OnUpdatingData(TableDataSourceUpdatingDataEventArgs e)
        {
            if (UpdatingData != null) UpdatingData(this, e);
        }

        protected virtual void OnUpdatedData(TableDataSourceUpdatedDataEventArgs e)
        {
            if (UpdatedData != null)
            {
                UpdatedData(this, e);
                if (e.Exception != null)
                {
                    TableDataSourceErrorEventArgs eventArgs;
                    eventArgs = new TableDataSourceErrorEventArgs(e.Exception, e, true, e.ThrowException);
                    OnError(eventArgs);
                    e.ThrowException = eventArgs.ThrowException;
                }
            }
        }

        protected virtual void OnClearingData(TableDataSourceClearingDataEventArgs e)
        {
            if (ClearingData != null) ClearingData(this, e);
        }

        protected virtual void OnClearedData(TableDataSourceClearedDataEventArgs e)
        {
            if (ClearedData != null)
            {
                ClearedData(this, e);
                if (e.Exception != null)
                {
                    TableDataSourceErrorEventArgs eventArgs;
                    eventArgs = new TableDataSourceErrorEventArgs(e.Exception, e, true, e.ThrowException);
                    OnError(eventArgs);
                    e.ThrowException = eventArgs.ThrowException;
                }
            }
        }

        protected override void OnDeleted(ObjectDataSourceStatusEventArgs e)
        {
            base.OnDeleted(e);
            if (e.Exception != null)
            {
                TableDataSourceErrorEventArgs eventArgs;
                eventArgs = new TableDataSourceErrorEventArgs(e.Exception, e, true, !e.ExceptionHandled);
                OnError(eventArgs);
                e.ExceptionHandled = !eventArgs.ThrowException;
            }
        }

        protected override void OnInserted(ObjectDataSourceStatusEventArgs e)
        {
            base.OnInserted(e);
            if (e.Exception != null)
            {
                TableDataSourceErrorEventArgs eventArgs;
                eventArgs = new TableDataSourceErrorEventArgs(e.Exception, e, true, !e.ExceptionHandled);
                OnError(eventArgs);
                e.ExceptionHandled = !eventArgs.ThrowException;
            }
        }

        protected override void OnUpdated(ObjectDataSourceStatusEventArgs e)
        {
            base.OnUpdated(e);
            if (e.Exception != null)
            {
                TableDataSourceErrorEventArgs eventArgs;
                eventArgs = new TableDataSourceErrorEventArgs(e.Exception, e, true, !e.ExceptionHandled);
                OnError(eventArgs);
                e.ExceptionHandled = !eventArgs.ThrowException;
            }
        }

        protected virtual void OnError(TableDataSourceErrorEventArgs e)
        {
            if (Error != null) Error(this, e);
        }

        protected virtual void OnCurrentChanged(CurrentChangeEventArgs e)
        {
            if (CurrentChanged != null) CurrentChanged(this, e);
        }

        protected virtual void OnCurrentChanging(CurrentChangingEventArgs e)
        {
            if (CurrentChanging != null) CurrentChanging(this, e);
        }

        protected virtual void OnAddingNewRow(TableDataSourceAddingRowEventArgs e)
        {
            if (AddingRow != null) AddingRow(this, e);
        }

        protected virtual void OnAddedNewRow(TableDataSourceAddedRowEventArgs e)
        {
            if (AddedRow != null) AddedRow(this, e);
        }

        #endregion

        #region other

        public override bool CanPage
        {
            get { return EnablePaging; }
        }

        public override bool CanRetrieveTotalRowCount
        {
            get { return true; }
        }

        public virtual bool SetCurrent(DataRow row)
        {
            int index = FindHelper.Find(row, DataView, CurrentIndex);
            CurrentIndex = index;
            return CurrentIndex == index;
        }

        public virtual void CheckPosition()
        {
            if (currentKeys == null) return;
            ResetCurrentPosition();
//            if (CurrentIndex >= DataView.Count) ResetCurrentPosition();
//            else
//            {                
//                if(Current != null)
//                {
//                    object[] keys = GetKeys(Current.Row);
//                    for (int i = 0; i < keys.Length; i++)
//                    {
//                        if(!currentKeys[i].Equals(keys[i]))
//                        {
//                            ResetCurrentPosition();
//                            return;
//                        }
//                    }
//                }
//            }
        }

        protected virtual void ResetCurrentPosition()
        {
            ResetCurrentPosition(-1);
        }

        protected virtual void ResetCurrentPosition(int oldIndex)
        {
            if (beginLoad)
            {
                needResetPosition = true;
                return;
            }
            int newIndex = -1;
            if (currentKeys != null && Table != null)
            {
                DataRow row = Table.Rows.Find(currentKeys);
                newIndex = FindHelper.Find(row, DataView, currentIndex);
                // todo: Придумать какое-нибудь решение, если новая запись исключится фильтром.
                if (newIndex < 0)
                {
                    currentKeys = null;
                    isEditRow = false;
                }
            }
            if ((DataMode == TableDataSourceDataMode.OnlyCurrent ||
                 DataMode == TableDataSourceDataMode.All) && newIndex == -1 &&
                oldIndex > 0 && DataView.Count >= oldIndex)
            {
                newIndex = oldIndex - 1;
                currentKeys = GetKeys(DataView[newIndex].Row);
            }
            if (currentIndex != newIndex)
            {
                int oldValue = currentIndex;
                currentIndex = newIndex;
                OnCurrentChanged(new CurrentChangeEventArgs(null, null, newIndex, oldValue));
            }
        }

        protected override int ExecuteDelete(IDictionary keys, IDictionary oldValues)
        {
            IOrderedDictionary dictionary2 = new OrderedDictionary(StringComparer.OrdinalIgnoreCase);
//            MergeDictionaries(DeleteParameters, DeleteParameters.GetValues(_context, _owner), dictionary2);
            MergeDictionaries(DeleteParameters, keys, dictionary2);
            var args2 = new ObjectDataSourceMethodEventArgs(dictionary2);
            OnDeleting(args2);
            if (args2.Cancel)
                return 0;
            DataRow row = FindHelper.FindRow(dictionary2, Table);
            int effectedRows = 0;
            Exception exception = null;
            if (row != null)
            {
                try
                {
                    SetCurrent(row);
                    row.Delete();
                    effectedRows = 1;
                    newKeys = null;
                    isEditRow = false;
                    SetCurrent(null);
                }
                catch (Exception e)
                {
                    exception = e;
                }
            }
            var args =
                new ObjectDataSourceStatusEventArgs(effectedRows, new OrderedDictionary(), exception);
            OnDeleted(args);
            if (exception != null && !args.ExceptionHandled) throw exception;
            return effectedRows;
        }

        protected override int ExecuteInsert(IDictionary values)
        {
            IOrderedDictionary dictionary2 = new OrderedDictionary(StringComparer.OrdinalIgnoreCase);
            MergeDictionaries(InsertParameters, InsertParameters.GetValues(_context, _owner), dictionary2);
            MergeDictionaries(InsertParameters, values, dictionary2);
            var args2 = new ObjectDataSourceMethodEventArgs(dictionary2);
            OnInserting(args2);
            if (args2.Cancel)
                return 0;
            DataRow row = Table.NewRow();
            Exception exception = null;
            int effectedRows = 0;
            try
            {
                if (IsEditRow) EndEdit();
                SetValues(row, dictionary2);
                row.EndEdit();
                var argsAdding = new TableDataSourceAddingRowEventArgs(row);
                OnAddingNewRow(argsAdding);
                if (!argsAdding.Cancel)
                {
                    Table.Rows.Add(row);
                    effectedRows = 1;
                    SetCurrent(row);
                    OnAddedNewRow(new TableDataSourceAddedRowEventArgs(row));
                }
            }
            catch (Exception e)
            {
                exception = e;
            }
            ResetCurrentPosition();
            ObjectDataSourceStatusEventArgs args;
            args = new ObjectDataSourceStatusEventArgs(effectedRows, GetValues(row), exception);
            OnInserted(args);
            if (exception != null && !args.ExceptionHandled) throw exception;
            return effectedRows;
        }

        protected override int ExecuteUpdate(IDictionary keys, IDictionary values, IDictionary oldValues)
        {
            IOrderedDictionary dicOldValues = new OrderedDictionary(StringComparer.OrdinalIgnoreCase);
            string oldValuesParameterFormatString = OldValuesParameterFormatString;
//            MergeDictionaries(UpdateParameters, UpdateParameters.GetValues(_context, _owner), dicOldValues);
            MergeDictionaries(UpdateParameters, keys, dicOldValues, oldValuesParameterFormatString);

            IOrderedDictionary dicNewValues = new OrderedDictionary(StringComparer.OrdinalIgnoreCase);
            MergeDictionaries(UpdateParameters, UpdateParameters.GetValues(_context, _owner), dicNewValues);
            MergeDictionaries(UpdateParameters, values, dicNewValues);

            var args2 = new ObjectDataSourceMethodEventArgs(dicOldValues);
            OnUpdating(args2);
            if (args2.Cancel)
                return 0;
            DataRow row = FindHelper.FindRow(dicOldValues, Table);
            int effectedRows = 0;
            Exception exception = null;
            if (row != null)
            {
                effectedRows = 1;
                try
                {
                    SetCurrent(row);
                    SetValues(row, dicNewValues);
                    row.EndEdit();
                }
                catch (Exception e)
                {
                    exception = e;
                }
            }
            ResetCurrentPosition();
            var args =
                new ObjectDataSourceStatusEventArgs(effectedRows, GetValues(row), exception);
            OnUpdated(args);
            if (exception != null && !args.ExceptionHandled) throw exception;
            return effectedRows;
        }

        protected override void OnDataSourceViewChanged(EventArgs e)
        {
            if (!isExecuteSelect)
                base.OnDataSourceViewChanged(e);
        }

/*
        protected IEnumerable ExecuteSelectOld(DataSourceSelectArguments arguments)
        {
            if (isExecuteSelect) return null;
            isExecuteSelect = true;
            string filterExpression = FilterExpression;
            try
            {
                if (Table != null)
                {
                    DataView.Sort = GetSortExpression(arguments);
                    DataView.RowFilter = GetFilterString();
                    FilterExpression = "";
                    ResetCurrentPosition();
                }
                IOrderedDictionary dic = new OrderedDictionary(StringComparer.OrdinalIgnoreCase);
                MergeDictionaries(SelectParameters, SelectParameters.GetValues(_context, _owner), dic);

//                base.ExecuteSelect(arguments);
                BaseExecuteSelect(arguments);
                if (EnablePaging)
                {
                    _countRows = arguments.TotalRowCount;
                    //постраничная загрузка, чиститься если возращаем количество записей 0
                    if (_countRows <= 0) Table.Clear();
                }
                if (beginLoad) return null;
                switch (dataMode)
                {
                    case TableDataSourceDataMode.All:
                        return DataView;
                    case TableDataSourceDataMode.OnlyCurrent:
                        if (Current != null)
                            return new[] {Current};
                        else
                            return new DataRowView[] {};
                    case TableDataSourceDataMode.CurrentOrAll:
                        if (Current == null) return DataView;
                        return new[] {Current};
                    default:
                        throw new ArgumentException(string.Format("Неизвестный режим возврата данных ({0})", dataMode));
                }
            }
            finally
            {
                FilterExpression = filterExpression;
                isExecuteSelect = false;
            }
        }
*/

        protected override IEnumerable ExecuteSelect(DataSourceSelectArguments arguments)
        {
            if (isExecuteSelect) return null;
            isExecuteSelect = true;

            if (SelectMethod.Length == 0)
                throw new InvalidOperationException("Empty Select Method not supported");
            if (CanSort)
                arguments.AddSupportedCapabilities(DataSourceCapabilities.Sort);
            if (CanPage)
                arguments.AddSupportedCapabilities(DataSourceCapabilities.Page);
            if (CanRetrieveTotalRowCount)
                arguments.AddSupportedCapabilities(DataSourceCapabilities.RetrieveTotalRowCount);
            arguments.RaiseUnsupportedCapabilitiesError(this);

            string filterExpression = FilterExpression;
            try
            {
                if (Table != null)
                {
                    DataView.Sort = GetSortExpression(arguments);
                    DataView.RowFilter = GetFilterString();
                    FilterExpression = "";
                    ResetCurrentPosition();
                }
                IOrderedDictionary dic = new OrderedDictionary(StringComparer.OrdinalIgnoreCase);
                MergeDictionaries(SelectParameters, SelectParameters.GetValues(_context, _owner), dic);

                IOrderedDictionary parameters = new OrderedDictionary(StringComparer.OrdinalIgnoreCase);
                foreach (DictionaryEntry entry in SelectParameters.GetValues(_context, _owner))
                    parameters[entry.Key] = entry.Value;
                var e = new ObjectDataSourceSelectingEventArgs(parameters, arguments, false);
                OnSelecting(e);
                if (!e.Cancel)
                {
                    var listParams = new QueryConditionList();
                    foreach (DictionaryEntry parameter in parameters)
                    {
                        var condition = new QueryCondition((string) parameter.Key, ColumnFilterType.Equal,
                                                           parameter.Value, null)
                                            {
                                                EmptyCondition = true
                                            };
                        listParams.Add(condition);
                    }

                    if (!string.IsNullOrEmpty(SortParameterName))
                    {
                        listParams.Add(new QueryCondition(SortParameterName, ColumnFilterType.Equal, arguments.SortExpression, null));
                        arguments.SortExpression = string.Empty;
                    }
                    ExecuteFill(arguments, listParams);

                    if (EnablePaging)
                    {
                        _countRows = arguments.TotalRowCount;
                        //постраничная загрузка, чиститься если возращаем количество записей 0
                        if (_countRows <= 0) Table.Clear();
                    }
                }
                if (beginLoad) return null;
                switch (dataMode)
                {
                    case TableDataSourceDataMode.All:
                        return DataView;
                    case TableDataSourceDataMode.OnlyCurrent:
                        if (Current != null)
                            return new[] {Current};
                        else
                            return new DataRowView[] {};
                    case TableDataSourceDataMode.CurrentOrAll:
                        if (Current == null) return DataView;
                        return new[] {Current};
                    default:
                        throw new ArgumentException(string.Format("Неизвестный режим возврата данных ({0})", dataMode));
                }
            }
            finally
            {
                FilterExpression = filterExpression;
                isExecuteSelect = false;
            }
        }

        private void ExecuteFill(DataSourceSelectArguments arguments, QueryConditionList listParams)
        {
            QueryGenerator generator = CreateQueryGenerator(listParams);
            int totalRowCount = -1;
            var table = (DataTable)Activator.CreateInstance(TableGetType());
            generator.Fill(table, listParams);
            if (arguments.RetrieveTotalRowCount)
            {
                totalRowCount = generator.GetRowsCount(listParams);
                arguments.TotalRowCount = totalRowCount;
            }
            DataTable overDateTable = GetHistoricalDataOverDate(table);
            if (overDateTable != null) table = overDateTable;
            Table = table;
            var args3 = new ObjectDataSourceStatusEventArgs(table, new OrderedDictionary());
            OnSelected(args3);
            if (Table != null && arguments.RetrieveTotalRowCount && totalRowCount == -1)
                arguments.TotalRowCount = Table.Rows.Count;
        }

        private Type TableGetType()
        {
            if (Table != null) return Table.GetType();
            return Activator.CreateInstance(GetType(TypeName)).GetType().GetMethod(SelectMethod).ReturnType;
        }

/*
        protected virtual int QueryTotalRowCount(IOrderedDictionary mergedParameters,
                                                 DataSourceSelectArguments arguments, bool disposeInstance,
                                                 ref object instance)
        {
            if (SelectCountMethod.Length > 0)
            {
                var e = new ObjectDataSourceSelectingEventArgs(mergedParameters, arguments, true);
                OnSelecting(e);
                if (e.Cancel)
                    return -1;
                Type type = GetType(TypeName);
                ObjectDataSourceMethod method = GetResolvedMethodData(type, SelectCountMethod, mergedParameters,
                                                                      DataSourceOperation.SelectCount);
                ObjectDataSourceResult result = InvokeMethod(method, disposeInstance, ref instance);
                if ((result.ReturnValue != null) && (result.ReturnValue is int))
                    return (int) result.ReturnValue;
            }
            else
            {
                DbInitializer.Init();
                Type typeTableAdapter = BuildManager.GetType(TypeName, true, true);
                var adapter = (Component) Activator.CreateInstance(typeTableAdapter);
                int commandIndex = TableAdapterTools.GetSelectCommandIndex(typeTableAdapter, SelectMethod);
                DbInitializer.Init();
                QueryGenerator queryGenerator = CreateQueryGenerator(adapter, commandIndex);
                var list = new QueryConditionList();
                if (ShowHistoricalData)
                {
                    //добавляем уловия по историчности
                    list.AddRange(HistoricalData.GetHistoricalCondition(endDateField, startDateField, histolicalPoint));
                }
                list.AddRange(customConditions);
                IOrderedDictionary dic = new OrderedDictionary(StringComparer.OrdinalIgnoreCase);
                MergeDictionaries(SelectParameters, SelectParameters.GetValues(_context, _owner), dic);
                foreach (DictionaryEntry entry in dic)
                {
                    var condition = new QueryCondition((string) entry.Key, ColumnFilterType.Equal, entry.Value, null);
                    condition.EmptyCondition = true;
                    list.Add(condition);
                }
                return queryGenerator.GetRowsCount(list);
            }
            return -1;
        }
*/

        private string GetSortExpression(DataSourceSelectArguments arguments)
        {
            if (string.IsNullOrEmpty(_sortExpression))
                return arguments.SortExpression;
            if (string.IsNullOrEmpty(arguments.SortExpression))
                return _sortExpression;
            return arguments.SortExpression;
            //todo: добавить возможность управлять объединением сортировок
            //return string.Format("{0}, {1}", _sortExpression, arguments.SortExpression);
        }

        protected override object SaveViewState()
        {
            var properties = new Hashtable();
            var pair = new Pair(base.SaveViewState(), properties);

            properties["inputParameters"] = inputParameters;
            properties["startFromIndex"] = startFromIndex;
            properties["lastFilter"] = lastFilter;

            if (SessionWorker == null) properties["table"] = _table;
            else properties["table"] = null;

            properties["sessionWorkerControl"] = sessionWorkerControl;
            properties["TableName"] = TableName;
            properties["currentIndex"] = currentIndex;
            properties["currentKeys"] = currentKeys;
            properties["newKeys"] = newKeys;

            if (Table != null)
            {
                properties["Sort"] = DataView.Sort;
                properties["RowFilter"] = DataView.RowFilter;
            }
            else
            {
                properties["Sort"] = "";
                properties["RowFilter"] = "";
            }

            properties["fillType"] = fillType;
            properties["isEditRow"] = isEditRow;
            properties["rowState"] = rowState;
            properties["histolicalPoint"] = histolicalPoint;
            properties["startDateField"] = startDateField;
            properties["endDateField"] = endDateField;
            properties["showHistoricalData"] = _showHistoricalData;
            properties["loadAllHistoricalData"] = loadAllHistoricalData;
            properties["editingItem"] = editingItem;
            properties["_defaultOrderBy"] = _defaultOrderBy;
            properties["_enablePaging"] = _enablePaging;
            properties["_countRows"] = _countRows;
            properties["_setFilterByCustomConditions"] = _setFilterByCustomConditions;
            properties["_sortExpression"] = _sortExpression;
            properties["_historicalKey"] = _historicalKey;
            properties["_historicalSelectMethod"] = _historicalSelectMethod;
            properties["_historicalValues"] = _historicalValues;

            return pair;
        }

        protected override void LoadViewState(object savedState)
        {
            var pair = (Pair) savedState;
            if (savedState == null)
            {
                base.LoadViewState(null);
                return;
            }
            base.LoadViewState(pair.First);
            var properties = pair.Second as Hashtable;
            if (properties != null)
            {
                inputParameters = properties["inputParameters"] as IDictionary;
                startFromIndex = (int)properties["startFromIndex"];
                lastFilter = (string)properties["lastFilter"];
                _table = properties["table"] as DataTable;
                sessionWorkerControl = properties["sessionWorkerControl"] as string;
                TableName = properties["TableName"] as string;
                currentIndex = (int) properties["currentIndex"];
                currentKeys = properties["currentKeys"] as object[];
                newKeys = properties["newKeys"] as object[];
                sort = properties["Sort"] as string;
                filter = properties["RowFilter"] as string;
                fillType = (TableDataSourceFillType) properties["fillType"];
                isEditRow = (bool) properties["isEditRow"];
                rowState = (DataRowState) properties["rowState"];
                histolicalPoint = (DateTime?) properties["histolicalPoint"];
                startDateField = (string) properties["startDateField"];
                endDateField = (string) properties["endDateField"];
                _showHistoricalData = (bool) properties["showHistoricalData"];
                loadAllHistoricalData = (bool) properties["loadAllHistoricalData"];
                editingItem = (object[]) properties["editingItem"];
                _defaultOrderBy = (string) properties["_defaultOrderBy"];
                _enablePaging = (bool) properties["_enablePaging"];
                _countRows = (int) properties["_countRows"];
                _setFilterByCustomConditions = (bool) properties["_setFilterByCustomConditions"];
                _sortExpression = (string) properties["_sortExpression"];
                _historicalKey = (string) properties["_historicalKey"];
                _historicalSelectMethod = (string) properties["_historicalSelectMethod"];
                _historicalValues = (object[]) properties["_historicalValues"];
            }
            loadState = true;
        }

        protected virtual DataTable GetHistoricalDataOverDate(DataTable loadTable)
        {
            WebInitializer.Initialize();
            DataTable table = null;
            if (!string.IsNullOrEmpty(HistoricalSelectMethod) && HistoricalCountKeys > 0)
            {
                Type typeTableAdapter = BuildManager.GetType(TypeName, true, true);
                var tableAdapter = (Component) Activator.CreateInstance(typeTableAdapter);
                var queryGenerator = new QueryGenerator(tableAdapter, HistoricalSelectMethod);
                var list = new QueryConditionList();
                if (ShowHistoricalData && !LoadAllHistoricalData)
                    list.AddRange(HistoricalData.GetHistoricalOverCondition(EndDateField, HistolicalPoint));
                var keysCondition = new QueryCondition();
                list.Add(keysCondition);
                keysCondition.Conditions = new QueryConditionList();
                keysCondition.Conditions.ConditionJunction = ConditionJunction.Or;
                foreach (object value in HistoricalValues)
                {
                    if (value != null && value != DBNull.Value)
                        keysCondition.Conditions.Add(new QueryCondition(_historicalKey, ColumnFilterType.Equal, value,
                                                                        null));
                }
                if (keysCondition.Conditions.Count > 0)
                {
                    table = loadTable.Clone();
                    queryGenerator.Fill(table, list);
                }
            }
            if (table != null)
            {
                table.Merge(loadTable);
                return table;
            }
            return loadTable;
        }

        #endregion

        #endregion

        #region protected

//        protected DataRow FindRow(IDictionary dictionary2)
//        {
//            DataRow row = FindRowByPrimaryKey(dictionary2);
//            if (row == null)
//            {
//                DataView view = new DataView(Table);
//                string sort = "";
//                foreach (DictionaryEntry entry in dictionary2)
//                    sort += entry.Key + " ";
//                view.Sort = sort;
//                int index = view.Find(GetValues(dictionary2, dictionary2));
//                if (index > -1) row = view[index].Row;
//            }
//            return row;
//        }
//
//        protected DataRow FindRowByPrimaryKey(IDictionary dictionary2)
//        {
//            if (Table.PrimaryKey.Length == dictionary2.Count && dictionary2.Count > 0)
//            {
//                object[] values = new object[dictionary2.Count];
//                int i = 0;
//                foreach (DataColumn column in Table.PrimaryKey)
//                {
//                    if (dictionary2.Contains(column.ColumnName))
//                        values[i++] = dictionary2[column.ColumnName];
//                    else i = -1;
//                }
//                if (i > -1) return Table.Rows.Find(values);
//            }
//            return null;
//        }
//
//        protected static object[] GetValues(IDictionary keys, IDictionary value)
//        {
//            object[] values = new object[keys.Count];
//            int i = 0;
//            foreach (DictionaryEntry entry in keys)
//            {
//                if (value.Contains(entry.Key))
//                    values[i++] = value[entry.Key];
//            }
//            return values;
//        }

        protected static IDictionary GetValues(DataRow row)
        {
            if (row == null) return new OrderedDictionary();
            var values = new OrderedDictionary(row.Table.Columns.Count);
            foreach (DataColumn column in row.Table.Columns)
                values[column.ColumnName] = row[column];
            return values;
        }

        protected static void SetValues(DataRow row, IDictionary values)
        {
            Type type = typeof (string);
            foreach (DictionaryEntry entry in values)
            {
                DataColumn column = row.Table.Columns[(string) entry.Key];
//                Debug.Assert(
//                    column != null || "original_id".Equals((string) entry.Key, StringComparison.OrdinalIgnoreCase),
//                    string.Format("column == null, when ColumnName '{0}'", entry.Key));

                if (column == null || column.ReadOnly)
                    continue;
#if NULL_REFERENCE
                bool nullRef = false;

                if (column.DataType == typeof (Int64) || column.DataType == typeof (Int32) ||
                    column.DataType == typeof (Int16))
                {
                    // поиск связи, где эта колонка ссылается на другую таблицу.
                    foreach (DataRelation dataRelation in column.Table.ParentRelations)
                    {
                        if (dataRelation.ChildColumns.Length == 1 && dataRelation.ChildColumns[0] == column)
                        {
                            nullRef = true;
                            break;
                        }
                    }
                }

                if (entry.Value != null && entry.Value != DBNull.Value &&
                    (!nullRef || Convert.ToInt64(entry.Value) != 0) &&
                    // условие для пустой ссылки, чтоб ссылка была не 0, а DBNull.Value
                    (entry.Value.ToString() != "" || column.DataType == type))
                    row[column] = Convert.ChangeType(entry.Value, column.DataType);
                else row[column] = DBNull.Value;
#else
                if (entry.Value != null && entry.Value != DBNull.Value &&
                    (entry.Value.ToString() != "" || column.DataType == type))
                    row[column] = Convert.ChangeType(entry.Value, column.DataType);
                else row[column] = DBNull.Value;
#endif
            }
        }


/*
        protected int QueryTotalRowCount(IOrderedDictionary mergedParameters, DataSourceSelectArguments arguments)
        {
            if (this.SelectMethod.Length > 0)
            {
                QueryGenerator queryGenerator;
                Type typeTableAdapter;
                Component adapter;
                int commandIndex;

//                ObjectDataSourceSelectingEventArgs e = new ObjectDataSourceSelectingEventArgs(mergedParameters, arguments, true);
//                this.OnSelecting(e);
//                if (e.Cancel)
//                {
//                    return -1;
//                }
                Type type = this.GetType(this.TypeName);
                // создания даптреа для историчности
                if (ShowHistoricalData && e.ObjectInstance == null)
                {
                    if (Table != null && (!Table.Columns.Contains(EndDateField) || !Table.Columns.Contains(StartDateField)))
                        throw new Exception(string.Format("Таблица '{0}' не содержит поля '{1}'(EndDateField) или '{2}'(StartDateField)", Table.TableName, EndDateField, StartDateField));
                    if (!LoadAllHistoricalData)
                        e.ObjectInstance = HistoricalData.GetTableAdapterToHistoricalData(EndDateField, StartDateField, HistolicalPoint, TypeName, SelectMethod);
                }

                // создание или изменение адаптера для дополнительных условий
                if (customConditions.Count > 0 || (_extArguments != null && _extArguments.AllowSelectTop) || EnablePaging)
                {

                    if (e.ObjectInstance == null)
                    {
                        typeTableAdapter = BuildManager.GetType(TypeName, true, true);
                        adapter = (Component)Activator.CreateInstance(typeTableAdapter);
                    }
                    else
                    {
                        adapter = (Component)e.ObjectInstance;
                        typeTableAdapter = adapter.GetType();
                    }
                    commandIndex = QueryGenerator.GetIndexNumber(typeTableAdapter, SelectMethod);
                    queryGenerator = new QueryGenerator(adapter, commandIndex);
                    if (_extArguments != null && _extArguments.AllowSelectTop)
                        queryGenerator.TopCount = _extArguments.TopCount;
                    if (EnablePaging)
                    {
                        queryGenerator.AllowPaging = true;
                        queryGenerator.OrderByDefault = DefaultOrderBy;
                        queryGenerator.OrderBy = _arguments.SortExpression;
                        queryGenerator.MaximumRows = _arguments.MaximumRows;
                        queryGenerator.StartRowIndex = _arguments.StartRowIndex;
                    }
                    adapter = queryGenerator.GetTableAdapter(customConditions);
                    if (EnablePaging)
                    {
                        queryGenerator.AllowPaging = false;
                        adapter = HistoricalData.GetTableAdapterToHistoricalData(adapter, endDateField, startDateField, histolicalPoint, typeTableAdapter, QueryGenerator.GetIndexNumber(typeTableAdapter, SelectCountMethod));
                    }
                    e.ObjectInstance = adapter;
                }                
                ObjectDataSourceMethod method = this.GetResolvedMethodData(type, this.SelectCountMethod, mergedParameters, DataSourceOperation.SelectCount);
                ObjectDataSourceResult result = this.InvokeMethod(method, disposeInstance, ref instance);
                if ((result.ReturnValue != null) && (result.ReturnValue is int))
                {
                    return (int)result.ReturnValue;
                }
            }
            return -1;
        }
*/

        #endregion

        #region private, internal функции из .NET

/*
        private void BaseExecuteSelect(DataSourceSelectArguments arguments)
        {
            if (SelectMethod.Length == 0)
                throw new InvalidOperationException("Empty Select Method not supported");
            if (CanSort)
                arguments.AddSupportedCapabilities(DataSourceCapabilities.Sort);
            if (CanPage)
                arguments.AddSupportedCapabilities(DataSourceCapabilities.Page);
            if (CanRetrieveTotalRowCount)
                arguments.AddSupportedCapabilities(DataSourceCapabilities.RetrieveTotalRowCount);
            arguments.RaiseUnsupportedCapabilitiesError(this);
            IOrderedDictionary parameters = new OrderedDictionary(StringComparer.OrdinalIgnoreCase);
            foreach (DictionaryEntry entry in SelectParameters.GetValues(_context, _owner))
                parameters[entry.Key] = entry.Value;
            foreach (QueryCondition condition in customConditions)
            {
                if (condition.EmptyCondition)
                {
                    if (!condition.Param1IsNull && !string.IsNullOrEmpty(condition.Param1.Name))
                    {
                        if (condition.Param1.Name.StartsWith(SpecificInstances.DbConstants.SqlParameterPrefix))
                        {
                            string name =
                                condition.Param1.Name.Substring(SpecificInstances.DbConstants.SqlParameterPrefix.Length);
                            parameters[name] = condition.Param1.Value;
                        }
                        else
                            parameters[condition.Param1.Name] = condition.Param1.Value;
                    }
                    if (!condition.Param2IsNull)
                    {
                        if (condition.Param2.Name.StartsWith(SpecificInstances.DbConstants.SqlParameterPrefix))
                        {
                            string name =
                                condition.Param2.Name.Substring(SpecificInstances.DbConstants.SqlParameterPrefix.Length);
                            parameters[name] = condition.Param2.Value;
                        }
                        else
                            parameters[condition.Param2.Name] = condition.Param2.Value;
                    }
                }
            }
            var e = new ObjectDataSourceSelectingEventArgs(parameters, arguments, false);
            OnSelecting(e);
            if (e.Cancel)
                return;
            var mergedParameters = new OrderedDictionary(parameters.Count);
            foreach (DictionaryEntry entry2 in parameters)
                mergedParameters.Add(entry2.Key, entry2.Value);
            string sortParameterName = SortParameterName ?? "";
            if (sortParameterName.Length > 0)
            {
                parameters[sortParameterName] = arguments.SortExpression;
                arguments.SortExpression = string.Empty;
            }
//            if (EnablePaging)
//            {
//                string maximumRowsParameterName = MaximumRowsParameterName;
//                string startRowIndexParameterName = StartRowIndexParameterName;
//                if (string.IsNullOrEmpty(maximumRowsParameterName) || string.IsNullOrEmpty(startRowIndexParameterName))
//                    throw new InvalidOperationException("ObjectDataSourceView_MissingPagingSettings " + _owner.ID);
//                IDictionary source = new OrderedDictionary(StringComparer.OrdinalIgnoreCase);
////                source[maximumRowsParameterName] = arguments.MaximumRows;
////                source[startRowIndexParameterName] = arguments.StartRowIndex;
//                MergeDictionaries(SelectParameters, source, parameters);
//            }
            Type type = GetType(TypeName);
            object instance = null;
            ObjectDataSourceResult result = null;
            int totalRowCount = -1;
            try
            {
                ObjectDataSourceMethod method = GetResolvedMethodData(type, SelectMethod, parameters,
                                                                      DataSourceOperation.Select);
                result = InvokeMethod(method, false, ref instance);
                if (result.ReturnValue == null)
                    return;
                if (arguments.RetrieveTotalRowCount)
                {
                    if (totalRowCount < 0)
                    {
                        totalRowCount = QueryTotalRowCount(mergedParameters, arguments, true, ref instance);
                        arguments.TotalRowCount = totalRowCount;
                    }
                }
            }
            finally
            {
                if (instance != null)
                    ReleaseInstance(instance);
            }
            if (Table != null && arguments.RetrieveTotalRowCount && totalRowCount == -1)
                arguments.TotalRowCount = Table.Rows.Count;
        }

        private ObjectDataSourceMethod GetResolvedMethodData(Type type, string methodName, IDictionary allParameters,
                                                             DataSourceOperation operation)
        {
            bool flag = operation == DataSourceOperation.SelectCount;
            DataObjectMethodType select = DataObjectMethodType.Select;
            if (!flag)
                select = GetMethodTypeFromOperation(operation);
            MethodInfo[] methods =
                type.GetMethods(BindingFlags.FlattenHierarchy | BindingFlags.Public | BindingFlags.Static |
                                BindingFlags.Instance);
            MethodInfo methodInfo = null;
            ParameterInfo[] infoArray2 = null;
            int num = -1;
            bool flag2 = false;
            int count = allParameters.Count;
            foreach (MethodInfo info2 in methods)
            {
                if (string.Equals(methodName, info2.Name, StringComparison.OrdinalIgnoreCase) &&
                    !info2.IsGenericMethodDefinition)
                {
                    ParameterInfo[] infoArray3 = info2.GetParameters();
                    if (infoArray3.Length == count)
                    {
                        bool flag3 = false;
                        foreach (ParameterInfo info3 in infoArray3)
                        {
                            if (!allParameters.Contains(info3.Name))
                            {
                                flag3 = true;
                                break;
                            }
                        }
                        if (!flag3)
                        {
                            int num4 = 0;
                            if (!flag)
                            {
                                var attribute =
                                    Attribute.GetCustomAttribute(info2, typeof (DataObjectMethodAttribute), true) as
                                    DataObjectMethodAttribute;
                                if ((attribute != null) && (attribute.MethodType == select))
                                {
                                    if (attribute.IsDefault)
                                        num4 = 2;
                                    else
                                        num4 = 1;
                                }
                            }
                            if (num4 == num)
                                flag2 = true;
                            else if (num4 > num)
                            {
                                num = num4;
                                flag2 = false;
                                methodInfo = info2;
                                infoArray2 = infoArray3;
                            }
                        }
                    }
                }
            }
            if (flag2)
                throw new InvalidOperationException("ObjectDataSourceView_MultipleOverloads");
            if (methodInfo == null)
            {
                if (count == 0)
                    throw new InvalidOperationException(
                        string.Format("ObjectDataSourceView_MethodNotFoundNoParams {0} {1}", _owner.ID, methodName));
                var array = new string[count];
                allParameters.Keys.CopyTo(array, 0);
                string str = string.Join(", ", array);
                throw new InvalidOperationException(
                    string.Format("ObjectDataSourceView_MethodNotFoundWithParams {0} {1} {2}", _owner.ID, methodName,
                                  str));
            }
            OrderedDictionary parameters = null;
            int length = infoArray2.Length;
            if (length > 0)
            {
                parameters = new OrderedDictionary(length, StringComparer.OrdinalIgnoreCase);
                bool convertNullToDBNull = ConvertNullToDBNull;
                for (int i = 0; i < infoArray2.Length; i++)
                {
                    ParameterInfo info4 = infoArray2[i];
                    string name = info4.Name;
                    object obj2 = allParameters[name];
                    if (convertNullToDBNull && (obj2 == null))
                        obj2 = DBNull.Value;
                    else
                        obj2 = BuildObjectValue(obj2, info4.ParameterType, name);
                    parameters.Add(name, obj2);
                }
            }
            return new ObjectDataSourceMethod(operation, type, methodInfo, parameters);
        }
*/

/*
        private object BuildObjectValue(object value, Type destinationType, string paramName)
        {
            return reflectionBuildObjectValue.Invoke(this, new[] {value, destinationType, paramName});
        }

        private ObjectDataSourceResult InvokeMethod(ObjectDataSourceMethod method, bool disposeInstance,
                                                    ref object instance)
        {
            if (method.MethodInfo.IsStatic)
            {
                if (instance != null)
                    ReleaseInstance(instance);
                instance = null;
            }
            else if (instance == null)
            {
                var e = new ObjectDataSourceEventArgs(null);
                OnObjectCreating(e);
                if (e.ObjectInstance == null)
                {
                    e.ObjectInstance = Activator.CreateInstance(method.Type);
                    OnObjectCreated(e);
                }
                instance = e.ObjectInstance;
            }
            object returnValue = null;
            int affectedRows = -1;
            bool flag = false;
            object[] parameters = null;
            if ((method.Parameters != null) && (method.Parameters.Count > 0))
            {
                parameters = new object[method.Parameters.Count];
                for (int i = 0; i < method.Parameters.Count; i++)
                    parameters[i] = method.Parameters[i];
            }
            try
            {
                returnValue = method.MethodInfo.Invoke(instance, parameters);
                var table = returnValue as DataTable;
                if (table != null)
                {
                    DataTable overDateTable = GetHistoricalDataOverDate(table);
                    if (overDateTable != null)
                        returnValue = overDateTable;
                }
            }
            catch (Exception exception)
            {
                IDictionary outputParameters = GetOutputParameters(method.MethodInfo.GetParameters(), parameters);
                var args2 = new ObjectDataSourceStatusEventArgs(returnValue, outputParameters, exception);
                flag = true;
                switch (method.Operation)
                {
                    case DataSourceOperation.Delete:
                        OnDeleted(args2);
                        break;

                    case DataSourceOperation.Insert:
                        OnInserted(args2);
                        break;

                    case DataSourceOperation.Select:
                        OnSelected(args2);
                        break;

                    case DataSourceOperation.Update:
                        OnUpdated(args2);
                        break;

                    case DataSourceOperation.SelectCount:
                        OnSelected(args2);
                        break;
                }
                affectedRows = args2.AffectedRows;
                if (!args2.ExceptionHandled)
                    throw;
            }
            finally
            {
                try
                {
                    if (!flag)
                    {
                        IDictionary dictionary2 = GetOutputParameters(method.MethodInfo.GetParameters(), parameters);
                        var args3 = new ObjectDataSourceStatusEventArgs(returnValue, dictionary2);
                        switch (method.Operation)
                        {
                            case DataSourceOperation.Delete:
                                OnDeleted(args3);
                                break;

                            case DataSourceOperation.Insert:
                                OnInserted(args3);
                                break;

                            case DataSourceOperation.Select:
                                OnSelected(args3);
                                break;

                            case DataSourceOperation.Update:
                                OnUpdated(args3);
                                break;

                            case DataSourceOperation.SelectCount:
                                OnSelected(args3);
                                break;
                        }
                        affectedRows = args3.AffectedRows;
                    }
                }
                finally
                {
                    if ((instance != null) && disposeInstance)
                    {
                        ReleaseInstance(instance);
                        instance = null;
                    }
                }
            }
            return new ObjectDataSourceResult(returnValue, affectedRows);
        }
*/

/*
        private void ReleaseInstance(object instance)
        {
            var e = new ObjectDataSourceDisposingEventArgs(instance);
            OnObjectDisposing(e);
            if (!e.Cancel)
            {
                var disposable = instance as IDisposable;
                if (disposable != null)
                    disposable.Dispose();
            }
        }

        private static DataObjectMethodType GetMethodTypeFromOperation(DataSourceOperation operation)
        {
            switch (operation)
            {
                case DataSourceOperation.Delete:
                    return DataObjectMethodType.Delete;

                case DataSourceOperation.Insert:
                    return DataObjectMethodType.Insert;

                case DataSourceOperation.Select:
                    return DataObjectMethodType.Select;

                case DataSourceOperation.Update:
                    return DataObjectMethodType.Update;
            }
            throw new ArgumentOutOfRangeException("operation");
        }

        private IDictionary GetOutputParameters(ParameterInfo[] parameters, object[] values)
        {
            IDictionary dictionary = new OrderedDictionary(StringComparer.OrdinalIgnoreCase);
            for (int i = 0; i < parameters.Length; i++)
            {
                ParameterInfo info = parameters[i];
                if (info.ParameterType.IsByRef)
                    dictionary[info.Name] = values[i];
            }
            return dictionary;
        }
*/

        private Type GetType(string typeName)
        {
            if (typeName.Length == 0)
                throw new InvalidOperationException("ObjectDataSourceView_TypeNotSpecified " + _owner.ID);
            Type type = BuildManager.GetType(typeName, false, true);
            if (type == null)
                throw new InvalidOperationException("ObjectDataSourceView_TypeNotFound " + _owner.ID);
            return type;
        }

        private static void MergeDictionaries(ParameterCollection reference, IDictionary source, IDictionary destination)
        {
            MergeDictionaries(reference, source, destination, null);
        }

        private static void MergeDictionaries(ParameterCollection reference, IDictionary source, IDictionary destination,
                                              string parameterNameFormatString)
        {
            if (source != null)
            {
                foreach (DictionaryEntry entry in source)
                {
                    object obj2 = entry.Value;
                    Parameter parameter = null;
                    var key = (string) entry.Key;
                    if (parameterNameFormatString != null)
                        key = string.Format(CultureInfo.InvariantCulture, parameterNameFormatString, new object[] {key});
                    foreach (Parameter parameter2 in reference)
                    {
                        if (string.Equals(parameter2.Name, key, StringComparison.OrdinalIgnoreCase))
                        {
                            parameter = parameter2;
                            break;
                        }
                    }
                    if (parameter != null)
                    {
                        obj2 =
                            GetValue(obj2, parameter.DefaultValue, parameter.Type, parameter.ConvertEmptyStringToNull,
                                     true);
                    }
                    destination[key] = obj2;
                }
            }
        }

        private static object GetValue(object value, string defaultValue, TypeCode type, bool convertEmptyStringToNull,
                                       bool ignoreNullableTypeChanges)
        {
            if (type == TypeCode.DBNull)
                return DBNull.Value;
            if (convertEmptyStringToNull)
            {
                var text = value as string;
                if ((text != null) && (text.Length == 0))
                    value = null;
            }
            if (value == null)
            {
                if (convertEmptyStringToNull && string.IsNullOrEmpty(defaultValue))
                    defaultValue = null;
                if (defaultValue == null)
                    return null;
                value = defaultValue;
            }
            if ((type == TypeCode.Object) || (type == TypeCode.Empty))
                return value;
            if (ignoreNullableTypeChanges)
            {
                Type type2 = value.GetType();
                if (type2.IsGenericType && (type2.GetGenericTypeDefinition() == typeof (Nullable<>)))
                    return value;
            }
            return (value = Convert.ChangeType(value, type, CultureInfo.CurrentCulture));
        }

        #endregion

        #region events

        public event EventHandler<CurrentChangeEventArgs> CurrentChanged;
        public event EventHandler<CurrentChangingEventArgs> CurrentChanging;
        public event TableDataSourceUpdatedDataEventHandler UpdatedData;
        public event TableDataSourceUpdatingDataEventHandler UpdatingData;
        public event TableDataSourceClearingDataEventHandler ClearingData;
        public event TableDataSourceClearedDataEventHandler ClearedData;
        public event TableDataSourceErrorEventHandler Error;
        public event TableDataSourceAddedRowEventHandler AddedRow;
        public event TableDataSourceAddingRowEventHandler AddingRow;

        #endregion

        #region Nested type: ObjectDataSourceMethod

/*
        [StructLayout(LayoutKind.Sequential)]
        private struct ObjectDataSourceMethod
        {
            internal readonly MethodInfo MethodInfo;
            internal readonly DataSourceOperation Operation;
            internal readonly OrderedDictionary Parameters;
            internal readonly Type Type;

            internal ObjectDataSourceMethod(DataSourceOperation operation, Type type, MethodInfo methodInfo,
                                            OrderedDictionary parameters)
            {
                Operation = operation;
                Type = type;
                Parameters = parameters;
                MethodInfo = methodInfo;
            }
        }
*/

        #endregion

        #region Nested type: ObjectDataSourceResult

/*
        private class ObjectDataSourceResult
        {
            // Fields
            internal readonly object ReturnValue;
            internal int AffectedRows;

            // Methods
            internal ObjectDataSourceResult(object returnValue, int affectedRows)
            {
                ReturnValue = returnValue;
                AffectedRows = affectedRows;
            }
        }
*/

        #endregion
    }
}