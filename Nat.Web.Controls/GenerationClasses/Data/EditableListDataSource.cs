using System;
using System.Collections;
using System.Data.Linq;
using System.Web.UI;

namespace Nat.Web.Controls.GenerationClasses.Data
{
    public abstract class EditableListDataSource : Control, IDataSource
    {
        public abstract EditableListDataSourceView BaseView { get; }

        public event EventHandler DataSourceChanged;
        
        public string ValuesDataSourceID
        {
            get { return BaseView.ValuesDataSourceID; }
            set { BaseView.ValuesDataSourceID = value; }
        }

        public virtual DataSourceView GetView(string viewName)
        {
            return BaseView;
        }

        public ICollection GetViewNames()
        {
            return null;
        }

        public event EventHandler<EditableListDataSourceView.InsertingEventArgs> Inserting
        {
            add { BaseView.Inserting += value; }
            remove { BaseView.Inserting -= value; }
        }

        public event EventHandler<EditableListDataSourceView.DeletingEventArgs> Deleting
        {
            add { BaseView.Deleting += value; }
            remove { BaseView.Deleting -= value; }
        }

        public event EventHandler<EditableListDataSourceView.InsertedEventArgs> Inserted
        {
            add { BaseView.Inserted += value; }
            remove { BaseView.Inserted -= value; }
        }

        public event EventHandler<EditableListDataSourceView.DeletedEventArgs> Deleted
        {
            add { BaseView.Deleted += value; }
            remove { BaseView.Deleted -= value; }
        }

        public event EventHandler<EditableListDataSourceView.EnabledEventArgs> EnabledRow
        {
            add { BaseView.EnabledRow += value; }
            remove { BaseView.EnabledRow -= value; }
        }
    }

    public abstract class EditableListDataSource<TDataSourceView, TKeyValues> : EditableListDataSource
        where TDataSourceView : EditableListDataSourceView<TKeyValues>
        where TKeyValues : struct
    {
        private TDataSourceView _view;

        public TDataSourceView View
        {
            get { return _view ?? (_view = CreateDataSourceView("Default")); }
        }

        public override EditableListDataSourceView BaseView
        {
            get { return View; }
        }

        public string ValueDataField
        {
            get { return View.ValueDataField; }
            set { View.ValueDataField = value; }
        }

        public bool IsFile
        {
            get { return View.IsFile; }
            set { View.IsFile = value; }
        }

        public bool ValuesOnly
        {
            get { return true; }
            set { }
        }

        protected abstract TDataSourceView CreateDataSourceView(string viewName);
    }

    public abstract class EditableListDataSource<TDataSourceView, TKeyValues, TKeyMembers> : EditableListDataSource
        where TDataSourceView : EditableListDataSourceView<TKeyValues, TKeyMembers>
        where TKeyValues : struct
        where TKeyMembers : struct
    {
        private TDataSourceView _view;

        public TDataSourceView View
        {
            get { return _view ?? (_view = CreateDataSourceView("Default")); }
        }

        public string MembersDataSourceID
        {
            get { return View.MembersDataSourceID; }
            set { View.MembersDataSourceID = value; }
        }

        public string ValueDataField
        {
            get { return View.ValueDataField; }
            set { View.ValueDataField = value; }
        }

        public bool ValuesOnly
        {
            get { return View.ValuesOnly; }
            set { View.ValuesOnly = value; }
        }

        public override EditableListDataSourceView BaseView
        {
            get { return View; }
        }

        protected abstract TDataSourceView CreateDataSourceView(string viewName);
    }

    public abstract class EditableListDataSource<TDataSourceView, TKeyValues, TTableValues, TDataContextValues, TRowValues> : EditableListDataSource
        where TDataSourceView : EditableListDataSourceView<TKeyValues, TTableValues, TDataContextValues, TRowValues>
        where TKeyValues : struct
        where TTableValues : class, new()
        where TDataContextValues : DataContext, new()
        where TRowValues : BaseRow, new()
    {
        private TDataSourceView _view;

        public TDataSourceView View
        {
            get { return _view ?? (_view = CreateDataSourceView("Default")); }
        }

        public override EditableListDataSourceView BaseView
        {
            get { return View; }
        }

        public string ValueDataField
        {
            get { return View.ValueDataField; }
            set { View.ValueDataField = value; }
        }

        public bool IsFile
        {
            get { return View.IsFile; }
            set { View.IsFile = value; }
        }

        public bool ValuesOnly
        {
            get { return true; }
            set {  }
        }

        protected abstract TDataSourceView CreateDataSourceView(string viewName);
    }

    public abstract class EditableListDataSource<TDataSourceView, TKeyValues, TTableValues, TDataContextValues, TRowValues, TKeyMembers, TTableMembers, TDataContextMembers, TRowMembers> : EditableListDataSource
        where TDataSourceView : EditableListDataSourceView<TKeyValues, TTableValues, TDataContextValues, TRowValues, TKeyMembers, TTableMembers, TDataContextMembers, TRowMembers>
        where TKeyValues : struct
        where TTableValues : class, new()
        where TDataContextValues : DataContext, new()
        where TRowValues : BaseRow, new()
        where TKeyMembers : struct
        where TTableMembers : class
        where TDataContextMembers : DataContext, new()
        where TRowMembers : BaseRow, new()
    {
        private TDataSourceView _view;

        public TDataSourceView View
        {
            get { return _view ?? (_view = CreateDataSourceView("Default")); }
        }

        public string MembersDataSourceID
        {
            get { return View.MembersDataSourceID; }
            set { View.MembersDataSourceID = value; }
        }

        public string ValueDataField
        {
            get { return View.ValueDataField; }
            set { View.ValueDataField = value; }
        }

        public bool ValuesOnly
        {
            get { return View.ValuesOnly; }
            set { View.ValuesOnly = value; }
        }

        public override EditableListDataSourceView BaseView
        {
            get { return View; }
        }

        protected abstract TDataSourceView CreateDataSourceView(string viewName);
    }
}