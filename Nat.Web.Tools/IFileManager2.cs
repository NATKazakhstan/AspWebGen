namespace Nat.Web.Tools
{
    public interface IFileManager2 : IFileManager
    {
        byte[] GetFile(long id, string fieldName);
    }
}