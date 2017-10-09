using System;
using System.ComponentModel;
using System.Web.Compilation;
using Nat.Tools.Data;
using Nat.Tools.Filtering;
using Nat.Tools.QueryGeneration;
using Nat.Web.Tools.Initialization;

namespace Nat.Web.Tools
{
    public static class HistoricalData
    {
        public static Component GetTableAdapterToHistoricalData(string endDateField, string StartDateField, DateTime? HistolicalPoint, string typeName, string selectMethod)
        {
            Type type = BuildManager.GetType(typeName, false, true);
            return GetTableAdapterToHistoricalData(endDateField, StartDateField, HistolicalPoint, type, TableAdapterTools.GetSelectCommandIndex(type, selectMethod));
        }

        public static Component GetTableAdapterToHistoricalData(string endDateField, string StartDateField, DateTime? HistolicalPoint, Type typeTableAdapter, int commandIndex)
        {
            WebInitializer.Initialize();
            QueryConditionList queryConditions = GetHistoricalCondition(endDateField, StartDateField, HistolicalPoint);

            var adapter = (Component)Activator.CreateInstance(typeTableAdapter);
            var queryGenerator = new QueryGenerator(adapter, commandIndex);
            return queryGenerator.GetTableAdapter(queryConditions);
        }

        public static Component GetTableAdapterToHistoricalData(Component adapter, string endDateField, string StartDateField, DateTime? HistolicalPoint, Type typeTableAdapter, int commandIndex)
        {
            WebInitializer.Initialize();
            QueryConditionList queryConditions = GetHistoricalCondition(endDateField, StartDateField, HistolicalPoint);
            var queryGenerator = new QueryGenerator(adapter, commandIndex);
            return queryGenerator.GetTableAdapter(queryConditions);
        }

        public static QueryConditionList GetHistoricalCondition(string endDateField, string StartDateField, DateTime? HistolicalPoint)
        {
            var queryConditions = new QueryConditionList();
            DateTime Date = HistolicalPoint ?? DateTime.Now;
            var startDateCondition = new QueryCondition(StartDateField, ColumnFilterType.LessOrEqual, Date, null);
            queryConditions.Add(startDateCondition);

            var nullEndDateCondition = new QueryCondition(endDateField, ColumnFilterType.IsNull);
            var lessEndDateCondition = new QueryCondition(endDateField, ColumnFilterType.MoreOrEqual, Date, null);

            var endDateCondition = new QueryCondition();
            endDateCondition.Conditions = new QueryConditionList {ConditionJunction = ConditionJunction.Or};
            endDateCondition.Conditions.Add(nullEndDateCondition);
            endDateCondition.Conditions.Add(lessEndDateCondition);
            queryConditions.Add(endDateCondition);
            return queryConditions;
        }

        public static QueryConditionList GetHistoricalOverCondition(string endDateField, DateTime? HistolicalPoint)
        {
            var queryConditions = new QueryConditionList();
            DateTime Date = HistolicalPoint ?? DateTime.Now;
            var condition = new QueryCondition(endDateField, ColumnFilterType.Less, Date, null);
            queryConditions.Add(condition);
            return queryConditions;
        }

        public static string GetHistoricalFilter(string endDateField, string StartDateField, DateTime? HistolicalPoint)
        {
            return GetHistoricalCondition(endDateField, StartDateField, HistolicalPoint).NonSqlFilter;            
        }
    }
}