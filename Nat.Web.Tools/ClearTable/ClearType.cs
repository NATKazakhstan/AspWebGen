namespace Nat.Web.Tools
{
    /// <summary>
    /// Тип чистки данных.
    /// </summary>
    public enum ClearType
    {
        /// <summary>
        /// Тип очистки не установлен.
        /// </summary>
        NotSet,
        /// <summary>
        /// Все чистить.
        /// </summary>
        All,
        /// <summary>
        /// Не чистить.
        /// </summary>
        Not,
        /// <summary>
        /// Удолять строку из таблици, если нет дочерних строк.
        /// </summary>
        ChildsNotExist,
        /// <summary>
        /// Удолять строку из таблици, если нет родительских строк.
        /// </summary>
        ParentNotExist
    }
}