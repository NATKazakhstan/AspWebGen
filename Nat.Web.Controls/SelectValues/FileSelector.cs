using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Script.Serialization;
using System.Web.UI;
using Nat.Tools.Specific;
using Nat.Web.Controls.Data;
using Nat.Web.Controls.GenerationClasses;
using Nat.Web.Controls.Preview;
using Nat.Web.Controls.Properties;
using Nat.Web.Tools.Initialization;
using Nat.Web.Tools.Security;

namespace Nat.Web.Controls.SelectValues
{
    public class FileSelector : BaseListDataBoundControl
    {
        private FileLinkBuilder _fileLinkBuilder;
        private FileLinkBuilder _defaultFileLinkBuilder;
        private bool _dontRenderTd;
        public string FileFieldName { get; set; }
        public string FileDataFieldName { get; set; }
        public string ProjectName { get; set; }
        public string TableName { get; set; }

        public FileSelector()
        {
            ShowOnlySelected = true;
        }

        protected FileLinkBuilder FileLinkBuilder
        {
            get
            {
                return _fileLinkBuilder
                       ?? (_fileLinkBuilder = new FileLinkBuilder
                                                  {
                                                      FieldName = FileFieldName,
                                                      FileManager = ProjectName + "." + TableName + "FileManager",
                                                  });
            }
        }

        protected FileLinkBuilder DefaultFileLinkBuilder
        {
            get
            {
                return _defaultFileLinkBuilder
                       ?? (_defaultFileLinkBuilder = new FileLinkBuilder
                                                         {
                                                             FieldName = "dataFileName",
                                                             FileManager = "Event_LOG.SYS_FileUploadsFileManager",
                                                         });
            }
        }

        protected override void RenderItem(HtmlTextWriter writer, ListControlItem item, BaseListDataBoundRenderEventArgs args)
        {
            if (!_dontRenderTd)
            {
                if (!string.IsNullOrEmpty(item.ToolTip))
                    writer.AddAttribute(HtmlTextWriterAttribute.Title, item.ToolTip);
                writer.AddStyleAttribute(HtmlTextWriterStyle.Width, args.Width);
                writer.RenderBeginTag(HtmlTextWriterTag.Td);
            }

            #region Download file
            
            var linkBuilder = string.IsNullOrEmpty(item.SelectedKey) ? DefaultFileLinkBuilder : FileLinkBuilder;
            var fileLink = new FileLinkWithPreview
                               {
                                   FileName = item.Text,
                                   FileLinkBuilder = linkBuilder,
                               };
            linkBuilder.FileName = item.Text;
            linkBuilder.KeyValue = item.SelectedKey ?? item.Value;
            fileLink.FileManager = linkBuilder.FileManager;
            fileLink.FieldName = linkBuilder.FieldName;
            fileLink.RenderControl(writer);

            #endregion

            #region Delete file

            if (!ReadOnly)
            {
                var link = new BaseHyperLink
                               {
                                   Url = "javascript:void();",
                                   OnClick = string.Format("RemoveListItem(this, '{0}', '{1}', '{2}'); return false;", ClientID, item.SelectedKey, item.Value),
                                   ImgUrl = Themes.IconUrlRemove,
                                   ToolTip = Resources.SDeleteText,
                                   Text = Resources.SDeleteText,
                               };
                HtmlGenerator.RenderHyperLink(writer, link);
            }

            #endregion

            if (!_dontRenderTd)
                writer.RenderEndTag();//Td
        }

