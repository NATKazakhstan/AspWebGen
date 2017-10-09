using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;

namespace Nat.Web.Controls.GenerationClasses
{
    public class DublicateRender : Control
    {
        private Control _control;

        public string TargetContolID { get; set; }

        public Control Control
        {
            get
            {
                if (_control == null && !string.IsNullOrEmpty(TargetContolID))
                    _control = NamingContainer.FindControl(TargetContolID);
                return _control;
            }
            set { _control = value; }
        }

        protected override void Render(HtmlTextWriter writer)
        {
            if (Control != null) Control.RenderControl(writer);
        }
    }
}
