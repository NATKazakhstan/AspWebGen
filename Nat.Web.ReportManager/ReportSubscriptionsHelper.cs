namespace Nat.Web.ReportManager
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Data.Linq;
    using System.IO;
    using System.Linq;
    using System.Runtime.Serialization.Formatters.Binary;
    using System.Web.Compilation;
    using System.Web.UI;
    using System.Web.UI.WebControls;

    using Nat.ReportManager.QueryGeneration;
    using Nat.ReportManager.ReportGeneration;
    using Nat.Tools.Filtering;
    using Nat.Web.ReportManager.Data;
    using Nat.Web.Tools;

    public static class ReportSubscriptionsHelper
    {
        public static Binary ObjectToBinary(object value)
        {
            Binary bufferBinary = null;
            if (value != null)
            {
                byte[] buffer;
                using (var stream = new MemoryStream())
                {
                    var bf = new BinaryFormatter();
                    bf.Serialize(stream, value);
                    buffer = stream.ToArray();
                }

                bufferBinary = new Binary(buffer);                
            }

            return bufferBinary;
        }

        public static object BinaryToObject(byte[] value)
        {
            using (var stream = new MemoryStream(value))
            {
                var bf = new BinaryFormatter();
                return bf.Deserialize(stream);
            }
        }

        public static void GetUpdateReportSubscriptionValuesParams(UpdateReportSubscriptionValuesParamsArgs args)
        {
            var storageValues = (StorageValues)BinaryToObject(args.Values.ToArray());
            var storageConstants = (IDictionary)BinaryToObject(args.Constants.ToArray());
            var rowParams =
                args.db.ReportSubscriptions_Params.Where(
                    q => q.refReportSubscriptions == args.refReportSubscriptions);
            foreach (var rowParam in rowParams)
            {
                // Если параметры публикации не настроены, то не изменяем параметры отчета
                if (rowParam.refReportTimePeriodsParameters == null) continue;

                IList storageValue = rowParam.DynamicAttributeIndex == null
                                            ? storageValues.GetStorageValues(rowParam.ParamName)
                                            : storageValues.GetCircleStorageValues(
                                                rowParam.ParamName,
                                                (int)rowParam.DynamicAttributeIndex);
                IList storageConstant;
                var constantValues = (IList)storageConstants[rowParam.ParamName];
                if (rowParam.DynamicAttributeIndex == null)
                    storageConstant = constantValues;
                else
                {
                    if (rowParam.DynamicAttributeIndex == 0 && !(constantValues[0] is IList)) 
                        storageConstant = constantValues;
                    else
                        storageConstant = (IList)constantValues[(int)rowParam.DynamicAttributeIndex];
                }

                if (storageValue != null & storageValue != null)
                {
                    if (args.DateNext != null)
                    {
                        var paramReportSubscriptions = new ParamReportSubscriptions(
                            rowParam, args.DateNext.Value.Date);
                        if (rowParam.ParamDataType == TypeCode.DateTime.ToString())
                        {
                            storageValue[0] =
                                storageConstant[0] = paramReportSubscriptions.DateParams[0];
                            if (storageValue.Count > 1)
                            {
                                storageValue[1] =
                                    storageConstant[1] = paramReportSubscriptions.DateParams[1];
                            }
                        }

                        if (rowParam.ParamDataType == TypeCode.Int16.ToString()
                            || rowParam.ParamDataType == TypeCode.Int32.ToString())
                        {
                            if (args.DateNext != null)
                            {
                                storageValue[0] =
                                    storageConstant[0] = paramReportSubscriptions.DateParams[0].Year;
                                if (storageValue.Count > 1)
                                {
                                    storageValue[1] =
                                        storageConstant[1] = paramReportSubscriptions.DateParams[1].Year;
                                }
                            }
                        }
                    }
                }

                if (storageValue != null)
                {
                    string[] textValues;
                    if (rowParam.DynamicAttributeIndex != null)
                    {
                        storageValues.SetCircleStorageValues(
                            rowParam.ParamName,
                            (int)rowParam.DynamicAttributeIndex,
                            storageValue.Cast<object>().ToArray());
                        textValues =
                            storageValue.Cast<object>().Select(p => (p ?? string.Empty).ToString()).
                                ToArray();
                        storageValues.SetCircleStorageValues(
                            rowParam.ParamName, (int)rowParam.DynamicAttributeIndex, textValues);
                        var storageConstantList = (IList)storageConstants[rowParam.ParamName];
                        storageConstantList[(int)rowParam.DynamicAttributeIndex] = storageConstant;
                    }
                    else
                    {
                        storageValues.SetStorageValues(
                            rowParam.ParamName, storageValue.Cast<object>().ToArray());
                        textValues =
                            storageValue.Cast<object>().Select(p => (p ?? string.Empty).ToString()).
                                ToArray();
                        storageValues.SetStorageTextValues(rowParam.ParamName, textValues);
                        storageConstants[rowParam.ParamName] = storageConstant;
                    }                    
                    
                    rowParam.ParamTextValuesKz =
                        rowParam.ParamTextValuesRu = string.Join(", ", textValues);
                    args.db.SubmitChanges();
                }
            }

            args.Values = ObjectToBinary(storageValues);
            args.Constants = ObjectToBinary(storageConstants);
        }

        public static void UpdateReportSubscriptionParams(
            DBDataContext db,
            long refReportSubscriptions,
            Page page,
            PlaceHolder ph,
            StorageValues values,
            string reportName)
        {
            var rowParams = db.ReportSubscriptions_Params.Where(q => q.refReportSubscriptions == refReportSubscriptions);
            db.ReportSubscriptions_Params.DeleteAllOnSubmit(rowParams);
            db.SubmitChanges();

            var type = BuildManager.GetType(reportName, false, true);

            var isCultureRu = LocalizationHelper.IsCultureRU;
            LocalizationHelper.SetCulture("ru-ru", null);
            var pluginRu = (IReportPlugin)Activator.CreateInstance(type);
            var conditionsRu = pluginRu.Conditions;
            pluginRu.SetCountCircleFillConditions(values.CountListValues, true);

            LocalizationHelper.SetCulture("kk-kz", null);
            var pluginKz = (IReportPlugin)Activator.CreateInstance(type);
            var conditionsKz = pluginKz.Conditions;
            pluginKz.SetCountCircleFillConditions(values.CountListValues, true);

            // Create Conditions
            CreateReportSubscriptionParams(db, refReportSubscriptions, conditionsRu, conditionsKz, null, values, null);
            
            // Create CircleConditions            
            if (pluginRu.CircleFillConditions != null)
            {
                int i = -1;
                foreach (var circleConditionsRu in pluginRu.CircleFillConditions)
                {
                    i++;
                    var circleConditionsKz = pluginKz.CircleFillConditions;
                    CreateReportSubscriptionParams(db, refReportSubscriptions, circleConditionsRu, circleConditionsKz[i], null, values, i);
                }
            }

            // Save
            db.SubmitChanges();
            LocalizationHelper.SetCulture(isCultureRu ? "ru-ru" : "kk-kz", null);
        }

        public static void CreateReportSubscriptionParams(
            DBDataContext db,
            long refReportSubscriptions,
            List<BaseReportCondition> conditionsRu,
            List<BaseReportCondition> conditionsKz,
            PlaceHolder ph,
            StorageValues values,
            int? index)
        {
            var i = -1;
            foreach (var conditionRu in conditionsRu)
            {
                i++;
                if (!conditionRu.Visible) continue;
                var conditionKz = conditionsKz[i];

                //ph.Controls.Add((Control)conditionRu.ColumnFilter);
                var storageRu = conditionRu.ColumnFilter.GetStorage();
                if (index == null)
                    values.SetStorage(storageRu);
                else
                    values.SetListStorage(storageRu, index.Value);
                conditionRu.ColumnFilter.SetStorage(storageRu);

                //ph.Controls.Add((Control)conditionKz.ColumnFilter);
                var storageKz = conditionKz.ColumnFilter.GetStorage();
                if (index == null)
                    values.SetStorage(storageKz);
                else
                    values.SetListStorage(storageKz, index.Value);
                conditionKz.ColumnFilter.SetStorage(storageKz);                
                
                string[] textValues = conditionsKz[i].ColumnFilter.GetTexts();
                var paramTextValuesKz = string.Empty;
                if (textValues != null)
                    paramTextValuesKz = string.Join(", ", textValues);
                var paramCaptionKz = storageKz.Caption;

                var paramTextValuesRu = string.Empty;
                textValues = conditionRu.ColumnFilter.GetTexts();
                if (textValues != null)
                    paramTextValuesRu = string.Join(", ", textValues);
                var paramCaptionRu = storageRu.Caption;

                var record = new ReportSubscriptions_Param
                    {
                                     refReportSubscriptions = refReportSubscriptions,
                                     ParamName = storageRu.Name,
                                     ParamCaptionKz = paramCaptionKz,
                                     ParamCaptionRu = paramCaptionRu,
                                     ParamTextValuesKz = paramTextValuesKz,
                                     ParamTextValuesRu = paramTextValuesRu,
                                     DynamicAttributeIndex = index,
                                     ParamDataType = storageRu.DataType.Name,
                                     ParamFilterType = storageRu.FilterType.ToString()
                                 };       
                db.ReportSubscriptions_Params.InsertOnSubmit(record);

                //ph.Controls.Remove((Control)conditionRu.ColumnFilter);
                //ph.Controls.Remove((Control)conditionKz.ColumnFilter);
            }
        }
    }
}