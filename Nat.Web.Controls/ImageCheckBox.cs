/*
 * Created by: Sergey V. Shpakovskiy
 * Created: 1 èþëÿ 2008 ã.
 * Copyright © JSC New Age Technologies 2008
 */

using System;
using System.ComponentModel;
using System.Web;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using Nat.Web.Tools;

namespace Nat.Web.Controls
{
    [Localizable(true)]
    public class ImageCheckBox : WebControl
    {
        private ImageButton button;
        private HiddenField hiddenField;
        private string hideIDControl;
        private string imageUrlChecked;
        private string imageUrlUnchecked;
        private string text;
        private string toolTipChecked;
        private string toolTipUnchecked;
        private bool autoPostBack;
        private Control owner;

        public event EventHandler CheckedChanged;

        [Localizable(false)]
        public string ImageUrlChecked
        {
            get { return imageUrlChecked; }
            set { imageUrlChecked = value; }
        }

        [Localizable(false)]
        public string ImageUrlUnchecked
        {
            get { return imageUrlUnchecked; }
            set { imageUrlUnchecked = value; }
        }

        [Localizable(true)]
        public string ToolTipChecked
        {
            get { return toolTipChecked; }
            set { toolTipChecked = value; }
        }

        [Localizable(true)]
        public string ToolTipUnchecked
        {
            get { return toolTipUnchecked; }
            set { toolTipUnchecked = value; }
        }

        [Localizable(false)]
        public string HideIDControl
        {
            get { return hideIDControl; }
            set { hideIDControl = value; }
        }

        [Localizable(false)]
        public bool Checked
        {
            get
            {
                EnsureChildControls();
                return hiddenField.Value == "1";
            }
            set
            {
                EnsureChildControls();
                hiddenField.Value = value ? "1" : "0";
            }
        }

        [Browsable(false)]
        [Localizable(false)]
        public Control Owner
        {
            get { return owner; }
            set { owner = value; }
        }

        [Localizable(true)]
        public string Text
        {
            get { return text; }
            set { text = value; }
        }

        [DefaultValue(false)]
        [Localizable(false)]
        public bool AutoPostBack
        {
            get { return autoPostBack; }
            set { autoPostBack = value; }
        }

        protected override void CreateChildControls()
        {
            button = new ImageButton();
            Controls.Add(button);
            hiddenField = new HiddenField();
            hiddenField.ValueChanged += HiddenField_OnValueChanged;
            Controls.Add(hiddenField);
            ChildControlsCreated = true;
        }

        private void HiddenField_OnValueChanged(object sender, EventArgs e)
        {
//            Checked = hiddenField.Value == "1";
            OnCheckedChanged(EventArgs.Empty);
        }

//        private ImageButton GetButton()
//        {
//            EnsureChildControls();
//            return button;
//        }
//
//        private HiddenField GetHiddenField()
//        {
//            EnsureChildControls();
//            return hiddenField;
//        }

        protected override void RenderChildren(HtmlTextWriter writer)
        {
            base.RenderChildren(writer);
            writer.Write("&nbsp;");
            HtmlGenericControl t = new HtmlGenericControl("span");
            t.InnerText = Text;
            t.Style.Add("vertical-align", "top");
            t.RenderControl(writer);
        }

        protected override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);
            Page.PreRenderComplete += Page_OnPreRenderComplete;
        }

        private void Page_OnPreRenderComplete(object sender, EventArgs e)
        {
            WebControl control;
            if (owner == null)
                control = ControlHelper.FindControlRecursive(Page, HideIDControl) as WebControl;
            else
                control = ControlHelper.FindControlRecursive(owner, HideIDControl) as WebControl;
            if (control == null)
            {
                Visible = false;
                return;
            }
            if (autoPostBack)
            {
                button.OnClientClick =
                    @"
$this = $(this);
if (this.parentNode.children[1].value == '1'){ 
    this.src = $this.attr('imageUrlUnchecked');
    this.parentNode.children[1].value = '0';
    this.parentNode.title = $this.attr('toolTipUnchecked');
}
else {
    this.src = $this.attr('imageUrlChecked');
    this.parentNode.children[1].value = '1';
    this.parentNode.title = $this.attr('toolTipChecked');
}

return true;
";
            }
            else
            {
                button.OnClientClick =
                    @"
var target = $get('" + control.ClientID +
                    @"');
if (!target) return false;
$this = $(this);
if (this.parentNode.children[1].value == '1'){ 
    this.src = $this.attr('imageUrlUnchecked');
    this.parentNode.children[1].value = '0';
    this.parentNode.title = $this.attr('toolTipUnchecked');
    target.style.display = 'none';
    target.width = '1px';
}
else {
    this.src = $this.attr('imageUrlChecked');
    this.parentNode.children[1].value = '1';
    this.parentNode.title = $this.attr('toolTipChecked');
    target.style.display = '';
    target.width = $this.attr('widthControl');
}

return false;
";
            }
            if (!string.IsNullOrEmpty(ImageUrlUnchecked))
                button.Attributes["imageUrlUnchecked"] = VirtualPathUtility.ToAbsolute(ImageUrlUnchecked);
            if (!string.IsNullOrEmpty(ImageUrlChecked))
                button.Attributes["imageUrlChecked"] = VirtualPathUtility.ToAbsolute(ImageUrlChecked);
            button.Attributes["widthControl"] = control.Width.ToString();
            button.Attributes["toolTipChecked"] = ToolTipChecked;
            button.Attributes["toolTipUnchecked"] = ToolTipUnchecked;
            button.ImageUrl = Checked ? ImageUrlChecked : ImageUrlUnchecked;
            ToolTip = Checked ? ToolTipChecked : ToolTipUnchecked;
            if (autoPostBack)
                control.Visible = Checked;
            else
                control.Style["display"] = Checked ? "" : "none";
        }

        protected void OnCheckedChanged(EventArgs args)
        {
            if(CheckedChanged != null)
                CheckedChanged(this, args);
        }
    }
}