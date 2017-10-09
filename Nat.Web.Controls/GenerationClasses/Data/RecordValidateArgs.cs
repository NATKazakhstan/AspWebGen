using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel;

namespace Nat.Web.Controls.GenerationClasses.Data
{
    public class RecordValidateArgs : CancelEventArgs
    {
        public RecordValidateArgs()
        {
        }

        public RecordValidateArgs(bool ignoreHistoryFilter)
            : this()
        {
            IgnoreHistoryFilter = ignoreHistoryFilter;
        }
        
        public RecordValidateArgs(bool ignoreHistoryFilter, bool selectForAddChild)
            : this()
        {
            IgnoreHistoryFilter = ignoreHistoryFilter;
            SelectForAddChild = selectForAddChild;
        }

        public bool IgnoreHistoryFilter { get; set; }

        public bool SelectForAddChild { get; set; }
    }
}