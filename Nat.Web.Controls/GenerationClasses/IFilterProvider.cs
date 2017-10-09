/*
 * Created by: Sergey V. Shpakovskiy
 * Created: 13 марта 2009 г.
 * Copyright © JSC New Age Technologies 2009
 */

using System;
using System.Linq;

namespace Nat.Web.Controls.GenerationClasses
{
    public interface IFilterProvider
    {
        MainPageUrlBuilder Url { get; set; }
        bool IsSelect { get; set; }
        string ProjectName { get; set; }
        void SetFilters(ref IQueryable enumerable);
        void Init(Type tableType);
    }
}