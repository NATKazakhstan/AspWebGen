namespace Nat.Web.Controls
{
    using System.Web.SessionState;

    public abstract class BaseMainPageReadOnly : BaseMainPage, IReadOnlySessionState
    {
    }
}