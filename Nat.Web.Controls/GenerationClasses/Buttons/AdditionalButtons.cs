/*
 * Created by: Sergey V. Shpakovskiy
 * Created: 12 ноября 2008 г.
 * Copyright © JSC New Age Technologies 2008
 */

namespace Nat.Web.Controls.GenerationClasses
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Web;
    using System.Web.Script.Serialization;
    using System.Web.UI;

    public class AdditionalButtons
    {
        private readonly Page page;
        private readonly Control control;
        private readonly ExtenderAjaxControl extenderAjaxControl;
        private readonly StringBuilder sb = new StringBuilder();

        public Control Control
        {
            get { return control; }
        }

        public AdditionalButtons()
        {
        }

        public AdditionalButtons(Page page, Control control)
        {
            this.page = page;
            this.control = control;
        }

        public AdditionalButtons(Page page, Control control, ExtenderAjaxControl extenderAjaxControl)
        {
            this.page = page;
            this.control = control;
            this.extenderAjaxControl = extenderAjaxControl;
        }

        public string CurrentArgument { get; set; }

        public Page Page => page;

        public void AddCustom(string html)
        {
            sb.Append(html);
        }

        public void AddCustom(Action<StringBuilder> action)
        {
            action(sb);
        }

        public void AddHyperLink(string url, string text, string toolTip)
        {
            sb.AppendFormat(
                "<a href=\"{0}\" title=\"{1}\" class=\"linkAsButton\">{2}</a>",
                url,
                HttpUtility.HtmlAttributeEncode(toolTip),
                text);
        }

        public void AddHyperLinkWithTarget(string url, string text, string toolTip, string target)
        {
            sb.AppendFormat(
                "<a href=\"{0}\" title=\"{1}\" class=\"linkAsButton\" target=\"{3}\">{2}</a>",
                url,
                HttpUtility.HtmlAttributeEncode(toolTip),
                text,
                target);
        }

        public void AddHyperLink(string url, string text, string toolTip, string question)
        {
            sb.AppendFormat(
                "<a href=\"{0}\" title=\"{1}\" class=\"linkAsButton\" onclick=\"return confirm('{3}')\">{2}</a>",
                url,
                HttpUtility.HtmlAttributeEncode(toolTip),
                text,
                HttpUtility.HtmlAttributeEncode(question));
        }

        public void AddHyperLinkWithClick(string url, string text, string toolTip, string onclick)
        {
            sb.AppendFormat(
                "<a href=\"{0}\" title=\"{1}\" class=\"linkAsButton\" onclick=\"{3}\">{2}</a>",
                url,
                HttpUtility.HtmlAttributeEncode(toolTip),
                text,
                onclick);
        }

        public void AddLinkButton(string argument, string text, string toolTip)
        {
            sb.AppendFormat(
                "<a href=\"javascript:{1}\" title=\"{0}\" class=\"linkAsButton\">{2}</a>",
                HttpUtility.HtmlAttributeEncode(toolTip),
                page.ClientScript.GetPostBackEventReference(control, CurrentArgument + argument),
                text);
        }

        public void AddImageButton(string argument, string url, string toolTip)
        {
            sb.AppendFormat(
                "<a href=\"javascript:{1}\" title=\"{0}\"><img src=\"{2}\" alt=\"{0}\" border=0/></a>",
                HttpUtility.HtmlAttributeEncode(toolTip),
                page.ClientScript.GetPostBackEventReference(control, CurrentArgument + argument),
                url);
        }

        public void AddLinkButtonWithout(string argument, string text, string toolTip)
        {
            sb.AppendFormat(
                "<a href=\"javascript:{1}; EndRequest();\" title=\"{0}\" class=\"linkAsButton\">{2}</a>",
                HttpUtility.HtmlAttributeEncode(toolTip),
                page.ClientScript.GetPostBackEventReference(control, CurrentArgument + argument),
                text);
        }

        public void AddLinkButton(string argument, string text, string toolTip, string question)
        {
            sb.AppendFormat(
                "<a href=\"javascript:if(confirm('{1}')) {2};\" title=\"{0}\" class=\"linkAsButton\">{3}</a>",
                HttpUtility.HtmlAttributeEncode(toolTip),
                HttpUtility.HtmlAttributeEncode(question),
                page.ClientScript.GetPostBackEventReference(control, CurrentArgument + argument),
                text);
        }

        public void AddLinkButtonValidation(string argument, string text, string toolTip, string validationGroup)
        {
            sb.AppendFormat(
                "<a href=\"javascript:{1}\" title=\"{0}\" class=\"linkAsButton\">{2}</a>",
                HttpUtility.HtmlAttributeEncode(toolTip),
                page.ClientScript.GetPostBackEventReference(
                    new PostBackOptions(control, CurrentArgument + argument)
                        {
                            ValidationGroup = validationGroup,
                            PerformValidation = true,
                        }),
                text);
        }

        public void AddLinkButtonValidation(
            string argument, string text, string toolTip, string question, string validationGroup)
        {
            sb.AppendFormat(
                "<a href=\"javascript:if(confirm('{1}')) {2};\" title=\"{0}\" class=\"linkAsButton\">{3}</a>",
                toolTip,
                question,
                page.ClientScript.GetPostBackEventReference(
                    new PostBackOptions(control, CurrentArgument + argument)
                        {
                            ValidationGroup = validationGroup,
                            PerformValidation = true,
                        }),
                text);
        }

        public void AddLinkButtonValidationWithSkip(
            string argument,
            string text,
            string toolTip,
            string question,
            string questionForContinue,
            string validationGroup)
        {
            var validPostBack = page.ClientScript.GetPostBackEventReference(
                new PostBackOptions(control, CurrentArgument + argument)
                    {
                        ValidationGroup = validationGroup,
                        PerformValidation = true,
                    });
            var withoutValidPostBack = page.ClientScript.GetPostBackEventReference(
                control, CurrentArgument + argument);
            sb.Append("<a href=\"javascript:");
            if (!string.IsNullOrEmpty(question))
                sb.AppendFormat("if(confirm('{0}')){{", HttpUtility.HtmlAttributeEncode(question));
            sb.AppendFormat(
                "{0}; if(!isRequstPerforming && confirm('{2}')){1};",
                HttpUtility.HtmlAttributeEncode(validPostBack),
                HttpUtility.HtmlAttributeEncode(withoutValidPostBack),
                HttpUtility.HtmlAttributeEncode(questionForContinue));
            if (!string.IsNullOrEmpty(question))
                sb.Append("}");
            sb.AppendFormat(
                "\" title=\"{0}\" class=\"linkAsButton\">{1}</a>", HttpUtility.HtmlAttributeEncode(toolTip), text);
        }

        public void AddLinkButtonForArrayQuestionNotValid(
            string argument,
            string text,
            string toolTip,
            string validationGroup,
            string question,
            IEnumerable<string> keys)
        {
            string href = GetHref(keys, validationGroup, CurrentArgument + argument);
            string href2 = GetHref(keys, null, CurrentArgument + argument);
            sb.AppendFormat(
                "<a href=\"javascript:{1};if(!isRequstPerforming && confirm('{4}')){2};\" title=\"{0}\" class=\"linkAsButton\">{3}</a>",
                HttpUtility.HtmlAttributeEncode(toolTip),
                HttpUtility.HtmlAttributeEncode(href),
                HttpUtility.HtmlAttributeEncode(href2),
                HttpUtility.HtmlAttributeEncode(text),
                HttpUtility.HtmlAttributeEncode(question));
        }

        public void AddLinkButtonForArray(
            string argument,
            string text,
            string toolTip,
            string validationGroup,
            string question,
            IEnumerable<string> keys)
        {
            string href = GetHref(keys, validationGroup, CurrentArgument + argument);
            sb.AppendFormat(
                "<a href=\"javascript:if(confirm('{3}')){1};\" title=\"{0}\" class=\"linkAsButton\">{2}</a>",
                HttpUtility.HtmlAttributeEncode(toolTip),
                HttpUtility.HtmlAttributeEncode(href),
                HttpUtility.HtmlAttributeEncode(text),
                HttpUtility.HtmlAttributeEncode(question));
        }

        public void AddLinkButtonForArray(
            string argument, string text, string toolTip, string validationGroup, IEnumerable<string> keys)
        {
            string href = GetHref(keys, validationGroup, CurrentArgument + argument);
            sb.AppendFormat(
                "<a href=\"javascript:{1}\" title=\"{0}\" class=\"linkAsButton\">{2}</a>",
                HttpUtility.HtmlAttributeEncode(toolTip),
                HttpUtility.HtmlAttributeEncode(href),
                HttpUtility.HtmlAttributeEncode(text));
        }

        public void AddLinkButtonForArray(string argument, string text, string toolTip, params string[] keys)
        {
            string href = GetHref(keys, null, CurrentArgument + argument);
            sb.AppendFormat(
                "<a href=\"javascript:{1}\" title=\"{0}\" class=\"linkAsButton\">{2}</a>",
                HttpUtility.HtmlAttributeEncode(toolTip),
                HttpUtility.HtmlAttributeEncode(href),
                HttpUtility.HtmlAttributeEncode(text));
        }

        private string GetHref(IEnumerable<string> keys, string validationGroup, string argument)
        {
            var jss = new JavaScriptSerializer();
            string serializedKeys = jss.Serialize(keys.Where(p => !string.IsNullOrEmpty(p)));
            var postArgument = string.Format("'{0}' + GetSerializedValues('{1}')", CurrentArgument + argument, serializedKeys);
            if (string.IsNullOrEmpty(validationGroup))
            {
                var postBackOptions = new PostBackOptions(control, "{0}") { TrackFocus = true };
                var postBackEventReference = page.ClientScript.GetPostBackEventReference(postBackOptions);
                return postBackEventReference.Replace("\"{0}\"", postArgument).Replace("'{0}'", postArgument);
            }

            var eventReference = page.ClientScript.GetPostBackEventReference(
                new PostBackOptions(control, "{0}")
                    {
                        ValidationGroup = validationGroup,
                        PerformValidation = true,
                        TrackFocus = true,
                    });
            return eventReference.Replace("\"{0}\"", postArgument);
        }

        public string GetHtml()
        {
            return sb.ToString();
        }

        public bool ContainsAny()
        {
            return sb.Length > 0;
        }

        public void AddMenuItem(MenuItem menuItem, string clientID)
        {
            AddMenuItem(menuItem);
        }

        public void AddMenuItem(MenuItem menuItem)
        {
            menuItem.GenerateHtml(sb, "id_" + Guid.NewGuid(), extenderAjaxControl, true);
        }
    }
}