using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Xml.Linq;
using System.Xml.Serialization;
using Nat.Web.Controls.GenerationClasses;
using Nat.Web.Controls.GenerationClasses.Filter;
using Nat.Web.Tools;
using Nat.Web.Controls.Properties;

namespace Nat.Web.Controls.GenerationClasses
{
    public class FieldEditFilter : TextBox, IFieldControl<XElement>
    {
        public const string SetFilters = "SetFilters:";
        public const string ClearFilter = "ClearFilter";

        private ScriptManager _ScriptManager;
        private bool? _HasAjax;
        private XElement _gValue;

        public ScriptManager ScriptManager
        {
            get
            {
                if (_ScriptManager == null)
                    _ScriptManager = ScriptManager.GetCurrent(Page);
                return _ScriptManager;
            }
        }

        public bool HasAjax
        {
            get
            {
                if (_HasAjax == null)
                {
                    var updatePanel = ControlHelper.FindControl<UpdatePanel>(this);
                    _HasAjax = ScriptManager != null && updatePanel != null;
                }
                return _HasAjax.Value;
            }
        }

        /*
        #region IFilterSupport Members

        [DefaultValue("")]
        [IDReferenceProperty]
        [TypeConverter(typeof(FilterConverter))]
        [Themeable(false)]
        public string FilterControl { get; set; }

        public string GetDefaultFilterControl()
        {
            return "View_PPS_IncidentsFilter";
        }

        #endregion
        */

        public string GetTextValue()
        {
            return null;
        }

        public event EventHandler<BrowseFilterParameters> GetFilterParameters;

        private void OnGetFilterParameters(BrowseFilterParameters e)
        {
            var parameters = GetFilterParameters;
            if (parameters != null) parameters(this, e);
        }

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

        [Bindable(BindableSupport.Default, BindingDirection.TwoWay)]
        public XElement GValue
        {
            get
            {
                //if (!Enabled) return null;
                if (_gValue == null)
                {
                    var filterItems = MainPageUrlBuilder.GetFilterItemsByFilterContent(Text);
                    var ser = new XmlSerializer(typeof(List<FilterItem>));
                    var doc = new XDocument();
                    using (var stream = doc.CreateWriter())
                        ser.Serialize(stream, filterItems);
                    _gValue = doc.Root;
                }
                return _gValue;
            }
            set
            {
                _gValue = value;
                Text = "";
                if (value == null) return;
                var ser = new XmlSerializer(typeof(List<FilterItem>));
                List<FilterItem> filterItems;
                using (var stream = value.CreateReader())
                    filterItems = (List<FilterItem>) ser.Deserialize(stream);
                Text = MainPageUrlBuilder.GetFilterContentByFilterItems(filterItems);
            }
        }

        [Bindable(BindableSupport.Yes, BindingDirection.TwoWay)]
        public object Value
        {
            get { return GValue; }
            set { GValue = (XElement)value; }
        }

        protected override void Render(HtmlTextWriter writer)
        {            
            writer.RenderBeginTag(HtmlTextWriterTag.Div);
            //Attributes["fID"] = ClientID + "_filter";
            Style.Add("display", "none");
            Style.Add("visibility", "hidden");
            Style.Add("width", "0px");
            Attributes["fClearID"] = ClientID + "_filterClear";
            base.Render(writer);
            RenderLinkFilter(writer, Themes.IconUrlFilter, "OpenFilterAsLookup(this);", ClientID + "_filter", false);
            RenderLinkFilter(writer, Themes.IconUrlFilterCancel, "OpenFilterAsLookupClear(this);", ClientID + "_filterClear", string.IsNullOrEmpty(Text));
            
            #region browse values

            writer.AddStyleAttribute(HtmlTextWriterStyle.Display, "none");
            writer.AddAttribute(HtmlTextWriterAttribute.Id, ClientID + "_values");
            writer.AddAttribute("isIgnoreVisible", "true");
            writer.RenderBeginTag(HtmlTextWriterTag.Span);
            
            var browseFilterParameters = new BrowseFilterParameters();
            OnGetFilterParameters(browseFilterParameters);
            writer.Write(browseFilterParameters.GetClientParameters());
            writer.RenderEndTag();
            
            #endregion

            writer.RenderEndTag();
        }

        private void RenderLinkFilter(HtmlTextWriter writer, string src, string javascript, string id, bool setDisplayNone)
        {
            writer.AddAttribute("valueID", ClientID);
            writer.AddAttribute("valuesID", ClientID + "_values");
            writer.AddAttribute(HtmlTextWriterAttribute.Id, id);
            writer.AddAttribute("isIgnoreVisible", "true");
            writer.AddAttribute(HtmlTextWriterAttribute.Title, Resources.SFilter);
            writer.AddAttribute(HtmlTextWriterAttribute.Href, "javascript:void(0);");
            writer.AddAttribute(HtmlTextWriterAttribute.Onclick, javascript);
            if (setDisplayNone)
                writer.AddStyleAttribute(HtmlTextWriterStyle.Display, "none");
            writer.RenderBeginTag(HtmlTextWriterTag.A);
            writer.AddAttribute(HtmlTextWriterAttribute.Src, src);
            writer.AddAttribute(HtmlTextWriterAttribute.Border, "0");
            writer.AddAttribute(HtmlTextWriterAttribute.Alt, Resources.SFilter);
            writer.RenderBeginTag(HtmlTextWriterTag.Img);
            writer.RenderEndTag();
            writer.RenderEndTag();
        }
    }
}
