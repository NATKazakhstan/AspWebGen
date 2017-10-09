namespace SubsystemReferencesConflictResolver
{
    public interface IReferencesConflictResolver
    {
        void OnReferenceConflictResolving(TableParametersArgs args);
    }
}