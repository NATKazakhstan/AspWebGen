/*
 * Created by: Denis M. Silkov
 * Created: 19 сентября 2007 г.
 */

using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Web.UI;
using System.Web.UI.WebControls;
using AjaxControlToolkit;
using Nat.Tools.Data;
using Nat.Web.Controls.Properties;
using Nat.Web.Tools;

[assembly: WebResource("Nat.Web.Controls.DropDownListExtBehavior.js", "text/javascript")]

namespace Nat.Web.Controls
{
    /// <summary>
    /// Расширенный DropDownList.
    /// </summary>
    [ClientScriptResource("Nat.Web.Controls.DropDownListExtBehavior", "Nat.Web.Controls.DropDownListExtBehavior.js")]
    [DefaultProperty("SelectedValueObject")]
    public class DropDownListExt : DropDownList, IScriptControl
    {
        private readonly ListItem nullItem = new ListItem(Resources.SNotSpecified, "");
        private bool includeNullItem = true;
        private NullValueType nullValue = NullValueType.Null;
        private string selectedValueHistory;
        private string selectedValue;
        private bool _allowSetHistoricalValue = true;
        private int _historicalIndexValue = 0;

        /// <summary>
        /// Включать в список null-элемент.
        /// </summary>
        [Category("Behavior")]
        [DefaultValue(true)]
        [Description("Включать в список null-элемент.")]
        public bool IncludeNullItem
        {
            get { return includeNullItem; }
            set
            {
                if(includeNullItem != value)
                {
                    includeNullItem = value;
                    if(value)
                    {
                        if(base.Items.IndexOf(nullItem) < 0)
                            base.Items.Insert(0, nullItem);
                    }
                    else
                    {
                        if(base.Items.IndexOf(nullItem) > -1)
                            base.Items.Remove(nullItem);
                    }
                }
            }
        }

        private bool inItems = false;

        public override ListItemCollection Items
        {
            get
            {
                if(IncludeNullItem && DesignMode && base.Items.IndexOf(nullItem) > -1)
                {
                    ListItemCollection items = new ListItemCollection();
                    if(base.Items.Count > 1)
                    {
                        ListItem[] itemArray = new ListItem[base.Items.Count - 1];
                        base.Items.CopyTo(itemArray, 1);
                        items.AddRange(itemArray);
                    }

                    return items;
                }
                if (!inItems)
                    try
                    {
                        inItems = true;
                        var items = Items;
                        var indexOf = items.IndexOf(nullItem);
                        if (IncludeNullItem && (items.Count == 0 || indexOf == -1))
                            items.Insert(0, nullItem);
                        if(indexOf > 0)
                        {
                            items.RemoveAt(indexOf);
                            items.Insert(0, nullItem);
                        }
                    }
                    finally
                    {
                        inItems = false;
                    }

                return base.Items;
            }
        }

        /// <summary>
        /// Текст для null-элемента.
        /// </summary>
        [Category("Behavior")]
        [Description("Текст для null-элемента.")]
        [Localizable(true)]
        public string NullText
        {
            get { return nullItem.Text; }
            set { nullItem.Text = value; }
        }

        /// <summary>
        /// При использование загрузки устаревших данных указывается индекс для указания текущего значения.
        /// </summary>
        [DefaultValue(0)]
        [Description("При использование получения устаревших данных указывается индекс значения")]
        public int HistoricalIndexValue
        {
            get { return _historicalIndexValue; }
            set { _historicalIndexValue = value; }
        }

        /// <summary>
        /// Разрешить использование загрузки устаревших данных.
        /// </summary>
        [DefaultValue(true)]
        [Description("Разрешить использование загрузку устаревших данных")]
        [Category("Behavior")]
        public bool AllowSetHistoricalValue
        {
            get { return _allowSetHistoricalValue; }
            set { _allowSetHistoricalValue = value; }
        }

        [Bindable(true)]
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public new object SelectedValue
        {
            get
            {
                if(IncludeNullItem)
                    return (SelectedItem == null || SelectedItem.Value == nullItem.Value) ? GetNullValue() : SelectedItem.Value;
                
                return base.SelectedValue;
            }
            set
            {
                selectedValue = (value ?? "").ToString();
                if(IncludeNullItem && IsNullValue(value))
                {
                    SelectedIndex = 0;
                    selectedValueHistory = "";
                }
                else
                {
                    if(value == null)
                    {
                        base.SelectedValue = null;
                        return;
                    }
                    if (_allowSetHistoricalValue && Items.FindByValue(value.ToString()) == null)
                    {
                        TableDataSourceView data = GetData() as TableDataSourceView;
                        if (data != null && data.HistoricalCountKeys > _historicalIndexValue)
                        {
                            selectedValueHistory = value.ToString();
                            try
                            {
                                DataBind();
                            }
                            finally
                            {
                                selectedValueHistory = null;
                            }
                        }
                    }
                    base.SelectedValue = value.ToString();
                }
            }
        }

