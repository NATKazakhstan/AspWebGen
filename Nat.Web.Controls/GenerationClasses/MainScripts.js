var isRequstPerforming = false;
var valueID;
var valueOther;
var valueText;
var valueOText;
var valueAddtionalInfo;
var lookupControl;
var infoUrl;
//var sFeatures = 'resizable:yes;dialogHeight=700px;dialogWidth=1000px';
var sFeatures = 'resizable:yes;dialogHeight=' + window.screen.height + 'px;dialogWidth=' + window.screen.width + 'px';
var filterValue;
var filterValueForPostBack;
var hfSelectedValues;
var modalPopup;
var useUPBarOnPostBack = true;
var customValueFilterNames;
var customValueFilterValues;
var applyFilterUrl;
var setSizewidth = 800;
var setSizeheight = 600;
var selectedAddtionalInfoValues;
var selectedTexts;
var selectedParams;
var buttonsForMultipleSelect = new Array();

function fireEventOnChange(control) {
    if (document.createEvent) {// all browsers except IE before version 9
        var onchangeEvent = document.createEvent("TextEvent");
        onchangeEvent.initTextEvent('change', true, false, null, control.value, 9, 'ru-ru');
        control.dispatchEvent(onchangeEvent);
    } else {
        control.fireEvent("onchange");
    }
}

function fireEventOnClick(control) {
    if (document.createEvent) {// all browsers except IE before version 9
        var onclickEvent = document.createEvent("MouseEvents");
        onclickEvent.initMouseEvent("click", true, true, window, 0, 0, 0, 0, 0, false, false, false, false, 0, null);
        control.dispatchEvent(onclickEvent);
    } else {
        control.fireEvent("onclick");
    }
}

var UpdateValue = function () {
    var r = lookupControl;
    if (r.cells[0].children.length > 0) {
        r.cells[0].children[0].value = valueOText;
        fireEventOnChange(r.cells[0].children[0]); //r.cells[0].children[0].fireEvent('onchange');
    }
    r.cells[1].children[0].value = valueText;
    fireEventOnChange(r.cells[1].children[0]); //r.cells[1].children[0].fireEvent('onchange');
    r.cells[2].children[1].value = valueID;
    fireEventOnChange(r.cells[2].children[1]); //r.cells[2].children[1].fireEvent('onchange');
    r.cells[2].children[2].value = valueOther;
    r.cells[4].children[0].href = infoUrl + valueID;
    r.cells[4].children[0].style.visibility = (valueID == null || valueID == '' || valueID.indexOf(',') > -1) ? 'hidden' : '';

    var addtionalInfo = GetLookupAdditionalInfo();
    if (addtionalInfo != null) {
        UpdateAdditionalFields(addtionalInfo);
    }

    if (window.closeModalDialog != null)
        window.closeModalDialog();
};

function UpdateAdditionalFields(addtionalInfo) {
    var i = 0;
    for (var uc in addtionalInfo.UserControls) {
        if (!addtionalInfo.UserControls.hasOwnProperty(uc))
            continue;
        if (valueAddtionalInfo[i] == null)
            continue;

        var controlId = addtionalInfo.UserControls[uc].hfID;
        if (controlId != null && controlId != '') {
            var control = $get(addtionalInfo.UserControls[uc].hfID);
            if (control != null) {
                $(control).val(valueAddtionalInfo[i]);
                fireEventOnChange(control);
            }
        }

        var setValueScript = addtionalInfo.UserControls[uc].SetValueScript;
        if (setValueScript != null && setValueScript != '') {
            setValueScript = String.format(setValueScript, '\'' + valueAddtionalInfo[i].replace('\'','\\\'') + '\'');
            eval(setValueScript);
        }
        
        if (addtionalInfo.UserControls[uc].LabelID != null) {
            var control = $get(addtionalInfo.UserControls[uc].LabelID);
            var $control = $(control);
            if (control != null) {
                var format = $control.attr('format');
                if (format == null || format == "")
                    control.innerText = valueAddtionalInfo[i];
                else
                    control.innerText = String.format(format, valueAddtionalInfo[i]);
            }
        }

        var setLabelScript = addtionalInfo.UserControls[uc].SetLabelScript;
        if (setLabelScript != null && setLabelScript != '') {
            setLabelScript = String.format(setLabelScript, '\'' + valueAddtionalInfo[i].replace('\'', '\\\'') + '\'');
            eval(setLabelScript);
        }
        
        if (addtionalInfo.UserControls[uc].FileLinkID != null) {
            var control = $get(addtionalInfo.UserControls[uc].FileLinkID);
            var $control = $(control);
            if (control != null) {
                var linkBuilder = Sys.Serialization.JavaScriptSerializer.deserialize(valueAddtionalInfo[i]);
                if (linkBuilder.FileName == null)
                    control.innerText = '';
                else
                    control.innerText = linkBuilder.FileName;
                var fileManager = linkBuilder.FileManager == null ? $control.attr('FileManager') : linkBuilder.FileManager;
                var fieldName = linkBuilder.FieldName == null ? $control.attr('FieldName') : linkBuilder.FieldName;
                control.href = String.format("/MainPage.aspx/download?ManagerType={0}&fieldName={1}&ID={2}", fileManager, fieldName, linkBuilder.KeyValue);
                if (linkBuilder.FileName != null) {
                    control.onclick = _onclickForPreviewAftreUpdate;
                    control.attr('fileName', linkBuilder.FileName);
                    control.attr('clickScript', String.format("OpenPreviewPicture('{0}', '{1}');", control.href, linkBuilder.FileName));
                } else
                    control.onclick = null;
            }
        }
        i++;
    }
}

function ChangeExtNetLookupValue(store, newValue, additionalInfoHidden) {
    var values = store.getRecordsValues();
    for (var i = 0; i < values.length; i++) {
        if (values[i].id != newValue)
            continue;

        if (values[i].AdditionalValues != null && values[i].AdditionalValues != '' && additionalInfoHidden != null) {
            valueAddtionalInfo = Sys.Serialization.JavaScriptSerializer.deserialize(values[i].AdditionalValues);
            var hfValue = additionalInfoHidden.getValue();
            if (hfValue != null && hfValue != '') {
                var additionalInfo = Sys.Serialization.JavaScriptSerializer.deserialize(hfValue);
                UpdateAdditionalFields(additionalInfo);
            }
        }

        break;
    }
}

function _onclickForPreviewAftreUpdate() {
    var regex = /.+\.(jpg|jpeg|jpe|jfif|tif|tiff|gif|png|bmp)$/gi;
    if (regex.exec(this.fileName) == null)
        return true;
    OpenPreviewPicture(this.href, this.fileName);
    return false;
}

function ReadValues(url) {
    var r = lookupControl;
    if (r.cells[0].children.length > 0)
        valueOText = r.cells[0].children[0].value;
    valueText = r.cells[1].children[0].value;
    valueID = r.cells[2].children[1].value;
    valueOther = r.cells[2].children[2].value;
    selectedTexts = new Array();
    selectedAddtionalInfoValues = new Array();
    if (url == '')
        infoUrl = String.format('/MainPage.aspx/data/{0}Edit/read?ref{0}=', $(r.cells[1].children[0]).attr('tableCode'));
    else
        infoUrl = url;

    var addtionalInfo = GetLookupAdditionalInfo();
    if (addtionalInfo != null) {
        ReadAdditionalFields(addtionalInfo);
    }
}

function ReadAdditionalFields(addtionalInfo) {
    valueAddtionalInfo = new Array();
    for (var uc in addtionalInfo.UserControls) {
        if (!addtionalInfo.UserControls.hasOwnProperty(uc))
            continue;
        var item = new Object();
        var controlId = addtionalInfo.UserControls[uc].hfID;
        if (controlId != null && controlId != '') {
            var control = $get(controlId);
            if (control != null)
                item.Key = control.value;
        }

        var getValueScript = addtionalInfo.UserControls[uc].GetValueScript;
        if (getValueScript != null && getValueScript != '') {
            item.Key = eval(getValueScript);
        }

        Array.add(valueAddtionalInfo, item.Key);
    }
}

function ReadExtNetAdditionalFields(additionalInfoHidden) {
    var values = additionalInfoHidden.getValue();
    var additionalInfo = Sys.Serialization.JavaScriptSerializer.deserialize(values);
    ReadAdditionalFields(additionalInfo);
}

function NullValues() {
    var r = lookupControl;
    if (r.cells[0].children.length > 0)
        r.cells[0].children[0].value = '';
    r.cells[1].children[0].value = '';
    r.cells[2].children[1].value = '';
    fireEventOnChange(r.cells[2].children[1]);
    r.cells[2].children[2].value = '';
    r.cells[4].children[0].href = infoUrl + '0';
    r.cells[4].children[0].disabled = true;
    r.cells[4].children[0].style.visibility = 'hidden';

    var addtionalInfo = GetLookupAdditionalInfo();
    if (addtionalInfo != null) {
        NullValuesAdditionalFields(addtionalInfo);
    }
}

function NullValuesAdditionalFields(addtionalInfo){
    for (var uc in addtionalInfo.UserControls) {
        if (!addtionalInfo.UserControls.hasOwnProperty(uc))
            continue;
        var controlId = addtionalInfo.UserControls[uc].hfID;
        if (controlId != null && controlId != '') {
            var control = $get(controlId);
            if (control != null)
                control.value = '';
        }

        controlId = addtionalInfo.UserControls[uc].LabelID;
        if (controlId != null && controlId != '') {
            control = $get(controlId);
            if (control != null) control.innerText = '';
        }

        var setValueScript = addtionalInfo.UserControls[uc].SetValueScript;
        if (setValueScript != null && setValueScript != '') {
            setValueScript = String.format(setValueScript, '""');
            eval(setValueScript);
        }

        var setLabelScript = addtionalInfo.UserControls[uc].SetLabelScript;
        if (setLabelScript != null && setLabelScript != '') {
            setLabelScript = String.format(setLabelScript, '""');
            eval(setLabelScript);
        }
    }
}

function NullValuesExtNetAdditionalFields(additionalInfoHidden) {
    var values = additionalInfoHidden.getValue();
    var additionalInfo = Sys.Serialization.JavaScriptSerializer.deserialize(values);
    NullValuesAdditionalFields(additionalInfo);
}

function SetLookup(lookup) {
    lookupControl = lookup.parentNode.parentNode;
}

function GetLookupAdditionalInfo() {
    var lookupC = lookupControl;
    if (lookupC.cells[2].children.length >= 5)
        return Sys.Serialization.JavaScriptSerializer.deserialize(lookupC.cells[2].children[4].innerText);
    return null;
}

function GetLookupSelectParameters() {
    var addInfo = GetLookupAdditionalInfo();
    return GetLookupSelectParametersByValues(addInfo);
}

function GetExtNetLookupSelectParameters(additionalInfoHidden) {
    var values = additionalInfoHidden.getValue();
    var additionalInfo = Sys.Serialization.JavaScriptSerializer.deserialize(values);
    return GetLookupSelectParametersByValues(additionalInfo);
}

function GetLookupSelectParametersByValues(addInfo) {
    if (addInfo == null) return "";
    var params = new Object();
    params.SessionKey = addInfo.SessionKey;
    params.FieldName = addInfo.FieldName;
    params.FieldValues = new Object();
    params.FieldValues = new Object();
    for (var i in addInfo.FieldInfoItems) {
        if (!addInfo.FieldInfoItems.hasOwnProperty(i))
            continue;
        var item = addInfo.FieldInfoItems[i];
        var value;
        if (item.GetValueScript != null && item.GetValueScript != '')
            value = eval(item.GetValueScript);
        else {
            var itemControl = $("#" + item.ControlID);
            if (itemControl.length > 0 && item.TypeComponent == '2') {
                if (itemControl[0].type == "radio" || itemControl[0].type == "checkbox")
                    value = itemControl[0].checked;
                else
                    value = $(itemControl[0]).find("> input[checked]").val();
            }
            else
                value = itemControl.val();
        }
        var fieldValue = new Object();
        fieldValue.Key = item.FieldName;
        fieldValue.Value = value;
        params.FieldValues[item.FieldName] = value;
    }
    
    return "&__selParams=" + encodeURIComponent(Sys.Serialization.JavaScriptSerializer.serialize(params));
}

function GetValues(controlValues, values) {
    var result = '';
    if (controlValues != null && controlValues != '') {

        var items = Sys.Serialization.JavaScriptSerializer.deserialize(controlValues);
        for (var i in items) {
            if (!items.hasOwnProperty(i))
                continue;
            var con = $get(items[i].ID);
            result = result + '&' + items[i].Property + '=';
            if (items[i].TypeComponent == '1')
                result = result + con.value;
            else if (items[i].TypeComponent == '2')
                result = result + con.checked;
        }
    }
    
    if (values != null && values != '') {

        var valueItems = Sys.Serialization.JavaScriptSerializer.deserialize(values);
        for (var i in valueItems) {
            if (!valueItems.hasOwnProperty(i))
                continue;
            result = result + '&' + valueItems[i].Property + '=' + valueItems[i].Value;
        }
    }
    
    return result;
}

