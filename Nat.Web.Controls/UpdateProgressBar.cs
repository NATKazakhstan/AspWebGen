/*
 * Created by : Daniil Kovalev
 * Created    : 06.12.2007
 */

using System;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Drawing.Design;
using System.Web.UI;
using System.Web.UI.WebControls;
using Nat.Web.Controls.Properties;


#region Resources

[assembly: WebResource("Nat.Web.Controls.ewr133.gif", "image/gif")]

#endregion


namespace Nat.Web.Controls
{
    public class UpdateProgressBar : CompositeControl
    {
        #region Fields

        private Image image;

        #endregion


        #region Methods

        protected override void OnInit(EventArgs e)
        {
            if(DesignMode) return;
            var popupControl = new PopupControl
                                   {
                                       ID = "popupControlID",
                                       ModalPopupBehaviorID = ModalPopupBehaviorID,
                                   };
            Controls.Add(popupControl);
            popupControl.ShowWhileUpdating = true;

            var table = new Table();
            popupControl.Controls.Add(table);
            table.Rows.Add(new TableRow());
            table.Rows[0].Cells.Add(new TableCell());
            table.Rows[0].Cells.Add(new TableCell());

            table.Rows[0].Cells[0].Text = Resources.SPleaseWait;
            table.Rows[0].Cells[0].Style["font-size"] = "40px";

            image = new Image();
            image.ID = "imageID";
            table.Rows[0].Cells[1].Controls.Add(image);
            base.OnInit(e);
        }

        protected override void OnPreRender(EventArgs e)
        {
            if(DesignMode) return;

            if(String.IsNullOrEmpty(ImageUrl))
                image.ImageUrl = Page.ClientScript.GetWebResourceUrl(GetType(), "Nat.Web.Controls.ewr133.gif");
            else
                image.ImageUrl = ImageUrl;

            base.OnPreRender(e);
        }

        protected override void Render(HtmlTextWriter writer)
        {
            if(DesignMode) return;

            base.Render(writer);
        }

        #endregion


        #region Properties

        [DefaultValue("")]
        [UrlProperty]
        [Bindable(true)]
        [SuppressMessage("Microsoft.Design", "CA1056:UriPropertiesShouldNotBeStrings", Justification = "Using string to avoid Uri complications")]
        [Editor("System.Web.UI.Design.ImageUrlEditor, System.Design, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", typeof(UITypeEditor))]
        public String ImageUrl
        {
            get { return (String)ViewState["ImageUrl"]; }
            set { ViewState["ImageUrl"] = value; }
        }

        public string ModalPopupBehaviorID { get; set; }
        #endregion
    }
}