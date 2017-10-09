/*
 * Created by : Daniil Kovalev
 * Created    : 28.11.2007
 */

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Web.UI;
using System.Web.UI.WebControls;
using AjaxControlToolkit;

#region Resources

[assembly: WebResource("Nat.Web.Controls.PopupControl.css", "text/css", PerformSubstitution = true)]
[assembly: WebResource("Nat.Web.Controls.PopupControl.js", "text/javascript")]

#endregion

namespace Nat.Web.Controls
{
    [ClientCssResource("Nat.Web.Controls.PopupControl.css")]
    [ClientScriptResource("Nat.Web.Controls.PopupControl", "Nat.Web.Controls.PopupControl.js")]
    public class PopupControl : Panel, IScriptControl, INamingContainer
    {
        #region Fields

        private Label _header;
        private Button _hiddenButton;
        private HiddenField _hiddenField;
        private ModalPopupExtender _modalPopupExtender;
        private Point _position = new Point(-1, -1);

        #endregion

        #region Methods

        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);

            _hiddenField = new HiddenField {ID = "hiddenFieldID"};
            Controls.Add(_hiddenField);

            _header = new Label {ID = "headerID"};
            Controls.AddAt(0, _header);
            _header.CssClass = "modalPopupCaption";
            _header.Width = Unit.Percentage(100);

            _hiddenButton = new Button {ID = "hiddenButtonId"};
            _hiddenButton.Style["display"] = "none";
            Controls.Add(_hiddenButton);

            _modalPopupExtender = new ModalPopupExtender
                                      {
                                          ID = "modalPopupExtenderID",
                                          BackgroundCssClass = "modalBackground",
                                          OnOkScript = OnOkScript,
                                          OnCancelScript = OnCancelScript,
                                          OkControlID = OkControlID,
                                          CancelControlID = CancelControlID,
                                      };
            if (ModalPopupBehaviorID == null)
                ModalPopupBehaviorID = ClientID + "_mpb";
            _modalPopupExtender.BehaviorID = ModalPopupBehaviorID;
            _modalPopupExtender.PopupControlID = ID;
            _modalPopupExtender.TargetControlID = _hiddenButton.ID;
            _modalPopupExtender.RepositionMode = ModalPopupRepositionMode.None;
            //_modalPopupExtender.PopupDragHandleControlID = _header.ID;
            Controls.Add(_modalPopupExtender);
            _modalPopupExtender.X = _position.X;
            _modalPopupExtender.Y = _position.Y;

            Style["display"] = "none";
            CssClass = "modalPopup";
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            _header.Text = HeaderText;
        }

        protected override void OnPreRender(EventArgs e)
        {
            if (!DesignMode)
            {
                ScriptObjectBuilder.RegisterCssReferences(this);
                ScriptManager.GetCurrent(Page).RegisterScriptControl(this);
            }
            base.OnPreRender(e);
        }

        protected override void Render(HtmlTextWriter writer)
        {
            if (String.IsNullOrEmpty(HeaderText))
                _header.Style["display"] = "none";

            if (!DesignMode)
                ScriptManager.GetCurrent(Page).RegisterScriptDescriptors(this);
            base.Render(writer);
        }

        public void Show()
        {
            _modalPopupExtender.Show();
            _hiddenField.Value = "on";
        }

        public void Hide()
        {
            _modalPopupExtender.Hide();
            _hiddenField.Value = "";
        }

        #endregion

        #region Properties

        public string ModalPopupBehaviorID { get; set; }

        [Browsable(true)]
        public String HeaderText
        {
            get { return (String) (ViewState["HeaderText"] ?? ""); }
            set { ViewState["HeaderText"] = value; }
        }

        [DefaultValue(false)]
        public Boolean ShowWhileUpdating
        {
            get { return !String.IsNullOrEmpty(Attributes["showWhileUpdating"]); }
            set { Attributes["showWhileUpdating"] = value ? "on" : null; }
        }

        [DefaultValue(false)]
        public Boolean AlwaysShow
        {
            get { return !String.IsNullOrEmpty(Attributes["alwaysShow"]); }
            set { Attributes["alwaysShow"] = value ? "on" : null; }
        }

        [DefaultValue("")]
        public Point Position
        {
            get
            {
                if (_modalPopupExtender != null)
                    return new Point(_modalPopupExtender.X, _modalPopupExtender.Y);
                return _position;
            }
            set
            {
                _position = value;
                if (_modalPopupExtender != null)
                {
                    _modalPopupExtender.X = value.X;
                    _modalPopupExtender.Y = value.Y;
                }
            }
        }

        [Browsable(false)]
        public Boolean Shown
        {
            get { return !String.IsNullOrEmpty(_hiddenField.Value); }
        }


        private string _onOkScript;
        public string OnOkScript
        {
            get
            {
                if (_modalPopupExtender == null)
                    return _onOkScript;
                return _modalPopupExtender.OnOkScript;
            }
            set
            {
                if (_modalPopupExtender != null)
                    _modalPopupExtender.OnOkScript = value;
                _onOkScript = value;
            }
        }

        private string _onCancelScript;
        public string OnCancelScript
        {
            get
            {
                if (_modalPopupExtender == null)
                    return _onCancelScript;
                return _modalPopupExtender.OnCancelScript;
            }
            set
            {
                if (_modalPopupExtender != null)
                    _modalPopupExtender.OnCancelScript = value;
                _onCancelScript = value;
            }
        }
        
        private string _okControlID;
        public string OkControlID
        {
            get
            {
                if (_modalPopupExtender == null)
                    return _okControlID;
                return _modalPopupExtender.OkControlID;
            }
            set
            {
                if (_modalPopupExtender != null)
                    _modalPopupExtender.OkControlID = value;
                _okControlID = value;
            }
        }

        private string _cancelControlID;
        public string CancelControlID
        {
            get
            {
                if (_modalPopupExtender == null)
                    return _cancelControlID;
                return _modalPopupExtender.CancelControlID;
            }
            set
            {
                if (_modalPopupExtender != null)
                    _modalPopupExtender.CancelControlID = value;
                _cancelControlID = value;
            }
        }

        #endregion

        #region IScriptControl Members

        public IEnumerable<ScriptDescriptor> GetScriptDescriptors()
        {
            if (Page != null && Visible)
            {
                var desc = new ScriptControlDescriptor("Nat.Web.Controls.PopupControl", ClientID);

                desc.AddProperty("modalPopupBehaviorID", _modalPopupExtender.BehaviorID);
                desc.AddProperty("hiddenFieldID", _hiddenField.ClientID);
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
    }
}