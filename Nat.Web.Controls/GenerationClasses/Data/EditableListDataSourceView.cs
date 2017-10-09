using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Linq;
using System.Linq;
using System.Linq.Expressions;
using System.Web.UI;
using Nat.Web.Tools;

namespace Nat.Web.Controls.GenerationClasses.Data
{
    using System.Text;

    using Nat.Tools.System;
    using Nat.Web.Controls.GenerationClasses.BaseJournal;
    using Nat.Web.Controls.SelectValues;

    public abstract class EditableListDataSourceView : DataSourceView
    {
        protected readonly EditableListDataSource Owner;

        protected EditableListDataSourceView(EditableListDataSource owner, string viewName)
            : base(owner, viewName)
        {
            Owner = owner;
        }

        public event EventHandler<SelectedEventArgs> SelectedRow;

        public string ValuesDataSourceID { get; set; }

        protected virtual void OnSelectedRow(SelectedEventArgs e)
        {
            var handler = SelectedRow;
            if (handler != null)
                handler(this, e);
        }

        public class SelectedItems
        {
            public string id
            {
                get
                {
                    var key = string.IsNullOrEmpty(SelectedKey) ? string.Empty : Convert.ToBase64String(Encoding.UTF8.GetBytes(SelectedKey));
                    var value = string.IsNullOrEmpty(Value) ? string.Empty : Convert.ToBase64String(Encoding.UTF8.GetBytes(Value));
                    return key + "," + value;
                }
            }

            public static KeyValuePair<string, string> GetValues(string id)
            {
                var split = id.Split(',');
                var key = string.IsNullOrEmpty(split[0])
                                 ? string.Empty
                                 : Encoding.UTF8.GetString(Convert.FromBase64String(split[0]));
                var value = string.IsNullOrEmpty(split[1])
                                ? string.Empty
                                : Encoding.UTF8.GetString(Convert.FromBase64String(split[1]));

                return new KeyValuePair<string, string>(key, value);
            }

            public IDataRow ValueItem { get; set; }
            public string SelectedKey { get; set; }
            public string Value { get; set; }
            public string Name { get; set; }
            public bool Selected { get; set; }
            public bool Deleted { get; set; }
            public bool Enabled { get; set; }
        }

        public class SelectedEventArgs : EventArgs
        {
            public SelectedEventArgs(string value, bool selected)
            {
                Value = value;
                Selected = selected;
            }

            public string Value { get; set; }

            public bool Selected { get; set; }
        }

        public event EventHandler<InsertingEventArgs> Inserting;

        public event EventHandler<DeletingEventArgs> Deleting;

        public event EventHandler<UpdatingEventArgs> Updating;

        public event EventHandler<InsertedEventArgs> Inserted;

        public event EventHandler<DeletedEventArgs> Deleted;

        public event EventHandler<UpdatedEventArgs> Updated;
        
        public event EventHandler<EnabledEventArgs> EnabledRow;
        
        protected virtual void OnDeleted(DeletedEventArgs e)
        {
            var handler = Deleted;
            if (handler != null) handler(this, e);
        }

        protected virtual void OnUpdated(UpdatedEventArgs e)
        {
            Updated?.Invoke(this, e);
        }

        protected virtual void OnInserted(InsertedEventArgs e)
        {
            var handler = Inserted;
            if (handler != null) handler(this, e);
        }

        protected virtual void OnDeleting(DeletingEventArgs e)
        {
            var handler = Deleting;
            if (handler != null) handler(this, e);
        }

        protected virtual void OnUpdating(UpdatingEventArgs e)
        {
            Updating?.Invoke(this, e);
        }

        protected virtual void OnInserting(InsertingEventArgs e)
        {
            var handler = Inserting;
            if (handler != null) handler(this, e);
        }

        protected virtual void OnEnabledRow(EnabledEventArgs e)
        {
            var handler = EnabledRow;
            if (handler != null) handler(this, e);
        }

        public class InsertingEventArgs : CancelEventArgs
        {
            public DataContext DB { get; set; }
            public object Row { get; set; }
            public bool CancelSubmitChanges { get; set; }
            public bool CancelInsertItem { get; set; }
        }

