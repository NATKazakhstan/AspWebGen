using System;
using System.Web.UI;
using System.Data.Linq;
using System.Collections;
using Nat.Web.Controls.GenerationClasses.Data;

namespace Nat.Web.Controls.GenerationClasses
{
    public abstract class BaseDataSource<TKey> : Control, IDataSource
        where TKey : struct
    {
        public abstract BaseDataSourceView<TKey> BaseView { get; }
        public virtual event EventHandler<DataSourceSelectingEventArgs> Selecting;
        public virtual event EventHandler<SelectedQueryParametersEventArgs> SelectedQueryParameters;

        public bool EmptyLoad { get; set; }
        public bool AllowCreateSelectedRow { get; set; }
        public bool AllowCustomCache { get; set; }
        public bool AllowSelectOnlyNames { get; set; }

        public string SelectForAddType
        {
            get { return BaseView.SelectForAddType; }
            set { BaseView.SelectForAddType = value; }
        }

        public bool HideRecordCanNotSelected
        {
            get { return BaseView.HideRecordCanNotSelected; }
            set { BaseView.HideRecordCanNotSelected = value; }
        }

        public virtual bool LookupValuesHidden
        {
            get { return BaseView.LookupValuesHidden; }
            set { BaseView.LookupValuesHidden = value; }
        }

        public virtual void OnSelecting(DataSourceSelectingEventArgs e)
        {
            if (Selecting != null) Selecting(this, e);
        }

        public virtual void OnSelectedQueryParameters(SelectedQueryParametersEventArgs e)
        {
            if (SelectedQueryParameters != null) SelectedQueryParameters(this, e);
        }
        
        protected virtual void OnDataSourceChanged(EventArgs e)
        {
            if (DataSourceChanged != null)
                DataSourceChanged(this, e);
        }

        public virtual DataSourceView GetView(string viewName)
        {
            return BaseView;
        }

        public virtual ICollection GetViewNames()
        {
            return null;
        }

        public virtual event EventHandler DataSourceChanged;
    }

    public abstract class BaseDataSource<TKey, TTable, TDataContext, TRow> : BaseDataSource<TKey>, IDataSource
        where TKey : struct
        where TTable : class
        where TDataContext : DataContext, new()
        where TRow : BaseRow, new()
    {
        public abstract BaseDataSourceView<TKey, TTable, TDataContext, TRow> BaseView2 { get; }
        
        public override BaseDataSourceView<TKey> BaseView
        {
            get { return BaseView2; }
        }
    }
}
