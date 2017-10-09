using System.Collections.Specialized;
using System.ComponentModel;
using System.Web.UI;

namespace Nat.Web.Controls.GenerationClasses
{
    using System;
    using System.Collections.Generic;
    using System.Text;

    using Nat.Web.Controls.GenerationClasses.HtmlGeneration;

    [DefaultProperty("Value")]
    [DefaultBindingProperty("Value")]
    [ValidationPropertyAttribute("Value")]
    public class BaseLookup : BaseEditControl, ILookup, IPostBackDataHandler
    {
        public event EventHandler<GetExtenderAjaxControlEventArgs> GetExtenderAjaxControl;

        public string SelectMode { get; set; }
        public string ViewMode { get; set; }
        public string ProjectName { get; set; }
        public string TableName { get; set; }
        public string AlternativeCellWidth { get; set; }
        public BrowseFilterParameters BrowseFilterParameters { get; set; }
        public override object Value { get; set; }
        public override string Text { get; set; }
        public object AlternativeColumnValue { get; set; }
        public string AlternateText { get; set; }
        public int MinimumPrefixLength { get; set; }
        public string SelectKeyValueColumn { get; set; }
        public bool IsMultipleSelect { get; set; }
        public string OnChangedValue { get; set; }
        public SelectColumnParameters SelectInfo { get; set; }

        public ExtenderAjaxControl ExtenderAjaxControl { get; set; }

        public void OnExtenderAjaxControlGet(GetExtenderAjaxControlEventArgs e)
        {
            e.ExtenderAjaxControl = ExtenderAjaxControl;
            var handler = GetExtenderAjaxControl;
            if (handler != null) handler(this, e);
        }

        protected override void RenderContents(HtmlTextWriter writer)
        {
            HtmlGenerator.RenderLookup(writer, this, null);
        }

        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);
            var args = new GetExtenderAjaxControlEventArgs();
            OnExtenderAjaxControlGet(args);
            if (args.ExtenderAjaxControl != null)
            {
                HtmlGenerator.AddAutoCompliteForLookup(
                    args.ExtenderAjaxControl,
                    "tbT_" + ((IRenderComponent)this).ClientID,
                    MinimumPrefixLength,
                    this);
            }
        }
        
        public override void Render(HtmlTextWriter writer, ExtenderAjaxControl extenderAjaxControl)
        {
            HtmlGenerator.RenderLookup(writer, this, extenderAjaxControl);
        }

        public override void InitAjax(Control control, ExtenderAjaxControl extenderAjaxControl)
        {
        }

        public bool LoadPostData(string postDataKey, NameValueCollection postCollection)
        {
            Value = postCollection[UniqueID];
            Text = postCollection[UniqueID + "$tbT"];
            AlternateText = postCollection[UniqueID + "$tbA"];
            return true;
        }

        public void RaisePostDataChangedEvent()
        {
        }

        public static IEnumerable<string> GetLookupControlIds(string clientID)
        {
            return new[]
                {
                    "tbA_" + clientID,
                    "tbT_" + clientID,
                    "ibBrowse_" + clientID,
                    "tbValues_" + clientID,
                    "bNull_" + clientID,
                    "hlInfo_" + clientID,
                };
        }
    }
}