namespace Nat.Web.Controls.SelectValues
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using System.Web.UI;

    using Nat.Web.Controls.GenerationClasses;

    public class MultipleValuesSelect : BaseListDataBoundControl //, IRenderComponent
    {
        public string UserControlPath { get; set; }

        protected RenderUserControl UserControl { get; set; }

        protected override void LoadControlState(object savedState)
        {
            var items = savedState as List<ListControlItem>;
            if (items != null)
            {
                Items.AddRange(items);
                RequiresDataBinding = false;
            }

            LoadPostBackValues();
        }

        protected override object SaveControlState()
        {
            return Items;
        }

        public override TypeComponent GetTypeComponent()
        {
            return TypeComponent.ListValues;
        }

        public override string GetClientID(string value)
        {
            var item = GetAllItems().FirstOrDefault(r => r.Value.Equals(value));
            return item == null ? null : UserControl.GetClientID(item);
        }
        
        private void LoadPostBackValues()
        {
            foreach (var item in GetAllItems())
                item.Selected = UserControl.LoadPostBackValues(item);
        }

        public virtual void SetFilters(BrowseFilterParameters filters)
        {
        }

        protected override void OnInit(EventArgs e)
        {
            UserControl = (RenderUserControl)Page.LoadControl(UserControlPath);
            UserControl.DataBoundControl = this;
            Controls.Add(UserControl);
            base.OnInit(e);
        }

        protected override ListControlItem CreateListControlItem()
        {
            return UserControl.CreateListControlItem();
        }

        protected override bool CheckNeedUpdate(ListControlItem item)
        {
            return UserControl.CheckNeedUpdate(item);
        }

        protected override void InitUpdateValues(IDictionary values, ListControlItem item)
        {
            base.InitUpdateValues(values, item);
            UserControl.InitUpdateValues(values, item);
        }

        protected override void InitializeItem(ListControlItem item, object dataItem)
        {
            base.InitializeItem(item, dataItem);
            UserControl.InitializeItem(item, dataItem);
        }

        protected override void InitDeleteValues(IDictionary keys, ListControlItem item)
        {
            base.InitDeleteValues(keys, item);
            UserControl.InitDeleteValues(keys, item);
        }

        protected override void InitInsertValues(IDictionary values, ListControlItem item)
        {
            base.InitInsertValues(values, item);
            UserControl.InitInsertValues(values, item);
        }

        protected override void InitValues(IDictionary values, ListControlItem item)
        {
            base.InitValues(values, item);
            UserControl.InitValues(values, item);
        }

        public override IEnumerable<string> SelectedValues
        {
            set
            {
                var oldValues = SelectedValues.ToArray();
                base.SelectedValues = value;
                UserControl.SelectedValuesChanged(oldValues, SelectedValues.ToArray());
            }
        }

        public abstract class RenderUserControl : UserControl
        {
            public MultipleValuesSelect DataBoundControl { get; internal set; }

            protected internal abstract ListControlItem CreateListControlItem();

            protected internal abstract void InitializeItem(ListControlItem item, object dataItem);

            protected internal abstract bool LoadPostBackValues(ListControlItem item);

            protected internal abstract string GetClientID(ListControlItem item);

            public abstract void InitDeleteValues(IDictionary keys, ListControlItem item);

            public abstract void InitInsertValues(IDictionary values, ListControlItem item);

            public abstract void InitValues(IDictionary values, ListControlItem item);

            public abstract bool CheckNeedUpdate(ListControlItem item);

            public abstract void InitUpdateValues(IDictionary values, ListControlItem item);

            public abstract void SelectedValuesChanged(string[] oldValues, string[] selectedValues);
        }

        public abstract class RenderUserControl<TListControlItem, TMember, TValue> : RenderUserControl
            where TListControlItem : ListControlItem, new()
        {
            protected internal override ListControlItem CreateListControlItem()
            {
                return new TListControlItem();
            }

            protected internal override void InitializeItem(ListControlItem item, object dataItem)
            {
                InitializeItem((TListControlItem)item, (TMember)GetPropertyValue(dataItem, "MemberItem"), (TValue)GetPropertyValue(dataItem, "ValueItem"));
            }

            protected internal override bool LoadPostBackValues(ListControlItem item)
            {
                return LoadPostBackValues((TListControlItem)item);
            }

            protected internal override string GetClientID(ListControlItem item)
            {
                return GetClientID((TListControlItem)item);
            }

            public override void InitDeleteValues(IDictionary keys, ListControlItem item)
            {
                InitDeleteValues(keys, (TListControlItem)item);
            }

            public override void InitInsertValues(IDictionary values, ListControlItem item)
            {
                InitInsertValues(values, (TListControlItem)item);
            }

            public override void InitValues(IDictionary values, ListControlItem item)
            {
                InitValues(values, (TListControlItem)item);
            }

            public override bool CheckNeedUpdate(ListControlItem item)
            {
                return CheckNeedUpdate((TListControlItem)item);
            }

            public override void InitUpdateValues(IDictionary values, ListControlItem item)
            {
                InitUpdateValues(values, (TListControlItem)item);
            }

            protected internal abstract void InitializeItem(TListControlItem item, TMember member, TValue value);
            protected internal abstract bool LoadPostBackValues(TListControlItem item);
            protected internal abstract string GetClientID(TListControlItem item);
            protected internal abstract void InitDeleteValues(IDictionary keys, TListControlItem item);
            protected internal abstract void InitInsertValues(IDictionary values, TListControlItem item);
            protected internal abstract void InitValues(IDictionary values, TListControlItem item);
            protected internal abstract void InitUpdateValues(IDictionary values, TListControlItem item);
            protected internal abstract bool CheckNeedUpdate(TListControlItem item);
        }

        #region Render

        protected override void RenderContents(HtmlTextWriter writer)
        {
            UserControl.RenderControl(writer);
        }

        public override void RenderBeginTag(HtmlTextWriter writer)
        {
        }

        public override void RenderEndTag(HtmlTextWriter writer)
        {
        }

        protected override void RenderItem(HtmlTextWriter writer, ListControlItem item, BaseListDataBoundRenderEventArgs args)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}