namespace Nat.Web.Controls.GenerationClasses
{
    using System.Data.Linq;
    using System.Linq;

    public interface IBaseFilterControl
    {
        string GetTableName();

        void SetUrl(MainPageUrlBuilder url);

        IQueryable SetFilters(IQueryable source);

        void SetDB(DataContext db);
    }
}