using System.Linq.Expressions;

namespace Nat.Web.WorkFlow.Data
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using System.Web.UI;
    using Nat.Web.Controls;
    using Nat.Web.Controls.GenerationClasses;
    using Nat.Web.Controls.GenerationClasses.Data;
    using Nat.Web.Tools;

    public abstract class WorkFlowDataSourceView : DataSourceView, IDataSourceView2, IDataSourceViewGetName
    {
        protected WorkFlowDataSourceView(WorkFlowDataSource owner, string viewName)
            : base(owner, viewName)
        {
            Owner = owner;
        }

        protected WorkFlowDataSource Owner { get; private set; }

        public abstract IQueryable<IDataRow> GetSelectIRow(string queryParameters);
        public abstract IQueryable<IDataCodeRow> GetSelectICodeRow(string queryParameters);
        public abstract bool CheckPermit();
        public abstract bool SupportSelectICodeRow { get; }
        public abstract string SelectedRowKey { get; set; }
        public abstract Type KeyType { get; }
        public abstract object GetKey(string key);
        public abstract object GetKey(object key);
        public abstract string GetName(string key);
        public abstract string GetName(object key);
        public abstract string GetCode(string key);
        public abstract string GetCode(object key);
        public abstract object GetTableParameterValue(string key, string tableParameterName);
    }

    public abstract class WorkFlowDataSourceView<TStatus> : WorkFlowDataSourceView
    {
        private Dictionary<TStatus, WorkFlowDataRow<TStatus>> _dictionary;

        public const string FilterByPrimaryKeyEquals = "Status.Equals";
        public const string FilterByPrimaryKeyNotEquals = "Status.NotEquals";
        public const string FilterByPrimaryKeyEqualsCollection = "Status.EqualsCollection";
        public const string FilterByPrimaryKeyNotEqualsCollection = "Status.NotEqualsCollection";

        protected WorkFlowDataSourceView()
            : base(null, "Defalut")
        {
        }

        protected WorkFlowDataSourceView(WorkFlowDataSource owner, string viewName)
            : base(owner, viewName)
        {
        }

        protected WorkFlowDataSourceView(IDataSource owner, string viewName)
            : base((WorkFlowDataSource)owner, viewName)
        {
        }

        protected Dictionary<TStatus, WorkFlowDataRow<TStatus>> Dictionary
        {
            get
            {
                if (_dictionary == null)
                {
                    InitializeDictionary();
                    if (_dictionary == null)
                        throw new Exception("Dictionary of datasource is not initialized");
                }

                return _dictionary;
            }

            set { _dictionary = value; }
        }

        protected void Add(WorkFlowDataRow<TStatus> item)
        {
            Dictionary[item.id] = item;
        }

        protected void Add(params WorkFlowDataRow<TStatus>[] items)
        {
            foreach (var item in items)
                Dictionary[item.id] = item;
        }

        protected abstract void InitializeDictionary();

        protected virtual void OnSelecting(WorkFlowDataSourceSelectingEventArgs e)
        {
            if (Owner != null)
                Owner.OnSelecting(e);
        }

        protected override IEnumerable ExecuteSelect(DataSourceSelectArguments arguments)
        {
            var args = new WorkFlowDataSourceSelectingEventArgs();
            OnSelecting(args);

            var data = Dictionary.Values.AsQueryable();
            if (args.BrowseFilterParameters != null)
                data = Filter(args.BrowseFilterParameters, data);

            if (arguments.RetrieveTotalRowCount)
                arguments.TotalRowCount = data.Count();

            if (arguments.MaximumRows > 0)
                data = data.Skip(arguments.StartRowIndex).Take(arguments.MaximumRows);

            return OrderBy(data);
        }

        protected virtual IQueryable<WorkFlowDataRow<TStatus>> OrderBy(IQueryable<WorkFlowDataRow<TStatus>> data)
        {
            Expression<Func<WorkFlowDataRow<TStatus>, string>> orderBy = LocalizationHelper.IsCultureKZ
                ? (Expression < Func<WorkFlowDataRow<TStatus>, string>>)(r => r.nameKz)
                : (r => r.nameRu);
            return data.OrderBy(orderBy);
        }

        private IQueryable<WorkFlowDataRow<TStatus>> Filter(
            BrowseFilterParameters browseFilterParameters,
            IQueryable<WorkFlowDataRow<TStatus>> data)
        {
            foreach (var parameterValue in browseFilterParameters.GetParameterValues())
                data = LinqFilterGenerator.GenerateFilter(data, "Equals", parameterValue.Key, parameterValue.Value, null);

            return data;
        }

        public override IQueryable<IDataRow> GetSelectIRow(string queryParameters)
        {
            return Dictionary.Select(r => (IDataRow)r.Value).AsQueryable();
        }

        public override IQueryable<IDataCodeRow> GetSelectICodeRow(string queryParameters)
        {
            throw new NotImplementedException();
        }

        public override bool SupportSelectICodeRow { get { return false; } }
        public override string SelectedRowKey { get; set; }
        public override Type KeyType { get { return typeof(TStatus); } }

        public override object GetKey(object key)
        {
            if (key is TStatus) return key;
            return GetKey(Convert.ToString(key));
        }

        public override string GetName(string key)
        {
            if (string.IsNullOrEmpty(key))
                return null;
            var keyValue = GetKey(key);
            var defaultStatus = default(TStatus);
            if (keyValue == null || (keyValue.Equals(defaultStatus) && (defaultStatus == null || !Dictionary.ContainsKey(defaultStatus))))
                return null;
            var statusKey = (TStatus) keyValue;
            if (statusKey.Equals(defaultStatus) && (defaultStatus == null || !Dictionary.ContainsKey(defaultStatus)) || "".Equals(statusKey))
                return null;
            if (Dictionary.ContainsKey(statusKey))
                return Dictionary[statusKey].Name;
            return statusKey.ToString();
        }

        public override string GetName(object key)
        {
            var defaultStatus = default(TStatus);
            if (key == null || (key.Equals(defaultStatus) && (defaultStatus == null || !Dictionary.ContainsKey(defaultStatus))))
                return null;
            var keyValue = GetKey(key);
            var statusKey = (TStatus)keyValue;
            if (statusKey == null || statusKey.Equals(defaultStatus) && (defaultStatus == null || !Dictionary.ContainsKey(defaultStatus)) || "".Equals(statusKey))
                return null;
            if (Dictionary.ContainsKey(statusKey))
                return Dictionary[statusKey].Name;
            return statusKey.ToString();
        }

        public override string GetCode(string key)
        {
            throw new NotImplementedException();
        }

        public override string GetCode(object key)
        {
            throw new NotImplementedException();
        }

        public override object GetTableParameterValue(string key, string tableParameterName)
        {
            throw new NotImplementedException();
        }
        
        public WorkFlowDataRow<TStatus> Validate(TStatus value, RecordValidateArgs args)
        {
            args.Cancel = !Dictionary.ContainsKey(value);
            return args.Cancel ? null : Dictionary[value];
        }
    }
}