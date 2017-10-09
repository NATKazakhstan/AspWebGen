/*
 * Created by: Arman K. Karibaev
 * Created: 30.10.2007
 * Copyright © JSC NAT Kazakhstan 2007
 */

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing.Design;
using System.Globalization;
using System.Security.Permissions;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Nat.Web.Controls.Properties;

[assembly : WebResource("Nat.Web.Controls.CommandFieldExt.Images.cancel.gif", "image/gif")]
[assembly : WebResource("Nat.Web.Controls.CommandFieldExt.Images.del.gif", "image/gif")]
[assembly : WebResource("Nat.Web.Controls.CommandFieldExt.Images.edit.gif", "image/gif")]
[assembly : WebResource("Nat.Web.Controls.CommandFieldExt.Images.insert.gif", "image/gif")]
[assembly : WebResource("Nat.Web.Controls.CommandFieldExt.Images.new.gif", "image/gif")]
[assembly : WebResource("Nat.Web.Controls.CommandFieldExt.Images.ok.gif", "image/gif")]
[assembly : WebResource("Nat.Web.Controls.CommandFieldExt.Images.select.gif", "image/gif")]

namespace Nat.Web.Controls.CommandFieldExt
{
    [AspNetHostingPermission(SecurityAction.LinkDemand, Level = AspNetHostingPermissionLevel.Minimal),
     AspNetHostingPermission(SecurityAction.InheritanceDemand, Level = AspNetHostingPermissionLevel.Minimal)]
    public class CommandFieldExt : ButtonFieldBase
    {
        # region Поля

        private const string JScriptCode = "javascript:if(!confirm('{0}')) return false;";
        private readonly List<AdditionalButton> additionalButtons = new List<AdditionalButton>();
        private WebControl cancelButton;
        private WebControl deleteButton;
        private WebControl editButton;
        private string fieldName;
        private WebControl insertButton;
        private WebControl newButton;
        private WebControl selectButton;
        private WebControl updateButton;

        # endregion

        public CommandFieldExt()
        {
            PreficsButtonsID = "";
        }

        # region Свойства

        # region Кнопки

        [Browsable(false)]
        public WebControl CancelButton
        {
            get { return cancelButton; }
        }

        [Browsable(false)]
        public WebControl DeleteButton
        {
            get { return deleteButton; }
        }

        [Browsable(false)]
        public WebControl EditButton
        {
            get { return editButton; }
        }

        [Browsable(false)]
        public WebControl InsertButton
        {
            get { return insertButton; }
        }

        [Browsable(false)]
        public WebControl NewButton
        {
            get { return newButton; }
        }

        [Browsable(false)]
        public WebControl SelectButton
        {
            get { return selectButton; }
        }

        [Browsable(false)]
        public WebControl UpdateButton
        {
            get { return updateButton; }
        }

        # endregion

        # region ImageUrl

        [Category("Appearance")]
        [DefaultValue("")]
        [Description("CommandField_CancelImageUrl")]
        [UrlProperty]
        [Editor(
            "System.Web.UI.Design.ImageUrlEditor, System.Design, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a"
            , typeof (UITypeEditor))]
        public virtual string CancelImageUrl
        {
            get
            {
                object obj2 = ViewState["CancelImageUrl"];
                if (obj2 != null)
                    return (string) obj2;
                return string.Empty;
            }
            set
            {
                if (!Equals(value, ViewState["CancelImageUrl"]))
                {
                    ViewState["CancelImageUrl"] = value;
                    OnFieldChanged();
                }
            }
        }

