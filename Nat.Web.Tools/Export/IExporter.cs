/*
* Created by: Sergey V. Shpakovskiy
* Created: 2012.08.29
* Copyright © JSC NAT Kazakhstan 2012
*/

namespace Nat.Web.Tools
{
    using System;
    using System.IO;

    using Nat.Web.Tools.Export;

    public interface IExporter
    {
        Stream GetExcelByType(Type journalType, object properties, ILogMonitor logMonitor, bool checkPermit, out string fileNameExtention);

        Stream GetExcelByType(Type journalType, string format, object properties, string culture, ILogMonitor logMonitor, bool checkPermit, out string fileNameExtention);

        Stream GetExcelByType(Type journalType, string format, long idProperties, StorageValues storageValues, string culture, ILogMonitor logMonitor, bool checkPermit, out string fileNameExtention);

        Stream GetExcelByTypeName(string journalType, string format, long idProperties, StorageValues storageValues, string culture, ILogMonitor logMonitor, bool checkPermit, out string fileNameExtention);

        Stream GetExcelStream(JournalExportEventArgs args);
    }
}
