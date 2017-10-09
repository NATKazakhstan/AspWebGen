using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.UI;
using System.ComponentModel;
using Nat.Web.Controls.GenerationClasses.HierarchyFields;

namespace Nat.Web.Controls.GenerationClasses.BaseJournal
{
    #region delegates

    public delegate bool GetResultBool(RenderContext context);

    public delegate TResult GetResult<TResult>(RenderContext context) where TResult : class;

    public delegate TResult GetResultStruct<TResult>(RenderContext context) where TResult : struct;

    public delegate TResult GetResult1<TValue, TResult>(RenderContext context, TValue value)
        where TResult : class;

    public delegate TResult GetResult<TValue, TResult>(RenderContext context, TValue value)
        where TValue : EventArgs
        where TResult : class;

    public delegate object GetValue<TRow, TItem>(TRow row, TItem item)
        where TRow : class
        where TItem : class;

    public delegate string GetName<TRow, TItem>(TRow row, TItem item)
        where TRow : class
        where TItem : class;

    public delegate void GetContent<TRow, TItem>(TRow row, TItem item, HtmlTextWriter writer)
        where TRow : class
        where TItem : class;

    public delegate bool ShowAsTemplate<TRow, TItem>(TRow row, TItem item)
        where TRow : class
        where TItem : class;

    public delegate object GetValue<TRow>(TRow row)
        where TRow : class;

    public delegate string GetName<TRow>(TRow row)
        where TRow : class;

    public delegate void GetContent<TRow>(TRow row, HtmlTextWriter writer)
        where TRow : class;

    public delegate bool ShowAsTemplate(RenderContext context);

    public delegate int GetRowsCount<TRow>(TRow row)
        where TRow : class;

    public delegate string GetRowNumber<TRow, TItem>(BaseJournalRow<TRow> journalRow, BaseJournalControl journal, TRow row, TItem item)
        where TRow : BaseRow
        where TItem : class;

    public delegate string GetRowNumber<TRow>(BaseJournalRow<TRow> journalRow, BaseJournalControl journal, TRow row)
        where TRow : BaseRow;

    public delegate bool VisibleCrossColumn<THeaderTable>(THeaderTable row)
        where THeaderTable : class;

    public delegate object GetValueAggregate(string crossColumnId, object[] groupValues);

    public delegate string GetNameAggregate(string crossColumnId, object[] groupValues);

    public delegate void GetContentAggregate(HtmlTextWriter writer, string crossColumnId, object[] groupValues);

    public delegate int GetRowsCountAggregate(BaseJournalControl journal, object[] groupValues);

    #endregion

    public abstract class BaseJournalHeaderControl : Control
    {
        private Dictionary<string, BaseColumn> _columnsDic;
        private List<BaseColumn> _columns;
        private List<ColumnHierarchy> _columnHierarchy;
        private List<RowProperties> _rowsProperties;

        protected BaseJournalHeaderControl()
        {
            ColumnsContext = new Dictionary<string, BaseColumn>();
        }

        #region properties

        public BaseJournalControl Journal { get; set; }
        public List<ColumnHierarchy> ColumnHierarchy
        {
            get
            {
                EnsureColumnHierarchyInitialized();
                return _columnHierarchy;
            }
            set { _columnHierarchy = value; }
        }

        public List<RowProperties> RowsProperties
        {
            get
            {
                if (_rowsProperties == null)
                {
                    _rowsProperties = new List<RowProperties>();
                    InitializeRowsProperties();
                }
                return _rowsProperties;
            }
            set
            {
                _rowsProperties = value;
                if (Journal.SelectingColumnControl != null)
                    Journal.SelectingColumnControl.SetHeaderRowsProperties(value);
            }
        }

