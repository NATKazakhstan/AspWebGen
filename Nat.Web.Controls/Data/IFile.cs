namespace Nat.Tools.Data.DataContext
{
    public interface IFile
    {
        byte[] GetData();
        string FileName { get; }
    }
}