using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Globalization;
using System.Web.UI;
using System.Web.UI.WebControls;
using Nat.Tools;
using Nat.Web.Controls.Properties;
using Nat.Web.Tools;

namespace Nat.Web.Controls
{
    [ToolboxData("<{0}:DetailsViewExt runat=\"server\" Width=\"125px\" Height=\"50px\"></{0}:DetailsViewExt>")]
    public class DetailsViewExt : DetailsView
    {
        private TableDataSourceView _view;
        private bool _viewFinded = false;
        private bool _afterPreRender = false;
        private bool _requiresDataBinding = false;
        private bool _cancelingEdit = false;
        private bool _dataBound = false;
        private TableDataSourceDataMode _previouseDataMode;
        private ColumnIndexer _columnIndexer;
        private bool allowLocalizationDicColumns = true;

        public DetailsViewExt()
        {
            base.EmptyDataText = Resources.SEmptyDataText;
        }

        private TableDataSourceView TableDataSourceView
        {
            get
            {
                if (!_viewFinded)
                {
                    _view = GetData() as TableDataSourceView;
                    _viewFinded = true;
                }
                return _view;
            }
        }


        #region публичные свойства

        [DefaultValue(true)]
        [Description("Разрешить локализацию справочных колонок")]
        public bool AllowLocalizationDicColumns
        {
            get { return allowLocalizationDicColumns; }
            set { allowLocalizationDicColumns = value; }
        }

        [Category("Styles")]
        [DefaultValue(true)]
        [Description("Использовать стиль по умолчанию")]
        public bool UseDefaultCssStyle
        {
            get { return (bool?)ViewState["UseDefaultCssStyle"] ?? true; }
            set { ViewState["UseDefaultCssStyle"] = value; }
        }

        [DefaultValue(false)]
        [Category("Behavior")]
        [Description("Разрешить переключение DataMode у TableDataSource")]
        public bool AllowChangeDataMode
        {
            get { return (bool?)ViewState["_allowChangeDataMode"] ?? false; }
            set { ViewState["_allowChangeDataMode"] = value; }
        }

        [DefaultValue(true)]
        [Category("Behavior")]
        [Description("Разрешить изменение мода у DetailsViewExt")]
        public bool AllowChangeViewMode
        {
            get { return (bool?)ViewState["_allowChangeViewMode"] ?? true; }
            set { ViewState["_allowChangeViewMode"] = value; }
        }

        public Dictionary<string, int> GridColumnIndexes
        {
            get
            {
                if (_columnIndexer == null)
                {
                    _columnIndexer = new ColumnIndexer(Fields);
                }
                return _columnIndexer.GridColumnIndexes;
            }
        }
        #endregion

        /// <summary>
        /// Для внутренних вызовов, чтоб была обработка событий.
        /// </summary>
        /// <param name="mode">новый мод</param>
        /// <param name="cancelingEdit">является ли отменой изменения</param>
        public void ChangeModeWithEvent(DetailsViewMode mode, bool cancelingEdit)
        {
            DetailsViewModeEventArgs args = new DetailsViewModeEventArgs(mode, cancelingEdit);
            OnModeChanging(args);
            if (args.Cancel) return;
            ChangeMode(mode);
            OnModeChanged(EventArgs.Empty);
        }

        private bool inEnsureDataBound = false;
        protected override void EnsureDataBound()
        {
            if (inEnsureDataBound) return;
            if (AllowChangeDataMode && TableDataSourceView != null)
            {
                _previouseDataMode = TableDataSourceView.DataMode;
                TableDataSourceView.DataMode = TableDataSourceDataMode.OnlyCurrent;
            }
            try
            {
                // В RequiresDataBinding если не IsPostBack, то сразу вызывается EnsureDataBound().
                if (_requiresDataBinding && !RequiresDataBinding && !_afterPreRender && !_dataBound)
                {
                    inEnsureDataBound = true;
                    try
                    {
                        RequiresDataBinding = true;
                    }
                    finally
                    {
                        inEnsureDataBound = false;
                    }
                }
                base.EnsureDataBound();
            }
            finally
            {
                if (AllowChangeDataMode && TableDataSourceView != null)
                    TableDataSourceView.DataMode = _previouseDataMode;
            }
        }

        protected override int CreateChildControls(IEnumerable dataSource, bool dataBinding)
        {
            if (AllowLocalizationDicColumns && !(dataSource is object[]) && dataSource is DataView)
                LocalizationHelper.ChangeColumnsExpression((dataSource as DataView).Table);

            return base.CreateChildControls(dataSource, dataBinding);
        }

