/*
* Created by: Sergey V. Shpakovskiy
* Created: 2012.09.17
* Copyright © JSC NAT Kazakhstan 2012
*/

using Nat.Web.Controls;

namespace Nat.Web.Tools.WorkFlow
{
    using System;
    using System.Collections.Generic;
    using System.Data.Linq;
    using System.Text;
    using System.Web.UI;
    using System.Web.UI.WebControls;

    using Nat.Web.Controls.GenerationClasses;

    public interface IWorkFlow
    {
        bool HasActions { get; }
        Page Page { get; set; }
        Control ControlForPostBack { get; set; }
        
        Action EnsureLogicExecute { get; set; }
        Action SaveData { get; set; }
        Action<bool> UpdateForm { get; set; }
        Action<object, object> UpdatingRow { get; set; }

        IEnumerable<IWorkFlowActionResult> ExecuteAction(string argument);
        IEnumerable<IWorkFlowAction> GetActions(string selectedKey);
        void GetHtmlOfAction(StringBuilder sb, string selectedKey, object row, bool forCell);
        void GetStaticHtmlOfAction(StringBuilder sb);
        void OnUpdating(object originalRow, object newRow);
        void OnInserting(object newRow);
        bool OnUpdated(object row, string newKey);
        bool OnInserted(object row);
        DataContext DataContext { get; set; }
        AbstractUserControl JournalControl { get; set; }
        BaseControlInfo BaseControlInfo { get; set; }
        ExtenderAjaxControl ExtenderAjaxControl { get; set; }
        MainPageUrlBuilder Url { get; set; }
        void SetFilters(BaseFilterEventArgs filterArgs);
        void SetEditFilters(BaseFilterEventArgs filterArgs);
        void SetDeleteFilters(BaseFilterEventArgs filterArgs);
        void SetAddChildsFilters(BaseFilterEventArgs filterArgs);
        void SetDefaultFilter(DefaultFilters defaultFilters);
        void InitializeAddLinks(WebControl link1, WebControl link2);
        void SetValidation(Dictionary<Control, ValidateInformation> validationInfo, string selectedKey);
        void SetValidation(ValidateInformationForm validationInfo, string selectedKey);

        void InitializeGridColumns(BaseGridColumns gridColumns);

        void InitializeGroupColumns(List<List<GridHtmlGenerator.Column>> groupColumns, BaseGridColumns gridColumns);

        void InitializeClientManagmentControl(ICollection<BaseClientManagmentControl> managmentControls, Func<BaseClientManagmentControl> createClientManagmentControl);

        void BeforeRowDeleted(object row, object contextInfo);

        void DeleteRow(object row, object contextInfo);

        void AfterRowDeleted(object row, object contextInfo);

        void InsertLogic();
        
        void EditLogic();
        
        void ReadLogic();
        
        void InitControlPanels();
    }

    public interface IWorkFlow<TTable, TDataContext, TRow, TKey, TStatus, TControlInfo> : IWorkFlow
        where TRow : class
        where TTable : class
        where TDataContext : DataContext
        where TControlInfo : BaseControlInfo
        where TKey : struct
    {
        Func<TTable> GetRenderItem { get; set; }

        Func<TRow> GetRenderRow { get; set; }
    }
}