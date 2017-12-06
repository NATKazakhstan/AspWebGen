﻿namespace Nat.ExportInExcel
{
    using System.Collections.Generic;

    public class ExportDataRow
    {
        private Dictionary<string, string> Values { get; } = new Dictionary<string, string>();
        private Dictionary<string, string> HyperLinks { get; } = new Dictionary<string, string>();

        public void SetValue(string columnName, string value)
        {
            Values[columnName] = value;
        }

        public string GetValue(string columnName)
        {
            return Values.ContainsKey(columnName) ? Values[columnName] : null;
        }

        public void SetHyperLink(string columnName, string value)
        {
            HyperLinks[columnName] = value;
        }

        public string GetHyperLink(string columnName)
        {
            return HyperLinks.ContainsKey(columnName) ? HyperLinks[columnName] : null;
        }
    }
}