/*
 * Created by: Sergey V. Shpakovskiy
 * Created: 21 декабря 2008 г.
 * Copyright © JSC New Age Technologies 2008
 */

namespace Nat.Web.Controls.GenerationClasses
{
    using System;
    using System.Text;
    using System.Web;
    using System.Web.Script.Serialization;
    using System.Web.UI;

    public class BaseInformationValues
    {
        public const string TextStyleError = "errorText";

        public const string TextStyleInformation = "informationText";

        public const string TextStyleWarning = "warningText";

        private readonly StringBuilder _sbIcons = new StringBuilder();

        private readonly StringBuilder _sbMessage = new StringBuilder();

        public BaseInformationValues()
        {
            IsValid = true;
            CanEdit = true;
            CanLook = true;
            CanDelete = true;
            CanSelect = true;
        }

        /// <summary>
        ///     Валидна ли запись, если не валидна, то в справочнике нельзя выбрать
        /// </summary>
        public bool IsValid { get; set; }

        /// <summary>
        ///     Можно ли редактировать
        /// </summary>
        public virtual bool CanEdit { get; set; }

        /// <summary>
        ///     Можно ли просматривать
        /// </summary>
        public virtual bool CanLook { get; set; }

        /// <summary>
        ///     Можно ли удалить
        /// </summary>
        public virtual bool CanDelete { get; set; }

        /// <summary>
        ///     Можно ли выбирать запись, т.е. нужна ли в журнале кнопка "выбрать"
        /// </summary>
        public virtual bool CanSelect { get; set; }

        /// <summary>
        ///     Выполнялась валидация или нет
        /// </summary>
        public bool IsValidated { get; set; }

        [ScriptIgnore]
        public Page Page { get; set; }

        [ScriptIgnore]
        public Control Control { get; set; }

        public string MessageHtml => _sbMessage.ToString();

        public string IconHtml => _sbIcons.ToString();

        public virtual void EnsureValidate()
        {
            if (!IsValidated)
            {
                Validate();
                IsValidated = true;
            }
        }

        public virtual void Validate()
        {
            IsValidated = true;
        }

        public void AddHtml(Action<StringBuilder> action)
        {
            action(_sbIcons);
        }

        public void AddIcon(string iconUrl)
        {
            _sbIcons.Append($"<img src=\"{iconUrl}\" style=\"padding-right:5px;\"/>");
        }

        public void AddIcon(string iconUrl, string toolTip)
        {
            toolTip = HttpUtility.HtmlAttributeEncode(toolTip);
            _sbIcons.Append($"<img src=\"{iconUrl}\" alt=\"{toolTip}\" title=\"{toolTip}\" style=\"padding-right:5px;\"/>");
        }

        public void AddIcon(string iconUrl, string toolTip, int width, int height)
        {
            toolTip = HttpUtility.HtmlAttributeEncode(toolTip);
            _sbIcons.Append($"<img src=\"{iconUrl}\" alt=\"{toolTip}\" title=\"{toolTip}\" style=\"padding-right:5px;width:{width}px;height:{height}px\"/>");
        }

        public void AddIcon(string iconUrl, string toolTip, string url)
        {
            toolTip = HttpUtility.HtmlAttributeEncode(toolTip);
            _sbIcons.Append(
                $"<a href=\"{url}\" target=\"_blank\"><img style=\"padding-right:5px;border:0px\" src=\"{iconUrl}\" alt=\"{toolTip}\" title=\"{toolTip}\"/></a>");
        }

        /// <summary>
        ///     Добавить сообщение.
        /// </summary>
        /// <param name="message">Текст сообщения</param>
        /// <param name="textStyle">Стиль текста, т.е. ClassCss</param>
        public void AddMessage(string message, string textStyle)
        {
            _sbMessage.AppendFormat("<span class=\"{1}\">&nbsp;{0}</span>", message, textStyle);
        }

        /// <summary>
        ///     Добавить сообщение.
        /// </summary>
        /// <param name="message">Текст сообщения</param>
        public void AddMessage(string message)
        {
            _sbMessage.AppendFormat(" {0}", message);
        }

        public void AddHyperLink(string url, string text, string toolTip)
        {
            _sbMessage.AppendFormat(
                "&nbsp;<a href=\"{0}\" title=\"{1}\" class=\"font13\">{2}</a>&nbsp;",
                url,
                HttpUtility.HtmlAttributeEncode(toolTip),
                text);
        }

        public void AddLinkButton(string argument, string text, string toolTip, string question)
        {
            _sbMessage.AppendFormat(
                "&nbsp;<a href=\"post\" title=\"{0}\" class=\"font13\" onclick=\"if(confirm('{1}')) {2}; return false;\">{3}</a>&nbsp;",
                HttpUtility.HtmlAttributeEncode(toolTip),
                HttpUtility.HtmlAttributeEncode(question),
                Page.ClientScript.GetPostBackEventReference(Control, argument),
                text);
        }

        public void AddLinkButton(string argument, string text, string toolTip)
        {
            _sbMessage.AppendFormat(
                "&nbsp;<a href=\"post\" title=\"{0}\" class=\"font13\" onclick=\"{1}; return false;\">{2}</a>&nbsp;",
                HttpUtility.HtmlAttributeEncode(toolTip),
                Page.ClientScript.GetPostBackEventReference(Control, argument),
                text);
        }
    }
}