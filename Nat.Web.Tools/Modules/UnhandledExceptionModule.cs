namespace Nat.Web.Tools.Modules
{
    using System;
    using System.Diagnostics;
    using System.Globalization;
    using System.IO;
    using System.Runtime.InteropServices;
    using System.Text;
    using System.Threading;
    using System.Web;

    public class UnhandledExceptionModule : IHttpModule
    {
        private static int _unhandledExceptionCount;

        private static string _sourceName;

        private static readonly object _initLock = new object();

        private static bool _initialized;

        public void Init(HttpApplication app)
        {
            // Do this one time for each AppDomain.
            if (_initialized)
                return;

            lock (_initLock)
            {
                if (_initialized) return;

                var webenginePath = Path.Combine(RuntimeEnvironment.GetRuntimeDirectory(), "webengine.dll");

                if (!File.Exists(webenginePath))
                {
                    throw new Exception(
                        string.Format(
                            CultureInfo.InvariantCulture,
                            "Failed to locate webengine.dll at '{0}'.  This module requires .NET Framework 2.0.",
                            webenginePath));
                }

                var ver = FileVersionInfo.GetVersionInfo(webenginePath);
                _sourceName = string.Format(
                    CultureInfo.InvariantCulture,
                    "ASP.NET {0}.{1}.{2}.0",
                    ver.FileMajorPart,
                    ver.FileMinorPart,
                    ver.FileBuildPart);

                if (!EventLog.SourceExists(_sourceName))
                {
                    throw new Exception(
                        string.Format(
                            CultureInfo.InvariantCulture,
                            "There is no EventLog source named '{0}'. This module requires .NET Framework 2.0.",
                            _sourceName));
                }

                AppDomain.CurrentDomain.UnhandledException += OnUnhandledException;
                _initialized = true;
            }
        }

        public void Dispose()
        {
        }

        private void OnUnhandledException(object o, UnhandledExceptionEventArgs e)
        {
            // Let this occur one time for each AppDomain.
            if (Interlocked.Exchange(ref _unhandledExceptionCount, 1) != 0)
                return;

            var appId = (string)AppDomain.CurrentDomain.GetData(".appId");
            var message = new StringBuilder($@"UnhandledException logged by Nat.Web.Tools.Modules.UnhandledExceptionModule: appId={appId}");
            message.AppendLine();
            message.AppendLine();
            message.AppendLine(((Exception)e.ExceptionObject).ToString());
            EventLog.WriteEntry(_sourceName, message.ToString(), EventLogEntryType.Error, 1334);
        }
    }
}