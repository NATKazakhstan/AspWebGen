/*
 * Created by: Sergey V. Shpakovskiy
 * Created: 11 сентября 2008 г.
 * Copyright © JSC New Age Technologies 2008
 */

namespace Nat.Web.Tools.Initialization
{
    public static class WebInitializer
    {
        private static readonly object _lock = new object();
        private static InitializerSectionCollection initializerSection;

        public static void Initialize()
        {
            lock (_lock)
            {
                if (initializerSection == null)
                {
                    initializerSection = InitializerSection.GetSection().InitializerClasses;
                    initializerSection.Initialize();
                }
            }
        }
    }
}