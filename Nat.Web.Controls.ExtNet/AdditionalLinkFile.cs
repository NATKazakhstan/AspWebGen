namespace Nat.Web.Controls.ExtNet
{
    using System;
    using System.Web;

    using Ext.Net;

    using Nat.Web.Controls.Properties;

    public class AdditionalLinkFile
    {
        public string FieldLabel { get; set; }
        public string FileName { get; set; }
        public string ClickHandler { get; set; }

        public AbstractComponent CreateLinkFile()
        {
            var container = new Ext.Net.Container
            {
                ID = "LinkFileContainer",
                Layout = "Fit"
            };
            var fileLinkField = new Ext.Net.TextField
            {
                ID = "LinkFileField",
                FieldLabel = FieldLabel,
                ReadOnly = true,
                LabelWidth = 200,
                Padding = 5,
                Width = 500,
                AutoDataBind = true
            };
            var fileLink = new Ext.Net.LinkButton
            {
                ID = "LinkFileButton",
                Text = FileName,
                PaddingSpec = "1 0 1 4",
            };
            fileLinkField.LeftButtons.Add(fileLink);
            fileLink.Listeners.Click.Handler = ClickHandler;
            container.Items.Add(fileLinkField);
            return container;
        }

        public void AddBefore(FormPanel formPanel, AbstractComponent archiveNumberControl)
        {
            var indexOfControl = formPanel.Items.IndexOf(archiveNumberControl.ParentComponent);
            formPanel.Items.Insert(indexOfControl, CreateLinkFile());
        }
    }
}