        [Category("Appearance")]
        [DefaultValue("")]
        [Description("CommandField_DeleteImageUrl")]
        [UrlProperty]
        [Editor(
            "System.Web.UI.Design.ImageUrlEditor, System.Design, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a"
            , typeof (UITypeEditor))]
        public virtual string DeleteImageUrl
        {
            get
            {
                object obj2 = ViewState["DeleteImageUrl"];
                if (obj2 != null)
                    return (string) obj2;
                return string.Empty;
            }
            set
            {
                if (!Equals(value, ViewState["DeleteImageUrl"]))
                {
                    ViewState["DeleteImageUrl"] = value;
                    OnFieldChanged();
                }
            }
        }

        [Category("Appearance")]
        [DefaultValue("")]
        [Description("CommandField_EditImageUrl")]
        [UrlProperty]
        [Editor(
            "System.Web.UI.Design.ImageUrlEditor, System.Design, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a"
            , typeof (UITypeEditor))]
        public virtual string EditImageUrl
        {
            get
            {
                object obj2 = ViewState["EditImageUrl"];
                if (obj2 != null)
                    return (string) obj2;
                return string.Empty;
            }
            set
            {
                if (!Equals(value, ViewState["EditImageUrl"]))
                {
                    ViewState["EditImageUrl"] = value;
                    OnFieldChanged();
                }
            }
        }

        [Category("Appearance")]
        [DefaultValue("")]
        [Description("CommandField_InsertImageUrl")]
        [UrlProperty]
        [Editor(
            "System.Web.UI.Design.ImageUrlEditor, System.Design, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a"
            , typeof (UITypeEditor))]
        public virtual string InsertImageUrl
        {
            get
            {
                object obj2 = ViewState["InsertImageUrl"];
                if (obj2 != null)
                    return (string) obj2;
                return string.Empty;
            }
            set
            {
                if (!Equals(value, ViewState["InsertImageUrl"]))
                {
                    ViewState["InsertImageUrl"] = value;
                    OnFieldChanged();
                }
            }
        }

        [Category("Appearance")]
        [DefaultValue("")]
        [Description("CommandField_NewImageUrl")]
        [UrlProperty]
        [Editor(
            "System.Web.UI.Design.ImageUrlEditor, System.Design, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a"
            , typeof (UITypeEditor))]
        public virtual string NewImageUrl
        {
            get
            {
                object obj2 = ViewState["NewImageUrl"];
                if (obj2 != null)
                    return (string) obj2;
                return string.Empty;
            }
            set
            {
                if (!Equals(value, ViewState["NewImageUrl"]))
                {
                    ViewState["NewImageUrl"] = value;
                    OnFieldChanged();
                }
            }
        }

        [Category("Appearance")]
        [DefaultValue("")]
        [Description("CommandField_SelectImageUrl")]
        [UrlProperty]
        [Editor(
            "System.Web.UI.Design.ImageUrlEditor, System.Design, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a"
            , typeof (UITypeEditor))]
        public virtual string SelectImageUrl
        {
            get
            {
                object obj2 = ViewState["SelectImageUrl"];
                if (obj2 != null)
                    return (string) obj2;
                return string.Empty;
            }
            set
            {
                if (!Equals(value, ViewState["SelectImageUrl"]))
                {
                    ViewState["SelectImageUrl"] = value;
                    OnFieldChanged();
                }
            }
        }

        [Category("Appearance")]
        [DefaultValue("")]
        [Description("CommandField_UpdateImageUrl")]
        [UrlProperty]
        [Editor(
            "System.Web.UI.Design.ImageUrlEditor, System.Design, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a"
            , typeof (UITypeEditor))]
        public virtual string UpdateImageUrl
        {
            get
            {
                object obj2 = ViewState["UpdateImageUrl"];
                if (obj2 != null)
                    return (string) obj2;
                return string.Empty;
            }
            set
            {
                if (!Equals(value, ViewState["UpdateImageUrl"]))
                {
                    ViewState["UpdateImageUrl"] = value;
                    OnFieldChanged();
                }
            }
        }

        # endregion

        # region ButtonText

