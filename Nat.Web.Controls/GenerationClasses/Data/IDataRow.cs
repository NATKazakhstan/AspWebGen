namespace Nat.Web.Controls.GenerationClasses
{
    public interface IDataRow
    {
        /// <summary>
        /// Ключевое значение.
        /// </summary>
        string Value { get; }

        /// <summary>
        /// Наименование на русском.
        /// </summary>
        string nameRu { get; }

        /// <summary>
        /// Наименование на казахском.
        /// </summary>
        string nameKz { get; }

        /// <summary>
        /// Наименование с учетом текущей локализации.
        /// </summary>
        string Name { get; }

        bool CanAddChild { get; }

        bool CanEdit { get; }

        bool CanDelete { get; }

        string[] GetAdditionalValues(SelectParameters selectParameters);
    }
}
