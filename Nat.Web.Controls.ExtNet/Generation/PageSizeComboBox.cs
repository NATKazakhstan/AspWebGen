namespace Nat.Web.Controls.ExtNet.Generation
{
    using System.Linq;

    using Ext.Net;

    using Nat.Web.Tools;

    public class PageSizeComboBox : ComboBox
    {
        public PageSizeComboBox()
            : base (GetConfig())
        {
            Listeners.Select.Fn = "PageSizeComboBoxSelect";
            Listeners.BoxReady.Fn = "PageSizeComboBoxReady";
        }

        private static Config GetConfig()
        {
            var config = new Config();
            config.Items.AddRange(new[] { 10, 15, 20, 25, 30, 35, 40, 45, 50 }.Select(r => new ListItem(r.ToString(), r)));
            config.Editable = false;
            config.InputWidth = 40;
            config.LabelWidth = LocalizationHelper.IsCultureKZ ? 30 : 15;
            config.FieldLabel = Properties.Resources.SPageSizeLabel;
            config.LabelAlign = LabelAlign.Right;
            config.LabelSeparator = string.Empty;
            config.PaddingSpec = "0 5 0 0";
            return config;
        }
    }
}