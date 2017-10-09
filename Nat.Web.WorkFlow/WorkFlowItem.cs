using System;
using System.Collections.Generic;
using System.Data.Linq;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Nat.Web.Controls.GenerationClasses;
using Nat.Web.Controls.GenerationClasses.Filter;
using Nat.Web.Tools.Security;
using Nat.Web.Tools.WorkFlow;
using Nat.Web.WorkFlow.Properties;

namespace Nat.Web.WorkFlow
{
    using Nat.Web.Controls;

    public abstract class WorkFlowItem : IWorkFlow
    {
        private LogMonitor logMonitor;

        public bool CurrentExecutionInJob { get; set; }

        public virtual Page Page { get; set; }

        public virtual Control ControlForPostBack { get; set; }

        public virtual WorkFlow BaseWorkFlow { get; set; }

        public virtual DataContext DataContext { get; set; }

        public virtual AbstractUserControl JournalControl { get; set; }

        public virtual ExtenderAjaxControl ExtenderAjaxControl { get; set; }

        public MainPageUrlBuilder Url { get; set; }

        public abstract BaseControlInfo BaseControlInfo { get; set; }

        public abstract bool HasActions { get; }

        public Action EnsureLogicExecute { get; set; }

        public Action SaveData { get; set; }

        public Action<bool> UpdateForm { get; set; }

        public Action<object, object> UpdatingRow { get; set; }

        public abstract object SelectedValue { get; }

        public virtual bool SupportChangeEditButton
        {
            get { return false; }
        }

        public Action<string> AddErrorMessage { get; set; }

        public Action<string> AddInfoMessage { get; set; }

        public Func<IEnumerable<string>, bool> AddErrorMessages { get; set; }

        public string CurrentSID { get; set; }

        public string LogPrefix { get; set; }

        #region abstracts
        
        public abstract void InitializeFilters();
        public abstract void InitializeValidation();
        public abstract void InitializeActions();
        public abstract IEnumerable<IWorkFlowActionResult> ExecuteAction(string argument);
        public abstract IEnumerable<IWorkFlowAction> GetActions(string selectedKey);
        public abstract IEnumerable<IWorkFlowAction> GetActions(object row);
        public abstract IEnumerable<IWorkFlowAction> GetActions();
        public abstract Type GetTableType();
        public abstract void SetFilters(BaseFilterEventArgs filterArgs);
        public abstract void SetEditFilters(BaseFilterEventArgs filterArgs);
        public abstract void SetDeleteFilters(BaseFilterEventArgs filterArgs);
        public abstract void SetAddChildsFilters(BaseFilterEventArgs filterArgs);
        public abstract void SetDefaultFilter(DefaultFilters defaultFilters);
        public abstract void SetValidation(Dictionary<Control, ValidateInformation> validationInfo, string keyString);
        public abstract void SetValidation(ValidateInformationForm validationInfo, string keyString);

        public abstract void BeforeRowDeleted(object row, object contextInfo);

        public abstract void DeleteRow(object row, object contextInfo);

        public abstract void AfterRowDeleted(object row, object contextInfo);

        #endregion

        public virtual void InitializeGridColumns(BaseGridColumns gridColumns)
        {
        }

        public virtual void InitializeGroupColumns(List<List<GridHtmlGenerator.Column>> groupColumns, BaseGridColumns gridColumns)
        {
        }

        public virtual void InitializeClientManagmentControl(ICollection<BaseClientManagmentControl> managmentControls, Func<BaseClientManagmentControl> createClientManagmentControl)
        {
        }

        public virtual void InitializeDefaultFilters(DefaultFilters defaultFilters)
        {
        }

        public virtual string GetEditButton(object row)
        {
            throw new NotSupportedException();
        }

