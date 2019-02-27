using System;
using System.Collections.Generic;
using System.Data.Linq;
using System.Linq;
using System.Linq.Expressions;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.Web;
using System.Web.Compilation;
using System.Web.UI;
using Nat.Web.Controls.GenerationClasses;
using Nat.Web.Controls.GenerationClasses.BaseJournal;
using Nat.Web.Controls.GenerationClasses.Navigator;
using Nat.Tools.Data;

namespace Nat.Web.Controls.AggregateConstructions
{
    public abstract class QueryBuilder
    {
        protected abstract object Execute();
        protected abstract void InitializeDataSources();
        protected MainPageUrlBuilder Url { get; set; }
        protected Page Page { get; set; }

        public virtual bool CheckAccess()
        {
            return true;
        }

        public virtual object Execute(MainPageUrlBuilder url, Page page)
        {
            Page = page;
            Url = url;
            InitializeDataSources();
            return Execute();
        }

        public static object Execute(Page page, MainPageUrlBuilder url, string functionClassName)
        {
            if (functionClassName != null)
            {
                var functionClassType = BuildManager.GetType(functionClassName, false, true);
                if (functionClassType != null)
                {
                    var function = (QueryBuilder)Activator.CreateInstance(functionClassType);
                    if (page == null)
                        using (page = new Page())
                            return function.Execute(url, page);
                    return function.Execute(url, page);
                }
            }
            return null;
        }

        public static object Execute(MainPageUrlBuilder url, string functionClassName)
        {
            using (var page = new Page())
                return Execute(page, url, functionClassName);
        }
    }

    public abstract class QueryBuilder<TDataContext, TTable, TRow> : QueryBuilder
        where TDataContext : DataContext, new()
        where TTable : class
        where TRow : BaseRow, new()
    {
        protected TDataContext DB { get; set; }

        protected abstract IQueryable<TRow> MainData { get; }
    }

    public abstract class QueryBuilder<TDataContext, TFilterControl, TKey, TTable, TDataSource, TRow, TJournal, TNavigatorControl, TNavigatorValues, TFilter>
        : QueryBuilder<TDataContext, TTable, TRow>
        where TDataContext : DataContext, new()
        where TKey : struct
        where TTable : class
        where TFilterControl : BaseFilterControl<TKey, TTable>, new()
        where TDataSource : BaseDataSource<TKey, TTable, TDataContext, TRow>
        where TRow : BaseRow, new()
        where TJournal : BaseJournalControl<TKey, TTable, TRow, TDataContext>
        where TNavigatorControl : BaseNavigatorControl<TNavigatorValues>
        where TNavigatorValues : BaseNavigatorValues, new()
        where TFilter : BaseFilter<TKey, TTable, TDataContext>, new()
    {
        protected TJournal Journal { get; set; }
        protected BaseJournalUserControl<TDataContext, TFilterControl, TKey, TTable, TDataSource, TRow, TJournal, TNavigatorControl, TNavigatorValues, TFilter> InnerParentUserControl { get; set; }
        IQueryable<TRow> mainData;

        protected override IQueryable<TRow> MainData => mainData ?? (mainData = InnerParentUserControl.DataSource.BaseView2.GetSelect());

        protected void ClearMainData()
        {
            mainData = null;
        }

        protected override void InitializeDataSources()
        {
            if (InnerParentUserControl != null)
            {
                InnerParentUserControl.Url = Url;
                InnerParentUserControl.InitializeControls();
                Journal = InnerParentUserControl.Journal;
                DB = InnerParentUserControl.DataSource.BaseView2.DB;
            }
        }

        protected virtual Expression<Func<TRow, IQueryable<TCrossResult>>> GetCrossTableExpression<TCrossResult>(
            BaseJournalCrossTable<TRow> crossTable)
            where TCrossResult : class
        {
            var paramTRow = Expression.Parameter(typeof(TRow), "qbTRow");
            var dbExp = Expression.Constant(DB);
            var exp = (Expression<Func<TDataContext, TRow, IQueryable<TCrossResult>>>)
                      crossTable.GetSelectExpressionInner();
            return Expression.Lambda<Func<TRow, IQueryable<TCrossResult>>>(Expression.Invoke(exp, dbExp, paramTRow), paramTRow);
        }

        /*
        protected virtual Expression<Func<TRow, TResult>> GetCrossTableExpression<TCrossResult, TResult>(
            BaseJournalCrossTable<TRow> crossTable,
            Expression<Func<IQueryable<TCrossResult>, TResult>> crossExpression)
            where TCrossResult : class
        {
            var paramTRow = Expression.Parameter(typeof(TRow), "qbTRow");
            var dbExp = Expression.Constant(DB);
            var crossDataExp = Expression.Invoke(crossTable.GetSelectExpressionInner(), dbExp, paramTRow);//IQueryable<TCrossResult>
            var exp = Expression.Invoke(crossExpression, crossDataExp);
            return Expression.Lambda<Func<TRow, TResult>>(exp, paramTRow);
        }*/

        protected virtual Expression<Func<TRow, TResult>> GetCrossTableExpression<TCrossResult, TResult>(
            BaseJournalCrossTable<TRow> crossTable,
            Expression<Func<IQueryable<TCrossResult>, TResult>> crossExpression)
            where TCrossResult : class
        {
            var paramTRow = Expression.Parameter(typeof(TRow), "qbTRow");
            var dbExp = Expression.Constant(DB);
            var crossDataExp = Expression.Invoke(crossTable.GetSelectExpressionInner(), dbExp, paramTRow);//IQueryable<TCrossResult>
            var exp = Expression.Invoke(crossExpression, crossDataExp);
            return Expression.Lambda<Func<TRow, TResult>>(exp, paramTRow);
        }
    }
}
