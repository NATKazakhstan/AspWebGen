namespace Nat.Web.Tools
{
    using System;
    using System.Web.UI;

    using Nat.Web.Controls;

    public class AccessControl : IAccessControl
    {
        private readonly Func<bool> checkPermit;

        public AccessControl(Func<bool> checkPermit)
        {
            this.checkPermit = checkPermit;
        }

        #region Implementation of IAccessControl

        public bool CheckPermit(Page page)
        {
            return checkPermit();
        }

        #endregion
    }
}