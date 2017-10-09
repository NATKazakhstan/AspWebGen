using System;
using System.Collections.Generic;

namespace Nat.Web.Controls.GenerationClasses.Data
{
    using System.Linq;

    public class SimpleCache
    {
        private Dictionary<string, KeyValuePair<DateTime, object>> _data =
            new Dictionary<string, KeyValuePair<DateTime, object>>();

        private object _lock = new object();
        private DateTime nextCheck;

        public SimpleCache()
        {
            CacheTime = TimeSpan.FromSeconds(30);
        }

        public TimeSpan CacheTime { get; set; }

        public object this[string key]
        {
            get
            {
                KeyValuePair<DateTime, object> value;
                lock (_lock)
                {
                    EnsureRemoved();
                    if (!_data.ContainsKey(key))
                        return null;

                    value = _data[key];
                    if (value.Key < DateTime.Now)
                    {
                        _data.Remove(key);
                        return null;
                    }
                }

                return value.Value;
            }

            set
            {
                lock (_lock)
                {
                    _data[key] = new KeyValuePair<DateTime, object>(DateTime.Now.Add(CacheTime), value);
                }
            }
        }

        public void Remove(string key)
        {
            lock (_lock)
                if (_data.ContainsKey(key))
                    _data.Remove(key);
        }

        private void EnsureRemoved()
        {
            var dateTime = DateTime.Now;
            if (nextCheck > dateTime)
                return;

            nextCheck = dateTime.AddMinutes(1);
            var removeList = _data.Where(r => r.Value.Key < dateTime).Select(r => r.Key).ToList();
            foreach (var key in removeList)
                _data.Remove(key);
        }
    }
}