        public object SelectedValueObject
        {
            get { return SelectedValue; }
            set { SelectedValue = value; }
        }

        [Browsable(false)]
        public string SelectedText
        {
            get
            {
                return SelectedItem == null ? null : SelectedItem.Text;
            }
            set
            {
                //TODO: СОВСЕМ ПУСТОЙ СЕТТЕР !!!
            }
        }
        
        [Localizable(true)]
        public override string DataTextField
        {
            get 
            {
/*                if (LocalizationHelper.IsCultureKZ)
                {
                    if (base.DataTextField == "nameRu")
                        return "nameKz";
                    if (base.DataTextField.StartsWith("nameRu"))
                        return "nameKz" + base.DataTextField.Substring(6);
                    if (base.DataTextField.EndsWith("Ru"))
                        return base.DataTextField.Substring(0, base.DataTextField.Length - 2) + "Kz";
                }*/
                return base.DataTextField; 
            }
            set 
            {
                if (LocalizationHelper.IsCultureKZ && !string.IsNullOrEmpty(value))
                {
                    if (!string.IsNullOrEmpty(DataTextFieldKz))
                        value = DataTextFieldKz;
                    else if (value == "nameRu")
                        value = "nameKz";
                    else if (value.StartsWith("nameRu"))
                        value = "nameKz" + value.Substring(6);
                    else if (value.EndsWith("Ru"))
                        value = value.Substring(0, value.Length - 2) + "Kz";
                }
                base.DataTextField = value; 
            }
        }

        public string DataTextFieldKz { get; set; }

        /// <summary>
        /// Тип пустого значения.
        /// </summary>
        [Category("Behavior")]
        [DefaultValue(NullValueType.Null)]
        [Description("Тип пустого значения.")]
        public NullValueType NullValue
        {
            get { return nullValue; }
            set
            {
                nullValue = value;
                object strNullValue = GetNullValue();
                if (strNullValue != null)
                    nullItem.Value = strNullValue.ToString();
                else
                    nullItem.Value = null;
            }
        }
        
        /// <summary>
        /// Возвращает пустое значение в зависимости от свойства NullValue.
        /// </summary>
        /// <returns>Пустое значение.</returns>
        private object GetNullValue()
        {
            switch(nullValue)
            {
                case NullValueType.Null:
                    return null;
                case NullValueType.DBNull:
                    return DBNull.Value;
                case NullValueType.Zero:
                    return 0;
                default:
                    throw new Exception(string.Format("Неизвестный тип пустого значения ({0}).", nullValue));
            }
        }

        /// <summary>
        /// Проверяет, является ли значение "пустым" (null, DBNull либо текущий NullValue).
        /// </summary>
        /// <param name="value">Значение.</param>
        /// <returns>true, если значение "пустое".</returns>
        private bool IsNullValue(object value)
        {
            if(value == null || value == DBNull.Value)
                return true;

            return value.Equals(GetNullValue());
        }

        /// <summary>
        /// Сбрасывает текст на значение по умолчанию.
        /// </summary>
        internal void ResetNullText()
        {
            NullText = Resources.SNotSpecified;
        }

        /// <summary>
        /// Возвращает true, если нужно сериализовать свойство NullText.
        /// </summary>
        /// <returns></returns>
        internal bool ShouldSerializeNullText()
        {
            return NullText != Resources.SNotSpecified;
        }

        #region Перекрытые методы

        protected override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);

            AddCustomAttributesToItems();

