using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Script.Serialization;
using System.Web.UI;
using System.Web.UI.WebControls;
using AjaxControlToolkit;
using Nat.Web.Controls.GenerationClasses;
using Nat.Web.Controls.Properties;

namespace Nat.Web.Controls.SelectValues
{
    using System.Collections.Specialized;

    public abstract class BaseListDataBoundControl : DataBoundControl, INamingContainer
    {
        private List<ListControlItem> _items = new List<ListControlItem>();
        private string[] _lastGroupValues;
        private ListControlItem[] _lastGroups;
        private ScriptManager _ScriptManager;

        protected BaseListDataBoundControl()
        {
            GroupFields = new List<GroupFieldItem>();
            Columns = 1;
            ExtenderAjaxControl = new ExtenderAjaxControl();         
        }


        protected abstract void RenderItem(HtmlTextWriter writer, ListControlItem item,
                                           BaseListDataBoundRenderEventArgs args);

        [Localizable(true)]
        public string DataToolTipField { get; set; }
        [Localizable(true)]
        public string DataToolTipFormatString { get; set; }
        public string DataEnabledField { get; set; }
        public string DataDeletedField { get; set; }
        public string DataSelectedField { get; set; }
        [Localizable(true)]
        public string DataTextField { get; set; }
        [Localizable(true)]
        public string DataTextFormatString { get; set; }
        public string DataValueField { get; set; }
        public string DataSelectedKeyField { get; set; }
        public virtual int Columns { get; set; }
        public bool ReadOnly { get; set; }
        public bool ShowOnlySelected { get; set; }
        public string ClassCSS { get; set; }
        public string SortExpression { get; set; }
        public string DataReferenceValueField { get; set; }

        public string ReferenceTableName { get; set; }

        protected bool RenderOnlyText { get; set; }

        public string DefaultSortExpression { get; set; }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public List<ListControlItem> Items
        {
            get
            {
                EnsureDataBound();
                return _items;
            }
            protected set { _items = value; }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        [PersistenceMode(PersistenceMode.InnerProperty)]
        public List<GroupFieldItem> GroupFields { get; set; }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Browsable(false)]
        public ExtenderAjaxControl ExtenderAjaxControl { get; set; }

        public ScriptManager ScriptManager
        {
            get
            {
                if (_ScriptManager == null)
                    _ScriptManager = ScriptManager.GetCurrent(Page);
                return _ScriptManager;
            }
        }

        public IEnumerable<ListControlItem> SelectedItems
        {
            get
            {
                return GetAllItems().Where(r => r.Selected);
            }
        }

        public virtual IEnumerable<string> SelectedValues
        {
            get
            {
                return SelectedItems.Select(r => r.Value);
            }
            set
            {
                foreach (var item in Items)
                    item.Selected = value.Contains(item.Value);
            }
        }

        public BrowseFilterParameters.IControl SelectedItemsHidden
        {
            get
            {
                return new BrowseFilterParameters.SimpleIDControl
                    {
                        ClientID = GetMemberItemsClientID(),
                        TypeComponent = TypeComponent.ValueControl,
                    };
            }
        }

        public virtual TypeComponent GetTypeComponent()
        {
            throw new NotSupportedException();
        }

        public virtual string GetClientID(string value)
        {
            throw new NotSupportedException();
        }

        protected override void OnInit(EventArgs e)
        {
            Controls.Add(ExtenderAjaxControl);
            Page.RegisterRequiresControlState(this);
            base.OnInit(e);
        }

        protected override void OnLoad(EventArgs e)
        {
            ExtenderAjaxControl.OnLoad();
            base.OnLoad(e);
        }

        protected override void OnPreRender(EventArgs e)
        {
            ExtenderAjaxControl.OnPreRender();
            EnsureDataBound();
            base.OnPreRender(e);
        }

        protected override void Render(HtmlTextWriter writer)
        {
            if (Page != null && !RenderOnlyText)
                ExtenderAjaxControl.Render();
            base.Render(writer);
        }

