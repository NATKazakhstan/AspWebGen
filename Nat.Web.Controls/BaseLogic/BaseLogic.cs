/*
 * Created by: Sergey V. Shpakovskiy
 * Created: 5 θώνÿ 2009 γ.
 * Copyright © JSC New Age Technologies 2009
 */

using System;
using System.Collections.Generic;
using System.Web.UI;
using Nat.Web.Controls.GenerationClasses;
using Nat.Web.Controls.GenerationClasses.Navigator;

namespace Nat.Web.Controls.BaseLogic
{
    public abstract class BaseLogic : ILogic
    {
        public virtual object ControlInfo { get; set; }
        public MainPageUrlBuilder Url { get; set; }
        public bool IsNew { get; set; }
        public bool IsEdit { get; set; }
        public bool IsStartAction { get; set; }
        public bool IsMultipleLogic { get; set; }

        public virtual bool SupportMultipleSelect { get{ return false; } }
        public IDictionary<Type, SelectedParameterNavigator.ItemNavigator> ItemNavigators { get; set; }
        public BaseNavigatorValues NavigatorValues { get; set; }
        public abstract void Logic();
        public virtual IList<string> Validate()
        {
            return null;
        }

        public virtual void InitClientManagmentControl(ICollection<BaseClientManagmentControl> managmentControls)
        {
        }

        public virtual void OnPreRender()
        {
        }

        public virtual void OnRender()
        {
        }

        public virtual void BeforeSave()
        {
        }

        public virtual void BeforeSave(object row)
        {
            BeforeSave();
        }

        public virtual IList<string> BeforeSaveExec(object row)
        {
            BeforeSave(row);
            return null;
        }

        public virtual IList<string> AfterSave(bool saved)
        {
            return null;
        }

        public virtual IList<string> AfterSave(bool saved, object savedRow)
        {
            return AfterSave(saved);
        }

        public virtual void GetValidators(ValidateInformation validateInformation, Control control)
        {
        }
    }
}