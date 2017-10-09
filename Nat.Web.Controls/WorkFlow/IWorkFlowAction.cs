/*
* Created by: Sergey V. Shpakovskiy
* Created: 2012.09.17
* Copyright © JSC NAT Kazakhstan 2012
*/

namespace Nat.Web.Tools.WorkFlow
{
    using System.Web.UI;

    public interface IWorkFlowAction
    {
        string Group { get; }

        string ArgumentName { get; }

        string ActionName { get; }

        int OrderIndex { get; }

        string[] Roles { get; }

        bool MultipleSelect { get; }

        string GetArguments(string selectedKey, object row);
        
        void Render(HtmlTextWriter writer, IWorkFlow wf, bool forCell, string selectedKey, object row);

        void RenderStaticHtml(HtmlTextWriter writer, IWorkFlow wf);
    }
}