namespace Nat.Web.Controls.GenerationClasses.BaseJournal
{
    using System.Linq.Expressions;

    using Nat.Web.Controls.GenerationClasses.Filter;

    public interface IBaseFilterParameterContainer
    {
        Expression GetFilter(QueryParameters queryParameters);
    }
}