        [Category("Appearance")]
        [Description("CommandField_CancelText")]
        [Localizable(true)]
        public virtual string CancelText
        {
            get
            {
                object obj2 = ViewState["CancelText"];
                if (obj2 != null)
                    return (string) obj2;
                return Resources.SCancelText;
            }
            set
            {
                if (!Equals(value, ViewState["CancelText"]))
                {
                    ViewState["CancelText"] = value;
                    OnFieldChanged();
                }
            }
        }

        [Category("Appearance")]
        [Description("CommandField_DeleteText")]
        [Localizable(true)]
        public virtual string DeleteText
        {
            get
            {
                object obj2 = ViewState["DeleteText"];
                if (obj2 != null)
                    return (string) obj2;
                return Resources.SDeleteText;
            }
            set
            {
                if (!Equals(value, ViewState["DeleteText"]))
                {
                    ViewState["DeleteText"] = value;
                    OnFieldChanged();
                }
            }
        }

        [Category("Appearance")]
        [Description("CommandField_EditText")]
        [Localizable(true)]
        public virtual string EditText
        {
            get
            {
                object obj2 = ViewState["EditText"];
                if (obj2 != null)
                    return (string) obj2;
                return Resources.SEditText;
            }
            set
            {
                if (!Equals(value, ViewState["EditText"]))
                {
                    ViewState["EditText"] = value;
                    OnFieldChanged();
                }
            }
        }

        [Category("Appearance")]
        [Description("CommandField_InsertText")]
        [Localizable(true)]
        public virtual string InsertText
        {
            get
            {
                object obj2 = ViewState["InsertText"];
                if (obj2 != null)
                    return (string) obj2;
                return Resources.SInsertText;
            }
            set
            {
                if (!Equals(value, ViewState["InsertText"]))
                {
                    ViewState["InsertText"] = value;
                    OnFieldChanged();
                }
            }
        }

        [Category("Appearance")]
        [Description("CommandField_NewText")]
        [Localizable(true)]
        public virtual string NewText
        {
            get
            {
                object obj2 = ViewState["NewText"];
                if (obj2 != null)
                    return (string) obj2;
                return Resources.SNewText;
            }
            set
            {
                if (!Equals(value, ViewState["NewText"]))
                {
                    ViewState["NewText"] = value;
                    OnFieldChanged();
                }
            }
        }

        [Category("Appearance")]
        [Description("CommandField_SelectText")]
        [Localizable(true)]
        public virtual string SelectText
        {
            get
            {
                object obj2 = ViewState["SelectText"];
                if (obj2 != null)
                    return (string) obj2;
                return Resources.SSelectText;
            }
            set
            {
                if (!Equals(value, ViewState["SelectText"]))
                {
                    ViewState["SelectText"] = value;
                    OnFieldChanged();
                }
            }
        }

        [Category("Appearance")]
        [Description("CommandField_UpdateText")]
        [Localizable(true)]
        public virtual string UpdateText
        {
            get
            {
                object obj2 = ViewState["UpdateText"];
                if (obj2 != null)
                    return (string) obj2;
                return Resources.SUpdateText;
            }
            set
            {
                if (!Equals(value, ViewState["UpdateText"]))
                {
                    ViewState["UpdateText"] = value;
                    OnFieldChanged();
                }
            }
        }

        # endregion

        # region ShowButton

        [Category("Behavior")]
        [DefaultValue(true)]
        [Description("CommandField_ShowCancelButton")]
        public virtual bool ShowCancelButton
        {
            get
            {
                object obj2 = ViewState["ShowCancelButton"];
                if (obj2 != null)
                    return (bool) obj2;
                return true;
            }
            set
            {
                object obj2 = ViewState["ShowCancelButton"];
                if ((obj2 == null) || (((bool) obj2) != value))
                {
                    ViewState["ShowCancelButton"] = value;
                    OnFieldChanged();
                }
            }
        }

