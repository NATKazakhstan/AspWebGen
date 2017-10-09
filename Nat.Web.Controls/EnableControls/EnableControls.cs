using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Web.Script.Serialization;
using System.Web.UI;
using System.Web.UI.WebControls;
using AjaxControlToolkit;
using Nat.Web.Controls.DataBinding.Tools;

[assembly : WebResource("Nat.Web.Controls.EnableControls.EnableControlsBehavior.js", "text/javascript")]

namespace Nat.Web.Controls
{
    [ParseChildren(true)]
    [PersistChildren(false)]
    [ClientScriptResource("Nat.Web.Controls.EnableControls.EnableControlsBehavior", "Nat.Web.Controls.EnableControls.EnableControlsBehavior.js")]
    public class EnableControls : WebControl, IScriptControl
    {
        private readonly List<EnableItem> listControls = new List<EnableItem>();
        private readonly IList<EnableItem> clientListControls = new List<EnableItem>();
        private string _javaControls;

        public EnableControls()
        {
            AllowInvokeUpdateMethod = true;
        }

        /// <summary>
        /// Список контролов которым нужно задать условия активности.
        /// </summary>
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        [PersistenceMode(PersistenceMode.InnerProperty)]
        [Description("Список контролов которым нужно задать условия активности.")]
        public List<EnableItem> ListControls
        {
            get { return listControls; }
        }

