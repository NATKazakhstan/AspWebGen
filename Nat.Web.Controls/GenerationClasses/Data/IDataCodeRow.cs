﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Nat.Web.Controls.GenerationClasses
{
    public interface IDataCodeRow : IDataRow
    {
        string code { get; }
    }
}