        [Category("Behavior")]
        [DefaultValue(false)]
        [Description("CommandField_ShowDeleteButton")]
        public virtual bool ShowDeleteButton
        {
            get
            {
                object obj2 = ViewState["ShowDeleteButton"];
                if (obj2 != null)
                    return (bool) obj2;
                return false;
            }
            set
            {
                object obj2 = ViewState["ShowDeleteButton"];
                if ((obj2 == null) || (((bool) obj2) != value))
                {
                    ViewState["ShowDeleteButton"] = value;
                    OnFieldChanged();
                }
            }
        }

        [Category("Behavior")]
        [DefaultValue(false)]
        [Description("CommandField_ShowEditButton")]
        public virtual bool ShowEditButton
        {
            get
            {
                object obj2 = ViewState["ShowEditButton"];
                if (obj2 != null)
                    return (bool) obj2;
                return false;
            }
            set
            {
                object obj2 = ViewState["ShowEditButton"];
                if ((obj2 == null) || (((bool) obj2) != value))
                {
                    ViewState["ShowEditButton"] = value;
                    OnFieldChanged();
                }
            }
        }

        [Category("Behavior")]
        [DefaultValue(false)]
        [Description("CommandField_ShowInsertButton")]
        public virtual bool ShowInsertButton
        {
            get
            {
                object obj2 = ViewState["ShowInsertButton"];
                if (obj2 != null)
                    return (bool) obj2;
                return false;
            }
            set
            {
                object obj2 = ViewState["ShowInsertButton"];
                if ((obj2 == null) || (((bool) obj2) != value))
                {
                    ViewState["ShowInsertButton"] = value;
                    OnFieldChanged();
                }
            }
        }

        [Category("Behavior")]
        [DefaultValue(false)]
        [Description("CommandField_ShowSelectButton")]
        public virtual bool ShowSelectButton
        {
            get
            {
                object obj2 = ViewState["ShowSelectButton"];
                if (obj2 != null)
                    return (bool) obj2;
                return false;
            }
            set
            {
                object obj2 = ViewState["ShowSelectButton"];
                if ((obj2 == null) || (((bool) obj2) != value))
                {
                    ViewState["ShowSelectButton"] = value;
                    OnFieldChanged();
                }
            }
        }

        # endregion

        # region ConfirmDelete

        [Category("ConfirmDelete")]
        [DefaultValue(true)]
        [Description("Запрос на подтверждение удаления при ConfirmDelete = true")]
        public virtual bool ConfirmDelete
        {
            get
            {
                object obj2 = ViewState["ConfirmDelete"];
                if (obj2 != null)
                    return (bool) obj2;
                return true;
            }
            set
            {
                object obj2 = ViewState["ConfirmDelete"];
                if ((obj2 == null) || (((bool) obj2) != value))
                {
                    ViewState["ConfirmDelete"] = value;
                    OnFieldChanged();
                }
            }
        }

        [Category("ConfirmDelete")]
        [Description("Текст выводимый при подтверждении удаления")]
        [Localizable(true)]
        public virtual string ConfirmDeleteText
        {
            get
            {
                object obj2 = ViewState["ConfirmDeleteText"];
                if (obj2 != null)
                    return (string) obj2;
                return Resources.SConfirmDeleteText;
            }
            set
            {
                if (!Equals(value, ViewState["ConfirmDeleteText"]))
                {
                    ViewState["ConfirmDeleteText"] = value;
                    OnFieldChanged();
                }
            }
        }

        # endregion

        [Category("Behavior")]
        [DefaultValue(true)]
        [Description("ButtonFieldBase_CausesValidation")]
        public override bool CausesValidation
        {
            get
            {
                object obj2 = ViewState["CausesValidation"];
                if (obj2 != null)
                    return (bool) obj2;
                return true;
            }
            set { base.CausesValidation = value; }
        }

