namespace Nat.Web.Controls.ExtNet.Generation
{
    using System.Collections.Generic;
    using System.Web;

    using Nat.Web.Tools;
    using Ext.Net;

    public abstract class AbstractUserControl : GenerationClasses.AbstractUserControl
    {
        static AbstractUserControl()
        {
            ShowWarningMessage = (title, message) => X.Msg.Show(new MessageBoxConfig
            {
                Title = title,
                Message = message,
                Buttons = MessageBox.Button.OK,
                Icon = MessageBox.Icon.WARNING,
            });
        }

        public static void NotifyInfoMessage(string title, string message)
        {
            var count = (int?)HttpContext.Current.Items["Nat.Web.Controls.ExtNet.Generation.NotifyInfoMessage.Count"] ?? 0;
            X.Msg.Notify(
                new NotificationConfig
                {
                    AutoHide = true,
                    Resizable = true,
                    Closable = true,
                    Modal = false,
                    Icon = Icon.Information,
                    Html = message,
                    AlignCfg = new NotificationAlignConfig
                    {
                        ElementAnchor = AnchorPoint.BottomLeft,
                        TargetAnchor = AnchorPoint.BottomLeft,
                        OffsetX = 20,
                        OffsetY = -20,
                    },
                    //ShowFx = new SlideIn { Options = new FxConfig { Delay = count * 500 } },
                    Width = 250,
                    BodyStyle = "height: auto; padding: 8px",
                    HideDelay = 3000,
                }).Show();
            HttpContext.Current.Items["Nat.Web.Controls.ExtNet.Generation.NotifyInfoMessage.Count"] = ++count;
        }

        public static void NotifyErrorMessage(string title, string message)
        {
            var count = (int?)HttpContext.Current.Items["Nat.Web.Controls.ExtNet.Generation.NotifyErrorMessage.Count"] ?? 0;
            X.Msg.Notify(
                new NotificationConfig
                {
                    AutoHide = false,
                    Resizable = true,
                    Closable = true,
                    Modal = false,
                    Icon = Icon.Error,
                    Html = message,
                    AlignCfg = new NotificationAlignConfig
                    {
                        ElementAnchor = AnchorPoint.BottomLeft,
                        TargetAnchor = AnchorPoint.BottomLeft,
                        OffsetX = 20,
                        OffsetY = -20,
                    },
                    //ShowFx = new SlideIn { Options = new FxConfig { Delay = count * 500 } },
                    Width = 250,
                    BodyStyle = "height: auto; padding: 8px",
                }).Show();
            HttpContext.Current.Items["Nat.Web.Controls.ExtNet.Generation.NotifyErrorMessage.Count"] = ++count;
        }

        protected virtual void AddInfoMessage(string title, string message)
        {
            NotifyInfoMessage(title, message);
        }

        protected override void AddInfoMessage(string message)
        {
            NotifyInfoMessage(Web.Controls.Properties.Resources.SInfoMessageTitle, message);
        }

        protected virtual void AddErrorMessage(string title, string message)
        {
            NotifyErrorMessage(title, message);
        }

        protected override void AddErrorMessage(string message)
        {
            NotifyErrorMessage(Web.Controls.Properties.Resources.SErrorMessageTitle, message);
        }

        protected override void OnPreRenderExt()
        {
            base.OnPreRenderExt();

            if (!IsPostBack)
            {
                if (LocalizationHelper.IsCultureKZ)
                {
                    ResourceManager.AddInstanceScript(Properties.ResourceFiles.ExtLocaleKz);
                }
                ResourceManager.AddInstanceScript(Properties.ResourceFiles.GlobalScripts);
            }
        }
    }

    public abstract class AbstractUserControl<TKey> : GenerationClasses.AbstractUserControl<TKey>
        where TKey : struct
    {
        protected virtual void AddInfoMessage(string title, string message)
        {
            AbstractUserControl.NotifyInfoMessage(title, message);
        }

        protected override void AddInfoMessage(string message)
        {
            AbstractUserControl.NotifyInfoMessage(Web.Controls.Properties.Resources.SInfoMessageTitle, message);
        }

        protected virtual void AddErrorMessage(string title, string message)
        {
            AbstractUserControl.NotifyErrorMessage(title, message);
        }

        protected override void AddErrorMessage(string message)
        {
            AbstractUserControl.NotifyErrorMessage(Web.Controls.Properties.Resources.SErrorMessageTitle, message);
        }

        protected bool AddErrorMessage(IEnumerable<string> messages)
        {
            var addErrors = false;
            foreach (var message in messages)
            {
                addErrors = true;
                AbstractUserControl.NotifyErrorMessage(Web.Controls.Properties.Resources.SErrorMessageTitle, message);
            }

            return addErrors;
        }

        protected override void OnPreRenderExt()
        {
            base.OnPreRenderExt();

            if (!IsPostBack)
            {
                if (LocalizationHelper.IsCultureKZ)
                {
                    ResourceManager.AddInstanceScript(Properties.ResourceFiles.ExtLocaleKz);
                }
                ResourceManager.AddInstanceScript(Properties.ResourceFiles.GlobalScripts);
            }
        }
    }
}