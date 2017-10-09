using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Nat.Web.Controls.GenerationClasses
{
    public class BaseCodeRow : BaseRow,  ICodeRow, IDataCodeRow
    {
        #region ICodeRow, IDataCodeRow Members

        public virtual string code { get; set; }

        #endregion
    }
}
