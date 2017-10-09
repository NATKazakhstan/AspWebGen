/*
 * Created by: Sergey V. Shpakovskiy
 * Created: 11 ����� 2009 �.
 * Copyright � JSC New Age Technologies 2009
 */

using System;
using System.Collections.Generic;

namespace Nat.Web.Controls.GenerationClasses
{
    public interface IItemNavigatorsInitializer
    {
        void Initialize(IDictionary<Type, SelectedParameterNavigator.ItemNavigator> itemNavigators);
    }
}