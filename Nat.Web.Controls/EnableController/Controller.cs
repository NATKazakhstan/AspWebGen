using System;
using System.Collections.Generic;
using System.Web.UI;
using Nat.Web.Controls.GenerationClasses.BaseJournal;
using System.Text;

namespace Nat.Web.Controls.EnableController
{
    public class Controller : Control
    {
        public Controller()
        {
            ActiveControls = new Dictionary<string, ControllerControlItem>();
        }

        public List<ControllerItem> Items { get; set; }
        public IEnumerable<RenderContext> RenderContext { get; set; }
        public Dictionary<string, ControllerControlItem> ActiveControls { get; set; }

        protected bool InitedValues { get; set; }
        protected bool ComputedActive { get; set; }

        public void EnsureInitValues()
        {
            if (InitedValues) return;
            InitedValues = true;
            InitValues();
        }

        public void EnsureComputeActive()
        {
            if(ComputedActive) return;
            ComputedActive = true;
            ComputeActive();
        }

        public void RequiredInitValues()
        {
            InitedValues = false;
        }

        public void RequiredComputeActive()
        {
            ComputedActive = false;
        }

        protected void InitValues()
        {
            foreach (var item in Items)
                item.InitValues(RenderContext, this);
        }

        protected void ComputeActive()
        {
            foreach (var item in Items)
                item.ComputeActive(this);
        }

        protected void ChangeActiveControls()
        {
            foreach (var item in ActiveControls.Values)
                item.ChangeActiveControls();
        }

        protected StringBuilder GetJavaScript()
        {
            var sb = new StringBuilder();
            var list = new Dictionary<string, bool>();
            var functionName = "activeControl" + Guid.NewGuid().ToString("N");
            sb.AppendLine();
            sb.AppendFormat("function {0}(){{", functionName);
            sb.AppendLine();
            foreach (var item in Items)
                item.GetJavaScript(sb, list);
            sb.AppendLine("}");
            sb.AppendFormat("{0}();", functionName);
            sb.AppendLine();
            //this._changeHandler = Function.createDelegate(this, this._change);
            sb.AppendFormat("var {0}Handler = Function.createDelegate(this, {0});", functionName);
            sb.AppendLine();
            foreach (var clientID in list.Keys)
            {
                sb.AppendFormat(
                    "var control = $get('{0}'); if(control != null) $addHandler(control, 'change', {1}Handler);",
                    clientID, functionName);
                sb.AppendLine();
            }
            return sb;
        }

        protected override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);
            EnsureInitValues();
            EnsureComputeActive();
            ChangeActiveControls();
            var sb = GetJavaScript();
            if (sb.Length > 0)
                Page.ClientScript.RegisterStartupScript(GetType(), UniqueID, sb.ToString(), true);
        }

        protected override void Render(HtmlTextWriter writer)
        {
        }
    }
}