        public virtual void GetHtmlOfAction(StringBuilder sb, string selectedKey, object row, bool forCell)
        {
            using (var textWriter = new StringWriter(sb))
            using (var htmlWriter = new HtmlTextWriter(textWriter))
            {
                var actions = (row != null ? GetActions(row) : GetActions(selectedKey))
                    .Where(r => r.Roles == null || r.Roles.Length == 0 || UserRoles.IsInAnyRoles(r.Roles))
                    .OrderBy(r => r.OrderIndex)
                    .ThenBy(r => r.ActionName);
                foreach (var action in actions)
                    action.Render(htmlWriter, this, forCell, selectedKey, row);
                htmlWriter.Flush();
                textWriter.Flush();
            }
        }

        public virtual void GetStaticHtmlOfAction(StringBuilder sb)
        {
            using (var textWriter = new StringWriter(sb))
            using (var htmlWriter = new HtmlTextWriter(textWriter))
            {
                var actions = GetActions()
                    .Where(r => r.Roles == null || r.Roles.Length == 0 || UserRoles.IsInAnyRoles(r.Roles))
                    .OrderBy(r => r.OrderIndex)
                    .ThenBy(r => r.ActionName);
                foreach (var action in actions)
                    action.RenderStaticHtml(htmlWriter, this);
                htmlWriter.Flush();
                textWriter.Flush();
            }
        }

        public abstract void OnUpdating(object originalRow, object newRow);

        public abstract void OnInserting(object newRow);

        public abstract bool OnUpdated(object row, string newKey);

        public abstract bool OnInserted(object row);

        public abstract void SetSelectedValues(string value);

        #region virtuals for logic

        public virtual void InitializeAddLinks(WebControl link1, WebControl link2)
        {
        }

        public virtual void IninitializeDataContext()
        {
        }

        public virtual string CreateJobForAction(string argument, string key)
        {
            throw new NotImplementedException();
        }

        public virtual void EnsureCurrentExecutionInJob()
        {
            if (!CurrentExecutionInJob) 
                throw new WorkFlowCreateJobException();
        }

        public virtual void InsertLogic()
        {
        }

        public virtual void EditLogic()
        {
        }

        public virtual void ReadLogic()
        {
        }

        public virtual void InitControlPanels()
        {
        }

        #endregion
    }

