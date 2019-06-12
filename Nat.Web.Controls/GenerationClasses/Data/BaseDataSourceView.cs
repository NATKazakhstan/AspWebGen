using System.Data.SqlClient;

namespace Nat.Web.Controls.GenerationClasses
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Data;
    using System.Data.Common;
    using System.Data.Linq;
    using System.Diagnostics;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Web;
    using System.Web.Compilation;
    using System.Web.UI;

    using Nat.Tools.Filtering;
    using Nat.Web.Controls.GenerationClasses.BaseJournal;
    using Nat.Web.Controls.GenerationClasses.Data;
    using Nat.Web.Controls.GenerationClasses.Filter;
    using Nat.Web.Controls.GenerationClasses.Navigator;
    using Nat.Web.Controls.Properties;
    using Nat.Web.Controls.Trace;
    using Nat.Web.Tools;
    using Nat.Web.Tools.WorkFlow;
    using Nat.SqlDbInitializer.Wrappers;

    public abstract class BaseDataSourceView<TKey> : DataSourceView,
                                                     IDataSourceView,
                                                     IDataSourceViewGetName,
                                                     IDataSourceView3
        where TKey : struct
    {
        private DbConnection _initConnection;

        protected BaseDataSourceView(IDataSource owner, string viewName)
            : base(owner, viewName)
        {
        }

        public abstract bool SupportSelectICodeRow { get; }

        public string SelectedRowKey { get; set; }

        public virtual string TableName { get; set; }

        public virtual Type TableType { get; private set; }

        public virtual Type RowType { get; private set; }

        protected string SelectQueryEndText { get; set; }

        public virtual DbConnection InitConnection
        {
            get
            {
                if (_initConnection != null)
                    return _initConnection;

                _initConnection = !string.IsNullOrEmpty(SelectQueryEndText) ? new DbConnectionWrapper() : _initConnection;
                return _initConnection;
            }
            set
            {
                _initConnection = value;
                if (DataContext.Connection != value)
                    DataContext = null;
            }
        }

        public virtual DbTransaction InitTransaction { get; set; }
        public virtual DataContext DataContext { get; protected set; }

        internal protected virtual bool CancelTreeUse { get; set; }

        public virtual bool LookupValuesHidden { get; set; }

        public virtual bool SupportFlagCanAddChild
        {
            get { return false; }
        }

        public string SelectForAddType { get; set; }

        public long? LogViewData { get; set; }

        public Type KeyType
        {
            get { return typeof(TKey); }
        }

        #region IDataSourceView3 Members

        bool IDataSourceView3.SupportFlagCanAddChild
        {
            get { return false; }
        }

        public SelectParameters SelectParameters { get; set; }

        public bool HideRecordCanNotSelected { get; set; }

        #endregion

        public abstract IQueryable<IRow> GetSelectIRow(string queryParameters);

        public abstract IQueryable<ICodeRow> GetSelectICodeRow(string queryParameters);

        public abstract IQueryable<IRow> GetSelectIRow();

        public abstract IQueryable<ICodeRow> GetSelectICodeRow();

        public abstract IQueryable<IRow> GetSelectIRow(TKey value);

        public abstract IQueryable<ICodeRow> GetSelectICodeRow(TKey value);

        public abstract bool CheckPermit();

        public abstract BaseInformationValues Validate(TKey value, CancelEventArgs args);

        public virtual BaseInformationValues Validate(TKey value, RecordValidateArgs args)
        {
            return Validate(value, (CancelEventArgs)args);
        }

        public virtual BaseRow[] ValidateAllBase(TKey[] values, CancelEventArgs args)
        {
            return null;
        }

        public virtual IQueryable GetSelectIQueryable()
        {
            throw new NotSupportedException();
        }

        public virtual IQueryable GetSelectIQueryableWithoutFilters()
        {
            throw new NotSupportedException();
        }

        public virtual IQueryable GetSelectIQueryableWithoutFilters(
            IEnumerable<string> selectedValues, string selectColumnName, string sortExpression, bool isKz)
        {
            throw new NotSupportedException();
        }

        public virtual IQueryable GetSelectItemsIQueryable(string key)
        {
            throw new NotSupportedException();
        }

        public virtual string GetName(TKey value, string selectColumnName, bool isKz)
        {
            return null;
        }

        public virtual string GetName(TKey value)
        {
            return GetName(value, string.Empty, LocalizationHelper.IsCultureKZ);
        }

        public virtual string GetCode(TKey value)
        {
            return null;
        }

        public virtual string GetName(TKey value, bool isKz)
        {
            return GetName(value, string.Empty, isKz);
        }

        public virtual string GetName(TKey value, string selectColumnName)
        {
            return GetName(value, selectColumnName, LocalizationHelper.IsCultureKZ);
        }

        public virtual IQueryable<BaseCodeRow> GetSelectCodeRow(TKey value)
        {
            return GetSelectCodeRow(value, string.Empty);
        }

        public virtual IQueryable<BaseCodeRow> GetSelectCodeRow(TKey value, string selectColumnName)
        {
            throw new NotImplementedException();
        }

        public virtual BaseCodeRow GetCodeRow(TKey value)
        {
            return GetCodeRow(value, string.Empty);
        }

        public virtual BaseCodeRow GetCodeRow(TKey value, string selectColumnName)
        {
            return GetSelectCodeRow(value, selectColumnName)
                /*.Select(
                    r =>
                        new BaseCodeRow
                        {
                            Value = r.Value,
                            nameRu = r.nameRu,
                            nameKz = r.nameKz,
                            code = r.code,
                        })*/
                .FirstOrDefault();
        }

        public virtual IQueryable<BaseRow> GetSelectRow(TKey value)
        {
            return GetSelectRow(value, string.Empty);
        }

        public virtual IQueryable<BaseRow> GetSelectRow(TKey value, string selectColumnName)
        {
            throw new NotImplementedException();
        }

        public virtual BaseRow GetRow(TKey value)
        {
            return GetRow(value, string.Empty);
        }

        public virtual BaseRow GetRow(TKey value, string selectColumnName)
        {
            return GetSelectRow(value, selectColumnName)
                /*.Select(
                    r =>
                        new BaseRow
                        {
                            Value = r.Value,
                            nameRu = r.nameRu,
                            nameKz = r.nameKz,
                        })*/
                .FirstOrDefault();
        }

        public virtual BaseFilterControl<TKey> CreateDefaultFilter(
            string typeName,
            string tableName,
            string selectMode,
            TKey? slectedID,
            BrowseFilterParameters parameters,
            DataContext db)
        {
            var urlBuilder = new MainPageUrlBuilder { SelectMode = selectMode, IsSelect = true, IsDataControl = true, };
            var control = CreateDefaultFilter(urlBuilder);
            if (control == null)
            {
                urlBuilder.UserControl = tableName + "Journal";
                var type = BuildManager.GetType(typeName, true);
                control = (BaseFilterControl<TKey>)Activator.CreateInstance(type);
            }

            if (slectedID != null)
                control.SelectedID = slectedID;
            parameters.SetQueryParameters(urlBuilder);
            control.SetUrl(urlBuilder);
            control.SetDB(db);
            return control;
        }

        public virtual BaseFilterControl<TKey> CreateDefaultFilter(MainPageUrlBuilder urlBuilder)
        {
            return null;
        }

        #region IDataSourceViewGetName Members

        public virtual object GetKey(string key)
        {
            return Convert.ChangeType(key, typeof(TKey));
        }

        public virtual string GetName(string key)
        {
            return GetName((TKey)GetKey(key));
        }

        public virtual string GetName(object key)
        {
            return GetName((TKey)key);
        }

        public string GetCode(string key)
        {
            return GetCode((TKey)GetKey(key));
        }

        public string GetCode(object key)
        {
            return GetCode((TKey)key);
        }

        public virtual object GetTableParameterValue(string key, string tableParameterName)
        {
            return null;
        }

        #endregion

        public virtual void SetLookupVisible(string serverMaping, bool visible)
        {
            var lookupPrefix = "Lookup.";
            var namePostfix = "_Name";
            if (string.IsNullOrEmpty(serverMaping)
                || !serverMaping.StartsWith(lookupPrefix)
                || !serverMaping.EndsWith(namePostfix))
            {
                return;
            }

            var properties = TypeDescriptor.GetProperties(GetType());
            var lookupDescriptor = properties.Find("Lookup", false);
            if (lookupDescriptor == null)
                return;

            var lookup = lookupDescriptor.GetValue(this);
            if (lookup == null)
                return;

            var lookupProperties = TypeDescriptor.GetProperties(lookupDescriptor.PropertyType);
            var propertyName = serverMaping.Substring(lookupPrefix.Length, serverMaping.Length - lookupPrefix.Length - namePostfix.Length) + "Hidden";
            var hiddenDescriptor = lookupProperties.Find(propertyName, false);
            if (hiddenDescriptor != null)
                hiddenDescriptor.SetValue(lookup, !visible);
        }
    }

    public abstract class BaseDataSourceView<TKey, TTable> : BaseDataSourceView<TKey>,
                                                             IParentDataSourceViews,
                                                             IDataSourceView3
        where TKey : struct
        where TTable : class
    {
        protected BaseDataSourceView(IDataSource owner, string viewName)
            : base(owner, viewName)
        {
        }

        #region IDataSourceView3 Members

        bool IDataSourceView3.SupportFlagCanAddChild
        {
            get { return true; }
        }

        #endregion

        public override Type TableType
        {
            get { return typeof(TTable); }
        }

        public Expression GetEditAndDeleteExpression(Expression source)
        {
            var param = Expression.Parameter(typeof(TTable), "t");
            source = GetEditAndDeleteExpression<TTable>(source, param, param, new Expression[0]);
            if (source == null)
                return null;
            return Expression.Lambda(source, param);
        }

        public Expression GetEditExpression(Expression source)
        {
            var param = Expression.Parameter(typeof(TTable), "t");
            source = GetEditExpression<TTable>(source, param, param, new Expression[0]);
            if (source == null)
                return null;
            return Expression.Lambda(source, param);
        }

        public Expression GetDeleteExpression(Expression source)
        {
            var param = Expression.Parameter(typeof(TTable), "t");
            source = GetDeleteExpression<TTable>(source, param, param, new Expression[0]);
            if (source == null)
                return null;
            return Expression.Lambda(source, param);
        }

        public Expression GetEditAndDeleteExpression<T>(
            Expression source,
            Expression upToTable,
            ParameterExpression param,
            IEnumerable<Expression> fieldsToCheckReference)
            where T : class
        {
            var filterArgs = GetEditAndDeleteFilter(typeof(T));
            source = filterArgs.GetFilterExpression(source, upToTable, param, fieldsToCheckReference);
            var paramT = param;
            if (paramT == null)
            {
                paramT = Expression.Parameter(typeof(TTable), "t");
                upToTable = paramT;
            }

            source = ExecuteByParentCollection(
                source,
                upToTable,
                fieldsToCheckReference,
                delegate(ParentDataSourceViewInfo args,
                         Expression sourceArgs,
                         Expression parentTable,
                         IEnumerable<Expression> parentCheckReference)
                    {
                        return args.DataSource.GetEditAndDeleteExpression<T>(
                            sourceArgs, parentTable, param, parentCheckReference);
                    });
            return source;
        }

        public Expression GetEditExpression<T>(
            Expression source,
            Expression upToTable,
            ParameterExpression param,
            IEnumerable<Expression> fieldsToCheckReference)
            where T : class
        {
            var filterArgs = GetEditFilter(typeof(T));
            source = filterArgs.GetFilterExpression(source, upToTable, param, fieldsToCheckReference);
            var paramT = param;
            if (paramT == null)
            {
                paramT = Expression.Parameter(typeof(TTable), "t");
                upToTable = paramT;
            }

            source = ExecuteByParentCollection(
                source,
                upToTable,
                fieldsToCheckReference,
                delegate(ParentDataSourceViewInfo args,
                         Expression sourceArgs,
                         Expression parentTable,
                         IEnumerable<Expression> parentCheckReference)
                    {
                        return args.DataSource.GetEditExpression<T>(
                            sourceArgs, parentTable, param, parentCheckReference);
                    });
            return source;
        }

        public Expression GetDeleteExpression<T>(
            Expression source,
            Expression upToTable,
            ParameterExpression param,
            IEnumerable<Expression> fieldsToCheckReference)
            where T : class
        {
            var filterArgs = GetDeleteFilter(typeof(T));
            source = filterArgs.GetFilterExpression(source, upToTable, param, fieldsToCheckReference);
            var paramT = param;
            if (paramT == null)
            {
                paramT = Expression.Parameter(typeof(TTable), "t");
                upToTable = paramT;
            }

            source = ExecuteByParentCollection(
                source,
                upToTable,
                fieldsToCheckReference,
                delegate(ParentDataSourceViewInfo args,
                         Expression sourceArgs,
                         Expression parentTable,
                         IEnumerable<Expression> parentCheckReference)
                    {
                        return args.DataSource.GetDeleteExpression<T>(
                            sourceArgs, parentTable, param, parentCheckReference);
                    });
            return source;
        }

        public Expression GetAddChildExpression(
            Expression source,
            Type getForType,
            Expression upToTable,
            ParameterExpression param,
            IEnumerable<Expression> fieldsToCheckReference)
        {
            var filterArgs = GetAddRule(getForType);
            source = filterArgs.GetFilterExpression(source, upToTable, param, fieldsToCheckReference);
            ParameterExpression paramT = param;
            if (paramT == null)
            {
                paramT = Expression.Parameter(typeof(TTable), "t");
                upToTable = paramT;
            }

            source = ExecuteByParentCollection(
                source,
                upToTable,
                fieldsToCheckReference,
                delegate(ParentDataSourceViewInfo args,
                         Expression sourceArgs,
                         Expression parentTable,
                         IEnumerable<Expression> parentCheckReference)
                    {
                        return args.DataSource.GetAddChildExpression(
                            sourceArgs, getForType, parentTable, param, parentCheckReference);
                    });
            return source;
        }

        public virtual BaseFilterEventArgs<TTable> GetEditAndDeleteFilter(Type typeOfData)
        {
            return new BaseFilterEventArgs<TTable>(null) { TypeOfData = typeOfData };
        }

        public virtual BaseFilterEventArgs<TTable> GetEditFilter(Type typeOfData)
        {
            return new BaseFilterEventArgs<TTable>(null) { TypeOfData = typeOfData };
        }

        public virtual BaseFilterEventArgs<TTable> GetDeleteFilter(Type typeOfData)
        {
            return new BaseFilterEventArgs<TTable>(null) { TypeOfData = typeOfData };
        }

        public List<string> AllowAddRowForTableTypes(BaseNavigatorValues values, string tableHeader)
        {
            var errorDic = AllowAddRowForTableTypes(values, tableHeader, typeof(TTable));
            if (!errorDic.ContainsKey(typeof(TTable)))
                return new List<string>();
            return errorDic[typeof(TTable)];
        }

        public List<string> GetEditErrors(TKey key, string tableHeader)
        {
            var errorDic = GetEditErrorsForTableTypes(key, tableHeader, typeof(TTable));
            if (!errorDic.ContainsKey(typeof(TTable)))
                return new List<string>();
            return errorDic[typeof(TTable)];
        }

        public List<string> GetDeleteErrors(TKey key, string tableHeader)
        {
            var errorDic = GetDeleteErrorsForTableTypes(key, tableHeader, typeof(TTable));
            if (!errorDic.ContainsKey(typeof(TTable)))
                return new List<string>();
            return errorDic[typeof(TTable)];
        }

        public Dictionary<Type, List<string>> AllowAddRowForTableTypes(
            BaseNavigatorValues values, string tableHeader, params Type[] tableTypeForCheck)
        {
            var sources = AllowAddRowGetSources(values);
            var list = new List<BaseFilterExpression>();
            IEnumerable<bool> selectExp = null;
            foreach (var source in sources)
            {
                var partList = new List<BaseFilterExpression>();

                var param = Expression.Parameter(source.ElementType, "t");
                var parentCollection = GetParentCollection().ToList();
                foreach (var type in tableTypeForCheck)
                {
                    var item = parentCollection.FirstOrDefault(r => r.TableType == source.ElementType);
                    if (item != null)
                        item.DataSource.GetExpressionCollection(partList, type, param, new Expression[0]);
                }

                list.AddRange(partList);
                var partSelectExp = GetExpressionsResults(source, source.ElementType, partList, param);
                selectExp = selectExp == null ? partSelectExp : selectExp.Concat(partSelectExp);
            }

            if (list.Count == 0)
                return new Dictionary<Type, List<string>>();

            var errorDic = CreateErrorsDictionary(sources.Last().Provider, tableHeader, list, selectExp);
            return errorDic;
        }

        public Dictionary<Type, List<string>> GetEditErrorsForTableTypes(
            TKey key, string tableHeader, params Type[] tableTypeForCheck)
        {
            var source = GetSelectItems(key);
            var list = new List<BaseFilterExpression>();
            var param = Expression.Parameter(source.ElementType, "t");
            var parentCollection = GetParentCollection().ToList();
            foreach (var type in tableTypeForCheck)
            {
                GetEditExpressionCollection(list, type, param, new Expression[0]);
                var item = parentCollection.FirstOrDefault(r => r.TableType == source.ElementType);
                if (item != null)
                    item.DataSource.GetEditExpressionCollection(list, type, param, new Expression[0]);
            }

            var selectExp = GetExpressionsResults(source, source.ElementType, list, param);
            if (list.Count == 0)
                return new Dictionary<Type, List<string>>();

            var errorDic = CreateErrorsDictionary(source.Provider, tableHeader, list, selectExp);
            return errorDic;
        }

        public Dictionary<Type, List<string>> GetDeleteErrorsForTableTypes(
            TKey key, string tableHeader, params Type[] tableTypeForCheck)
        {
            var source = GetSelectItems(key);
            var list = new List<BaseFilterExpression>();
            var param = Expression.Parameter(source.ElementType, "t");
            var parentCollection = GetParentCollection().ToList();
            foreach (var type in tableTypeForCheck)
            {
                GetDeleteExpressionCollection(list, type, param, new Expression[0]);
                var item = parentCollection.FirstOrDefault(r => r.TableType == source.ElementType);
                if (item != null)
                    item.DataSource.GetDeleteExpressionCollection(list, type, param, new Expression[0]);
            }

            list.AddRange(list);
            var selectExp = GetExpressionsResults(source, source.ElementType, list, param);

            if (list.Count == 0)
                return new Dictionary<Type, List<string>>();

            var errorDic = CreateErrorsDictionary(source.Provider, tableHeader, list, selectExp);
            return errorDic;
        }

        public abstract IEnumerable<IQueryable> AllowAddRowGetSources(BaseNavigatorValues values);

        public List<string> AllowAddRowForTableTypes(
            IQueryable<TTable> source, string tableHeader, Type tableTypeForCheck)
        {
            return AllowAddRowForTableTypes(source, tableHeader, new[] { tableTypeForCheck })[tableTypeForCheck];
        }

        public Dictionary<Type, List<string>> AllowAddRowForTableTypes(
            IQueryable<TTable> source, string tableHeader, params Type[] tableTypeForCheck)
        {
            var list = new List<BaseFilterExpression>();
            var selectExp = AllowAddRowGetSelectExpression(source, typeof(TTable), tableTypeForCheck, list);
            if (selectExp == null)
            {
                var errors = new Dictionary<Type, List<string>>();
                foreach (var type in tableTypeForCheck)
                {
                    errors[type] = new List<string> { string.Empty };
                }
                return errors;
            }

            return CreateErrorsDictionary(source.Provider, tableHeader, list, selectExp);
        }

        private static Dictionary<Type, List<string>> CreateErrorsDictionary(
            IQueryProvider provider, string tableHeader, List<BaseFilterExpression> list, IEnumerable<bool> selectExp)
        {
            var errorDic = new Dictionary<Type, List<string>>();
            if (selectExp == null)
                return errorDic;
            var result = selectExp.ToArray();
            Type prevTableType = null;
            for (int i = 0; i < list.Count; i++)
            {
                // данное условие добавлено для того что бы не проверять каждый раз на наличие в славаре, т.е. таблички идут подрят
                if (prevTableType != list[i].FilterForType)
                {
                    if (!errorDic.ContainsKey(list[i].FilterForType))
                        errorDic.Add(list[i].FilterForType, new List<string>());
                    prevTableType = list[i].FilterForType;
                }

                if (!result[i])
                {
                    string message = string.IsNullOrEmpty(list[i].Message)
                                         ? string.Empty
                                         : string.Format(list[i].Message, tableHeader);
                    if (!errorDic[list[i].FilterForType].Contains(message))
                        errorDic[list[i].FilterForType].Add(message);
                }
            }

            return errorDic;
        }

        private IEnumerable<bool> AllowAddRowGetSelectExpression(
            IQueryable source, Type sourceType, Type[] tableTypeForCheck, List<BaseFilterExpression> list)
        {
            var param = Expression.Parameter(sourceType, "t");
            foreach (var type in tableTypeForCheck)
            {
                GetExpressionCollection(list, type, param, new Expression[0]);
            }
            var arrayInitializer = list.Select(r => (Expression)Expression.Convert(r.whereExpression, typeof(bool?)));
            var arrExp = Expression.NewArrayInit(typeof(bool?), arrayInitializer);
            Expression pred = Expression.Lambda(arrExp, param);
            var selectExp = Expression.Call(
                typeof(Queryable), "Select", new[] { sourceType, typeof(bool?[]) }, source.Expression, pred);
            var query = source.Provider.CreateQuery<bool?[]>(selectExp);
            var row = query.FirstOrDefault();
            if (row == null)
                return null;
            return row.Select(r => r ?? true);
        }

        private IEnumerable<bool> GetExpressionsResults(
            IQueryable source, Type sourceType, List<BaseFilterExpression> list, ParameterExpression param)
        {
            var arrExp = Expression.NewArrayInit(typeof(bool), list.Select(r => r.whereExpression));
            if (arrExp.Expressions.Count == 0)
                return new bool[0];
            Expression pred = Expression.Lambda(arrExp, param);
            var selectExp = Expression.Call(
                typeof(Queryable), "Select", new[] { sourceType, typeof(bool[]) }, source.Expression, pred);
            var query = source.Provider.CreateQuery<bool[]>(selectExp);
            return query.FirstOrDefault();
        }
        
        public void GetExpressionCollection(
            List<BaseFilterExpression> list,
            Type getForType,
            Expression upToTable,
            IEnumerable<Expression> fieldsToCheckReference)
        {
            var filterArgs = GetAddRule(getForType);
            filterArgs.GetExpressionCollection(list, getForType, upToTable, fieldsToCheckReference);
            ExecuteByParentCollection(
                upToTable,
                fieldsToCheckReference,
                delegate(ParentDataSourceViewInfo args,
                         Expression parentTable,
                         IEnumerable<Expression> parentCheckReference)
                    {
                        args.DataSource.GetExpressionCollection(list, getForType, parentTable, parentCheckReference);
                        return true;
                    });
        }

        public void GetEditExpressionCollection(
            List<BaseFilterExpression> list,
            Type getForType,
            Expression upToTable,
            IEnumerable<Expression> fieldsToCheckReference)
        {
            GetEditFilter(getForType).GetExpressionCollection(list, getForType, upToTable, fieldsToCheckReference);
            GetEditAndDeleteFilter(getForType).GetExpressionCollection(list, getForType, upToTable, fieldsToCheckReference);

            ExecuteByParentCollection(
                upToTable,
                fieldsToCheckReference,
                delegate(ParentDataSourceViewInfo args,
                         Expression parentTable,
                         IEnumerable<Expression> parentCheckReference)
                    {
                        args.DataSource.GetEditExpressionCollection(list, getForType, parentTable, parentCheckReference);
                        return true;
                    });
        }

        public void GetDeleteExpressionCollection(
            List<BaseFilterExpression> list,
            Type getForType,
            Expression upToTable,
            IEnumerable<Expression> fieldsToCheckReference)
        {
            GetDeleteFilter(getForType).GetExpressionCollection(list, getForType, upToTable, fieldsToCheckReference);
            GetEditAndDeleteFilter(getForType).GetExpressionCollection(list, getForType, upToTable, fieldsToCheckReference);

            ExecuteByParentCollection(
                upToTable,
                fieldsToCheckReference,
                delegate(ParentDataSourceViewInfo args,
                         Expression parentTable,
                         IEnumerable<Expression> parentCheckReference)
                    {
                        args.DataSource.GetDeleteExpressionCollection(list, getForType, parentTable, parentCheckReference);
                        return true;
                    });
        }

        protected virtual BaseFilterEventArgs<TTable> GetAddRule(Type typeOfData)
        {
            return new BaseFilterEventArgs<TTable>(null) { TypeOfData = typeOfData };
        }

        public abstract IEnumerable<ParentDataSourceViewInfo> GetParentCollection();

        private void ExecuteByParentCollection(
            Expression upToTable,
            IEnumerable<Expression> fieldsToCheckReference,
            Func<ParentDataSourceViewInfo, Expression, IEnumerable<Expression>, bool> handler)
        {
            foreach (var item in GetParentCollection())
            {
                var parentTable = Expression.Property(upToTable, item.ReferenceName);
                var parentCheckReference = new List<Expression>(fieldsToCheckReference);
                if (!item.Mandatory)
                    parentCheckReference.Add(Expression.Property(upToTable, item.FieldName));
                handler(item, parentTable, parentCheckReference);
            }
        }

        private T ExecuteByParentCollection<T>(
            T source,
            Expression upToTable,
            IEnumerable<Expression> fieldsToCheckReference,
            Func<ParentDataSourceViewInfo, T, Expression, IEnumerable<Expression>, T> handler)
        {
            foreach (var item in GetParentCollection())
            {
                var parentTable = Expression.Property(upToTable, item.ReferenceName);
                var parentCheckReference = new List<Expression>(fieldsToCheckReference);
                if (!item.Mandatory)
                    parentCheckReference.Add(Expression.Property(upToTable, item.FieldName));
                source = handler(item, source, parentTable, parentCheckReference);
            }

            return source;
        }

        protected Expression GetSelectedAdditionalValuesExpression(DataContext db, ParameterExpression param)
        {
            if (SelectParameters == null || !SelectParameters.LoadFromSession()
                || SelectParameters.SelectValues.Count == 0)
                return Expression.NewArrayInit(typeof(object));

            /*
            убрал т.к. похоже что лишнее, например на форме при автозаполнении значения в поле, открывается форма без AdditionalFields
                return Expression.NewArrayInit(typeof(object));*/

            var list = new List<Expression>(SelectParameters.SelectValues.Count);
            var isKz = LocalizationHelper.IsCultureKZ;
            foreach (var item in SelectParameters.SelectValues)
            {
                Expression exp = null;
                switch (item.Type)
                {
                    case SelectParameters.SelectInfoType.Column:
                        exp = ManyProperties(param, item.Name);
                        break;
                    case SelectParameters.SelectInfoType.TableParameter:
                        var tableParameterType = BuildManager.GetType(item.Name, true, true);
                        var tableParameter = (BaseTableParameter)Activator.CreateInstance(tableParameterType);
                        exp = tableParameter.GetExpression(isKz, param);
                        break;
                    case SelectParameters.SelectInfoType.RowInfo:
                        var rowInfoType = BuildManager.GetType(item.Name, true, true);
                        var rowInfo = (BaseAdditionalRowInfo)Activator.CreateInstance(rowInfoType);
                        var items = new Expression[item.Parameters.Count];
                        for (int i = 0; i < item.Parameters.Count; i++)
                        {
                            items[i] = SelectParameters.GetExpression(item.Parameters[i]);
                        }
                        exp = rowInfo.GetExpression(db, param, items);
                        break;
                    default:
                        break;
                }
                
                list.Add(Expression.Convert(MakeNullableType(exp), typeof(object)));
            }

            return Expression.NewArrayInit(typeof(object), list);
        }

        protected Expression ManyProperties(Expression exp, string properties)
        {
            return properties.Split('.').Aggregate(exp, Expression.Property);
        }

        protected Expression MakeNullableType(Expression exp)
        {
            if (exp.Type.IsClass || LinqFilterGenerator.IsNullableType(exp.Type))
                return exp;
            var nullableT = typeof(Nullable<>);
            var genericType = nullableT.MakeGenericType(exp.Type);
            return Expression.Convert(exp, genericType);
        }

        public override object GetTableParameterValue(string key, string tableParameterName)
        {
            IQueryable<TTable> source;
            try
            {
                source = GetSelectItems(key);
            }
            catch
            {
                return null;
            }
            var param = Expression.Parameter(typeof(TTable), "item");
            var tableParameterType = BuildManager.GetType(tableParameterName, true, false);
            var tableParameter = (BaseTableParameter)Activator.CreateInstance(tableParameterType);
            var exp = tableParameter.GetExpression(LocalizationHelper.IsCultureKZ, param);
            var selectExp = Expression.Call(
                typeof(Queryable),
                "Select",
                new[] { typeof(TTable), tableParameter.GetResultDataType() },
                source.Expression,
                Expression.Lambda(exp, param));
            selectExp = Expression.Call(
                typeof(Queryable), "First", new[] { tableParameter.GetResultDataType() }, selectExp);
            return source.Provider.Execute(selectExp);
        }

        public override IQueryable GetSelectItemsIQueryable(string key)
        {
            return GetSelectItems((TKey)GetKey(key));
        }

        public virtual IQueryable<TTable> GetSelectItems(string key)
        {
            return GetSelectItems((TKey)GetKey(key));
        }

        public virtual IQueryable<TTable> GetSelectItems(TKey key)
        {
            return null;
        }

        public virtual IQueryable<TTable> GetSelectItems()
        {
            return null;
        }

        protected bool AddSearchInFilter(MainPageUrlBuilder urlBuilder, BaseFilterControl<TKey, TTable> filter)
        {
            if (!urlBuilder.QueryParameters.ContainsKey(BaseFilterParameterSearch<object>.SearchQueryParameter))
                return false;

            var search = urlBuilder.QueryParameters[BaseFilterParameterSearch<object>.SearchQueryParameter];
            if (string.IsNullOrEmpty(search))
                return false;

            var filters = MainPageUrlBuilder.GetFilterItemsDicByFilterContent(urlBuilder.GetFilter(filter.GetFilterTableName()));
            filters[BaseFilterParameterSearch<object>.DefaultFilterName] =
                new List<FilterItem>
                {
                    new FilterItem(
                        BaseFilterParameterSearch<object>.DefaultFilterName,
                        ColumnFilterType.ContainsWords,
                        new object[] { search })
                };
            urlBuilder.SetFilter(filter.GetFilterTableName(), filters);

            return true;
        }
    }

    public abstract class BaseDataSourceView<TKey, TTable, TDataContext, TRow> : BaseDataSourceView<TKey, TTable>
        where TKey : struct
        where TTable : class
        where TDataContext : DataContext, new()
        where TRow : BaseRow, new()
    {
        private readonly BaseDataSource<TKey> owner;
        private int? cacheCount;
        private List<TRow> cache;
        private TDataContext db;
        private IWorkFlow[] workFlows;
        private string typeName;
        protected TKey? _selectedID;

        protected BaseDataSourceView(IDataSource owner, string viewName)
            : base(owner, viewName)
        {
            this.owner = (BaseDataSource<TKey>)owner;
        }

        #region public properties

        public override Type RowType
        {
            get { return typeof(TRow); }
        }

        public override DataContext DataContext
        {
            get { return DB; }
            protected set { DB = (TDataContext)value; }
        }

        public TDataContext DB
        {
            get { return db ?? (db = CreateDataContext()); }
            set { db = value; }
        }

        public int CountData { get; protected set; }

        public bool AllowCreateSelectedRow
        {
            get { return owner != null && owner.AllowCreateSelectedRow; }
        }

        public bool AllowSelectOnlyNames
        {
            get { return owner != null && owner.AllowSelectOnlyNames; }
        }

        public bool AllowCustomCache
        {
            get { return owner != null && owner.AllowCustomCache; }
        }

        public virtual bool SupportGlobalCache
        {
            get { return false; }
        }

        protected IWorkFlow[] WorkFlows
        {
            get { return workFlows ?? (workFlows = CreateWorkFlows()); }
        }

        #endregion

        #region internal

        [Conditional("DEBUG")]
        protected internal void WriteTrace(string value)
        {
            HttpContext.Current.Trace.WriteExt(value);
        }

        [Conditional("DEBUG")]
        protected internal void WriteTrace(string category, string value)
        {
            HttpContext.Current.Trace.WriteExt(category, value);
        }

        internal string TypeName
        {
            get
            {
                if (typeName == null)
                    typeName = GetType().Name;
                return typeName;
            }
        }

        #endregion

        #region abstract

        protected abstract string DefaultSort { get; }

        protected abstract string RequiredSort { get; }

        protected abstract bool UseTreeFilter { get; }

        public abstract string GetStringID(TTable row);

        public abstract TKey? GetIDByRefParent(TTable row);

        public abstract TKey? GetID(TTable row);

        #endregion

        #region help methods

        public virtual void ClearCustomCache()
        {
            cacheCount = null;
            cache = null;
        }

        public static Expression<Func<TTable, TLookupValues, TInformationValues>>
            ChangeSelect<TLookupValues, TInformationValues, TOtherValue>(
            Expression<Func<TTable, TLookupValues, TOtherValue>> selectOtherValueExpression,
            Expression<Func<TTable, TLookupValues, TOtherValue, TInformationValues>> selectExpression)
        {
            var paramI = Expression.Parameter(typeof(TTable), "itemCS");
            var paramL = Expression.Parameter(typeof(TLookupValues), "lookupCS");
            Expression body = Expression.Invoke(
                selectExpression,
                paramI,
                paramL,
                Expression.Invoke(selectOtherValueExpression, paramI, paramL));

            return Expression.Lambda<Func<TTable, TLookupValues, TInformationValues>>(
                body, paramI, paramL);
        }

        public static Expression<Func<TTable, TLookupValues, TInformationValues>>
            ChangeSelect<TLookupValues, TInformationValues, TOtherValue1, TOtherValue2>(
            Expression<Func<TTable, TLookupValues, TOtherValue1>> selectOtherValueExpression1,
            Expression<Func<TTable, TLookupValues, TOtherValue2>> selectOtherValueExpression2,
            Expression<Func<TTable, TLookupValues, TOtherValue1, TOtherValue2, TInformationValues>> selectExpression)
        {
            var paramI = Expression.Parameter(typeof(TTable), "itemCS");
            var paramL = Expression.Parameter(typeof(TLookupValues), "lookupCS");
            Expression body = Expression.Invoke(
                selectExpression,
                paramI,
                paramL,
                Expression.Invoke(selectOtherValueExpression1, paramI, paramL),
                Expression.Invoke(selectOtherValueExpression2, paramI, paramL));

            return Expression.Lambda<Func<TTable, TLookupValues, TInformationValues>>(
                body, paramI, paramL);
        }

        #endregion

        #region override/virtual methods

        public override BaseRow[] ValidateAllBase(TKey[] values, CancelEventArgs args)
        {
            return ValidateAll(values, args).Cast<BaseRow>().ToArray();
        }

        public virtual TRow[] ValidateAll(TKey[] values, CancelEventArgs args)
        {
            return null;
        }
        
        protected internal QueryParameters InternalQueryParameters { get; set; }

        protected virtual TDataContext CreateDataContext()
        {
            if (InitConnection != null)
            {
                var ctr = typeof(TDataContext).GetConstructor(new[] { typeof(IDbConnection) });
                if (ctr != null)
                {
                    var dataContext = (TDataContext)ctr.Invoke(new object[] { InitConnection });
                    dataContext.Transaction = InitTransaction;
                    return dataContext;
                }
            }

            return new TDataContext();
        }

        protected override IEnumerable ExecuteSelect(DataSourceSelectArguments arguments)
        {
            if (InitConnection != null && InitTransaction?.Connection == InitConnection) DB.Transaction = InitTransaction;
            WriteTrace(TypeName + ".ExecuteSelect.Begin");
            if (AllowCustomCache)
            {
                if (cacheCount != null)
                    CountData = arguments.TotalRowCount = cacheCount.Value;
                if (cache != null)
                {
                    WriteTrace(TypeName + ".ExecuteSelect.End.ReturnCache");
                    return cache;
                }
            }

            var qParams = new QueryParameters<TDataContext, TTable>(DB) { SupportParameterExpression = SupportGlobalCache };
            InternalQueryParameters = qParams;

            var tableExp = qParams.GetTable();
            BaseFilterControl<TKey, TTable, TDataContext> filterControl = null;
            tableExp = GetFilteredExpression(tableExp, qParams, out filterControl);
            //далее проверки на наличие полнотекстового фильтра
            bool fullTextSorted = false;
            if (filterControl != null)
            {
                //тут хранятся значения фильтров
                var filters = filterControl.GetFilterItemsIternal();
                //здесь сами обработчики фильтров
                var handlers = filterControl.GetFilterHandlers();
                
                //SRH_V_ConclusionCards.FiltersCache
                //FullTextSearchFilter
                foreach (var handler in handlers)
                {
                    var filter = handler.Value;
                    var filterName = handler.Key;
                    var item = filters.ContainsKey(filterName) ? filters[filterName] : null ;
                    if (filter is BaseFilterParameterContent<TTable> && item != null && item.Count > 0 && !string.IsNullOrEmpty(item[0].Value1))
                    {
                        string value = item[0].Value1;
                        BaseFilterParameterContent<TTable> contentFilter = filter as BaseFilterParameterContent<TTable>;
                        tableExp = contentFilter.GetJoinExpression(tableExp, qParams, value);
                        //уже отсортировано по релевантности
                        //fullTextSorted = true;
                    }
                }
            }
            if (UseTreeFilter && !CancelTreeUse)
            {
                var treeFilterControl = (BaseTreeFilterControl<TKey, TTable, TDataContext>)filterControl;
                if (treeFilterControl.UseTree && treeFilterControl.CancelTreeUse && treeFilterControl.ParentID != null)
                {
                    tableExp = treeFilterControl.FilterDataOfChilds(tableExp, qParams);
                }
                else if (treeFilterControl.UseTree && !treeFilterControl.CancelTreeUse)
                {
                    if (treeFilterControl.IsSelectParentRows)
                        tableExp = treeFilterControl.FilterDataOfParents(tableExp, qParams);
                    else
                        tableExp = treeFilterControl.FilterDataOfChilds(tableExp, qParams);
                }
            }

            if (arguments.RetrieveTotalRowCount)
            {
                DB.Connection.SetCommandTextAddToWrappedConn(SelectQueryEndText);
                CountData = arguments.TotalRowCount = GetCount(tableExp, DB, qParams);
                DB.Connection.SetCommandTextAddToWrappedConn("");

                if (owner.EmptyLoad && arguments.TotalRowCount > arguments.MaximumRows)
                {
                    arguments.TotalRowCount = 0;
                    InternalQueryParameters = null;
                    return new TRow[0]; // возврат пустых данных без выполения запроса
                }

                if (AllowCustomCache)
                    cacheCount = CountData = arguments.TotalRowCount;
            }
            else if (owner.EmptyLoad)
            {
                InternalQueryParameters = null;
                return new TRow[0]; // возврат пустых данных без выполения запроса
            }
            //если уже отсортировано по релевантности, больше не сортируем
            if (!fullTextSorted)
            {
                if (!string.IsNullOrEmpty(arguments.SortExpression))
                    tableExp = GetSortedExpression(tableExp, arguments.SortExpression, qParams);
                else
                    tableExp = GetSortedExpression(tableExp, DefaultSort, qParams);
            }
            if (arguments.MaximumRows > 0)
                tableExp = GetPagedExpression(tableExp, arguments.StartRowIndex, arguments.MaximumRows, qParams);
            WriteTrace(TypeName + ".GetSelectedRowExpression.Begin");
            tableExp = GetSelectedRowExpression(tableExp, string.Empty, LocalizationHelper.IsCultureKZ, qParams);
            WriteTrace(TypeName + ".GetSelectedRowExpression.End");

            if (HideRecordCanNotSelected)
            {
                Expression<Func<IQueryable<TRow>, IQueryable<TRow>>> exp = data => data.Where(r => r.CanAddChild);
                tableExp = Expression.Invoke(exp, tableExp);
            }

            List<TRow> returnValue;
            try
            {
                DB.Connection.SetCommandTextAddToWrappedConn(SelectQueryEndText);
                returnValue = qParams.GetCompiled<TRow>(tableExp, SupportGlobalCache).ToList();
                DB.Connection.SetCommandTextAddToWrappedConn("");
            }
            catch (SqlException exception)
            {
                if (exception.Number != -2)
                    throw;
                returnValue = new List<TRow>(0);
                AbstractUserControl.ShowWarningMessage?.Invoke(Resources.SInformationText, Resources.STimeOut);
            }

            EnsureExistRow(returnValue);

            if (AllowCustomCache)
                cache = returnValue;
            OnSelectedDataByQuetyParameters(qParams);
            WriteTrace(TypeName + ".ExecuteSelect.End");
            InternalQueryParameters = null;
            return returnValue;
        }

        public override BaseFilterEventArgs<TTable> GetDeleteFilter(Type typeOfData)
        {
            var filterArgs = new BaseFilterEventArgs<TTable, TDataContext>(null, InternalQueryParameters)
                {
                    TypeOfData = typeOfData
                };
            if (WorkFlows != null)
            {
                foreach (var workFlow in WorkFlows)
                {
                    workFlow.SetDeleteFilters(filterArgs);
                }
            }

            return filterArgs;
        }

        public override BaseFilterEventArgs<TTable> GetEditAndDeleteFilter(Type typeOfData)
        {
            return new BaseFilterEventArgs<TTable, TDataContext>(null, InternalQueryParameters)
                {
                    TypeOfData = typeOfData
                };
        }

        public override BaseFilterEventArgs<TTable> GetEditFilter(Type typeOfData)
        {
            var filterArgs = new BaseFilterEventArgs<TTable, TDataContext>(null, InternalQueryParameters)
                {
                    TypeOfData = typeOfData
                };
            if (WorkFlows != null)
            {
                foreach (var workFlow in WorkFlows)
                {
                    workFlow.SetEditFilters(filterArgs);
                }
            }

            return filterArgs;
        }

        protected override BaseFilterEventArgs<TTable> GetAddRule(Type typeOfData)
        {
            var filterArgs = new BaseFilterEventArgs<TTable, TDataContext>(null, InternalQueryParameters)
                {
                    TypeOfData = typeOfData
                };
            if (WorkFlows != null)
            {
                foreach (var workFlow in WorkFlows)
                {
                    workFlow.SetAddChildsFilters(filterArgs);
                }
            }

            return filterArgs;
        }

        protected virtual IWorkFlow[] CreateWorkFlows()
        {
            return null;
        }

        #region GetData

        public virtual IQueryable<TRow> GetDataByRefParent(TKey? refParent)
        {
            if (InitConnection != null && InitTransaction?.Connection == InitConnection) DB.Transaction = InitTransaction;
            var qParam = Expression.Parameter(typeof(StructQueryParameters), "qParam");
            var dbParam = Expression.Parameter(typeof(TDataContext), "db_DRP");
            var qParams = new QueryParameters<TDataContext, TTable>(qParam, dbParam) { SupportParameterExpression = SupportGlobalCache, DB = DB, InternalDB = DB };
            InternalQueryParameters = qParams;

            var tableExp = (Expression)Expression.Call(dbParam, "GetTable", new[] { typeof(TTable) });

            tableExp = GetFilteredByParent(tableExp, refParent, qParams);
            BaseFilterControl<TKey, TTable, TDataContext> filterControl;
            tableExp = GetFilteredExpression(tableExp, qParams, out filterControl);
            tableExp = GetSortedExpression(tableExp, DefaultSort, qParams);
            tableExp = GetSelectedRowExpression(tableExp, string.Empty, LocalizationHelper.IsCultureKZ, qParams);

            var query = qParams.GetCachedQuery<IQueryable<TRow>>(SupportGlobalCache);
            if (query == null)
            {
                var lambda = Expression.Lambda<Func<TDataContext, StructQueryParameters, IQueryable<TRow>>>(
                    tableExp, dbParam, qParam);
                query = CompiledQuery.Compile(lambda);
                qParams.SetCachedQuery(query, SupportGlobalCache);
            }
            InternalQueryParameters = null;
            return query(DB, qParams.GetExecuteParameter());
        }

        public virtual IQueryable<TRow> GetDataByRefParent(TTable row)
        {
            return GetDataByRefParent(GetID(row));
        }

        protected virtual int GetCount(Expression query, TDataContext db, QueryParameters<TDataContext, TTable> qParams)
        {
            qParams.Action = "Count";
            var cacheQuery = qParams.GetCachedQuery<int>(SupportGlobalCache);
            if (cacheQuery == null)
            {
                Expression<Func<IQueryable<TTable>, int>> exp = data => data.Count();
                var lambda =
                    Expression.Lambda<Func<TDataContext, StructQueryParameters, int>>(
                        Expression.Invoke(exp, query), qParams.DBParameterExpression, qParams.ParameterExpression);
                WriteTrace("Query", "BeginCompileGetCount");
                cacheQuery = CompiledQuery.Compile(lambda);
                WriteTrace("Query", "EndCompileGetCount");
                qParams.SetCachedQuery(cacheQuery, SupportGlobalCache);
            }
            qParams.Action = null;
            WriteTrace("Query", "BeginExecuteGetCount");
            var count = cacheQuery(db, qParams.GetExecuteParameter());
            WriteTrace("Query", "EndExecuteGetCount");
            return count;
        }

        #endregion

        #region filteredExpressions

        protected abstract Expression GetSelectedRowExpression(
            Expression query, string selectColumnName, bool isKz, QueryParameters qParams);

        protected abstract Expression GetFilteredExpression(
            Expression query, QueryParameters qParams, out BaseFilterControl<TKey, TTable, TDataContext> filterControl);

        //todo: сделать сортировку относительно записи Row, для того что бы можно было выполнять сортировку по кастомным полям
        protected virtual Expression GetSortedExpression(
            Expression query, string sortExpression, QueryParameters qParams)
        {
            //WriteTrace(TypeName + ".GetSortedExpression.Begin");
            qParams.OrderBy = sortExpression;
            string str = "OrderBy";
            string str2 = "OrderByDescending";
            foreach (var orderBy in sortExpression.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
            {
                var split = orderBy.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                bool ascending = true;
                if (split.Length == 2)
                    ascending = split[1].Equals("Asc", StringComparison.OrdinalIgnoreCase);

                string orderByColumn = split[0];
                ParameterExpression param = Expression.Parameter(typeof(TTable), "c");
                Type fieldType;
                var property = LinqFilterGenerator.GetProperty(typeof(TTable), orderByColumn, param, out fieldType);
                query = Expression.Call(
                    typeof(Queryable),
                    ascending ? str : str2,
                    new[] { typeof(TTable), fieldType },
                    query,
                    Expression.Lambda(property, param));
                str = "ThenBy";
                str2 = "ThenByDescending";
            }
            //WriteTrace(TypeName + ".GetSortedExpression.End");
            return query;
        }

        protected virtual Expression GetPagedExpression(
            Expression query, int startRowIndex, int maximumRows, QueryParameters qParams)
        {
            qParams.AllowPaging = true;
            qParams.StartRowIndex = startRowIndex;
            qParams.MaximumRows = maximumRows;
            var startRowIndexExp = Expression.Property(qParams.ParameterExpression, "StartRowIndex");
            var maximumRowsExp = Expression.Property(qParams.ParameterExpression, "MaximumRows");
            Expression<Func<IQueryable<TTable>, int, int, IQueryable<TTable>>> exp =
                (data, startIndex, mRows) => data.Skip(startIndex).Take(mRows);
            return Expression.Invoke(exp, query, startRowIndexExp, maximumRowsExp);
        }

        protected virtual Expression GetParentFilteredExpression(
            Expression query, Expression refParent, ParameterExpression dbParam)
        {
            return null;
        }

        protected virtual Expression GetParentFilteredExpression(
            Expression query, Expression refParent1, Expression refParent2, ParameterExpression dbParam)
        {
            return null;
        }

        protected virtual Expression GetFilteredByParent(Expression query, TKey? refParent, QueryParameters qParams)
        {
            qParams.RefParent = refParent;
            var param = Expression.Parameter(typeof(TTable), "row");
            //Expression<Func<StructQueryParameters, long?>> expGetRefParent = qp => qp.RefParentLong;
            Expression refParentExp = Expression.Property(param, "refParent");
            Expression refParentParam;
            if (refParent == null)
            {
                refParentParam = Expression.Constant(null, typeof(TKey?));
                qParams.AllowFileterByRefParentIsNull = true;
            }
            else
            {
                refParentParam = Expression.Property(qParams.ParameterExpression, "RefParentLong");
                qParams.AllowFileterByRefParent = true;
            }
            //Expression refParentParam = Expression.Convert(Expression.Property(qParams.ParameterExpression, "RefParent"), typeof(TKey?));
            //Expression refParentParam = Expression.Invoke(expGetRefParent, qParams.ParameterExpression);
            //Expression refParentParam = qParams.GetExpression("refParent.Equals", (long?)(object)refParent);
            refParentExp = Expression.Equal(refParentExp, refParentParam);
            var pred = Expression.Lambda(refParentExp, param);
            return Expression.Call(typeof(Queryable), "Where", new[] { typeof(TTable) }, query, pred);
        }

        public virtual Expression<Func<TDataContext, IQueryable<TTable>>> GetFilteredSimpleExpression(
            Expression<Func<TTable>> exp)
        {
            //Expression<Func<TDataContext, IQueryable<TTable>>> query = db => db.GetTable<TTable>();
            /* var dbParam = Expression.Parameter(typeof(TDataContext), "db_FSE");
             var tableExp = (Expression)Expression.Call(dbParam, "GetTable", new[] { typeof(TTable) });
             tableExp = GetFilteredExpression(tableExp, dbParam);
             tableExp = GetSortedExpression(tableExp, DefaultSort);*/
            return null;
        }

        #endregion

        #endregion

        #region mehtods

        public IQueryable<TRow> GetSelect(string selectColumnName, bool isKz)
        {
            return GetSelect(selectColumnName, DefaultSort, isKz, false);
        }

        public IQueryable<TRow> GetSelect(string selectColumnName)
        {
            return GetSelect(selectColumnName, DefaultSort, LocalizationHelper.IsCultureKZ, false);
        }

        public IQueryable<TRow> GetSelect(bool isKz, string sortExpression)
        {
            return GetSelect(null, sortExpression, isKz, false);
        }

        public IQueryable<TRow> GetSelect(bool isKz)
        {
            return GetSelect(null, DefaultSort, isKz, false);
        }

        public IQueryable<TRow> GetSelect()
        {
            return GetSelect(null, DefaultSort, LocalizationHelper.IsCultureKZ, false);
        }

        public override IQueryable GetSelectIQueryable()
        {
            return this.GetSelect();
        }

        public override IQueryable GetSelectIQueryableWithoutFilters()
        {
            return this.GetSelectWithoutFilters();
        }

        public override IQueryable GetSelectIQueryableWithoutFilters(
            IEnumerable<string> selectedValues, string selectColumnName, string sortExpression, bool isKz)
        {
            return GetSelectWithoutFilters(selectedValues, selectColumnName, sortExpression, isKz);
        }

        public IQueryable<TRow> GetSelect(string selectColumnName, string sortExpression, bool isKz, bool useTreeFilter)
        {
            if (InitConnection != null && InitTransaction?.Connection == InitConnection) DB.Transaction = InitTransaction;
            Expression tableExp;
            var qParams = CreateQuery(selectColumnName, sortExpression, isKz, useTreeFilter, null, out tableExp);
            var lambda = Expression.Lambda<Func<TDataContext, StructQueryParameters, IQueryable<TRow>>>(
                tableExp, qParams.DBParameterExpression, qParams.ParameterExpression);
            InternalQueryParameters = null;
            var expression = Expression.Invoke(
                lambda, Expression.Constant(DB), Expression.Constant(qParams.GetExecuteParameter()));
            return ((IQueryable)DB.GetTable<TTable>()).Provider.CreateQuery<TRow>(expression);
        }


        public TRow GetCacheSelect(TKey id)
        {
            if (InitConnection != null && InitTransaction?.Connection == InitConnection) DB.Transaction = InitTransaction;
            Expression tableExp;
            var qParams = CreateQuery(
                string.Empty,
                string.Empty,
                LocalizationHelper.IsCultureKZ,
                false,
                qp =>
                    {
                        var idValue = qp.GetExpression("id", id);
                        var tParam = Expression.Parameter(typeof(TTable), "r");
                        var whereId = Expression.Invoke(GetWhereByKey(), tParam, idValue);
                        return Expression.Lambda<Func<TTable, bool>>(whereId, tParam);
                    },
                out tableExp);
            
            InternalQueryParameters = null;
            qParams.DB = DB;
            return qParams.GetCompiled<TRow>(tableExp, SupportGlobalCache).FirstOrDefault();
        }


        protected QueryParameters<TDataContext, TTable> CreateQuery(string selectColumnName, string sortExpression, bool isKz, bool useTreeFilter, Func<QueryParameters<TDataContext, TTable>, Expression<Func<TTable, bool>>> getWhere, out Expression tableExp)
        {
            if (InitConnection != null && InitTransaction?.Connection == InitConnection) DB.Transaction = InitTransaction;
            var qParam = Expression.Parameter(typeof(StructQueryParameters), "qParam");
            var dbParam = Expression.Parameter(typeof(TDataContext), "db_FSE");
            var qParams = new QueryParameters<TDataContext, TTable>(qParam, dbParam) { SupportParameterExpression = SupportGlobalCache, DB = DB, InternalDB = DB };
            InternalQueryParameters = qParams;

            tableExp = (Expression)Expression.Call(dbParam, "GetTable", new[] { typeof(TTable) });
            BaseFilterControl<TKey, TTable, TDataContext> filterControl = null;
            tableExp = GetFilteredExpression(tableExp, qParams, out filterControl);
            if (useTreeFilter && UseTreeFilter)
            {
                var treeFilterControl = (BaseTreeFilterControl<TKey, TTable, TDataContext>)filterControl;
                if (treeFilterControl.UseTree && treeFilterControl.CancelTreeUse && treeFilterControl.ParentID != null)
                {
                    tableExp = treeFilterControl.FilterDataOfChilds(tableExp, qParams);
                    // GetFilteredByParent(tableExp, treeFilterControl.ParentID, qParams);
                }
                else if (treeFilterControl.UseTree && !treeFilterControl.CancelTreeUse)
                {
                    if (treeFilterControl.IsSelectParentRows)
                        tableExp = treeFilterControl.FilterDataOfParents(tableExp, qParams);
                    else
                        tableExp = treeFilterControl.FilterDataOfChilds(tableExp, qParams);
                    //tableExp = GetFilteredByParent(tableExp, treeFilterControl.IsSelectParentRows ? null : treeFilterControl.ParentID, qParams);
                }
            }

            if (getWhere != null)
                tableExp = Expression.Call(typeof(Queryable), "Where", new[] { typeof(TTable) }, tableExp, getWhere(qParams));

            if (!string.IsNullOrEmpty(sortExpression))
                tableExp = GetSortedExpression(tableExp, sortExpression, qParams);

            tableExp = GetSelectedRowExpression(tableExp, selectColumnName, isKz, qParams);

            return qParams;
        }

        public QueryParameters<TDataContext, TTable> GetQueryParameters(
            string selectColumnName, string sortExpression, bool isKz, bool useTreeFilter)
        {
            if (InitConnection != null && InitTransaction?.Connection == InitConnection) DB.Transaction = InitTransaction;
            var qParams = new QueryParameters<TDataContext, TTable>(DB) { SupportParameterExpression = SupportGlobalCache };
            InternalQueryParameters = qParams;

            var tableExp = qParams.GetTable();
            BaseFilterControl<TKey, TTable, TDataContext> filterControl;
            tableExp = GetFilteredExpression(tableExp, qParams, out filterControl);
            if (useTreeFilter && UseTreeFilter)
            {
                var treeFilterControl = (BaseTreeFilterControl<TKey, TTable, TDataContext>)filterControl;
                if (treeFilterControl.UseTree && treeFilterControl.CancelTreeUse && treeFilterControl.ParentID != null)
                    tableExp = treeFilterControl.FilterDataOfChilds(tableExp, qParams);
                else if (treeFilterControl.UseTree && !treeFilterControl.CancelTreeUse)
                {
                    tableExp = treeFilterControl.IsSelectParentRows
                                   ? treeFilterControl.FilterDataOfParents(tableExp, qParams)
                                   : treeFilterControl.FilterDataOfChilds(tableExp, qParams);
                }
            }
            if (!string.IsNullOrEmpty(sortExpression))
                tableExp = GetSortedExpression(tableExp, sortExpression, qParams);
            tableExp = GetSelectedRowExpression(tableExp, selectColumnName, isKz, qParams);
            qParams.CurrentExpression = tableExp;
            InternalQueryParameters = null;
            return qParams;
        }

        public IQueryable<TRow> GetSelectWithoutFilters()
        {
            return GetSelectWithoutFilters(null, null, LocalizationHelper.IsCultureKZ);
        }

        public IQueryable<TRow> GetSelectWithoutFilters(string selectColumnName)
        {
            return GetSelectWithoutFilters(selectColumnName, null, LocalizationHelper.IsCultureKZ);
        }

        public IQueryable<TRow> GetSelectWithoutFilters(string selectColumnName, string sortExpression, bool isKz)
        {
            if (InitConnection != null && InitTransaction?.Connection == InitConnection) DB.Transaction = InitTransaction;
            var qParam = Expression.Parameter(typeof(StructQueryParameters), "qParam");
            var dbParam = Expression.Parameter(typeof(TDataContext), "db_FSE");
            var qParams = new QueryParameters<TDataContext, TTable>(qParam, dbParam) { SupportParameterExpression = SupportGlobalCache, DB = DB, InternalDB = DB };
            InternalQueryParameters = qParams;

            var tableExp = (Expression)Expression.Call(dbParam, "GetTable", new[] { typeof(TTable) });
            BaseFilterControl<TKey, TTable, TDataContext> filterControl = null;
            if (!string.IsNullOrEmpty(sortExpression))
                tableExp = GetSortedExpression(tableExp, sortExpression, qParams);
            else
                tableExp = GetSortedExpression(tableExp, DefaultSort, qParams);
            tableExp = GetSelectedRowExpression(tableExp, selectColumnName, isKz, qParams);
            var lambda = Expression.Lambda<Func<TDataContext, StructQueryParameters, IQueryable<TRow>>>(
                tableExp, dbParam, qParam);
            InternalQueryParameters = null;
            return
                ((IQueryable)DB.GetTable<TTable>()).Provider.CreateQuery<TRow>(
                    Expression.Invoke(
                        lambda, Expression.Constant(DB), Expression.Constant(qParams.GetExecuteParameter())));
        }

        public IQueryable<TRow> GetSelectWithoutFilters(
            IEnumerable<string> selectedValues, string selectColumnName, string sortExpression, bool isKz)
        {
            var values = selectedValues.Select(r => (TKey)GetKey(r));
            return GetSelectWithoutFilters(values, selectColumnName, sortExpression, isKz);
        }

        public IQueryable<TRow> GetSelectWithoutFilters(
            IEnumerable<TKey> selectedValues, string selectColumnName, string sortExpression, bool isKz)
        {
            if (InitConnection != null && InitTransaction?.Connection == InitConnection) DB.Transaction = InitTransaction;
            var qParam = Expression.Parameter(typeof(StructQueryParameters), "qParam");
            var dbParam = Expression.Parameter(typeof(TDataContext), "db_FSE");
            var qParams = new QueryParameters<TDataContext, TTable>(qParam, dbParam) { SupportParameterExpression = SupportGlobalCache, DB = DB, InternalDB = DB };
            InternalQueryParameters = qParams;

            var tableExp = (Expression)Expression.Call(dbParam, "GetTable", new[] { typeof(TTable) });
            if (selectedValues != null)
            {
                var whereByKey = GetWhereByKey();
                Expression expFilterByKey = null;
                var param = Expression.Parameter(typeof(TTable), "rowSV");
                foreach (var value in selectedValues)
                {
                    var filterByKey = Expression.Invoke(whereByKey, param, qParams.GetExpression(whereByKey, value));
                    if (expFilterByKey == null)
                        expFilterByKey = filterByKey;
                    else
                        expFilterByKey = Expression.Or(expFilterByKey, filterByKey);
                }
                if (expFilterByKey != null)
                {
                    var lambdaWhere = Expression.Lambda(expFilterByKey, param);
                    tableExp = Expression.Call(
                        typeof(Queryable),
                        "Where",
                        new[] { typeof(TTable) },
                        tableExp,
                        lambdaWhere);
                }
            }
            BaseFilterControl<TKey, TTable, TDataContext> filterControl = null;
            if (!string.IsNullOrEmpty(sortExpression))
                tableExp = GetSortedExpression(tableExp, sortExpression, qParams);
            else
                tableExp = GetSortedExpression(tableExp, DefaultSort, qParams);
            tableExp = GetSelectedRowExpression(tableExp, selectColumnName, isKz, qParams);
            var lambda = Expression.Lambda<Func<TDataContext, StructQueryParameters, IQueryable<TRow>>>(
                tableExp, dbParam, qParam);
            InternalQueryParameters = null;
            return
                ((IQueryable)DB.GetTable<TTable>()).Provider.CreateQuery<TRow>(
                    Expression.Invoke(
                        lambda, Expression.Constant(DB), Expression.Constant(qParams.GetExecuteParameter())));
        }

        public virtual Expression<Func<TTable, TKey, bool>> GetWhereByKey()
        {
            throw new NotImplementedException(
                "Не реализован метод GetWhereByKey, необходимо перегенерить журнал " + typeof(TTable).Name);
        }

        protected void OnSelectedDataByQuetyParameters(QueryParameters<TDataContext, TTable> parameters)
        {
            if (owner != null)
                owner.OnSelectedQueryParameters(new SelectedQueryParametersEventArgs { QueryParameters = parameters });
        }

        private void EnsureExistRow(List<TRow> rows)
        {
            if (AllowCreateSelectedRow
                && _selectedID != null
                && rows.FirstOrDefault(r => r.Value == _selectedID.ToString()) == null)
            {
                var row = new TRow
                    {
                        Value = _selectedID.ToString(),
                        nameRu = Resources.SNotExistRow,
                        nameKz = Resources.SNotExistRow,
                    };
                row.SetValue(_selectedID.ToString());
                rows.Add(row);
            }
        }

        public Expression<Func<TRow, TTable>> GetToItemExpression()
        {
            var param = Expression.Parameter(typeof(TRow), "r");
            return Expression.Lambda<Func<TRow, TTable>>(Expression.Property(param, "Item"), param);
        }

        public Func<TRow, TTable> GetToItemFunc()
        {
            return GetToItemExpression().Compile();
        }

        #endregion
    }
}