function GetLookupFilters(lookup) {
    var values;
    if ($(lookupControl).attr("valuesID") != null)
        values = $get($(lookupControl).attr("valuesID")).innerText;
    else
        values = lookupControl.cells[2].children[3].innerText;

    return GetLookupFiltersByValues(values);
}

function GetExtNetLookupFilters(hidden, record) {
    var values = hidden.getValue();
    return GetLookupFiltersByValues(values, record);
}

// record используется в Ext.Net, для получения значений из строки
function GetLookupFiltersByValues(values, record) {
    if (values == null || values == '') return '';
    var r = Sys.Serialization.JavaScriptSerializer.deserialize(values);
    var result = '';
    if (r == null) return '';
    if (r.First != null && r.First != '')
        r.First = Sys.Serialization.JavaScriptSerializer.deserialize(r.First);
    if (r.Second != null && r.Second != '')
        r.Second = Sys.Serialization.JavaScriptSerializer.deserialize(r.Second);

    if (r.First != null && r.First != '') {

        var items = r.First;
        for (var i in items) {
            if (!items.hasOwnProperty(i))
                continue;
            var value = '';
            if (items[i].GetValueScript != null && items[i].GetValueScript != '')
                value = eval(items[i].GetValueScript);
            else {
                var itemControl = $("#" + items[i].ID);
                if (itemControl.length > 0 && items[i].TypeComponent == '2') {
                    if (itemControl[0].type == "radio" || itemControl[0].type == "checkbox")
                        value = itemControl[0].checked;
                    else
                        value = $(itemControl[0]).find("> input[checked]").val();
                }
                else if (items[i].TypeComponent == '5') {
                    itemControl.find('::checkbox[checked]').each(function () {
                        value = value + $(this).attr('CheckedValue') + ',';
                    });
                }
                else if(itemControl.length > 0 && itemControl[0].tagName == "A")
                    value = itemControl.attr('value');
                else
                    value = itemControl.val();
            }

            if (customValueFilterNames == null || customValueFilterNames.length == 0) {
                if (value != null) {
                    result = result + '&' + items[i].Property + '=' + value;
                }
            } else {
                var found = false;
                for (var iCFN = 0; iCFN < customValueFilterNames.length; iCFN++) {
                    if (customValueFilterNames[iCFN] == items[i].Property) {
                        customValueFilterValues[iCFN] = value;
                        found = true;
                    }
                }

                if (!found)
                    result = result + '&' + items[i].Property + '=' + value;
            }
        }
    }

    if (r.Second != null && r.Second != '') {

        var valueItems = r.Second;
        for (var i in valueItems) {
            if (!valueItems.hasOwnProperty(i))
                continue;
            if (customValueFilterNames == null || customValueFilterNames.length == 0)
                result = result + '&' + valueItems[i].Property + '=' + valueItems[i].Value;
            else {
                var found = false;
                for (var iCFN = 0; iCFN < customValueFilterNames.length; iCFN++) {
                    if (customValueFilterNames[iCFN] == valueItems[i].Property) {
                        customValueFilterValues[iCFN] = valueItems[i].Value;
                        found = true;
                    }
                }

                if (!found)
                    result = result + '&' + valueItems[i].Property + '=' + valueItems[i].Value;
            }
        }
    }

    if (r.Third != null && r.Third != '') {
        result = result + '&__SNColumn=' + r.Third;
    }

    return result;
}

function OpenFilter(hl, filterUrl, url) {

    filterValue = null;
    filterValueForPostBack = false;
    window.UpdateFilterValue = function(value) {
        filterValue = value;

        if (filterValue != null) {
            if (url.indexOf('?') > -1)
                window.location = url + '&__filters=' + filterValue;
            else
                window.location = url + '?__filters=' + filterValue;
            return true;
        }

        return false;
    };

    window.showModalDialog(filterUrl, window, sFeatures);

    return false;
}



