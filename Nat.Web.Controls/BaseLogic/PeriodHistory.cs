/*
 * Created by: Sergey V. Shpakovskiy
 * Created: 23 ����� 2009 �.
 * Copyright � JSC New Age Technologies 2009
 */

namespace Nat.Web.Controls.BaseLogic
{
    using System;
    using System.Collections.Generic;
    using System.Web.UI;

    using Nat.Web.Controls.GenerationClasses;

    public class PeriodHistory : BaseLogic
    {
        public override void Logic()
        {
            if (IsStartAction && (IsEdit || IsNew))
            {
                var property = ControlInfo.GetType().GetProperty("DateStart") ?? ControlInfo.GetType().GetProperty("dateStart");
                if (property == null)
                    throw new Exception("Logic 'PeriodHistory' can not found property DateStart");

                property.SetValue(ControlInfo, DateTime.Now, null);
            }
        }

        public override void InitClientManagmentControl(ICollection<BaseClientManagmentControl> managmentControls)
        {
            if (!IsNew) return;
            var property = ControlInfo.GetType().GetProperty("DateEndControl") ?? ControlInfo.GetType().GetProperty("dateEndControl");
            if (property == null)
                throw new Exception("Logic 'PeriodHistory' can not found property DateEndControl");

            var item = new BaseClientManagmentControl();
            var control = (Control)property.GetValue(ControlInfo, null);
            var enableItem = new EnableItem { TargetControl = control };
            enableItem.EnableItems.Items.Add(new EnableByEmpty { Disable = true });
            item.EnableItems.Add(enableItem);
            managmentControls.Add(item);
        }
    }
}