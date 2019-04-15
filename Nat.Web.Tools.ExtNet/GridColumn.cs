/*
* Created by: Sergey V. Shpakovskiy
* Created: 2012.09.27
* Copyright © JSC NAT Kazakhstan 2012
*/

namespace Nat.Web.Tools.ExtNet
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Web;
    using System.Web.UI;
    using System.Web.UI.WebControls;

    using Ext.Net;

    using Nat.Web.Controls.ExtNet;
    using Nat.Web.Controls.GenerationClasses;
    using Nat.Web.Controls.Properties;
    using Nat.Web.Tools.ExtNet.Extenders;
    using Nat.Web.Tools.ExtNet.ExtNetConfig;
    using Nat.Web.Tools.Initialization;

    /// <summary>
    /// Колонка для формирования колонок и фильтров в ExtNet.GridPanel.
    /// </summary>
    public class GridColumn : GridHtmlGenerator.Column, IGridColumn
    {
        private const string ComboBoxRenderHandler = "if (Ext.isEmpty(value)) return ''; var comboStore = #{{{0}}}; var r = comboStore.getById(value); if (Ext.isEmpty(r)) return record.data.{1}; return r.data.RowName;";
        private bool? showInGrid;
        private Func<object, object> getValueForExport;
        private string columnNameIndex;
        private string columnNameIndexOriginal;
        private int? flex = null;

        public int  Flex
        {
            get
            {
                if (flex == null)
                {
                    if (ModelFieldType == ModelFieldType.Date)
                        return 0;
                    return 1;
                }

                return flex.Value;
            }
            set
            {
                flex = value;
                Width = string.Empty;
            }
        }

        /// <summary>
        /// Для редактирования boolean значений без фокуса на ячейки, сразу рендерится checkbox.
        /// </summary>
        public bool EditModeForBool { get; set; }

        /// <summary>
        /// Текст для битового поля со значением false.
        /// </summary>
        public string FalseText { get; set; }

        /// <summary>
        /// Текст для битового поля со значением true.
        /// </summary>
        public string TrueText { get; set; }

        /// <summary>
        /// Колонка с большим объемом данных, подгрузка через автокомплит или выбор из журнала.
        /// </summary>
        public bool IsLookup { get; set; }

        /// <summary>
        /// Колонка с большим объемом данных в фильтре, подгрузка через автокомплит или выбор из журнала.
        /// </summary>
        public bool IsFilterLookup { get; set; }

        /// <summary>
        /// Колонка ссылочная.
        /// </summary>
        public bool IsForeignKey { get; set; }

        public bool ShowInGrid
        {
            get { return showInGrid ?? Visible; }
            set { showInGrid = value; }
        }

        /// <summary>
        /// Колонка в которой рисуется дерево. Для древовидных журналов.
        /// </summary>
        public bool IsTreeColumn { get; set; }

        /// <summary>
        /// Для итогов указывается тип аггригции.
        /// </summary>
        public SummaryType? SummaryType { get; set; }

        public string SummaryRendererHandler { get; set; }

        public string CustomSummaryType { get; set; }

        /// <summary>
        /// При создании колонки указывается Renderer.Handler. 
        /// Функция создается: function (value, metadata, record, rowIndex, colIndex, store, view).
        /// Содержимое функции заполняется в свойстве.
        /// </summary>
        public string RendererFunction { get; set; }

        /// <summary>
        /// Идентификатор источника данных для ComboBox.
        /// </summary>
        public string StoreId { get; set; }

        public bool DefaultHidden { get; set; }

        public bool ChartColumn { get; set; }

        public IListConfig ComboBoxView { get; set; }

        /// <summary>
        /// Функция для создания колонки журнала.
        /// </summary>
        public Func<GridColumn, ColumnBase> CreateColumnHandler { get; set; }

        public Action<ColumnBase> ConfigureColumnHandler { get; set; }

        /// <summary>
        /// Функция для создания фильтра журнала.
        /// </summary>
        public Func<GridColumn, GridFilter> CreateFilterHandler { get; set; }

        public Action<BaseFilterEventArgs, FilterCondition> SetGridFilterHandler { get; set; }

        /// <summary>
        /// Функция для создания модели данных журнала.
        /// </summary>
        public Func<GridColumn, IEnumerable<ModelField>> CreateModelFieldsHandler { get; set; }

        public Func<Field> CreateEditorHandler { get; set; }

        public Func<object, string> GetValueForExportHandler { get; set; }

        #region IGridColumn Members

        public string FilterColumnMapping { get; set; }

        /// <summary>
        /// Строка доступа к полю с данными, для вывода пользователю.
        /// </summary>
        public string ServerMapping { get; set; }

        /// <summary>
        /// Строка доступа к полю с данными, которая хранит значение (ключ/ref) ссылочного поля.
        /// </summary>
        public string ServerMappingRefValue { get; set; }

        /// <summary>
        /// Имя колонки/поля с данными, которая хранит значение (ключ/ref) ссылочного поля на клиенте.
        /// </summary>
        public string ColumnNameIndexRefValue { get; set; }

        /// <summary>
        /// Имя колонки/поля с данными, которая хранит значение текстовое значение ссылочного поля на клиенте.
        /// </summary>
        public string ColumnNameIndexOriginal
        {
            get { return columnNameIndexOriginal ?? ColumnNameIndex; }
            set { columnNameIndexOriginal = value; }
        }

        public string ColumnNameIndex
        {
            get
            {
                return columnNameIndex ?? ColumnName;
            }

            set
            {
                if (string.IsNullOrEmpty(columnNameIndexOriginal) && !string.IsNullOrEmpty(columnNameIndex))
                    columnNameIndexOriginal = columnNameIndex;
                columnNameIndex = value;
            }
        }

        /// <summary>
        /// Тип поля.
        /// </summary>
        public ModelFieldType ModelFieldTypeRefValue { get; set; }

        Unit IGridColumn.Width
        {
            get { return new Unit(Width); }
        }

        /// <summary>
        /// Тип поля.
        /// </summary>
        public ModelFieldType ModelFieldType { get; set; }
       
        /// <summary>
        /// Есть ли фильтр по полю.
        /// </summary>
        public bool HasFilter { get; set; }

        /// <summary>
        /// Имеются ли дочерние колонки.
        /// </summary>
        public bool HasChildren
        {
            get { return ChildColumns != null && ChildColumns.Count > 0; }
        }

        IEnumerable<IGridColumn> IGridColumn.Children
        {
            get { return ChildColumns.Cast<IGridColumn>(); }
            set { ChildColumns = value.Cast<GridHtmlGenerator.Column>().ToList(); }
        }

        public virtual ColumnBase CreateColumn()
        {
            ColumnBase column;
            if (HasChildren)
            {
                column = new Column { Text = Header };
                
                if (ConfigureColumnHandler != null)
                    ConfigureColumnHandler(column);

                return column;
            }
            
            var editor = CreateEditor();

            if (CreateColumnHandler != null)
            {
                column = CreateColumnHandler(this);
                if (editor != null)
                    column.Editor.Add(editor);

                if (ConfigureColumnHandler != null)
                    ConfigureColumnHandler(column);
                
                return column;
            }

            if (IsTreeColumn)
            {
                column = new TreeColumn
                    {
                        DataIndex = ColumnNameIndex,
                        Text = Header,
                        Width = new Unit(Width),
                        Flex = string.IsNullOrEmpty(Width) ? Flex : 0,
                        Sortable = !string.IsNullOrEmpty(Sort),
                    };
            }
            else
            {
                switch (ModelFieldType)
                {
                    case ModelFieldType.Auto:
                        column = new Column
                            {
                                DataIndex = ColumnNameIndex,
                                Text = Header,
                                Width = new Unit(Width),
                                Flex = string.IsNullOrEmpty(Width) ? Flex : 0,
                                Hidden = DefaultHidden,
                                Sortable = !string.IsNullOrEmpty(Sort),
                            };
                        break;
                    case ModelFieldType.String:
                        column = new Column
                            {
                                DataIndex = ColumnNameIndex,
                                Text = Header,
                                Width = new Unit(Width),
                                Flex = string.IsNullOrEmpty(Width) ? Flex : 0,
                                Hidden = DefaultHidden,
                                Wrap = Wrap ?? true,
                                Sortable = !string.IsNullOrEmpty(Sort),
                            };
                        break;

                    case ModelFieldType.Int:
                        var intColumn = new NumberColumn
                            {
                                DataIndex = ColumnNameIndex,
                                Text = Header,
                                Width = new Unit(Width),
                                Flex = string.IsNullOrEmpty(Width) ? Flex : 0,
                                Hidden = DefaultHidden,
                                Sortable = !string.IsNullOrEmpty(Sort),
                            };
                        if (!string.IsNullOrEmpty(Format))
                            intColumn.Format = GetIntFormat();
                        intColumn.Align = Alignment.Right;
                        //intColumn.Renderer.Handler = "debugger;return record.raw[metadata.column.dataIndex] == null && !value ? '' : value;";
                        column = intColumn;
                        break;

                    case ModelFieldType.Float:
                        var floatColumn = new NumberColumn
                            {
                                DataIndex = ColumnNameIndex,
                                Text = Header,
                                Flex = string.IsNullOrEmpty(Width) ? Flex : 0,
                                Hidden = DefaultHidden,
                                Sortable = !string.IsNullOrEmpty(Sort),
                            };
                        if (!string.IsNullOrEmpty(Format))
                            floatColumn.Format = GetFloatFormat();
                        floatColumn.Align = Alignment.Right;
                        //floatColumn.Renderer.Handler = "debugger;return record.raw[metadata.column.dataIndex] == null && !value ? '' : value;";
                        column = floatColumn;
                        break;

                    case ModelFieldType.Boolean:
                        if (EditModeForBool && CanEdit)
                        {
                            column = new CheckColumn
                                {
                                    DataIndex = ColumnNameIndex,
                                    Text = Header,
                                    StopSelection = false,
                                    Editable = CanEdit,
                                    Width = new Unit(Width),
                                    Flex = string.IsNullOrEmpty(Width) ? Flex : 0,
                                    Hidden = DefaultHidden,
                                    Sortable = !string.IsNullOrEmpty(Sort),
                                };
                            
                            if (column.Renderer == null)
                                column.Renderer = new Renderer();
                            column.Renderer.Handler = string.Format(
                                "if (!record.data.CanEdit) return '<span style=\"font-size:11px\">' + (record.data.{0} ? {1} : {2}) + '</span>'; return (new Ext.ux.CheckColumn()).renderer(record.data.{0});",
                                ColumnNameIndex,
                                JSON.Serialize(TrueText),
                                JSON.Serialize(FalseText));
                        }
                        else
                        {
                            column = new BooleanColumn
                                {
                                    DataIndex = ColumnNameIndex,
                                    Text = Header,
                                    TrueText = TrueText,
                                    FalseText = FalseText,
                                    Width = new Unit(Width),
                                    Flex = string.IsNullOrEmpty(Width) ? Flex : 0,
                                    Hidden = DefaultHidden,
                                    Sortable = !string.IsNullOrEmpty(Sort),
                                };
                        }

                        break;

                    case ModelFieldType.Date:
                        column = new DateColumn
                            {
                                DataIndex = ColumnNameIndex,
                                Text = Header,
                                Format = GetDateFormat(),
                                Width = new Unit(Width),
                                Flex = string.IsNullOrEmpty(Width) ? Flex : 0,
                                Hidden = DefaultHidden,
                                Sortable = !string.IsNullOrEmpty(Sort),
                            };
                        break;

                    default:
                        throw new ArgumentOutOfRangeException();
                }

                if (SummaryType != null)
                {
                    column.SummaryType = SummaryType.Value;
                    column.SummaryRenderer = new Renderer {Handler = SummaryRendererHandler};
                    column.CustomSummaryType = CustomSummaryType;
                }
            }

            if (editor != null)
                column.Editor.Add(editor);

            if (!string.IsNullOrEmpty(RendererFunction))
            {
                var handler = RendererFunction.Contains(" ") ?
                    $"function (value, metadata, record, rowIndex, colIndex, store, view) {{\n{RendererFunction}\n}}"
                    : RendererFunction;
                column.Renderer = new Renderer { Handler = handler, };
            }

            ConfigureColumnHandler?.Invoke(column);

            return column;
        }

        public bool? Wrap { get; set; }

        /// <summary>
        /// Для FilterType.Numeric, настраевает  максимальное количество символов на дробную часть
        /// </summary>
        public int DecimalPrecision { get; set; }

        public int DecimalLength { get; set; }

        public int StringMaxLength { get; set; }

        public bool UseNull { get; set; }

        public virtual GridFilter CreateFilter()
        {
            if (HasChildren || !HasFilter)
                return null;

            if (CreateFilterHandler != null)
                return CreateFilterHandler(this);

            var isReadArgument = (HttpContext.Current.Request.Form["__EVENTARGUMENT"] ?? "").EndsWith("|postback|read");

            if (IsForeignKey && isReadArgument)
                return new ListFilter
                    {
                        DataIndex = ColumnNameIndex,
                        Options = new string[0],
                        LoadingText = Resources.SLoading,
                    };

            if (IsForeignKey && !IsFilterLookup)
                EnsureDataSourceOtherFilled(50);
            if (DataSourceOther != null && DataSourceOther.Any())
            {
                return new ListFilter
                    {
                        DataIndex = ColumnNameIndex,
                        Options = DataSourceOther.Select(r => r.Value).ToArray(),
                        LoadingText = Resources.SLoading,
                    };
            }

            if (IsForeignKey && DataSource != null && !IsLookup)
            {
                IEnumerable data = null;
                DataSource.GetView("").Select(new DataSourceSelectArguments { MaximumRows = 50 }, c => data = c);
                return new ListFilter
                    {
                        DataIndex = ColumnNameIndex,
                        Options = data.Cast<IDataRow>().Select(r => r.Name).ToArray(),
                        LoadingText = Resources.SLoading,
                    };
            }

            switch (ModelFieldType)
            {
                case ModelFieldType.Auto:
                case ModelFieldType.String:
                    return new StringFilter
                        {
                            DataIndex = ColumnNameIndex,
                        };
                case ModelFieldType.Float:
                case ModelFieldType.Int:
                    return new NumericFilter
                        {
                            DataIndex = ColumnNameIndex,
                            CustomConfig =
                                {
                                    new ConfigItem(
                                        "fieldCfg",
                                        $"{{ lt: {{ decimalPrecision: {DecimalPrecision} }}, gt: {{ decimalPrecision: {DecimalPrecision} }}, eq: {{ decimalPrecision: {DecimalPrecision} }} }}",
                                        ParameterMode.Raw)
                                }
                        };
                case ModelFieldType.Boolean:
                    return new BooleanFilter
                        {
                            DataIndex = ColumnNameIndex,
                            YesText = TrueText,
                            NoText = FalseText,
                        };
                case ModelFieldType.Date:
                    return new DateFilter
                        {
                            DataIndex = ColumnNameIndex,
                            Format = GetDateFormat(),
                            AfterText = InitializerSection.StaticFilterNamesResources.SDateMore,
                            BeforeText = InitializerSection.StaticFilterNamesResources.SDateLess,
                            OnText = InitializerSection.StaticFilterNamesResources.SEquals,
                        };
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public virtual IEnumerable<ModelField> CreateModelFields()
        {
            if (HasChildren)
                return new ModelField[0];

            if (CreateModelFieldsHandler != null)
                return CreateModelFieldsHandler(this);

            var modelField = new ModelField(ColumnNameIndexOriginal, ModelFieldType)
                {
                    ServerMapping = ServerMapping,
                    UseNull = UseNull
                };

            if (string.IsNullOrEmpty(ServerMappingRefValue))
                return new[] { modelField, };

            return new[]
                {
                    modelField,
                    new ModelField(ColumnNameIndexRefValue, ModelFieldTypeRefValue)
                        {
                            ServerMapping = ServerMappingRefValue,
                            UseNull = UseNull
                        }
                };
        }

        protected override string GetValueForExport(object row)
        {
            if (GetValueForExportHandler != null)
                return GetValueForExportHandler(row);
            if (getValueForExport == null)
            {
                var param = Expression.Parameter(typeof(object), "row");
                string[] split = ServerMapping.Split(new[] { '.' }, StringSplitOptions.RemoveEmptyEntries);
                Expression exp = Expression.Convert(param, row.GetType());
                foreach (var field in split)
                {
                    // обработка массива
                    if (field.EndsWith("]"))
                    {
                        var startIndex = field.IndexOf("[", StringComparison.Ordinal);
                        var indexStr = field.Substring(startIndex + 1, field.Length - startIndex - 2);
                        exp = Expression.PropertyOrField(exp, field.Substring(0, startIndex));
                        if (typeof(IDictionary).IsAssignableFrom(exp.Type))
                        {
                            var itemParams = exp.Type.GetProperty("Item").GetIndexParameters();
                            exp = Expression.Property(exp, "Item", Expression.Constant(Convert.ChangeType(indexStr, itemParams[0].ParameterType)));
                        }
                        else
                            exp = Expression.ArrayIndex(exp, Expression.Constant(Convert.ToInt32(indexStr)));
                    }
                    else
                        exp = Expression.PropertyOrField(exp, field);
                }

                if (ModelFieldType == ModelFieldType.Boolean)
                {
                    if (exp.Type == typeof(bool?))
                        exp = Expression.Condition(Expression.Equal(exp, Expression.Constant(null, typeof(bool?))),
                            Expression.Constant(""),
                            Expression.Condition(Expression.Property(exp, "Value"),
                                Expression.Constant(TrueText),
                                Expression.Constant(FalseText)));
                    else
                        exp = Expression.Condition(exp, Expression.Constant(TrueText), Expression.Constant(FalseText));
                }
                exp = Expression.Convert(exp, typeof(object));

                var lambda = Expression.Lambda<Func<object, object>>(exp, param);
                getValueForExport = lambda.Compile();
            }

            var value = getValueForExport(row);
            if (value == null)
                return null;

            if (string.IsNullOrEmpty(Format))
                return value.ToString();
            
            return string.Format(Format, value);
        }

        protected virtual Field CreateEditor()
        {
            if (!CanEdit || HasChildren)
                return null;

            if (CreateEditorHandler != null)
                return CreateEditorHandler();

            switch (ModelFieldType)
            {
                case ModelFieldType.Auto:
                    return null;
                case ModelFieldType.String:
                    var textField = new TextField { ID = "gridEditorField" + ColumnName };
                    if (StringMaxLength > 0)
                    {
                        textField.MaxLength = StringMaxLength;
                        textField.MaxLengthText = string.Format(Resources.SMaxLength, StringMaxLength);
                    }
                    return textField;
                case ModelFieldType.Int:
                    return new NumberField { ID = "gridEditorField" + ColumnName, DecimalPrecision = 0 };
                case ModelFieldType.Float:
                    var numberField = new NumberField
                        {
                            ID = "gridEditorField" + ColumnName,
                            DecimalPrecision = DecimalPrecision,
                        };
                    if (MaxValue != null)
                        numberField.MaxValue = MaxValue.Value;
                    else if (DecimalLength > 0)
                        numberField.MaxValue = Math.Pow(10, DecimalLength - DecimalPrecision) - Math.Pow(0.1, DecimalPrecision);
                    
                    if (MinValue != null)
                        numberField.MinValue = MinValue.Value;
                    else if (DecimalLength > 0)
                        numberField.MinValue = -(Math.Pow(10, DecimalLength - DecimalPrecision) - Math.Pow(0.1, DecimalPrecision));

                    return numberField;
                case ModelFieldType.Boolean:
                    if (EditModeForBool)
                        return null;
                    return new Checkbox { ID = "gridEditorField" + ColumnName };
                case ModelFieldType.Date:
                    return new DateField
                        {
                            ID = "gridEditorField" + ColumnName,
                            Format = GetDateFormat(),
                        };
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        #endregion

        public void SetEditorComboBox()
        {
            ColumnNameIndex = ColumnNameIndexRefValue;
            CreateEditorHandler = CreateComboboxEditor;
            RendererFunction = string.Format(ComboBoxRenderHandler, StoreId, ColumnNameIndexOriginal);
        }

        public Field CreateComboboxEditor()
        {
            return CreateComboboxEditor(IsLookup);
        }

        public Field CreateComboboxEditor(bool isLookup)
        {
            ComboBox comboBox;
            if (isLookup)
            {
                comboBox = new LookupBox
                    {
                        ID = "gridEditorField" + ColumnName,
                        DisplayField = "RowName",
                        ValueField = "id",
                        StoreID = StoreId,
                        QueryCaching = false,
                        TableName = TableName,
                        LookupFiltersID = "Hidden" + ColumnNameIndexRefValue + "BrowseFilterParams",
                        WindowID = ColumnNameIndexRefValue + "ModalWindow",
                        NullValue = "0",
                        WindowTitle = TableTitle,
                    };
            }
            else
            {
                comboBox = new ComboBox
                    {
                        ID = "gridEditorField" + ColumnName,
                        DisplayField = "RowName",
                        ValueField = "id",
                        StoreID = StoreId,
                        Editable = false,
                        QueryMode = DataLoadMode.Local,
                    };
            }

            comboBox.Listeners.Change.Handler =
                string.Format(
                    "var r = Ext.isEmpty(newValue) ? null : #{{{0}}}.getById(newValue); #{{grid}}.selModel.getSelection()[0].set('{1}', Ext.isEmpty(r) ? '' : r.data.RowName);",
                    StoreId,
                    ColumnNameIndexOriginal);

            comboBox.InitializeListConfig(ComboBoxView);

            return comboBox;
        }

        #region methods

        private string GetIntFormat()
        {
            if (string.IsNullOrEmpty(Format) || Format.Equals("{0}")) return "0";
            return Format.Substring(3, Format.Length - 4);
        }

        private string GetFloatFormat()
        {
            if (string.IsNullOrEmpty(Format) || Format.Equals("{0}")) return "";
            return Format.Substring(3, Format.Length - 4);
        }

        private string GetDateFormat()
        {
            if (string.IsNullOrEmpty(Format) || Format.Equals("{0}")) return "dd.MM.yyyy";
            if ("{0:g}".Equals(Format)) return "dd.MM.yyyy HH:mm";
            return Format.Substring(3, Format.Length - 4);
        }

        #endregion
    }
}
