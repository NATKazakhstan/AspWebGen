using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Compilation;
using Nat.Tools.Filtering;
using Nat.Tools.Specific;
using Nat.Web.Tools.Initialization;

namespace Nat.Web.ReportManager
{
    using Nat.Web.Controls.Filters;

    public class ReportManagerInitializer : IInitializer
    {
        #region IInitializer Members

        public void Initialize()
        {
            // инициализация ColumnFilterFactory
            ColumnFilterFactory columnFilterFactory = null;
            try
            {
                columnFilterFactory = SpecificInstances.ColumnFilterFactory;
            }
            catch (NullReferenceException)
            {
            }

            if (columnFilterFactory == null)
            {
                var section = ReportInitializerSection.GetReportInitializerSection();
                var type = Type.GetType(section.ColumnFilterFactoryType, false, true)
                           ?? BuildManager.GetType(section.ColumnFilterFactoryType, false, true)
                           ?? typeof(WebColumnFilterFactory);
                SpecificInstances.ColumnFilterFactory = (ColumnFilterFactory)Activator.CreateInstance(type);
            }

            var rm = new WebReportManager(null);
            rm.GetPlugins();
        }

        #endregion
    }
}