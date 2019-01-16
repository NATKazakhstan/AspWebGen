/*
* Created by: Sergey V. Shpakovskiy
* Created: 2012.10.03
* Copyright © JSC NAT Kazakhstan 2012
*/

using System;

namespace Nat.Web.Tools.ExtNet
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Web.UI;
    using System.Web.UI.WebControls;

    using Ext.Net;

    using Nat.Web.Controls.DataBinding.Tools;
    using Nat.Web.Controls.Properties;

    public class GridButtonsColumn : GridColumn
    {
        private List<ActionItem> additionalButtons;

        public const string ActionColumnsID = "DefaultButtons";

        public GridButtonsColumn()
        {
            EditVisible = true;
            LookVisible = true;
            DeleteVisible = true;
        }

        public bool EditVisible { get; set; }

        public bool LookVisible { get; set; }

        public bool DeleteVisible { get; set; }

        public bool EditInJournal { get; set; }

        public string EditUrl { get; set; }

        public string LookUrl { get; set; }

        public string DeleteUrl { get; set; }

        public string ControlIDWithDirectMethod { get; set; }

        public Control JournalControl { get; set; }

        public string StoreClientID { get; set; }

        /// <summary>
        /// Необходимо создавать ActionItem. И инициализировать Icon, Tooltip, Handler = "function(view, rowIndex, colIndex, item, eArgs, record){sctipt}".
        /// Для создания PostBack необходимо использовать метод Page.ClientScript.GetPostBackEventReference(this, argument) для получения скрипта на PostBack с аргументами.
        /// </summary>
        public List<ActionItem> AdditionalButtons
        {
            get
            {
                return additionalButtons ?? (additionalButtons = new List<ActionItem>());
            }

            set
            {
                additionalButtons = value;
            }
        }
        
        public virtual int CountIcons
        {
            get
            {
                var buttons = GetAdditionalButtons();
                return (EditVisible && !string.IsNullOrEmpty(LookUrl) ? 1 : 0)
                    + (LookVisible && !string.IsNullOrEmpty(LookUrl) ? 1 : 0)
                    + (DeleteVisible ? 1 : 0)
                    + (buttons == null ? 0 : buttons.Count);
            }
        }

        public override string Width
        {
            get { return (CountIcons * 20).ToString() + "px"; }
        }

        protected List<ActionItem> GetAdditionalButtons()
        {
            return additionalButtons;
        }

        public override IEnumerable<ModelField> CreateModelFields()
        {
            return new[]
                {
                    new ModelField("CanEdit", ModelFieldType.Boolean)
                        {
                            ServerMapping = "CanEdit",
                        }, 
                    new ModelField("CanDelete", ModelFieldType.Boolean)
                        {
                            ServerMapping = "CanDelete",
                        },  
                    new ModelField("RowName", ModelFieldType.String)
                        {
                            ServerMapping = "Name",
                        },
                    new ModelField("AdditionalValues", ModelFieldType.String)
                        {
                            ServerMapping = "AdditionalValues",
                        }, 
                };
        }

        public override ColumnBase CreateColumn()
        {
            return CreateActionColumn();
        }

        private ColumnBase CreateActionColumn()
        {
            var column = new ActionColumn
                {
                    ID = "DefaultButtons",
                    Width = new Unit(Width),
                    MenuDisabled = true,
                    Align = Alignment.Center,
                    MenuText = Properties.Resources.SActionColumnText,
                };

            column.PreRender += AddActionItems;
            
            return column;

            void AddActionItems(object sender, EventArgs e)
            {
                if (column.Items.Count > 0)
                    return;

                if (LookVisible && !string.IsNullOrEmpty(LookUrl))
                {
                    var actionItem = new ActionItem
                    {
                        Icon = Icon.Note,
                        Tooltip = Resources.SLook,
                        Handler = string.Format(@"function(view, rowIndex, colIndex, item, eArgs, record){{{0}}}", GetLookScript()),
                        IconCls = ResourceManager.GetIconClassName(Icon.Note) + " CursorPointer",
                    };
                    actionItem.CustomConfig.Add(new ConfigItem("ActionType", "Look"));
                    column.Items.Add(actionItem);
                }

                if (EditVisible && !string.IsNullOrEmpty(EditUrl))
                {
                    var actionItem = new ActionItem
                    {
                        Icon = Icon.NoteEdit,
                        Tooltip = Resources.SEditText,
                        Handler = string.Format(@"function(view, rowIndex, colIndex, item, eArgs, record){{{0}}}", GetEditScript()),
                        IconCls = ResourceManager.GetIconClassName(Icon.NoteEdit) + " CursorPointer",
                    };
                    actionItem.IsDisabled.Handler = "return !record.data.CanEdit || record.data.id == null;";
                    actionItem.CustomConfig.Add(new ConfigItem("ActionType", "Edit"));
                    column.Items.Add(actionItem);
                }

                if (DeleteVisible)
                {
                    var actionItem = new ActionItem
                    {
                        Icon = Icon.Cross,
                        Tooltip = Resources.SDeleteText,
                        Handler = string.Format(@"function(view, rowIndex, colIndex, item, eArgs, record){{{0}}}", GetDeleteScript()),
                        IconCls = ResourceManager.GetIconClassName(Icon.Cross) + " CursorPointer",
                    };
                    actionItem.IsDisabled.Handler = "return !record.data.CanDelete;";
                    actionItem.CustomConfig.Add(new ConfigItem("ActionType", "Delete"));
                    column.Items.Add(actionItem);
                }

                var buttons = GetAdditionalButtons();
                if (buttons != null)
                {
                    foreach (var actionItem in buttons)
                    {
                        if (actionItem.Icon != Icon.None)
                        {
                            var iconCls = ResourceManager.GetIconClassName(actionItem.Icon);
                            actionItem.IconCls += iconCls + " CursorPointer";
                        }
                        else
                            actionItem.IconCls += " CursorPointer";

                        if (actionItem.CustomConfig.Contains("insertIndex"))
                        {
                            var index = Convert.ToInt32(actionItem.CustomConfig.First(r => r.Name == "insertIndex").Value);
                            column.Items.Insert(index, actionItem);
                        }
                        else
                            column.Items.Add(actionItem);
                    }
                }
            }
        }

        protected virtual void AddEditButton(StringBuilder listener, CommandColumn commandColumn)
        {
            var editCommand = new GridCommand
                {
                    CommandName = "Edit",
                    Icon = Icon.NoteEdit,
                };
            editCommand.ToolTip.Text = Resources.SEditText;
            commandColumn.Commands.Add(editCommand);
            listener.Append("if (command == 'Edit'){");
            listener.Append(GetEditScript());
            listener.Append("}");
        }

        public Func<string, string> EditUrlJavaScript { get; set; } =
            urlTemplate => $"'{urlTemplate}'.replace('{{0}}', record.internalId)";

        public Func<string, string> EditWindowTitleJavaScript { get; set; } =
            title => $"'{title}' + ': ' + record.data.RowName";

        private string GetEditScript()
        {
            return string.Format(
                @"
    if (!record.data.CanEdit || record.data.id == null){{
        Ext.Msg.alert({1}, '{2}');
    }}
    else {{
        var w = #{{Window}};
        w.loader.url = {0};
        w.setTitle({1});
        w.show();
        w.loader.load();
        var frame = w.getFrame();
        frame.CloseOnSaveSuccessfull = function (newValue, refParent) {{
            #{{Window}}.hide();
            #{{SelectIDHidden}}.setValue(newValue);
            var store = {3};
            if (store.tree == null)
                store.reload();
            else if (refParent != null && store.getNodeById(refParent) != null) {{
                if (!store.getNodeById(newValue) || store.getNodeById(newValue).data.refParent == 0){{
                    Array.add(needTreeReload, function() {{
                        if (store.getNodeById(refParent) != null)
                            store.getNodeById(refParent).reload();
                    }});
                    store.getNodeById('Root').reload();
                }}
                else {{
                    store.getNodeById(refParent).data.leaf = false;

                    var parentNode = store.getNodeById(store.getNodeById(newValue).data.refParent);
                    if (parentNode.childNodes.length == 1) {{
                        parentNode.data.leaf = true;
                    }}

                    parentNode.reload();
                    store.getNodeById(refParent).reload();
                }}
            }}
            else
                store.getNodeById('Root').reload();
        }}
    }}",
                EditUrlJavaScript(EditUrl),
                EditWindowTitleJavaScript(Resources.SEdit),
                Resources.ECanNotEditRecord, 
                StoreClientID ?? "#{store}");
        }

        protected virtual void AddLookButton(StringBuilder listener, CommandColumn commandColumn)
        {
            var editCommand = new GridCommand
                {
                    CommandName = "Look",
                    Icon = Icon.Note,
                };
            editCommand.ToolTip.Text = Resources.SLook;
            commandColumn.Commands.Add(editCommand);
            listener.Append("if (command == 'Look'){");
            listener.Append(GetLookScript());
            listener.Append("}");
        }

        public Func<string, string> LookUrlJavaScript { get; set; } = 
            urlTemplate => $"'{urlTemplate}'.replace('{{0}}', record.internalId)";

        public Func<string, string> LookWindowTitleJavaScript { get; set; } =
            title => $"'{title}' + ': ' + record.data.RowName";

        private string GetLookScript()
        {
            return string.Format(
                @"
    var w = #{{Window}};
    w.loader.url = {0};
    w.setTitle({1});
    w.show();
    if (!w.collapsed)
        w.loader.load();",
                LookUrlJavaScript(LookUrl),
                LookWindowTitleJavaScript(Resources.SLook));
        }

        protected virtual void AddDeleteButton(StringBuilder listener, CommandColumn commandColumn)
        {
            var editCommand = new GridCommand
                {
                    CommandName = "Delete",
                    Icon = Icon.Cross,
                };
            editCommand.ToolTip.Text = Resources.SDeleteText;
            commandColumn.Commands.Add(editCommand);
            listener.Append("if (command == 'Delete'){");
            listener.Append(GetDeleteScript());
            listener.Append("}");
        }

        private string GetDeleteScript()
        {
            string deleteScript;
            if (EditInJournal)
            {
                deleteScript = "record.store.remove(record);";
            }
            else if (!string.IsNullOrEmpty(DeleteUrl))
            {
                deleteScript = string.Format(@"
var w = #{{ModalWindow}};
w.loader.url = '{0}'.replace('{{0}}', record.internalId);
w.setTitle('{1}' + ': ' + record.data.RowName);
w.show();
if (!w.collapsed)
    w.loader.load();
", DeleteUrl, Resources.SDeleteText);
            }
            else // fix: используется ActionColumn который не верно рендерит доступ до DirectMethods
            {
                if (string.IsNullOrEmpty(ControlIDWithDirectMethod))
                    ControlIDWithDirectMethod = JournalControl?.ClientID ?? "PlaceHolderMain_item";

                deleteScript = @"
    if (record.data.id == null){{
        record.store.remove(record);
    }}
    else {{
        var treeStore = record.store;
        if (treeStore.tree != null && record.data.refParent != null && treeStore.getNodeById(record.data.refParent) != null) {{
            var node = treeStore.getNodeById(record.data.refParent);
            if (!node.isLeaf() && node.childNodes.length == 1) {
                node.data.leaf = true;
            }
        }}

        #{DirectMethods}." + ControlIDWithDirectMethod + @".DeleteRow(record.internalId);
    }}";
            }

            return string.Format(
                    @"
    if (!record.data.CanDelete){{
        Ext.Msg.alert('{1}', '{3}');
    }}
    else {{
        Ext.Msg.confirm(
            '{1}',
            '{2}',
            function (btn) {{
                if (btn == 'yes') {{
                    {0}
                }}
            }});
    }}",
                    deleteScript,
                    Resources.SDeleteText,
                    Resources.SConfirmDeleteText,
                    Resources.ENoPermitToDeleteRecord);
          
        }

        public static string AddButtonHandler(string addUrl)
        {
            return AddButtonHandler(addUrl, null);
        }

        public static string AddButtonHandler(string addUrl, string storeID)
        {
            return string.Format(
                @"
    var w = #{{Window}};
    w.loader.url = '{0}';
    w.setTitle('{1}');
    w.show();
    if (!w.collapsed)
        w.loader.load();
    var frame = w.getFrame();
    frame.CloseOnSaveSuccessfull = function (newValue, refParent) {{
        #{{Window}}.hide();
        #{{SelectIDHidden}}.setValue(newValue);
        var store = {2};
        if (store.tree == null)
            store.reload();
        else if (refParent != null && store.getNodeById(refParent) != null) {{
            store.getNodeById(refParent).data.leaf = false;
            store.getNodeById(refParent).reload();
        }}
        else
            store.getNodeById('Root').reload();
    }}",
                addUrl,
                Resources.SAddingText, 
                storeID ?? "#{store}");
        }
    }
}
