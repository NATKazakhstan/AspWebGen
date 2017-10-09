/*
 * Created by : Daniil Kovalev
 * Created    : 28.11.2007
 */

using System;
using System.Collections.Generic;
using Nat.Controls.DataGridViewTools;
using Nat.Tools.QueryGeneration;

namespace Nat.Web.Controls.Filters
{
    /// <summary>
    /// Stores the list of columnFilterStorages
    /// </summary>
    [Serializable]
    public class ColumnFilterStorageList : List<ColumnFilterStorage>
    {
        /// <summary>
        /// Returns QueryConditionList of stored columnFilterStorages
        /// </summary>
        public QueryConditionList QueryConditions
        {
            get
            {
                QueryConditionList queryConditions = new QueryConditionList();
                foreach(ColumnFilterStorage columnFilterStorage in this)
                {
                    QueryConditionList queryConditionList = columnFilterStorage.GetQueryConditions();
                    if(queryConditionList != null)
                        queryConditions.AddRange(queryConditionList);
                }
                return queryConditions;
            }
        }
    }
}