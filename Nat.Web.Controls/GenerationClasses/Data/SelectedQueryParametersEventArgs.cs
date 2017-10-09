using System;
using Nat.Web.Controls.GenerationClasses.Filter;

namespace Nat.Web.Controls.GenerationClasses.Data
{
    public class SelectedQueryParametersEventArgs : EventArgs
    {
        public QueryParameters QueryParameters { get; set; }
    }
}