        [Category("Behavior")]
        [DefaultValue(true)]
        [Description("Автоматически ставить стиль")]
        public virtual bool AutoDisplayStyle
        {
            get
            {
                object obj2 = ViewState["AutoDisplayStyle"];
                if (obj2 != null)
                    return (bool) obj2;
                return true;
            }
            set
            {
                if (!Equals(value, ViewState["AutoDisplayStyle"]))
                {
                    ViewState["AutoDisplayStyle"] = value;
                    OnFieldChanged();
                }
            }
        }

        [Category("Behavior")]
        [DefaultValue(FieldDisplayStyle.ImageAndText)]
        [Description("Стиль вывода полей")]
        [Localizable(true)]
        public virtual FieldDisplayStyle DisplayStyle
        {
            get
            {
                object obj2 = ViewState["DisplayStyle"];
                if (obj2 != null)
                    return (FieldDisplayStyle) obj2;
                return FieldDisplayStyle.ImageAndText;
            }
            set
            {
                if (!Equals(value, ViewState["DisplayStyle"]))
                {
                    ViewState["DisplayStyle"] = value;
//                    OnFieldChanged();
                }
            }
        }

        [Browsable(false)]
        public override ButtonType ButtonType
        {
            get { return base.ButtonType; }
            set { base.ButtonType = value; }
        }

        [Browsable(true)]
        [Category("Data")]
        [Description("Коллекция дополнительных кнопок")]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        [Localizable(false)]
        [PersistenceMode(PersistenceMode.InnerProperty)]
        public List<AdditionalButton> AdditionalButtons
        {
            get { return additionalButtons; }
        }

        [Themeable(false)]
        [ParenthesizePropertyName(true)]
        [MergableProperty(false)]
        [Filterable(false)]
        public virtual string FieldName
        {
            get { return fieldName; }
            set { fieldName = value; }
        }

        [DefaultValue("")]
        [Themeable(false)]
        [Localizable(false)]
        public string PreficsButtonsID { get; set; }

        # endregion

        # region Методы

        protected virtual IDataControlButton AddButtonToCell(DataControlFieldCell cell, string commandName,
                                                             string buttonText,
                                                             bool causesValidation, string validationGroup, int rowIndex,
                                                             string imageUrl)
        {
            IDataControlButton control;
            IPostBackContainer container = Control as IPostBackContainer;
            bool flag = true;

            if (AutoDisplayStyle)
            {
                if (Control is GridViewExt)
                    DisplayStyle = FieldDisplayStyle.Image;
                else if (Control is DetailsViewExt)
                    DisplayStyle = FieldDisplayStyle.ImageAndText;
            }

            if (string.IsNullOrEmpty(imageUrl))
                imageUrl = GetImageUrl(commandName);
            FieldDisplayStyle displayStyle = string.IsNullOrEmpty(imageUrl) ? FieldDisplayStyle.Text : DisplayStyle;

//#if LOCAL
           displayStyle = FieldDisplayStyle.Text;
//#endif

            switch (displayStyle)
            {
                case FieldDisplayStyle.Text:
                    if ((container == null) || causesValidation)
                        control = new DataControlLinkButton(null);
                    else
                    {
                        control = new DataControlLinkButton(container);
                        flag = false;
                    }
                    control.Text = buttonText;
                    break;
                case FieldDisplayStyle.Image:
                    if ((container != null) && !causesValidation)
                    {
                        control = new DataControlImageButton(container);
                        flag = false;
                    }
                    else
                        control = new DataControlImageButton(null);
                    ((DataControlImageButton) control).AlternateText = buttonText;
                    ((DataControlImageButton) control).ImageUrl = imageUrl;
                    break;
                case FieldDisplayStyle.ImageAndText:
                    if ((container != null) && !causesValidation)
                    {
                        control = new DataControlImageTextButton(container);
                        flag = false;
                    }
                    else
                        control = new DataControlImageTextButton(null);
                    ((DataControlImageTextButton) control).AlternateText = buttonText;
                    ((DataControlImageTextButton) control).ImageUrl = imageUrl;
                    break;
                default:
                    if ((container != null) && !causesValidation)
                    {
                        control = new DataControlImageButton(container);
                        flag = false;
                    }
                    else
                        control = new DataControlImageButton(null);
                    ((DataControlImageButton) control).AlternateText = buttonText;
                    ((DataControlImageButton) control).ImageUrl = imageUrl;
                    break;
            }
            control.CommandName = commandName;
            control.CommandArgument = rowIndex.ToString(CultureInfo.InvariantCulture);
            if (flag)
                control.CausesValidation = causesValidation;
            control.ValidationGroup = validationGroup;
            SetButton(control, commandName);
            cell.Controls.Add((WebControl) control);
            return control;
        }

