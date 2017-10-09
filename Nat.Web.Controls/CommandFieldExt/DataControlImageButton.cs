/*
 * Created by: Arman K. Karibaev
 * Created: 30.10.2007
 * Copyright © JSC NAT Kazakhstan 2007
 */

using System;
using System.Web.UI;
using System.Web.UI.WebControls;
using AjaxControlToolkit;

[assembly: WebResource("Nat.Web.Controls.CommandFieldExt.Styles.ButtonStyle.css", "text/css", PerformSubstitution = true)]

namespace Nat.Web.Controls.CommandFieldExt
{
    [ClientCssResource("Nat.Web.Controls.CommandFieldExt.Styles.ButtonStyle.css")]
    public class DataControlImageButton : ImageButton, IDataControlButton
    {
        private readonly IPostBackContainer _container;
        private string _callbackArgument;
        private bool _enableCallback;

        public DataControlImageButton(IPostBackContainer container)
        {
            _container = container;
        }

        #region IDataControlButton Members

        public override bool CausesValidation
        {
            get
            {
                if (_container != null)
                    return false;
                return base.CausesValidation;
            }
            set
            {
                if (_container != null)
                    throw new NotSupportedException("CannotSetValidationOnDataControlButtons");
                base.CausesValidation = value;
            }
        }

        #endregion

        public void EnableCallback(string argument)
        {
            _enableCallback = true;
            _callbackArgument = argument;
        }


        protected override PostBackOptions GetPostBackOptions()
        {
            if (_container != null)
                return _container.GetPostBackOptions(this);
            return base.GetPostBackOptions();
        }


        protected override void Render(HtmlTextWriter writer)
        {
            SetCallbackProperties();
            base.Render(writer);
        }


        protected override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);
            ScriptObjectBuilder.RegisterCssReferences(this);
        }

        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);
            CssClass = "image";
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
                        OnClientClick = callbackScript;
                }
            }
        }
    }
}