    public abstract class WorkFlowItem<TTable, TDataContext, TRow, TKey, TStatus, TControlInfo> : WorkFlowItem, IWorkFlow<TTable, TDataContext, TRow, TKey, TStatus, TControlInfo>
        where TRow : class
        where TTable : class
        where TDataContext : DataContext
        where TControlInfo : BaseControlInfo
        where TKey : struct
    {
        #region private fields

        Func<TDataContext, TTable, TStatus> _compiledTakeStatus;
        
        Func<TDataContext, TRow, TStatus> _compiledTakeStatusForRow;
        
        Func<TDataContext, TRow, bool> _compiledStatusIsNullForRow;
        
        private bool? _startingProcess;

        private List<string> selectedValues;

        #endregion


        #region properties

        protected Dictionary<TStatus, WorkFlowFilter<TTable, TDataContext>> Filters { get; set; }
        protected Dictionary<TStatus, WorkFlowFilter<TTable, TDataContext>> EditFilters { get; set; }
        protected Dictionary<TStatus, WorkFlowFilter<TTable, TDataContext>> DeleteFilters { get; set; }
        protected Dictionary<TStatus, WorkFlowFilter<TTable, TDataContext>> AddChildsFilters { get; set; }
        protected Dictionary<TStatus, List<WorkFlowValidation<TControlInfo>>> Validation { get; set; }
        protected Dictionary<TStatus, Dictionary<string, WorkFlowAction<TTable, TDataContext, TRow>>> Actions { get; set; }

        public virtual TDataContext DB { get; set; }

        public virtual TControlInfo ControlInfo { get; set; }

        public override DataContext DataContext
        {
            get { return DB; }
            set { DB = (TDataContext)value; }
        }

        public override BaseControlInfo BaseControlInfo
        {
            get { return ControlInfo; }
            set { ControlInfo = (TControlInfo)value; }
        }

        public Func<TRow> GetRenderRow { get; set; }

        public virtual TRow RenderRow
        {
            get
            {
                if (GetRenderRow != null)
                    return GetRenderRow();
                throw new NotImplementedException();
            }
        }

        public Func<TTable> GetRenderItem { get; set; }

        public virtual TTable RenderItem
        {
            get
            {
                if (GetRenderItem != null)
                    return GetRenderItem();
                throw new NotImplementedException();
            }
        }

        public virtual TKey? SelectedValueKey
        {
            get { return (TKey?)SelectedValue; }
        }

        public override object SelectedValue
        {
            get { return JournalControl.SelectedValue; }
        }

        #endregion


        #region abstract 

        public abstract Expression<Func<TDataContext, TTable, TStatus>> TakeStatus { get; }
        public abstract Expression<Func<TDataContext, TTable, bool>> StatusIsNull { get; }
        public abstract Expression<Func<TRow, TTable>> TakeItem { get; }

        protected abstract Expression<Func<TDataContext, TKey, TTable>> GetTableRow();
        protected abstract TKey? GetKey(string key);

        #endregion


        #region virtual for logic

        protected virtual void InitializeValidation(Dictionary<TStatus, List<WorkFlowValidation<TControlInfo>>> validation)
        {
        }

        protected virtual void InitializeFilters(Dictionary<TStatus, WorkFlowFilter<TTable, TDataContext>> filters)
        {
        }

        protected virtual void InitializeEditFilters(Dictionary<TStatus, WorkFlowFilter<TTable, TDataContext>> filters)
        {
        }

        protected virtual void InitializeDeleteFilters(Dictionary<TStatus, WorkFlowFilter<TTable, TDataContext>> filters)
        {
        }

        protected virtual void InitializeAddChildsFilters(Dictionary<TStatus, WorkFlowFilter<TTable, TDataContext>> filters)
        {
        }

        protected virtual void InitializeActions(Dictionary<TStatus, List<WorkFlowAction<TTable, TDataContext, TRow>>> actions)
        {
        }

        protected virtual void OnUpdating(TTable originalRow, TTable newRow)
        {
        }

        protected virtual void OnInserting(TTable newRow)
        {
        }

        protected virtual bool OnUpdated(TTable row, TKey? newKey)
        {
            return false;
        }

        protected virtual bool OnInserted(TTable row)
        {
            return false;
        }
        
        protected virtual void OnBeforeRowDeleted(TRow row, DeleteRowContext<TTable, TDataContext, TRow, TKey> contextInfo)
        {
        }

        protected virtual void DeleteRow(TRow row, DeleteRowContext<TTable, TDataContext, TRow, TKey> contextInfo)
        {
        }

        protected virtual void OnAfterRowDeleted(TRow row, DeleteRowContext<TTable, TDataContext, TRow, TKey> contextInfo)
        {
        }

        #endregion


        #region virtuals

        public virtual Expression<Func<TDataContext, TRow, bool>> StatusIsNullForRow
        {
            get
            {
                var dbParam = Expression.Parameter(typeof(TDataContext), "db");
                var rowParam = Expression.Parameter(typeof(TRow), "row");
                Expression exp = Expression.Invoke(TakeItem, rowParam);
                exp = Expression.Invoke(StatusIsNull, dbParam, exp);
                return Expression.Lambda<Func<TDataContext, TRow, bool>>(exp, dbParam, rowParam);

            }
        }

        public virtual Expression<Func<TDataContext, TRow, TStatus>> TakeStatusForRow
        {
            get
            {
                var dbParam = Expression.Parameter(typeof(TDataContext), "db");
                var rowParam = Expression.Parameter(typeof(TRow), "row");
                Expression exp = Expression.Invoke(TakeItem, rowParam);
                exp = Expression.Invoke(TakeStatus, dbParam, exp);
                return Expression.Lambda<Func<TDataContext, TRow, TStatus>>(exp, dbParam, rowParam);
            }
        }

        protected virtual TStatus GetStatus(string keyString)
        {
            if (string.IsNullOrEmpty(keyString)) return GetStartingStatusOrEmpty();
            var expTableRow = GetTableRow();
            var dbParam = Expression.Parameter(typeof(TDataContext), "db");
            var keyParam = Expression.Parameter(typeof(TKey), "key");
            var exp = Expression.Invoke(expTableRow, dbParam, keyParam);
            exp = Expression.Invoke(TakeStatus, dbParam, exp);
            var function = GetCacheFunction(exp);

            if (function == null)
            {
                var lambda = Expression.Lambda<Func<TDataContext, TKey, TStatus>>(exp, dbParam, keyParam);
                function = CompiledQuery.Compile(lambda);
                SetCacheFunction(function, exp);
            }
            var key = GetKey(keyString);
            if (key == null) return GetStartingStatusOrEmpty();
            var status = function(DB, key.Value);
            return status == null ? GetStartingStatusOrEmpty() : status;
        }

        protected virtual TStatus GetStartingStatusOrEmpty()
        {
            if (StartingProcess) return StartStatus;
            return GetEmptyStatus();
        }

        protected virtual TStatus StartStatus { get; set; }

        protected virtual bool StartingProcess
        {
            get
            {
                if (_startingProcess == null)
                    _startingProcess = InitializeProcess();
                return _startingProcess.Value;
            }
            set { _startingProcess = value; }
        }

        protected virtual bool InitializeProcess()
        {
            return false;
        }

        protected virtual TStatus GetStatus(TTable row)
        {
            if (_compiledTakeStatus == null) _compiledTakeStatus = TakeStatus.Compile();
            return _compiledTakeStatus(DB, row);
        }

        protected virtual TStatus GetStatus(TRow row)
        {
            if (_compiledTakeStatusForRow == null) _compiledTakeStatusForRow = TakeStatusForRow.Compile();
            if (_compiledStatusIsNullForRow == null) _compiledStatusIsNullForRow = StatusIsNullForRow.Compile();
            return _compiledStatusIsNullForRow(DB, row) ? GetEmptyStatus() : _compiledTakeStatusForRow(DB, row);
        }

        protected virtual TStatus GetStatus(object rowObject)
        {
            if (rowObject == null)
                throw new ArgumentNullException("rowObject");

            var tableRow = rowObject as TTable;
            if (tableRow != null)
                return GetStatus(tableRow);
            var row = rowObject as TRow;
            if (row != null)
                return GetStatus(row);
            throw new ArgumentException("type of parameter rowObject is incorect");
        }

        protected virtual void AddAction(Dictionary<TStatus, List<WorkFlowAction<TTable, TDataContext, TRow>>> actions, WorkFlowAction<TTable, TDataContext, TRow> action, params TStatus[] statuses)
        {
            if (statuses == null || statuses.Length == 0)
                statuses = new [] {GetEmptyStatus()};
            foreach (var status in statuses)
            {
                 List<WorkFlowAction<TTable, TDataContext, TRow>> coll;
                 if (actions.ContainsKey(status))
                     coll = actions[status];
                 else
                     actions[status] = coll = new List<WorkFlowAction<TTable, TDataContext, TRow>>();
                coll.Add(action);
            }
        }

        protected virtual void AddFilter(Dictionary<TStatus, WorkFlowFilter<TTable, TDataContext>> filters,  WorkFlowFilter<TTable, TDataContext> filter, params TStatus[] statuses)
        {
            foreach (var status in statuses)
                filters.Add(status, filter);
        }

        protected virtual void AddValidation(Dictionary<TStatus, List<WorkFlowValidation<TControlInfo>>> validation, WorkFlowValidation<TControlInfo> workFlowValidation, params TStatus[] statuses)
        {
            workFlowValidation.BaseWorkFlowItem = this;
            workFlowValidation.Info = ControlInfo;
            foreach (var status in statuses)
            {
                List<WorkFlowValidation<TControlInfo>> list;
                if (validation.ContainsKey(status))
                    list = validation[status];
                else
                    validation[status] = list = new List<WorkFlowValidation<TControlInfo>>();
                list.Add(workFlowValidation);
            }
        }

        protected virtual TStatus GetEmptyStatus()
        {
            return default(TStatus);
        }

        #endregion


        #region functions

        protected virtual void GetFilters(BaseFilterEventArgs<TTable, TDataContext> args,
                                          Dictionary<TStatus, WorkFlowFilter<TTable, TDataContext>> filters)
        {
            Func<FilterDataArgs, Expression> resultFilters = null;
            //проходим по фильтрам группируя одинаковые для разных статусов
            foreach (var filter in filters.GroupBy(r => r.Value))
            {
                var filterArgs = new BaseFilterEventArgs<TTable, TDataContext>(args._url, args.QueryParameters)
                    {
                        TypeOfData = args.TypeOfData,
                    };
                //получаем фильтры
                filter.Key.GetFilters(filterArgs);
                if (!filterArgs.HasFilter())
                    continue;

                //получаем список статусов для данного фильтра
                var keys = filter.Select(r => r.Key).ToList();

                Expression<Func<TDataContext, TTable, bool>> expression;
                //создаем выражение для одного статуса или калекции статусов
                if (keys.Count == 1)
                {
                    var status =
                        Expression.Lambda<Func<TStatus>>(args.QueryParameters.GetExpression(keys[0].ToString(), keys[0]));
                    expression = GetExprssion((value, s) => value.Equals(s), status);
                }
                else
                {
                    var statuses = Expression.Lambda<Func<List<TStatus>>>(args.QueryParameters.GetExpression(
                        string.Join("_", keys.Select(r => r.ToString()).ToArray()), keys));
                    expression = GetExprssion((values, s) => values.Contains(s), statuses);
                }
                //добавляем условие по стутусу в фильтр
                filterArgs.AddFilter(expression);
                //объединяем фильтры через Or
                var resultFilters1 = resultFilters;
                resultFilters = resultFilters1 == null
                                    ? filterDataArgs => filterArgs.GetExpression(filterDataArgs)
                                    : (Func<FilterDataArgs, Expression>)
                                      (r => Expression.Or(resultFilters1(r), filterArgs.GetExpression(r)));
                //GetFiltersWithUnion(resultFilters, filterArgs, (e1, e2) => Expression.Or(e1, e2));
            }
            //объединение фильтров через And
            if (resultFilters != null)
            {
                #region Добавляем условие Or если статуса нету, т.е. процесс не был запущен

                var filterArgs = new BaseFilterEventArgs<TTable, TDataContext>(args._url, args.QueryParameters)
                    {
                        TypeOfData = args.TypeOfData,
                    };
                filterArgs.AddFilter(StatusIsNull);

                var resultFilters1 = resultFilters;
                resultFilters = r => Expression.Or(resultFilters1(r), filterArgs.GetExpression(r));

                #endregion

                args.Filters = args.Filters == null
                                   ? resultFilters
                                   : r => Expression.And(resultFilters(r), args.Filters(r));
                                   //GetFiltersWithUnion(resultFilters, args.Filters, (e1, e2) => Expression.And(e1, e2));
            }
        }

        #endregion


        #region help functions
        /*
        private Func<FilterDataArgs, Expression> GetFiltersWithUnion(
            Func<FilterDataArgs, Expression> filters,
            BaseFilterEventArgs<TTable> filterArgs, Func<Expression, Expression, Expression> unionExp)
        {
            return GetFiltersWithUnion(filters,
                                       filterDataArgs => filterArgs.FilterData<TTable>(filterDataArgs),
                                       unionExp);
        }

        private Func<FilterDataArgs, Expression> GetFiltersWithUnion(
            Func<FilterDataArgs, Expression> filters1,
            Func<FilterDataArgs, Expression> filters2,
            Func<Expression, Expression, Expression> unionExp)
        {
            var param = Expression.Parameter(typeof (FilterDataArgs), "filterDataArgs");
            var exp1 = Expression.Invoke(filters1, param);
            var exp2 = Expression.Invoke(filters2, param);
            var exp = Expression.Invoke(unionExp, exp1, exp2);
            return Expression.Lambda<Func<FilterDataArgs, Expression>>(exp, param);
        }*/
        private Expression<Func<TDataContext, TTable, bool>> GetExprssion<T>(
            Expression<Func<T, TStatus, bool>> statusEqualsExp, Expression<Func<T>> values)
        {
            var db = Expression.Parameter(typeof (TDataContext), "db");
            var t = Expression.Parameter(typeof (TTable), "t");
            var statusExp = Expression.Invoke(TakeStatus, db, t);
            var exp = Expression.Invoke(statusEqualsExp, Expression.Invoke(values), statusExp);
            return Expression.Lambda<Func<TDataContext, TTable, bool>>(exp, db, t);
        }

        private Expression<Func<TDataContext, TTable, bool>> GetExprssionStatusIsNull()
        {
            var db = Expression.Parameter(typeof(TDataContext), "db");
            var t = Expression.Parameter(typeof(TTable), "t");
            var statusExp = Expression.Invoke(TakeStatus, db, t);
            var exp = Expression.Equal(statusExp, Expression.Constant(null, typeof(TStatus)));
            return Expression.Lambda<Func<TDataContext, TTable, bool>>(exp, db, t);
        }

        private void SetCacheFunction(Func<TDataContext, TKey, TStatus> func, Expression expression)
        {
            if (HttpContext.Current == null) return;
            var key = GetCacheKey(expression, typeof(TStatus));
            HttpContext.Current.Cache[key] = func;
        }

        private Func<TDataContext, TKey, TStatus> GetCacheFunction(Expression expression)
        {
            var key = GetCacheKey(expression, typeof(TStatus));
            if (HttpContext.Current == null) return null;
            return (Func<TDataContext, TKey, TStatus>)HttpContext.Current.Cache[key];
        }

        private static string GetCacheKey(Expression expression, Type resultType)
        {
            return string.Format("WorkFlowItem#{0}#{1}#{2}#{3}#{4}", expression, typeof(TKey).FullName, typeof(TTable).FullName, typeof(TDataContext), resultType.FullName);
        }

        #endregion


        #region EnsureInitialized...

        protected bool InitializedActions { get; set; }
        protected bool InitializedFilters { get; set; }
        protected bool InitializedValidation { get; set; }

        protected void EnsureInitializedActions()
        {
            if (InitializedActions) return;
            InitializedActions = true;
            InitializeActions();
        }

        protected void EnsureInitializedFilters()
        {
            if (InitializedFilters) return;
            InitializedFilters = true;
            InitializeFilters();
        }

        protected void EnsureInitializedValidation()
        {
            if (InitializedValidation) return;
            InitializedValidation = true;
            InitializeValidation();
        }

        #endregion


        #region overrides

        public override Type GetTableType()
        {
            return typeof (TTable);
        }

        public override void InitializeFilters()
        {
            InitializeFilters(Filters = new Dictionary<TStatus, WorkFlowFilter<TTable, TDataContext>>());
            InitializeEditFilters(EditFilters = new Dictionary<TStatus, WorkFlowFilter<TTable, TDataContext>>());
            InitializeDeleteFilters(DeleteFilters = new Dictionary<TStatus, WorkFlowFilter<TTable, TDataContext>>());
            InitializeAddChildsFilters(AddChildsFilters = new Dictionary<TStatus, WorkFlowFilter<TTable, TDataContext>>());
        }

        public override void InitializeValidation()
        {
            InitializeValidation(Validation = new Dictionary<TStatus, List<WorkFlowValidation<TControlInfo>>>());
        }

        public override void InitializeActions()
        {
            Actions = new Dictionary<TStatus, Dictionary<string, WorkFlowAction<TTable, TDataContext, TRow>>>();
            var actions = new Dictionary<TStatus, List<WorkFlowAction<TTable, TDataContext, TRow>>>();
            InitializeActions(actions);
            foreach (var action in actions)
            {
                if (!Actions.ContainsKey(action.Key))
                    Actions[action.Key] = new Dictionary<string, WorkFlowAction<TTable, TDataContext, TRow>>();
                foreach (var flowAction in action.Value)
                    Actions[action.Key][flowAction.ArgumentName] = flowAction;
            }
        }

        public override IEnumerable<IWorkFlowActionResult> ExecuteAction(string argument)
        {
            EnsureInitializedActions();
            var split = argument.Split(':');
            var key = split.Length < 2 ? string.Empty : split[1];
            var actionName = split[0];
            try
            {
                if (string.IsNullOrEmpty(key))
                {
                    if (selectedValues == null && string.IsNullOrEmpty(JournalControl.SelectedValues)) 
                        return new IWorkFlowActionResult[0];

                    var results = new List<IWorkFlowActionResult>();
                    var values = selectedValues ?? JournalControl.GetSelectedValues();
                    foreach (var selectedValue in values) 
                        results.AddRange(ExecuteAction(split, actionName, selectedValue));
                    return results;
                }

                return ExecuteAction(split, actionName, key);
            }
            catch (WorkFlowCreateJobException)
            {
                var url = CreateJobForAction(argument, key);
                string message = string.Format(Resources.SAddedJob, url);
                return new[]
                    {
                        new WorkFlowActionResult
                            {
                                Successfull = true,
                                ActionExecuted = true,
                                ResultMessage = message,
                            }
                    };
            }
        }

        protected virtual IEnumerable<IWorkFlowActionResult> ExecuteAction(string[] split, string actionName, string key)
        {
            var status = GetStatus(key);
            if (!Actions.ContainsKey(status)) return new IWorkFlowActionResult[0];
            var actions = Actions[status];
            if (!actions.ContainsKey(actionName)) return new IWorkFlowActionResult[0];
            var action = actions[actionName];

            var result = new WorkFlowActionResult { ActionExecuted = true };

            if (!CurrentExecutionInJob && action.Roles != null && action.Roles.Length > 0 && !UserRoles.IsInAnyRoles(action.Roles))
            {
                result.ResultMessage = string.Format(Resources.SAccessDenied, action.ActionName);
                return new[] { result };
            }

            if (!action.AutoInitializeTransaction)
                result.Successfull = action.ExecuteAction(key, split, (TDataContext)DataContext, this);
            else
            {
                try
                {
                    DataContext.Connection.Open();
                    using (var transaction = DataContext.Connection.BeginTransaction())
                    {
                        DataContext.Transaction = transaction;
                        result.Successfull = action.ExecuteAction(key, split, (TDataContext)DataContext, this);
                        if (result.Successfull) transaction.Commit();
                        else transaction.Rollback();
                        DataContext.Transaction = null;
                    }
                }
                finally
                {
                    DataContext.Connection.Close();
                }
            }

            if (!string.IsNullOrEmpty(action.ResultMessage))
                result.ResultMessage = action.ResultMessage;
            else
            {
                var format = result.Successfull ? Resources.SSucessfullAction : Resources.SFailedAction;
                result.ResultMessage = string.Format(format, action.ActionName);
            }

            return new[] { result };
        }

        public override IEnumerable<IWorkFlowAction> GetActions(string selectedKey)
        {
            EnsureInitializedActions();
            var status = GetStatus(selectedKey);
            var multipleActions = Actions
                .SelectMany(r => r.Value.Values)
                .Select(r => (IWorkFlowAction)r)
                .Where(r => r.MultipleSelect);
            if (!Actions.ContainsKey(status))
                return multipleActions;
            return Actions[status].Select(r => (IWorkFlowAction)r.Value).Union(multipleActions).Distinct();
        }

        public override IEnumerable<IWorkFlowAction> GetActions(object row)
        {
            EnsureInitializedActions();
            var status = GetStatus(row);
            if (!Actions.ContainsKey(status))
                return new IWorkFlowAction[0];
            return Actions[status].Select(r => (IWorkFlowAction)r.Value);
        }

        public override void OnInserting(object newRow)
        {
            OnInserting((TTable)newRow);
        }

        public override void OnUpdating(object originalRow, object newRow)
        {
            OnUpdating((TTable)originalRow, (TTable)newRow);
        }

        public override bool OnInserted(object row)
        {
            return OnInserted((TTable)row);
        }

        public override bool OnUpdated(object row, string newKey)
        {
            return OnUpdated((TTable)row, GetKey(newKey));
        }
        
        public override void SetDefaultFilter(DefaultFilters defaultFilters)
        {
            InitializeDefaultFilters(defaultFilters);
        }

        public override void SetFilters(BaseFilterEventArgs filterArgs)
        {
            EnsureInitializedFilters();
            GetFilters((BaseFilterEventArgs<TTable, TDataContext>)filterArgs, Filters);
        }

        public override void SetEditFilters(BaseFilterEventArgs filterArgs)
        {
            EnsureInitializedFilters();
            GetFilters((BaseFilterEventArgs<TTable, TDataContext>)filterArgs, EditFilters);
        }

        public override void SetDeleteFilters(BaseFilterEventArgs filterArgs)
        {
            EnsureInitializedFilters();
            GetFilters((BaseFilterEventArgs<TTable, TDataContext>)filterArgs, DeleteFilters);
        }

        public override void SetAddChildsFilters(BaseFilterEventArgs filterArgs)
        {
            EnsureInitializedFilters();
            GetFilters((BaseFilterEventArgs<TTable, TDataContext>)filterArgs, AddChildsFilters);
        }

        public override void SetValidation(Dictionary<Control, ValidateInformation> validationInfo, string keyString)
        {
            EnsureInitializedValidation();
            var status = GetStatus(keyString);
            if (!Validation.ContainsKey(status)) return;
            var validation = Validation[status];
            foreach (var workFlowValidation in validation)
                workFlowValidation.InitializeValidations(validationInfo);
        }

        public override void SetValidation(ValidateInformationForm validationInfo, string keyString)
        {
            EnsureInitializedValidation();
            var status = GetStatus(keyString);
            if (!Validation.ContainsKey(status)) return;
            var validation = Validation[status];
            foreach (var workFlowValidation in validation)
                workFlowValidation.InitializeValidations(validationInfo);
        }

        public override void BeforeRowDeleted(object row, object contextInfo)
        {
            OnBeforeRowDeleted((TRow)row, ((DeleteRowContext<TKey>)contextInfo) as DeleteRowContext<TTable, TDataContext, TRow, TKey>);
        }

        public override void DeleteRow(object row, object contextInfo)
        {
            DeleteRow((TRow)row, ((DeleteRowContext<TKey>)contextInfo) as DeleteRowContext<TTable, TDataContext, TRow, TKey>);
        }

        public override void AfterRowDeleted(object row, object contextInfo)
        {
            OnAfterRowDeleted((TRow)row, ((DeleteRowContext<TKey>)contextInfo) as DeleteRowContext<TTable, TDataContext, TRow, TKey>);
        }

        public override void SetSelectedValues(string value)
        {
            if (selectedValues == null)
                selectedValues = new List<string>();
            selectedValues.Add(value);
        }

        public override bool HasActions
        {
            get
            {
                InitializeActions();
                return Actions.Count > 0;
            }
        }

        public override IEnumerable<IWorkFlowAction> GetActions()
        {
            return Actions.Values.SelectMany(r => r.Values.Cast<IWorkFlowAction>()).Distinct();
        }

        #endregion
    }
}