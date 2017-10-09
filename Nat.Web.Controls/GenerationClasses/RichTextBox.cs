using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.SharePoint.WebControls;
using System.Web.UI;
using System.Security.Permissions;
using System.Web.UI.WebControls;
using System.Reflection;

namespace Nat.Web.Controls.GenerationClasses
{
    [ValidationProperty("Text"), AspNetHostingPermission(SecurityAction.InheritanceDemand, Level = AspNetHostingPermissionLevel.Minimal), AspNetHostingPermission(SecurityAction.LinkDemand, Level = AspNetHostingPermissionLevel.Minimal)]
    public class RichTextBox : InputFormTextBox, IFieldControl<string>
    {
        #region IFieldControl<string> Members

        public string GValue
        {
            get
            {
                #if LOCAL
                if (string.IsNullOrEmpty(Text))
                    return "LocalText";
                #endif

                return Text;
            }
            set
            {
                Text = value;
            }
        }

        #endregion

        #region IFieldControl Members

        public object Value
        {
            get
            {
                return GValue;
            }
            set
            {
                GValue = (value ?? "").ToString();
            }
        }

        public string GetTextValue()
        {
            return GValue;
        }

        public event EventHandler<BrowseFilterParameters> GetFilterParameters;

        public string GetClientID()
        {
            return ClientID;
        }

        public void InitEnableControls(EnableItem item)
        {
        }

        public string FieldName { get; set; }
        public string ParentTableName { get; set; }
        public string ParentTableProject { get; set; }
        public string ParentTableType { get; set; }
        public string NameRefToParentTable { get; set; }

        #endregion

        public override void RenderBeginTag(HtmlTextWriter writer)
        {
            writer.AddAttribute(HtmlTextWriterAttribute.Class, CssClass);
            writer.RenderBeginTag(HtmlTextWriterTag.Div);
            base.RenderBeginTag(writer);
        }

        protected override void Render(HtmlTextWriter writer)
        {
#if !LOCAL
            base.Render(writer);
#endif
            }

        public override void RenderEndTag(HtmlTextWriter writer)
        {
            base.RenderEndTag(writer);
            writer.RenderEndTag();
        }
    }
}