        protected bool ColumnsInitialized { get; set; }
        protected bool CrossColumnDataSourcesInitialized { get; set; }
        protected bool ColumnHierarchyInitialized { get; set; }

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public List<BaseColumn> Columns
        {
            get
            {
                EnsureColumnsInitialized();
                return _columns;
            }
        }

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Dictionary<string, BaseColumn> ColumnsDic
        {
            get
            {
                EnsureColumnsInitialized();
                return _columnsDic;
            }
        }

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Dictionary<string, BaseColumn> ColumnsContext { get; private set; }

        private List<ConcatenateColumn> _concatenateColumns = new List<ConcatenateColumn>();
        public List<ConcatenateColumn> ConcatenateColumns
        {
            get { return _concatenateColumns; }
        }

        public override bool EnableViewState
        {
            get
            {
                return false;
            }
        }

        #endregion

        #region methods

        public abstract void RenderBackUrls(HtmlTextWriter writer);
        protected abstract IEnumerable<BaseJournalCustomColumn> GetColumnTemplates();
        protected abstract void InitColumnsByColumnHierarchy();
        protected abstract void InitializeRowsProperties();

        protected virtual void EnsureColumnHierarchyInitialized()
        {
            EnsureColumnsInitialized();
            if (!ColumnHierarchyInitialized)
            {
                ColumnHierarchyInitialized = true;
                InitColumnsByColumnHierarchy();
            }
        }

        public virtual void EnsureColumnsInitialized()
        {
            if (!ColumnsInitialized)
            {
                ColumnsInitialized = true;
                _columns = new List<BaseColumn>();
                InitializeColumns();
            }
        }

        protected virtual void InitializeColumns()
        {
            ColumnsInitialized = true;
            //InitializeConcatenateColumns();
            _columnsDic = Columns.ToDictionary(c => c.ColumnName);
            foreach (var column in Columns)
                column.DetectIsCrossColumn(Journal, "");
            foreach (var columnTemplate in GetColumnTemplates())
            {
                if (!ColumnsDic.ContainsKey(columnTemplate.ColumnName))
                    throw new Exception("Not found template.ColumnName in columns");
                ColumnsDic[columnTemplate.ColumnName].TemplateColumn = columnTemplate;
            }

            EnsureColumnsInitialized();
        }

        protected void InitializeConcatenateColumns()
        {
            if (Journal.SelectingColumnControl == null) return;
            // Get list from hidden field
            var concatenateColumnTransporters = Journal.SelectingColumnControl.GetConcatenateColumnTransporters();
            _concatenateColumns = ConcatenateColumnMaker.GetConcatenateColumns(concatenateColumnTransporters, ColumnsDic);
            // Remove columns
            var removeConcList = Journal.SelectingColumnControl.GetRemoveConcatenatedColumns();
            ConcatenateColumnMaker.RemoveConcatenateColumns(removeConcList, _concatenateColumns);
            // Add new column
            var newConcList = Journal.SelectingColumnControl.GetNewConcatenatedColumns();
            ConcatenateColumnMaker.AddNewConcatenateColumn(newConcList, _concatenateColumns, ColumnsDic);
            // Add concatenate columns to common column list
            foreach (var concColumn in ConcatenateColumns)
                ColumnsDic.Add(concColumn.ColumnName, concColumn);
        }

        protected virtual void InitializeCrossColumnDataSources()
        {
            CrossColumnDataSourcesInitialized = true;
        }

        protected virtual void EnsureCrossColumnDataSourcesInitialized()
        {
            if (!CrossColumnDataSourcesInitialized)
                InitializeCrossColumnDataSources();
        }

        protected virtual List<ColumnHierarchy> GetDefaultColumnHierarchy()
        {
            return null;
        }
        
        private List<ColumnHierarchy> _dataColumns;
        private List<ColumnHierarchy> _allDataColumns;

        public List<ColumnHierarchy> GetDataColumns()
        {
            if(_dataColumns == null)
            {
                var columnNames = ColumnHierarchy.GetVisibleColumns(ColumnsDic);
                _dataColumns = columnNames.Where(r => !(ColumnsDic[r.ColumnName].IsCrossColumn)).ToList();
            }
            return _dataColumns;
        }