        protected virtual void AddButtonToCell(DataControlFieldCell cell, string commandName, string buttonText,
                                               bool causesValidation, string validationGroup, int rowIndex,
                                               string imageUrl, string controlID)
        {
            IDataControlButton control = AddButtonToCell(cell, commandName, buttonText, causesValidation,
                                                         validationGroup,
                                                         rowIndex, imageUrl);
            ((WebControl) control).ID = controlID;
        }

        private string GetImageUrl(string commandName)
        {
            switch (commandName)
            {
                case "Cancel":
                    return Control.Page.ClientScript.GetWebResourceUrl(GetType(),
                                                                       "Nat.Web.Controls.CommandFieldExt.Images.cancel.gif");
                case "Delete":
                    return Control.Page.ClientScript.GetWebResourceUrl(GetType(),
                                                                       "Nat.Web.Controls.CommandFieldExt.Images.del.gif");
                case "Edit":
                    return Control.Page.ClientScript.GetWebResourceUrl(GetType(),
                                                                       "Nat.Web.Controls.CommandFieldExt.Images.edit.gif");
                case "Insert":
                    return Control.Page.ClientScript.GetWebResourceUrl(GetType(),
                                                                       "Nat.Web.Controls.CommandFieldExt.Images.insert.gif");
                case "New":
                    return Control.Page.ClientScript.GetWebResourceUrl(GetType(),
                                                                       "Nat.Web.Controls.CommandFieldExt.Images.new.gif");
                case "Select":
                    return Control.Page.ClientScript.GetWebResourceUrl(GetType(),
                                                                       "Nat.Web.Controls.CommandFieldExt.Images.select.gif");
                case "Update":
                    return Control.Page.ClientScript.GetWebResourceUrl(GetType(),
                                                                       "Nat.Web.Controls.CommandFieldExt.Images.ok.gif");
            }
            return "";
        }

        private void SetButton(IDataControlButton control, string commandName)
        {
            switch (commandName)
            {
                case "Cancel":
                    cancelButton = (WebControl) control;
                    cancelButton.ID = PreficsButtonsID + "btnCancelCommandField";
                    break;
                case "Delete":
                    deleteButton = (WebControl) control;
                    deleteButton.ID = PreficsButtonsID + "btnDeleteCommandField";
                    if (ConfirmDelete)
                        ((IDataControlButton) deleteButton).OnClientClick = string.Format(JScriptCode, ConfirmDeleteText);
                    break;
                case "Edit":
                    editButton = (WebControl) control;
                    editButton.ID = PreficsButtonsID + "btnEditCommandField";
                    break;
                case "Insert":
                    insertButton = (WebControl) control;
                    insertButton.ID = PreficsButtonsID + "btnInsertCommandField";
                    break;
                case "New":
                    newButton = (WebControl) control;
                    newButton.ID = PreficsButtonsID + "btnNewCommandField";
                    break;
                case "Select":
                    selectButton = (WebControl) control;
                    selectButton.ID = PreficsButtonsID + "btnSelectCommandField";
                    break;
                case "Update":
                    updateButton = (WebControl) control;
                    updateButton.ID = PreficsButtonsID + "btnUpdateCommandField";
                    break;
            }
        }

