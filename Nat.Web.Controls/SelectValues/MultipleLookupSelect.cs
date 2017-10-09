using System.ComponentModel;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Nat.Web.Controls.GenerationClasses;

namespace Nat.Web.Controls.SelectValues
{
    using System;

    using Nat.Web.Controls.GenerationClasses.Data;

    public class MultipleLookupSelect : BaseListDataBoundControl
    {
        BaseLookup Lookup;

        public MultipleLookupSelect()
        {
            ShowOnlySelected = true;
        }

        [Localizable(true)]
        public string ArchiveMessage { get; set; }
        public string ArchiveIcon { get; set; }
        public string LookupTableName { get; set; }
        public string LookupProjectName { get; set; }
        public string SharedDataSourceName { get; set; }
        public string OtherDataSourceView { get; set; }
        public string OtherDataSourceLibrary { get; set; }
        public string SelectMode { get; set; }
        public BrowseFilterParameters BrowseFilterParameters { get; set; }

        public override int Columns
        {
            get { return 1; }
        }

        public virtual void SetFilters(BrowseFilterParameters filters)
        {
            BrowseFilterParameters = filters;
        }

        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);
            if (!ReadOnly)
            {
                if (!string.IsNullOrEmpty(OtherDataSourceLibrary) && !string.IsNullOrEmpty(OtherDataSourceView))
                {
                    throw new NotSupportedException();
                }

                if (!string.IsNullOrEmpty(SharedDataSourceName))
                {
                    var dataSource = new SharedDataSource<long>();
                    dataSource.DataSourceName = SharedDataSourceName;
                    LookupProjectName = dataSource.BaseView.GetType().Assembly.FullName.Split(',')[0];
                    if (string.IsNullOrEmpty(dataSource.BaseView.TableName))
                        LookupTableName = dataSource.BaseView.TableType.Name;
                    else
                        LookupTableName = dataSource.BaseView.TableName;
                }

                Lookup = new BaseLookup
                             {
                                 ExtenderAjaxControl = ExtenderAjaxControl,
                                 ProjectName = LookupProjectName,
                                 TableName = LookupTableName,
                                 Width = new Unit(300, UnitType.Pixel),
                                 IsMultipleSelect = true,
                                 SelectMode = SelectMode,
                                 MinimumPrefixLength = 2,
                                 ID = "lookup",
                             };
                Controls.Add(Lookup);
            }

            LoadItemsFromPostData();
        }

        protected override void RenderBeginRows(HtmlTextWriter writer)
        {
            base.RenderBeginRows(writer);
            if (ReadOnly || RenderOnlyText) return;

            writer.RenderBeginTag(HtmlTextWriterTag.Tr);
            writer.RenderBeginTag(HtmlTextWriterTag.Td);

            RenderHiddenFieldForItems(writer);

            Lookup.OnChangedValue = string.Format("MultipleLookupSelect_ValueChanged(this, '{0}');", ClientID);
            Lookup.BrowseFilterParameters = BrowseFilterParameters;
            Lookup.Render(writer, ExtenderAjaxControl);

            writer.RenderEndTag();//Td
            writer.RenderEndTag();//Tr
        }

        public override void RenderBeginTag(HtmlTextWriter writer)
        {
            if (!RenderOnlyText)
                writer.AddAttribute("deleteIconUrl", Themes.IconUrlDelete);
            base.RenderBeginTag(writer);
        }

        protected override void RenderItem(HtmlTextWriter writer, ListControlItem item, BaseListDataBoundRenderEventArgs args)
        {
            if (RenderOnlyText)
            {
                writer.Write(item.Text);
                return;
            }
            
            if (!string.IsNullOrEmpty(item.ToolTip))
                writer.AddAttribute(HtmlTextWriterAttribute.Title, item.ToolTip);
            writer.AddStyleAttribute(HtmlTextWriterStyle.Width, args.Width);
            writer.RenderBeginTag(HtmlTextWriterTag.Td);
            if (!item.Selected)
                writer.WriteBeginTag("del");

            #region checkbox

            if (!ReadOnly && item.Enabled && item.Selected)
            {
                var link = new BaseHyperLink
                               {
                                   Url = "javascript:void(0);",
                                   OnClick = string.Format("MultipleLookupSelect_DeleteItem(this, '{0}', '{1}', '{2}'); return false;", ClientID, item.SelectedKey, item.Value),
                                   ImgUrl = Themes.IconUrlDelete,
                               };
                link.Render(writer, null);
                writer.Write(" ");
            }

            #endregion

            #region Label

            writer.RenderBeginTag(HtmlTextWriterTag.Span);

            if (!string.IsNullOrEmpty(item.Text))
            {
                var text = HttpUtility.HtmlEncode(item.Text);
                if (!string.IsNullOrEmpty(text))
                    writer.Write(text.Replace("\r\n", "<br />"));
            }

            writer.RenderEndTag();

            #endregion

            if (!item.Selected)
                writer.WriteEndTag("del");

            writer.RenderEndTag();
        }
    }
}