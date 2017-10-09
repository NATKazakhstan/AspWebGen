Type.registerNamespace("Nat.Web.Controls");

function mainCheckedField(ctrl)
{
    var checked = ctrl.checked;
    var column = $(ctrl).attr("column");
    var elements = ctrl.parentNode.parentNode.parentNode.parentNode.getElementsByTagName('input');
    for (var i = 1; i < elements.length; i++)
    {
        var chb = elements[i];
        var display = chb.parentNode.parentNode.style.display;
        if ($(chb).attr("column") == column && display != "none")
            chb.checked = checked;
    }
}

function checkFieldForItems(ctrl) {
    var checked = ctrl.checked;
    var allSame = true;
    var column = $(ctrl).attr("column");
    var elements = ctrl.parentNode.parentNode.parentNode.parentNode.getElementsByTagName('input');
    for (var i = 1; i < elements.length; i++) {
        var chb = elements[i];
        var display = chb.parentNode.parentNode.style.display;
        if ($(chb).attr("column") == column && chb.checked != checked && display != "none") {
            allSame = false;
            break;
        }
    }

    if (allSame && $(elements[0]).attr("column") == column) {
        elements[0].checked = checked;
    }
}

function checkedField(checkBox)
{
    var checked = checkBox.checked;
    var column = checkBox.column;
    var parentRow = checkBox.parentNode.parentNode;
    var tbl = parentRow.parentNode;
    var parentItems = parentRow.getElementsByTagName('input');
	var parentExpand = parentItems.item(1);
	var parentItem = parentItems.item(2);
	
	var items = parentRow.getElementsByTagName('input');
	var expand = items.item(1);
	var visible = items.item(3);
	
	//Expand
	if (checked == true && expand.value == '')
	{
        gverChange(parentRow.children[0]);
    }
    //Collapse    
	if (checked == false && expand.value == '1')
	{
        gverChange(parentRow.children[0]);
    }   
    
    //Проходим по строкам
    for (var i = 0; i < tbl.rows.length; i++)
    {
        var row = tbl.rows[i];
        var items = row.getElementsByTagName('input');
        var parent = items.item(0);
        var item = items.item(2);
        var visible = items.item(3);
        
        if (parent.value == parentItem.value)
        {
            //Проходим по колонкам
            for (var num = 0; num < row.children.length; num++)
            {
                cell = row.children[num];
                //Проходим по вложенным элементам ячейки
                if (cell.children.length > 0 && cell.children[0].column == column)
                {
                    chb = cell.children[0];
                    chb.checked = checked;
                    checkedField(chb);
                }
            }
        }
    }
}

function gverCollapse(tbl, index, value, parentValue)
{
    for (var i = index; i < tbl.rows.length; i++)
    {
        var row = tbl.rows[i];
        var items = row.getElementsByTagName('input');
        var parent = items.item(0);
        var item = items.item(2);
        var visible = items.item(3);
        
        if (parent.value == parentValue)
        {
            row.style.display = 'none';
            visible.value = '';
            value = item.value;
            continue;
        }
        if (parent.value == value)
            i = gverCollapse(tbl, i, value, value);
        else return i - 1;
    }    
}

function gverExpand(tbl, index, value, parentValue, collapsed)
{
    for (var i = index; i < tbl.rows.length; i++)
    {
        var row = tbl.rows[i];
        var items = row.getElementsByTagName('input');
        var parent = items.item(0);
        var expand = items.item(1);
        var item = items.item(2);
        var visible = items.item(3);
        var collapsedItem;
        
        if (parent.value == parentValue)
        {
            if (!collapsed)
            {
                row.style.display = '';
                visible.value = '1';
            }
            value = item.value;
            collapsedItem = expand.value == '';
            continue;
        }
        if (parent.value == value)
        {
            i = gverExpand(tbl, i, value, value, collapsed || collapsedItem)
        }
        else return i - 1;
    }    
}

function gverChange(ctl)
{
	var row = ctl.parentNode;
	var tbl = row.parentNode;
	var crow = tbl.rows[row.rowIndex];	
    var items = row.getElementsByTagName('input');
	var expand = items.item(1);
	var item = items.item(2);
	var text = ctl.getElementsByTagName('a').item(0);
    var grid = tbl.parentNode;

	var expandClass = grid.attributes.getNamedItem('expandClass').value;
	var collapseClass = grid.attributes.getNamedItem('collapseClass').value;
	var expandText = grid.attributes.getNamedItem('expandText').value;
	var collapseText = grid.attributes.getNamedItem('collapseText').value;
    var levelTab = text.attributes.getNamedItem("levelTab");
    if (levelTab != null) levelTab = levelTab.value;
    else levelTab = "";
    
    if('#' == text.innerHTML.substring(0,1)) return false;
    if(expand.value == '')
    {
        expand.value = '1';
	    text.innerHTML = collapseText + levelTab;
		ctl.className = collapseClass;
        gverExpand(tbl, row.rowIndex + 1, item.value, item.value, false);
        if (ctl.notNeedPostBack) return false;
        return true;
    }
    else
    {
        expand.value = '';
	    text.innerHTML = expandText + levelTab;
		ctl.className = expandClass;
        gverCollapse(tbl, row.rowIndex + 1, item.value, item.value);
        ctl.notNeedPostBack = true;
        return false;
    }
    
    return false;
}