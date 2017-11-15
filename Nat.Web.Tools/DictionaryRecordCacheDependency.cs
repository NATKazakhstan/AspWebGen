namespace Nat.Web.Tools
{
    using System;
    using System.Web;
    using System.Web.Caching;

    /// <summary>
    /// В зависимом классе TSource вызывается метод DictionaryCacheDependency&lt;TSource&gt;.Increcment(id), при изменении, например, в partial DataSaved.
    /// При добавлении кэша указать new DictionaryCacheDependency&lt;TSource&gt;(id).
    /// </summary>
    /// <typeparam name="TSource"></typeparam>
    public class DictionaryRecordCacheDependency<TSource> : CacheDependency
    {
        public DictionaryRecordCacheDependency(string id)
            : base(null, new[] { GetKey(id) })
        {
            SetCache(0, id);
        }

        internal static void SetCache(int value, string id)
        {
            var key = GetKey(id);

            if (HttpContext.Current.Cache[key] == null)
                HttpContext.Current.Cache.Add(
                    key,
                    value,
                    null,
                    DateTime.Today.AddDays(1),
                    Cache.NoSlidingExpiration,
                    CacheItemPriority.High,
                    null);
            else if (value != 0)
                HttpContext.Current.Cache[key] = value;
        }

        internal static string GetKey(string id)
        {
            return typeof(DictionaryRecordCacheDependency<TSource>).Name + "[" + typeof(TSource).Name + "]:" + id;
        }

        public static int GetValue(string id)
        {
            return (int?)HttpContext.Current.Cache[GetKey(id)] ?? 0;
        }

        public static void Increcment(string id)
        {
            SetCache(GetValue(id) + 1, id);
        }
    }
}