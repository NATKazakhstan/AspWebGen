using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Web.UI;
using System.Web.UI.WebControls;
using Nat.Web.Tools;
using Nat.Web.Tools.Security;

namespace Nat.Web.Controls.Security
{
    [ParseChildren(true)]
    [ProvideProperty("ActionBinds", typeof(Component))]
    [PersistChildren(false)]
    public class ModuleActionBinder : Control
    {
        private readonly Dictionary<string, bool> userInRoles = new Dictionary<string, bool>(new CaseInsensitiveCultureComparer());
        private readonly List<ActionBind> actionBinds = new List<ActionBind>();
        private ModeActionBinderEnum mode = ModeActionBinderEnum.Invisible;

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        [PersistenceMode(PersistenceMode.InnerProperty)]
        [NotifyParentProperty(true)]
        public List<ActionBind> ActionBinds
        {
            get { return actionBinds; }
        }

        [DefaultValue(ModeActionBinderEnum.Invisible)]
        public ModeActionBinderEnum Mode
        {
            get { return mode; }
            set { mode = value; }
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            Page.PreRenderComplete += Page_OnPreRenderComplete;
        }

        private void Page_OnPreRenderComplete(object sender, EventArgs e)
        {
            SetNoAccess();
        }

        protected override void Render(HtmlTextWriter writer)
        {
            SetNoAccess();
            base.Render(writer);
        }

        private void SetNoAccess()
        {
            switch (mode)
            {
                case ModeActionBinderEnum.Invisible:
                    foreach (Control control in GetNoAccessControls())
                        control.Visible = false;
                    break;
                case ModeActionBinderEnum.Disable:
                    foreach (Control control in GetNoAccessControls())
                    {
                        WebControl webControl = control as WebControl;
                        if (webControl != null) webControl.Enabled = false;
                        else
                        {
                            control.Visible = false;
                            Debug.Assert(false, string.Format("Control '{0}' can't be disabled (it's not inherit WebControl)", control.ID));
                        }
                    }
                    break;
                default:
                    break;
            }
        }

        private IEnumerable GetNoAccessControls()
        {
#if LOCAL
            return new object[0];
#endif

            List<Control> denyControls = new List<Control>();
            SortedList<string, string> allowControlIDs = new SortedList<string, string>();

            foreach (ActionBind bind in actionBinds)
            {
                string[] roles = bind.Role.Split(';');
                bool isInRoles = true;
                foreach (string role in roles)
                {
                    if (!userInRoles.ContainsKey(role))
                        userInRoles.Add(role, UserRoles.IsInRole(role));
                    if (!(userInRoles[role] && !string.IsNullOrEmpty(bind.ControlID)
                        && bind.AccessMode == AccessMode.Allow && !allowControlIDs.ContainsKey(bind.ControlID)))
                        isInRoles = false;
                }
                if (!userInRoles.ContainsKey(bind.Role))
                    userInRoles.Add(bind.Role, isInRoles);
                if (isInRoles && !string.IsNullOrEmpty(bind.ControlID)
                    && bind.AccessMode == AccessMode.Allow && !allowControlIDs.ContainsKey(bind.ControlID))
                {
                    allowControlIDs.Add(bind.ControlID, bind.Role);
                }
            }
            foreach (ActionBind bind in actionBinds)
            {
                if (string.IsNullOrEmpty(bind.ControlID)) continue;
                if (userInRoles[bind.Role])
                {
                    if (bind.AccessMode == AccessMode.Deny)
                        GetControls(Page, bind.ControlID, denyControls);
                }
                else if (!allowControlIDs.ContainsKey(bind.ControlID) && bind.AccessMode == AccessMode.Allow)
                    GetControls(Page, bind.ControlID, denyControls);
            }
            return denyControls;
        }

        private static void GetControls(Control inControl, string controlID, List<Control> controls)
        {
            if (controlID.Equals(inControl.ID, StringComparison.OrdinalIgnoreCase))
                controls.Add(inControl);
            else
            {
                foreach (Control con in inControl.Controls)
                    GetControls(con, controlID, controls);
            }
        }
        
    }

    public enum ModeActionBinderEnum
    {
        Invisible,
        Disable
    }

    public enum AccessMode
    {
        Allow,
        Deny
    }

    [Serializable]
    public class ActionBind
    {
        private string role;
        private string controlID;
        private AccessMode accessMode = AccessMode.Allow;

        public ActionBind()
        {
        }

        public ActionBind(string controlID, string role)
        {
            this.controlID = controlID;
            this.role = role;
        }


        public ActionBind(string controlID, string role, AccessMode accessMode)
        {
            this.role = role;
            this.controlID = controlID;
            this.accessMode = accessMode;
        }

        [TypeConverter(typeof(ActionConverter))]
        [NotifyParentProperty(true)]
        [DefaultValue(null)]
        public string Role
        {
            get { return role; }
            set { role = value; }
        }

        [TypeConverter(typeof (ControlIDConverterForActionBind))]
        [NotifyParentProperty(true)]
        [DefaultValue(null)]
        public string ControlID
        {
            get { return controlID; }
            set { controlID = value; }
        }

        [NotifyParentProperty(true)]
        [DefaultValue(AccessMode.Allow)]
        public AccessMode AccessMode
        {
            get { return accessMode; }
            set { accessMode = value; }
        }

        public override string ToString()
        {
            return string.IsNullOrEmpty(controlID) ? base.ToString() : controlID;
        }
    }
}