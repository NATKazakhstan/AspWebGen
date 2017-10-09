using System;
using System.Collections.Generic;
using System.Web.UI;
using System.Web.UI.WebControls;
using Nat.Web.Controls.Properties;
using Nat.Web.Tools.Security;

namespace Nat.Web.Controls.UserControls
{
    public abstract class DictionariesView : UserControl, IAccessControl
    {
        private readonly List<Control> _controls = new List<Control>();
        private readonly List<Control> _controlsToDispose = new List<Control>();
        private bool _inited;
        private MainPageUrlBuilder _url;

        #region declared in ascx

        protected TextBox tbFilter;
        protected Button bFilter;
        protected TreeView tvDictionaries;
        protected Panel pList;
        protected ImageCheckBox ImageCheckBox;
        protected Panel Panel1;
        protected PlaceHolder PlaceHolder1;

        #endregion

        protected virtual string[] OpenPermissions { get { return null; } }
        protected virtual string[] ReadPermissions { get { return null; } }
        protected virtual string EditPermissionPrefix { get { return null; } }
        protected virtual string EditPermissionPostfix { get { return "_EDIT"; } }

        private List<DictionariesViewItem> Items
        {
            get
            {
                var state = (List<DictionariesViewItem>) ViewState["DictionariesView_Items"];
                if (state == null)
                {
                    state = new List<DictionariesViewItem>();
                    ViewState["DictionariesView_Items"] = state;
                }
                return state;
            }
        }

        public override ControlCollection Controls
        {
            get
            {
                if (_inited) EnsureChildDictionaries();
                return base.Controls;
            }
        }

        protected virtual bool ChildDictionariesCreated { get; set; }

        protected MainPageUrlBuilder Url
        {
            get { return _url ?? (_url = MainPageUrlBuilder.Current); }
        }