        protected override DataSourceSelectArguments CreateDataSourceSelectArguments()
        {
            var arguments = base.CreateDataSourceSelectArguments();
            if (GroupFields != null && GroupFields.Count > 0)
            {
                var order = GroupFields.Aggregate(
                    string.Empty, (current, g) => (current == string.Empty ? string.Empty : current + ",")
                                        + g.GroupField
                                        + (g.Ascending ? string.Empty : " desc"));
                if (string.IsNullOrEmpty(arguments.SortExpression))
                    arguments.SortExpression = order;
                else
                    arguments.SortExpression += "," + order;
            }
            if (string.IsNullOrEmpty(arguments.SortExpression))
                arguments.SortExpression = SortExpression ?? string.Empty;
            else
                arguments.SortExpression += "," + SortExpression;

            if (!string.IsNullOrEmpty(DefaultSortExpression))
            {
                arguments.SortExpression = string.IsNullOrEmpty(arguments.SortExpression)
                                               ? DefaultSortExpression
                                               : arguments.SortExpression + "," + DefaultSortExpression;
            }

            return arguments;
        }

        protected override void PerformDataBinding(IEnumerable dataSource)
        {
            if (dataSource == null)
                return;
            var dataCollection = dataSource as ICollection;
            if (dataCollection != null)
                Items.Capacity = dataCollection.Count;
            _lastGroups = null;
            Items.Clear();

            foreach (var dataItem in dataSource)
            {
                var item = CreateListControlItem();
                InitializeItem(item, dataItem);
                AddItem(item, dataItem);
            }
        }

        protected virtual void InitializeItem(ListControlItem item, object dataItem)
        {
            if (!string.IsNullOrEmpty(DataTextField))
                item.Text = GetPropertyValue(dataItem, DataTextField, DataTextFormatString);
            if (!string.IsNullOrEmpty(DataValueField))
                item.Value = GetPropertyValue(dataItem, DataValueField, null);
            if (!string.IsNullOrEmpty(this.DataReferenceValueField))
                item.ReferenceValue = GetPropertyValue(dataItem, this.DataReferenceValueField, null);
            if (!string.IsNullOrEmpty(DataToolTipField))
                item.ToolTip = GetPropertyValue(dataItem, DataToolTipField, DataToolTipFormatString);
            item.Enabled = string.IsNullOrEmpty(DataEnabledField)
                           || Convert.ToBoolean(GetPropertyValue(dataItem, DataEnabledField, null));
            if (!string.IsNullOrEmpty(DataDeletedField))
                item.Deleted = Convert.ToBoolean(GetPropertyValue(dataItem, DataDeletedField, null));
            if (!string.IsNullOrEmpty(DataSelectedField))
                item.Selected = Convert.ToBoolean(GetPropertyValue(dataItem, DataSelectedField, null));
            if (!string.IsNullOrEmpty(DataSelectedKeyField))
                item.SelectedKey = GetPropertyValue(dataItem, DataSelectedKeyField, null);
        }

        protected virtual ListControlItem CreateListControlItem()
        {
            return new ListControlItem();
        }

        private string[] GetGroupValues(object dataItem)
        {
            var values = new string[GroupFields.Count];
            for (int i = 0; i < GroupFields.Count; i++)
            {
                var groupField = GroupFields[i];
                values[i] = GetPropertyValue(dataItem, groupField.GroupField, groupField.GroupFieldFormatString);
            }
            return values;
        }

        private void AddItem(ListControlItem item, object dataItem)
        {
            if (GroupFields == null || GroupFields.Count == 0)
            {
                Items.Add(item);
                return;
            }
            var groupValues = GetGroupValues(dataItem);
            if (_lastGroups == null)
            {
                _lastGroups = new ListControlItem[GroupFields.Count];
                AddGroupItems(0, groupValues);
            }
            else
            {
                for (int i = 0; i < GroupFields.Count; i++)
                {
                    if ((groupValues[i] != null && groupValues[i].Equals(_lastGroupValues[i]))
                        || (string.IsNullOrEmpty(groupValues[i]) && string.IsNullOrEmpty(_lastGroupValues[i])))
                        continue;
                    AddGroupItems(i, groupValues);
                    break;
                }
            }
            _lastGroups[GroupFields.Count - 1].Children.Add(item);
        }

