/*
 * Created by: Sergey V. Shpakovskiy
 * Created: 10 апреля 2009 г.
 * Copyright © JSC New Age Technologies 2009
 */

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Web.UI;
using System.Web.UI.WebControls;
using AjaxControlToolkit;
using Nat.Tools.Classes;
using TripletItem = Nat.Tools.Classes.Triplet<AjaxControlToolkit.ExtenderControlBase, System.Collections.Generic.IEnumerable<string>, System.Web.UI.Control>;

namespace Nat.Web.Controls.GenerationClasses
{
    public class ExtenderAjaxControl : Control, IScriptControl
    {
        private readonly IList<TripletItem> _extenderControls = new List<TripletItem>();
        private readonly Dictionary<string, bool> registeredControls = new Dictionary<string, bool>();
        private bool _loadedClientStateValues;

        public event EventHandler<ClientArgs> RetriveClientID;

        public void AddExtender(ExtenderControlBase control, string clientID, Control dependedControl)
        {
            if (registeredControls.ContainsKey(control.BehaviorID)) return;
            _extenderControls.Add(clientID == null
                                      ? new TripletItem(control, null, dependedControl)
                                      : new TripletItem(control, new[] {clientID}, dependedControl));
            registeredControls[control.BehaviorID] = true;
            if (_loadedClientStateValues)
                LoadClientStateValues(control);
        }

        public void AddExtender(ExtenderControlBase control, IEnumerable<string> clientIDs, Control dependedControl)
        {
            if (registeredControls.ContainsKey(control.BehaviorID)) return;
            _extenderControls.Add(new TripletItem(control, clientIDs, dependedControl));
            registeredControls[control.BehaviorID] = true;
            if (_loadedClientStateValues)
                LoadClientStateValues(control);
        }

        public void AddExtender(ExtenderControlBase control, string clientID)
        {
            AddExtender(control, clientID, null);
        }

        public void AddExtender(ExtenderControlBase control, IEnumerable<string> clientIDs)
        {
            AddExtender(control, clientIDs, null);
        }

        public void AddExtender(ExtenderControlBase control)
        {
            AddExtender(control, (IEnumerable<string>)null, null);
        }

        public IEnumerable<ScriptDescriptor> GetScriptDescriptors()
        {
            var control = new ClientControl();
            var res = new List<ScriptDescriptor>();
            foreach (var extenderControl in _extenderControls)
            {
                if (extenderControl.Third != null && (!extenderControl.Third.Visible || IsExcluded(extenderControl.Third)))
                    continue;

                if (extenderControl.Second == null)
                {
                    /*if (!string.IsNullOrEmpty(extenderControl.First.TargetControlID))
                    {
                        var enumerable = ((IExtenderControl)extenderControl.First).GetScriptDescriptors(control);
                        if (enumerable != null) res.AddRange(enumerable);
                        continue;
                    }*/
                    var clientArgs = new ClientArgs();
                    OnRetriveClientID(clientArgs);
                    extenderControl.Second = clientArgs.ClientIDs;
                }
                var id = extenderControl.First.ID;
                extenderControl.First.Page = Page;
                extenderControl.First.SetTargetControl(this);
                foreach (var clientID in extenderControl.Second)
                {
                    control.SetClientID(clientID);
                    extenderControl.First.ID = id + clientID;
                    var enumerable = ((IExtenderControl) extenderControl.First).GetScriptDescriptors(control);
                    if (enumerable != null) res.AddRange(enumerable);
                }
            }
            return res;
        }

        private bool IsExcluded(Control control)
        {
            if (control.Parent == null || control.Page == null) return true;
            if (control.Parent != null && control.Parent == control.Page) return false;
            return IsExcluded(control.Parent);
        }

        protected virtual void OnRetriveClientID(ClientArgs args)
        {
            if (RetriveClientID != null) RetriveClientID(this, args);
        }

        public IEnumerable<ScriptReference> GetScriptReferences()
        {
            var res = new List<ScriptReference>();
            foreach (var extenderControl in _extenderControls)
            {
                extenderControl.First.Page = Page;
                var enumerable = ((IExtenderControl)extenderControl.First).GetScriptReferences();
                if(enumerable != null) res.AddRange(enumerable);
            }
            return res;
        }

