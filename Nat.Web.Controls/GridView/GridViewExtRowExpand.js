
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
