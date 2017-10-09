using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Design;
using System.Web.UI;
using System.Web.UI.WebControls;
using Nat.Web.Controls.DataBinding.Resources;
using Nat.Web.Controls.DataBinding.Tools;

namespace Nat.Web.Controls.DataBinding.Extended
{
    /// <summary>
    /// A simple ErrorDisplay control that can be used to display errors consistently
    /// on Web Pages. The class includes several ways to do display data (ShowError,
    /// ShowMessage) as well as direct assignment and lists displays.
    /// </summary>
    [ToolboxBitmap(typeof(ValidationSummary))]
    [ToolboxData("<{0}:ErrorDisplay runat='server' />")]
    public class ErrorDisplay : Control
    {
        private string _CellPadding = "10";
        private bool _CenterDisplay = true;
        private string _CssClass = "errordisplay";
        private int _DisplayTimeout = 0;
        private string _ErrorImage = "WarningResource";
        private string _InfoImage = "InfoResource";
        private RenderModes _RenderMode = RenderModes.Html;
        private string _Text = "";
        private bool _UseFixedHeightWhenHiding = false;


        private string _UserMessage = "";
        private Unit _Width = Unit.Pixel(400);

        /// <summary>
        /// The detail text of the error message
        /// </summary>
        [Description("The error message to be displayed.")]
        [Category("ErrorMessage")]
        [DefaultValue("")]
        public string Text
        {
            get { return _Text; }
            set { _Text = value; }
        }

        /// <summary>
        /// The message to display above the error message.
        /// For example: Please correct the following:
        /// </summary>
        [Description("The message to display above the error strings.")]
        [Category("ErrorMessage")]
        [DefaultValue("")]
        public string UserMessage
        {
            get { return _UserMessage; }
            set { _UserMessage = value; }
        }

        /// <summary>
        /// Image displayed with the error message
        /// </summary>
        [Description("The image to display when an error occurs. Default is WarningResource which is loaded as a resource image.")]
        [Category("ErrorMessage")]
        [Editor("System.Web.UI.Design.ImageUrlEditor", typeof(UITypeEditor))]
        [DefaultValue("WarningResource")]
        public string ErrorImage
        {
            get { return _ErrorImage; }
            set { _ErrorImage = value; }
        }


        [Description("The image to display when ShowMessage is called. Default value is InfoResource which loads an image resource.")]
        [Category("ErrorMessage")]
        [Editor("System.Web.UI.Design.ImageUrlEditor", typeof(UITypeEditor))]
        [DefaultValue("InfoResource")]
        public string InfoImage
        {
            get { return _InfoImage; }
            set { _InfoImage = value; }
        }


        /// <summary>
        /// Determines whether the display box is centered
        /// </summary>
        [Category("ErrorMessage")]
        [Description("Centers the Error Display on the page.")]
        [DefaultValue(true)]
        public bool Center
        {
            get { return _CenterDisplay; }
            set { _CenterDisplay = value; }
        }

        /// <summary>
        /// Determines whether the control keeps its space padding
        /// when it is is hidden in order not to jump the display
        /// </summary>
        public bool UseFixedHeightWhenHiding
        {
            get { return _UseFixedHeightWhenHiding; }
            set { _UseFixedHeightWhenHiding = value; }
        }


        /// <summary>
        /// Determines how the error dialog renders
        /// </summary>
        [Category("ErrorMessage")]
        [Description("Determines whether the control renders text or Html")]
        [DefaultValue(RenderModes.Html)]
        public RenderModes RenderMode
        {
            get { return _RenderMode; }
            set { _RenderMode = value; }
        }

        /// <summary>
        /// The width of the ErrorDisplayBox
        /// </summary>
        [Description("The width for the control")]
        public Unit Width
        {
            get { return _Width; }
            set { _Width = value; }
        }

        /// <summary>
        /// Determines the padding inside of the error display box.
        /// </summary>
        [Description("The Cellpadding for the wrapper table that bounds the Error Display.")]
        [DefaultValue("10")]
        public string CellPadding
        {
            get { return _CellPadding; }
            set { _CellPadding = value; }
        }

        /// <summary>
        /// The CSS Class used for the table and column to display this item.
        /// </summary>
        [DefaultValue("errordisplay")]
        public string CssClass
        {
            get { return _CssClass; }
            set { _CssClass = value; }
        }


        /// <summary>
        /// A timeout in milliseconds for how long the error display is visible. 0 means no timeout.
        /// </summary>
        [Description("A timeout in milliseconds for how long the error display is visible. 0 means no timeout.")]
        [DefaultValue(0)]
        public int DisplayTimeout
        {
            get { return _DisplayTimeout; }
            set { _DisplayTimeout = value; }
        }
        
