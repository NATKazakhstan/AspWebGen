using System;
using System.Web.UI;
using System.Web.UI.WebControls;
using AjaxControlToolkit;

[assembly: WebResource("Nat.Web.Controls.CommandFieldExt.Styles.ButtonStyle.css", "text/css", PerformSubstitution = true)]
[assembly: WebResource("Nat.Web.Controls.CommandFieldExt.Images.buttom_bg.gif", "image/gif")]

namespace Nat.Web.Controls.CommandFieldExt
{
    [ClientCssResource("Nat.Web.Controls.CommandFieldExt.Styles.ButtonStyle.css")]
    public class DataControlLinkButton : LinkButton, IDataControlButton
    {
        private readonly IPostBackContainer _container;
        private string _callbackArgument;
        private bool _enableCallback;

        public DataControlLinkButton(IPostBackContainer container)
        {
            _container = container;
        }

        public override bool CausesValidation
        {
            get
            {
                if (_container != null)
                {
                    return false;
                }
                return base.CausesValidation;
            }
            set
            {
                if (_container != null)
                {
                    throw new NotSupportedException("CannotSetValidationOnDataControlButtons");
                }
                base.CausesValidation = value;
            }
        }

        public void EnableCallback(string argument)
        {
            _enableCallback = true;
            _callbackArgument = argument;
        }

        protected override PostBackOptions GetPostBackOptions()
        {
            if (_container != null)
            {
                return _container.GetPostBackOptions(this);
            }
            return base.GetPostBackOptions();
        }

        protected override void Render(HtmlTextWriter writer)
        {
            SetCallbackProperties();
            base.Render(writer);
        }

        private void SetCallbackProperties()
        {
            if (_enableCallback)
            {
                ICallbackContainer container = _container as ICallbackContainer;
                if (container != null)
                {
                    string callbackScript = container.GetCallbackScript(this, _callbackArgument);
                    if (!string.IsNullOrEmpty(callbackScript))
                    {
                        OnClientClick = callbackScript;
                    }
                }
            }
        }

        protected override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);
            ScriptObjectBuilder.RegisterCssReferences(this);
        }

        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);
            CssClass = "text_butt";
        }

        public override void RenderBeginTag(HtmlTextWriter writer)
        {
            writer.WriteBeginTag(HtmlTextWriterTag.Span.ToString());
            writer.WriteAttribute(HtmlTextWriterAttribute.Class.ToString(), "main");
            writer.WriteAttribute(HtmlTextWriterAttribute.Style.ToString(),
                                  string.Format("{0}: url({1}) repeat-x", HtmlTextWriterAttribute.Background,
                                                Page.ClientScript.GetWebResourceUrl(GetType(),
                                                                                    "Nat.Web.Controls.CommandFieldExt.Images.buttom_bg.gif")));
            writer.Write(HtmlTextWriter.TagRightChar);
            base.RenderBeginTag(writer);
        }

        public override void RenderEndTag(HtmlTextWriter writer)
        {
            base.RenderEndTag(writer);
            writer.WriteEndTag(HtmlTextWriterTag.Span.ToString());
        }
    }
}