        private void AddGroupItems(int groupIndex, string[] groupValues)
        {
            _lastGroupValues = groupValues;
            for (int i = groupIndex; i < GroupFields.Count; i++)
            {
                var groupField = GroupFields[i];
                var group = new ListControlItem
                {
                    Children = new List<ListControlItem>(),
                    IsGroup = true,
                    Selectable = groupField.Selectable,
                    Text = string.IsNullOrEmpty(groupValues[i]) ? groupField.NullText : groupValues[i],
                    Value = "msc_" + Guid.NewGuid().ToString("N"),
                };
                group.HideGroup = string.IsNullOrEmpty(group.Text) && groupField.HideGroupIfValueNull;
                if (groupField.Collapsible && !group.HideGroup && Page != null)
                {
                    var clientID = group.Value;
                    var extender = new CollapsiblePanelExtender
                                       {
                                           ID = "cp_" + clientID,
                                           BehaviorID = "cpb_" + clientID,
                                           Collapsed = groupField.DefaultCollapced,
                                           ExpandDirection = CollapsiblePanelExpandDirection.Vertical,
                                           CollapseControlID = clientID + "_Legend",
                                           ExpandControlID = clientID + "_Legend",
                                       };
                    ExtenderAjaxControl.AddExtender(extender, clientID);
                }
                if (i == 0)
                    Items.Add(group);
                else
                    _lastGroups[i - 1].Children.Add(group);
                _lastGroups[i] = group;
            }
        }

        private static string GetPropertyValue(object dataItem, string dataTextField, string dataTextFormatString)
        {
            if (dataItem == null) return null;
            if (dataTextField.Contains(","))
            {
                var oDataItem = dataItem;
                var fields = dataTextField.Split(',').Select(r => GetPropertyValue(oDataItem, r)).ToArray();
                if (fields.All(r => r == null)) return null;
                if (string.IsNullOrEmpty(dataTextFormatString))
                    return string.Join("; ", fields.Where(r => r != null && !"".Equals(r)).Select(r => r.ToString()).ToArray());
                dataTextFormatString = dataTextFormatString.Replace("\\n", "\r\n");
                return string.Format(dataTextFormatString, fields);
            }
            dataItem = GetPropertyValue(dataItem, dataTextField);
            if (dataItem == null) return null;
            var strValue = dataItem as string;
            if (strValue != null) return strValue;
            if (string.IsNullOrEmpty(dataTextFormatString))
                return dataItem.ToString();
            return string.Format(dataTextFormatString, dataItem);
        }

        internal static object GetPropertyValue(object dataItem, string dataTextField)
        {
            if (dataItem == null) return null;
            foreach (var propertyName in dataTextField.Split('.'))
            {
                var property = TypeDescriptor.GetProperties(dataItem).Find(propertyName, false);
                if (property == null)
                    throw new ArgumentException("Не найдено свойство " + dataTextField, "dataTextField");
                dataItem = property.GetValue(dataItem);
                if (dataItem == null) return null;
            }
            return dataItem;
        }

        public void Render(StringBuilder sb)
        {
            EnsureDataBound();
            using (var sbWriter = new StringWriter(sb))
            using (var writer = new HtmlTextWriter(sbWriter))
            {
                Render(writer);
                writer.Flush();
                sbWriter.Flush();
            }
        }

        public void RenderText(StringBuilder sb)
        {
            RenderOnlyText = true;
            Render(sb);
            RenderOnlyText = false;
        }

        public virtual IEnumerable<ListControlItem> GetAllItems()
        {
            return Items.Union(Items.Where(r => r.IsGroup).SelectMany(r => r.AllChildren()));
        }

        public void SelectAllItems()
        {
            foreach (var item in GetAllItems())
                item.Selected = !item.IsGroup;
        }

        public void UpdateItems(IDictionary parentValues)
        {
            if (ReadOnly) return;
            var dataSourceView = GetData();
            var keys = new Dictionary<string, string>();
            var oldValues = new Dictionary<string, object>();
            var items = GetAllItems().Where(r => r.Enabled && !r.IsGroup);

            foreach (var item in items) //если запись не была активной, то с ней ничего не делаем.
            {
                //delete
                if (!string.IsNullOrEmpty(item.SelectedKey) && !item.Selected)
                {
                    InitValues(parentValues, item);
                    InitDeleteValues(keys, item);
                    dataSourceView.Delete(keys, parentValues, OnDeleted);
                }
                //insert
                else if (string.IsNullOrEmpty(item.SelectedKey) && item.Selected)
                {
                    InitValues(parentValues, item);
                    InitInsertValues(parentValues, item);
                    dataSourceView.Insert(parentValues, OnInserted);
                }
                else if (CheckNeedUpdate(item))
                {
                    InitValues(parentValues, item);
                    InitValues(oldValues, item);
                    InitUpdateValues(parentValues, item);
                    InitDeleteValues(keys, item);
                    InitDeleteValues(oldValues, item);
                    dataSourceView.Update(keys, parentValues, oldValues, OnInserted);
                }
            }
        }

        protected virtual bool CheckNeedUpdate(ListControlItem item)
        {
            return false;
        }

