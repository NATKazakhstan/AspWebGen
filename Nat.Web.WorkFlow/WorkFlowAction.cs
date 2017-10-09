using System;
using System.Data.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;
using Nat.Web.Controls;
using Nat.Web.Controls.GenerationClasses;
using Nat.Web.Tools.WorkFlow;

namespace Nat.Web.WorkFlow
{
    public delegate string WorkFlowActionGetArguments<in TRow>(TRow row);

    public delegate void WorkFlowActionExecute<in TDataContext, in T>(T value, TDataContext db, WorkFlowActionEventArgs args)
        where TDataContext : DataContext;

    public abstract class WorkFlowAction : IWorkFlowAction
    {
        protected WorkFlowAction()
        {
            AutoInitializeTransaction = true;
        }

        public virtual string ArgumentName { get; set; }

        public virtual int OrderIndex { get; set; }

        public virtual string OnClickQuestion { get; set; }

        public virtual string ResultMessage { get; set; }

        public virtual string ActionName { get; set; }

        public virtual string ToolTip { get; set; }

        public virtual string Group { get; set; }

        public virtual string ImgUrl { get; set; }

        public virtual bool GenerateOnlyInRow { get; set; }

        public virtual bool MultipleSelect { get; set; }

        public bool AutoInitializeTransaction { get; set; }

        public string[] Roles { get; set; }

        public abstract string GetArguments(string selectedKey, object row);

        protected virtual bool IsVisibleForRow(IWorkFlow wf, string selectedKey, object row)
        {
            return true;
        }

        public virtual void Render(HtmlTextWriter htmlWriter, IWorkFlow wf, bool forCell, string selectedKey, object row)
        {
            RenderLink(htmlWriter, wf, forCell, selectedKey, row);
        }

        protected virtual bool RenderLink(HtmlTextWriter writer, IWorkFlow wf, bool forCell, string selectedKey, object row)
        {
            if (forCell && string.IsNullOrEmpty(ImgUrl)) return false;
            if (!forCell && GenerateOnlyInRow) return false;
            if (forCell && !IsVisibleForRow(wf, selectedKey, row)) return false;

            var postBackScript = GetPostBackScript(wf, forCell, selectedKey, row);
            writer.RenderHyperLink(
                new BaseHyperLink
                    {
                        Text = ActionName,
                        ToolTip = ToolTip,
                        ImgUrl = forCell ? ImgUrl : null,
                        Url = GetActionUrl(wf, forCell, selectedKey, row),
                        OnClick = postBackScript,
                        OnClickQuestion = OnClickQuestion,
                        RenderAsButton = true,
                        Width = forCell ? new Unit(20, UnitType.Pixel) : new Unit(),
                        Height = forCell ? new Unit(20, UnitType.Pixel) : new Unit(),
                    });
            return true;
        }

        protected virtual string GetPostBackScript(IWorkFlow wf, bool forCell, string selectedKey, object row)
        {
            var arguments = GetArguments(selectedKey, row);
            var argument = ArgumentName + ((MultipleSelect && !forCell) || string.IsNullOrEmpty(arguments) ? string.Empty : ":" + arguments);
            return wf.Page.ClientScript.GetPostBackEventReference(wf.ControlForPostBack, argument);
        }

        protected virtual string GetActionUrl(IWorkFlow wf, bool forCell, string selectedKey, object row)
        {
            return "javascript:void(0);";
        }

        public abstract bool ExecuteAction(object value, WorkFlowActionEventArgs args, DataContext db);

        public virtual void RenderStaticHtml(HtmlTextWriter writer, IWorkFlow wf)
        {
        }
    }

    public abstract class WorkFlowAction<TTable, TDataContext, TRow> : WorkFlowAction
        where TRow : class 
    {
        public abstract string GetArguments(TRow row);

        public abstract bool ExecuteAction(string value, string[] arguments, TDataContext db, IWorkFlow wf);

        public override string GetArguments(string selectedKey, object row)
        {
            if (row == null) return selectedKey;
            return GetArguments((TRow)row);
        }
    }

    public class WorkFlowAction<TTable, TDataContext, TRow, T> : WorkFlowAction<TTable, TDataContext, TRow>
        where TRow : class
        where TDataContext : DataContext
    {
        public WorkFlowActionExecute<TDataContext, T> ExecuteFunction { get; set; }

        public WorkFlowActionGetArguments<TRow> GetArgumentsFunction { get; set; }

        protected virtual T ConvertValue(string value)
        {
            var type = typeof(T);
            if (LinqFilterGenerator.IsNullableType(type))
                type = type.GetGenericArguments()[0];
            return (T)Convert.ChangeType(value, type);
        }

        public override bool ExecuteAction(string value, string[] arguments, TDataContext db, IWorkFlow wf)
        {
            return ExecuteAction(ConvertValue(value), arguments, db, wf);
        }

        public override string GetArguments(TRow row)
        {
            if (GetArgumentsFunction == null)
                return null;
            return GetArgumentsFunction(row);
        }

        public virtual bool ExecuteAction(T value, string[] arguments, TDataContext db, IWorkFlow wf)
        {
            if (ExecuteFunction == null)
                throw new ArgumentNullException(string.Empty, "ExecuteFunction is not seted and ExecuteAction is not overrided");

            var args = new WorkFlowActionEventArgs { Arguments = arguments, WorkFlow = wf, };
            ExecuteFunction(value, db, args);
            ResultMessage = args.ResultMessage;
            return args.Successfull;
        }

        public override bool ExecuteAction(object value, WorkFlowActionEventArgs args, DataContext db)
        {
            if (ExecuteFunction == null)
                throw new ArgumentNullException(string.Empty, "ExecuteFunction is not seted and ExecuteAction is not overrided");

            ExecuteFunction((T)value, (TDataContext)db, args);
            ResultMessage = args.ResultMessage;
            return args.Successfull;
        }
    }
}