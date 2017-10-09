namespace Nat.Web.Controls.Adapters
{
    using System;
    using System.Web.UI.WebControls;

    public class HiddenFieldAdapter : HiddenField
    {
        public HiddenFieldAdapter(Func<string> getValue, Action<string> setValue)
        {
            GetValue = getValue;
            SetValue = setValue;
        }

        public Func<string> GetValue { get; private set; }

        public Action<string> SetValue { get; private set; }

        public override string Value
        {
            get { return GetValue(); }
            set { SetValue(value); }
        }
    }
}