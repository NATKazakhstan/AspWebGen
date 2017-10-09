/*
 * Created by: Sergey V. Shpakovskiy
 * Created: 23 марта 2009 г.
 * Copyright © JSC New Age Technologies 2009
 */
using System;
using System.Web.UI;
using System.Collections.Generic;
using Nat.Web.Controls.GenerationClasses;
using Nat.Web.Controls.GenerationClasses.Navigator;

namespace Nat.Web.Controls.BaseLogic
{
    public interface ILogic
    {
        object ControlInfo { get; set; }
        MainPageUrlBuilder Url { get; set; }
        bool IsNew { get; set; }
        bool IsEdit { get; set; }
        bool IsStartAction { get; set; }
        bool IsMultipleLogic { get; set; }
        bool SupportMultipleSelect { get; }
        IDictionary<Type, SelectedParameterNavigator.ItemNavigator> ItemNavigators { get; set; }
        BaseNavigatorValues NavigatorValues { get; set; }
        void Logic();
        IList<string> Validate();
        void InitClientManagmentControl(ICollection<BaseClientManagmentControl> managmentControls);
        void OnPreRender();
        void OnRender();
        void BeforeSave();
        void BeforeSave(object row);
        IList<string> BeforeSaveExec(object row);
        IList<string> AfterSave(bool saved);
        IList<string> AfterSave(bool saved, object savedRow);
        void GetValidators(ValidateInformation validateInformation, Control control);
    }
}