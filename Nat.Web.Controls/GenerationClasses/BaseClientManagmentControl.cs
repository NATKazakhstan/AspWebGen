/*
 * Created by: Sergey V. Shpakovskiy
 * Created: 30 декабря 2008 г.
 * Copyright © JSC New Age Technologies 2008
 */

using System.Collections.Generic;

namespace Nat.Web.Controls.GenerationClasses
{
    public class BaseClientManagmentControl
    {
        private int lastIndex = -1;

        public BaseClientManagmentControl()
        {
            EnableItems = new List<EnableItem>();
        }

        public IList<EnableItem> EnableItems { get; private set; }

        public void SetForAllItemsEnableMode(EnableMode mode)
        {
            for (lastIndex = 0; lastIndex < EnableItems.Count; lastIndex++)
                EnableItems[lastIndex].EnableMode = mode;
        }

        public void SetForNewItemsEnableMode(EnableMode mode)
        {
            if (lastIndex < 0)
            {
                SetForAllItemsEnableMode(mode);
                return;
            }
            for (; lastIndex < EnableItems.Count; lastIndex++)
                EnableItems[lastIndex].EnableMode = mode;
        }

        public void MoveCurrentIndexPosition()
        {
            lastIndex = EnableItems.Count;
        }
    }
}