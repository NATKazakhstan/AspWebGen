using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using AjaxControlToolkit;
using Nat.Web.Controls.GenerationClasses;
using Nat.Web.Controls.Properties;

namespace Nat.Web.Controls.SelectValues
{
    public class MultipleSelect : BaseListDataBoundControl//, IRenderComponent
    {
        [Localizable(true)]
        public string ArchiveMessage { get; set; }
        public string ArchiveIcon { get; set; }

        protected override void LoadControlState(object savedState)
        {
            var items = savedState as List<ListControlItem>;
            if (items != null)
            {
                Items.AddRange(items);
                RequiresDataBinding = false;
            }
            LoadPostBackValues();
        }

        protected override object SaveControlState()
        {
            return Items;
        }

        public override TypeComponent GetTypeComponent()
        {
            return TypeComponent.CheckedControl;
        }

        public override string GetClientID(string value)
        {
            var item = GetAllItems().FirstOrDefault(r => r.Value.Equals(value));
            return item == null ? null : CheckBoxClientID(item);
        }

        private string CheckBoxClientID(ListControlItem item)
        {
            return ClientID + "_" + item.Value + "_" + item.SelectedKey;
        }

        private string CheckBoxUniqueID(ListControlItem item)
        {
            return UniqueID + "$" + item.Value + "$" + item.SelectedKey;
        }

        private void LoadPostBackValues()
        {
            foreach (var item in GetAllItems())
                item.Selected = Page.Request.Form[CheckBoxUniqueID(item)] == "on";
        }

        public virtual void SetFilters(BrowseFilterParameters filters)
        {
        }

        #region Render

        protected override void RenderItem(HtmlTextWriter writer, ListControlItem item, BaseListDataBoundRenderEventArgs args)
        {
            if (RenderOnlyText)
            {
                writer.Write(item.Text);
                return;
            }

            var checkBoxID = CheckBoxClientID(item);

            if (!string.IsNullOrEmpty(item.ToolTip))
                writer.AddAttribute(HtmlTextWriterAttribute.Title, item.ToolTip);
            writer.AddStyleAttribute(HtmlTextWriterStyle.Width, args.Width);
            writer.RenderBeginTag(HtmlTextWriterTag.Td);

            #region checkbox

            if (!ReadOnly)
            {
                writer.AddAttribute(HtmlTextWriterAttribute.Type, "checkbox");
                if (item.Selected)
                {
                    writer.AddAttribute(HtmlTextWriterAttribute.Value, "on");
                    writer.AddAttribute(HtmlTextWriterAttribute.Checked, "checked");
                }
                writer.AddAttribute(HtmlTextWriterAttribute.Id, checkBoxID);
                writer.AddAttribute(HtmlTextWriterAttribute.Name, CheckBoxUniqueID(item));
                writer.AddAttribute("CheckedValue", item.Value);
                if (!item.Enabled || !item.Selectable)
                    writer.AddAttribute(HtmlTextWriterAttribute.Disabled, "disabled");
                writer.RenderBeginTag(HtmlTextWriterTag.Input);
                writer.RenderEndTag();
            }

            #endregion

            RenderItemIcon(writer, item);

            if (string.IsNullOrEmpty(item.ReferenceValue)) 
                RenderLabelItem(writer, item, args, checkBoxID);
            else
                RenderRefereceItem(writer, item, args);

            writer.RenderEndTag();
        }

        private void RenderRefereceItem(HtmlTextWriter writer, ListControlItem item, BaseListDataBoundRenderEventArgs args)
        {
            if (!item.Enabled)
                writer.AddAttribute(HtmlTextWriterAttribute.Disabled, "disabled");
            var url = string.Format("/MainPage.aspx/data/{0}Edit/read?ref{0}={1}", ReferenceTableName, item.ReferenceValue);
            writer.AddAttribute(HtmlTextWriterAttribute.Href, url);
            writer.AddAttribute(HtmlTextWriterAttribute.Target, "_blank");
            writer.RenderBeginTag(HtmlTextWriterTag.A);
            writer.Write(HttpUtility.HtmlEncode(item.Text));
            writer.RenderEndTag();
        }

        protected virtual void RenderLabelItem(HtmlTextWriter writer, ListControlItem item, BaseListDataBoundRenderEventArgs args, string checkBoxID)
        {
            writer.AddAttribute(HtmlTextWriterAttribute.For, checkBoxID);
            if (!item.Enabled)
                writer.AddAttribute(HtmlTextWriterAttribute.Disabled, "disabled");
            writer.RenderBeginTag(HtmlTextWriterTag.Label);
            
            if (!string.IsNullOrEmpty(item.Text))
            {
                var text = HttpUtility.HtmlEncode(item.Text);
                if (!string.IsNullOrEmpty(text))
                    writer.Write(text.Replace("\r\n", "<br />"));
            }

            writer.RenderEndTag();
        }

        protected virtual void RenderItemIcon(HtmlTextWriter writer, ListControlItem item)
        {
            if (!item.Deleted) return;

            // todo: метод для динамического определения иконки
            var archiveIcon = string.IsNullOrEmpty(ArchiveIcon) ? Themes.IconUrlArchive : ArchiveIcon;
            writer.AddAttribute(HtmlTextWriterAttribute.Src, archiveIcon);
            writer.AddAttribute(HtmlTextWriterAttribute.Border, "0");
            // todo: метод для динамического определения сообщения
            var archiveMessage = string.IsNullOrEmpty(ArchiveMessage) ? Resources.SRowArchive : ArchiveMessage;
            writer.AddAttribute(HtmlTextWriterAttribute.Title, archiveMessage);
            writer.AddAttribute(HtmlTextWriterAttribute.Alt, archiveMessage);
            writer.RenderBeginTag(HtmlTextWriterTag.Img);
            writer.RenderEndTag();
        }

        #endregion

        /*
        public void Render(HtmlTextWriter writer, ExtenderAjaxControl extenderAjaxControl)
        {
            EnsureDataBound();
            Render(writer);
        }

        public void InitAjax(Control control, ExtenderAjaxControl extenderAjaxControl)
        {
        }

        string IRenderComponent.ClientID { get; set; }
        string IRenderComponent.UniqueID { get; set; }
        object IRenderComponent.Value { get; set; }
        string IRenderComponent.Text { get; set; }*/
    }
}