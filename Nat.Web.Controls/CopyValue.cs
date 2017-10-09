using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Web.UI;
using System.Web.UI.WebControls;
using AjaxControlToolkit;
using Nat.Web.Controls.DataBinding.Tools;

[assembly: WebResource("Nat.Web.Controls.CopyValue.js", "text/javascript")]
namespace Nat.Web.Controls
{
    [ClientScriptResource("Nat.Web.Controls.CopyValue", "Nat.Web.Controls.CopyValue.js")]
    public class CopyValue : WebControl, IScriptControl
    {
        private string _controlOne;
        private string _controlTwo;
        private bool _changeValueIfNull = true;

        public string ControlOne
        {
            get
            {
                if (_controlOne == null)
                    throw new InvalidOperationException("ControlOne is null");

                return _controlOne;
            }
            set
            {
                _controlOne = value;
            }
        }

        public string ControlTwo
        {
            get
            {
                if (_controlTwo == null)
                    throw new InvalidOperationException("ControlTwo is null");

                return _controlTwo;
            }
            set
            {
                _controlTwo = value;
            }
        }

        [DefaultValue(true)]
        [Description("Изменять значение второго элемента только, если его значение пусто")]
        public bool ChangeValueIfNull
        {
            get { return _changeValueIfNull; }
            set { _changeValueIfNull = value; }
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
                                      new object[] { ID }));
                }
                return sm;
            }
        }

        protected override void Render(HtmlTextWriter writer)
        {
            if (!DesignMode)
                CurrentScriptManager.RegisterScriptDescriptors(this);

            base.Render(writer);
        }

        protected override void OnPreRender(EventArgs e)
        {
            if (!DesignMode)
                CurrentScriptManager.RegisterScriptControl(this);

            base.OnPreRender(e);
        }

        public IEnumerable<ScriptDescriptor> GetScriptDescriptors()
        {
            if (Page != null && Visible)
            {
                Control control = Nat.Web.Controls.DataBinding.Tools.WebUtils.FindControlRecursive(Page, ControlOne);
                if (control != null)
                {
                    string rusId = control.ClientID;
                    control = WebUtils.FindControlRecursive(Page, ControlTwo);
                    if (control != null)
                    {
                        string kzId = control.ClientID;

                        ScriptBehaviorDescriptor desc =
                            new ScriptBehaviorDescriptor("Nat.Web.Controls.CopyValue", rusId);

                        desc.ID = String.Format("CopyValue_{0}", ID);
                        desc.AddProperty("kzId", kzId);
                        desc.AddProperty("changeValueIfNull", _changeValueIfNull);

                        yield return desc;
                    }
                }
            }
        }

        public IEnumerable<ScriptReference> GetScriptReferences()
        {
            var references = new List<ScriptReference>();
            references.AddRange(ScriptObjectBuilder.GetScriptReferences(GetType()));
            return references;
        }
    }
}