        public override void InitializeCell(DataControlFieldCell cell, DataControlCellType cellType,
                                            DataControlRowState rowState, int rowIndex)
        {
            base.InitializeCell(cell, cellType, rowState, rowIndex);
            cell.Style["white-space"] = "nowrap";
            bool showEditButton = ShowEditButton;
            bool showDeleteButton = ShowDeleteButton;
            bool showInsertButton = ShowInsertButton;
            bool showSelectButton = ShowSelectButton;
            bool showCancelButton = ShowCancelButton;
            bool flag6 = true;
            bool causesValidation = CausesValidation;
            string validationGroup = ValidationGroup;
            if (cellType == DataControlCellType.DataCell)
            {
                LiteralControl control;
                if ((rowState & (DataControlRowState.Insert | DataControlRowState.Edit)) != DataControlRowState.Normal)
                {
                    if (((rowState & DataControlRowState.Edit) != DataControlRowState.Normal) && showEditButton)
                    {
                        AddButtonToCell(cell, "Update", UpdateText, causesValidation, validationGroup, rowIndex,
                                        UpdateImageUrl);
                        if (showCancelButton)
                        {
//                            control = new LiteralControl("&nbsp;");
//                            cell.Controls.Add(control);
                            AddButtonToCell(cell, "Cancel", CancelText, false, string.Empty, rowIndex, CancelImageUrl);
                        }
                    }
                    if (((rowState & DataControlRowState.Insert) != DataControlRowState.Normal) && showInsertButton)
                    {
                        AddButtonToCell(cell, "Insert", InsertText, causesValidation, validationGroup, rowIndex,
                                        InsertImageUrl);
                        if (showCancelButton)
                        {
//                            control = new LiteralControl("&nbsp;");
//                            cell.Controls.Add(control);
                            AddButtonToCell(cell, "Cancel", CancelText, false, string.Empty, rowIndex, CancelImageUrl);
                        }
                    }
                }
                else
                {
                    if (showInsertButton)
                    {
                        AddButtonToCell(cell, "New", NewText, false, string.Empty, rowIndex, NewImageUrl);
                        flag6 = false;
                    }
                    if (showEditButton)
                    {
                        if (!flag6)
                        {
//                            control = new LiteralControl("&nbsp;");
//                            cell.Controls.Add(control);
                        }
                        AddButtonToCell(cell, "Edit", EditText, false, string.Empty, rowIndex, EditImageUrl);
                        flag6 = false;
                    }
                    if (showSelectButton)
                    {
                        if (!flag6)
                        {
//                            control = new LiteralControl("&nbsp;");
//                            cell.Controls.Add(control);
                        }
                        AddButtonToCell(cell, "Select", SelectText, false, string.Empty, rowIndex, SelectImageUrl);
                        flag6 = false;
                    }
                    if (showDeleteButton)
                    {
                        if (!flag6)
                        {
//                            control = new LiteralControl("&nbsp;");
//                            cell.Controls.Add(control);
                        }
                        AddButtonToCell(cell, "Delete", DeleteText, false, string.Empty, rowIndex, DeleteImageUrl);
                        flag6 = false;
                    }
                    foreach (AdditionalButton button in AdditionalButtons)
                    {
                        if (!flag6)
                        {
//                            control = new LiteralControl("&nbsp;");
//                            cell.Controls.Add(control);
                        }
                        AddButtonToCell(cell, button.CommandName, button.Text, false, string.Empty, rowIndex,
                                        button.ImageUrl, button.ControlID);
                        flag6 = false;
                    }
                }
            }
        }

        protected override DataControlField CreateField()
        {
            return new CommandFieldExt();
        }

