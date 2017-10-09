/*
* Created by: Sergey V. Shpakovskiy
* Created: 2013.02.20
* Copyright © JSC NAT Kazakhstan 2013
*/

namespace Nat.Web.Tools.ExtNet.Data
{
    using System.Linq;

    using Nat.Web.Controls.GenerationClasses;

    public interface IDataSourceViewExtNet
    {
        bool CheckPermit();

        IQueryable<IDataRow> GetFullModelData(string queryParameters);

        IQueryable<IDataRow> GetFullModelData(string queryParameters, string refParent);
    }
}