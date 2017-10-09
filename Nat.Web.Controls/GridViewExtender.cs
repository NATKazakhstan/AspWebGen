/*
 * Created by : Daniil Kovalev
 * Created    : 28.11.2007
 */

using System;
using System.ComponentModel;
using System.Web.Script.Serialization;
using System.Web.UI;
using System.Web.UI.WebControls;
using AjaxControlToolkit;

[assembly: WebResource("Nat.Web.Controls.GridViewBehavior.js", "text/javascript")]

namespace Nat.Web.Controls
{
    [Designer(typeof(GridViewDesigner))]
    [ClientScriptResource("Nat.Web.Controls.GridViewBehavior", "Nat.Web.Controls.GridViewBehavior.js")]
    [TargetControlType(typeof(GridView))]
    public class GridViewExtender : ExtenderControlBase
    {
        protected override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);

            if (((GridView)TargetControl).DataKeyNames.Length == 0)
                throw new Exception(String.Format("DataKeyNames collection of control '{0}'\nhas to contain at least one element", TargetControl.ClientID));
        }
        [ExtenderControlProperty]
        [ClientPropertyName("dataKeys")]
        public String DataKeys
        {
            get
            {
                JavaScriptSerializer javaScriptSerializer = new JavaScriptSerializer();
                DataKeyArray dataKeyArray = ((GridView)TargetControl).DataKeys;
                return javaScriptSerializer.Serialize(dataKeyArray);
            }
        }

        [ExtenderControlProperty]
        [ClientPropertyName("selectedRowCssClass")]
        public String SelectedRowCssClass
        {
            get { return ((GridView)TargetControl).SelectedRowStyle.CssClass; }
        }

        [ExtenderControlProperty]
        [ClientPropertyName("rowCssClass")]
        public String RowCssClass
        {
            get { return ((GridView)TargetControl).RowStyle.CssClass; }
        }

        [ExtenderControlProperty]
        [ClientPropertyName("alternatingRowCssClass")]
        public String AlternatingRowCssClass
        {
            get { return ((GridView)TargetControl).AlternatingRowStyle.CssClass; }
        }

        [ExtenderControlProperty]
        [ClientPropertyName("dataDisableRowField")]
        public String DataDisableRowField
        {
            get { return GetPropertyValue("DataDisableRowField", ""); }
            set { SetPropertyValue("DataDisableRowField", value); }
        }

        [ExtenderControlProperty]
        [ClientPropertyName("conditionValue")]
        [DefaultValue(1)]
        public Int32 ConditionValue
        {
            get { return GetPropertyValue("ConditionValue", 1); }
            set { SetPropertyValue("ConditionValue", value); }
        }
    }
}