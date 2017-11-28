/*
 * Created by: Sergey V. Shpakovskiy
 * Created: 13 мая 2009 г.
 * Copyright © JSC New Age Technologies 2009
 */

using System.Linq;
using System;

namespace Nat.Web.Controls.GenerationClasses
{
    public interface IDataSourceView
    {
        IQueryable<IRow> GetSelectIRow(string queryParameters);
        IQueryable<ICodeRow> GetSelectICodeRow(string queryParameters);
        bool CheckPermit();
        bool SupportSelectICodeRow { get; }
        string SelectedRowKey { get; set; }
        string TableName { get; set; }
        long? LogViewData { get; set; }
    }

    public interface IDataSourceView2
    {
        IQueryable<IDataRow> GetSelectIRow(string queryParameters);
        IQueryable<IDataCodeRow> GetSelectICodeRow(string queryParameters);
        bool CheckPermit();
        bool SupportSelectICodeRow { get; }
        string SelectedRowKey { get; set; }
    }

    public interface IDataSourceView3
    {
        bool SupportFlagCanAddChild { get; }
        SelectParameters SelectParameters { get; }
        bool HideRecordCanNotSelected { get; set; }
    }

    public interface IDataSourceView4 : IDataSourceView2
    {
        bool CheckPermitExport();
        IQueryable<IDataRow> GetSelectIRowByID(string queryParameters, params string[] id);
    }

    public interface IDataSourceViewGetName
    {
        Type KeyType { get; }
        object GetKey(string key);
        string GetName(string key);
        string GetName(object key);
        string GetCode(string key);
        string GetCode(object key);
        object GetTableParameterValue(string key, string tableParameterName);
    }
}