        protected override void CopyProperties(DataControlField newField)
        {
            ((CommandFieldExt) newField).CancelImageUrl = CancelImageUrl;
            ((CommandFieldExt) newField).CancelText = CancelText;
            ((CommandFieldExt) newField).DeleteImageUrl = DeleteImageUrl;
            ((CommandFieldExt) newField).DeleteText = DeleteText;
            ((CommandFieldExt) newField).EditImageUrl = EditImageUrl;
            ((CommandFieldExt) newField).EditText = EditText;
            ((CommandFieldExt) newField).InsertImageUrl = InsertImageUrl;
            ((CommandFieldExt) newField).InsertText = InsertText;
            ((CommandFieldExt) newField).NewImageUrl = NewImageUrl;
            ((CommandFieldExt) newField).NewText = NewText;
            ((CommandFieldExt) newField).SelectImageUrl = SelectImageUrl;
            ((CommandFieldExt) newField).SelectText = SelectText;
            ((CommandFieldExt) newField).UpdateImageUrl = UpdateImageUrl;
            ((CommandFieldExt) newField).UpdateText = UpdateText;
            ((CommandFieldExt) newField).ShowCancelButton = ShowCancelButton;
            ((CommandFieldExt) newField).ShowDeleteButton = ShowDeleteButton;
            ((CommandFieldExt) newField).ShowEditButton = ShowEditButton;
            ((CommandFieldExt) newField).ShowSelectButton = ShowSelectButton;
            ((CommandFieldExt) newField).ShowInsertButton = ShowInsertButton;
            ((CommandFieldExt) newField).ConfirmDelete = ConfirmDelete;
            ((CommandFieldExt) newField).ConfirmDeleteText = ConfirmDeleteText;
            ((CommandFieldExt) newField).FieldName = FieldName;
            ((CommandFieldExt) newField).DisplayStyle = DisplayStyle;
            foreach (AdditionalButton button in additionalButtons)
            {
                ((CommandFieldExt) newField).additionalButtons.Add(button);
            }
            base.CopyProperties(newField);
        }

        public override void ValidateSupportsCallback()
        {
            if (ShowSelectButton)
                throw new NotSupportedException("CommandField_CallbacksNotSupported_" + Control.ID);
        }

        # region Значение по умолчанию из ресурсов

        internal void ResetCancelText()
        {
            CancelText = Resources.SCancelText;
        }

        internal bool ShouldSerializeCancelText()
        {
            return CancelText != Resources.SCancelText;
        }

        internal void ResetDeleteText()
        {
            DeleteText = Resources.SDeleteText;
        }

        internal bool ShouldSerializeDeleteText()
        {
            return DeleteText != Resources.SDeleteText;
        }

        internal void ResetEditText()
        {
            EditText = Resources.SEditText;
        }

        internal bool ShouldSerializeEditText()
        {
            return EditText != Resources.SEditText;
        }

        internal void ResetInsertText()
        {
            InsertText = Resources.SInsertText;
        }

        internal bool ShouldSerializeInsertText()
        {
            return InsertText != Resources.SInsertText;
        }

        internal void ResetNewText()
        {
            NewText = Resources.SNewText;
        }

        internal bool ShouldSerializeNewText()
        {
            return NewText != Resources.SNewText;
        }

        internal void ResetSelectText()
        {
            SelectText = Resources.SSelectText;
        }

        internal bool ShouldSerializeSelectText()
        {
            return SelectText != Resources.SSelectText;
        }

        internal void ResetUpdateText()
        {
            UpdateText = Resources.SUpdateText;
        }

        internal bool ShouldSerializeUpdateText()
        {
            return UpdateText != Resources.SUpdateText;
        }

        internal void ResetConfirmDeleteText()
        {
            ConfirmDeleteText = Resources.SConfirmDeleteText;
        }

        internal bool ShouldSerializeConfirmDeleteText()
        {
            return ConfirmDeleteText != Resources.SConfirmDeleteText;
        }

        # endregion

        # endregion
    }
}