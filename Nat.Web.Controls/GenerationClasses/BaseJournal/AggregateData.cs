using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Nat.Web.Controls.GenerationClasses.BaseJournal
{
    public abstract class AggregateData
    {
        public object KeyValue;
//        public string KeyName;
        public double? Value;
        public int Count;
        public double? UserValue;
        public string StrValue;
        public string GroupColumnName;
        public ColumnAggregateType AggregateType { get; set; }
        public abstract AggregateData Clone();
        public abstract AggregateData GetChildGroup(object keyValue);
        public abstract IEnumerable<AggregateData> Childs { get; }

        /// <summary>
        /// для агрегативных колонок
        /// </summary>
        /// <param name="groupValues"></param>
        /// <param name="isInlineGroup"></param>
        /// <param name="index"></param>
        /// <returns></returns>
        public abstract int GetCount(object[] groupValues, bool[] isInlineGroup, int index);

        /// <summary>
        /// Для инлайн групп
        /// </summary>
        /// <param name="groupValues"></param>
        /// <param name="isInlineGroup"></param>
        /// <param name="computeIndex"></param>
        /// <param name="index"></param>
        /// <returns></returns>
        public abstract int GetCount(object[] groupValues, bool[] isInlineGroup, int computeIndex, int index);

        public abstract IEnumerable<double?> GetValues(object[] groupValues, bool[] isInlineGroup, int index);

        public abstract IEnumerable<string> GetValues(object[] groupValues, bool[] isInlineGroup, int computeIndex, int index);

        public abstract IEnumerable<object> GetKeys(object[] groupValues, bool[] isInlineGroup, int index);

        /*public abstract int GetCount(object[] groupValues, int computeIndex, int index, int groupIndex);
        public abstract IEnumerable<object> GetTotalRowsValues(object[] groupValues, int computeIndex, int index, int groupIndex, bool userValue);*/
        //public abstract IEnumerable<string> GetTotalRowsStrValues(object[] groupValues, int computeIndex, int index, int groupIndex);

        public override string ToString()
        {
            var sb = new StringBuilder();
            ToString(sb, 0);
            return sb.ToString();
        }

        public void ToString(StringBuilder sb, int tabs)
        {
            for (int i = 0; i < tabs; i++)
                sb.Append("\t");
            sb.Append("KeyValue: ");
            sb.Append(KeyValue);
            sb.Append("; StrValue: ");
            sb.Append(StrValue);
            sb.Append("; Value: ");
            sb.Append(Value);
            sb.AppendLine();
            foreach (var child in Childs)
                child.ToString(sb, tabs + 1);
        }
    }
/*
    public class AggregateDataEmpty : AggregateData
    {
        public override AggregateData Clone()
        {
            return null;
        }

        public override AggregateData GetChildGroup(object keyValue, string strValue)
        {
            return null;
        }

        public override IEnumerable<AggregateData> Childs
        {
            get { return new AggregateData[0]; }
        }
    }*/

    public class AggregateDataSimple : AggregateData
    {
        public AggregateDataSimple()
        {
            Values = new Dictionary<object, AggregateData>();
        }

        public Dictionary<object, AggregateData> Values;
        public AggregateData NullValue;

        public override AggregateData GetChildGroup(object keyValue)
        {
            if (keyValue == null)
            {
                if (NullValue == null)
                    NullValue = Clone();
                return NullValue;
            }
            if (Values.ContainsKey(keyValue))
                return Values[keyValue];
            if (Values.Keys.Count > 0 && Values.Keys.First().GetType() != keyValue.GetType())
            {
                throw new Exception(
                    string.Format("Can't add value '{0}' of type '{1}'. Exist data with type '{2}'. CurrentState: {3}", keyValue, keyValue.GetType().Name, Values.Keys.First().GetType().Name, this));
            }

            var child = Clone();
            Values[keyValue] = child;
            child.KeyValue = keyValue;
            //child.KeyName = keyName;
            return child;
        }

        public override IEnumerable<AggregateData> Childs
        {
            get
            {
                if (NullValue == null)
                    return Values.Values;
                return new[] { NullValue }.Union(Values.Values);
            }
        }

        public override int GetCount(object[] groupValues, bool[] isInlineGroup, int index)
        {
            if (isInlineGroup.Length > index && !isInlineGroup[index])
            {
                if (groupValues.Length > index && ((groupValues[index] != null && Values.ContainsKey(groupValues[index])) || NullValue != null))
                {
                    if (groupValues.Length == index + 1)
                        return 1;
                    //проверяем что вложенные группы есть
                    foreach (var child in Childs)
                    {
                        var count = child.GetCount(groupValues, isInlineGroup, index + 1);
                        if (count > 0) return count;
                    }
                }
                return 0;
            }
            if (groupValues.Length == 1)
                return Childs.Count();

            var sumCount = 0;
            foreach (var child in Childs)
                sumCount += child.GetCount(groupValues, isInlineGroup, index + 1);
            return sumCount;
            //if (isInlineGroup.Take(groupValues.Length).Skip(index + 1).Contains(true))
        }

        public override int GetCount(object[] groupValues, bool[] isInlineGroup, int computeIndex, int index)
        {
            if (isInlineGroup.Length > index && !isInlineGroup[index])
            {
                if (groupValues.Length > index && ((groupValues[index] != null && Values.ContainsKey(groupValues[index]) || NullValue != null)))
                {
                    if (groupValues.Length == index + 1)
                        return 1;
                    //проверяем что вложенные группы есть
                    foreach (var child in Childs)
                    {
                        var count = child.GetCount(groupValues, isInlineGroup, computeIndex, index + 1);
                        if (count > 0) return count;
                    }
                }
                return 0;
            }
            if (groupValues.Length == 1)
                return Childs.Count();
            if (index == computeIndex)
            {
                var count = 0;
                foreach (var child in Childs)
                    count += child.GetCount(groupValues, isInlineGroup, computeIndex, index + 1);
                return count;
            }
            foreach (var child in Childs)
            {
                var count = child.GetCount(groupValues, isInlineGroup, computeIndex, index + 1);
                if (count > 0) return count;
            }
            return 0;
        }

        public override IEnumerable<double?> GetValues(object[] groupValues, bool[] isInlineGroup, int index)
        {
            if (isInlineGroup.Length > index && !isInlineGroup[index])
            {
                if (groupValues.Length > index && ((groupValues[index] != null && Values.ContainsKey(groupValues[index])) || NullValue != null))
                {
                    if (groupValues.Length == index + 1)
                        return new[] { groupValues[index] == null ? NullValue.UserValue : Values[groupValues[index]].UserValue };
                    //проверяем что вложенные группы есть
                    foreach (var child in Childs)
                    {
                        var values = child.GetValues(groupValues, isInlineGroup, index + 1);
                        if (values != null) return values;
                    }
                }
                return null;
            }
            if (isInlineGroup.Length > index && isInlineGroup[index])
                return Childs.Select(r => r.UserValue);
            if (groupValues.Length == 1)
                return Childs.Select(r => r.UserValue);
            var allValues = new List<double?>();
            foreach (var child in Childs)
            {
                var values = child.GetValues(groupValues, isInlineGroup, index + 1);
                if (values != null)
                    allValues.AddRange(values);
            }
            if (allValues.Count > 0)
                return allValues;
            return null;
        }

        public override IEnumerable<object> GetKeys(object[] groupValues, bool[] isInlineGroup, int index)
        {
            if (isInlineGroup.Length > index && !isInlineGroup[index])
            {
                if (groupValues.Length > index && ((groupValues[index] != null && Values.ContainsKey(groupValues[index])) || NullValue != null))
                {
                    if (groupValues.Length == index + 1)
                        return new[] { KeyValue };
                    //проверяем что вложенные группы есть
                    foreach (var child in Childs)
                    {
                        var values = child.GetKeys(groupValues, isInlineGroup, index + 1);
                        if (values != null) return values;
                    }
                }
                return null;
            }
            if (isInlineGroup.Length > index && isInlineGroup[index])
                return Childs.Select(r => r.KeyValue);
            if (groupValues.Length == 1)
                return Childs.Select(r => r.KeyValue);
            var allValues = new List<object>();
            foreach (var child in Childs)
            {
                var values = child.GetKeys(groupValues, isInlineGroup, index + 1);
                if (values != null)
                    allValues.AddRange(values);
            }
            if (allValues.Count > 0)
                return allValues;
            return null;
        }

        public override IEnumerable<string> GetValues(object[] groupValues, bool[] isInlineGroup, int computeIndex, int index)
        {
/*            if (groupValues.Length == 1)
            {
                if (isInlineGroup[0])
                    return Values.Values.Select(r => r.StrValue);
                return Values.Values.Select(r => (r.KeyValue ?? "").ToString());
            }*/
            if (!isInlineGroup[index])
            {
                if (groupValues.Length > index && ((groupValues[index] != null && Values.ContainsKey(groupValues[index])) || NullValue != null))
                {
                    if (groupValues.Length == index + 1)
                        return new[] { groupValues[index] == null ? NullValue.StrValue : Values[groupValues[index]].StrValue};
                    //проверяем что вложенные группы есть
                    foreach (var child in Childs)
                    {
                        var values = child.GetValues(groupValues, isInlineGroup, computeIndex, index + 1);
                        if (values != null) return values;
                    }
                }
                return null;
            }
            if (isInlineGroup.Length > index && isInlineGroup[index])
                return Childs.Select(r => r.StrValue);
            if (groupValues.Length == 0)
                return Childs.Select(r => r.StrValue);
            if (index == computeIndex)
            {
                var allValues = new List<string>();
                foreach (var child in Childs)
                {
                    var values = child.GetValues(groupValues, isInlineGroup, computeIndex, index + 1);
                    if (values != null)
                        allValues.AddRange(values);
                }
                if (allValues.Count > 0)
                    return allValues;
                return null;
            }
            if (isInlineGroup[index] && index > computeIndex)
            {
                return new[] { (KeyValue ?? string.Empty).ToString() };
            }
            foreach (var child in Childs)
            {
                var values = child.GetValues(groupValues, isInlineGroup, computeIndex, index + 1);
                if (values != null) return values;
            }
            return null;
        }

        /*
                public override int GetCount(object[] groupValues, int computeIndex, int index, int groupIndex)
                {
                    if (index == groupIndex)
                    {
                        if (Values.ContainsKey(groupValues[index]))
                            return 1;
                        return 0;
                    }
                    if (computeIndex >= index && groupIndex > computeIndex)
                    {
                        var count = 0;
                        foreach (var child in Childs)
                            count += child.GetCount(groupValues, computeIndex, index + 1, groupIndex);
                        return count;
                    }
                    foreach (var child in Childs)
                    {
                        if (child.GetCount(groupValues, computeIndex, index + 1, groupIndex) > 0)
                            return 1;
                    }
                    return 0;
                }
         * 
                public override IEnumerable<string> GetTotalRowsStrValues(object[] groupValues, int computeIndex, int index, int groupIndex)
                {
                    if (index == groupIndex)
                    {
                        if (Values.ContainsKey(groupValues[index]))
                            return new [] { StrValue };
                        return null;
                    }
                    if (computeIndex >= index && groupIndex > computeIndex)
                    {
                        var list = new List<string>();
                        foreach (var child in Childs)
                        {
                            var values = child.GetTotalRowsStrValues(groupValues, computeIndex, index + 1, groupIndex);
                            if (values != null)
                                list.AddRange(values);
                        }
                        if (list.Count > 0)
                            return list;
                        return null;
                    }
                    foreach (var child in Childs)
                    {
                        var values = child.GetTotalRowsStrValues(groupValues, computeIndex, index + 1, groupIndex);
                        if (values != null)
                            return values;
                    }
                    return null;
                }

        public override IEnumerable<object> GetTotalRowsValues(object[] groupValues, int computeIndex, int index, int groupIndex)
        {
            if (index == groupIndex)
            {
                if (Values.ContainsKey(groupValues[index]))
                    return new object[] {userValue ? UserValue : Value};
                return null;
            }
            if (computeIndex >= index && groupIndex > computeIndex)
            {
                var list = new List<object>();
                foreach (var child in Childs)
                {
                    var values = child.GetTotalRowsValues(groupValues, computeIndex, index + 1, groupIndex, userValue);
                    if (values != null)
                        list.AddRange(values);
                }
                if (list.Count > 0)
                    return list;
                return null;
            }
            foreach (var child in Childs)
            {
                var values = child.GetTotalRowsValues(groupValues, computeIndex, index + 1, groupIndex, userValue);
                if (values != null)
                    return values;
            }
            return null;
        }
        */
        public override AggregateData Clone()
        {
            return new AggregateDataSimple {GroupColumnName = GroupColumnName, AggregateType = AggregateType,};
        }
    }
    /*
    public class AggregateData<TKey> : AggregateData
    {
        public AggregateData()
        {
            Values = new Dictionary<TKey, AggregateData>();
        }

        public Dictionary<TKey, AggregateData> Values;
        public AggregateData NullValue;

        public override AggregateData GetChildGroup(object keyValue)
        {
            if (keyValue == null)
            {
                if (NullValue == null)
                    NullValue = Clone();
                return NullValue;
            }
            var key = (TKey)keyValue;
            if (!Values.ContainsKey(key))
                Values[key] = Clone();
            return Values[key];
        }

        public override IEnumerable<AggregateData> Childs
        {
            get
            {
                if (NullValue == null)
                    return Values.Values;
                return new[] {NullValue}.Union(Values.Values);
            }
        }

        public override AggregateData Clone()
        {
            return new AggregateData<TKey> {GroupColumnName = GroupColumnName};
        }
    }*/
}