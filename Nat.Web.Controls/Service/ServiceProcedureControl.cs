/*
 * Created by: Sergey V. Shpakovskiy
 * Created: 15 июн€ 2009 г.
 * Copyright © JSC New Age Technologies 2009
 */

using System;
using System.Web.UI;
using System.Web.UI.WebControls;
using Nat.Web.Controls.Properties;

namespace Nat.Web.Controls.Service
{
    public class ServiceProcedureControl : WebControl
    {
        private ServiceProcedure _serviceProcedure;

        public ServiceProcedure ServiceProcedure
        {
            get
            {
                if (_serviceProcedure == null)
                {
                    _serviceProcedure = new ServiceProcedure();
                    _serviceProcedure.InitializeCauses();
                }
                return _serviceProcedure;
            }
        }

        /// <summary>
        /// ‘лаг, указывающий что текуща€ страница, это страница с сообщением о проведении технический работ, иначе считаетс€ что это рабоча€ страница.
        /// </summary>
        public bool IsServiceProcedurePage { get; set; }

        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);
            if (!IsServiceProcedurePage && (!Page.Request.Url.ToString().Contains("ServiceProcedure")) && !ServiceProcedure.MayOpenSite())
                ServiceProcedure.Redirect();
        }

        public override void RenderBeginTag(HtmlTextWriter writer)
        {
            writer.WriteBeginTag("div");
            if (!IsServiceProcedurePage)
            {
                if (ServiceProcedure.Enabled)
                    writer.WriteAttribute("class", "ServiceProcedure_Enabled");
                else
                    writer.WriteAttribute("class", "ServiceProcedure_Before");
            }
            else
                writer.WriteAttribute("class", "ServiceProcedurePage");
            writer.Write(HtmlTextWriter.TagRightChar);
        }

        public override void RenderEndTag(HtmlTextWriter writer)
        {
            writer.WriteEndTag("div");
        }

        protected override void RenderContents(HtmlTextWriter writer)
        {
            if (IsServiceProcedurePage)
            {
                var msg = ServiceProcedure.ServiceProcedureMessage;
                if (string.IsNullOrEmpty(msg))
                    msg = Resources.SServiceProcedure_Disabled;
                writer.Write(msg);
            }
            else if (ServiceProcedure.Enabled)
                writer.Write(ServiceProcedure.EnabledMessage);
            else
                writer.Write(ServiceProcedure.BeforeEnabledMessage);
        }
    }
}