        private ScriptManager CurrentScriptManager
        {
            get
            {
                ScriptManager sm = ScriptManager.GetCurrent(Page);

                if (sm == null)
                {
                    throw new InvalidOperationException(
                        string.Format(CultureInfo.InvariantCulture,
                                      "The control with ID '{0}' requires a ScriptManager on the page. The ScriptManager must appear before any controls that need it.",
                                      new object[] {ID}));
                }
                return sm;
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Browsable(false)]
        public Control Owner { get; set; }

        public bool AllowInvokeUpdateMethod { get; set; }
        public bool AllowUseControlIdAsClientId { get; set; }
        public IList<string[]> ValuesForFormatControlID { get; set; }

        #region IScriptControl Members

        public IEnumerable<ScriptDescriptor> GetScriptDescriptors()
        {
            if (Page != null && Visible)
            {
                var desc = new ScriptBehaviorDescriptor("Nat.Web.Controls.EnableControls.EnableControlsBehavior",
                                                        ClientID) {ID = String.Format("{0}_enableControls", ClientID)};

                desc.AddProperty("controls", _javaControls);
                desc.AddProperty("items", GenerateEnableItems());

                yield return desc;
            }
        }

        public IEnumerable<ScriptReference> GetScriptReferences()
        {
            var references = new List<ScriptReference>();
            references.AddRange(ScriptObjectBuilder.GetScriptReferences(GetType()));
            return references;
        }

        #endregion

        protected override void OnPreRender(EventArgs e)
        {
            if (!DesignMode)
            {
                CurrentScriptManager.RegisterScriptControl(this);
                Page.PreRenderComplete += delegate
                                              {
                                                  Update();
                                                  if (AllowInvokeUpdateMethod && !IsEmptyJavaControls())
                                                  {
                                                      //todo: Можно попробовать сделать провероку на необходимость обновления
                                                      var panel = Parent as UpdatePanel;
                                                      if (panel != null) panel.Update();
                                                      else
                                                      {
                                                          panel = Parent.Parent as UpdatePanel;
                                                          if (panel != null && panel.ContentTemplateContainer == Parent)
                                                          {
                                                              panel.Update();
                                                          }
                                                      }
                                                  }
                                              };
            }
            base.OnPreRender(e);
        }

        public void Update()
        {
            _javaControls = GenerateControls();
        }

        protected bool IsEmptyJavaControls()
        {
            return string.IsNullOrEmpty(_javaControls) || "{}".Equals(_javaControls);
        }

        protected override void Render(HtmlTextWriter writer)
        {
            if (!DesignMode)
            {
                if (IsEmptyJavaControls())
                    Update();
                CurrentScriptManager.RegisterScriptDescriptors(this);
                base.Render(writer);
            }
            else
                writer.Write("EnableControls - " + ID);
        }

        private string GenerateEnableItems()
        {
            var jss = new JavaScriptSerializer();
            string serializedInfo = jss.Serialize(clientListControls);
            return serializedInfo;
        }

        private string GenerateControls()
        {
            var list = new Dictionary<string, string>();
            foreach (EnableItem item in listControls)
            {
                var webControl = item.TargetControl as WebControl;
                var control = item.TargetControl;
                if (control == null && item.TargetControlID != null)
                {
                    if (Owner != null)
                        control = WebUtils.FindControlRecursive(Owner, item.TargetControlID);
                    else if (Page != null)
                        control = WebUtils.FindControlRecursive(Page, item.TargetControlID);
                    webControl = control as WebControl;
                }
                if (control == null && !AllowUseControlIdAsClientId) continue;

                item.targetID = control != null ? control.ClientID : item.TargetControlID;
                if (AllowUseControlIdAsClientId)
                {
                    if (ValuesForFormatControlID == null)
                    {
                        GenerateControlsAllowClientID(item.EnableItems, list, this, null);
                        clientListControls.Add(item);
                    }
                    else
                    {
                        foreach (var values in ValuesForFormatControlID)
                        {
                            var clone = item.Clone();
                            clone.TargetControlID = string.Format(clone.TargetControlID, values);
                            for (int i = 0; i < clone.aditinalTargetID.Count; i++)
                                clone.aditinalTargetID[i] = string.Format(clone.aditinalTargetID[i], values);
                            clone.targetID = string.Format(clone.targetID, values);
                            GenerateControlsAllowClientID(clone.EnableItems, list, this, values);
                            clientListControls.Add(clone);
                        }
                    }
                }
                else
                {
                    var res = GenerateControls(item.EnableItems, list, this);
                    item.Result = res; //note: тут то нормально, а вот если по клиентским id, то надеюсь не критично не выставление результата
                    if (webControl != null && !(webControl is CheckBox))
                    {
                        if ((item.EnableMode & EnableMode.Disable) == EnableMode.Disable)
                            webControl.Enabled = item.Result;
                        else
                            webControl.Enabled = true;

                        if ((item.EnableMode & EnableMode.Hide) == EnableMode.Hide && !item.Result)
                            webControl.Style["display"] = "none";
                        else if (webControl.Attributes["isIgnoreVisible"] == null)
                            webControl.Style.Remove("display");

                        if ((item.EnableMode & EnableMode.ReadOnly) == EnableMode.ReadOnly && !item.Result)
                            webControl.Attributes["readonly"] = "true";
                        else
                            webControl.Attributes.Remove("readonly");

                        if ((item.EnableMode & EnableMode.ChangeValidateGroup) == EnableMode.ChangeValidateGroup && !item.Result)
                            webControl.Attributes["EnableMode.ChangeValidateGroup"] = "true";
                        else
                            webControl.Attributes.Remove("EnableMode.ChangeValidateGroup");
                    }

                    clientListControls.Add(item);
                }
                /*
                 * Возможно нужно оставить только не в полном варианте, т.е. часть по скрытию контролов без id и не серверных ляжет на клиента.
                 * 
                    bool enable = GenerateControls(item.EnableItems, list, this);
                    if(item.EnableMode == EnableMode.Disable)
                        control.Enabled = enable;
                    else if (!enable)
                    {
                        control.Style.Add("display", "none");
                        SetParentDisplayNone(control);
                    }
                    */
            }
            var jss = new JavaScriptSerializer();
            string serializedInfo = jss.Serialize(list);
            return serializedInfo;
        }

        private static void GenerateControlsAllowClientID(EnableList enableList, IDictionary<string, string> list,
                                     EnableControls enableControls, string[] values)
        {
            foreach (IEnableControl control in enableList.Items)
            {
                control.EnableControls = enableControls;
                control.RefreshValues();
                var innerList = control as EnableList;
                if (innerList != null)
                    GenerateControlsAllowClientID(innerList, list, enableControls, values);
                else if (control.ControlID != null)
                {
                    var controlId = control.ControlID;
                    var clientId = control.GetClientID() ?? controlId;//позволит использовать на клиентские контролы без серверных
                    if (values != null)
                    {
                        control.ControlID = controlId = string.Format(controlId, values);
                        clientId = string.Format(clientId, values);
                    }
                    if (!list.ContainsKey(controlId)) list.Add(controlId, clientId);
                }
            }
        }

        private static bool GenerateControls(EnableList enableList, IDictionary<string, string> list,
                                     EnableControls enableControls)
        {
            bool res;
            switch (enableList.ListMode)
            {
                case EnableListMode.And:
                    res = true;
                    break;
                case EnableListMode.Or:
                    res = false;
                    break;
                default:
                    res = true;
                    break;
            }
            foreach (IEnableControl control in enableList.Items)
            {
                control.EnableControls = enableControls;
                control.RefreshValues();
                var innerList = control as EnableList;
                if (innerList != null)
                {
                    bool value = GenerateControls(innerList, list, enableControls);

                    switch (enableList.ListMode)
                    {
                        case EnableListMode.And:
                            res &= value;
                            break;
                        case EnableListMode.Or:
                            res |= value;
                            break;
                    }
                }
                else if (control.ControlID != null || control.TypeComponent == TypeComponent.Disable)
                {
                    if (control.TypeComponent != TypeComponent.Disable && !list.ContainsKey(control.ControlID))
                        list.Add(control.ControlID, control.GetClientID());

                    bool equal = control.IsEquals();
                    bool disable = control.Disable;
                    bool value = (equal && !disable) || (!equal && disable);
                    switch (enableList.ListMode)
                    {
                        case EnableListMode.And:
                            res &= value;
                            break;
                        case EnableListMode.Or:
                            res |= value;
                            break;
                    }
                }
                else
                    res = false;

            }
            return res;
        }
        
        /*
        private static void GenerateControls(EnableList enableList, IDictionary<string, string> list,
                                     EnableControls enableControls)
        {
            foreach (IEnableControl control in enableList.Items)
            {
                control.EnableControls = enableControls;
                control.RefreshValues();
                var innerList = control as EnableList;
                if (innerList != null)
                    GenerateControls(innerList, list, enableControls);
                else if (control.ControlID != null && !list.ContainsKey(control.ControlID))
                    list.Add(control.ControlID, control.GetClientID());
            }
        }*/

        /*

                private static void SetParentDisplayNone(Control control)
                {
                    if (control.Parent != null && IsDisplayNone(control.Parent))
                    {
                        var webControl = control.Parent as WebControl;
                        if (webControl != null) webControl.Style.Add("display", "none");
                        SetParentDisplayNone(control.Parent);
                    }
                }
                private static bool IsDisplayNone(Control control)
                {
                    IEnumerable controls;
                    var cell = control as TableCell;
                    controls = control.Controls;
        //            var table = control as Table;
        //            if (table != null)
        //                controls = table.Rows;
        //            else
        //            {
        //                var row = control as TableRow;
        //                if (row != null)
        //                    controls = row.Cells;
        //                else
        //                    controls = control.Controls;
        //            }
                    foreach (Control item in controls)
                    {
                        if (cell != null && item.Parent.Parent != control.Parent) continue;
                        var webControl = item as WebControl;
                        if (!string.IsNullOrEmpty(item.ID) && webControl != null && !"none".Equals(webControl.Style["display"]))
                            return false;
                        if ((webControl == null || !"none".Equals(webControl.Style["display"]))
                            && !IsDisplayNone(item))
                            return false;
                    }
                    return true;
                }

                private static bool GenerateControls(EnableList enableList, IDictionary<string, string> list,
                                                     EnableControls enableControls)
                {
                    bool res;
                    switch (enableList.ListMode)
                    {
                        case EnableListMode.And:
                            res = true;
                            break;
                        case EnableListMode.Or:
                            res = false;
                            break;
                        default:
                            res = true;
                            break;
                    }
                    foreach (IEnableControl control in enableList.Items)
                    {
                        control.EnableControls = enableControls;
                        control.RefreshValues();
                        var innerList = control as EnableList;
                        if (innerList != null)
                        {
                            bool value = GenerateControls(innerList, list, enableControls);

                            switch (enableList.ListMode)
                            {
                                case EnableListMode.And:
                                    res &= value;
                                    break;
                                case EnableListMode.Or:
                                    res |= value;
                                    break;
                            }
                        }
                        else if (control.Control != null)
                        {
                            if (!list.ContainsKey(control.ControlID))
                                list.Add(control.ControlID, control.GetClientID());

                            bool equal = control.IsEquals();
                            bool disable = control.Disable;
                            bool value = (equal && !disable) || (!equal && disable);
                            switch (enableList.ListMode)
                            {
                                case EnableListMode.And:
                                    res &= value;
                                    break;
                                case EnableListMode.Or:
                                    res |= value;
                                    break;
                            }
                        }
                        else
                            res = false;
                    }
                    return res;
                }
         */
    }