        protected override void Render(HtmlTextWriter writer)
        {
            if(Text == "" && !DesignMode)
            {
                base.Render(writer);
                return;
            }

            if(RenderMode == RenderModes.Text)
                Text = Text.Replace("\r\n", "</br>");

            //if (this.Center)
            //    writer.AddStyleAttribute(HtmlTextWriterStyle.TextAlign, "center");
            //writer.RenderBeginTag(HtmlTextWriterTag.Div);

            // *** <Center> is still the only reliable way to get block structures centered
            if(Center)
                writer.RenderBeginTag(HtmlTextWriterTag.Center);

            writer.AddAttribute(HtmlTextWriterAttribute.Id, ClientID);
            writer.AddAttribute(HtmlTextWriterAttribute.Class, CssClass);
            writer.AddAttribute(HtmlTextWriterAttribute.Cellpadding, CellPadding);
            writer.AddStyleAttribute(HtmlTextWriterStyle.Height, "30px");
            writer.AddStyleAttribute(HtmlTextWriterStyle.Width, Width.ToString());
            writer.AddStyleAttribute(HtmlTextWriterStyle.TextAlign, "left");

            writer.RenderBeginTag(HtmlTextWriterTag.Table);

            writer.RenderBeginTag(HtmlTextWriterTag.Tr);

            // *** Set up  image <td> tag
            writer.AddAttribute(HtmlTextWriterAttribute.Class, CssClass);
            writer.AddAttribute(HtmlTextWriterAttribute.Valign, "top");
            writer.AddStyleAttribute(HtmlTextWriterStyle.BorderWidth, "0px");
            writer.AddStyleAttribute(HtmlTextWriterStyle.Width, "16px");

            writer.RenderBeginTag(HtmlTextWriterTag.Td);

            if(ErrorImage != "")
            {
                string ImageUrl = ErrorImage.ToLower();
                if(ImageUrl == "warningresource")
                    ImageUrl = Page.ClientScript.GetWebResourceUrl(GetType(), ControlsResources.WARNING_ICON_RESOURCE);
                else if(ImageUrl == "inforesource")
                    ImageUrl = Page.ClientScript.GetWebResourceUrl(GetType(), ControlsResources.INFO_ICON_RESOURCE);
                else
                    ImageUrl = ResolveUrl(ErrorImage);

                writer.AddAttribute(HtmlTextWriterAttribute.Src, ImageUrl);
                writer.RenderBeginTag(HtmlTextWriterTag.Img);
                writer.RenderEndTag();
            }

            writer.RenderEndTag(); // image <td>

            // *** Render content <td> tag
            writer.AddAttribute(HtmlTextWriterAttribute.Valign, "top");
            writer.RenderBeginTag(HtmlTextWriterTag.Td);

            if(UserMessage != "")
                writer.Write("<span style='font-weight:normal'>" + UserMessage + "</span><hr />");

            writer.Write(Text);

            writer.RenderEndTag(); // Content <td>
            writer.RenderEndTag(); // <tr>
            writer.RenderEndTag(); // <table>

            if(Center)
                writer.RenderEndTag(); // </center>

            writer.WriteBreak();

            //writer.RenderEndTag();  // </div>
        }

        protected override void OnPreRender(EventArgs e)
        {
            if(DisplayTimeout > 0)
            {
                // *** Use HoverPanel.js library code
                string ScriptResource = Page.ClientScript.GetWebResourceUrl(typeof(ErrorDisplay), ControlsResources.HOVERPANEL_SCRIPT_RESOURCE);
                Page.ClientScript.RegisterClientScriptInclude("HoverPanel", ScriptResource);

                string Script =
                    @"window.setTimeout(""Fadeout('" + ClientID + @"',true,2);""," + DisplayTimeout + @");";

                //@"window.setTimeout(""document.getElementById('" + this.ClientID + @"').style.display='none';""," + this.DisplayTimeout.ToString() + @");";

//                this.Page.ClientScript.RegisterStartupScript(typeof(ErrorDisplay), "DisplayTimeout", Script, true);
                ScriptManager.RegisterStartupScript(Page, typeof(ErrorDisplay), "DisplayTimeout", Script, true);
            }
            base.OnPreRender(e);
        }

        /// <summary>
        /// Assigns an error message to the control
        /// </summary>
        /// <param name="text"></param>
        public void ShowError(string text)
        {
            ShowError(text, null);
        }

        /// <summary>
        /// Assigns an error message to the control as well as a UserMessage
        /// </summary>
        /// <param name="text"></param>
        /// <param name="Message"></param>
        public void ShowError(string text, string Message)
        {
            Text = text;

            if(Message != null)
                UserMessage = Message;
            else
                UserMessage = "";

            Visible = true;
        }

        /// <summary>
        /// Displays a simple message in the display area along with the info icon if set.
        /// </summary>
        /// <param name="Message"></param>
        public void ShowMessage(string Message)
        {
            UserMessage = "";
            ErrorImage = InfoImage;
            Text = Message;
            Visible = true;
        }
    }

    public enum RenderModes
    {
        /// <summary>
        /// Error Text is Text and needs fixing up
        /// </summary>
        Text,
        /// <summary>
        /// The text is HTML and ready to display
        /// </summary>
        Html,
        /// <summary>
        /// Text is plain text and should be rendered as a bullet list
        /// </summary>
        TextAsBulletList
    }
}