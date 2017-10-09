using System;

namespace Nat.Web.Controls
{
    /// <summary>
    /// –ежим возврата данных.
    /// </summary>
    [Serializable]    
    public enum TableDataSourceDataMode
    {
        /// <summary>
        /// ѕри запросе данных, всегда возвращаютс€ все данные.
        /// </summary>
        All,
        /// <summary>
        /// ѕри запросе данных, возвращаетс€ всегда текуща€ запись.
        /// </summary>
        OnlyCurrent,
        /// <summary>
        /// ѕри запросе данных, возвращаетс€ текуща€ запись, если ее нет, то все данные.
        /// </summary>
        CurrentOrAll
    }
}