    [ParseChildren(true)]
    [PersistChildren(false)]
    public class EnableItem
    {
        private EnableList _list = new EnableList();
        private Control targetControl;
        private string targetControlID;
        public string targetID;
        public List<string> aditinalTargetID = new List<string>();

        public EnableItem()
        {
            EnableMode = EnableMode.Disable;
        }

        /// <summary>
        /// Целевой контрол который будет активироваться и деактивироваться от каких то условий.
        /// </summary>
        [ScriptIgnore]
        [Description("Целевой контрол который будет активироваться и деактивироваться от каких то условий.")]
        public string TargetControlID
        {
            get { return targetControlID; }
            set { targetControlID = value; }
        }

        /// <summary>
        /// Список контролов по которым определяется активность целевого контрола.
        /// </summary>
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        [PersistenceMode(PersistenceMode.InnerProperty)]
        [Description("Список контролов по которым определяется активность целевого контрола.")]
        public EnableList EnableItems
        {
            get { return _list; }
            set { _list = value; }
        }

        /// <summary>
        /// Целевой контрол который будет активироваться и деактивироваться от каких то условий.
        /// </summary>
        [ScriptIgnore]
        [Description("Целевой контрол который будет активироваться и деактивироваться от каких то условий.")]
        public Control TargetControl
        {
            get { return targetControl; }
            set
            {
                if (value != null)
                    targetControlID = value.ID;
                else
                    targetControlID = null;
                targetControl = value;
            }
        }

        [DefaultValue(EnableMode.Disable)]
        public EnableMode EnableMode { get; set; }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [ScriptIgnore]
        public bool Result { get; set; }

        public EnableItem Clone()
        {
            var clone = new EnableItem()
                            {
                                _list = (EnableList)_list.Clone(),
                                aditinalTargetID = aditinalTargetID,
                                targetControl = targetControl,
                                targetControlID = targetControlID,
                                targetID = targetID,
                                EnableMode = EnableMode
                            };
            return clone;
        }
    }

    [Flags]
    public enum EnableMode
    {
        Disable = 1,
        Hide = 2,
        ReadOnly = 4,
        ChangeValidateGroup = 8,
    }
}