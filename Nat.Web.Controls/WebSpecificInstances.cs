/*
 * Created by: Sergey V. Shpakovskiy
 * Created: 27 θώνÿ 2008 γ.
 * Copyright © JSC New Age Technologies 2008
 */

using Nat.Tools.Specific;
using Nat.Web.Tools;
using Nat.Web.Tools.Initialization;

namespace Nat.Web.Controls
{
    public class WebSpecificInstances : SpecificInstances
    {
        static WebSpecificInstances()
        {
            WebInitializer.Initialize();
        }

        public static IExporter GetExcelExporter()
        {
            return InitializerSection.GetSection().GetExcelExporter();
        }

        public static IExporter GetPdfExporter()
        {
            return InitializerSection.GetSection().GetPdfExporter();
        }
    }
}