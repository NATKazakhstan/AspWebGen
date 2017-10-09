namespace Nat.Web.Controls.GenerationClasses.Navigator
{
    using System;
    using System.Web;
    using System.Web.UI;

    public abstract class BaseNavigatorCustomControl : UserControl
    {
        public virtual bool AllowCache
        {
            get { return true; }
        }

        public BaseNavigatorValues BaseNavigatorValues { get; set; }

        public string JournalUrl { get; set; }

        public string LookUrl { get; set; }

        public override bool EnableViewState
        {
            get { return false; }
        }

        public abstract void SetValue(string value);

        public abstract void Initialize();
    }

    public abstract class BaseNavigatorCustomControl<TKey, TDataItem> : BaseNavigatorCustomControl
        where TKey : struct 
        where TDataItem : class
    {
        public TKey? KeyValue { get; protected set; }
        
        public TDataItem Item { get; private set; }

        protected abstract TDataItem GetItemInformation(TKey id);

        public override void SetValue(string value)
        {
            if (string.IsNullOrEmpty(value))
                KeyValue = null;
            else
                KeyValue = (TKey?)Convert.ChangeType(value, typeof(TKey));
        }

        protected virtual string CacheKey()
        {
            var baseType = GetType().BaseType;

            if (baseType == typeof(BaseNavigatorCustomControl<TKey, TDataItem>))
                return GetType().FullName;

            if (baseType == null)
                throw new Exception("GetType().BaseType is null");

            return baseType.FullName;
        }
        
        public override void Initialize()
        {
            if (KeyValue == null)
                return;

            if (AllowCache)
            {
                Item = GetCache(KeyValue.Value);
                if (Item != null)
                    return;
            }

            Item = GetItemInformation(KeyValue.Value);
            if (AllowCache)
                SetCache(KeyValue.Value, Item);
        }

        public void ClearCache(TKey id)
        {
            HttpContext.Current.Cache[CacheKey() + id] = null;
        }

        public TDataItem GetCache(TKey id)
        {
            return (TDataItem)HttpContext.Current.Cache[CacheKey() + id];
        }

        protected void SetCache(TKey id, TDataItem item)
        {
            if (item == null) 
                return;

            HttpContext.Current.Cache.Add(
                CacheKey() + id,
                item,
                null,
                DateTime.Now.AddMinutes(0.5),
                System.Web.Caching.Cache.NoSlidingExpiration,
                System.Web.Caching.CacheItemPriority.Normal,
                null);
        }
    }
}