        public override void Dispose()
        {
            base.Dispose();
            foreach (Control control in _controlsToDispose)
                control.Dispose();
        }

        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);
            _inited = true;
        }

        protected virtual void EnsureChildDictionaries()
        {
            if (!ChildDictionariesCreated)
            {
                CreateChildDictionaries();
                ChildDictionariesCreated = true;
            }
        }

        protected virtual void CreateChildDictionaries()
        {
            if (!string.IsNullOrEmpty(Url.UserControl)) AddControl(Url.UserControl);
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            Page.LoadComplete += Page_OnLoadComplete;
            ImageCheckBox.ImageUrlChecked = Themes.ThemePath + "/open_spr.gif";
            ImageCheckBox.ImageUrlUnchecked = Themes.ThemePath + "/close_spr.gif";
            EnsureChildDictionaries();

            if (Url.IsFilterWindow)
            {
                tvDictionaries.Visible = false;
                pList.Visible = false;
                ImageCheckBox.Checked = false;
                ImageCheckBox.Visible = false;
            }

            if (ReadPermissions != null && !UserRoles.IsInAnyRoles(ReadPermissions))
                SetVisibleTreeViewItems();
            if (!IsPostBack && !string.IsNullOrEmpty(Url.UserControl)) ImageCheckBox.Checked = false;
        }

        private void SetVisibleTreeViewItems()
        {
            if (!tvDictionaries.Visible) return;
            for (int iNode = tvDictionaries.Nodes.Count - 1; iNode >= 0; iNode--)
            {
                var treeNode = tvDictionaries.Nodes[iNode];
                for (int i = treeNode.ChildNodes.Count - 1; i >= 0; i--)
                {
                    if (EditPermissionPrefix != null && !UserRoles.IsInRole(EditPermissionPrefix + treeNode.ChildNodes[i].Value.Substring(0, treeNode.ChildNodes[i].Value.Length - 7) + EditPermissionPostfix))
                        treeNode.ChildNodes.RemoveAt(i);
                }
                if (treeNode.ChildNodes.Count == 0)
                    tvDictionaries.Nodes.RemoveAt(iNode);
            }
        }

        private void Page_OnLoadComplete(object sender, EventArgs e)
        {
            Panel1.Visible = Items == null || Items.Count <= 1;
        }

        public void SelectItem(ISelectedValue selectItem, string idControl, string tableName)
        {
            var control = (Control) selectItem;
            control.Visible = false;
            AddControl(tableName);
            Items[Items.Count - 1].ControlID = idControl;
            Items.Add(new DictionariesViewItem(tableName, ""));
        }

        private void AddControl(string tableName)
        {
            var control = BaseMainPage.LoadControl(Page, tableName);
            Url.UserControl = tableName;
            control.ID = tableName + _controls.Count;
            var accessControl = control as IAccessControl;
            if (ReadPermissions != null && !UserRoles.IsInAnyRoles(ReadPermissions))
            {
                if (accessControl != null && !accessControl.CheckPermit(Page))
                {
                    var literal = new LiteralControl(Resources.SAccessDanied);
                    PlaceHolder1.Controls.Add(literal);
                    return;
                }
            }
            var sControl = control as ISelectedValue;
            if (sControl != null)
            {
                sControl.IsNew = Url.IsNew;
                sControl.IsRead = Url.IsRead;
                sControl.ShowHistory = Url.ShowHistory;
                sControl.IsSelect = Url.IsSelect;
            }
            PlaceHolder1.Controls.Add(control);
            _controls.Add(control);
            return;
        }

        protected void tvDictionaries_SelectedNodeChanged(object sender, EventArgs e)
        {
            if (!String.IsNullOrEmpty(tvDictionaries.SelectedValue) &&
                !tvDictionaries.Nodes.Contains(tvDictionaries.SelectedNode))
            {
                if (_controls.Count == 1)
                {
                    Items.Clear();
                    _controlsToDispose.Add(_controls[0]);
                    PlaceHolder1.Controls.Remove(_controls[0]);
                    _controls.Clear();
                }
                if (_controls.Count == 0)
                {
                    AddControl(tvDictionaries.SelectedValue);
                    Items.Add(new DictionariesViewItem(tvDictionaries.SelectedValue, ""));
                }
            }
        }

        protected override void OnPreRender(EventArgs e)
        {
            FilterTree();
            Page.Form.Action = Url.CreateUrl();
            base.OnPreRender(e);
        }

        private void FilterTree()
        {
            if (!tvDictionaries.Visible) return;
            if(!string.IsNullOrEmpty(Filter))
            {
                for (int index = tvDictionaries.Nodes.Count - 1; index >= 0; index--)
                {
                    var node = tvDictionaries.Nodes[index];
                    var findChild = false;
                    for (int i = node.ChildNodes.Count - 1; i >= 0; i--)
                    {
                        if (node.ChildNodes[i].Text.ToLower().Contains(Filter))
                        {
                            findChild = true;
                            continue;
                        }
                        node.ChildNodes.RemoveAt(i);
                    }
                    if(!findChild)
                        tvDictionaries.Nodes.RemoveAt(index);
                    else if(node.ChildNodes.Count <= 5)
                        node.Expand();
                    else
                        node.Collapse();
                }
            }
            else
                tvDictionaries.CollapseAll();
            if (!string.IsNullOrEmpty(Url.UserControl) && tvDictionaries.SelectedValue != Url.UserControl)
            {
                foreach (TreeNode node in tvDictionaries.Nodes)
                {
                    foreach (TreeNode childNode in node.ChildNodes)
                    {
                        if(childNode.Value == Url.UserControl)
                        {
                            childNode.Select();
                            break;
                        }
                    }
                    if (tvDictionaries.SelectedNode != null)
                        break;
                }
            }
            if (tvDictionaries.SelectedNode != null)
            {
                tvDictionaries.SelectedNode.Expand();
                if (tvDictionaries.SelectedNode.Parent != null) 
                    tvDictionaries.SelectedNode.Parent.Expand();
            }
        }

        protected void bApply_Click(object sender, EventArgs e)
        {
            Filter = tbFilter.Text;
        }

        protected string Filter
        {
            get { return (string)ViewState["filter"]; }
            set { ViewState["filter"] = value.ToLower(); }
        }

        #region Nested type: DictionariesViewItem

        [Serializable]
        public class DictionariesViewItem
        {
            public DictionariesViewItem(string tableName, string controlID)
            {
                TableName = tableName;
                ControlID = controlID;
            }

            public string ControlID { get; set; }
            public string TableName { get; set; }
        }

        #endregion

        public bool CheckPermit(Page page)
        {
            return OpenPermissions == null || UserRoles.IsInAnyRoles(OpenPermissions);
        }
    }
}