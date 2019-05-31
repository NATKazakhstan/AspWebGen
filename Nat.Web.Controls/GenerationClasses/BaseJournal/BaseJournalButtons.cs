/*
* Created by: Sergey V. Shpakovskiy
* Created: 2012.08.31
* Copyright © JSC NAT Kazakhstan 2012
*/

namespace Nat.Web.Controls.GenerationClasses.BaseJournal
{
    using System.ComponentModel;
    using System.Web.UI;

    using Nat.Web.Controls.Properties;

    public class BaseJournalButtons : Control
    {
        public BaseJournalButtons()
        {
            ShowExportButton = true;
            ShowViewSettings = true;
            ShowConcatenateColumns = true;
            ShowSuscriptionButton = true;
            ShowSaveViewSettingsButton = true;
            ShowLoadViewSettingsButton = true;
            ShowFixedHeaderSettingsButton = true;
            ShowToReportPluginButton = true;
        }

        public BaseJournalUserControl JournalUC { get; set; }

        [DefaultValue(true)]
        public bool ShowExportButton { get; set; }
        [DefaultValue(true)]
        public bool ShowViewSettings { get; set; }
        [DefaultValue(true)]
        public bool ShowConcatenateColumns { get; set; }
        [DefaultValue(true)]
        public bool ShowSuscriptionButton { get; set; }
        [DefaultValue(true)]
        public bool ShowSaveViewSettingsButton { get; set; }
        [DefaultValue(true)]
        public bool ShowLoadViewSettingsButton { get; set; }
        [DefaultValue(true)]
        public bool ShowFixedHeaderSettingsButton { get; set; }
        [DefaultValue(true)]
        public bool ShowToReportPluginButton { get; set; }

        protected override void Render(HtmlTextWriter writer)
        {
            if (DesignMode || JournalUC == null) return;

            if (ShowViewSettings) AddViewSettingsButton(writer);
            if (ShowConcatenateColumns) AddConcatenateColumnsButton(writer);
            if (ShowFixedHeaderSettingsButton && !JournalUC.BaseJournal.DetailsRender) AddFixedHeaderSettingsButton(writer);

            if (ShowLoadViewSettingsButton) AddLoadViewSettingsButton(writer);
            if (ShowSaveViewSettingsButton) AddSaveViewSettingsButton(writer);

            if (ShowSuscriptionButton && !string.IsNullOrEmpty(JournalUC.ReportPluginName)
                 && !JournalUC.BaseJournal.DetailsRender) AddSuscriptionButton(writer);
            if (ShowExportButton && !JournalUC.BaseJournal.DetailsRender) AddExportButton(writer);
            if (ShowToReportPluginButton) AddToReportPluginButton(writer);
        }

        private void AddExportButton(HtmlTextWriter writer)
        {
            var postBack = Page.ClientScript.GetPostBackEventReference(JournalUC.BaseJournal, BaseJournalControl.ExportExcel);
            var url = "javascript:useUPBarOnPostBack=false;" + postBack;
            AddButton(writer, url, string.Empty, Resources.SExport, Themes.IconUrlExport);
        }

        private void AddViewSettingsButton(HtmlTextWriter writer)
        {
            var script = JournalUC.BaseJournal.SelectingColumnControl.GetOpenViewSettingsScript();
            AddButton(writer, "javascript:void(0);", script, Resources.SViewSettings, Themes.IconUrlViewSettings);
        }

        private void AddConcatenateColumnsButton(HtmlTextWriter writer)
        {
            var script = JournalUC.BaseJournal.SelectingColumnControl.GetOpenCreateConcatenateColumnScript();
            AddButton(writer, "javascript:void(0);", script, Resources.SConcatenateColumns, Themes.IconUrlConcatenateColumns);
        }
        
        private void AddFixedHeaderSettingsButton(HtmlTextWriter writer)
        {
            var script = JournalUC.BaseJournal.SelectingColumnControl.GetOpenFixedHeaderScript();
            AddButton(
                writer,
                "javascript:void(0);",
                script,
                Resources.SViewSettingsFixedHeader,
                Themes.IconUrlViewSettingsFixedHeader);
        }

        private void AddSuscriptionButton(HtmlTextWriter writer)
        {
            var postBack = Page.ClientScript.GetPostBackClientHyperlink(JournalUC.BaseJournal, BaseJournalControl.Subscription);
            AddButton(writer, postBack, string.Empty, Resources.SSubscription, Themes.IconUrlSubscription);
        }

        private void AddSaveViewSettingsButton(HtmlTextWriter writer)
        {
            var script = JournalUC.BaseJournal.SavingJournalSettingsControl.GetOpenSaveViewSettingsScript();
            AddButton(writer, "javascript:void(0);", script, Resources.SSaveViewSettings, Themes.IconUrlSaveSettings);
        }

        private void AddLoadViewSettingsButton(HtmlTextWriter writer)
        {
            var script = JournalUC.BaseJournal.SavingJournalSettingsControl.GetOpenLoadViewSettingsScript();
            AddButton(writer, "javascript:void(0);", script, Resources.SLoadViewSettings, Themes.IconUrlLoadSettings);
        }

        private void AddToReportPluginButton(HtmlTextWriter writer)
        {
            if (string.IsNullOrEmpty(JournalUC.Url.ReportPluginName) || JournalUC.Url.CustomQueryParameters.ContainsKey("InIFrame")) return;
            writer.Write("&nbsp;");
            writer.Write("&nbsp;");
            AddButton(
                writer,
                string.Format("/ReportViewer.aspx?open=off&setDefaultParams=on&ClassName={0}", JournalUC.Url.ReportPluginName),
                string.Empty,
                Resources.SToReportPlugin,
                Themes.IconUrlToReportPlugin);
        }

        public void AddButton(HtmlTextWriter writer, string href, string onclick, string title, string urlIcon)
        {
            writer.AddAttribute(HtmlTextWriterAttribute.Href, href);
            writer.AddAttribute(HtmlTextWriterAttribute.Onclick, onclick);
            writer.AddAttribute(HtmlTextWriterAttribute.Title, title);
            writer.RenderBeginTag(HtmlTextWriterTag.A);

            writer.AddAttribute(HtmlTextWriterAttribute.Src, urlIcon);
            writer.AddAttribute(HtmlTextWriterAttribute.Alt, title);
            writer.AddStyleAttribute(HtmlTextWriterStyle.BorderWidth, "0px");
            writer.RenderBeginTag(HtmlTextWriterTag.Img);
            writer.RenderEndTag(); // Img

            writer.RenderEndTag(); // A
            writer.Write("&nbsp;");
        }
    }
}
