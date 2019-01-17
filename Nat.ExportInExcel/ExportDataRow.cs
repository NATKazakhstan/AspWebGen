namespace Nat.ExportInExcel
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;

    public class ExportDataRow
    {
        private Dictionary<string, string> Values { get; } = new Dictionary<string, string>();
        private Dictionary<string, string> HyperLinks { get; } = new Dictionary<string, string>();

        public ExportDataRow()
        {
        }

        public ExportDataRow(object initValues)
        {
            var properties = TypeDescriptor.GetProperties(initValues);
            foreach (PropertyDescriptor property in properties)
                Values[property.Name] = Convert.ToString(property.GetValue(initValues));
        }

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