/*
 * Created by : Daniil Kovalev
 * Created    : 28.11.2007
 */

using System;
using System.Collections.Generic;
using System.Data;
using System.Web.UI;
using System.Web.UI.WebControls;
using AjaxControlToolkit;
using Nat.Controls.DataGridViewTools;
using Nat.Tools.Constants;
using Nat.Tools.Data;
using Nat.Tools.Filtering;
using Nat.Tools.ResourceTools;
using Nat.Tools.System;
using Nat.Tools.Validation;
using Nat.Web.Controls.DateTimeControls;
using Nat.Web.Controls.Properties;
using Nat.Web.Tools;


#region Resources

[assembly: WebResource("Nat.Web.Controls.Filters.ColumnFilter.css", "text/css", PerformSubstitution = true)]
[assembly: WebResource("Nat.Web.Controls.Filters.ColumnFilter.png", "image/png")]
[assembly: WebResource("Nat.Web.Controls.Filters.ColumnFilter.js", "text/javascript")]

#endregion


namespace Nat.Web.Controls
{
    using System.ComponentModel;
    using System.Linq;

    using Nat.Tools.Classes;

    [ClientCssResource("Nat.Web.Controls.Filters.ColumnFilter.css")]
    [ClientScriptResource("Nat.Web.Controls.ColumnFilter", "Nat.Web.Controls.Filters.ColumnFilter.js")]
    public class ColumnFilter : WebControl, IScriptControl, INamingContainer,
                                IColumnFilter, ISupportSessionWorker, ISupportPostBack, IDefaultFilterValues, IColumnFilterStorageChanged
    {
        #region Fields

        private WebControl[] controls;
        private DropDownList dropDownList;
        private TableDataSource tableDataSource;
        private SessionWorker sessionWorker;
        private Table table;
        private string errorText;
        private Boolean storageChanged;
        private ColumnFilterStorage columnFilterStorage;
        private Boolean postBack;
        private bool inited;
        private CheckBox checkBoxForFilterCondition;
        TableRow filterRow;

        #endregion

        public ColumnFilter()
        {
            DefaultFilterType = ColumnFilterType.NotSet;
        }

        public event EventHandler<EventArgs> ColumnFilterStorageChanged;

        public Dictionary<ColumnFilterType, string> CustomColumnFilterTypeCaptions { get; set; }

        /// <summary>
        /// Имя условия из CustomConditions, для фильтрации текущего списка полей. Для этого фильтра создается CheckBox.
        /// </summary>
        public string CheckedFilterCondition { get; set; }

        /// <summary>
        /// Всплывающая подсказка для CheckBox, создаваемого на основании свойства CheckedFilterCondition.
        /// </summary>
        public string CheckedFilterConditionTooltip { get; set; }

        public Unit? TextBoxHeight { get; set; }

        #region Methods

        protected virtual void OnColumnFilterStorageChanged()
        {
            var handler = this.ColumnFilterStorageChanged;
            if (handler != null) handler(this, EventArgs.Empty);
        }

        protected override void CreateChildControls()
        {
            base.CreateChildControls();

            // Ensure ColumnFilterStorage
            if(ColumnFilterStorage == null)
                throw new NullReferenceException("ColumnFilterStorage must be specified");

            // Create controls hierarchy
            table = new Table();
            Controls.Add(table);
            table.Width = Width != Unit.Empty ? Width : Unit.Percentage(100);
            table.Style["table-layout"] = "fixed";
            //table.CssClass = "ms-gridtext";
            filterRow = new TableRow();
            filterRow.Cells.Add(new TableCell());
            filterRow.Cells.Add(new TableCell());

            int countItems;
            if ((ColumnFilterStorage.IsRefBound && ((ColumnFilterStorage.AvailableFilters & ColumnFilterType.Between) == 0))
                || ColumnFilterStorage.DataType == typeof(String))
            {
                countItems = 1;
                filterRow.Cells.Add(new TableCell { ColumnSpan = 2, Width = Unit.Percentage(100) });
                filterRow.Cells.Add(new TableCell { ID = "fillEmpty" });
            }
            else
            {
                countItems = 2;
                filterRow.Cells.Add(new TableCell { Width = Unit.Pixel(150) });
                filterRow.Cells.Add(new TableCell { Width = Unit.Pixel(150) });
                filterRow.Cells.Add(new TableCell { ID = "fillEmpty" });
            }

            dropDownList = new DropDownList();
            dropDownList.ID = "dropDownListID";
            filterRow.Cells[1].Controls.Add(dropDownList);

            controls = new WebControl[2];

            for(int i = 0; i != countItems; i++)
            {
                if(ColumnFilterStorage.IsRefBound)
                {
                    DataTable filterRefTable = DataSourceHelper.GetDataTable(ColumnFilterStorage.RefDataSource);

                    if (filterRefTable != null)
                    {
                        Type tableAdapterType = TableAdapterTools.GetTableAdapterType(filterRefTable.GetType());

                        tableDataSource = new TableDataSource();
                        tableDataSource.ID = String.Format("tableDataSource{0}_ID", i);
                        tableDataSource.TypeName = tableAdapterType.FullName;
                        tableDataSource.SelectMethod = ColumnFilterStorage.SelectMethod;
                        tableDataSource.FillType = TableDataSourceFillType.ParametersNotChanged;

                        if (SessionWorker == null)
                        {
                            SessionWorker sw = new SessionWorker(this.Page, Guid.NewGuid().ToString());
                            sw.Object = filterRefTable.DataSet;
                            SessionWorker = sw;
                        }

                        tableDataSource.SessionWorker = SessionWorker;
                        tableDataSource.SetFilterByCustomConditions = false;
                        tableDataSource.View.CustomConditions.AddRange(ColumnFilterStorage.CustomConditions);
                        tableDataSource.HistoricalCountKeys = 0;

                        if (filterRefTable.Columns.IndexOf("dateEnd") != -1 && filterRefTable.Columns.IndexOf("dateStart") != -1)
                            tableDataSource.ShowHistoricalData = true;

                        filterRow.Cells[2 + i].Controls.Add(tableDataSource);

                        // This is only for compability with SMSES
                        ColumnFilterStorage.RefTableRolledIn =
                            (bool)(DataSetResourceManager.GetTableExtProperty(filterRefTable, TableExtProperties.ROLLED_IN) ?? false);
                    }
                    else
                        ColumnFilterStorage.RefTableRolledIn = false;

                    if (String.IsNullOrEmpty(ColumnFilterStorage.ValueColumn) || String.IsNullOrEmpty(ColumnFilterStorage.DisplayColumn))
                        throw new Exception("FILTER_REF_DISPLAY_COLUMN and FILTER_REF_VALUE_COLUMN attribute must be specified");

                    if (!string.IsNullOrEmpty(CheckedFilterCondition))
                    {
                        checkBoxForFilterCondition = new CheckBox
                                                     {
                                                         ID = "checkBoxForFilter",
                                                         Text = CheckedFilterConditionTooltip,
                                                         AutoPostBack = true,
                                                         TextAlign = TextAlign.Right,
                                                     };
                        checkBoxForFilterCondition.CheckedChanged += OnCheckBoxForFilterConditionOnCheckedChanged;
                    }

                    if(ColumnFilterStorage.RefTableRolledIn)
                    {
                        LookupTextBox lookupTextBox = new LookupTextBox();
                        lookupTextBox.DataSource = tableDataSource;
                        lookupTextBox.DataTextField = ColumnFilterStorage.DisplayColumn;
                        lookupTextBox.DataValueField = ColumnFilterStorage.ValueColumn;
                        String Relation = (String)(DataSetResourceManager.GetTableExtProperty(filterRefTable, TableExtProperties.TREE_REF_RELATION) ?? "");
                        if (!String.IsNullOrEmpty(Relation))
                        {
                            String dataDisableRowField = (String)(DataSetResourceManager.GetTableExtProperty(filterRefTable, TableExtProperties.TREE_ALLOW_FIELD));

                            lookupTextBox.GridTreeMode = true;
                            if (!string.IsNullOrEmpty(dataDisableRowField))
                                lookupTextBox.DataDisableRowField = dataDisableRowField;
                        }
                        controls[i] = lookupTextBox;
                    }
                    else
                    {
                        var lookupList = new DropDownListExt();
                        if (tableDataSource != null)
                        {
                            lookupList.DataSource = tableDataSource;
                            tableDataSource.FillType = TableDataSourceFillType.Always;
                        }
                        else
                            lookupList.DataSource = ColumnFilterStorage.RefDataSource;
                        lookupList.DataTextField = ColumnFilterStorage.DisplayColumn;
                        lookupList.DataValueField = ColumnFilterStorage.ValueColumn;
                        lookupList.IncludeNullItem = true;
                        lookupList.DataBind();
                        controls[i] = lookupList;
                    }
                }
                else if (ColumnFilterStorage.DataType == typeof(Int64) ||
                         ColumnFilterStorage.DataType == typeof(Int32) ||
                         ColumnFilterStorage.DataType == typeof(Int16) ||
                         ColumnFilterStorage.DataType == typeof(Byte) ||
                         ColumnFilterStorage.DataType == typeof(Double) ||
                         ColumnFilterStorage.DataType == typeof(Decimal) ||
                         ColumnFilterStorage.DataType == typeof(Single) ||
                         ColumnFilterStorage.DataType == typeof(String))
                {
                    var textBox = new TextBox();
                    controls[i] = textBox;
                    if (TextBoxHeight != null)
                    {
                        textBox.Height = TextBoxHeight.Value;
                        textBox.TextMode = TextBoxMode.MultiLine;
                    }
                }
                else if(ColumnFilterStorage.DataType == typeof(DateTime))
                {
                    DatePicker datePicker = new DatePicker();

                    switch(ColumnFilterStorage.DateTimeFormat)
                    {
                        case "{0:d}":
                            datePicker.Mode = DatePickerMode.Date;
                            break;
                        case "{0:t}":
                            datePicker.Mode = DatePickerMode.Time;
                            break;
                        case "{0:f}":
                            datePicker.Mode = DatePickerMode.DateTime;
                            break;
                    }
                    datePicker.PopupBehaviorParentNode = PopupBehaviorParentNode;
                    datePicker.Width = Unit.Pixel(150);
                    controls[i] = datePicker;
                    ((DatePicker)controls[i]).AutoPostBack = postBack;
                }
                else if(ColumnFilterStorage.DataType == typeof(Boolean))
                {
                    DropDownList list = new DropDownList();
                    list.Items.Add(new ListItem(LookupControlsResources.STrue, true.ToString().ToLower()));
                    list.Items.Add(new ListItem(LookupControlsResources.SFalse, false.ToString().ToLower()));
                    controls[i] = list;
                }
                else
                    throw new Exception(String.Format("Data type not supported: {0}", ColumnFilterStorage.DataType.Name));

                controls[i].ID = String.Format("control{0}ID", i);
                filterRow.Cells[2 + i].Controls.Add(controls[i]);
                controls[i].Width = Unit.Percentage(100);
            }

            // Setup controls' properties
            filterRow.Cells[0].Text = ColumnFilterStorage.Caption;

            //filterRow.Cells[0].BackColor = Color.DarkTurquoise;
            //filterRow.Cells[1].BackColor = Color.DarkSeaGreen;
            //filterRow.Cells[2].BackColor = Color.Salmon;
            //filterRow.Cells[3].BackColor = Color.DodgerBlue;

            filterRow.Cells[0].Style["padding-right"] = "6px";
            filterRow.Cells[1].Style["padding-right"] = "6px";
            filterRow.Cells[2].Style["padding-right"] = "6px";
            filterRow.Cells[2].Style["display"] = "none";
            if (filterRow.Cells.Count > 3 && filterRow.Cells[3].ID != "fillEmpty")
            {
                filterRow.Cells[3].Style["padding-right"] = "6px";
                filterRow.Cells[3].Style["display"] = "none";
            }

            filterRow.Cells[0].Width = Unit.Pixel(170);
            filterRow.Cells[1].Width = Unit.Pixel(140);

            filterRow.Cells[0].HorizontalAlign = HorizontalAlign.Right;

            dropDownList.Width = Unit.Percentage(100);
            dropDownList.AutoPostBack = postBack;
            dropDownList.EnableViewState = false;
            foreach(ColumnFilterType columnFilterType in Enum.GetValues(typeof(ColumnFilterType)))
            {
                if(EnumHelper.Contains(columnFilterType, ColumnFilterStorage.AvailableFilters))
                {
                    ListItem ListItem = new ListItem();
                    ListItem.Value = Convert.ToInt64(columnFilterType).ToString();
                    if (CustomColumnFilterTypeCaptions != null && CustomColumnFilterTypeCaptions.ContainsKey(columnFilterType))
                        ListItem.Text = CustomColumnFilterTypeCaptions[columnFilterType];
                    else
                        ListItem.Text = columnFilterType.GetFilterTypeCaption();
                    dropDownList.Items.Add(ListItem);
                }
            }
            dropDownList.Enabled = dropDownList.Items.Count > 1;

            if (checkBoxForFilterCondition != null)
            {
                var tableRow = new TableRow();
                table.Rows.Add(tableRow);
                tableRow.Cells.Add(new TableCell { Width = filterRow.Cells[0].Width, Height = new Unit(24, UnitType.Pixel), });
                tableRow.Cells.Add(new TableCell { Width = filterRow.Cells[1].Width, Height = new Unit(24, UnitType.Pixel), });
                var tableCell = new TableCell
                                {
                                    Width = filterRow.Cells[2].Width,
                                    ColumnSpan = 2,
                                    Height = new Unit(24, UnitType.Pixel),
                                };
                tableRow.Cells.Add(tableCell);
                tableCell.Controls.Add(checkBoxForFilterCondition);
                checkBoxForFilterCondition.Style["position"] = "relative";
                checkBoxForFilterCondition.Style["top"] = "6px";

                tableRow.Cells[0].Style["padding-right"] = "6px";
                tableRow.Cells[1].Style["padding-right"] = "6px";
                tableRow.Cells[2].Style["padding-right"] = "6px";
            }

            table.Rows.Add(filterRow);
        }

        private void OnCheckBoxForFilterConditionOnCheckedChanged(object sender, EventArgs args)
        {
            var condition =
                tableDataSource.View.CustomConditions.FirstOrDefault(r => r.ColumnName == CheckedFilterCondition);
            if (condition == null)
                return;

            var control = (DropDownList)controls[0];
            var value = control.SelectedValue;
            control.SelectedValue = null;

            condition.EmptyCondition = checkBoxForFilterCondition.Checked;
            control.DataBind();
            var item = control.Items.FindByValue(value);
            if (item != null)
            {
                control.SelectedValue = value;
                ColumnFilterStorage.SetValue(0, null);
                OnColumnFilterStorageChanged();
            }

            storageChanged = true;
        }

        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);
            inited = true;
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            if(ChildControlsCreated)
            {
                // If it's never happen to get here
                // some of the parents controls was created in OnLoad instead OnInit
                UpdateStorage();
                storageChanged = false;
            }
        }
        
        protected override void OnPreRender(EventArgs e)
        {

            if (!DesignMode)
            {
                ScriptManager.GetCurrent(Page).RegisterScriptControl(this);
                ScriptObjectBuilder.RegisterCssReferences(this);
            }

            if (storageChanged)
            {
                UpdateControls();    
            }

            base.OnPreRender(e);
        }

        protected override void Render(HtmlTextWriter writer)
        {
            if(!DesignMode)
                ScriptManager.GetCurrent(Page).RegisterScriptDescriptors(this);
            base.Render(writer);
        }

        private void UpdateControls()
        {
            dropDownList.SelectedValue = Convert.ToInt64(ColumnFilterStorage.FilterType).ToString();

            Int32 controlsCount = 0;

            if (ColumnFilterStorage.FilterType == ColumnFilterType.Between ||
               ColumnFilterStorage.FilterType == ColumnFilterType.OutOf)
            {
                controlsCount = 2;
                filterRow.Cells[2].Style["display"] = "";
                if (filterRow.Cells.Count > 3)
                    filterRow.Cells[3].Style["display"] = "";
            }
            else if (ColumnFilterStorage.FilterType != ColumnFilterType.None)
            {
                controlsCount = 1;
                filterRow.Cells[2].Style["display"] = "";
                if (ColumnFilterStorage.DataType == typeof(String) && !ColumnFilterStorage.IsRefBound)
                {
                    filterRow.Cells[2].Width = Unit.Percentage(100);
                }
                else if (ColumnFilterStorage.DataType == typeof(DateTime))
                {
                    filterRow.Cells[2].Width = Unit.Pixel(110);
                    filterRow.Cells[3].Width = Unit.Pixel(110);
                }
                else if (filterRow.Cells.Count > 3 && filterRow.Cells[3].ID != "fillEmpty")
                {
                    filterRow.Cells[2].Width = Unit.Pixel((Int32) filterRow.Cells[3].Width.Value*2 + 9);
                    filterRow.Cells[3].Style["display"] = "none";
                }
            }
            else
            {
                filterRow.Cells[2].Style["display"] = "none";
                if (filterRow.Cells.Count > 3 && filterRow.Cells[3].ID != "fillEmpty")
                    filterRow.Cells[3].Style["display"] = "none";
            }

            if (ColumnFilterStorage.IsRefBound)
            {
                for (int i = 0; i != controlsCount; i++)
                {
                    if (ColumnFilterStorage.RefTableRolledIn)
                        ((LookupTextBox)controls[i]).Value = ColumnFilterStorage.GetValue(i);
                    else
                    {
                        var selectedValue = ColumnFilterStorage.GetValue(i);
                        var list = ((DropDownList)controls[i]);
                        if (selectedValue != null)
                        {
                            var item = list.Items.FindByValue(Convert.ToString(selectedValue));
                            if (item != null) list.SelectedValue = Convert.ToString(selectedValue);
                            else
                            {
                                ColumnFilterStorage.SetValue(i, null);
                                OnColumnFilterStorageChanged();
                            }
                        }
                    }
                }
            }
            else if (ColumnFilterStorage.DataType == typeof(Int64) ||
                    ColumnFilterStorage.DataType == typeof(Int32) ||
                    ColumnFilterStorage.DataType == typeof(Int16) ||
                    ColumnFilterStorage.DataType == typeof(Byte) ||
                    ColumnFilterStorage.DataType == typeof(Double) ||
                    ColumnFilterStorage.DataType == typeof(Decimal) ||
                    ColumnFilterStorage.DataType == typeof(Single) ||
                    ColumnFilterStorage.DataType == typeof(String))
            {
                for (int i = 0; i != controlsCount; i++)
                    ((TextBox)controls[i]).Text = Convert.ToString(ColumnFilterStorage.GetValue(i));
            }
            else if (ColumnFilterStorage.DataType == typeof(DateTime))
            {
                for (int i = 0; i != controlsCount; i++)
                    ((DatePicker)controls[i]).Date = ColumnFilterStorage.GetValue(i);
            }
            else if (ColumnFilterStorage.DataType == typeof(Boolean))
            {
                for (int i = 0; i != controlsCount; i++)
                    ((DropDownList)controls[i]).SelectedValue = Convert.ToString(ColumnFilterStorage.GetValue(i)).ToLower();
            }

            foreach (var webControl in controls.Where(r => r != null))
            {
                var property = TypeDescriptor.GetProperties(webControl).Find("AutoPostBack", false);
                property?.SetValue(webControl, postBack);
            }
        }

        private void UpdateStorage()
        {
            if (ColumnFilterStorage == null || dropDownList == null || dropDownList.SelectedItem == null) 
                return;
            ColumnFilterStorage.FilterType = (ColumnFilterType)Convert.ToInt64(dropDownList.SelectedItem.Value);

            Int32 controlsCount = 0;

            if (ColumnFilterStorage.FilterType == ColumnFilterType.Between ||
               ColumnFilterStorage.FilterType == ColumnFilterType.OutOf)
            {
                controlsCount = 2;
                filterRow.Cells[2].Style["display"] = "";
                if (filterRow.Cells.Count > 3)
                    filterRow.Cells[3].Style["display"] = "";
            }
            else if (ColumnFilterStorage.FilterType != ColumnFilterType.None)
            {
                controlsCount = 1;
                filterRow.Cells[2].Style["display"] = "";
                if (filterRow.Cells.Count > 3 && filterRow.Cells[3].ID != "fillEmpty")
                {
                    filterRow.Cells[2].Width = Unit.Pixel((Int32) filterRow.Cells[3].Width.Value*2 + 9);
                    filterRow.Cells[3].Style["display"] = "none";
                }
            }
            else
            {
                filterRow.Cells[2].Style["display"] = "none";
                if (filterRow.Cells.Count > 3 && filterRow.Cells[3].ID != "fillEmpty")
                    filterRow.Cells[3].Style["display"] = "none";
            }

            if (ColumnFilterStorage.FilterType == ColumnFilterType.ContainsWords)
                ColumnFilterStorage.Value = (GetValue(0) ?? "").ToString();
            else
                for(int i = 0; i != controlsCount; i++)
                {
                    ColumnFilterStorage.SetValue(i, GetValue(i));
                }

            OnColumnFilterStorageChanged();
        }

        public Object GetValue(int index)
        {
            if (!(index == 0 || index == 1))
                throw new IndexOutOfRangeException("Allowed indices are 0 and 1");

            Object value;


            if (ColumnFilterStorage.IsRefBound)
            {
                if (ColumnFilterStorage.RefTableRolledIn)
                    value = ((LookupTextBox)controls[index]).Value;
                else
                    value = ((DropDownList)controls[index]).SelectedValue;
            }
            else if (ColumnFilterStorage.DataType == typeof(Int64) ||
                    ColumnFilterStorage.DataType == typeof(Int32) ||
                    ColumnFilterStorage.DataType == typeof(Int16) ||
                    ColumnFilterStorage.DataType == typeof(Byte) ||
                    ColumnFilterStorage.DataType == typeof(Double) ||
                    ColumnFilterStorage.DataType == typeof(Decimal) ||
                    ColumnFilterStorage.DataType == typeof(Single) ||
                    ColumnFilterStorage.DataType == typeof(String))
            {
                value = ((TextBox)controls[index]).Text;
            }
            else if (ColumnFilterStorage.DataType == typeof(DateTime))
            {
                value = ((DatePicker)controls[index]).Date;
            }
            else if (ColumnFilterStorage.DataType == typeof(Boolean))
            {
                value = ((DropDownList)controls[index]).SelectedValue;
            }
            else
                throw new NotSupportedException(String.Format(@"Type ""{0}""not supported", ColumnFilterStorage.DataType));


            if (value == null || Equals(value, string.Empty) || value == DBNull.Value)
                return null;

            return Convert.ChangeType(value, ColumnFilterStorage.DataType);
        }

        #endregion


        #region Properties

        public ColumnFilterStorage ColumnFilterStorage
        {
            get { return columnFilterStorage; }
            set
            {
                columnFilterStorage = value;
                storageChanged = true;
                OnColumnFilterStorageChanged();
            }
        }

        //public ColumnFilterStorage ColumnFilterStorage
        //{
        //    get { return (ColumnFilterStorage)ViewState["ColumnFilterStorage"]; }
        //    set
        //    {
        //        ViewState["ColumnFilterStorage"] = value;
        //        storageChanged = true;
        //    }
        //}

        public String PopupBehaviorParentNode
        {
            get { return (String)ViewState["PopupBehaviorParentNode"]; }
            set { ViewState["PopupBehaviorParentNode"] = value; }
        }

        #endregion


        #region IColumnFilter Members

        public event ValidateEventHandler FilterValidate;

        protected virtual void OnFilterValidate(ValidateEventArgs e)
        {
            if (FilterValidate != null)
                FilterValidate(this, e);
        }

        public ColumnFilterStorage GetStorage()
        {
            return ColumnFilterStorage;
        }

        public void SetStorage(ColumnFilterStorage storage)
        {
            ColumnFilterStorage = storage;
            storageChanged = true;
            OnColumnFilterStorageChanged();
        }

        public DataRow GetRow(Int32 index)
        {
            if (columnFilterStorage.IsRefBound)
            {
                throw new NotImplementedException();
            }
            else
                throw new InvalidOperationException();
        }

        public String GetText(Int32 index)
        {
            if(!inited) throw new NotImplementedException();

            if (!(index == 0 || index == 1))
                throw new IndexOutOfRangeException("Allowed indices are 0 and 1");

            if (ColumnFilterStorage.IsRefBound)
            {
                if (ColumnFilterStorage.RefTableRolledIn)
                {
                    return ((LookupTextBox)controls[index]).Text;
                }
                else
                {
                    if (((DropDownList)controls[index]).SelectedItem == null)
                        return string.Empty;
                    else
                        return ((DropDownList) controls[index]).SelectedItem.Text;
                }
            }
            else if (ColumnFilterStorage.DataType == typeof(Int64) ||
                    ColumnFilterStorage.DataType == typeof(Int32) ||
                    ColumnFilterStorage.DataType == typeof(Int16) ||
                    ColumnFilterStorage.DataType == typeof(Byte) ||
                    ColumnFilterStorage.DataType == typeof(Double) ||
                    ColumnFilterStorage.DataType == typeof(Decimal) ||
                    ColumnFilterStorage.DataType == typeof(Single) ||
                    ColumnFilterStorage.DataType == typeof(String))
            {
                return ((TextBox)controls[index]).Text;
            }
            else if (ColumnFilterStorage.DataType == typeof(DateTime))
            {
                return Convert.ToString(((DatePicker)controls[index]).Date);
            }
            else if (ColumnFilterStorage.DataType == typeof(Boolean))
            {
                return ((DropDownList)controls[index]).Text;
            }
            else
                throw new NotSupportedException(String.Format(@"Type ""{0}""not supported", ColumnFilterStorage.DataType));

        }

        public string[] GetTexts()
        {
            EnsureChildControls();
            if (storageChanged)
                UpdateControls();
            if (ColumnFilterStorage.FilterType.IsMultipleArgumentsFilter())
            {
                var values = new List<string>(controls.Length);
                for (int i = 0; i < controls.Length; i++)
                    if (controls[i] != null)
                        values.Add(GetText(i));
                return values.ToArray();
            }
            return new [] { GetText(0) };
        }

        public String ErrorText
        {
            get { return errorText; }
            set { errorText = value; }
        }

        public Boolean Validate()
        {
            var args = new ValidateEventArgs();
            ColumnFilterStorage.Validate(args);
            errorText = args.ErrorText;
            var eventArgs = new ValidateEventArgs();
            OnFilterValidate(eventArgs);
            if (string.IsNullOrEmpty(errorText))
                errorText = eventArgs.ErrorText;
            else
                errorText += "\r\n" + eventArgs.ErrorText;
            return args.Valid && eventArgs.Valid;
        }

        #endregion


        #region IScriptControl Members

        public IEnumerable<ScriptDescriptor> GetScriptDescriptors()
        {
            if(Page != null && Visible)
            {
                var desc = new ScriptControlDescriptor("Nat.Web.Controls.ColumnFilter", ClientID);

                desc.AddProperty("dropDownListID", dropDownList.ClientID);
                desc.AddProperty("cell0ID", filterRow.Cells[2].ClientID);
                if (filterRow.Cells.Count > 3 && filterRow.Cells[3].ID != "fillEmpty")
                    desc.AddProperty("cell1ID", filterRow.Cells[3].ClientID);

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


        #region ISupportSessionWorker Members

        public SessionWorker SessionWorker
        {
            get { return sessionWorker; }
            set
            {
                sessionWorker = value;
                storageChanged = true;
            }
        }

        #endregion


        #region ISupportPostBack Members

        public Boolean PostBack
        {
            get { return postBack; }
            set { postBack = value; }
        }

        public string[] DefaultValues { get; set; }

        public ColumnFilterType DefaultFilterType { get; set; }

        #endregion

        /// <summary>
        /// Возвращает идентификаторы html-элементов, в которые осуществляется ввод данных.
        /// </summary>
        /// <returns></returns>
        public IEnumerable<DefaultFilterValues> GetInputElements()
        {
            List<DefaultFilterValues> list;

            this.EnsureChildControls();
            if (ColumnFilterStorage.DataType == typeof(DateTime) 
                || (ColumnFilterStorage.IsRefBound && ColumnFilterStorage.RefTableRolledIn))
            {
                list = controls
                    .OfType<IClientElementProvider>()
                    .SelectMany(r => r.GetInputElements())
                    .Select((r, index) => new DefaultFilterValues
                        {
                            ClientId = r,
                            Value = DefaultValues == null || DefaultValues.Length <= index 
                                ? null 
                                : DefaultValues[index],
                        })
                    .ToList();
            }
            else
            {
                list = controls.Where(r => r != null)
                    .Select((r, index) => new DefaultFilterValues
                    {
                        ClientId = r.ClientID,
                        Value = DefaultValues == null || DefaultValues.Length <= index
                            ? null
                            : DefaultValues[index],
                    })
                    .ToList();
            }

            if (DefaultFilterType != ColumnFilterType.NotSet)
            {
                list.Add(
                    new DefaultFilterValues
                        {
                            ClientId = dropDownList.ClientID,
                            Value = ((int)DefaultFilterType).ToString(),
                        });
            }
            else
            {
                list.Add(
                    new DefaultFilterValues
                        {
                            ClientId = dropDownList.ClientID,
                            Value = ColumnFilterStorage.GetDefaultFilterTypesArray().Select(r => (int)r).Min().ToString(),
                        });
            }

            return list;
        }
    }

    public interface IDefaultFilterValues
    {
        IEnumerable<DefaultFilterValues> GetInputElements();
    }

    public class DefaultFilterValues
    {
        public string ClientId { get; set; }

        public string Value { get; set; }
    }
}