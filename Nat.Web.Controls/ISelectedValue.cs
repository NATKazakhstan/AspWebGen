using System;
using System.Web.UI.WebControls;
using System.Linq.Expressions;
using Nat.Web.Controls.GenerationClasses.Filter;

namespace Nat.Web.Controls
{
    public interface ISelectedValue
    {
        long SelectedValueLong { get; }
        object SelectedValue { get; }
        DataKey SelectedDataKey { get; }
        string[] DataKeyNames { get; set; }
        event EventHandler SelectedIndexChanged;
        string ParentControl { get; set; }
        bool ShowHistory { get; set; }
        bool IsNew { get; set; }
        bool IsSelect { get; set; }
        bool IsRead { get; set; }
        Type TableType { get; }
        void SetParentValue(object value);
        MainPageUrlBuilder Url { get; }
        Expression GetExpression(string reference, Expression param);
        Expression GetExpression(string reference, Expression param, QueryParameters qParams);
    }
}