using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Collections;

namespace Nat.Web.Controls.GenerationClasses.Navigator
{
    public abstract class BaseNavigatorValues : IDictionary<Type, object>
    {
        protected Dictionary<Type, object> Values = new Dictionary<Type,object>();

        public void InitializeValues(IDictionary<Type, object> values)
        {
            foreach (var item in values)
                values[item.Key] = item.Value;
        }

        public string RefParentKeyInString 
        {
            get
            {
                if (!MainPageUrlBuilder.Current.QueryParameters.ContainsKey("ref" + TableName + "Parent")) return null;
                return MainPageUrlBuilder.Current.QueryParameters["ref" + TableName + "Parent"];
            }
            set
            {
                MainPageUrlBuilder.Current.QueryParameters["ref" + TableName + "Parent"] = value;
            }
        }

        public string RefHistoryKeyInString 
        {
            get
            {
                if (!MainPageUrlBuilder.Current.QueryParameters.ContainsKey("ref" + TableName + "History")) return null;
                return MainPageUrlBuilder.Current.QueryParameters["ref" + TableName + "History"];
            }
            set
            {
                MainPageUrlBuilder.Current.QueryParameters["ref" + TableName + "History"] = value;
            }
        }
        protected virtual string TableName { get { return ""; } }

        #region IDictionary<Type,object> Members

        void IDictionary<Type, object>.Add(Type key, object value)
        {
            Values.Add(key, value);
        }

        public bool ContainsKey(Type key)
        {
            return Values.ContainsKey(key);
        }

        ICollection<Type> IDictionary<Type, object>.Keys
        {
            get { return Values.Keys; }
        }

        bool IDictionary<Type, object>.Remove(Type key)
        {
            return Values.Remove(key);
        }

        bool IDictionary<Type, object>.TryGetValue(Type key, out object value)
        {
            return Values.TryGetValue(key, out value);
        }

        ICollection<object> IDictionary<Type, object>.Values
        {
            get { return Values.Values; }
        }

        public object this[Type key]
        {
            get
            {
                return Values[key];
            }
            set
            {
                Values[key] = value;
            }
        }

        #endregion

        #region ICollection<KeyValuePair<Type,object>> Members

        void ICollection<KeyValuePair<Type, object>>.Add(KeyValuePair<Type, object> item)
        {
            ((ICollection<KeyValuePair<Type, object>>)Values).Add(item);
        }

        void ICollection<KeyValuePair<Type, object>>.Clear()
        {
            Values.Clear();
        }

        bool ICollection<KeyValuePair<Type, object>>.Contains(KeyValuePair<Type, object> item)
        {
            return ((ICollection<KeyValuePair<Type, object>>)Values).Contains(item);
        }

        void ICollection<KeyValuePair<Type, object>>.CopyTo(KeyValuePair<Type, object>[] array, int arrayIndex)
        {
            ((ICollection<KeyValuePair<Type, object>>)Values).CopyTo(array, arrayIndex);
        }

        int ICollection<KeyValuePair<Type, object>>.Count
        {
            get { return Values.Count; }
        }

        bool ICollection<KeyValuePair<Type, object>>.IsReadOnly
        {
            get { return ((ICollection<KeyValuePair<Type, object>>)Values).IsReadOnly; }
        }

        bool ICollection<KeyValuePair<Type, object>>.Remove(KeyValuePair<Type, object> item)
        {
            return ((ICollection<KeyValuePair<Type, object>>)Values).Remove(item);
        }

        #endregion

        #region IEnumerable<KeyValuePair<Type,object>> Members

        IEnumerator<KeyValuePair<Type, object>> IEnumerable<KeyValuePair<Type, object>>.GetEnumerator()
        {
            return Values.GetEnumerator();
        }

        #endregion

        #region IEnumerable Members

        IEnumerator IEnumerable.GetEnumerator()
        {
            return Values.GetEnumerator();
        }

        #endregion
    }
}
