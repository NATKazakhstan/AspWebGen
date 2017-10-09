/*
 * Created by : Daniil Kovalev
 * Created    : 28.11.2007
 */

using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Web.Script.Serialization;
using System.Web.Script.Services;
using System.Web.Services;
using System.Web.UI;
using System.Web.UI.WebControls;
using Nat.Tools.Filtering;
using Nat.Tools.QueryGeneration;

namespace Nat.Web.Controls
{
    [WebService(Namespace = "http://tempuri.org/")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    [ToolboxItem(false)]
    [ScriptService]
    public class AutoComplete : WebService
    {
        [WebMethod(EnableSession = true)]
        [ScriptMethod]
        public string[] GetCompletionList(string prefixText, int count, string contextKey)
        {
            Hashtable props = (Hashtable)Session[contextKey];

            TableDataSourceView tableDataSourceView = new TableDataSourceView(
                null,
                (Boolean)props["ShowHistoricalData"],
                (Boolean)props["LoadAllHistoricalData"],
                (String)props["EndDateField"],
                (String)props["StartDateField"],
                (DateTime?)props["HistolicalPoint"], 
                (QueryConditionList)props["QueryConditionsList"],
                (String)props["SelectMethod"], 
                (String)props["TypeName"], 
                (List<Triplet>)props["SelectParameters"], 
                (Dictionary<String, String>)props["FilterParameters"],
                (String)props["FilterExpression"],
                (Boolean)props["SetFilterByCustomConditions"]);

            QueryCondition queryCondition = new QueryCondition((String)props["DataTextField"],
                ColumnFilterType.StartWith, prefixText, null);

            tableDataSourceView.CustomConditions.Add(queryCondition);

            QueryCondition disableRowFieldQueryCondition = null;
            String disableRowField = (String)props["DataDisableRowField"];
            if (!String.IsNullOrEmpty(disableRowField))
            {
                Int32 disableRowCondition = (Int32)props["ConditionValue"];
                disableRowFieldQueryCondition = new QueryCondition(disableRowField,
                    ColumnFilterType.Equal, disableRowCondition, null);

                tableDataSourceView.CustomConditions.Add(disableRowFieldQueryCondition);
            }

            DataView dataView;
            try
            {
                string filterString = tableDataSourceView.GetFilterString();
                dataView = (DataView)tableDataSourceView.Select(true, new DataSourceSelectArguments(), new DataSourceSelectExtArguments(count));
                dataView.RowFilter = filterString;
            }
            finally
            {
                tableDataSourceView.CustomConditions.Remove(queryCondition);

                if (disableRowFieldQueryCondition != null)
                    tableDataSourceView.CustomConditions.Remove(disableRowFieldQueryCondition);
            }

            List<string> items = new List<string>();
            JavaScriptSerializer javaScriptSerializer = new JavaScriptSerializer();

            foreach(DataRowView dataRowView in dataView)
            {
                String text = dataRowView[(String)props["DataTextField"]].ToString();
                String code = String.IsNullOrEmpty((String)props["DataCodeField"]) ?
                                                                                       "-1" : dataRowView[(String)props["DataCodeField"]].ToString();
                String id = dataRowView[(String)props["DataValueField"]].ToString();
                String value = javaScriptSerializer.Serialize(new Pair(id, code));
                String item = javaScriptSerializer.Serialize(new Pair(text, value));
                items.Add(item);
            }
            return items.ToArray();
        }
    }
}