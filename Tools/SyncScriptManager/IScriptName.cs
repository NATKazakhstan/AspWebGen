namespace SyncScriptManager
{
    public interface IScriptName
    {
        string GetName();
        bool MustHave { get; set; }
        bool SkipExecution { get; set; }
    }
}