namespace Nat.Web.Controls
{
    using global::System;
    using global::System.Web.UI.WebControls;

    public class DataPagerFixed : DataPager
    {
        private bool pagerFieldsCreated;
        private bool pagerFieldsCreatedBeforeLoadState;

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            if (pagerFieldsCreatedBeforeLoadState)
                CreatePagerFields();
        }

        protected override void LoadControlState(object savedState)
        {
            base.LoadControlState(savedState);
            if (pagerFieldsCreated)
                pagerFieldsCreatedBeforeLoadState = true;
        }

        protected override void CreatePagerFields()
        {
            base.CreatePagerFields();
            pagerFieldsCreated = true;
        }
    }
}