function OpenFilterPostBack(hl, filterUrl, url) {

    filterValue = null;
    filterValueForPostBack = true;

    window.UpdateFilterValue = function (value) {
        filterValue = value;

        if (filterValue != null) {
            window.location = hl.href.replace('SetFilters:', 'SetFilters:' + filterValue.replace(/\\"/g, '\\\\"'));
            return true;
        }

        return false;
    };

    window.showModalDialog(filterUrl, window, sFeatures);

    return false;
}

function getChildFilterTable(row) {
    var table = $(row.cells[0].children[0].children[0]).find('> div > table');
    if (table.length > 0)
        return table[0];
    
    table = $(row.cells[0].children[0].children[0]).find('> table');
    if (table.length > 0)
        return table[0];

    table = $(row.cells[0].children[0].children[0]).find('> div > div > table');
    if (table.length > 0)
        return table[0];

    return null;
}

function GetFilters(filterTable, filter) {

    var jFilterTable = $(filterTable);
    for (var i = 0; i < filterTable.rows.length; i++) {
        var row = filterTable.rows[i];
        if (row.cells[0].children.length > 0 && row.cells[0].children[0].tagName == "DIV") {
            GetFilters(getChildFilterTable(row), filter);
            continue;
        }
        
        if (row.cells[1].children[0].value == "Non")
            continue;
        //if (row.cells.length == 3)
        var valuesIsObjects = jFilterTable.attr("valuesIsObjects");
        var ftype = $(row).attr("ftype");
        if (valuesIsObjects === "true") {
            var newObj = new Object();
            newObj.FilterName = $(row.cells[1]).attr("field");
            newObj.FilterType = row.cells[1].children[0].value;
            newObj.IsDisabled = row.cells[1].children[0].disabled;

            if (ftype == "Boolean")
                newObj.Value1 = 'true';
            else if (newObj.FilterType == "ToDay")
                newObj.Value1 = '0';
            else if (ftype == "Reference" && row.cells[1].children[0].value.endsWith("ByRef"))
                newObj.Value2 = row.cells[3].children[0].value;
            else if ($(row.cells[2].children[0]).attr("isLookup") === "true") {
                newObj.Value1 = row.cells[2].children[0].rows[0].cells[2].children[1].value;
                newObj.Value2 = row.cells[2].children[0].rows[0].cells[1].children[0].value;
            }
            else if (newObj.FilterType == "DaysAgoAndMore" || newObj.FilterType == "DaysLeftAndMore") {
                newObj.Value1 = row.cells[4].children[0].value;
            }
            else if (row.cells[2].colSpan === 2)
                newObj.Value1 = row.cells[2].children[0].value;
            else {
                newObj.Value1 = row.cells[2].children[0].value;
                newObj.Value2 = row.cells[3].children[0].value;
            }
            Array.add(filter, newObj);
        }
        else {
            var fieldName = $(row.cells[1]).attr("field");
            if (ftype == "Boolean")
                Array.add(filter, [fieldName, row.cells[1].children[0].value, 'true', '']);
            else if (ftype == "Reference" && row.cells[1].children[0].value.endsWith("ByRef"))
                Array.add(filter, [fieldName, row.cells[1].children[0].value, '', row.cells[3].children[0].value]);
            else if ($(row.cells[2].children[0]).attr("isLookup") === "true")
                Array.add(filter, [fieldName, row.cells[1].children[0].value, row.cells[2].children[0].rows[0].cells[2].children[1].value, row.cells[2].children[0].rows[0].cells[1].children[0].value]);
            else if (row.cells[2].colSpan === 2)
                Array.add(filter, [fieldName, row.cells[1].children[0].value, row.cells[2].children[0].value, '']);
            else
                Array.add(filter, [fieldName, row.cells[1].children[0].value, row.cells[2].children[0].value, row.cells[3].children[0].value]);
        }
    }
}

function SetFilters(filterTable, filter) {

    var jfilterTable = $(filterTable);
    var valuesIsObjects = jfilterTable.attr("valuesIsObjects");
    
    for (var i = 0; i < filterTable.rows.length; i++) {
        var row = filterTable.rows[i];
        var ftype = $(row).attr("ftype");
        var originalRow = row;
        if (row.cells[0].children.length > 0 && row.cells[0].children[0].tagName == "DIV") {
            SetFilters(getChildFilterTable(row), filter);
            continue;
        }
        if (row.cells[1].children[0].disabled)
            continue;
        for (var fI = 0; fI < filter.length; fI++) {
            var fieldName = $(row.cells[1]).attr("field");
            if (valuesIsObjects === "true" && filter[fI].FilterName == fieldName) {
                if (row.cells[1].children[0].value != "Non") {
                    row = AddFilter(originalRow.cells[originalRow.cells.length - 1].children[0], filterTable);
                    i++;
                }
                row.cells[1].children[0].value = filter[fI].FilterType;
                ChangeActiveFieldsOfFilter(row.cells[1].children[0], filterTable);
                if (ftype == "Boolean") {
                }
                else if (ftype == "Reference" && row.cells[1].children[0].value.endsWith("ByRef")) {
                    row.cells[3].children[0].value = filter[fI].Value2;
                }
                else if ($(row.cells[2].children[0]).attr("isLookup") === "true") {
                    row.cells[2].children[0].rows[0].cells[2].children[1].value = filter[fI].Value1;
                    row.cells[2].children[0].rows[0].cells[1].children[0].value = filter[fI].Value2;
                }
                else if (filter[fI].FilterType == "DaysAgoAndMore" || filter[fI].FilterType == "DaysLeftAndMore") {
                    row.cells[4].children[0].value = filter[fI].Value1;
                }
                else if (row.cells[2].colSpan === 2) {
                    row.cells[2].children[0].value = filter[fI].Value1;
                }
                else {
                    row.cells[2].children[0].value = filter[fI].Value1;
                    row.cells[3].children[0].value = filter[fI].Value2;
                }
            }
            else if (filter[fI][0] == fieldName) {
                row.cells[1].children[0].value = filter[fI][1];
                ChangeActiveFieldsOfFilter(row.cells[1].children[0], filterTable);
                if (ftype == "Boolean") {
                }
                else if (ftype == "Reference" && row.cells[1].children[0].value.endsWith("ByRef")) {
                    row.cells[3].children[0].value = filter[fI][3];
                }
                else if ($(row.cells[2].children[0]).attr("isLookup") === "true") {
                    row.cells[2].children[0].rows[0].cells[2].children[1].value = filter[fI][2];
                    row.cells[2].children[0].rows[0].cells[1].children[0].value = filter[fI][3];
                }
                else if (row.cells[2].colSpan === 2) {
                    row.cells[2].children[0].value = filter[fI][2];
                }
                else {
                    row.cells[2].children[0].value = filter[fI][2];
                    row.cells[3].children[0].value = filter[fI][3];
                }
                break;
            }
        }
    }
}

var addFilterIncrementID = 0;

function AddFilter(link, filterTable) {
    var row = link.parentNode.parentNode;
    var table = row.parentNode;
    var newRow = document.createElement("tr");
    var newDdl = null;
    var jrow = $(row);
    var jnewRow = $(newRow);
    var jFilterTable = $(filterTable);

    jnewRow.attr("ftype", jrow.attr("ftype"));
    jnewRow.attr("mainGroup", jrow.attr("mainGroup"));
    for (var i = 0; i < row.cells.length; i++) {
        var newCell = document.createElement("td");
        var cell = row.cells[i];
        var jnewCell = $(newCell);
        var jcell = $(cell);
        newCell.colSpan = cell.colSpan;
        newCell.style.cssText = cell.style.cssText;
        newCell.innerHTML = cell.innerHTML;
        newRow.appendChild(newCell);
        if (i == 0) {
            newCell.style.display = "none";
            cell.rowSpan = cell.rowSpan + 1;
        }
        else if (i == 1) {
            newDdl = newCell.children[0];
            newDdl.value = cell.children[0].value;
            jnewCell.attr("field", jcell.attr("field"));
        }
        else if (i + 1 == row.cells.length) {
            newCell.children[0].children[0].src = jFilterTable.attr("iconSmallRemove");
            newCell.children[0].children[0].alt = $(newCell.children[0].children[0]).attr("altRemove");
            newCell.children[0].title = $(newCell.children[0].children[0]).attr("altRemove");
            newCell.children[0].onclick = _RemoveFilterHandler;
        }
    }
    
    /*var rowInsertBefore = null;
    var jRowCell1 = $(row.cells[1]);
    for (var i = 0; i < table.rows.length; i++) {
        if (rowInsertBefore != null && table.rows[i].cells.length < 2)
            break;
        if ((rowInsertBefore == null && table.rows[i] == row)
            || (rowInsertBefore != null && $(table.rows[i].cells[1]).attr("field") == jRowCell1.attr("field"))) {
            rowInsertBefore = row.nextSibling;
            if (rowInsertBefore != null && (rowInsertBefore.cells.length < 2 || $(rowInsertBefore.cells[1]).attr("field") != jRowCell1.attr("field")))
                break;
        }
    }*/
    /*
    if (rowInsertBefore == null)
        table.appendChild(newRow);
    else
        table.insertBefore(newRow, rowInsertBefore);*/
    $(newRow).insertAfter($(row));
    ChangeActiveFieldsOfFilter(newDdl, filterTable);

    jnewRow.find('*[id]').each(function() {
        this.id += '_copy' + addFilterIncrementID;
    });

    if (jnewRow.attr("ftype") == "Reference") {

        var autoComplites = jnewRow.find('input[id][dataSource]');
        for (var autoCompliteIndex = 0; autoCompliteIndex < autoComplites.length; autoCompliteIndex++) {
            var autoComplite = autoComplites[autoCompliteIndex];
            CreateAutoComplete(1000, "acb_" + autoComplite.id, autoComplite);
        }
    }

    addFilterIncrementID++;
    return newRow;
}

function _RemoveFilterHandler(event) {
    RemoveFilter(this);
}

function RemoveFilter(link) {
    var row = link.parentNode.parentNode;
    var table = row.parentNode;
    var firstRow = row;
    var currentItem = row;
    while (currentItem.previousSibling != null
            && (currentItem.previousSibling.tagName != "TR"
                || $(currentItem.previousSibling.cells[1]).attr("field") == $(row.cells[1]).attr("field"))) {
        {
            currentItem = currentItem.previousSibling;
            if (currentItem.tagName == "TR")
                firstRow = currentItem;
        }
    }
    firstRow.cells[0].rowSpan--;
    table.removeChild(row);
}

function ChangeShowAllFilters(filterTable) {
    var hfShowAllFilters = $get($(filterTable).attr('hfShowAllFilters'));
    if (hfShowAllFilters != null) {
        hfShowAllFilters.value = hfShowAllFilters.value == "true" ? "false" : "true";
        UpdateStyleOfShowAllFilters(filterTable);
        InitializeFilterFields(filterTable);
    }
}

function UpdateStyleOfShowAllFilters(filterTable) {
    var link = $get(filterTable.id + 'LinkShow');
    if (link == null) return;
    var hfShowAllFilters = $get($(filterTable).attr('hfShowAllFilters'));
    if (hfShowAllFilters != null) {
        $filterTable = $(filterTable);
        $link = $(link);
        link.children[0].src = hfShowAllFilters.value == "true" ? $filterTable.attr('iconShowMainGroupFilters') : $filterTable.attr('iconShowAllFilters');
        link.children[0].alt = hfShowAllFilters.value == "true" ? $link.attr('showMainFilters') : $link.attr('showAllFilters');
    }
}

function ChangeActiveFieldsOfFilter(ddl, filterTable) {

    var row = ddl.parentNode.parentNode;
    var $row = $(row);
    var visible = true;
    var ftype = $row.attr("ftype");
    
    if (filterTable != null) {
        var hfShowAllFilters = $get($(filterTable).attr('hfShowAllFilters'));
        if (hfShowAllFilters != null) {
            var value;
            if (ftype == "Boolean")
                value = 'true';
            else if (ftype == "Reference" && row.cells[1].children[0].value.endsWith("ByRef"))
                value = row.cells[3].children[0].value;
            else if ($(row.cells[2].children[0]).attr("isLookup") === "true") {
                value = row.cells[2].children[0].rows[0].cells[2].children[1].value;
            }
            else if (ddl.value == "DaysAgoAndMore" || ddl.value == "DaysLeftAndMore") {
                value = row.cells[4].children[0].value;
            }
            else if (row.cells[2].colSpan === 2)
                value = row.cells[2].children[0].value;
            else {
                value = row.cells[2].children[0].value;
            }

            row.style.display = (hfShowAllFilters.value == "true"
                || $row.attr("mainGroup") === "true"
                || (ddl.value != "Non" && value != null && value != ''))
                    ? ''
                    : 'none';
            visible = row.style.display != 'none';
        }
    }
    
    if (filterTable != null && $(filterTable).attr("valuesIsObjects") === "true") {
        if (ftype == "Boolean" || ddl.value == "Non" || ddl.value == "IsNotNull" || ddl.value == "IsNull" || ddl.value == "ToDay") {
            $row.removeClass('FilterTRTypeOne FilterTRTypeTwo FilterTRTypeSingle2');
            $row.addClass('FilterTRTypeNon');
        }
        else if (ddl.value.endsWith("ByRef")|| ddl.value == "DaysAgoAndMore" || ddl.value == "DaysLeftAndMore"|| ddl.value == "StartsWithCode" || ddl.value == "NotStartsWithCode") {
            $row.removeClass('FilterTRTypeOne FilterTRTypeNon FilterTRTypeTwo');
            $row.addClass('FilterTRTypeSingle2');
        } else if (ddl.value == "Between" || ddl.value == "BetweenColumns") {
            $row.removeClass('FilterTRTypeOne FilterTRTypeNon FilterTRTypeSingle2');
            $row.addClass('FilterTRTypeTwo');
            $row.find('.FilterTDDoubleValue1').attr('colSpan', 1);
        } else {
            $row.removeClass('FilterTRTypeNon FilterTRTypeTwo FilterTRTypeSingle2');
            $row.addClass('FilterTRTypeOne');
            $row.find('.FilterTDDoubleValue1').attr('colSpan', 2);
        }

        if (Sys.Browser.agent === Sys.Browser.InternetExplorer) {
            $row.find('.FilterTDHeader').hide().show();
        }
    }
    else {
        if (ftype != "Boolean") {
            if (ddl.value == "Non" || ddl.value == "IsNotNull" || ddl.value == "IsNull") {
                row.cells[2].style.display = "none";
                row.cells[row.cells.length - 1].style.display = "";
            }
            else {
                row.cells[2].style.display = "";
                row.cells[row.cells.length - 1].style.display = "none";
            }
            if (ftype == "Reference" && ddl.value.endsWith("ByRef")) {
                row.cells[2].style.display = "none";
                row.cells[row.cells.length - 2].style.display = "";
            }
            else if (ftype == "Reference") {
                row.cells[row.cells.length - 2].style.display = "none";
            }
        }
        if (ftype == "Numeric") {
            if (ddl.value != "Between") {
                row.cells[3].style.display = "none";
                row.cells[2].colSpan = 2;
                row.cells[2].style.width = "70%";
            }
            else {
                row.cells[3].style.display = "";
                row.cells[2].colSpan = 1;
                row.cells[2].style.width = "10%";
            }
        }
    }
    
    ChangeClassNameFieldsOfFilter($row, ddl.value);

    return visible;
}

function ChangeClassNameFieldsOfFilter($row, filterType) {

    if ($row.tagName == "SELECT") {
        filterType = $row.value;
        $row = $($row.parentNode.parentNode);
    }

    if (filterType == "Non") {
        $row.addClass("filterNotSelectedRowClass");
        $row.removeClass("filterSelectedRowClass");
    } else {
        $row.addClass("filterSelectedRowClass");
        $row.removeClass("filterNotSelectedRowClass");
    }
}

function InitializeFilterFields(filterTable, fieldSet) {

    var jfilterTable = $(filterTable);
    if (jfilterTable.attr("iconSmallRemove") == null)
        jfilterTable.attr("iconSmallRemove", '/_themes/KVV/SmallRemove.png');
    if (jfilterTable.attr("iconShowMainGroupFilters") == null)
        jfilterTable.attr("iconShowMainGroupFilters",  '/_themes/KVV/ShowMainGroupFilters.png');
    if (jfilterTable.attr("iconShowAllFilters") == null)
        jfilterTable.attr("iconShowAllFilters", '/_themes/KVV/ShowAllFilters.png');

    var hideTable = true;
    for (var i = 0; i < filterTable.rows.length; i++) {
        var row = filterTable.rows[i];
        if (row.cells[0].children.length > 0 && row.cells[0].children[0].tagName == "DIV") {
            $(getChildFilterTable(row)).attr('hfShowAllFilters', $(filterTable).attr('hfShowAllFilters'));
            var item = $(getChildFilterTable(row));
            item.attr("valuesIsObjects", jfilterTable.attr("valuesIsObjects"));
            item.iconSmallRemove = jfilterTable.attr("iconSmallRemove");
            item.iconShowMainGroupFilters = jfilterTable.attr("iconShowMainGroupFilters");
            item.iconShowAllFilters = jfilterTable.attr("iconShowAllFilters");
            if (InitializeFilterFields(getChildFilterTable(row), row.cells[0].children[0].children[0]))
                hideTable = false;
            continue;
        }
        
        var visible = ChangeActiveFieldsOfFilter(row.cells[1].children[0], filterTable);
        if (visible) hideTable = false;
    }

    if (fieldSet != null)
        fieldSet.style.display = hideTable ? 'none' : '';
    
    UpdateStyleOfShowAllFilters(filterTable);
    return !hideTable;
}

function ClearFilter(filterTable) {

    for (var i = 0; i < filterTable.rows.length; i++) {
        var row = filterTable.rows[i];
        if (row.cells[0].children.length > 0 && row.cells[0].children[0].tagName == "DIV") {
            ClearFilter(getChildFilterTable(row));
            continue;
        }
        if (row.cells[1].children[0].disabled)
            continue;
        if (row.cells[0].style.display == "none") {
            RemoveFilter(row.cells[row.cells.length - 1].children[0]);
            i--;
            continue;
        }
        //row.cells[1].children[0].value = "Non";

        var ftype = $(row).attr("ftype");
        var defaultFType = $(row).attr("defaultFType");
        if (defaultFType == '' || defaultFType == null)
            defaultFType = 'Non';

        if (ftype == "Boolean")
            row.cells[1].children[0].value = "Non";
        else if (ftype == "Reference" && row.cells[1].children[0].value.endsWith("ByRef")) {
            row.cells[1].children[0].value = defaultFType;
            row.cells[3].children[0].value = '';
        } else if ($(row.cells[2].children[0]).attr("isLookup") === "true") {
            row.cells[1].children[0].value = defaultFType;
            row.cells[2].children[0].rows[0].cells[2].children[1].value = '';
            row.cells[2].children[0].rows[0].cells[1].children[0].value = '';
        }
        else if (row.cells[2].colSpan === 2) {
            row.cells[1].children[0].value = defaultFType;
            row.cells[2].children[0].value = '';
        } else {
            row.cells[1].children[0].value = defaultFType;
            row.cells[2].children[0].value = '';
            row.cells[3].children[0].value = '';
        }
        
        ChangeActiveFieldsOfFilter(row.cells[1].children[0], filterTable);
    }
}

function SetSerializedFilters(filterTable, filterValues) {
    var filter = filterValues == '' ? new Array() : Sys.Serialization.JavaScriptSerializer.deserialize(filterValues);
    if (filter != null) {
        ClearFilter(filterTable);
        if (filter.length > 0)
            SetFilters(filterTable, filter);
    }
}

function GetSerializedFilters(filterTable) {
    var filter = new Array();
    GetFilters(filterTable, filter);
    if (filter.length == 0)
        return '';
    return encodeURIComponent(Sys.Serialization.JavaScriptSerializer.serialize(filter));
}

function savedUserFilter_change(control) {
    debugger;
    var fControl = savedUserFilter_getControl(control);
    var $fControl = $(fControl);
    var hvSettings = $get($fControl.attr('hvSettings'));
    var tbName = $get($fControl.attr('tbName'));
    var delButton = $get($fControl.attr('delButton'));
    delButton.style.display = control.selectedIndex > 0 ? '' : 'none';
    var $selectedOption = $(control.options[control.selectedIndex]);
    if ($selectedOption.attr('valueColl1') != null &&
        $selectedOption.attr('valueColl1').toLowerCase() == 'true')
        alert('Фильтр приводил к зависанию. Рекомендуется в ссылках использовать условие выбора "равно".');
    tbName.value = control.options[control.selectedIndex].innerText;
    hvSettings.value = $selectedOption.attr('valueColl0');
    SetSerializedFilters($get($fControl.attr('fControl')), hvSettings.value);
}

function savedUserFilter_saveSettings(control) {
    var fControl = savedUserFilter_getControl(control);
    var $fControl = $(fControl);
    var hvSettings = $get($fControl.attr('hvSettings'));
    hvSettings.value = decodeURIComponent(GetSerializedFilters($get($fControl.attr('fControl'))));
    return true;
}

function savedUserFilter_getControl(control) {
    while (control) {
        if ($(control).attr('tableName'))
            return control;
        control = control.parentNode;
    }

    return null;
}

function ApplyFilter(filterTable, url) {

    var dialogArguments = window.dialogArguments;
    var filter = new Array();
    GetFilters(filterTable, filter);
    if (applyFilterUrl == null && dialogArguments.filterValueForPostBack) {
        dialogArguments.UpdateFilterValue(Sys.Serialization.JavaScriptSerializer.serialize(filter));

        if (dialogArguments.closeModalDialog != null)
            dialogArguments.closeModalDialog();
        window.close();
        return;
    }

    var regex = /(\?|&)__filters=(.*?)(&|$)/g;
    var resExec = regex.exec(document.URL);
    var f;
    var curF;
    var fId = filterTable.fTableName != null ? filterTable.fTableName : filterTable.id.substring(6);
    if (resExec == null || resExec[2] == null || resExec[2] == '') f = new Array();
    else f = Sys.Serialization.JavaScriptSerializer.deserialize(decodeURIComponent(resExec[2]));
    for (var i = 0; i < f.length; ) {
        if (f[i].Key == fId) {
            Array.removeAt(f, i);
            continue;
        }
        i++;
    }
    curF = new Object();
    curF.Key = fId;
    Array.add(f, curF);

    if (filter.length == 0) {
        curF.SessionKey = null;
        curF.Value = '';
    }
    else {
        var fControl = $get(filterTable.id + 'UserSavedValues');
        if (fControl != null) {
            var selectedValue = fControl.getElementsByTagName('SELECT')[0].value;
            if (selectedValue != '') {
                if ($(filterTable).attr("valuesIsObjects") !== "true")
                    Array.add(filter, ['__refUserFilterValues', selectedValue]);
                else {
                    var newObj = new Object();
                    newObj.FilterName = '__refUserFilterValues';
                    newObj.FilterType = 'Non';
                    newObj.Value1 = selectedValue;
                    Array.add(filter, newObj);
                }
            }
        }
        curF.Value = Sys.Serialization.JavaScriptSerializer.serialize(filter);
        if (curF.Value.length > 100) {
            ApplyFilterCurF = curF;
            ApplyFilterF = f;
            Nat.Web.Controls.WebServiceFilters.SetFilterValue(curF.Value, SetFilterSucceededCallback, SetFilterFailedCallback);
            return;
        }
    }
    //curF.Value = GetSerializedFilters(filterTable);

    filterValue = encodeURIComponent(Sys.Serialization.JavaScriptSerializer.serialize(f));
    if (applyFilterUrl == null) {
        dialogArguments.UpdateFilterValue(filterValue);

        if (dialogArguments.closeModalDialog != null)
            dialogArguments.closeModalDialog();
        window.close();
    }
    else{
        //var url = window.location.replace(filterTable.id.substring(6) + "Filter/Filter", filterTable.id.substring(6) + "Journal");
        if (applyFilterUrl.indexOf('?') > -1)
            window.location = applyFilterUrl + '&__filters=' + filterValue;
        else
            window.location = applyFilterUrl + '?__filters=' + filterValue;
    }
}

var ApplyFilterCurF;
var ApplyFilterF;

function SetFilterSucceededCallback(result, eventArgs) {

    var dialogArguments = window.dialogArguments;
    ApplyFilterCurF.Value = null;
    ApplyFilterCurF.SessionKey = result;
    filterValue = encodeURIComponent(Sys.Serialization.JavaScriptSerializer.serialize(ApplyFilterF));
    if (applyFilterUrl == null) {
        dialogArguments.UpdateFilterValue(filterValue);
        
        if (dialogArguments.closeModalDialog != null)
            dialogArguments.closeModalDialog();
        window.close();
    }
    else {
        if (applyFilterUrl.indexOf('?') > -1)
            window.location = applyFilterUrl + '&__filters=' + filterValue;
        else
            window.location = applyFilterUrl + '?__filters=' + filterValue;
    }
}

function SetFilterFailedCallback(error) {
    alert(error.get_message());
}

function GetSerializedValues(serelizedKeys) {
    var result = new Array();
    var keys = Sys.Serialization.JavaScriptSerializer.deserialize(serelizedKeys);
    var controls = document.getElementsByTagName("input");
    FillSerializedValues(controls, keys, result);
    controls = document.getElementsByTagName("textarea");
    FillSerializedValues(controls, keys, result);
    controls = document.getElementsByTagName("select");
    FillSerializedValues(controls, keys, result);

    return Sys.Serialization.JavaScriptSerializer.serialize(result);
}

function FillSerializedValues(controls, keys, result) {
    var key;
    for (var i = 0; i < controls.length; i++) {

        if (controls[i].id != null) {
            for (var k = 0; k < keys.length; k++) {

                if (keys[k] === '') continue;
                key = keys[k] + "_";
                if (key == controls[i].id.substring(0, key.length) && !controls[i].disabled) {
                    var d = new Object();
                    d.StringID = controls[i].id.substring(key.length);
                    d.Value = controls[i].value;
                    d.Checked = controls[i].checked;
                    d.Key = keys[k];
                    Array.add(result, d);
                    break;
                }
                else if (keys[k] == controls[i].id && !controls[i].disabled) {
                    var d = new Object();
                    d.Value = controls[i].value;
                    d.Checked = controls[i].checked;
                    d.Key = keys[k];
                    Array.add(result, d);
                    break;
                }
            }
        }
    }
}
function PopulateAdditionalFields(control) {

    $find(control.behavior).populate(control.parameters + control.value);
}
function OnLookupPopulated(sender, eventArgs) {
}
function OnLookupPopulating(sender, eventArgs) {
    var element = sender.get_element();
    var jelement = $(element);
    SetLookup(element);
    var tb = $(lookupControl.cells[1].children[0]);
    var isKz = tb.attr("isKz") == null ? '' : tb.attr("isKz");
    var mode = tb.attr("mode") == null ? 'none' : tb.attr("mode");
    var sfAddType = tb.attr("sfAddType") == null ? '' : tb.attr("sfAddType");
    var isCode = jelement.attr("isCode") == null ? '' : jelement.attr("isCode");
    sender._contextKey = isKz + ',' + isCode + ',' + tb.attr("dataSource") + ',' + encodeURIComponent('&mode=' + mode + '&__SFAddType=' + sfAddType + GetLookupFilters(element) + GetLookupSelectParameters());
}
function OnLookupItemSelected(sender, eventArgs) {
    var element = sender.get_element();
    var jElement = $(element);
    SetLookup(element);
    var r = lookupControl;
    var value = eventArgs.get_value();
    var isCode = jElement.attr("isCode") == null ? '' : jElement.attr("isCode");
    infoUrl = String.format('/MainPage.aspx/data/{0}Edit/read?ref{0}=', $(r.cells[1].children[0]).attr("tableCode"));
    valueID = value.Second; //r.cells[2].children[1].value = value.Second;
    //r.cells[2].children[1].fireEvent('onchange');
    valueText = r.cells[1].children[0].value;
    selectedTexts = new Array();
    selectedTexts[valueID] = valueText;
    if (isCode) {
        valueText = value.First; //r.cells[1].children[0].value = value.First;
        valueOText = r.cells[0].children[0].value;
        //r.cells[1].children[0].fireEvent('onchange');
    }
    else {
        if (r.cells[0].children.length > 0) {
            valueOText = value.First; //r.cells[0].children[0].value = value.First;
            //r.cells[0].children[0].fireEvent('onchange');
        }
    }
    valueAddtionalInfo = value.Third;
    selectedAddtionalInfoValues = new Array();
    selectedAddtionalInfoValues[valueID] = value.Third;
    UpdateValue();
}
function GetSelectedValues() {
    UpdateRefSelectedValues();
    var hf = hfSelectedValues;
    if (hf != null) return hf.value;
    return null;
}
function AddOrRemoveSelectedValue(sender) {

    var dialogArguments = window.dialogArguments;
    UpdateRefSelectedValues();
    var hf = hfSelectedValues;
    if (sender == 'unselectall') {
        $('::checkbox[isMultipleSelect]').each(function () {
            this.checked = false;
            AddOrRemoveSelectedValue(this);
        });

        return;
    }

    var $sender = $(sender);
    var value = $sender.attr('value');
    if (value == "all") {
        var table = sender.parentNode.parentNode.parentNode;
        $(table).find('::checkbox[isMultipleSelect]').each(function () {
            if (this.checked == sender.checked)
                return;

            this.checked = sender.checked;
            AddOrRemoveSelectedValue(this);
        });

        return;
    }


    if (sender.checked) {
        valueText = $sender.attr('valueText');
        valueOText = $sender.attr('valueOText');
        if (hf.value == '') {
            AddSelectedValueInHiddenField(hf, value);
            if ($sender.attr('valueAddtionalInfo') != null)
                valueAddtionalInfo = Sys.Serialization.JavaScriptSerializer.deserialize($sender.attr('valueAddtionalInfo'));
            else
                valueAddtionalInfo = new Array();
        }
        else {
            AddSelectedValueInHiddenField(hf, sender.value);
            //valueText = '';
            //valueOText = '';
            //valueAddtionalInfo = '';
        }
        if ($sender.attr('valueAddtionalInfo') != null) {
            valueAddtionalInfo = Sys.Serialization.JavaScriptSerializer.deserialize($(sender).attr('valueAddtionalInfo'));
            if (dialogArguments != null) {
                if (dialogArguments.selectedAddtionalInfoValues == null)
                    dialogArguments.selectedAddtionalInfoValues = new Array();
                if (dialogArguments.selectedTexts == null)
                    dialogArguments.selectedTexts = new Array();
                dialogArguments.selectedAddtionalInfoValues[value] = valueAddtionalInfo;
                dialogArguments.selectedTexts[value] = valueText;
            }
        }
        if (selectedParams == null) selectedParams = new Array();
        selectedParams[value] = $sender.attr('selectParams');
    }
    else {
        RemoveSelectedValueInHiddenField(hf, sender.value);
        if (hf.value == '') {
            valueText = '';
            valueOText = '';
            valueAddtionalInfo = new Array();
        }
        if (dialogArguments != null && dialogArguments.selectedAddtionalInfoValues != null)
            dialogArguments.selectedAddtionalInfoValues[value] = null;
        if (dialogArguments != null && dialogArguments.selectedTexts != null)
            dialogArguments.selectedTexts[value] = null;
        if (selectedParams != null) selectedParams[value] = null;
    }
    for (var i = 0; i < buttonsForMultipleSelect.length; i++) {
        var control = $get(buttonsForMultipleSelect[i]);
        if (control == null) continue;
        if (control.activeFunctionName)
            eval(control.activeFunctionName + '("' + hf.value + '");');
        else
            control.disabled = hf.value == '' ? 'disabled' : '';
    }
    var tr = sender.parentNode;
    while (tr != null && tr.tagName != 'TR')
        tr = tr.parentNode;
    if (tr != null && tr.notSelectedCSS != null && tr.notSelectedCSS != '')
        tr.className = sender.checked ? 'ms-selected' : tr.notSelectedCSS;
}

function AddSelectedValueInHiddenField(hf, id) {
    if (hf.value == '') {
        hf.value = id;
    } else {
        hf.value = hf.value + ',' + id;
    }
}

function RemoveSelectedValueInHiddenField(hf, id) {
    var regex = new RegExp("," + id + "$|\\b" + id + "\\b,|^" + id + "$", "g");
    hf.value = hf.value.replace(regex, '');
}

function SetSelectToDialogArgumentsAndClose(message) {

    var dialogArguments = window.dialogArguments;
    UpdateRefSelectedValues();
    dialogArguments.valueID = hfSelectedValues.value;
    if (hfSelectedValues.value == null || hfSelectedValues.value == '')
        dialogArguments.valueText = '';
    else if (hfSelectedValues.value.indexOf(',') > -1 || window.valueText == null || window.valueText == '')
        dialogArguments.valueText = message.replace('{0}', hfSelectedValues.value.split(',').length);
    else {
        {
            dialogArguments.valueText = window.valueText;
            dialogArguments.valueOText = window.valueOText;
            dialogArguments.valueAddtionalInfo = window.valueAddtionalInfo;
        }
    }
    
    dialogArguments.UpdateValue();

    if (dialogArguments.closeModalDialog != null)
        dialogArguments.closeModalDialog();
    window.close();
}

function StartupInitLookup() {

    for (var i = 0; i < document.all.length; i++) {
        var item = document.all[i];
        var isLookup = $(item).attr("isLookup");
        if (isLookup === "true") {
            var value = item.rows[0].cells[2].children[1].value;
            item.rows[0].cells[4].children[0].style.visibility = (value == null || value == '' || value.indexOf(',') > -1) ? 'hidden' : '';
        }
    }
}

$(function() { StartupInitLookup() });

function AddToUrlSelectedValues(sender) {
    UpdateRefSelectedValues();
    sender.href = sender.href + (sender.href.indexOf('?') > -1 ? '&' : '?') + '__selected=' + hfSelectedValues.value;
}

var StartRequest = function() {
    if (window.UpdateRefModalPopap != null)
        window.UpdateRefModalPopap();
    var behavior = $find('UpdateProgressBar');
    if (behavior != null)
        behavior.show();
    isRequstPerforming = true;
};

var EndRequest = function() {
    if (window.UpdateRefModalPopap != null)
        window.UpdateRefModalPopap();
    var behavior = $find('UpdateProgressBar');
    if (behavior != null)
        behavior.hide();
    isRequstPerforming = false;
};

function __doPostBackInMainPage() {

    var dialogArguments = window.dialogArguments;
    $(document.body).focus();
    if (dialogArguments == null && useUPBarOnPostBack) StartRequest();
    return true;
}
function _changeHeight(control, controlId, wideText, narrowText) {
    var tbox = $get(controlId);
    if (tbox == null) return false;
    if (tbox.style.height != "400px") {
        tbox.previousHeight = tbox.style.height;
        tbox.style.height = "400px";
        control.innerText = narrowText;
    }
    else {
        tbox.style.height = tbox.previousHeight;
        control.innerText = wideText;
    }
    return false;
}
function _selectCell(currentCell, id) {
    var c = $get('buttons_' + id);
    var isSelect = true;
    if (c != null) {
        isSelect = c.style.display == 'none';
        c.style.display = isSelect ? '' : 'none';
        if (currentCell.parentNode.prevNode != null && currentCell.parentNode.prevNode != c)
            currentCell.parentNode.prevNode.style.display = 'none';
        currentCell.parentNode.prevNode = c;
    }
    var cells = currentCell.parentNode.cells;
    for (var i = 0; i < cells.length; i++) {
        if (cells[i].keyID == id && isSelect)
            cells[i].className = 'gridCellIsSelectedAsRow';
        else if (cells[i].keyID != null)
            cells[i].className = cells[i].parity == '1' ? 'gridCellIsNotSelectedAsRow1' : 'gridCellIsNotSelectedAsRow2';
    }
}
function disableSelectText(e) {return false;}

function ExpandTree(id, parentID, tableName, rowName, pagerText) {
    var table = $get(tableName + "__table");
    ExpandAndCollapceTree(table, id);
    
    var treeBackControl = $get(tableName + "__treeBackControl");
    var $treeBackControl = $(treeBackControl);
    var spanItem = document.createElement("span");
    var brItem = document.createElement("br");
    var spanTextItem = document.createElement("span");
    var aItem = document.createElement("a");

    spanTextItem.innerText = "";
    var countItems = parseInt($treeBackControl.attr('countItems'));
    for (var i = 0; i < countItems; i++)
        spanTextItem.innerHTML += "&nbsp;&nbsp;&nbsp;";
    $treeBackControl.attr('countItems', countItems + 1);

    aItem.href = "javascript:CollapceTree('" + parentID + "', '" + tableName + "')";
    aItem.title = treeBackControl.toolTip;
    aItem.innerText = rowName;

    var pagerContent = ExpandTree_SetPagerParameters(tableName + "__pager", pagerText);
    var pagerContent2 = ExpandTree_SetPagerParameters(tableName + "__pagerTop", pagerText);
    
    /*var pager = $get(tableName + "__pager");
    var pagerContent = pager.cells[0];
    pagerContent.style.display = "none";
    var pagerContentNew = document.createElement("td");
    pagerContentNew.className = "ms-vh2";
    pagerContentNew.innerHTML = pagerText;
    pager.appendChild(pagerContentNew);*/
    
    spanItem.id = "treeSpan" + tableName + parentID;
    spanItem.appendChild(pagerContent);
    if (pagerContent2 != null)
        spanItem.appendChild(pagerContent2);
    spanItem.appendChild(brItem);
    spanItem.appendChild(spanTextItem);
    spanItem.appendChild(aItem);

    treeBackControl.appendChild(spanItem);
}

function ExpandTree_SetPagerParameters(pagerID, pagerText) {
    var pager = $get(pagerID);
    if (pager == null) return null;
    var pagerContent = pager.cells[0];
    pagerContent.style.display = "none";
    var pagerContentNew = document.createElement("td");
    pagerContentNew.className = "ms-vh2";
    pagerContentNew.innerHTML = pagerText;
    pager.appendChild(pagerContentNew);
    return pagerContent;
}

function CollapceTree(id, tableName) {
    var table = $get(tableName + "__table");
    var removeID = "treeSpan" + tableName + id;
    var treeBackControl = $get(tableName + "__treeBackControl");
    if (treeBackControl.children.length == 1)
        ExpandAndCollapceTree(table, "");
    else
        ExpandAndCollapceTree(table, id);
    
    for (var i = 0; i < treeBackControl.children.length; i++) {
        if (treeBackControl.children[i].id == removeID) {
            CollapceTree_SetPagerParameters(tableName + "__pager", treeBackControl.children[i].children[0]);
            CollapceTree_SetPagerParameters(tableName + "__pagerTop", treeBackControl.children[i].children[0]);
            /*var pager = $get(tableName + "__pager");
            pager.removeChild(pager.cells[0]);
            var pagerContent = treeBackControl.children[i].children[0];
            pagerContent.style.display = "";
            pager.appendChild(pagerContent);*/

            while (treeBackControl.children.length > i) {
                treeBackControl.removeChild(treeBackControl.children[i]);
                $(treeBackControl).attr('countItems', parseInt($(treeBackControl).attr('countItems')) - 1);
            }
            break;
        }
    }
}

function CollapceTree_SetPagerParameters(pagerID, pagerContent) {
    var pager = $get(pagerID);
    if (pager == null) return;
    pager.removeChild(pager.cells[0]);
    pagerContent.style.display = "";
    pager.appendChild(pagerContent);
}

function ExpandAndCollapceTree(table, id) {
    $(table).find('tr[level]').hide();
    var rows = $(table).find('tr[level][parentID="' + id + '"]');
    if (rows.length > 0)
        rows.show();
    else {
        $(table).find('tr[level][parentID=""]').show();
    }
    /*
    var hasVisible = false;
    for (var iRow = 0; iRow < table.rows.length; iRow++) {
        var row = table.rows[iRow];
        if ($(row).attr('level')) {
            if ($(row).attr('parentID') == id) {
                row.style.display = "";
                hasVisible = true;
            }
            else
                row.style.display = "none";
        }
    }
    if (id != "" && !hasVisible)
        ExpandAndCollapceTree(table, "");*/
}

function SetValueOfControlIfEmpty(conFromID, conToID) {
    var conFrom = $get(conFromID);
    var conTo = $get(conToID);
    if (conFrom != null && conTo != null
        && (conTo.value == "" || conTo.value == null)) {
        conTo.value = conFrom.value;
        fireEventOnChange(conTo);
        //conTo.fireEvent('onchange');
    }
}

function _onmouseoverDropDownListToSetTitle(obj) {
    if (!obj.initTitles) {
        for (var i = 0; i < obj.options.length; i++) {
            if (obj.options[i].title == null || obj.options[i].title == '')
                obj.options[i].title = obj.options[i].text;
        }
        obj.initTitles = true;
    }
    if (obj.selectedIndex > -1) {
        if (obj.options[obj.selectedIndex].title != null && obj.options[obj.selectedIndex].title != '')
            obj.options.title = obj.options[obj.selectedIndex].title;
        else
            obj.options.title = obj.options[obj.selectedIndex].text;
    }
}

function SetSizeImgEditUrl(previewControlID) {
    var con = $get(previewControlID);
    var $con = $(con);
    var img = $get($con.attr('imgID'));
    var a = $get($con.attr('aID'));    
    var txtwidth = $get($con.attr('txtwidth'));
    var txtheight = $get($con.attr('txtheight'));
    var cbxWithCorner = $get($con.attr('cbxWithCorner'));       
    var url = $(img).attr('originalUrl');
    setSizewidth = txtwidth.value.replace(",", ".") * 72 / 2.54;
    setSizeheight = txtheight.value.replace(",", ".") * 72 / 2.54;
    url = url + String.format('&width={0}&height={1}&WithCorner={2}', txtwidth.value, txtheight.value, cbxWithCorner.checked);
    img.src = url;
    a.href = url;    
}

function SetSizeImgUrl(previewControlID) {
    var con = $get(previewControlID);
    var $con = $(con);
    var img = $get($con.attr('imgID'));
    var a = $get($con.attr('aID'));    
    var cbThumbSizeID = $get($con.attr('cbThumbSizeID'));
    var url = $(img).attr('originalUrl');
    var width;
    var height;

    switch (cbThumbSizeID.selectedIndex) {
        case 1:
            url = url + '&width=3&height=4&WithCorner=false';
            width = 3; height = 4;
            break;
        case 2:
            url = url + '&width=3&height=4&WithCorner=true';
            width = 3; height = 4;
            break;
        case 3:
            url = url + '&width=3.5&height=4.5&WithCorner=false';
            width = 3.5; height = 4.5;
            break;
        case 4:
            url = url + '&width=3.5&height=4.5&WithCorner=true';
            width = 3.5; height = 4.5;
            break;
        case 5:
            url = url + '&width=9&height=12&WithCorner=false';
            width = 9; height = 12;
            break;
    }

    setSizewidth = width * 72 / 2.54;
    setSizeheight = height * 72 / 2.54;
    img.src = url;
    a.href = url;
}

function _OpenPreviewPicture(previewControlID, url, fileName) {
    var con = $get(previewControlID);
    var $con = $(con);
    var b = $find($con.attr('bID'));
    var img = $get($con.attr('imgID'));
    var a = $get($con.attr('aID'));
    a.href = url;
    img.alt = fileName;
    $(img).attr('originalUrl', url);
    con.style.height = "600px";
    img.style.width = "1px";
    img.style.height = "1px";
    img.onload = function() {
        img.style.width = "";
        img.style.height = "";
        if (this.clientWidth > setSizewidth) {
            this.style.height = this.clientHeight * (setSizewidth / this.clientWidth);
            this.style.width = setSizewidth;
        }
        if (this.clientHeight > setSizeheight) {
            this.style.width = this.clientWidth * (setSizeheight / this.clientHeight);
            this.style.height = setSizeheight;
        }
    }
    img.onresize = function() {

    if (this.clientWidth > setSizewidth) {
        this.style.height = this.clientHeight * (setSizewidth / this.clientWidth);
        this.style.width = setSizewidth;
        }
        if (this.clientHeight > setSizeheight) {
            this.style.width = this.clientWidth * (setSizeheight / this.clientHeight);
            this.style.height = setSizeheight;
        }
    }
    img.src = url;
    b.show();
    con.style.height = "";
}

function OpenFilterAsLookup(filterField) {
    lookupControl = filterField;
    var valueControl = $get($(filterField).attr('valueID'));
    filterValue = valueControl.value;
    StartRequest();
    customValueFilterNames = ["FilterTableName", "FilterBaseTableName"];
    customValueFilterValues = new Array();
    var lookupFilters = GetLookupFilters(filterField);
    customValueFilterNames = null;
    filterValueForPostBack = true;

    var tableFilterName = "";
    var tableFilterOpen = "";
    var filterParams = "";
    if (customValueFilterValues[1] == "" || customValueFilterValues[1] == null) {
        tableFilterName = customValueFilterValues[0];
        tableFilterOpen = customValueFilterValues[0];
    }
    else {
        var dotIndex = customValueFilterValues[0].lastIndexOf(".");
        tableFilterName = customValueFilterValues[0].substring(dotIndex + 1, customValueFilterValues[0].length);
        tableFilterOpen = customValueFilterValues[1];
        filterParams = "&__customFCN=" + customValueFilterValues[0] + "Filter";
    }
    /*
    var tableFilters = new Array();
    var filterItem = new Object();
    filterItem.Key = tableFilterName;
    if (filterValue != "")
        filterItem.Value = filterValue;
    else
        filterItem.Value = "";
    Array.add(tableFilters, filterItem);
    tableFilters = encodeURIComponent(Sys.Serialization.JavaScriptSerializer.serialize(tableFilters));
    */
    //window.showModalDialog('/MainPage.aspx/data/' + tableFilterOpen + 'Filter/Filter?__deniedShortFilters=on&__filters=' + tableFilters + filterParams + lookupFilters, window, sFeatures);
    if (filterValue == null || filterValue === '')
        filterValue = "[]";
    window.UpdateFilterValue = function (value) {
        filterValue = value;
        valueControl.value = value;
        var valueSeted = value != null && value != '';
        var filterClearLink = $get($(valueControl).attr('fClearID'));
        if (valueSeted)
            filterClearLink.style.display = '';
        else
            filterClearLink.style.display = 'none';
    }

    window.showModalDialog('/MainPage.aspx/data/' + tableFilterOpen + 'Filter/Filter?__deniedShortFilters=on&__loadFilterFromParam=on' + filterParams + lookupFilters, window, sFeatures);

    EndRequest();
}

function OpenFilterAsLookupClear(filterField) {
    var valueControl = $get($(filterField).attr('valueID'));
    valueControl.value = '';
    filterField.style.display = 'none';
}

function checkArray(values, rightValues, joinResultsAsOr, compareOperator) {
    var arr = new Array();
    var result = null;
    for (var i = 0; i < values.length; i++) {
        for (var ri = 0; ri < rightValues.length; ri++) {
            eval("var subRes = '" + values[i] + "' " + compareOperator + " '" + rightValues[ri] + "'");

            if (result == null && joinResultsAsOr)
                result = false;
            else if (result == null && !joinResultsAsOr)
                result = true;

            if (joinResultsAsOr)
                result = result | subRes;
            else
                result = result & subRes;
        }
    }
    return result;
}

function setValuesToArray(controls, property, value) {
    for (var i = 0; i < controls.length; i++) {
        if (controls[i] != null)
            eval("controls[i]." + property + " = " + value);
    }
}

function disableValidationToArray(controls, value) {

    if (window.Page_Validators == null)
        return;

    for (var i = 0; i < controls.length; i++) {
        if (controls[i] != null) {
            for (var validatorIndex in window.Page_Validators) {

                var validator = window.Page_Validators[validatorIndex];
                if (validator.controltovalidate == controls[i].id) {
                    if (value && validator.disableValidationGroup == null) {
                        validator.disableValidationGroup = validator.validationGroup;
                        validator.validationGroup = "disableValidationGroup";

                        if (validator.display != 'Dynamic')
                            validator.style.visibility = 'hidden';
                        else
                            validator.style.display = 'none';
                    } else if (!value && validator.disableValidationGroup != null) {
                        validator.validationGroup = validator.disableValidationGroup;
                        validator.disableValidationGroup = null;
                    }
                }
            }
        }
    }
}

function disableAllComponentsAndIdRemove(control) {
    var tagName = control.tagName == null ? null : control.tagName.toLowerCase();
    if (tagName == "input" || tagName == "select" || tagName == "textarea") {
        control.disabled = true;
        if (control.id != null && control.id != '')
            control.id = null;
        return;
    }
    for (var i = 0; i < control.childNodes.length; i++)
        disableAllComponentsAndIdRemove(control.childNodes[i]);
}
function ClickInnerGroup(control) {
    //control is Span in Legend in Fieldset
    var div = control.parentNode.parentNode.children[1];
    var collapsed = div.CollapsiblePanelBehavior.get_Collapsed();
    if (collapsed)
        ClickInnerGroupC(div.children, collapsed, 0);
    else
        window.setTimeout('ClickInnerGroupC($get("' + div.id + '").children, ' + collapsed + ', 350)', 350);
}
function ClickInnerGroupC(children, collapsed, timeout) {
    for (var i = 0; i < children.length; i++) {
        if (children[i].tagName == "SPAN" && children[i].clickOnDblClickInParent == "on") {
            // children[i] is Span in Legend in Fieldset
            var div = children[i].parentNode.parentNode.children[1];
            if (div.CollapsiblePanelBehavior != null && div.CollapsiblePanelBehavior.get_Collapsed() != collapsed)
                fireEventOnClick(children[i]); // children[i].fireEvent("onclick");
                //div.CollapsiblePanelBehavior.set_Collapsed(collapsed);
        }
        else if (children[i].id != null && children[i].id != "" && !collapsed)
            window.setTimeout('ClickInnerGroupC($get("' + children[i].id + '").children, ' + collapsed + ', ' + timeout + 300 + ')', timeout + 300);
        else
            ClickInnerGroupC(children[i].children, collapsed, timeout);
    }
}
function CheckInInnerGroup(control, checked, children) {
    if (children == null) {
        var div = control.parentNode;
        checked = control.checked;
        while (div.tagName != "DIV") div = div.parentNode;
        CheckInInnerGroup(control, checked, div.children);
    }
    else {
        for (var i = 0; i < children.length; i++) {
            var child = children[i];
            if (child.tagName == "INPUT" && child.type == "checkbox" && !child.disabled)
                child.checked = checked;
            else
                CheckInInnerGroup(control, checked, child.children);
        }
    }
}

function PostFileFromClipboard(control, listTableId, fileNameId, subsystem, activeXId) {
    var activeX = $get(activeXId);
    var fileNameC = $get(fileNameId);
    var fileName = fileNameC == null || fileNameC.value == null || fileNameC.value == '' ? 'unkown' : fileNameC.value;
    if (activeX == null) {
        control.style.display = 'none';
        return;
    }
    try {
        /*if (!activeX.GetImageFileBase64String) {
            alert('Необходимо установить activeX и добавить сайт в надежные узлы');
            document.URL = "/Files/ActiveX.rar";
            return;
        }*/
        var data = activeX.GetImageFileBase64String();
        if (data != null)
            Nat.Web.Controls.SelectValues.WebServiceUploadFiles.UploadImageFile(data, fileName, subsystem, listTableId, UploadFileFromClipboardSucceededCallback, SetFilterFailedCallback);
        data = activeX.GetTextFileBase64String();
        if (data != null && data != '')
            Nat.Web.Controls.SelectValues.WebServiceUploadFiles.UploadTextFile(data, fileName, subsystem, listTableId, UploadFileFromClipboardSucceededCallback, SetFilterFailedCallback);

        var dataCount = activeX.GetFilesBase64String();
        if (dataCount > 0) {
            activeX.FileNameIndex = 0;
            if (fileName == 'unkown')
                fileName = '';
            if (dataCount != 1)
                fileName = '';
            else if (fileName != '')
                fileName = fileName + activeX.FileName.substr(activeX.FileName.lastIndexOf("."), activeX.FileName.length - activeX.FileName.lastIndexOf("."));
            for (var i = 0; i < dataCount; i++) {
                activeX.FileNameIndex = i;
                if (fileName == '')
                    Nat.Web.Controls.SelectValues.WebServiceUploadFiles.UploadFile(activeX.FileContent, activeX.FileName, subsystem, listTableId, UploadFileFromClipboardSucceededCallback, SetFilterFailedCallback);
                else
                    Nat.Web.Controls.SelectValues.WebServiceUploadFiles.UploadFile(activeX.FileContent, fileName, subsystem, listTableId, UploadFileFromClipboardSucceededCallback, SetFilterFailedCallback);
            }
        }
    }
    catch (err) {
        alert(err.description);
    }
    return;
}

function PostFileFromDialog(control, listTableId, fileNameId, subsystem, activeXId) {
    var activeX = $get(activeXId);
    var fileNameC = $get(fileNameId);
    var fileName = fileNameC == null || fileNameC.value == null || fileNameC.value == '' ? '' : fileNameC.value;
    if (activeX == null) {
        control.style.display = 'none';
        return;
    }
    try {
        var dataCount = activeX.OpenFilesBase64String();
        if (dataCount > 0) {
            activeX.FileNameIndex = 0;
            if (dataCount != 1)
                fileName = '';
            else if (fileName != '')
                fileName = fileName + activeX.FileName.substr(activeX.FileName.lastIndexOf("."), activeX.FileName.length - activeX.FileName.lastIndexOf("."));
            for (var i = 0; i < dataCount; i++) {
                activeX.FileNameIndex = i;
                if (fileName == '')
                    Nat.Web.Controls.SelectValues.WebServiceUploadFiles.UploadFile(activeX.FileContent, activeX.FileName, subsystem, listTableId, UploadFileFromClipboardSucceededCallback, SetFilterFailedCallback);
                else
                    Nat.Web.Controls.SelectValues.WebServiceUploadFiles.UploadFile(activeX.FileContent, fileName, subsystem, listTableId, UploadFileFromClipboardSucceededCallback, SetFilterFailedCallback);
            }
        }
    }
    catch (err) {
        alert(err.description);
    }
    return;
}

function UploadFileFromClipboardSucceededCallback(result, eventArgs) {
    result = Sys.Serialization.JavaScriptSerializer.deserialize(result);
    var table = $get(result.TableID);
    var itemsHF = $get(result.TableID + '_Items');
    var items = Sys.Serialization.JavaScriptSerializer.deserialize(itemsHF.value);
    Array.add(items, result.Item);
    itemsHF.value = Sys.Serialization.JavaScriptSerializer.serialize(items);
    var newRow = document.createElement("tr");
    var newCell = document.createElement("td");
    newCell.innerHTML = result.Html;
    newCell.style.width = "100%";
    newRow.appendChild(newCell);
    table.tBodies[0].appendChild(newRow);
}

function RemoveListItem(control, tableID, selectedKey, newValueId) {
    var table = $get(tableID);
    var itemsHF = $get(tableID + '_Items');
    var items = Sys.Serialization.JavaScriptSerializer.deserialize(itemsHF.value);
    for (var i = 0; i < items.length; i++) {
        if (selectedKey != '' && selectedKey == items[i].SelectedKey)
            items[i].Selected = false;
        else if (newValueId != '' && newValueId == items[i].Value) {
            Array.removeAt(items, i);
            Nat.Web.Controls.SelectValues.WebServiceUploadFiles.DeleteUploadedFile(newValueId);
        }
    }
    itemsHF.value = Sys.Serialization.JavaScriptSerializer.serialize(items);
    while (control.tagName != 'TR')
        control = control.parentNode;
    table.deleteRow(control.rowIndex);
}

function MultipleLookupSelect_ValueChanged(sender, tableID) {
    if (sender.value == '')
        return;

    var table = $get(tableID);
    var itemsHF = $get(tableID + '_Items');
    var items = Sys.Serialization.JavaScriptSerializer.deserialize(itemsHF.value);

    for (var key in selectedTexts) {
        if (!selectedTexts.hasOwnProperty(key))
            continue;
        if (selectedTexts[key] == null) continue;
        var nextKey = false;
        for (var i = 0; i < items.length; i++) {
            if (items[i].Value === key) {
                nextKey = true;
                if (!items[i].Selected) {
                    items[i].Selected = true;
                    MultipleLookupSelect_ValueChangedAddItem(table, tableID, items[i]);
                }

                break;
            }
        }

        if (nextKey) continue;
        var item = new Object();
        item.Value = key;
        item.Text = selectedTexts[key];
        item.ToolTip = '';
        item.Selected = true;
        item.Enabled = true;
        item.SelectedKey = null;
        Array.add(items, item);
        MultipleLookupSelect_ValueChangedAddItem(table, tableID, item);
    }

    itemsHF.value = Sys.Serialization.JavaScriptSerializer.serialize(items);
    sender.value = '';
    UpdateMemeberItemsIds(tableID);

    fireEventOnChange(sender);
    var textControl = sender.parentNode.parentNode.cells[1].children[0];
    textControl.value = '';
    fireEventOnChange(textControl);
}

function MultipleLookupSelect_ValueChangedAddItem(table, tableID, item) {
    var newRow = document.createElement("tr");
    var $newRow = $(newRow);
    $newRow.html("<td><a href='javascript:void(0);'><img border=0/></a> <span/></td>");
    $newRow.find("a").click(function() {
        MultipleLookupSelect_DeleteItem(this, tableID, item.SelectedKey, item.Value);
        return false;
    });
    $newRow.find("td").attr('title', item.ToolTip);
    $newRow.find("img").attr('src', $(table).attr('deleteIconUrl'));
    $newRow.find("span").text(item.Text);
    table.tBodies[0].appendChild(newRow);
}

function UpdateMemeberItemsIds(tableID) {
    var membersItemsHF = $get(tableID + '_MembersItems');
    if (membersItemsHF == null)
        return;

    var itemsHF = $get(tableID + '_Items');
    var items = Sys.Serialization.JavaScriptSerializer.deserialize(itemsHF.value);

    var membersItems = "";
    for (var i = 0; i < items.length; i++) {
        if (items[i].Selected)
            membersItems = membersItems + items[i].Value + ",";
    }

    membersItemsHF.value = membersItems;
}

function MultipleLookupSelect_DeleteItem(sender, tableID, selectedKey, newValueId) {
    var table = $get(tableID);
    var itemsHF = $get(tableID + '_Items');
    var items = Sys.Serialization.JavaScriptSerializer.deserialize(itemsHF.value);

    for (var i = 0; i < items.length; i++) {
        if (selectedKey != '' && selectedKey == items[i].SelectedKey)
            items[i].Selected = false;
        else if (newValueId != '' && newValueId == items[i].Value) {
            Array.removeAt(items, i);
        }
    }

    itemsHF.value = Sys.Serialization.JavaScriptSerializer.serialize(items);
    UpdateMemeberItemsIds(tableID);

    while (sender.tagName != 'TR')
        sender = sender.parentNode;
    table.deleteRow(sender.rowIndex);
}

function GetUrlBySelected(url, replaceValue, messageNotSelected) {
    var values = GetSelectedValues();
    if (values == '' || values == null) {
        alert(messageNotSelected);
        return '';
    }
    return url.replace(replaceValue, values);
}
function DownloadBySelected(url, replaceValue, messageNotSelected) {
    url = GetUrlBySelected(url, replaceValue, messageNotSelected);
    if (url == null || url == '') return false;
    var selectedValues = GetSelectedValues().replace(/,/g, '_');
    window.open(url, 'Download' + selectedValues, '');
    return true;
}

function DownloadBySelectedForCultures(url, replaceValue, messageNotSelected) {
    url = GetUrlBySelected(url, replaceValue, messageNotSelected);
    if (url == null || url == '') return false;
    var urlRu = url.replace("&culture=kz&", "&culture=ru&");
    var urlKz = url.replace("&culture=ru&", "&culture=kz&");
    var needKz = false;
    var needRu = false;
    for (var i in selectedParams){
        if (!selectedParams.hasOwnProperty(i))
            continue;
        if (selectedParams[i] == "kz")
            needKz = true;
        else if (selectedParams[i] == "ru")
            needRu = true;
    }
    var selectedValues = GetSelectedValues().replace(/,/g, '_');
    if (needRu)
        window.open(urlRu, 'DownloadRu' + selectedValues, '');
    if (needKz)
        window.open(urlKz, 'DownloadKz' + selectedValues, '');
    return true;
}

function CreateAutoComplete(completionInterval, id, autoCompliteControl) {
    $create(AjaxControlToolkit.AutoCompleteBehavior, { "completionInterval": completionInterval, "completionListCssClass": "autocomplete_completionListElement", "completionListItemCssClass": "autocomplete_listItem", "delimiterCharacters": "", "enableCaching": false, "firstRowSelected": true, "highlightedItemCssClass": "autocomplete_highlightedListItem", "id": id, "minimumPrefixLength": 1, "serviceMethod": "GetCompletionList", "servicePath": "/WebServiceAutoComplete.asmx", "useContextKey": true }, { "itemSelected": OnLookupItemSelected, "populated": OnLookupPopulated, "populating": OnLookupPopulating }, null, autoCompliteControl);
}

// in PrepareCommand we can modify command
var prepareRowCommandExtNet = function (grid, toolbar, rowIndex, record, editIndex, deleteIndex) {
    if (editIndex > -1 && !record.data.CanEdit) {
        toolbar.items.getAt(editIndex).hide();
    }
    if (deleteIndex > -1 && !record.data.CanDelete) {
        toolbar.items.getAt(deleteIndex).hide();
    }
};


var InitializeUniqueChecked = function() {
    $(':checkbox[uniqueGroup1],:checkbox[uniqueGroup2]').click(UniqueCheckedOnClick);
    $(':checkbox[uniqueGroup1],:checkbox[uniqueGroup2]').hover(function () {
        var $this = $(this);
        var group1 = $this.attr('uniqueGroup1');
        var group2 = $this.attr('uniqueGroup2');
        var isUnique = true;
        var checkedItem;

        if (group1 != null && group1 != '') {
            checkedItem = GetUniqueCheckedGroup(this, window.UniqueCheckedGroup1[group1]);
            if (checkedItem != null) {
                $(checkedItem).stop(true, true).animate({ "background-color": 'green' }, 200).delay(800).animate({ "background-color": $(checkedItem).parent().css('background-color') }, 200);
                isUnique = false;
            }
        }

        if (group2 != null && group2 != '') {
            checkedItem = GetUniqueCheckedGroup(this, window.UniqueCheckedGroup2[group2]);

            if (checkedItem != null) {
                $(checkedItem).stop(true, true).animate({ "background-color": 'green' }, 200).delay(800).animate({ "background-color": $(checkedItem).parent().css('background-color') }, 200);
                isUnique = false;
            }
        }

        $(this).stop(true, true).animate({ "background-color": isUnique ? 'green' : 'red' }, 200);
    }, function() {
        $(this).stop(true, true).animate({ "background-color": $(this).parent().css('background-color') }, 200);
    });

    window.UniqueCheckedGroup1 = {};
    $(':checkbox[uniqueGroup1]').each(function() {
        var $this = $(this);
        var group = $this.attr('uniqueGroup1');
        if (window.UniqueCheckedGroup1[group] == null)
            window.UniqueCheckedGroup1[group] = [];
        Array.add(window.UniqueCheckedGroup1[group], this);
    });
    window.UniqueCheckedGroup2 = {};
    $(':checkbox[uniqueGroup2]').each(function() {
        var $this = $(this);
        var group = $this.attr('uniqueGroup2');
        if (window.UniqueCheckedGroup2[group] == null)
            window.UniqueCheckedGroup2[group] = [];
        Array.add(window.UniqueCheckedGroup2[group], this);
    });
};

var UniqueCheckedOnClickDisabledDoubled;

var UniqueCheckedOnClick = function (e) {
    var me = this;
    if (me == UniqueCheckedOnClickDisabledDoubled)
        return false;

    UniqueCheckedOnClickDisabledDoubled = me;
    window.setTimeout(function () {
        if (me == UniqueCheckedOnClickDisabledDoubled)
            UniqueCheckedOnClickDisabledDoubled = null;
    }, 600);

    var $this = $(this);
    var group1 = $this.attr('uniqueGroup1');
    var group2 = $this.attr('uniqueGroup2');

    if (window.UniqueCheckedChangeCustom != null)
        window.UniqueCheckedChangeCustom(this);

    if (group1 != null && group1 != '')
        UniqueCheckedGroupChange(this, window.UniqueCheckedGroup1[group1]);

    if (group2 != null && group2 != '')
        UniqueCheckedGroupChange(this, window.UniqueCheckedGroup2[group2]);

    return true;
};

var UniqueCheckedGroupChange = function(item, uniqueCheckedGroup) {
    if (uniqueCheckedGroup == null)
        return;

    for (var i = 0; i < uniqueCheckedGroup.length; i++) {
        if (uniqueCheckedGroup[i].id != item.id && uniqueCheckedGroup[i].checked) {
            uniqueCheckedGroup[i].checked = false;

            if (window.UniqueCheckedChangeCustom != null)
                window.UniqueCheckedChangeCustom(uniqueCheckedGroup[i]);
        }
    }
};

var InUniqueCheckedGroup = function (item, uniqueCheckedGroup) {
    if (uniqueCheckedGroup == null)
        return true;

    for (var i = 0; i < uniqueCheckedGroup.length; i++) {
        if (uniqueCheckedGroup[i].id != item.id && uniqueCheckedGroup[i].checked)
            return false;
    }

    return true;
};

var GetUniqueCheckedGroup = function (item, uniqueCheckedGroup) {
    if (uniqueCheckedGroup == null)
        return null;

    for (var i = 0; i < uniqueCheckedGroup.length; i++) {
        if (uniqueCheckedGroup[i].id != item.id && uniqueCheckedGroup[i].checked)
            return uniqueCheckedGroup[i];
    }

    return null;
};
var LookupBoxSearchClick = function (el, trigger, tag, auto, lookupAddFields, lookupFilters, dialogwindow, windowTitle, url, initValueInField, nullValue, nullText, addNewFunction, frameInitialization) {
    var store = el.getStore();
    switch (tag) {
        case "add":
            addNewFunction(el, dialogwindow, store);
            break;
        case "open":
            dialogwindow.editor = el;
            dialogwindow.autoComplete = auto;
            valueID = el.getValue();
            if (lookupAddFields != null)
                ReadExtNetAdditionalFields(lookupAddFields);
            var newUrl = url;
            if (lookupFilters != null)
                newUrl += GetExtNetLookupFilters(lookupFilters, window.CurrentRecord);
            if (lookupAddFields != null)
                newUrl += GetExtNetLookupSelectParameters(lookupAddFields);

            var urlChanged = false;
            if (dialogwindow.loader.url != newUrl) {
                dialogwindow.loader.url = newUrl;
                urlChanged = true;
                dialogwindow.afterShow = function () { };
            }

            dialogwindow.show();
            dialogwindow.focus();

            if (urlChanged) {
                if (windowTitle != '' && windowTitle != null)
                    dialogwindow.setTitle(windowTitle);
                
                dialogwindow.loader.load();
                var frame = dialogwindow.getFrame();

                if (initValueInField != null)
                    eval('frame.' + initValueInField + ' = valueID == null ? null : valueID');

                if (valueID == null)
                    frame.selectedRecords = [];
                else {
                    var rowFound = store.getById(valueID);
                    frame.selectedRecords = rowFound == null ? [] : [rowFound];
                }
                
                if (valueID != null) frame.tryLoadPage = true;
                if (frameInitialization != null) frameInitialization(frame, valueID, store, el, true);

                frame.addSelectedValues = function (record) {
                    dialogwindow.hide();
                    
                    if (record.length != null && record.length > 0) {
                        for (var i = 0; i < record.length; i++) {
                            var item = record[i].data;
                            var newValue = item.id;
                            store.add({ id: item.id, RowName: item.RecordName });
                            el.setValue(newValue);
                            if (lookupAddFields != null)
                                ChangeExtNetLookupValue(store, newValue, lookupAddFields);
                        }
                    } else {
                        var newValue = record.id;
                        store.add(record);
                        el.setValue(newValue);
                        if (lookupAddFields != null)
                            ChangeExtNetLookupValue(store, newValue, lookupAddFields);
                    }
                };

                frame.closeModalWindow = function () {
                    dialogwindow.hide();
                };

                frame.removeSelectedValues = function(record) {
                    el.removeByValue(record.id);
                    el.clearValue();
                    if (lookupAddFields != null)
                        NullValuesExtNetAdditionalFields(lookupAddFields);
                };
                
                window.modalframe = frame;
            } else {

                var frame = dialogwindow.getFrame();
                if (valueID == null)
                    frame.selectedRecords = [];
                else {
                    var rowFound = store.getById(valueID);
                    frame.selectedRecords = rowFound == null ? [] : [rowFound];
                }
                if (valueID != null)
                    frame.tryLoadPage = true;
                if (frame.selectRecords != null)
                    frame.selectRecords(frame.selectedRecords);
                if (frameInitialization != null) frameInitialization(frame, valueID, store, el, false);
            }

            break;
        case "remove":
            if (nullValue == null) {
                el.removeByValue(el.getValue());
                el.clearValue();
            } else {
                if (store.getById(nullValue) == null) {
                    store.add({ id: nullValue, RowName: nullText });
                }
                el.setValue(nullValue);
            }
            
            if (lookupAddFields != null)
                NullValuesExtNetAdditionalFields(lookupAddFields);
            break;
    }
}

if (window.Ext != null) {
    Ext.onReady(function() {
        Ext.picker.Date.prototype.dayNames = Ext.Date.dayNames;
    });

    Ext.form.field.Date.override({
        initComponent: function() {
            if (!Ext.isDefined(this.initialConfig.startDay)) {
                this.startDay = Ext.picker.Date.prototype.startDay;
            }

            this.callParent();
        }
    });

    Ext.view.Table.override({
        refreshSize: function () {
            var me = this,
                grid = me.up('tablepanel');

            if (grid.editingPlugin && grid.editingPlugin.editing) {
                Ext.defer(this.refreshSize, 300, me);
                return;
            }

            me.callParent();
        }
    });

    if (Ext.ux.grid && Ext.ux.grid.filter && Ext.ux.grid.filter.NumericFilter) {
        Ext.ux.grid.filter.NumericFilter.override({
            createMenu: function(config) {
                var me = this,
                    menuCfg = config.menuItems ? { items: config.menuItems } : {},
                    menu;

                if (Ext.isDefined(config.emptyText)) {
                    menuCfg.menuItemCfgs = {
                        emptyText: config.emptyText,
                        selectOnFocus: false,
                        width: 155
                    };
                }

                //added
                if (config.fieldCfg) {
                    menuCfg.fieldCfg = config.fieldCfg;
                }
                //end of added

                menu = Ext.create('Ext.ux.grid.menu.RangeMenu', menuCfg);
                menu.on('update', me.fireUpdate, me);
                return menu;
            }
        });
    }
}

var GridStoreLoadHandler = function(grid, store, selectIDHidden) {
    if (window.frameElement != null && window.frameElement.selectedRecords != null) {
        if (window.frameElement.selectRecords(window.frameElement.selectedRecords))
            window.frameElement.selectedRecords = null;
        return;
    }

    var value = selectIDHidden.getValue();
    if (value == null || value == '')
        return;
    var selModel = grid.selModel;
    var r = store.getById(parseInt(value));
    if (!Ext.isEmpty(r)) {
        selModel.select([r]);
        selectIDHidden.setValue('');
        if (window.frameElement != null)
            window.frameElement.tryLoadPage = false;
    } else {
        if (window.tryLoadPage || (window.frameElement != null && window.frameElement.tryLoadPage)) {
            window.tryLoadPage = false;
            if (window.frameElement != null)
                window.frameElement.tryLoadPage = false;
            SelectPageByID(parseInt(value), store);
        }

        selModel.select([]);
    }
};

var setFrameElementSelectedRecords = function(tablePanel) {
    if (window.frameElement == null) return true;

    window.frameElement.selectRecords = function(records) {
        var store = tablePanel.getStore();
        var selected = [];
        for (var i = 0; i < records.length; i++) {
            if (records[i] == null) continue;

            var r = store.getById(records[i].data.id);
            if (!Ext.isEmpty(r)) {
                Array.add(selected, r);
            }
            else if (records.length == 1 && window.frameElement.tryLoadPage) {
                window.frameElement.tryLoadPage = false;
                SelectPageByID(records[i].data.id, store);
                return false;
            }
        }

        window.frameElement.tryLoadPage = false;
        tablePanel.selModel.select(selected);
        return true;
    }
}

var setFrameElementSelectedRecordsForUserGrid = function (userGrid) {
    if (window.frameElement == null)
        return;

    window.frameElement.selectRecords = function(records) {
        var store = userGrid.getStore();
        store.clearData();
        userGrid.getView().refresh();
        for (var i = 0; i < records.length; i++) {
            userGrid.store.add({ id: records[i].data.id, RecordName: records[i].data.RowName });
        }

        return true;
    }
}

var SelectPageByID = function (id, store, afterLoadFunction) {
    if (App.direct == null)
        return;

    var getPageNumber;
    if (App.direct.PlaceHolderMain_item != null && App.direct.PlaceHolderMain_item.GetPageNumber != null)
        getPageNumber = App.direct.PlaceHolderMain_item.GetPageNumber;
    else if (App.direct.item != null && App.direct.item.GetPageNumber != null)
        getPageNumber = App.direct.item.GetPageNumber;
    else if (App.direct.GetPageNumber != null)
        getPageNumber = App.direct.GetPageNumber;

    if (getPageNumber == null) return;

    var sortArr = store.sort();

    getPageNumber(id, sortArr, store.pageSize, {
        success: function(value) {
            if (value == null || value == 0)
                return;

            if (afterLoadFunction != null)
                store.on("load", afterLoadFunction, null, { single: true });
            store.loadPage(value);
        }
    });
}

var needTreeReload = [];
var fixTreeStoreRecords = function(records) {
    if (records == null)
        return;

    for (var i = 0; i < records.length; i++) {
        var tempStore = records[i].stores[0];
        if (tempStore.treeStore != null) {
            records[i].stores[0] = records[i].stores[1];
            records[i].stores[1] = tempStore;
            records[i].store = records[i].stores[0];
        }
    }

    if (needTreeReload.length > 0) {
        for (var j = 0; j < needTreeReload.length; j++) {
            needTreeReload[j]();
        }

        needTreeReload = [];
    }
};

// респонз файла в урле
var saveFile = function (url) {
    var a = document.createElement('a');
    a.href = url;
    var fileName = url.substring(url.lastIndexOf('/') + 1);
    a.download = fileName;
    a.style.display = 'none';
    document.body.appendChild(a);
    a.click();
    document.body.removeChild(a);
};


var stringWordWrap = function(str, width, spaceReplacer) {
    if (spaceReplacer == null)
        spaceReplacer = '\n';
    var left;
    var right;
    if (str.length > width) {
        var p = width;
        for (;
            p > 0 &&
                str[p] !== ' ' &&
                str[p] !== '-' &&
                str[p] !== ',' &&
                str[p] !== '.' &&
                str[p] !== '@' &&
                str[p] !== '+' &&
                str[p] !== '*' &&
                str[p] !== '?' &&
                str[p] !== '!' &&
                str[p] !== '\'' &&
                str[p] !== '"';
            p--) {
        }

        if (p > 0) {
            left = str.substring(0, p);
            right = str.substring(p + 1);
            return left + spaceReplacer + stringWordWrap(right, width, spaceReplacer);
        }

        for (;
            p < str.length &&
                str[p] !== ' ' &&
                str[p] !== '-' &&
                str[p] !== ',' &&
                str[p] !== '.' &&
                str[p] !== '@' &&
                str[p] !== '+' &&
                str[p] !== '*' &&
                str[p] !== '?' &&
                str[p] !== '!' &&
                str[p] !== '\'' &&
                str[p] !== '»' &&
                str[p] !== '"';
            p++) {
        }

        if (p < str.length)
        {
            if (str[p] !== ' ')
                left = str.substring(0, p + 1);
            else
                left = str.substring(0, p);
            right = str.substring(p + 1);
            return left + spaceReplacer + stringWordWrap(right, width, spaceReplacer);
        }
    }

    return str;
};

var windowsDialogCache = [];
var windowsDialogCacheById = [];
var windowsDialogCacheNextId = 0;

$(function () {
    /*
    if (window.showModalDialog != null)
        return;
    //*/

    window.showModalDialog = function (url, parentWindow, feature, header, resultFunction, reloadPage) {
        if (reloadPage == null)
            reloadPage = true;

        if (window.frameElement && window.frameElement.showModalDialog) {
            window.frameElement.showModalDialog(url, parentWindow, feature, header, resultFunction, reloadPage);
            return;
        }

        var newWindow;
        newWindow = windowsDialogCache[url];
        var width = $(parentWindow).width() * 5 / 6;
        var height = $(parentWindow).height() * 5 / 6;
        if (parentWindow.frameElement)
        {
            width = $(window).width() * 5 / 6;
            height = $(window).height() * 5 / 6;
        }

        if (newWindow == null) {

            var html =
                "<div id='windowsDialog{0}' style='display: none; overflow: hidden;'><div class='LoadingControl'><div class='LoadingControl-info'><div><span class='text-center' role='text'>" +
                    (IsCultureKz() ? 'Енгізу' : 'Загрузка') +
                    "...</span><div role='img'><img src='/_themes/KVV/ewr133.gif'></div></div></div><div class='LoadingControl-hide'></div></div><iframe name='windowsDialog{0}Frame' width='100%' style='height: inherit' src='' frameborder='0' scrolling='none' /></div>";
            html = html.replace("{0}", windowsDialogCacheNextId).replace("{0}", windowsDialogCacheNextId);
            var $dialog = $(html);
            parentWindow.document.body.appendChild($dialog[0]);
            if (window.CoreLocalization != null)
                $dialog.find('.text-center').text(window.CoreLocalization.Loading);

            $dialog.dialog({
                autoOpen: false,
                width: width,
                height: height,
                modal: true,
                close: function(event, ui) {
                    parentWindow.document.body.style.overflow = '';
                }
            });
            newWindow = {
                id: 'windowsDialog' + windowsDialogCacheNextId,
                dialog: $dialog,
                loadingControl: $dialog.find('.LoadingControl'),
                iframe: $dialog.find('iframe')[0],
                parentWindow: parentWindow,
                isLoaded: false
            };

            var dialogButtons = IsCultureKz()
                ? {
                    'Болдырмау': function() {
                        newWindow.dialog.dialog('close');
                    }
                }
                : {
                    'Отмена': function() {
                        newWindow.dialog.dialog('close');
                    }
                };
            $dialog.dialog('option', 'buttons', dialogButtons);

            if (Sys.Browser.agent === Sys.Browser.InternetExplorer) {
                $(newWindow.iframe).css('height', '100%');
            }

            newWindow.loadingControl.addClass('LoadingControlShow');
            // todo: подумать на счет явного указания события и присвоения parentWindow
            $(newWindow.iframe).load(function () {
                newWindow.isLoaded = true;
                newWindow.loadingControl.removeClass('LoadingControlShow');
                newWindow.iframe.contentWindow.dialogArguments = parentWindow;
                if (newWindow.dialog.dialog("option", "title") == '') {
                    var doc = newWindow.iframe.contentDocument;
                    if (doc == null && newWindow.iframe.contentWindow != null)
                        doc = newWindow.iframe.contentWindow.document;
                    if (doc != null)
                        newWindow.dialog.dialog("option", "title", doc.title);
                }

                if (newWindow.iframe.contentWindow.InitialLoadFilterFromParam != null)
                    newWindow.iframe.contentWindow.InitialLoadFilterFromParam();
            });
            newWindow.iframe.src = url;
            newWindow.iframe.showModalDialog = showModalDialog;

            windowsDialogCache[url] = newWindow;
            windowsDialogCacheById[newWindow.id] = newWindow;
            windowsDialogCacheNextId++;
        } else {

            if (newWindow.isLoaded)
                newWindow.iframe.contentWindow.dialogArguments = parentWindow;

            if (reloadPage) {
                newWindow.loadingControl.addClass('LoadingControlShow');
                var doc = newWindow.iframe.contentDocument;
                if (doc == null && newWindow.iframe.contentWindow != null)
                    doc = newWindow.iframe.contentWindow.document;
                if (doc != null)
                    doc.body.innerHTML = '';
                newWindow.isLoaded = false;
                newWindow.iframe.src = url;
            }
        }

        parentWindow.document.body.style.overflow = 'hidden';

        newWindow.iframe.ModalWindowResultFunction = function (result) {
            resultFunction(result);
            newWindow.dialog.dialog('close');
        };

        parentWindow.closeModalDialog = function () {
            newWindow.dialog.dialog('close');
        };

        newWindow.dialog.dialog({ autoOpen: false, width: width, height: height });
        if (header != null)
            newWindow.dialog.dialog("option", "title", header);
        newWindow.dialog.dialog('open');
    };

    window.showModelessDialog = function (url, someStr, feature, header, resultFunction) {
        window.showModalDialog(url, window, feature, header, resultFunction, false);
    };
});

$(function() {
    $('div[onkeypress]').css('outline', 'none').each(function() {
        this.tabindex = 1;
        if (window.dialogArguments != null) {
            setTimeout(function() { this.focus(); }, 100);
        }
    });

    if (window.initIDDivTopDetecter != null)
        initIDDivTopDetecter();
});

function CodeDropDownListExtSynchronizeText(control) {
    $(control).parent().find('select[role="text"]').val(control.value);
}

function CodeDropDownListExtSynchronizeCode(control) {
    $(control).parent().find('select[role="code"]').val(control.value);
}

function DateTimeCompareValidator(source, args) {
    var compareTo = $get($(source).attr('compareTo'));
    if (args.Value == null || args.Value == '')
        args.Value = ValidatorGetValueCustom(source);

    if (compareTo == null || compareTo.value == null || compareTo.value == ''
        || args.Value == null || args.Value == '') {
        args.IsValid = false;
        return;
    }

    var compareToValue = Date.parseLocale(compareTo.value, 'dd.MM.yyyy HH:mm');
    var controlValue = Date.parseLocale(args.Value, 'dd.MM.yyyy HH:mm');
    switch ($(source).attr('operator')) {
    case 'Equal':
        args.IsValid = controlValue == compareToValue;
        break;
    case 'GreaterThan':
        args.IsValid = controlValue > compareToValue;
        break;
    case 'GreaterThanEqual':
        args.IsValid = controlValue >= compareToValue;
        break;
    case 'LessThan':
        args.IsValid = controlValue < compareToValue;
        break;
    case 'LessThanEqual':
        args.IsValid = controlValue <= compareToValue;
        break;
    case 'NotEqual':
        args.IsValid = controlValue != compareToValue;
        break;
    default:
        break;
    }
}

function ValidatorGetValueCustom(val) {
    if (typeof ($(val).attr('controltovalidate')) == "string") {
        return ValidatorGetValue($(val).attr('controltovalidate'));
    }

    return '';
}

function ShowHiddenEditor() {
    var c = $(this).hasClass('nat-hideEditorDIV') ? $(this) : $(this).closest('.nat-hideEditorDIV');
    setTimeout(function() {
            c.find("> div").show();
            c.find("> a").hide();
            c.find("> div input:visible").focus();
        },
        1);
    return false;
}

function HideHiddenEditor() {
    var c = $(this).closest('.nat-hideEditorDIV');
    var value = $(this).val();

    setTimeout(function() {
            if (value === '' || value == null)
                c.find("> a").text('');
            else
                c.find("> a").text(value);
            c.find("> div").hide();
            c.find("> a").show();
        },
        5);
    return false;
}

$(function() {
    $('.nat-hideEditorDIV').click(ShowHiddenEditor);
    $('.nat-hideEditorDIV > a').focus(ShowHiddenEditor);
    $('.nat-hideEditorDIV > div input').blur(HideHiddenEditor);
});

function IsCultureKz() {
    return document.location.search.indexOf('culture=ru-ru') === -1 &&
        (document.cookie.indexOf('lcid=1087') !== -1 || document.location.search.indexOf('culture=kk-kz') !== -1);
};

function GetFiltersParameter(tableName, filterValues) {
    var filtersObj = [{
        Key: tableName,
        Value: filterValues
    }];
    var filters = Sys.Serialization.JavaScriptSerializer.serialize(filtersObj);
    return '__filters=' + encodeURIComponent(filters);
}