        protected override void OnLoad(EventArgs e)
        {
            if (TableDataSourceView != null)
            {
                Page.PreRenderComplete += OnPreRenderComplete;
                TableDataSourceView.Table.RowChanged += Table_OnRowChanged;
                TableDataSourceView.CurrentChanged += TableDataSourceView_OnCurrentChanged;
            }
            base.OnLoad(e);
        }

        protected override void OnUnload(EventArgs e)
        {
            base.OnUnload(e);
            if (_view != null)
                _view.Table.RowChanged -= Table_OnRowChanged;
        }

        private void OnPreRenderComplete(object sender, EventArgs e)
        {
            _afterPreRender = true;
        }

        private void TableDataSourceView_OnCurrentChanged(object sender, CurrentChangeEventArgs e)
        {
            _requiresDataBinding = true;
        }

        private void Table_OnRowChanged(object sender, DataRowChangeEventArgs e)
        {
            if (_afterPreRender) return;
            _requiresDataBinding = true;
        }

        protected override void OnDataBound(EventArgs e)
        {
            if (TableDataSourceView != null && TableDataSourceView.HasNewRow && AllowChangeViewMode)
                ChangeModeWithEvent(DetailsViewMode.Edit, false);
            _dataBound = true;
            base.OnDataBound(e);
        }

        protected override void OnModeChanged(EventArgs e)
        {
            if (AllowChangeViewMode)
            {
                if (TableDataSourceView != null && CurrentMode == DetailsViewMode.ReadOnly && 
                    (TableDataSourceView.HasNewRow || TableDataSourceView.IsEditRow))
                {
                    if (_cancelingEdit && !(Parent is View)) TableDataSourceView.CancelEdit();
                    TableDataSourceView.EndEdit();
                }
                if (TableDataSourceView != null && CurrentMode == DetailsViewMode.Edit && 
                    !TableDataSourceView.IsEditRow)
                {
                    TableDataSourceView.BeginEdit();
                }
            }
            base.OnModeChanged(e);
        }

        protected override void OnModeChanging(DetailsViewModeEventArgs e)
        {
            _cancelingEdit = e.CancelingEdit;
            base.OnModeChanging(e);
            if (AllowChangeViewMode)
            {
                if (TableDataSourceView != null && !e.Cancel &&
                    e.CancelingEdit && Parent is View)
                {
                    if (TableDataSourceView.HasNewRow) e.Cancel = true;
                    TableDataSourceView.CancelEdit();
                }
            }
        }

        protected override void Render(HtmlTextWriter writer)
        {
            CssApply(UseDefaultCssStyle);
            base.Render(writer);
        }

        private void CssApply(bool style)
        {
            if (style)
            {
//                CssClass = "ms-vb";
//                HeaderStyle.CssClass = "ms-vh";
                RowStyle.CssClass = "ms-descriptiontext ms-alignleft";
                EditRowStyle.CssClass = "ms-descriptiontext";
                AlternatingRowStyle.CssClass = "ms-alternating ms-descriptiontext";
                FieldHeaderStyle.CssClass = "ms-descriptiontext ms-alignright";
                FooterStyle.CssClass = "";
                CommandRowStyle.CssClass = "ms-descriptiontext";
                PagerStyle.CssClass = "";
                GridLines = GridLines.None;
            }
        }

        protected override void OnDataSourceViewChanged(object sender, EventArgs e)
        {
            if(_view != null)
            {
                _viewFinded = false;
                _view.CurrentChanged -= TableDataSourceView_OnCurrentChanged;
                _view.Table.RowChanged -= Table_OnRowChanged;
                _view = null;
            }
            base.OnDataSourceViewChanged(sender, e);
        }

        #region публичные методы

        public IColumnName GetColumnName(string columnName)
        {
            if (GridColumnIndexes.ContainsKey(columnName))
                return Fields[GridColumnIndexes[columnName]] as IColumnName;
            else
                return null;
        }

        public T GetColumn<T>(string columnName) where T : class
        {
            if (GridColumnIndexes.ContainsKey(columnName))
                return Fields[GridColumnIndexes[columnName]] as T;
            else
                return null;
        }

        public BoundField GetBoundField(string dataField)
        {
            if (GridColumnIndexes.ContainsKey(dataField))
                return Fields[GridColumnIndexes[dataField]] as BoundField;
            else
                return null;
        }

        #endregion
    }
}