        protected override void RenderBeginRows(HtmlTextWriter writer)
        {
            base.RenderBeginRows(writer);

            if (ReadOnly) return;

            writer.RenderBeginTag(HtmlTextWriterTag.Tr);
            if (Columns > 0)
                writer.AddAttribute(HtmlTextWriterAttribute.Colspan, Columns.ToString());
            writer.RenderBeginTag(HtmlTextWriterTag.Td);

            #region hidden field of Items

            RenderHiddenFieldForItems(writer);

            #endregion

            #region File name

            var textBox = new BaseTextBox {Columns = 30, MaxLength = 64};
            var textBoxFileNameClientID = ClientID + "_filename";
            textBox.ID = textBoxFileNameClientID;
            textBox.ToolTip = Resources.SFileName;
            textBox.TextValue = Resources.SFileName;
            HtmlGenerator.RenderTextBox(writer, textBox, null);

            #endregion

            BaseHyperLink link;
            
            #region Post file image from clipboard

            link = new BaseHyperLink
                       {
                           Url = "javascript:void();",
                           OnClick = string.Format("PostFileFromClipboard(this, '{0}', '{1}', '{2}', '{3}'); return false;", ClientID, textBoxFileNameClientID, TableName, ClientID + "_ActivX"),
                           ImgUrl = Themes.IconUrlPaste,
                           ToolTip = Resources.SInsertText,
                           Text = Resources.SInsertText,
                       };
            writer.Write("&nbsp;");
            HtmlGenerator.RenderHyperLink(writer, link);
            writer.WriteLine();
            writer.AddAttribute(HtmlTextWriterAttribute.Id, ClientID + "_ActivX");
            writer.AddAttribute(HtmlTextWriterAttribute.Name, UniqueID + "$ActivX");
            writer.AddAttribute("classid", "clsid:DCDC88B0-09BD-41F4-B762-75D0CB2FFFA0");
            //writer.AddAttribute("classid", "/Files/Nat.ActiveX.Controls.dll#Nat.ActiveX.Controls.UploadImageFiles");
            writer.AddAttribute("VIEWASTEXT", null);
            writer.AddAttribute("codebase", "/Files/Nat.ActiveX.Controls.InstallCab.cab");
            //writer.AddAttribute("codebase", "/Files/Nat.ActiveX.Controls.dll");
            writer.AddStyleAttribute(HtmlTextWriterStyle.Width, "0px");
            writer.AddStyleAttribute(HtmlTextWriterStyle.Height, "0px");
            writer.RenderBeginTag(HtmlTextWriterTag.Object);
            writer.RenderEndTag();

            #endregion
            /*
            #region Post file text from clipboard

            link = new BaseHyperLink
                       {
                           Url = "javascript:void();",
                           OnClick = string.Format("PostFileTextFromClipboard(this, '{0}', '{1}', '{2}'); return false;", ClientID, textBoxFileNameClientID, TableName),
                           ImgUrl = Themes.IconUrlPostFileTextFromClipboard,
                           ToolTip = Resources.SPostFileTextFromClipboard,
                           Text = Resources.SPostFileTextFromClipboard,
                       };
            writer.Write("&nbsp;");
            HtmlGenerator.RenderHyperLink(writer, link);

            #endregion
            */
            #region Post file from dialog

            link = new BaseHyperLink
                       {
                           Url = "javascript:void();",
                           OnClick = string.Format("PostFileFromDialog(this, '{0}', '{1}', '{2}', '{3}'); return false;", ClientID, textBoxFileNameClientID, TableName, ClientID + "_ActivX"),
                           ImgUrl = Themes.IconUrlFolderOpen,
                           ToolTip = Resources.SPostFileFromDialog,
                           Text = Resources.SPostFileFromDialog,
                       };
            writer.Write("&nbsp;");
            HtmlGenerator.RenderHyperLink(writer, link);

            #endregion
            writer.RenderEndTag();//Td
            writer.RenderEndTag();//Tr
        }

        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);
            ScriptManager.Services.Add(new ServiceReference { Path = "/WebServiceUploadFiles.asmx", });
            LoadItemsFromPostData();
        }

        internal string Render(ListControlItem item)
        {
            _dontRenderTd = true;
            var sb = new StringBuilder();
            using (var textWriter = new StringWriter(sb))
            using (var writer = new HtmlTextWriter(textWriter))
            {
                var args = new BaseListDataBoundRenderEventArgs{RowsCount = 1, Width = "100%"};
                RenderItem(writer, item, args);
            }
            return sb.ToString();
        }

        protected override void InitInsertValues(System.Collections.IDictionary values, ListControlItem item)
        {
            base.InitInsertValues(values, item);
            WebInitializer.Initialize();
            using (var db = new DBUploadFilesDataContext(SpecificInstances.DbFactory.CreateConnection()))
            {
                var row = db.SYS_FileUploads.
                    FirstOrDefault(r => r.id == Convert.ToInt64(item.Value) && r.PersonSID == User.GetSID());
                if (row != null)
                {
                    values[FileDataFieldName] = row.data;
                    values[FileFieldName] = row.dataFileName;
                }
            }
        }
    }
}