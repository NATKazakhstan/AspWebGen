namespace Nat.Web.Controls.ExtNet
{
    using System;
    using System.Web;

    using Ext.Net;

    using Nat.Web.Controls.Properties;

    public class AdditionalButtons
    {
        private readonly Action<AbstractComponent> addButton;

        private readonly Action<AbstractComponent, AbstractComponent> insertAfterButton;

        public AdditionalButtons(Action<AbstractComponent> addButton)
        {
            if (addButton == null)
                throw new ArgumentNullException("addButton");

            this.addButton = addButton;
        }

        public AdditionalButtons(Action<AbstractComponent> addButton, Action<AbstractComponent, AbstractComponent> insertAfterButton)
        {
            if (addButton == null)
                throw new ArgumentNullException("addButton");
            if (insertAfterButton == null)
                throw new ArgumentNullException("insertAfterButton");

            this.addButton = addButton;
            this.insertAfterButton = insertAfterButton;
        }

        public ToolbarSeparator AddSeparator()
        {
            var separator = new ToolbarSeparator();
            addButton(separator);
            return separator;
        }

        public Button AddLinkButton(
            string id,
            ComponentDirectEvent.DirectEventHandler directClick,
            string text = "",
            string toolTip = "",
            Icon icon = Icon.None,
            string onclientClick = "",
            string confirmMessage = "")
        {
            if (string.IsNullOrEmpty(id))
                throw new ArgumentNullException("id");

            if (string.IsNullOrEmpty(text) && icon == Icon.None)
                throw new ArgumentNullException("text", "Должен быть заполенен хотябы один параметр text или icon");

            if (string.IsNullOrEmpty(text) && string.IsNullOrEmpty(toolTip))
            {
                throw new ArgumentNullException(
                    "toolTip", "Должен быть заполенен хотябы один параметр toolTip или text");
            }

            var button = new Button
                {
                    ID = id,
                    Text = text,
                    Icon = icon,
                    ToolTip = toolTip,
                    OnClientClick = onclientClick,
                };
            button.DirectEvents.Click.Event += directClick;
            button.DirectEvents.Click.EventMask.ShowMask = true;
            if (!string.IsNullOrEmpty(confirmMessage))
            {
                button.DirectEvents.Click.Confirmation.ConfirmRequest = true;
                button.DirectEvents.Click.Confirmation.Message = confirmMessage;
                button.DirectEvents.Click.Confirmation.Title = Resources.SConfirmTitleText;
            }

            addButton(button);

            return button;
        }

        public HyperLink AddHyperLink(
            string url,
            string text = "",
            Icon icon = Icon.None,
            string toolTip = "",
            string target = "")
        {
            if (string.IsNullOrEmpty(url))
                throw new ArgumentNullException("url");

            if (string.IsNullOrEmpty(text) && icon == Icon.None)
                throw new ArgumentNullException("text", "Должен быть заполенен хотябы один параметр text или icon");

            if (string.IsNullOrEmpty(text) && string.IsNullOrEmpty(toolTip))
            {
                throw new ArgumentNullException(
                    "toolTip", "Должен быть заполенен хотябы один параметр toolTip или text");
            }

            var hyperLink = new HyperLink
                {
                    NavigateUrl = url,
                    Text = text,
                    Icon = icon,
                    ToolTip = toolTip,
                    Target = target,
                };

            addButton(hyperLink);
            return hyperLink;
        }

        public void Add(AbstractComponent button)
        {
            if (button == null)
                throw new ArgumentNullException("button");

            addButton(button);
        }

        public void InsertAfter(AbstractComponent button, AbstractComponent afterComponent)
        {
            if (button == null)
                throw new ArgumentNullException("button");

            if (afterComponent == null)
                throw new ArgumentNullException("afterComponent");

            insertAfterButton(button, afterComponent);
        }
    }
}