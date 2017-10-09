/*
 * Created by : Daniil Kovalev
 * Created    : 04.12.2007
 */

using System;
using System.Collections.Generic;
using System.Data;
using System.Configuration;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Web.UI.HtmlControls;

namespace Nat.Web.Controls
{
    public class ToolBar : CompositeControl
    {
        #region Methods

        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);
            //if (DesignMode)
            foreach(Control item in Items)
            {
                Controls.Add(item);
            }
        }

        #endregion


        #region Properties

        public ICollection<Control> Items
        {
            get
            {
                if (ViewState["Items"] == null) 
                    ViewState["Items"] = new List<Control>();
                return (ICollection<Control>)ViewState["Items"];
            }

            set { ViewState["Items"] = value; }
        }


        #endregion

    }
}
