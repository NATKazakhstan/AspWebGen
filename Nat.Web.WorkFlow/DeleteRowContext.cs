/*
* Created by: Sergey V. Shpakovskiy
* Created: 2012.08.16
* Copyright © JSC NAT Kazakhstan 2012
*/

namespace Nat.Web.WorkFlow
{
    using System.Collections.Generic;
    using System.Data.Linq;

    using Nat.Web.Controls.GenerationClasses;

    public class DeleteRowContext<TKey>
        where TKey : struct
    {
        public DeleteRowContext()
        {
            ContextInfo = new Dictionary<string, object>();
        }

        public bool Cancel { get; set; }

        public string CancelMessage { get; set; }

        public bool? SuccessfullRowDeleted { get; set; }

        public string FailedRowDeletedMessage { get; set; }

        public DeleteEventArgs<TKey> DeleteEventArgs { get; set; }
        
        private Dictionary<string, object> ContextInfo { get; set; }
            
        public TValue GetContextInfo<TValue>(string key)
        {
            if (!ContextInfo.ContainsKey(key)) return default(TValue);
            return (TValue)ContextInfo[key];
        }

        public void SetContextInfo(string key, object value)
        {
            ContextInfo[key] = value;
        }
    }

    public class DeleteRowContext<TTable, TDataContext, TRow, TKey> : DeleteRowContext<TKey>
        where TRow : class
        where TKey : struct
        where TTable : class
        where TDataContext : DataContext
    {
        public TRow Row { get; set; }
        
        public TKey Key { get; set; }
        
        public TTable TableItem { get; set; }

        public TDataContext DB { get; set; }
    }
}