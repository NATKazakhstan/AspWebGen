using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Nat.Web.Controls.GenerationClasses.BaseJournal;

namespace Nat.Web.Controls.EnableController
{
    public class ControllerItem
    {
        public ControllerItem()
        {
            ActiveControls = new List<ControllerItemValue>();
            Values = new List<ControllerItemValue>();
            RightValues = new List<ControllerItemValue>();
            Childs = new List<ControllerItem>();
        }

        public ControllerItem(string name, ControllerCompareOperator compareOperator, ControllerActiveType activeType, List<ControllerItemValue> activeControls, List<ControllerItemValue> values, bool defaultResult, bool joinResultsAsOr)
        {
            Name = name;
            CompareOperator = compareOperator;
            ActiveType = activeType;
            ActiveControls = activeControls;
            Values = values;
            DefaultResult = defaultResult;
            JoinResultsAsOr = joinResultsAsOr;
        }

        public ControllerItem(string name, ControllerCompareOperator compareOperator, ControllerActiveType activeType, List<ControllerItemValue> activeControls, List<ControllerItemValue> values, List<ControllerItemValue> rightValues, bool defaultResult, bool joinResultsAsOr)
        {
            Name = name;
            CompareOperator = compareOperator;
            ActiveType = activeType;
            ActiveControls = activeControls;
            Values = values;
            RightValues = rightValues;
            DefaultResult = defaultResult;
            JoinResultsAsOr = joinResultsAsOr;
        }

        public ControllerItem(string name, ControllerCompareOperator compareOperator, List<ControllerItem> childs)
        {
            Name = name;
            CompareOperator = compareOperator;
            Childs = childs;
        }

        public ControllerCompareOperator CompareOperator { get; set; }
        public ControllerActiveType ActiveType { get; set; }
        public List<ControllerItemValue> ActiveControls { get; protected set; }
        public List<ControllerItemValue> Values { get; protected set; }
        public List<ControllerItemValue> RightValues { get; protected set; }
        public List<ControllerItem> Childs { get; protected set; }
        public bool Result { get; set; }
        public bool DefaultResult { get; set; }
        public bool JoinResultsAsOr { get; set; }
        public string Name { get; set; }

        public void InitValues(IEnumerable<RenderContext> context, Controller controller)
        {
            var items = RightValues == null ? Values : Values.Union(RightValues);
            if (items != null)
            {
                foreach (var item in items)
                    item.InitValues(context, controller);
            }

            if (ActiveControls != null)
            {
                foreach (var item in ActiveControls)
                    item.InitValues(context, controller);
            }

            if (Childs != null)
            {
                foreach (var child in Childs)
                    child.InitValues(context, controller);
            }
        }

