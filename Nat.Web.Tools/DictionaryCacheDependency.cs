namespace Nat.Web.Tools
{
    using System;
    using System.Web;
    using System.Web.Caching;

    /// <summary>
    /// В зависимом классе TSource вызывается метод DictionaryCacheDependency&lt;TSource&gt;.Increcment(), при изменении, например, в partial DataSaved.
    /// При добавлении кэша достаточно указать new DictionaryCacheDependency&lt;TSource&gt;().
    /// </summary>
    /// <typeparam name="TSource"></typeparam>
    public class DictionaryCacheDependency<TSource> : CacheDependency
    {
        public DictionaryCacheDependency()
            : base(null, new[] { GetKey() })
        {
            SetCache(0);
        }

        internal static void SetCache(int value)
        {
            var key = GetKey();

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

        internal static string GetKey()
        {
            return typeof(DictionaryCacheDependency<TSource>).Name + "[" + typeof(TSource).Name + "]";
        }

        public static int GetValue()
        {
            return (int?)HttpContext.Current.Cache[GetKey()] ?? 0;
        }

        public static void Increcment()
        {
            SetCache(GetValue() + 1);
        }
    }

    public class DictionaryCacheDependency<TSource1, TSource2> : CacheDependency
    {
        public DictionaryCacheDependency()
            : base(null, new[] { DictionaryCacheDependency<TSource1>.GetKey(), DictionaryCacheDependency<TSource2>.GetKey() })
        {
            DictionaryCacheDependency<TSource1>.SetCache(0);
            DictionaryCacheDependency<TSource2>.SetCache(0);
        }
    }

    public class DictionaryCacheDependency<TSource1, TSource2, TSource3> : CacheDependency
    {
        public DictionaryCacheDependency()
            : base(
                null,
                new[]
                    {
                        DictionaryCacheDependency<TSource1>.GetKey(),
                        DictionaryCacheDependency<TSource2>.GetKey(),
                        DictionaryCacheDependency<TSource3>.GetKey()
                    })
        {
            DictionaryCacheDependency<TSource1>.SetCache(0);
            DictionaryCacheDependency<TSource2>.SetCache(0);
            DictionaryCacheDependency<TSource3>.SetCache(0);
        }
    }

    public class DictionaryCacheDependency<TSource1, TSource2, TSource3, TSource4> : CacheDependency
    {
        public DictionaryCacheDependency()
            : base(
                null,
                new[]
                    {
                        DictionaryCacheDependency<TSource1>.GetKey(),
                        DictionaryCacheDependency<TSource2>.GetKey(),
                        DictionaryCacheDependency<TSource3>.GetKey(),
                        DictionaryCacheDependency<TSource4>.GetKey()
                    })
        {
            DictionaryCacheDependency<TSource1>.SetCache(0);
            DictionaryCacheDependency<TSource2>.SetCache(0);
            DictionaryCacheDependency<TSource3>.SetCache(0);
            DictionaryCacheDependency<TSource4>.SetCache(0);
        }
    }
}