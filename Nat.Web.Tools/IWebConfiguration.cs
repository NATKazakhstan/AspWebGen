using System.Configuration;

namespace Nat.Web.Tools
{
    public interface IWebConfiguration
    {
        Configuration WebConfiguration { get; set; }
    }
}