        protected virtual void InitInsertValues(IDictionary values, ListControlItem item)
        {
        }
        
        protected virtual void InitUpdateValues(IDictionary values, ListControlItem item)
        {
        }
        
        protected virtual void InitDeleteValues(IDictionary keys, ListControlItem item)
        {
            keys[DataSelectedKeyField] = item.SelectedKey;
        }

        protected virtual void InitValues(IDictionary values, ListControlItem item)
        {
            values[DataValueField] = item.Value;
        }

        protected virtual bool OnInserted(int affectedrecords, Exception exception)
        {
            return false;
        }

        protected virtual bool OnDeleted(int affectedRecords, Exception exception)
        {
            return false;
        }

        protected virtual void RenderBeginRows(HtmlTextWriter writer)
        {
        }

        protected virtual void RenderEndRows(HtmlTextWriter writer)
        {
        }

        #region Render

        public override void RenderBeginTag(HtmlTextWriter writer)
        {
            if (!RenderOnlyText)
            {
                writer.AddAttribute(HtmlTextWriterAttribute.Border, "0");
                writer.AddAttribute(HtmlTextWriterAttribute.Valign, "center");
                writer.AddAttribute(HtmlTextWriterAttribute.Align, "left");
                writer.AddAttribute(HtmlTextWriterAttribute.Id, ClientID);
                writer.AddAttribute(HtmlTextWriterAttribute.Class, CssClass);
                foreach (var styleKey in Style.Keys)
                    writer.AddStyleAttribute(styleKey.ToString(), Style[styleKey.ToString()]);

                writer.RenderBeginTag(HtmlTextWriterTag.Table);
            }
        }

        public override void RenderEndTag(HtmlTextWriter writer)
        {
            if (!RenderOnlyText)
                writer.RenderEndTag();
        }

        protected override void RenderContents(HtmlTextWriter writer)
        {
            RenderBeginRows(writer);
            //если контрол только для чтения, то отображаем выбранные записи
            var items = ReadOnly || ShowOnlySelected
                            ? Items.Where(r => r.AnySelected()).ToList()
                            : Items;
            var rowsCount = (int)Math.Ceiling(items.Count / (decimal)Columns);
            if (RenderOnlyText)
                rowsCount = 1;

            var width = (100 / Columns) + "%";
            var args = new BaseListDataBoundRenderEventArgs {RowsCount = rowsCount, Width = width};
            RenderItems(writer, items, args);
            RenderEndRows(writer);
        }

        protected virtual void RenderItems(HtmlTextWriter writer, ListControlItem item, BaseListDataBoundRenderEventArgs args)
        {
            var children = ReadOnly || ShowOnlySelected
                               ? item.Children.Where(r => r.AnySelected()).ToList()
                               : item.Children;
            RenderItems(writer, children, args);
        }

        protected virtual void RenderItems(HtmlTextWriter writer, List<ListControlItem> items, BaseListDataBoundRenderEventArgs args)
        {
            if (items.Any(r => r.Children != null && r.Children.Count > 0))
            {
                foreach (var child in items)
                {
                    if (child.IsGroup)
                    {
                        args.RowsCount = (int)Math.Ceiling(child.ChildrenCount() / (decimal)Columns);
                        if (child.HideGroup)
                            RenderItems(writer, child, args);
                        else
                            RenderGroupItem(writer, child, args);
                    }
                    else
                    {
                        if (!RenderOnlyText)
                            writer.RenderBeginTag(HtmlTextWriterTag.Tr);
                        
                        RenderItem(writer, child, args);

                        if (RenderOnlyText)
                            writer.Write("\r\n");
                        else
                            writer.RenderEndTag();
                    }
                }
            }
            else if (Columns == 1)
            {
                foreach (var item in items)
                {
                    writer.RenderBeginTag(HtmlTextWriterTag.Tr);
                    RenderItem(writer, item, args);
                    writer.RenderEndTag();
                }
            }
            else
            {
                // обход по строкам, затем выбиараем запись в слудующую колнку, т.е. шаг по записям по кол-ву строк. Пример обхода:
                // 1 4 7
                // 2 5 8
                // 3 6
                for (var rowIndex = 0; rowIndex < args.RowsCount; rowIndex++)
                    for (var index = rowIndex; index < items.Count; index += args.RowsCount)
                    {
                        var needBeginTr = index == rowIndex;
                        var needEndTr = index + args.RowsCount >= items.Count;

                        if (needBeginTr) writer.RenderBeginTag(HtmlTextWriterTag.Tr);
                        
                        RenderItem(writer, items[index], args);
                        
                        if (needEndTr) writer.RenderEndTag();
                    }
            }
        }
        
