/*
 * Created by: Sergey V. Shpakovskiy
 * Created: 13 ������ 2008 �.
 * Copyright � JSC New Age Technologies 2008
 */

using System;

namespace Nat.Web.Controls.GenerationClasses
{
    public interface IRow
    {
        long id { get; }
        string nameRu { get; }
        string nameKz { get; }
    }
}