        public List<ColumnHierarchy> GetAllDataColumns()
        {
            if (_allDataColumns == null)
            {
                var columnNames = ColumnHierarchy.GetAllItems().
                    Where(r => !string.IsNullOrEmpty(r.ColumnName) && ColumnsDic.ContainsKey(r.ColumnName));
                _allDataColumns = columnNames.Where(r => !(ColumnsDic[r.ColumnName].IsCrossColumn)).ToList();
            }
            return _allDataColumns;
        }
        #endregion
    }

    [ParseChildren(true)]
    public abstract class BaseJournalHeaderControl<TRow> : BaseJournalHeaderControl
        where TRow : BaseRow
    {
        #region fields

        private List<CrossColumnDataSource<TRow>> _crossColumnDataSource;

        private ILookup<string, ColumnHierarchy> columnHierarcyDic;

        #endregion

        public event EventHandler ColumnHierarchyInitizlizedEvent;

        #region properies

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public List<CrossColumnDataSource<TRow>> CrossColumnDataSources
        {
            get
            {
                if (_crossColumnDataSource == null)
                {
                    _crossColumnDataSource = new List<CrossColumnDataSource<TRow>>();
                    EnsureCrossColumnDataSourcesInitialized();
                }
                return _crossColumnDataSource;
            }
        }

        public ILookup<string, ColumnHierarchy> ColumnHierarcyDic
        {
            get
            {
                EnsureColumnHierarchyInitialized();
                return columnHierarcyDic;
            }
            private set
            {
                columnHierarcyDic = value;
            }
        }

        #endregion

        #region methods

        protected override void InitColumnsByColumnHierarchy()
        {
            if (ColumnHierarchy == null)
            {
                if (Journal.SelectingColumnControl != null)
                    ColumnHierarchy = Journal.SelectingColumnControl.GetColumnHierarchySettings();
                if (ColumnHierarchy == null)
                    ColumnHierarchy = GetDefaultColumnHierarchy();
            }
            if (ColumnHierarchy == null) return;
            if (ColumnHierarchy.Count == 0)
            {
                ColumnHierarchy = null;
                return;
            }
            foreach (var columnHierarchy in ColumnHierarchy)
                columnHierarchy.InitCrossColumnsHierarchy(ColumnsDic);

            //var settings = Journal.SelectingColumnControl.GetColumnHierarchySettings();
            //SetColumnHierarchySettings(ColumnHierarchy, settings);
            InitColumnHierarchyByConcatenateColumns();

            var columnNames = ColumnHierarchy.SelectMany(r => r.GetColumnNames()).ToArray();
            var defaultResourceManager = Journal.ResourceManager ?? GetDefaultColumnHierarchy().First().ResourceManager;
            foreach (var columnHierarchy in ColumnHierarchy)
                columnHierarchy.Init(defaultResourceManager, Journal);

            Columns.Clear();
            var columnHierarcyDicQuery = ColumnHierarchy.SelectMany(r => r.SelectAll()).ToArray();
            ColumnHierarcyDic = columnHierarcyDicQuery.ToLookup(r => r.ColumnKey);
            foreach (var columnName in columnNames)
            {
                if (columnName.Equals("EmptyCell"))
                    Columns.Add(new Column<TRow>());
                else if (ColumnsDic.ContainsKey(columnName))
                {
                    var column = ColumnsDic[columnName];
                    column.Visible |= columnHierarcyDic[columnName].Any(r => r.Visible);
                    Columns.Add(column);
                }
                else
                {
                    var removeColumn = columnHierarcyDicQuery.FirstOrDefault(r => r.ColumnName == columnName);
                    if (removeColumn != null) removeColumn.Parent.Childs.Remove(removeColumn);
                }
            }
            foreach (var groupColumn in Journal.GroupColumns)
                ColumnsDic[groupColumn.ColumnName].UsingInGroup = true;

            var maxLevelsCount = GetMaxRowSpan();
            foreach (var columnHierarchy in ColumnHierarchy)
            {
                columnHierarchy.CheckVisible(ColumnsDic);
                columnHierarchy.DetectColSpan(ColumnsDic);
                columnHierarchy.DetectRowSpan(maxLevelsCount, ColumnsDic);
            }

            ColumnHierarchyInitizlizedEvent?.Invoke(this, EventArgs.Empty);
        }

        public void InitColumnHierarchyByConcatenateColumns()
        {
            InitializeConcatenateColumns();
            // Add new concatenated columns
            foreach (var column in ConcatenateColumns)
            {
                var colHierarchy = ColumnHierarchy.SingleOrDefault(r => r.ColumnName == column.ColumnName);
                if (colHierarchy == null)
                {
                    var colHier = new ColumnHierarchy();
                    colHier.ColumnName = column.ColumnName;
                    colHier.HeaderTextRu = column.Header;
                    colHier.HeaderTextKz = column.Header;
                    colHier.BaseColumn = column;
                    ColumnHierarchy.Add(colHier);
                }
            }
            // Remove concatenated columns
            foreach (var column in ConcatenateColumns.Where(r => r.MarkAsDeleted).ToList())
            {
                var colHierarchy = ColumnHierarchy.SingleOrDefault(r => r.ColumnName == column.ColumnName);
                ColumnHierarchy.Remove(colHierarchy);
                ConcatenateColumns.Remove(column);
            }
        }

        /*
        private void SetColumnHierarchySettings(List<ColumnHierarchy> columnHierarchy, ICollection<ColumnHierarchy> settings)
        {
            for (int i = 0; i < settings.Count; i++)
            {
            }
        }*/

        public override void RenderBackUrls(HtmlTextWriter writer)
        {
            foreach (var dataSource in CrossColumnDataSources)
                dataSource.RenderBackUrls(writer);
        }

        private void RenderControlByColumnHierarchy(HtmlTextWriter writer)
        {
            int maxRowSpan = GetMaxRowSpan();
            for (int i = RowsProperties.Count; i < maxRowSpan; i++)
                RowsProperties.Add(new RowProperties());
            int level = 0;
            writer.WriteLine();
            writer.RenderBeginTag(HtmlTextWriterTag.Thead); //start Thead
            do
            {
                writer.AddAttribute(HtmlTextWriterAttribute.Class, "ms-vh");
                if (RowsProperties[level].Height != null)
                    writer.AddStyleAttribute(HtmlTextWriterStyle.Height, RowsProperties[level].Height + "px");
                writer.RenderBeginTag(HtmlTextWriterTag.Tr); //start tr

                foreach (var columnHierarchy in ColumnHierarchy.Where(r => r.IsVisibleColumn(ColumnsDic)))
                    columnHierarchy.Render(writer, level, 0, maxRowSpan, ColumnsDic, Journal, RowsProperties);
                //пустая колонка для возможности регулировать ширину колонок
                if (level == 0)
                {
                    writer.AddAttribute(HtmlTextWriterAttribute.Rowspan, maxRowSpan.ToString());
                    writer.AddAttribute(HtmlTextWriterAttribute.Class, "crossHidden");
                    writer.AddStyleAttribute(HtmlTextWriterStyle.Visibility, "hidden");
                    writer.RenderBeginTag(HtmlTextWriterTag.Th);
                    writer.RenderEndTag();
                }
                writer.RenderEndTag(); //end tr
            } while (++level < maxRowSpan);
            writer.RenderEndTag();//end Thead    
        }

        private void StandartRenderControl(HtmlTextWriter writer)
        {
            int maxRowSpan = GetMaxRowSpan();
            int level = maxRowSpan;
            writer.WriteLine();
            writer.RenderBeginTag(HtmlTextWriterTag.Thead);//start Thead
            writer.AddAttribute(HtmlTextWriterAttribute.Class, "ms-vh");
            writer.RenderBeginTag(HtmlTextWriterTag.Tr); //start tr
            foreach (var column in Columns.Where(r => r.VisibleColumn))
            {
                var crossColumn = column as CrossColumn<TRow>;
                if (crossColumn == null)
                {
                    writer.WriteLine();
                    writer.AddAttribute(HtmlTextWriterAttribute.Rowspan, maxRowSpan.ToString());
                    writer.AddAttribute(HtmlTextWriterAttribute.Id, column.ColumnName);
                    writer.RenderBeginTag(HtmlTextWriterTag.Th);
                    writer.Write(column.Header);
                    writer.RenderEndTag();
                }
                else if (crossColumn.CrossColumnDataSource != null)
                {
                    var changeLevel = maxRowSpan - crossColumn.CrossColumnDataSource.MaxLevel;
                    crossColumn.CrossColumnDataSource.RenderColumns(writer, level - changeLevel, changeLevel + 1);
                }
            }
            writer.RenderEndTag();//end tr
            while (--level > 0)
            {
                writer.WriteLine();
                writer.AddAttribute(HtmlTextWriterAttribute.Class, "ms-vh");
                writer.RenderBeginTag(HtmlTextWriterTag.Tr); //start tr 2
                foreach (var column in Columns.Where(r => r.VisibleColumn))
                {
                    var crossColumn = column as CrossColumn<TRow>;
                    if (crossColumn == null || crossColumn.CrossColumnDataSource == null) continue;

                    var changeLevel = maxRowSpan - crossColumn.CrossColumnDataSource.MaxLevel;
                    crossColumn.CrossColumnDataSource.RenderColumns(writer, level - changeLevel, changeLevel + 1);
                }
                writer.RenderEndTag();//end tr 2
            }
            writer.RenderEndTag();//end Thead       
        }

        protected override void InitializeRowsProperties()
        {
            if (Journal.SelectingColumnControl != null)
            {
                var rowsH = Journal.SelectingColumnControl.DesirializeRowsH();
                if (rowsH != null)
                {
                    RowsProperties = rowsH;
                    return;
                }
            }
            int count = GetMaxRowSpan();
            for (int i = 0; i < count; i++)
                RowsProperties.Add(new RowProperties());
        }

        public int GetMaxRowSpan()
        {
            EnsureCrossColumnDataSourcesInitialized();
            if (ColumnHierarchy != null)
                return ColumnHierarchy.Max(r => r.GetLevelsCount(ColumnsDic));
            if (CrossColumnDataSources == null || CrossColumnDataSources.Count == 0) return 1;
            var maxLevel = CrossColumnDataSources.Max(r => r.MaxLevel);
            if (maxLevel == 0)
                return 1;
            return maxLevel;
        }

        public int GetFullColSpan()
        {
            if (ColumnHierarchy != null)
                return ColumnHierarchy.Sum(r => r.ColSpan);
            var cols = 0;
            foreach (var column in Columns.Where(r => r.VisibleColumn))
            {
                var crossColumn = column as CrossColumn<TRow>;
                if (crossColumn != null)
                    cols += crossColumn.CrossColumnDataSource.GetFullColSpan();
                else
                    cols++;
            }
            return cols;
        }

        #endregion

        #region Override methods of Control, interfaces

        protected override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);
            foreach (var columnHierarchy in ColumnHierarchy.Where(r => r.IsVisibleColumn(ColumnsDic)))
                columnHierarchy.PreRender(ColumnsDic, Journal);
        }

        public override void RenderControl(HtmlTextWriter writer)
        {
            if (ColumnHierarchy == null || ColumnHierarchy.Count == 0)
                StandartRenderControl(writer);
            else
                RenderControlByColumnHierarchy(writer);
        }

        #endregion
    }
}