            if (!DesignMode && Visible)
            {
                ToolTip = Text;
                ScriptManager.GetCurrent(Page).RegisterScriptControl(this);
            }
        }

        protected override void Render(HtmlTextWriter writer)
        {
            if (!DesignMode && Visible)
            {
                ScriptManager.GetCurrent(Page).RegisterScriptDescriptors(this);
            }
            base.Render(writer);
        }

        protected override void OnDataBinding(EventArgs e)
        {
            bool setValue;
            TableDataSourceView data;
            var value = BeforeOnDataBinding(out setValue, out data);

            base.OnDataBinding(e);

            AfterDataBinding(setValue, data, value);
        }

        private void AfterDataBinding(bool setValue, TableDataSourceView data, object value)
        {
            if (setValue) data.HistoricalValues[_historicalIndexValue] = value;

            if (IncludeNullItem && (Items.Count == 0 || Items[0] != nullItem))
            {
                Items.Insert(0, nullItem);
                SelectedValue = selectedValue;
            }
        }

        private object BeforeOnDataBinding(out bool setValue, out TableDataSourceView data)
        {
            object value = null;
            setValue = false;
            data = null;
            if (Items.Count > -1 && selectedValueHistory != null)
            {
                data = GetData() as TableDataSourceView;
                if (data != null && data.HistoricalCountKeys > _historicalIndexValue)
                {
                    value = data.HistoricalValues[_historicalIndexValue];
                    data.HistoricalValues[_historicalIndexValue] = selectedValueHistory;
                    setValue = true;
                }
            }

            if (IncludeNullItem && (Items.Count == 0 || Items[0].Value != nullItem.Value))
            {
                Items.Insert(0, nullItem);
                AppendDataBoundItems = true;
            }

            return value;
        }

        # endregion

        #region IScriptControl Members

        public IEnumerable<ScriptDescriptor> GetScriptDescriptors()
        {
            if (Page != null && Visible)
            {
                ScriptBehaviorDescriptor desc = new ScriptBehaviorDescriptor("Nat.Web.Controls.DropDownListExtBehavior", ClientID);

                desc.ID = String.Format("{0}_DDL", ClientID);

                yield return desc;
            }
        }

        public IEnumerable<ScriptReference> GetScriptReferences()
        {
            var references = new List<ScriptReference>();
            references.AddRange(ScriptObjectBuilder.GetScriptReferences(GetType()));
            return references;
        }

        #endregion

        protected override void OnDataBound(EventArgs e)
        {
            base.OnDataBound(e);
            AddCustomAttributesToItems();
        }

        private void AddCustomAttributesToItems()
        {
            if (_titles != null)
            {
                foreach (ListItem item in Items)
                {
                    if (_titles.ContainsKey(item.Value))
                        item.Attributes["title"] = _titles[item.Value];
                }
            }

            if (_valuesCollection != null)
            {
                foreach (ListItem item in Items)
                {
                    if (_valuesCollection.ContainsKey(item.Value))
                    {
                        foreach (var value in _valuesCollection[item.Value])
                            item.Attributes[value.Key] = value.Value;
                    }
                }
            }
        }

        protected override void PerformDataBinding(IEnumerable dataSource)
        {
            if ((string.IsNullOrEmpty(DataDescriptionField) && string.IsNullOrEmpty(DataValuesCollectionField))
                || string.IsNullOrEmpty(DataValueField))
            {
                base.PerformDataBinding(dataSource);
                return;
            }
            var list = new List<object>();
            if (!string.IsNullOrEmpty(DataDescriptionField))
                _titles = new Dictionary<string, string>();
            if (!string.IsNullOrEmpty(DataValuesCollectionField))
                _valuesCollection = new Dictionary<string, Dictionary<string, string>>();
            foreach (var source in dataSource)
            {
                list.Add(source);
                Object obj = source;
                if (!string.IsNullOrEmpty(DataDescriptionField))
                {
                    foreach (var field in DataDescriptionField.Split('.'))
                    {
                        obj = DataBinder.GetPropertyValue(obj, field);
                        if (obj == null) break;
                    }

                    var title = (obj ?? "").ToString();
                    _titles[(DataBinder.GetPropertyValue(source, DataValueField) ?? "").ToString()] = title;
                }

                if (!string.IsNullOrEmpty(DataValuesCollectionField))
                {
                    int i = 0;
                    foreach (var dataValueField in DataValuesCollectionField.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
                    {
                        obj = source;
                        foreach (var field in dataValueField.Split('.'))
                            obj = DataBinder.GetPropertyValue(obj, field);
                        var value = (obj ?? "").ToString();
                        string key = (DataBinder.GetPropertyValue(source, DataValueField) ?? "").ToString();
                        if (!_valuesCollection.ContainsKey(key)) _valuesCollection[key] = new Dictionary<string, string>();
                        _valuesCollection[key]["valueColl" + i++] = value;
                    }
                }
            }
            base.PerformDataBinding(list);
        }

        public string DataDescriptionField { get; set; }
        private IDictionary<string, string> _titles;
        public string DataValuesCollectionField { get; set; }
        private IDictionary<string, Dictionary<string, string>> _valuesCollection;

        protected override void LoadViewState(object savedState)
        {
            if (savedState == null)
                base.LoadViewState(null);
            else
            {
                var triplet = (Triplet)savedState;
                _titles = (IDictionary<string, string>)triplet.First;
                _valuesCollection = (IDictionary<string, Dictionary<string, string>>)triplet.Second;
                base.LoadViewState(triplet.Third);
            }
        }

        protected override object SaveViewState()
        {
            return new Triplet(_titles, _valuesCollection, base.SaveViewState());
        }

        public void ReloadData()
        {
            Items.Clear();
            bool setValue;
            TableDataSourceView data;
            var value = BeforeOnDataBinding(out setValue, out data);

            var dataSourceView = GetData();
            dataSourceView.Select(new DataSourceSelectArguments(), PerformDataBinding);

            RequiresDataBinding = false;

            AfterDataBinding(setValue, data, value);
            AddCustomAttributesToItems();
        }
    }
}