        public void ComputeActive(Controller controller)
        {
            bool? result = null;
            switch (CompareOperator)
            {
                case ControllerCompareOperator.Equal:
                case ControllerCompareOperator.NotEqual:
                case ControllerCompareOperator.GreaterThan:
                case ControllerCompareOperator.GreaterThanEqual:
                case ControllerCompareOperator.LessThan:
                case ControllerCompareOperator.LessThanEqual:
                    foreach (var value in Values.Where(r => r.IsInited).SelectMany(r => r.Values))
                        foreach (var rightValue in RightValues.Where(r => r.IsInited).SelectMany(r => Values))
                        {
                            bool subRes = false;

                            #region compare

                            switch (CompareOperator)
                            {
                                case ControllerCompareOperator.Equal:
                                    subRes = (value == null && rightValue == null)
                                             ||
                                             (value != null && value.Equals(rightValue));
                                    break;
                                case ControllerCompareOperator.NotEqual:
                                    subRes = (value == null && rightValue != null)
                                             ||
                                             (value != null && !value.Equals(rightValue));
                                    break;
                                case ControllerCompareOperator.GreaterThan:
                                    if (value != null && rightValue != null)
                                    {
                                        var v1 = Convert.ToDecimal(value);
                                        var v2 = Convert.ToDecimal(rightValue);
                                        subRes = v1 > v2;
                                    }
                                    break;
                                case ControllerCompareOperator.GreaterThanEqual:
                                    if (value != null && rightValue != null)
                                    {
                                        var v1 = Convert.ToDecimal(value);
                                        var v2 = Convert.ToDecimal(rightValue);
                                        subRes = v1 >= v2;
                                    }
                                    break;
                                case ControllerCompareOperator.LessThan:
                                    if (value != null && rightValue != null)
                                    {
                                        var v1 = Convert.ToDecimal(value);
                                        var v2 = Convert.ToDecimal(rightValue);
                                        subRes = v1 < v2;
                                    }
                                    break;
                                case ControllerCompareOperator.LessThanEqual:
                                    if (value != null && rightValue != null)
                                    {
                                        var v1 = Convert.ToDecimal(value);
                                        var v2 = Convert.ToDecimal(rightValue);
                                        subRes = v1 <= v2;
                                    }
                                    break;
                                default:
                                    throw new ArgumentOutOfRangeException();
                            }

                            #endregion

                            if (JoinResultsAsOr)
                                result = (result ?? false) | subRes;
                            else
                                result = (result ?? true) & subRes;
                        }
                    break;
                case ControllerCompareOperator.IsEmpty:
                    foreach (var value in Values.Where(r => r.IsInited).SelectMany(r => r.Values))
                    {
                        bool subRes = value == null || "".Equals(value);
                        if (JoinResultsAsOr)
                            result = (result ?? false) | subRes;
                        else
                            result = (result ?? true) & subRes;
                    }
                    break;
                case ControllerCompareOperator.IsNotEmpty:
                    foreach (var value in Values.Where(r => r.IsInited).SelectMany(r => r.Values))
                    {
                        bool subRes = value != null && !"".Equals(value);
                        if (JoinResultsAsOr)
                            result = (result ?? false) | subRes;
                        else
                            result = (result ?? true) & subRes;
                    }
                    break;
                case ControllerCompareOperator.Or:
                    foreach (var item in Childs)
                    {
                        item.ComputeActive(controller);
                        if (result == null)
                            result = item.Result;
                        else
                            result |= item.Result;
                        if (result.Value) break;
                    }
                    break;
                case ControllerCompareOperator.And:
                    foreach (var item in Childs)
                    {
                        item.ComputeActive(controller);
                        if (result == null)
                            result = item.Result;
                        else
                            result &= item.Result;
                    }
                    break;
                case ControllerCompareOperator.IsEnabled:
                    break;
                case ControllerCompareOperator.IsDisabled:
                    break;
                case ControllerCompareOperator.IsHidden:
                    break;
                case ControllerCompareOperator.IsShown:
                    break;
                case ControllerCompareOperator.IsReadOnly:
                    break;
                case ControllerCompareOperator.IsNotReadOnly:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            Result = result ?? DefaultResult;

            SetEnabled(controller);
        }

        private void SetEnabled(Controller controller)
        {
            foreach (ControllerActiveType activeType in Enum.GetValues(typeof(ControllerActiveType)))
            {
                if (activeType == ControllerActiveType.None || (ActiveType & activeType) != activeType) continue;
                
                foreach (var activeControl in ActiveControls)
                {
                    if (activeControl.Controls != null)
                        foreach (var control in activeControl.Controls)
                        {
                            ControllerControlItem item;
                            if (!controller.ActiveControls.ContainsKey(control.ClientID))
                                item = controller.ActiveControls[control.ClientID] = new ControllerControlItem(control);
                            else
                                item = controller.ActiveControls[control.ClientID];

                            if (!item.Enabled.ContainsKey(activeType) || item.Enabled[activeType] == null)
                                item.Enabled[activeType] = Result;
                            else if (item.JoinControllersAsOr)
                                item.Enabled[activeType] |= Result;
                            else
                                item.Enabled[activeType] &= Result;
                        }
                    if (activeControl.ClientIDs != null)
                    {
                        var clientIDs = activeControl.ClientIDs;
                        if (activeControl.Controls != null)
                            clientIDs = clientIDs.
                                Where(r => activeControl.Controls.FirstOrDefault(c => c.ClientID == r) == null);

                        foreach (var clientID in clientIDs)
                        {
                            ControllerControlItem item;
                            if (!controller.ActiveControls.ContainsKey(clientID))
                                item = controller.ActiveControls[clientID] = new ControllerControlItem(clientID);
                            else
                                item = controller.ActiveControls[clientID];

                            if (!item.Enabled.ContainsKey(activeType) || item.Enabled[activeType] == null)
                                item.Enabled[activeType] = Result;
                            else if (item.JoinControllersAsOr)
                                item.Enabled[activeType] |= Result;
                            else
                                item.Enabled[activeType] &= Result;
                        }
                    }
                }
            }
        }

        public void GetJavaScript(StringBuilder sb, Dictionary<string, bool> list)
        {
            sb.Append("//");
            sb.AppendLine(Name);
            sb.AppendLine("var activeControls = new Array();");
            if (ActiveControls != null)
            {
                foreach (var clientID in ActiveControls.SelectMany(r => r.ClientIDs))
                {
                    sb.AppendFormat("Array.add(activeControls, $get('{0}'));", clientID);
                    sb.AppendLine();
                }
            }

            sb.AppendLine();

            #region инициализация колекций values, rightValues

            switch (CompareOperator)
            {
                case ControllerCompareOperator.NotEqual:
                case ControllerCompareOperator.Equal:
                case ControllerCompareOperator.GreaterThan:
                case ControllerCompareOperator.GreaterThanEqual:
                case ControllerCompareOperator.LessThan:
                case ControllerCompareOperator.LessThanEqual:
                    GetJavaScript_AddGetValues(sb, list);
                    GetJavaScript_AddGetRightValues(sb, list);
                    break;
                case ControllerCompareOperator.IsEmpty:
                case ControllerCompareOperator.IsNotEmpty:
                    GetJavaScript_AddGetValues(sb, list);
                    sb.AppendLine("var rightValues = new Array();");
                    sb.AppendLine("Array.add(rightValues,'');");
                    break;
                case ControllerCompareOperator.Or:
                    //todo: реализовать ControllerCompareOperator.Or
                    break;
                case ControllerCompareOperator.And:
                    //todo: реализовать ControllerCompareOperator.And
                    break;
                case ControllerCompareOperator.IsEnabled:
                    break;
                case ControllerCompareOperator.IsDisabled:
                    break;
                case ControllerCompareOperator.IsHidden:
                    break;
                case ControllerCompareOperator.IsShown:
                    break;
                case ControllerCompareOperator.IsReadOnly:
                    break;
                case ControllerCompareOperator.IsNotReadOnly:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            #endregion

            #region получение оператора сравнения opStr

            var opStr = "";
            switch (CompareOperator)
            {
                case ControllerCompareOperator.Equal:
                    opStr = "==";
                    break;
                case ControllerCompareOperator.NotEqual:
                    opStr = "!=";
                    break;
                case ControllerCompareOperator.GreaterThan:
                    opStr = ">";
                    break;
                case ControllerCompareOperator.GreaterThanEqual:
                    opStr = ">=";
                    break;
                case ControllerCompareOperator.LessThan:
                    opStr = "<";
                    break;
                case ControllerCompareOperator.LessThanEqual:
                    opStr = "<";
                    break;
                case ControllerCompareOperator.IsEmpty:
                    opStr = "==";
                    break;
                case ControllerCompareOperator.IsNotEmpty:
                    opStr = "!=";
                    break;
                case ControllerCompareOperator.Or:
                    opStr = "||";
                    break;
                case ControllerCompareOperator.And:
                    opStr = "&&";
                    break;
                case ControllerCompareOperator.IsEnabled:
                    break;
                case ControllerCompareOperator.IsDisabled:
                    break;
                case ControllerCompareOperator.IsHidden:
                    break;
                case ControllerCompareOperator.IsShown:
                    break;
                case ControllerCompareOperator.IsReadOnly:
                    break;
                case ControllerCompareOperator.IsNotReadOnly:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            #endregion

            sb.AppendFormat("var result = checkArray(values, rightValues, {0}, '{1}');", JoinResultsAsOr.ToString().ToLower(), opStr);
            sb.AppendLine();
            sb.AppendFormat("if (result == null) result = {0};", DefaultResult.ToString().ToLower());
            sb.AppendLine();

            foreach (ControllerActiveType activeType in Enum.GetValues(typeof(ControllerActiveType)))
            {
                if (activeType == ControllerActiveType.None || (ActiveType & activeType) != activeType) continue;
                switch (activeType)
                {
                    case ControllerActiveType.None:
                        break;
                    case ControllerActiveType.Enabled:
                        sb.AppendLine("setValuesToArray(activeControls, 'disabled', !result);");
                        break;
                    case ControllerActiveType.ReadOnly:
                        sb.AppendLine("setValuesToArray(activeControls, 'readonly', !result);");
                        break;
                    case ControllerActiveType.ValidationDisabled:
                        sb.AppendLine("disableValidationToArray(activeControls, !result);");
                        break;
                    case ControllerActiveType.Hide:
                        sb.AppendLine("setValuesToArray(activeControls, 'style.display', result ? '\"\"' : '\"none\"');"); 
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
                sb.AppendLine();
            }
            sb.AppendLine();
        }

        private void GetJavaScript_AddGetValues(StringBuilder sb, Dictionary<string, bool> list)
        {
            sb.AppendLine("var values = new Array();");
            if (Values == null)
                return;

            foreach (var value in Values.Where(r => r.IsInited).SelectMany(r => r.ValuesWithoutControl))
            {
                sb.AppendFormat("Array.add(values, '{0}');", value);
                sb.AppendLine();
            }

            foreach (var clientID in Values.Where(r => r.IsInited).SelectMany(r => r.ClientIDs))
            {
                list[clientID] = true;
                sb.AppendFormat("var valueConrol = $get('{0}'); if (valueConrol != null) Array.add(values, valueConrol.value);", clientID);
                sb.AppendLine();
            }

            sb.AppendLine();
        }

        private void GetJavaScript_AddGetRightValues(StringBuilder sb, Dictionary<string, bool> list)
        {
            sb.AppendLine("var rightValues = new Array();");
            foreach (var value in RightValues.Where(r => r.IsInited).SelectMany(r => r.ValuesWithoutControl))
            {
                sb.AppendFormat("Array.add(rightValues, '{0}');", value);
                sb.AppendLine();
            }
            foreach (var clientID in RightValues.Where(r => r.IsInited).SelectMany(r => r.ClientIDs))
            {
                list[clientID] = true;
                sb.AppendFormat("var valueConrol = $get('{0}'); if (valueConrol != null) Array.add(rightValues, valueConrol.value);", clientID);
                sb.AppendLine();
            }
            sb.AppendLine();
        }
    }
}