        protected override void OnInit(EventArgs e)
        {
            foreach (var extenderControl in _extenderControls)
            {
                if (extenderControl.First.EnableClientState)
                    CreateClientStateField(extenderControl.First);
            }
            Page.PreLoad += Page_PreLoad;
            base.OnInit(e);
        }

        private HiddenField CreateClientStateField(ExtenderControlBase extender)
        {
            // add a hidden field so we'll pick up the value
            //
            HiddenField field = new HiddenField();
            field.ID = GetClientStateFieldID(extender);
            Controls.Add(field);
            extender.ClientStateFieldID = field.ClientID;
            return field;
        }

        private string GetClientStateFieldID(ExtenderControlBase extender)
        {
            return string.Format(CultureInfo.InvariantCulture, "{0}_ClientState", extender.ID);
        }

        private void Page_PreLoad(object sender, EventArgs e)
        {
            LoadClientStateValues();            
        }

        protected override void OnLoad(EventArgs e)
        {
            OnLoad();
            base.OnLoad(e);
        }

        private void LoadClientStateValues()
        {
            foreach (var extenderPair in _extenderControls)
                LoadClientStateValues(extenderPair.First);
            _loadedClientStateValues = true;
        }

        private void LoadClientStateValues(ExtenderControlBase extenderControl)
        {
            if (extenderControl.EnableClientState)
            {
                var hiddenField = (HiddenField)NamingContainer.FindControl(GetClientStateFieldID(extenderControl));
                if (hiddenField != null)
                {
                    if (string.IsNullOrEmpty(hiddenField.Value) && !string.IsNullOrEmpty(Page.Request.Params[hiddenField.UniqueID]))
                        hiddenField.Value = Page.Request.Params[hiddenField.UniqueID];
                    extenderControl.ClientState = hiddenField.Value;
                }
                else
                {
                    var field = CreateClientStateField(extenderControl);
                    field.Value = Page.Request.Params[field.UniqueID];
                    extenderControl.ClientState = field.Value;
                }
            }
        }

        protected override void OnPreRender(EventArgs e)
        {
            OnPreRender();
            base.OnPreRender(e);
        }

        private void SaveClientStateValues(ExtenderControlBase extender)
        {
            if (extender.EnableClientState)
            {
                HiddenField hiddenField = null;

                if (string.IsNullOrEmpty(extender.ClientStateFieldID))
                    hiddenField = CreateClientStateField(extender);
                else
                    hiddenField = (HiddenField)NamingContainer.FindControl(GetClientStateFieldID(extender));

                if (hiddenField != null)
                    hiddenField.Value = extender.ClientState;
            }
        }

        protected override void Render(HtmlTextWriter writer)
        {
            Render();
            base.Render(writer);
        }

        public void RegisterScriptDescriptors()
        {
            ScriptManager.GetCurrent(Page).RegisterScriptDescriptors(this);
        }

        private class ClientControl : Control
        {
            private string _clientID;
            public override string ClientID
            {
                get
                {
                    return _clientID;
                }
            }
            public void SetClientID(string value)
            {
                _clientID = value;
            }
        }

        public class ClientArgs : EventArgs
        {
            public ClientArgs()
            {
                ClientIDs = new List<string>();
            }
            public ExtenderControl ExtenderControl { get; set; }
            public IList<string> ClientIDs { get; private set; }
        }

        internal void OnLoad()
        {
            if (!_loadedClientStateValues)
                LoadClientStateValues();
            foreach (var extenderControl in _extenderControls)
            {
                extenderControl.First.Page = Page;
                ScriptObjectBuilder.RegisterCssReferences(extenderControl.First);
            }
        }

        private bool _OnPreRenderExecuted;
        public void OnPreRender()
        {
            if (_OnPreRenderExecuted) return;
            _OnPreRenderExecuted = true;

            if (!DesignMode)
                ScriptManager.GetCurrent(Page).RegisterScriptControl(this);
            foreach (var extenderPair in _extenderControls)
            {
                var extenderControl = extenderPair.First;
                if (extenderControl.Enabled)
                    SaveClientStateValues(extenderControl);
            }
        }

        internal void Render()
        {
            if (!DesignMode) 
                ScriptManager.GetCurrent(Page).RegisterScriptDescriptors(this);            
        }
    }
}