        public class DeletingEventArgs : CancelEventArgs
        {
            public DataContext DB { get; set; }
            public List<object> Row { get; set; }
            public bool CancelSubmitChanges { get; set; }
            public bool CancelDeleteItem { get; set; }
        }

        public class UpdatingEventArgs : CancelEventArgs
        {
            public DataContext DB { get; set; }
            public object Row { get; set; }
            public bool CancelSubmitChanges { get; set; }
            public bool CancelUpdateItem { get; set; }
        }

        public class InsertedEventArgs : EventArgs
        {
            public object Row { get; set; }
        }

        public class DeletedEventArgs : EventArgs
        {
            public List<object> Rows { get; set; }
        }

        public class UpdatedEventArgs : EventArgs
        {
            public object Row { get; set; }
        }

        public class EnabledEventArgs : EventArgs
        {
            public EnabledEventArgs(IDataRow rowValues, bool enabled)
            {
                RowValues = rowValues;
                Enabled = enabled;
            }

            public IDataRow RowValues { get; set; }
            public bool Enabled { get; set; }
        }
    }

    public abstract class EditableListDataSourceView<TKeyValues> : EditableListDataSourceView
        where TKeyValues : struct
    {
        protected EditableListDataSourceView(EditableListDataSource owner, string viewName)
            : base(owner, viewName)
        {
        }

        public BaseDataSourceView<TKeyValues> ValuesDataSourceView { get; set; }
        
        public bool IsFile { get; set; }
        
        public string ValueDataField { get; set; }
        
        public Type ValuesTableType
        {
            get { return GetValuesDataSource().TableType; }
        }

        public Type ValuesRowType
        {
            get { return GetValuesDataSource().RowType; }
        }
        
        protected override IEnumerable ExecuteSelect(DataSourceSelectArguments arguments)
        {
            var valuesDataSource = GetValuesDataSource();
            var data = OrderValuesQuery(valuesDataSource.GetSelectIQueryable(), arguments.SortExpression);
            if (IsFile)
            {
                return data
                    .OfType<IDataRow>()
                    .Select(r => new { r.Value, Name = LocalizationHelper.IsCultureKZ ? r.nameKz : r.nameRu })
                    .Select(
                        r => new SelectedItems
                                 {
                                     SelectedKey = r.Value,
                                     Name = r.Name,
                                     Deleted = false,
                                     Selected = true,
                                     Enabled = true,
                                 });
            }
            return data.OfType<IDataRow>().ToList().Select(GetSelectedItems);
        }

        protected virtual IQueryable OrderValuesQuery(IQueryable query, string sortExpression)
        {
            if (string.IsNullOrEmpty(sortExpression)) return query;
            var queryExp = query.Expression;
            var str = "OrderBy";
            var str2 = "OrderByDescending";
            foreach (string orderBy in sortExpression.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
            {
                var split = orderBy.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                var ascending = true;
                if (split.Length == 2)
                    ascending = split[1].Equals("Asc", StringComparison.OrdinalIgnoreCase);

                var orderByColumn = split[0].StartsWith("RowValues.") ? split[0].Substring(10) : split[0];
                var param = Expression.Parameter(this.ValuesRowType, "c");
                Type fieldType;
                var property = LinqFilterGenerator.GetProperty(ValuesRowType, orderByColumn, param, out fieldType);
                queryExp = Expression.Call(
                    typeof(Queryable),
                    ascending ? str : str2,
                    new[] { ValuesRowType, fieldType },
                    queryExp,
                    Expression.Lambda(property, param));
                str = "ThenBy";
                str2 = "ThenByDescending";
            }

            return query.Provider.CreateQuery(queryExp);
        }

        private SelectedItems GetSelectedItems(IDataRow row)
        {
            var selectedItem = new SelectedItems
                                   {
                                       Selected = true,
                                       ValueItem = row,
                                       SelectedKey = row.Value,
                                       Value = row.Value,
                                       Deleted = false,
                                   };
            OnEnabledRow(selectedItem);
            return selectedItem;
        }

        private SelectedItems OnEnabledRow(SelectedItems selectedItem)
        {
            var args = new EnabledEventArgs(selectedItem.ValueItem, true);
            OnEnabledRow(args);
            selectedItem.Enabled = args.Enabled;
            return selectedItem;
        }

        protected BaseDataSourceView<TKeyValues> GetValuesDataSource()
        {
            if (ValuesDataSourceView == null)
            {
                var control = (IDataSource)ControlHelper.FindControl(Owner, ValuesDataSourceID);
                ValuesDataSourceView = (BaseDataSourceView<TKeyValues>)control.GetView("Default");
            }

            return ValuesDataSourceView;
        }

        public override void Insert(IDictionary values, DataSourceViewOperationCallback callback)
        {
            var item = Activator.CreateInstance(this.ValuesTableType);
            UpdateRowValues(values, item);
            var ds = GetValuesDataSource();
            var table = ds.DataContext.GetTable(this.ValuesTableType);
            var args = new InsertingEventArgs { Row = item, DB = ds.DataContext };
            try
            {
                OnInserting(args);
                if (!args.Cancel)
                {
                    if (!args.CancelInsertItem)
                        table.InsertOnSubmit(item);
                    if (!args.CancelSubmitChanges)
                        ds.DataContext.SubmitChanges();
                    OnInserted(new InsertedEventArgs { Row = item });
                    callback(1, null);
                }
                else
                    callback(0, null);
            }
            catch (Exception e)
            {
                if (!callback(0, e)) throw;
            }
        }

        private void UpdateRowValues(IDictionary values, object item)
        {
            var properties = TypeDescriptor.GetProperties(item);

            foreach (var key in values.Keys)
            {
                var value = values[key];
                if (key.Equals("Value"))
                {
                    var fieldValues = ((string)value).Split(',');
                    var i = 0;
                    foreach (var valueField in ValueDataField.Split(','))
                    {
                        var property = properties.Find(valueField, true);
                        if (property != null)
                            SetValue(item, property, fieldValues[i++]);
                    }
                }
                else
                {
                    var property = properties.Find(key.ToString(), true);
                    if (property != null)
                        SetValue(item, property, value);
                }
            }
        }

        private static void SetValue(object item, PropertyDescriptor property, object value)
        {
            if (LinqFilterGenerator.IsNullableType(property.PropertyType))
            {
                var type = property.PropertyType.GetGenericArguments()[0];
                property.SetValue(item, value != null ? Convert.ChangeType(value, type) : null);
            }
            else
                property.SetValue(item, Convert.ChangeType(value, property.PropertyType));
        }

        public override void Delete(IDictionary keys, IDictionary oldValues, DataSourceViewOperationCallback callback)
        {
            var valuesDataSource = GetValuesDataSource();
            var deletedRows = valuesDataSource
                .GetSelectItemsIQueryable((string)keys["SelectedKey"])
                .OfType<object>()
                .ToList();
            var ds = GetValuesDataSource();
            var table = ds.DataContext.GetTable(this.ValuesTableType);
            var args = new DeletingEventArgs { Row = deletedRows, DB = ds.DataContext };
            try
            {
                OnDeleting(args);
                if (!args.Cancel)
                {
                    if (!args.CancelDeleteItem)
                        table.DeleteAllOnSubmit(deletedRows);
                    if (!args.CancelSubmitChanges)
                        ds.DataContext.SubmitChanges();
                    OnDeleted(new DeletedEventArgs { Rows = deletedRows });
                    callback(deletedRows.Count, null);
                }
                else
                    callback(0, null);
            }
            catch (Exception e)
            {
                if (!callback(0, e)) throw;
            }
        }

        public override void Update(IDictionary keys, IDictionary values, IDictionary oldValues, DataSourceViewOperationCallback callback)
        {
            var valuesDataSource = GetValuesDataSource();
            var updatedRows = valuesDataSource
                .GetSelectItemsIQueryable((string)keys["SelectedKey"])
                .OfType<object>()
                .ToList();
            var ds = GetValuesDataSource();
            foreach (var row in updatedRows)
            {
                var args = new UpdatingEventArgs { Row = row, DB = ds.DataContext };
                try
                {
                    UpdateRowValues(values, row);
                    OnUpdating(args);
                    if (!args.Cancel)
                    {
                        if (args.CancelUpdateItem)
                            ds.DataContext.Refresh(RefreshMode.OverwriteCurrentValues, row);

                        if (!args.CancelSubmitChanges)
                            ds.DataContext.SubmitChanges();
                        OnUpdated(new UpdatedEventArgs { Row = row });
                        callback(updatedRows.Count, null);
                    }
                    else
                        callback(0, null);
                }
                catch (Exception e)
                {
                    if (!callback(0, e)) throw;
                }
            }
        }

        public class SelectedEventArgs1 : SelectedEventArgs
        {
            public SelectedEventArgs1(IDataRow rowValues, string value, bool selected)
                : base(value, selected)
            {
                RowValues = rowValues;
            }

            public IDataRow RowValues { get; set; }
        }

        public class SelectedItems : EditableListDataSourceView.SelectedItems
        {
        }
    }


    public abstract class EditableListDataSourceView<TKeyValues, TKeyMembers> : EditableListDataSourceView<TKeyValues>
        where TKeyValues : struct
        where TKeyMembers : struct
    {
        protected EditableListDataSourceView(EditableListDataSource owner, string viewName)
            : base(owner, viewName)
        {
        }

        public event EventHandler<EnabledEventArgs2> EnabledRow2;

        public event EventHandler<SelectedEventArgs2> SelectedRow2;

        public string MembersDataSourceID { get; set; }

        public bool ValuesOnly { get; set; }

        public Type MemberTableType
        {
            get { return GetMembersDataSource().TableType; }
        }

        public Type MemberRowType
        {
            get { return GetMembersDataSource().RowType; }
        }

        public BaseDataSourceView<TKeyMembers> MembersDataSourceView { get; set; }

        protected virtual void OnEnabledRow2(EnabledEventArgs2 e)
        {
            var handler = EnabledRow2;
            if (handler != null) handler(this, e);
        }

        protected virtual void OnSelectedRow2(SelectedEventArgs2 e)
        {
            OnSelectedRow(e);
            var handler = SelectedRow2;
            if (handler != null) handler(this, e);
        }

        protected override IEnumerable ExecuteSelect(DataSourceSelectArguments arguments)
        {
            var valuesDataSource = GetValuesDataSource();

            if (ValuesOnly)
            {
                return OrderValuesQuery(valuesDataSource.GetSelectIQueryable(), arguments.SortExpression)
                    .OfType<IDataRow>()
                    .ToList()
                    .Select(GetSelectedItems);
            }

            var membersDataSource = GetMembersDataSource();

            var keys = Select<SelectedItems>(valuesDataSource.GetSelectIQueryable(), GetValuesExpression(), ValuesRowType)
                .ToLookup(r => r.Value);
            var list = membersDataSource.GetSelectIQueryable().OfType<IDataRow>().ToList();
            var dic = list.ToDictionary(r => r.Value);
            var deletedValues = keys.Where(r => r.Key != null && !dic.ContainsKey(r.Key)).Select(r => r.Key).ToList();
            var data = list.SelectMany(r => GetSelectedItems(r, keys));
            if (deletedValues.Count > 0)
            {
                var deletedItems = membersDataSource
                    .GetSelectIQueryableWithoutFilters(deletedValues, null, null, LocalizationHelper.IsCultureKZ)
                    .OfType<IDataRow>()
                    .ToList();
                data = data.Union(deletedItems.SelectMany(r => GetSelectedItemsDeleted(r, keys)));
            }

            return OrderSelectedItems2(data.AsQueryable(), arguments.SortExpression);
        }

        private IQueryable<TResult> Select<TResult>(IQueryable source, Expression select, Type type)
        {
            var exp = Expression.Call(
                typeof(Queryable), "Select", new[] { type, typeof(TResult) }, source.Expression, select);
            return source.Provider.CreateQuery<TResult>(exp);
        }

        protected virtual IQueryable<SelectedItems2> OrderSelectedItems2(IQueryable<SelectedItems2> query, string sortExpression)
        {
            if (string.IsNullOrEmpty(sortExpression)) return query;
            var queryExp = query.Expression;
            var str = "OrderBy";
            var str2 = "OrderByDescending";
            foreach (string orderBy in sortExpression.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
            {
                var split = orderBy.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                var ascending = true;
                if (split.Length == 2)
                    ascending = split[1].Equals("Asc", StringComparison.OrdinalIgnoreCase);

                var orderByColumn = split[0];
                var param = Expression.Parameter(typeof(SelectedItems2), "c");
                Type fieldType;
                Expression property;
                if (orderByColumn.StartsWith("MemberItem.") && MemberRowType != null)
                {
                    Expression memberItem = Expression.Property(param, "MemberItem");
                    memberItem = Expression.Convert(memberItem, MemberRowType);
                    property = LinqFilterGenerator.GetProperty(MemberRowType, orderByColumn.Substring(11), memberItem, false, out fieldType);
                }
                else if (orderByColumn.StartsWith("ValueItem.") && ValuesRowType != null)
                {
                    Expression valueItem = Expression.Property(param, "ValueItem");
                    valueItem = Expression.Convert(valueItem, ValuesRowType);
                    property = LinqFilterGenerator.GetProperty(ValuesRowType, orderByColumn.Substring(10), valueItem, false, out fieldType);
                }
                else
                {
                    property = LinqFilterGenerator.GetProperty(
                        typeof(SelectedItems2), orderByColumn, param, out fieldType);
                }

                queryExp = Expression.Call(
                    typeof(Queryable),
                    ascending ? str : str2,
                    new[] { typeof(SelectedItems2), fieldType },
                    queryExp,
                    Expression.Lambda(property, param));
                str = "ThenBy";
                str2 = "ThenByDescending";
            }

            return query.Provider.CreateQuery<SelectedItems2>(queryExp);
        }

        private SelectedItems GetSelectedItems(IDataRow row)
        {
            var getValue = GetValuesExpressionForIDataRow().Compile();
            var selectedItem = new SelectedItems
                {
                    Selected = true,
                    ValueItem = row,
                    SelectedKey = row.Value,
                    Value = getValue(row).Value,
                    Deleted = false,
                    Name = row.Name,
                    Enabled = true,
                };
            var args = new EnabledEventArgs2(selectedItem.ValueItem, null, true);
            OnEnabledRow2(args);
            return selectedItem;
        }

        private string GetRowValue(IDataRow row)
        {
            return BaseListDataBoundControl.GetPropertyValue(row, "Item." + ValueDataField).ToString();
        }

        private IEnumerable<SelectedItems2> GetSelectedItems(IDataRow r, ILookup<string, SelectedItems> keys)
        {
            if (keys.Contains(r.Value))
                foreach (var item in keys[r.Value])
                {
                    var selectedItem = new SelectedItems2
                                           {
                                               MemberItem = r,
                                               Value = r.Value,
                                               Selected = true,
                                               ValueItem = item.ValueItem,
                                               SelectedKey = item.ValueItem.Value,
                                           };
                    var args = new EnabledEventArgs2(selectedItem.ValueItem, r, true);
                    OnEnabledRow2(args);
                    selectedItem.Enabled = args.Enabled;
                    yield return selectedItem;
                }
            else
            {
                var selectedItem = new SelectedItems2
                                       {
                                           MemberItem = r,
                                           Value = r.Value,
                                       };
                var args = new EnabledEventArgs2(selectedItem.ValueItem, r, true);
                OnEnabledRow2(args);
                selectedItem.Enabled = args.Enabled;
                var selArgs = new SelectedEventArgs2(selectedItem.ValueItem, r, r.Value, false);
                OnSelectedRow2(selArgs);
                selectedItem.Selected = selArgs.Selected;
                yield return selectedItem;
            }
        }

        private IEnumerable<SelectedItems2> GetSelectedItemsDeleted(IDataRow r, ILookup<string, SelectedItems> keys)
        {
            foreach (var item in keys[r.Value])
            {
                var selectedItem = new SelectedItems2
                                       {
                                           MemberItem = r,
                                           ValueItem = item.ValueItem,
                                           Value = r.Value,
                                           Selected = true,
                                           Deleted = true,
                                       };
                var args = new EnabledEventArgs2(selectedItem.ValueItem, r, true);
                OnEnabledRow2(args);
                selectedItem.Enabled = args.Enabled;
                selectedItem.SelectedKey = selectedItem.ValueItem.Value;
                yield return selectedItem;
            }
        }

        protected virtual Expression GetValuesExpression()
        {
            var param = Expression.Parameter(ValuesRowType, "getValue");
            Expression exp = null;
            var unionStrings = (Expression<Func<string, string, string>>)
                               ((first, second) => first + "," + second);
            foreach (var field in ValueDataField.Split(','))
            {
                Expression property = Expression.Property(Expression.Property(param, "Item"), field);
                if (property.Type != typeof(string))
                    property = Expression.Call(property, "ToString", new Type[0]);
                exp = exp == null
                    ? property
                    : Expression.Invoke(unionStrings, exp, property);
            }

            var newSelectedItems = (Expression<Func<IDataRow, string, SelectedItems>>)
                                   ((row, value) => new SelectedItems { ValueItem = row, Value = value });
            if (exp != null)
            {
                exp = Expression.Invoke(newSelectedItems, param, exp);
                return Expression.Lambda(exp, param);
            }

            return null;
        }

        private Expression<Func<IDataRow, SelectedItems>> GetValuesExpressionForIDataRow()
        {
            var param = Expression.Parameter(typeof(IDataRow), "dataRow");
            Expression exp = Expression.Convert(param, ValuesRowType);
            exp = Expression.Invoke(GetValuesExpression(), exp);
            return Expression.Lambda<Func<IDataRow, SelectedItems>>(exp, param);
        }

        protected virtual Expression GetValuesWhereExpression(string key)
        {
            var param = Expression.Parameter(ValuesTableType, "getValuesWhere");
            Expression exp = null;
            var keys = key.Split(',');
            var i = 0;
            foreach (var field in ValueDataField.Split(','))
            {
                var property = Expression.Property(param, field);
                Expression constant;
                if (property.Type == typeof(string))
                    constant = Expression.Constant(keys[i++]);
                else
                    constant = Expression.Constant(Convert.ChangeType(keys[i++], property.Type));

                exp = exp == null
                    ? Expression.Equal(property, constant)
                    : Expression.And(exp, Expression.Equal(property, constant));
            }

            if (exp != null)
                return Expression.Lambda(exp, param);
            return null;
        }

        #region help methods

        protected BaseDataSourceView<TKeyMembers> GetMembersDataSource()
        {
            if (MembersDataSourceView == null)
            {
                var control = (IDataSource)ControlHelper.FindControl(Owner, MembersDataSourceID);
                MembersDataSourceView = (BaseDataSourceView<TKeyMembers>)control.GetView("Default");
            }
            return MembersDataSourceView;
        }

        #endregion


        public class EnabledEventArgs2 : EnabledEventArgs
        {
            public EnabledEventArgs2(IDataRow rowValues, IDataRow rowMembers, bool enabled)
                : base(rowValues, enabled)
            {
                RowMembers = rowMembers;
            }

            public IDataRow RowMembers { get; set; }
        }

        public class SelectedEventArgs2 : SelectedEventArgs1
        {
            public SelectedEventArgs2(IDataRow rowValues, IDataRow rowMembers, string value, bool enabled)
                : base(rowValues, value, enabled)
            {
                RowMembers = rowMembers;
            }

            public IDataRow RowMembers { get; set; }
        }

        public class SelectedItems2 : SelectedItems
        {
            public IDataRow MemberItem { get; set; }
        }
    }

    public abstract class EditableListDataSourceView<TKeyValues, TTableValues, TDataContextValues, TRowValues> : EditableListDataSourceView<TKeyValues>
        where TKeyValues : struct
        where TRowValues : BaseRow, new()
        where TTableValues : class, new()
        where TDataContextValues : DataContext, new()
    {
        protected EditableListDataSourceView(EditableListDataSource owner, string viewName) : base(owner, viewName)
        {
        }
    }

    public class EditableListDataSourceView<TKeyValues, TTableValues, TDataContextValues, TRowValues, TKeyMembers, TTableMembers, TDataContextMembers, TRowMembers> : EditableListDataSourceView<TKeyValues, TKeyMembers>
        where TKeyValues : struct
        where TTableValues : class, new()
        where TDataContextValues : DataContext, new()
        where TRowValues : BaseRow, new()
        where TKeyMembers : struct
        where TTableMembers : class
        where TDataContextMembers : DataContext, new()
        where TRowMembers : BaseRow, new()
    {
        public EditableListDataSourceView(EditableListDataSource owner, string viewName)
            : base(owner, viewName)
        {
        }
    }
}