        protected virtual void RenderGroupItem(HtmlTextWriter writer, ListControlItem item, BaseListDataBoundRenderEventArgs args)
        {
            if (RenderOnlyText)
            {
                RenderItems(writer, item, args);
            }
            else
            {
                var clientID = item.Value;
                writer.RenderBeginTag(HtmlTextWriterTag.Tr); //tr
                writer.RenderBeginTag(HtmlTextWriterTag.Td); //Td
                writer.RenderBeginTag(HtmlTextWriterTag.Div); //outer DIV
                writer.RenderBeginTag(HtmlTextWriterTag.Fieldset); //Fieldset

                #region Legend

                writer.RenderBeginTag(HtmlTextWriterTag.Legend);

                if (item.Selectable && item.ChildrenCount() > 1)
                {
                    var checkBox = new BaseCheckBox
                                       {
                                           Checked = item.AllSelected(),
                                           OnClick = "CheckInInnerGroup(this);",
                                       };
                    checkBox.Render(writer, ExtenderAjaxControl);
                }

                writer.AddStyleAttribute(HtmlTextWriterStyle.Cursor, "pointer");
                writer.AddAttribute(HtmlTextWriterAttribute.Id, clientID + "_Legend");
                writer.AddAttribute(HtmlTextWriterAttribute.Title, Resources.PressToOpenCloseGroup);
                writer.AddAttribute("ondblclick", "ClickInnerGroup(this);");
                writer.AddAttribute("clickOnDblClickInParent", "on");
                writer.RenderBeginTag(HtmlTextWriterTag.Span);
                writer.Write(item.Text);
                writer.RenderEndTag(); //Span

                writer.RenderEndTag(); //Legend

                #endregion

                writer.AddAttribute(HtmlTextWriterAttribute.Id, clientID);
                writer.RenderBeginTag(HtmlTextWriterTag.Div); //inner DIV

                #region Table

                writer.AddAttribute(HtmlTextWriterAttribute.Border, "0");
                writer.AddStyleAttribute(HtmlTextWriterStyle.Width, "100%");
                writer.RenderBeginTag(HtmlTextWriterTag.Table);

                RenderItems(writer, item, args);

                writer.RenderEndTag(); //table

                #endregion

                writer.RenderEndTag(); //inner DIV

                writer.RenderEndTag(); //Fieldset
                writer.RenderEndTag(); //outer DIV
                writer.RenderEndTag(); //td
                writer.RenderEndTag(); //tr
            }
        }

        #endregion

        #region Реализация хранения колекции элементов в HiddenField

        protected string GetItemsUniqueID()
        {
            return UniqueID + "$Items";
        }

        protected string GetItemsClientID()
        {
            return ClientID + "_Items";
        }

        public string GetMemberItemsClientID()
        {
            return ClientID + "_MembersItems";
        }

        protected void LoadItemsFromPostData()
        {
            var postData = HttpContext.Current.Request.Form[GetItemsUniqueID()];
            if (postData != null)
            {
                var ser = new JavaScriptSerializer();
                Items = ser.Deserialize<List<ListControlItem>>(postData);
                RequiresDataBinding = false;
            }
        }

        protected void RenderHiddenFieldForItems(HtmlTextWriter writer)
        {
            var ser = new JavaScriptSerializer();

            writer.AddAttribute(HtmlTextWriterAttribute.Id, GetItemsClientID());
            writer.AddAttribute(HtmlTextWriterAttribute.Name, GetItemsUniqueID());
            writer.AddAttribute(HtmlTextWriterAttribute.Type, "hidden");
            writer.AddAttribute(HtmlTextWriterAttribute.Value, ser.Serialize(Items));
            writer.RenderBeginTag(HtmlTextWriterTag.Input);
            writer.RenderEndTag();

            writer.AddAttribute(HtmlTextWriterAttribute.Id, GetMemberItemsClientID());
            writer.AddAttribute(HtmlTextWriterAttribute.Type, "hidden");
            writer.AddAttribute(HtmlTextWriterAttribute.Value, string.Join(",", Items.Select(r => r.Value).ToArray()));
            writer.RenderBeginTag(HtmlTextWriterTag.Input);
            writer.RenderEndTag();
        }

        #endregion
    }
}