//***** работа с фиксированием заголовка
var originalCellCache = [];

function CreateFixedHeader(table, fixedHeader, rowsCount, colsCount) {
    var $table = $(table);
    $table.attr('rowsCount', rowsCount);
    $table.attr('colsCount', colsCount);
    $table.attr('fixedHeader', fixedHeader);
    $table.attr('crossJournalID', table.id);
    for (var i = 0; i < table.parentNode.children.length; ) {
        if ($(table.parentNode.children[i]).attr('isFixedHeader') == 'true')
            table.parentNode.removeChild(table.parentNode.children[i]);
        else if ($(table.parentNode.children[i]).attr('isFixedColumns') == 'true')
            table.parentNode.removeChild(table.parentNode.children[i]);
        else
            i++;
    }

    // todo: подумать, возможно условие нужно только для IE
    if (!$('#ctl00_PlaceHolderMain_item_Journal').is(":visible") && window.Ext == null)
        return;

    var rowsTable = document.createElement("table");
    var colsTable = document.createElement("table");
    var crosTable = document.createElement("table");
    if (fixedHeader) {
        $(rowsTable).attr('isFixedHeader', 'true');
        rowsTable.id = table.id + "_rows";
        initTableAttributes(rowsTable, table, "border-bottom-color: black; ");
        table.parentNode.appendChild(rowsTable);
        initContentOfNewTable(rowsTable, table, rowsCount, 0, false, true);

        if (rowsTable.rows.length > 0 && rowsTable.rows[0].cells.length > 0) {
            var lastCell = rowsTable.rows[0].cells[rowsTable.rows[0].cells.length - 1];
            lastCell.style.borderBottomColor = "white";
            lastCell.style.borderTopColor = "white";
            lastCell.style.borderRightColor = "white";
            lastCell.style.borderBottomStyle = "solid";
            lastCell.style.borderTopStyle = "solid";
            lastCell.style.borderRightStyle = "solid";
        }
    }
    if (colsCount > 0) {
        $(colsTable).attr('isFixedColumns', 'true');
        colsTable.id = table.id + "_cols";
        initTableAttributes(colsTable, table, "border-right-color: black; ");
        table.parentNode.appendChild(colsTable);
        initContentOfNewTable(colsTable, table, table.rows.length, colsCount, false, false);

        if (fixedHeader) {
            var $crosTable = $(crosTable);
            $crosTable.attr('isFixedHeader', 'true');
            $crosTable.attr('isFixedColumns', 'true');
            crosTable.id = table.id + "_cros";
            initTableAttributes(crosTable, table, "border-right-color: black; border-bottom-color: black; ");
            table.parentNode.appendChild(crosTable);
            initContentOfNewTable(crosTable, table, rowsCount, colsCount, true, true);
        }
    }
    $table.attr('rowsTableId', rowsTable.id);
    $table.attr('colsTableId', colsTable.id);
    $table.attr('crosTableId', crosTable.id);
    if (table.zIndex == null)
        table.zIndex = 1;
    rowsTable.zIndex = table.zIndex + 2;
    colsTable.zIndex = table.zIndex + 1;
    crosTable.zIndex = table.zIndex + 3;

    if ($table.attr('addedScrollHandler') != 'true') {
        $addHandler(table.parentNode, "scroll", this._onscrollFixedHeader);
        if (table.rows.length > 0 && table.rows[0].cells.length > 0 && $.browser.msie) {
            $addHandler(table.rows[0].cells[table.rows[0].cells.length - 1], "resize", this._onresizeFixedHeader);
        }

        $table.attr('addedScrollHandler', 'true');
    }
}

function initContentOfNewTable(newTable, table, rowsCount, colsCount, stopRight, stopDown) {
    var nHead = newTable.createTHead();
    var fullWidth = 0;
    var fullHeight = 0;
    var rowSpans = new Array();
    var notSetWidthOfCell = new Array();

    for (var r = 0; r < table.tHead.rows.length; r++) {
        var rowSpansN = new Array();
        var row = table.tHead.rows[r];
        /*if (stopDown && row.clientHeight + row.offsetTop + 50 > table.parentNode.clientHeight)
            break;*/
        fullHeight += row.clientHeight;
        //newTable.style.height = fullHeight + "px";
        var newRow = nHead.appendChild(document.createElement("tr"));
        if (row.className != "" && row.className != null)
            newRow.className = row.className;  //+ "-fixed";
        newRow.style.cssText = row.style.cssText;
        newRow.style.height = row.clientHeight + "px";
        var indexCol = 0; //остановить обход по ячейкам если дошли до ограничевающей колнки
        for (var key in rowSpans) {
            if (!rowSpans.hasOwnProperty(key))
                continue;
            rowSpans[key] -= 1;
        }
        for (var c = 0; c < row.cells.length; c++) {
            var cell = row.cells[c];
            if (stopRight && (cell.clientWidth + cell.offsetLeft + 50 > table.parentNode.clientWidth))
                break;
            while (rowSpans[indexCol] > 0)
                indexCol++;
            for (var iSpan = 0; iSpan < cell.colSpan; iSpan++)
                rowSpansN[indexCol + iSpan] = cell.rowSpan;
            indexCol += cell.colSpan;
                
            if (colsCount > 0 && indexCol > colsCount) {
                if (cell.colSpan - (indexCol - colsCount) <= 0)
                    break;

                var newCell = document.createElement("th");
                initTHAttributes(newCell, cell, "hidden", cell.colSpan - (indexCol - colsCount));
                newRow.appendChild(newCell);
                /*var rowSpan = cell.rowSpan;
                while (stopDown && newCell.clientHeight + newCell.offsetTop + 50 > table.parentNode.clientHeight && rowSpan > 1) {
                    newCell.rowSpan = --rowSpan;
                    var lastRow = table.tHead.rows[r + newCell.rowSpan];
                    newCell.style.height = (newCell.clientHeight - lastRow.clientHeight) + "px";
                }*/
                var obj = new Object();
                obj.cell = newCell;
                obj.indexCol = indexCol - cell.colSpan;
                obj.colSpan = newCell.colSpan;
                Array.add(notSetWidthOfCell, obj);
                break;
            }
            var newCell = document.createElement("th");
            initTHAttributes(newCell, cell, "", cell.colSpan);
            newRow.appendChild(newCell);
            /*var rowSpan = cell.rowSpan;
            while (stopDown && newCell.clientHeight + newCell.offsetTop + 50 > table.parentNode.clientHeight && rowSpan > 1) {
                newCell.rowSpan = --rowSpan;
                var lastRow = table.tHead.rows[r + newCell.rowSpan];
                newCell.style.height = (newCell.clientHeight - lastRow.clientHeight) + "px";
            }*/
            if (c == 0 && r == 0)
                fullWidth += cell.clientWidth;
            if (colsCount > 0 && indexCol == colsCount)
                break;
        }
        for (var key in rowSpansN) {
            if (!rowSpansN.hasOwnProperty(key))
                continue;
            rowSpans[key] = rowSpansN[key];
        }
        //row.style.visibility = "hidden";
    }
    if (rowsCount > 0 && table.tBodies.length > 0) {
        var tBody = table.tBodies[0];
        var ntBody = null;
        if (newTable.tBodies.length > 0)
            ntBody = newTable.tBodies[0];
        else
            ntBody = newTable.appendChild(document.createElement("tbody"));
        rowSpans = new Array();
        for (var i = 0; i < rowsCount && i < tBody.rows.length; i++) {
            var rowSpansN = new Array();
            var row = tBody.rows[i];
            if (stopDown && row.clientHeight + row.offsetTop + 50 > table.parentNode.clientHeight)
                break;
            fullHeight += row.clientHeight;
            //newTable.style.height = fullHeight + "px";
            var newRow = ntBody.appendChild(document.createElement("tr"));
            if (row.className != "" && row.className != null)
                newRow.className = row.className;  //+ "-fixed";
            newRow.style.cssText = row.style.cssText;
            newRow.style.height = row.clientHeight + "px";
            var indexCol = 0; //остановить обход по ячейкам если дошли до ограничевающей колнки
            for (var key in rowSpans) {
                if (!rowSpans.hasOwnProperty(key))
                    continue;
                rowSpans[key] -= 1;
            }
            for (var c = 0; c < row.cells.length; c++) {
                var cell = row.cells[c];
                if (stopRight && (cell.clientWidth + cell.offsetLeft + 50 > table.parentNode.clientWidth))
                    break;

                while (rowSpans[indexCol] > 0)
                    indexCol++;
                for (var iSpan = 0; iSpan < cell.colSpan; iSpan++)
                    rowSpansN[indexCol + iSpan] = cell.rowSpan;
                indexCol += cell.colSpan;
                if (colsCount > 0 && indexCol > colsCount) {
                    if (cell.colSpan - (indexCol - colsCount) <= 0)
                        break;

                    var newCell = document.createElement("td");
                    initTDAttributes(newCell, cell, "hidden", cell.colSpan - (indexCol - colsCount));
                    newRow.appendChild(newCell);
                    /*var rowSpan = cell.rowSpan;
                    while (stopDown && newCell.clientHeight + newCell.offsetTop + 50 > table.parentNode.clientHeight && rowSpan > 1) {
                        newCell.rowSpan = --rowSpan;
                        var lastRow = tBody.rows[i + rowSpan];
                        newCell.style.height = (newCell.clientHeight - lastRow.clientHeight) + "px";
                    }*/
                    var obj = new Object();
                    obj.cell = newCell;
                    obj.indexCol = indexCol - cell.colSpan;
                    obj.colSpan = newCell.colSpan;
                    Array.add(notSetWidthOfCell, obj);
                    break;
                }
                var newCell = document.createElement("td");
                initTDAttributes(newCell, cell, "", cell.colSpan);
                newRow.appendChild(newCell);
                /*var rowSpan = cell.rowSpan;
                while (stopDown && newCell.clientHeight + newCell.offsetTop + 50 > table.parentNode.clientHeight && rowSpan > 1) {
                    newCell.rowSpan = --rowSpan;
                    var lastRow = tBody.rows[i + rowSpan];
                    newCell.style.height = (newCell.clientHeight - lastRow.clientHeight) + "px";
                }*/
                if (colsCount > 0 && indexCol == colsCount)
                    break;
            }
            for (var key in rowSpansN) {
                if (!rowSpansN.hasOwnProperty(key))
                    continue;
                rowSpans[key] = rowSpansN[key];
            }
            //row.style.visibility = "hidden";
        }
    }
    for (var iSetWidth = notSetWidthOfCell.length - 1; iSetWidth >= 0; iSetWidth--) {
        var objCell = notSetWidthOfCell[iSetWidth];
        rowSpans = new Array();
        for (var r = 0; r < newTable.rows.length; r++) {
            var rowSpansN = new Array();
            var row = newTable.rows[r];
            var indexCol = 0; //остановить обход по ячейкам если дошли до ограничевающей колнки
            for (var key in rowSpans) {
                if (!rowSpans.hasOwnProperty(key))
                    continue;
                rowSpans[key] -= 1;
            }
            for (var c = 0; c < row.cells.length; c++) {
                var cell = row.cells[c];
                while (rowSpans[indexCol] > 0)
                    indexCol++;
                for (var iSpan = 0; iSpan < cell.colSpan; iSpan++)
                    rowSpansN[indexCol + iSpan] = cell.rowSpan;
                if (indexCol == objCell.indexCol && cell.colSpan <= objCell.cell.colSpan && cell.style.width != "0px") {
                    objCell.cell.style.width = objCell.cell.style.pixelWidth + cell.style.pixelWidth + "px";
                    objCell.cell.children[0].style.width = objCell.cell.style.width;
                    objCell.indexCol += cell.colSpan;
                    objCell.colSpan -= cell.colSpan;
                    if (objCell.colSpan <= 0)
                        break;
                }
                indexCol += cell.colSpan;
                if (colsCount > 0 && indexCol >= colsCount)
                    break;
            }
            if (objCell.colSpan <= 0)
                break;
            for (var key in rowSpansN) {
                if (!rowSpansN.hasOwnProperty(key))
                    continue;
                rowSpans[key] = rowSpansN[key];
            }
        }
    }
    newTable.style.width = fullWidth + "px";
//    newTable.style.height = fullHeight + "px";
}

function initTableAttributes(newTable, table, style) {
    if (table.className != "" && table.className != null)
        newTable.className = table.className;  //+ "-fixed";
    var $newTable = $(newTable);
    var $table = $(table);
    $newTable.attr('originalTableID', $table.attr('crossJournalID'));
    $newTable.attr('hfColsID', $table.attr('hfColsID'));
    $newTable.attr('crossJournalID', $table.attr('crossJournalID'));
    newTable.style.cssText = table.style.cssText + "; background-color: white; " + style;
    newTable.border = table.border;
    newTable.cellSpacing = table.cellSpacing;
    //colsTable.style.left = table.parentNode.offsetLeft + "px";
    newTable.style.position = "absolute";
    newTable.onclick = table.onclick;
    newTable.ondblclick = table.ondblclick;

    if ($newTable.attr('isFixedHeader') == 'true')
        newTable.style.top = table.parentNode.scrollTop + "px";
    else
        newTable.style.top = "0px";

    if ($newTable.attr('isFixedColumns') == 'true')
        newTable.style.left = table.parentNode.scrollLeft + "px";
    else
        newTable.style.left = "0px";
}

function initTHAttributes(newCell, cell, overflow, colSpan) {
    //cell.style.padding = "0px";
    if (cell.className != "" && cell.className != null)
        newCell.className = cell.className;  //+ "-fixed";
    if (cell.id != null && cell.id != '')
        $(newCell).attr('originalID', cell.id);
    else
        setOriginalCell(cell, newCell);

    newCell.rowSpan = cell.rowSpan;
    newCell.colSpan = colSpan;
    newCell.onmousemove = _changeWidthOnCellMouseMove;
    newCell.onmousedown = _changeWidthCellMouseDown;
    var content = newCell.appendChild(document.createElement("div"));
    initStyleAttributes(content, cell, newCell);
    newCell.title = cell.innerText;
    cell.title = cell.innerText;
    if (overflow != "") {
        //content.style.cssText = content.style.cssText;  //+ "; white-space: nowrap; ";
        //content.style.overflow = overflow;
        //устанавливается 0 для того что бы после определить ширину ячейки
        content.style.width = "0px";
        newCell.style.width = "0px";
    }
    if ($(cell).attr('_tracking') == 'true') {
        $(newCell).attr('_tracking', 'true');
        newCell.onmouseout = changeWidthCellMouseOut;
        _controlFixedChangeWidth = newCell;
    }
    
    content.innerHTML = cell.innerHTML;
    var firstChild = content.firstChild;
    if (firstChild != null /* && firstChild.style.height == ""*/) {
//        firstChild.style.height = "100%";
    }
    //cell.style.padding = "";
}

function setOriginalCell(cell, newCell) {

    var cellIndex = $(cell).attr('originalCellIndex');
    if (!cellIndex) {
        cellIndex = originalCellCache.length;
        originalCellCache[cellIndex] = cell;
        $(cell).attr('originalCellIndex', cellIndex);
    }

    $(newCell).attr('originalCell', cellIndex);
}

function getOriginalCell(cell) {
    var cellIndex = $(cell).attr('originalCell');
    if (cellIndex)
        return originalCellCache[parseInt(cellIndex)];

    return null;
}

function initStyleAttributes(content, cell, newCell) {
    content.style.cssText = cell.style.cssText + "; padding-right: 0px; magin-right: 0px; ";
    var newWidth = cell.offsetWidth - 11;
    var newHeight = cell.offsetHeight - 11;
    if (newWidth <= 0) newWidth = 1;
    if (newHeight <= 0) newHeight = 1;
    content.style.width = newWidth + "px";
    content.style.height = newHeight + "px";
    content.style.margin = "0px";
    content.style.overflow = "hidden";
    //content.style.padding = "5px";
    //newCell.style.padding = "0px";
    newCell.style.width = newWidth + "px";
    newCell.style.height = newHeight + "px";
//    newCell.style.backgroundColor = cell.style.backgroundColor;
    newCell.style.color = cell.style.color;
}

function initTDAttributes(newCell, cell, overflow, colSpan) {
    //cell.style.padding = "0px";
    if (cell.className != "" && cell.className != null)
        newCell.className = cell.className;  //+ "-fixed";
    newCell.rowSpan = cell.rowSpan;
    newCell.colSpan = colSpan;
    if (cell.id != null && cell.id != '')
        $(newCell).attr('originalID', cell.id);
    else
        setOriginalCell(cell, newCell);
    var content = newCell.appendChild(document.createElement("div"));
    initStyleAttributes(content, cell, newCell);
    newCell.title = cell.innerText;
    cell.title = cell.innerText;
    if (overflow != "") {
        //устанавливается 0 для того что бы после определить ширину ячейки
        content.style.width = "0px";
        newCell.style.width = "0px";
    }
    content.innerHTML = cell.innerHTML;
    disableAllComponentsAndIdRemove(content);
    //cell.style.padding = "";
}

function _onresizeFixedHeader(e, oSelf) {
    var con = e ? e.target : oSelf;
    while (con.tagName.toLowerCase() != "table")
        con = con.parentNode;
    var $table= $(con);
    var lastWidth = $table.attr('lastWidth');
    var lastCellWidth = $table.attr('lastCellWidth');
    var lastCellHeight = $table.attr('lastCellHeight');
    var $lastCell = $(con.rows[0].cells[con.rows[0].cells.length - 1]);
    if (lastWidth != $table.width() || lastCellWidth != $lastCell.width() || lastCellHeight != $lastCell.height()) {
        $table.attr('lastWidth', $table.width());
        $table.attr('lastCellWidth', $lastCell.width());
        $table.attr('lastCellHeight', $lastCell.height());
        CreateFixedHeader(con, $table.attr('fixedHeader') == 'true', $table.attr('rowsCount'), $table.attr('colsCount'));
    }    
}

function _onscrollFixedHeader(e, oSelf) {
    for (var i = 0; i < e.target.children.length; i++) {
        if ($(e.target.children[i]).attr('isFixedHeader') == 'true')
            e.target.children[i].style.top = e.target.scrollTop + "px";
        if ($(e.target.children[i]).attr('isFixedColumns') == 'true')
            e.target.children[i].style.left = e.target.scrollLeft + "px";
    }
}


//***** работа с настрокой заголовка (видимость, направление текста, перетаскивание колонок)
var changeOrderOfColumns_colHDic = null;
var changeOrderOfColumns_colH = null;
var changeOrderOfColumnsCurrentCell = null;
var changeOrderOfColumns_sortCols = null;
var colHObjects = null;

function ShowTableSettings(divId) {
    var divT = $get(divId);
    var $divT = $(divT);
    var hfCols = $get($divT.attr('hfColsID'));
    if (hfCols.value == '') return;
    var hfSort = $get($divT.attr('hfSortID'));
    changeOrderOfColumns_sortCols = hfSort.value.split(',');
    var colH = Sys.Serialization.JavaScriptSerializer.deserialize(hfCols.value);
    changeOrderOfColumns_colH = colH;
    CreateTableByColumnHierarchy(divT, colH);
    changeOrderOfColumns_colHDic = new Array();
    changeOrderOfColumns_CreateListCells(colH, null);
    $find($divT.attr('behaviorID')).show();
}

function ApplyTableSettings(divId) {
    var divT = $get(divId);
    var $divT = $(divT);
    var hfCols = $get($divT.attr('hfColsID'));
    var hfSort = $get($divT.attr('hfSortID'));
    hfCols.value = Sys.Serialization.JavaScriptSerializer.serialize($(divT.firstChild).data('colH'));
    hfSort.value = changeOrderOfColumns_sortCols.join(",");
}

function CreateTableByColumnHierarchy(container, colH) {
    var t = document.createElement("table");
    t.border = 1;
    $(t).data('colH', colH);
    if (container.firstChild != null)
        container.removeChild(container.firstChild);
    container.appendChild(t);
    if (t.tBodies.length > 0)
        ntBody = t.tBodies[0];
    else
        ntBody = t.appendChild(document.createElement("tbody"));
    var r = null;
    var index = -1;
    var selectAgg = $get($(container).attr('selectAggID'));
    var dicContols = new Array();
    do {
        if (r != null)
            ntBody.appendChild(r);
        r = document.createElement("tr");
        index++;
    } while (CreateTableRowByColumnHierarchy(container, colH, r, index, dicContols, index.toString(), selectAgg));
}

function CreateTableRowByColumnHierarchy(container, colH, r, index, dicContols, key, selectAgg) {
    if (index < 0) return false;
    var result = false;
    for (var i = 0; i < colH.length; i++) {
        var col = colH[i];
        if (!col.VisibleInClient)
            continue;
        else if (index == 0) {
            col.key = key + i.toString();
            dicContols[col.key] = new Object();
            //добавляем ячейку
            var td = document.createElement("td");
            td.id = "settingColH_" + col.ClientID;
            td.colSpan = col.ClientColSpan;
            td.rowSpan = col.ClientRowSpan;
            td.style.textAlign = "center";
            td.onmousedown = changeOrderOfColumnsMouseDown;
            td.onmousemove = changeOrderOfColumnsMouseMove;
            var div = document.createElement("div");
            td.appendChild(div);
            //добавляем флаг видимости
            var visibleCheckbox = document.createElement("input");
            visibleCheckbox.type = "checkbox";
            div.appendChild(visibleCheckbox);
            dicContols[col.key].visible = visibleCheckbox;
            visibleCheckbox.col = col;
            visibleCheckbox.dicContols = dicContols;
            visibleCheckbox.onclick = _changeVisibleInColumnHierarchy;
            visibleCheckbox.ondblclick = _changeAllVisibleInColumnHierarchy;
            visibleCheckbox.title = container.visibleText;
            visibleCheckbox.checked = col.Visible;
            //добавляем заголовок колонки
            var input = createHiddenInputElement(col.Header);
            input.col = col;
            var span = createSpanElement(col.Header);
            //span.onclick = _clickHeaderSpanInColumnHierarchy;
            span.checkbox = visibleCheckbox;
            $(span).dblclick(function () {
                this.style.display = 'none';
                this.nextSibling.style.display = '';
                this.nextSibling.focus();
            });
            $(input).focusout(function () {
                this.col.Header = this.value;
                this.col.ShowHeaderText = true;
                this.style.display = 'none';
                this.previousSibling.innerHTML = this.value;
                this.previousSibling.style.display = '';
            });
            div.appendChild(span);
            div.appendChild(input);

            td.appendChild(document.createElement("br"));

            //иконка вертикального разворота
            var verticalHeaderCon = document.createElement("img");
            verticalHeaderCon.style.cursor = "pointer";
            if (col.IsVerticalHeader) {
                verticalHeaderCon.src = "/_themes/kvv/TextDirectionBU.png";
                //div.style.writingMode = "tb-rl";
                //div.style.filter = "flipv fliph";
            }
            else {
                verticalHeaderCon.src = "/_themes/kvv/TextDirectionLR.png";
            }
            td.appendChild(verticalHeaderCon);
            dicContols[col.key].verticalHeader = verticalHeaderCon;
            verticalHeaderCon.col = col;
            verticalHeaderCon.dicContols = dicContols;
            verticalHeaderCon.onclick = _changeIsVerticalHeaderInColumnHierarchy;
            verticalHeaderCon.ondblclick = _changeAllIsVerticalHeaderInColumnHierarchy;
            verticalHeaderCon.title = container.isVerticalHeaderText;

            //иконки сортировки
            if (col.OrderByColumn != null && col.OrderByColumn != "") {
                var orderDiv = document.createElement("div");
                var sortCol = col.OrderByColumn;
                if (Array.contains(changeOrderOfColumns_sortCols, sortCol))
                    CreateTableRowByColumnHierarchy_AddOrderButton(container.SOrderByColumnRemove, container.SOrderByColumn, "/_themes/KVV/OrderByColumnAsc", 'Asc', 'Remove', col.OrderByColumn, orderDiv);
                else
                    CreateTableRowByColumnHierarchy_AddOrderButton(container.SOrderByColumnRemove, container.SOrderByColumn, "/_themes/KVV/OrderByColumnAsc", 'Asc', 'Asc', col.OrderByColumn, orderDiv);
                if (Array.contains(changeOrderOfColumns_sortCols, sortCol + " desc"))
                    CreateTableRowByColumnHierarchy_AddOrderButton(container.SOrderByColumnRemove, container.SOrderByColumnDesc, "/_themes/KVV/OrderByColumnDesc", 'Desc', 'Remove', col.OrderByColumn, orderDiv);
                else
                    CreateTableRowByColumnHierarchy_AddOrderButton(container.SOrderByColumnRemove, container.SOrderByColumnDesc, "/_themes/KVV/OrderByColumnDesc", 'Desc', 'Desc', col.OrderByColumn, orderDiv);
                td.appendChild(orderDiv);
            }

            //возможность менять агрегацию пользователем
            if (selectAgg != null && col.AllowAggregate/*col.ColumnKey != null && col.ColumnKey != ''*/) {
                var aggDiv = document.createElement("div");
                aggDiv.innerHTML = selectAgg.outerHTML;
                aggDiv.firstChild.selectedIndex = col.AggregateType;
                aggDiv.firstChild.col = col;
                aggDiv.firstChild.onchange = CreateTableRowByColumnHierarchy_AggregateTypeChanged;
                if (!col.AllowAggregateSeted)
                    for (var sI = 1; sI < 6; sI++)
                        aggDiv.firstChild.options[sI].disabled = true;
                    
                td.appendChild(aggDiv);
            }
            
            r.appendChild(td);
            result = true;
        }
        else
            result |= CreateTableRowByColumnHierarchy(container, col.Childs, r, index - col.ClientRowSpan, dicContols, key + i.toString() + "_", selectAgg);
    }
    return result;
}
function CreateTableRowByColumnHierarchy_AggregateTypeChanged() {
    this.col.AggregateType = this.selectedIndex;
}
function CreateTableRowByColumnHierarchy_AddOrderButton(removeMessage, orderMessage, src, orderType, currentOrderType, columnName, orderDiv) {
    var a = document.createElement("a");
    var img = document.createElement("img");
    img.style.border = "0px";
    a.columnName = columnName;
    a.removeMessage = removeMessage;
    a.orderMessage = orderMessage;
    a.src = src;
    a.orderType = orderType;
    a.currentOrderType = currentOrderType;
    a.appendChild(img);
    a.onclick = CreateTableRowByColumnHierarchy_ClickOrderButton;
    a.href = 'javascript:void(0);';
    CreateTableRowByColumnHierarchy_SetOrderButtonParams(a);
    orderDiv.appendChild(a);
}

function CreateTableRowByColumnHierarchy_ClickOrderButton() {
    if (Array.contains(changeOrderOfColumns_sortCols, this.columnName))
        Array.remove(changeOrderOfColumns_sortCols, this.columnName);
    if (Array.contains(changeOrderOfColumns_sortCols, this.columnName + " desc"))
        Array.remove(changeOrderOfColumns_sortCols, this.columnName + " desc");
    var sort = "";
    if (this.currentOrderType == 'Remove')
        this.currentOrderType = this.orderType;
    else {
        this.currentOrderType = 'Remove';
        sort = this.columnName + (this.orderType == "Asc" ? "" : " desc");
        changeOrderOfColumns_sortCols.unshift(sort);
    }
    var otherA = this.nextSibling;
    if (otherA == null)
        otherA = this.previousSibling;
    otherA.currentOrderType = otherA.orderType;
    CreateTableRowByColumnHierarchy_SetOrderButtonParams(this);
    CreateTableRowByColumnHierarchy_SetOrderButtonParams(otherA);
}

function CreateTableRowByColumnHierarchy_SetOrderButtonParams(a) {
    var img = a.firstChild;
    if (a.currentOrderType == 'Remove'){
        img.src = a.src + 'S.png';
        img.alt = a.removeMessage;
        a.title = a.removeMessage;
    }
    else {
        img.src = a.src + '.png';
        img.alt = a.removeMessage;
        a.title = a.removeMessage;
    }
}

function createSpanElement(html) {
    var span = document.createElement("span");
    span.innerText = html;
    return span;
}

function createHiddenInputElement(text) {
    var input = document.createElement("input");
    input.value = text;
    input.style.display = 'none';
    return input;
}

function changeOrderOfColumnsMouseDown() {
    if (changeOrderOfColumnsCurrentCell != null) return;
    document.onmouseup = changeOrderOfColumnsMouseUp;
    if (document.onselectstart == null)
        document.onselectstart = changeWidthCellSelectStart;
    changeOrderOfColumnsSetStyle(changeOrderOfColumns_colHDic[this.id], "move", "#e0e0f0");
    changeOrderOfColumnsCurrentCell = this;
}
function changeOrderOfColumnsSetStyle(obj, cursor, backgroundColor) {
    var con = $get(obj.id);
    con.style.cursor = cursor;
    con.style.backgroundColor = backgroundColor;
    for (var i = 0; i < obj.childs.length; i++)
        changeOrderOfColumnsSetStyle(obj.childs[i], cursor, backgroundColor);
}

function changeOrderOfColumnsMouseMove() {
    if (changeOrderOfColumnsCurrentCell == null) {
        this.style.cursor = "default";
        return;
    }
    this.style.cursor = "move";
    var colH1 = changeOrderOfColumns_colHDic[this.id];
    var colH2 = changeOrderOfColumns_colHDic[changeOrderOfColumnsCurrentCell.id];
    var offsetX = event.offsetX;
    var con = event.srcElement;
    while (con.tagName.toLowerCase() != "td") {
        offsetX += con.offsetLeft;
        con = con.parentNode;
    }
    if (changeOrderOfColumnsCurrentCell.parentNode != this.parentNode
        || offsetX > changeOrderOfColumnsCurrentCell.offsetWidth
        || colH1.parent != colH2.parent)
        return;
    if ((changeOrderOfColumnsCurrentCell.cellIndex - this.cellIndex) != -1
            && (changeOrderOfColumnsCurrentCell.cellIndex - this.cellIndex) != 1)
        return;

    if (changeOrderOfColumnsCurrentCell.cellIndex > this.cellIndex) {
        this.parentNode.insertBefore(changeOrderOfColumnsCurrentCell, this);
        changeOrderOfColumnsMoveChilds(colH1, colH2, this, changeOrderOfColumnsCurrentCell);
    }
    else {
        this.parentNode.insertBefore(this, changeOrderOfColumnsCurrentCell);
        changeOrderOfColumnsMoveChilds(colH2, colH1, changeOrderOfColumnsCurrentCell, this);
    }

    var tempObj = colH1.array[colH1.index];
    colH1.array[colH1.index] = colH2.array[colH2.index];
    colH2.array[colH2.index] = tempObj;

    if (colH1.parent != null) {
        tempObj = colH1.parent.childs[colH1.index];
        colH1.parent.childs[colH1.index] = colH2.parent.childs[colH2.index];
        colH2.parent.childs[colH2.index] = tempObj;
    }
    
    var tempIndex = colH1.index;
    colH1.index = colH2.index;
    colH2.index = tempIndex;
}
function changeOrderOfColumnsMoveChilds(colH1, colH2, cell1, cell2) {
    var rowIndex1 = cell1.parentNode.rowIndex + cell1.rowSpan;
    var rowIndex2 = cell2.parentNode.rowIndex + cell2.rowSpan;
    var moveCount = changeOrderOfColumnsComputeChildsCount(rowIndex1, rowIndex2, colH2);
    for (var i1 = colH1.childs.length - 1; i1 >= 0; i1--) {
        var cell = $get(colH1.childs[i1].id);
        var nextIndex = cell.cellIndex + moveCount + 1;
        if (cell.parentNode.cells.length > nextIndex)
            cell.parentNode.insertBefore(cell, cell.parentNode.cells[nextIndex]);
        else
            cell.parentNode.appendChild(cell);
        changeOrderOfColumnsMoveChilds(colH1.childs[i1], colH2, cell, cell2);
    }
}
function changeOrderOfColumnsComputeChildsCount(rowIndex, currentRowIndex, colH) {
    if (rowIndex == currentRowIndex) {
        return colH.childs.length;
    }
    var count = 0;
    for (var i = 0; i < colH.childs.length; i++) {
        var cell = $get(colH.childs[i].id);
        count += changeOrderOfColumnsComputeChildsCount(rowIndex, currentRowIndex + cell.rowSpan, colH.childs[i]);
    }
    return count;
}

function changeOrderOfColumnsMouseUp() {
    document.onmouseup = null;
    if (document.onselectstart == changeWidthCellSelectStart)
        document.onselectstart = null;
    if (changeOrderOfColumnsCurrentCell != null) {
        changeOrderOfColumnsCurrentCell.onmouseout = null;
        changeOrderOfColumnsSetStyle(changeOrderOfColumns_colHDic[changeOrderOfColumnsCurrentCell.id], "default", "");
        changeOrderOfColumnsCurrentCell = null;
    }
}

function changeOrderOfColumns_LoadColHByCell(cell) {
    changeWidthCell_LoadColH(cell.parentNode.parentNode.parentNode);
}
function changeOrderOfColumns_CreateListCells(colH, parent) {
    var childs = new Array();
    for (var i = 0; i < colH.length; i++) {
        if (colH[i].ColumnKey != null) {
            var obj = new Object();
            var id = "settingColH_" + colH[i].ClientID;
            var con = $get(id);
            if (con == null) continue;
            obj.id = id;
            obj.item = colH[i];
            obj.parent = parent;
            obj.array = colH;
            obj.index = i;
            changeOrderOfColumns_colHDic[id] = obj;
            Array.add(childs, obj);
            if (colH[i].Childs != null)
                obj.childs = changeOrderOfColumns_CreateListCells(colH[i].Childs, obj);
        }
    }
    return childs;
}

function _changeVisibleInColumnHierarchy(event) {
    this.col.Visible = this.checked;
}

function _changeAllVisibleInColumnHierarchy(event) {
    this.col.Visible = this.checked;
    changeAllVisibleInColumnHierarchy(this.col.Childs, this.dicContols, this.checked);
}

function changeAllVisibleInColumnHierarchy(colH, dicContols, visible) {
    for (var i = 0; i < colH.length; i++) {
        var col = colH[i];
        col.Visible = visible;
        dicContols[col.key].visible.checked = visible;
        changeAllVisibleInColumnHierarchy(col.Childs, dicContols, visible);
    }
}
function _changeVerticalStyle(con) {
    if (con.col.IsVerticalHeader) {
        con.src = "/_themes/kvv/TextDirectionBU.png";
//        con.parentNode.firstChild.style.writingMode = "tb-rl";
//        con.parentNode.firstChild.style.filter = "flipv fliph";
    }
    else {
        con.src = "/_themes/kvv/TextDirectionLR.png";
//        con.parentNode.firstChild.style.writingMode = "";
//        con.parentNode.firstChild.style.filter = "";
    }
}
function _changeIsVerticalHeaderInColumnHierarchy(event) {
    this.col.IsVerticalHeader = !this.col.IsVerticalHeader;
    _changeVerticalStyle(this);
}

function _changeAllIsVerticalHeaderInColumnHierarchy(event) {
    this.col.IsVerticalHeader = !this.col.IsVerticalHeader;
    _changeVerticalStyle(this);
    changeAllIsVerticalHeaderInColumnHierarchy(this.col.Childs, this.dicContols, this.col.IsVerticalHeader);
}

function changeAllIsVerticalHeaderInColumnHierarchy(colH, dicContols, isVertical) {
    for (var i = 0; i < colH.length; i++) {
        var col = colH[i];
        col.IsVerticalHeader = isVertical;
        _changeVerticalStyle(dicContols[col.key].verticalHeader);
        changeAllIsVerticalHeaderInColumnHierarchy(col.Childs, dicContols, isVertical);
    }
}

function _clickHeaderSpanInColumnHierarchy(event) {
    this.checkbox.checked = !this.checkbox.checked;
    this.checkbox.fireEvent('onclick');
}


//***** загруска, сохранение, удаление настроек представлений
function getArgumentsForSaveViewSettings(argument, saveFiltersID, nameRuID, nameKzID, ddlID, saveAsSharedID) {
    var saveFilters = $get(saveFiltersID);
    var saveAsShared = $get(saveAsSharedID);
    var nameRu = $get(nameRuID);
    var nameKz = $get(nameKzID);
    var ddl = $get(ddlID);
    var param = new Object();
    param.saveFilters = saveFilters.checked;
    param.nameRu = nameRu.value;
    param.nameKz = nameKz.value;
    if (ddl.selectedIndex > 0 && ddl.options[ddl.selectedIndex].value != '' && !ddl.options[ddl.selectedIndex].disabled)
        param.id = ddl.value;
    param.saveAsShared = saveAsShared.checked;
    return argument + Sys.Serialization.JavaScriptSerializer.serialize(param);
}

function getArgumentsForLoadViewSettings(argument, loadFiltersID, ddlID) {
    var loadFilters = $get(loadFiltersID);
    var ddl = $get(ddlID);
    var param = new Object();
    param.loadFilters = loadFilters.checked;
    param.id = ddl.value;
    return argument + Sys.Serialization.JavaScriptSerializer.serialize(param);
}

function changeSelectedSaveViewSettings(ddl, nameRuID, nameKzID, saveAsSharedID, saveFiltersID, hlID) {
    if (ddl.selectedIndex > 0 && ddl.options[ddl.selectedIndex].value != '') {
        var nameRu = $get(nameRuID);
        var nameKz = $get(nameKzID);
        var saveAsShared = $get(saveAsSharedID);
        var saveFilters = $get(saveFiltersID);

        nameRu.value = ddl.options[ddl.selectedIndex].valueColl0;
        nameKz.value = ddl.options[ddl.selectedIndex].valueColl1;
        saveAsShared.checked = ddl.options[ddl.selectedIndex].valueColl2.toLowerCase() == 'true';
        saveFilters.checked = ddl.options[ddl.selectedIndex].valueColl3.toLowerCase() == 'true';
    }
    var hl = $get(hlID);
    hl.style.display = ddl.options.value == '' ? "none" : "";
}

function changeSelectedLoadViewSettings(ddl, hlID) {
    var hl = $get(hlID);
    hl.style.display = ddl.options.value == '' ? "none" : "";
}

function deleteSavedViewSettings(hl, hlOtherID, ddlID, ddlOtherID) {
    var ddl = $get(ddlID);
    var ddlOther = $get(ddlOtherID);
    var hlOther = $get(hlOtherID);
    if (ddl.selectedIndex > 0 && ddl.options[ddl.selectedIndex].value != '') {
        Nat.Web.Controls.GenerationClasses.WebServiceSavedJournalSettings.DeleteSavedSettings(ddl.value);
        var value = ddl.value;
        ddl.remove(ddl.selectedIndex);
        for (var i = 0; i < ddlOther.options.length; i++) {
            if (ddlOther.options[i].value == value) {
                ddlOther.remove(i);
                break;
            }
        }
    }
    hl.style.display = ddl.options.value == '' ? "none" : "";
    hlOther.style.display = ddl.options.value == '' ? "none" : "";
}

/*function cancelDeleteSavedViewSettings(ddlID, hfDel) {
    hfDel.value = "";
    ddl = $get(ddlID);
    for (var i = 0; i < ddl.options.length; i++)
        ddl.options[i].disabled = false;
}*/

//***** изменение ширины заголовков кросс таблиц
var _controlChangeWidth = null;
var _controlFixedChangeWidth = null;
var _changeWidthCell_colH = null;
var _changeWidthCell_colHDic = null;
var _changeWidthCell_rows = null;
var changeWidthCell_nextID = 0;

function changeWidthCellMouseMove(e) {
    if (!e) e = event;
    var $controlChangeWidth = $(_controlChangeWidth);
    if ($controlChangeWidth.attr('_tracking') == 'true') {
        var deltaX = $controlChangeWidth.attr('_trackingX') == 'true' ? (e.clientX - $controlChangeWidth.attr('_lastClientX')) : 0;
        var deltaY = $controlChangeWidth.attr('_trackingY') == 'true' ? (e.clientY - $controlChangeWidth.attr('_lastClientY')) : 0;
        changeWidthCell_ResizeControl(_controlChangeWidth, e.clientX, e.clientY, deltaX, deltaY);
    }
}
function changeWidthCellMouseUp(e) {
    $(_controlChangeWidth).attr('_tracking', 'false');
    changeWidthCell_RememberSizeByCell(_controlChangeWidth);
    document.onmousemove = null;
    document.onmouseup = null;
    if (document.onselectstart == changeWidthCellSelectStart)
        document.onselectstart = null;
    _controlChangeWidth.onmouseout = null;
    _controlChangeWidth.style.cursor = "";
    _controlChangeWidth.style.borderRight = "";
    _controlChangeWidth.style.borderBottom = "";
    _controlChangeWidth = null;
    if (_controlFixedChangeWidth != null) {
        _controlFixedChangeWidth.onmouseout = null;
        _controlFixedChangeWidth.style.cursor = "";
        _controlFixedChangeWidth.style.borderRight = "";
        _controlFixedChangeWidth.style.borderBottom = "";
        _controlFixedChangeWidth = null;
    }
}

function changeWidthCellMouseOut(e) {
    if ($(this).attr('_tracking') != "true") {
        this.style.cursor = "";
        this.style.borderRight = "";
        this.style.borderBottom = "";
    }
}
function changeWidthCellSelectStart(e) {
    return false;
}

function changeWidthCellMouseLeave(event, con) {
    if (_mousePressed == null) {
        con.style.cursor = "";
    }
    else {
        if (event.offsetX < 12)
            return;
        con.style.width = event.offsetX - 11 + "px";
    }
}

function _changeWidthCellMouseDown(e) {
    if (!e) e = event;
    changeWidthCellMouseDown(e, this);
}
function changeWidthCellMouseDown(e, con) {
    if (!e) e = event;
    var $con = $(con);
    if ($con.attr('originalID') != null)
        con = $get($con.attr('originalID'));
    else if ($con.attr('originalCell') != null)
        con = getOriginalCell(con);

    if (con == null) return;

    var trackingX = false;
    var trackingY = false;
    if (event.offsetX >= con.offsetWidth - 5)
        trackingX = true;
    if (event.offsetY >= con.offsetHeight - 5)
        trackingY = true;
    
    if (!trackingX && !trackingY) 
        return;
    
    _controlChangeWidth = con;
    changeWidthCell_LoadColHByCell(con);
    $con = $(con);
    $con.attr('_tracking', 'true');
    $con.attr('_trackingX', trackingX);
    $con.attr('_trackingY', trackingY);
    changeWidthCell_ResizeControl(con, e.clientX, e.clientY, 0, 0);
    document.onmousemove = changeWidthCellMouseMove;
    document.onmouseup = changeWidthCellMouseUp;
    if (document.onselectstart == null)
        document.onselectstart = changeWidthCellSelectStart;
    con.onmouseout = changeWidthCellMouseOut;
    if (trackingX)
        con.style.borderRight = "black 3px double";
    if (trackingY)
        con.style.borderBottom = "black 3px double";
}

function _changeWidthOnCellMouseMove(e) {
    if (!e) e = event;
    var con = this;
    var $con = $(con);
    if ($con.attr('originalID') != null)
        con = $get($con.attr('originalID'));
    else if ($con.attr('originalCell') != null)
        con = getOriginalCell(con);
    changeWidthOnCellMouseMove(e, this, con);
}
function changeWidthOnCellMouseMove(event, con, originalCon) {
    if (originalCon == null)
        originalCon = con;
    if ($(originalCon).attr('_tracking') != 'true') {
        var trackingX = false;
        var trackingY = false;
        if (event.offsetX >= con.offsetWidth - 5)
            trackingX = true;
        if (event.offsetY >= con.offsetHeight - 5)
            trackingY = true;

        if (trackingX && trackingY)
            con.style.cursor = "se-resize";
        else if (trackingX)
            con.style.cursor = "w-resize";
        else if (trackingY)
            con.style.cursor = "s-resize";
        else
            con.style.cursor = "";
    }
}

function changeWidthCell_ResizeControl(con, clientX, clientY, deltaX, deltaY) {
    $(con).attr('_lastClientX', clientX);
    $(con).attr('_lastClientY', clientY);
    changeWidthCell_changeInColH(con, deltaX);
    changeWidthCell_changeInRows(con, deltaY);
    if (!$.browser.msie)
        _onresizeFixedHeader(null, con);
/*    if (deltaX != 0) {
        //-12 потому что padding 5, то слева и справа по 5 это 10, плюс 1 за границу и 1 за увеличенную ширину границы
        var width = con.offsetWidth - 12 + deltaX;
        if (width < 10) width = 10;
        con.style.width = width + "px";
        if (con.firstChild != null)
            con.firstChild.style.width = width + "px";
        //подстройка из-за неправильного определения ширины, если полчилась ошибка на N пиксилей, то на них и изменяем ширину
        if (width != con.offsetWidth - 12) {
            width = width + (width - (con.offsetWidth - 12));
            if (width > 10) {
                con.style.width = width + "px";
                if (con.firstChild != null)
                    con.firstChild.style.width = width + "px";
            }
        }
    }
    if (deltaY != 0) {
        var height = con.offsetHeight - 12 + deltaY;
        if (height < 10) height = 10;
        con.style.height = height + "px";
        if (con.firstChild != null)
            con.firstChild.style.height = height + "px";
        //подстройка пока не нужна (подобная той что в ширине)
    }*/
}
function changeWidthCell_LoadColHByCell(cell) {
    changeWidthCell_LoadColH(cell.parentNode.parentNode.parentNode);
    changeWidthCell_LoadRows(cell.parentNode.parentNode.parentNode);
}
function changeWidthCell_LoadColH(table) {
    var hfCols = $get($(table).attr('hfColsID'));
    if (hfCols == null || hfCols.value == '') return;
    _changeWidthCell_colH = Sys.Serialization.JavaScriptSerializer.deserialize(hfCols.value);
    _changeWidthCell_colHDic = new Array();
    changeWidthCell_CreateListCells(_changeWidthCell_colH, $(table).attr('crossJournalID'), null);
}
function changeWidthCell_LoadRows(table) {
    var hf = $get($(table).attr('hfRowsHID'));
    if (hf == null || hf.value == '') return;
    _changeWidthCell_rows = Sys.Serialization.JavaScriptSerializer.deserialize(hf.value);
}
function changeWidthCell_CreateListCells(colH, crossJournalID, parent) {
    for (var i = 0; i < colH.length; i++) {
        if (colH[i].ClientID == null || colH[i].ClientID == "")
            continue;

        var obj = new Object();
        var id = colH[i].ClientID;
        var con = $get(id);
        if (con == null) {
            if (colH[i].Childs != null)
                changeWidthCell_CreateListCells(colH[i].Childs, crossJournalID, parent);
            continue;
        } 
        obj.id = id;
        obj.item = colH[i];
        obj.parent = parent;
        if (colH[i].IsVerticalHeader)
            _changeWidthCell_colHDic[id] = obj;

        if (colH[i].Childs != null)
            changeWidthCell_CreateListCells(colH[i].Childs, crossJournalID, obj);

        if (!colH[i].IsVerticalHeader)
            _changeWidthCell_colHDic[id] = obj;
    }
}

function changeWidthCell_changeInColH(cell, deltaX) {
    var table = cell.parentNode.parentNode.parentNode;
    var id = (cell.id == null || cell.id == "") ? $(cell).attr('originalID') : cell.id;
    var dicItem = _changeWidthCell_colHDic[id];
    if (dicItem == null)
        return;

    var oldWidth = cell.offsetWidth;
    changeWidthCell_changeSizeByColHItemX(cell, dicItem.item, deltaX);
    var crossJournalID = $(table).attr('crossJournalID');
    changeWidthCell_changeInColH_Down(dicItem.item, crossJournalID, deltaX);
    changeWidthCell_changeInColH_Up(dicItem.parent, crossJournalID, deltaX);
    var difDeltaX = cell.offsetWidth - (oldWidth + deltaX);
    if (difDeltaX != 0) {
        changeWidthCell_changeSizeByColHItem(cell, dicItem.item, difDeltaX, 0);
        changeWidthCell_changeInColH_Down(dicItem.item, crossJournalID, difDeltaX);
        changeWidthCell_changeInColH_Up(dicItem.parent, crossJournalID, difDeltaX);
    }
}

function changeWidthCell_changeInColH_Down(colHItem, crossJournalID, deltaX) {
    if (colHItem.Childs == null || colHItem.Childs.length == 0) return;
    for (var i = colHItem.Childs.length - 1; i >= 0; i--) {
        var lastChild = colHItem.Childs[i];
        var id = lastChild.ClientID;
        var cell = $get(id);
        if (cell != null) {
            changeWidthCell_changeSizeByColHItemX(cell, lastChild, deltaX);
            changeWidthCell_changeInColH_Down(lastChild, crossJournalID, deltaX);
            return;
        }
    }
}

function changeWidthCell_changeInColH_Up(dicItem, crossJournalID, deltaX) {
    if (dicItem == null) return;
    var id = dicItem.item.ClientID;
    var cell = $get(id);
    if (cell != null)
        changeWidthCell_changeSizeByColHItemX(cell, dicItem.item, deltaX);

    changeWidthCell_changeInColH_Up(dicItem.parent, crossJournalID, deltaX);
}
function changeWidthCell_changeSizeByColHItem(cell, colHItem, deltaX, deltaY) {
    changeWidthCell_changeSizeByColHItemX(cell, colHItem, deltaX);
    changeWidthCell_changeSizeByColHItemY(cell, colHItem, deltaY);
}
function changeWidthCell_changeSizeByColHItemX(cell, colHItem, deltaX) {
    if (deltaX != 0) {
/*        if (colHItem.Width == 0) {
            colHItem.Width = cell.offsetWidth - 11;
            if (colHItem.Width < 0)
                colHItem.Width = 0;
        }*/
        colHItem.Width += deltaX;
/*        if (colHItem.Width <= 0)
            colHItem.Width = 0; //todo: если было меньше 0, то необходимо количество на которое уменьшено передать левой ячейки, для высоты тоже самое*/
        changeWidthCell_changeSizeByColHItemSetWidth(cell, colHItem.Width);
        if ($(cell).attr('originalID') != null)
            changeWidthCell_changeSizeByColHItemSetWidth($get($(cell).attr('originalID')), colHItem.Width);
        else if ($(cell).attr('originalCell') != null)
            changeWidthCell_changeSizeByColHItemSetWidth(getOriginalCell(cell), colHItem.Width);
        var diff = colHItem.Width - cell.offsetWidth + 12;
        if ((diff > 5 || diff < -5) && deltaX > -5)
            colHItem.Width = cell.offsetWidth - 12;
    }
}
function changeWidthCell_changeInRows(cell, deltaY, important) {
    var table = cell.parentNode.parentNode.parentNode;
    var rowIndexStart = cell.parentNode.rowIndex;
    var rowIndex = cell.parentNode.rowIndex + (cell.rowSpan - 1);
    var oldHeight = _changeWidthCell_rows[rowIndex].Height;//if (oldHeight == null) oldHeight = table.rows[rowIndex].offsetHeight - 11;
    _changeWidthCell_rows[rowIndex].Height += deltaY;
    if (_changeWidthCell_rows[rowIndex].Height < 0) {
        deltaY = deltaY - _changeWidthCell_rows[rowIndex].Height;
        _changeWidthCell_rows[rowIndex].Height = 0;
    }
    if (deltaY == 0 && !important) return;
    var rowProp = _changeWidthCell_rows[rowIndex];
    table.tHead.rows[rowIndex].style.height = rowProp.Height + "px";
    for (var iR = 0; iR <= rowIndex; iR++) {
        var row = table.tHead.rows[iR];
        for (var iC = 0; iC < row.cells.length; iC++) {
            var rowCell = row.cells[iC];
            if (rowCell.rowSpan == 1 && iR == rowIndex) {
                changeWidthCell_changeSizeByColHItemSetHeight(rowCell, rowProp.Height);
            }
            else if (rowCell.rowSpan - 1 + iR == rowIndex) {
                var sumHeight = 0;
                for (var i = iR; i <= rowIndex; i++) {
                    sumHeight += _changeWidthCell_rows[i].Height;
                    if (i > iR)
                        sumHeight += 11;
                }
                changeWidthCell_changeSizeByColHItemSetHeight(rowCell, sumHeight);
            }
            else {
                var sumHeight = 0;
                for (var i = iR; i <= rowCell.rowSpan - 1 + iR; i++) {
                    sumHeight += _changeWidthCell_rows[i].Height;
                    if (i > iR)
                        sumHeight += 11;
                }
                changeWidthCell_changeSizeByColHItemSetHeight(rowCell, sumHeight);
            }
        }
    }
}

function changeWidthCell_changeSizeByColHItemY(cell, height, deltaY) {
    if (deltaY != 0) {
        if (colHItem.Height == 0) {
            colHItem.Height = cell.offsetHeight - 11;
            if (colHItem.Height < 0)
                colHItem.Height = 0;
        }
        colHItem.Height += deltaY;
        if (colHItem.Height <= 0)
            colHItem.Height = 0;
        changeWidthCell_changeSizeByColHItemSetHeight(cell, colHItem.Height);
        if ($(cell).attr('originalID') != null)
            changeWidthCell_changeSizeByColHItemSetHeight($get($(cell).attr('originalID')), colHItem.Height);
        else if($(cell).attr('originalCell') != null)
            changeWidthCell_changeSizeByColHItemSetHeight(getOriginalCell(cell), colHItem.Height);
    }
}
function changeWidthCell_changeSizeByColHItemSetWidth(cell, width) {
    if (width <= 0) width = 1;
    cell.style.width = width + "px";
    if (cell.firstChild != null && $(cell.firstChild).hasClass('DefaultRotate270Deg')) {
        cell.firstChild.style.height = width + "px";
        cell.firstChild.style.overflow = "hidden";
        SetRotateMargin(cell.firstChild);
    }
    else if (cell.firstChild != null) {
        cell.firstChild.style.width = width + "px";
        cell.firstChild.style.overflow = "hidden";
    }
}
function changeWidthCell_changeSizeByColHItemSetHeight(cell, height) {
    if (height <= 0) height = 1;
    cell.style.height = height + "px";
    if (cell.firstChild != null && $(cell.firstChild).hasClass('DefaultRotate270Deg')) {
        cell.firstChild.style.width = height + "px";
        cell.firstChild.style.overflow = "hidden";
        SetRotateMargin(cell.firstChild);
    }
    else if (cell.firstChild != null) {
        cell.firstChild.style.height = height + "px";
        cell.firstChild.style.overflow = "hidden";
    }
}

function SetRotateMargin(div) {
    var $div = $(div);
    var height = $div.height();
    var width = $div.width();
    $div.css('margin-top', (width - height) / 2 + 'px');
    $div.css('margin-bottom', (width - height) / 2 + 'px');
    $div.css('margin-left', (height - width) / 2 + 'px');
    $div.css('margin-right', (height - width) / 2 + 'px');
}

function changeWidthCell_RememberSizeByCell(cell) {
    changeWidthCell_RememberSize(cell.parentNode.parentNode.parentNode);
    changeWidthCell_RememberHeightHRows(cell.parentNode.parentNode.parentNode);
}
function changeWidthCell_RememberSize(table) {
    var hfCols = $get($(table).attr('hfColsID'));
    if (hfCols == null || _changeWidthCell_colH == null) return;
    hfCols.value = Sys.Serialization.JavaScriptSerializer.serialize(_changeWidthCell_colH);
    _changeWidthCell_colH = null;
    _changeWidthCell_colHDic = null;
}
function changeWidthCell_RememberHeightHRows(table) {
    var hf = $get($(table).attr('hfRowsHID'));
    if (hf == null || _changeWidthCell_rows == null) return;
    hf.value = Sys.Serialization.JavaScriptSerializer.serialize(_changeWidthCell_rows);
    _changeWidthCell_rows = null;
}
function changeWidthCell_RememberSizeOnLoad(table) {
    $(table).attr('crossJournalID', table.id);
    $(table).width($(table).width() - 2);
    changeWidthCell_LoadColH(table);
    for (var key in _changeWidthCell_colHDic) {
        if (!_changeWidthCell_colHDic.hasOwnProperty(key))
            continue;
        var itemDic = _changeWidthCell_colHDic[key];
        if (itemDic.item.Width > 0 && !itemDic.item.IsVerticalHeader)
            continue;
        var con = $get(itemDic.id);
        if (con != null) {
            if (itemDic.item.IsVerticalHeader && Sys.Browser.agent !== Sys.Browser.InternetExplorer) {
                var width = con.offsetWidth - 11;
                if (width <= 0) width = 1;
                if (width > 150) width = 150;
                itemDic.item.Height = width;
                $(con.firstChild).addClass('DefaultRotate270Deg');
                changeWidthCell_changeSizeByColHItemSetHeight(con, width);
                width = con.offsetWidth - 11;
                if (itemDic.item.Width == 0)
                    itemDic.item.Width = width;
            }
            else if (itemDic.item.Width == 0) {
                var width = con.offsetWidth - 11;
                if (width <= 0) width = 1;
                itemDic.item.Width = width;
            }

            changeWidthCell_changeSizeByColHItemSetWidth(con, itemDic.item.Width);
        }
    }
    if (table.tHead.rows.length > 0) {
        var row = table.tHead.rows[0];
        if (row.cells.length > 0){
            var cell = row.cells[row.cells.length - 1];
            if (cell.style.visibility == "hidden")
                cell.style.width = "100%";
        }
    }
    changeWidthCell_LoadRows(table);
    if (_changeWidthCell_rows == null)
        _changeWidthCell_rows = new Array();
    for (var i = 0; i < table.tHead.rows.length; i++) {
        var obj = null;
        if (_changeWidthCell_rows.length <= i) {
            obj = new Object();
            Array.add(_changeWidthCell_rows, obj);
        }
        else
            obj = _changeWidthCell_rows[i];
        if (obj.Height == null)
            obj.Height = table.tHead.rows[i].offsetHeight - 11;
    }
    changeWidthCell_RememberSize(table);
    changeWidthCell_changeInRows(table.rows[0].cells[0], 0, true);
    changeWidthCell_RememberHeightHRows(table);
}

//***** раскарска журнала
var _fillCells_selectedColor = "#FFFFFF";
var _fillCells_action = null;
var _fillCells_actionCon = null;
var _fillCells_Clear = false;
var _fillCells_SFont = false;

function fillCellsByColor(table, action, con) {
    if (_fillCells_actionCon != null) {
        _fillCells_actionCon.firstChild.src = _fillCells_actionCon.originalSrc;
        _fillCells_actionCon.title = _fillCells_actionCon.originalTitle;
        _fillCells_actionCon.firstChild.alt = _fillCells_actionCon.originalTitle;
        _fillCells_actionCon = null;
        if (_fillCells_action == action) {
            table.onmousemove = null;
            table.onmousedown = null;
            return;
        }
    }
    _fillCells_action = action;
    _fillCells_actionCon = con;
    _fillCells_SFont = action.length > 7 && action.substring(0, 8).toLowerCase() == "fillfont";
    con.originalSrc = con.firstChild.src;
    con.originalTitle = con.title;
    con.title = con.cancelTitle;
    con.firstChild.alt = con.cancelTitle;
    if (action == "fillRow")
        con.firstChild.src = "/_themes/kvv/FillRowS.png";
    else if (action == "fillColumn")
        con.firstChild.src = "/_themes/kvv/FillColumnS.png";
    else if (action == "fillCell")
        con.firstChild.src = "/_themes/kvv/FillCellS.png";
    else if (action == "fillFontRow")
        con.firstChild.src = "/_themes/kvv/FontRowS.png";
    else if (action == "fillFontColumn")
        con.firstChild.src = "/_themes/kvv/FontColumnS.png";
    else if (action == "fillFontCell")
        con.firstChild.src = "/_themes/kvv/FontCellS.png";
    table.onclick = fillCellsByColorMouseDown;
    table.ondblclick = fillCellsByColorMouseDown;
    var otherTable = $get(table.id + "_cols");
    if (otherTable != null) {
        otherTable.onclick = fillCellsByColorMouseDown;
        otherTable.ondblclick = fillCellsByColorMouseDown;
    }
    otherTable = $get(table.id + "_rows");
    if (otherTable != null) {
        otherTable.onclick = fillCellsByColorMouseDown;
        otherTable.ondblclick = fillCellsByColorMouseDown;
    }
    otherTable = $get(table.id + "_cros");
    if (otherTable != null) {
        otherTable.onclick = fillCellsByColorMouseDown;
        otherTable.ondblclick = fillCellsByColorMouseDown;
    }
}
function fillCells_ColorChanged(sender) {
    _fillCells_selectedColor = "#" + sender.get_selectedColor();
}

function fillCellsByColorMouseDown(e) {
    e = event;
    logEvent(e.type);
    var cell = e.srcElement;
    while (cell.tagName.toLowerCase() != "td" && cell.tagName.toLowerCase() != "th") {
        if (cell.tagName.toLowerCase() == "table") return;
        cell = cell.parentNode;
    }
    var oldCell = null;
    if ($(cell).attr('originalID') != null) {
        oldCell = cell;
        cell = $get($(cell).attr('originalID'));
    }
    else if ($(cell).attr('originalCell')) {
        oldCell = cell;
        cell = getOriginalCell(cell);
    }
    var row = cell.parentNode;
    var table = row.parentNode;
    if (table.tagName.toLowerCase() != "table")
        table = table.parentNode;
    _fillCells_Clear = e.type == "dblclick";

    if (_fillCells_action == "fillRow" || _fillCells_action == "fillFontRow")
        fillCellsSetRowProps(cell, row, table);
    else if (_fillCells_action == "fillColumn" || _fillCells_action == "fillFontColumn")
        fillCellsSetColumnProps(cell, row, table);
    else if (_fillCells_action == "fillCell" || _fillCells_action == "fillFontCell")
        fillCellsSetCellProps(cell, row, table);

    if (oldCell != null || _fillCells_action == "fillRow" || _fillCells_action == "fillFontRow")
        CreateFixedHeader(table, $(table).attr('fixedHeader') == 'true', $(table).attr('rowsCount'), $(table).attr('colsCount'));
}

function fillCellsSetRowProps(cell, row, table) {
    if ($(row).attr('rowKey') == null) return;

    $table = $(table);
    var hf = $get($table.attr('hfRowsID'));
    var rowsProps = null;
    if (hf.value == '')
        rowsProps = new Array();
    else
        rowsProps = Sys.Serialization.JavaScriptSerializer.deserialize(hf.value);
    var props = null
    for (var i = 0; i < rowsProps.length; i++) {
        if (rowsProps[i].Key == $(row).attr('rowKey')) {
            props = rowsProps[i];
            break;
        }
    }
    if (props == null) {
        props = new Object();
        Array.add(rowsProps, props);
        props.Key = $(row).attr('rowKey');
    }
    fillCellsSetColor(row, props);

    hf.value = Sys.Serialization.JavaScriptSerializer.serialize(rowsProps);
}

function fillCellsSetCellProps(cell, row, table) {
    if ($(cell).attr('cellKey') == null) return;
    
    var hf = $get($(table).attr('hfCellsID'));
    var cellsProps = null;
    if (hf.value == '')
        cellsProps = new Array();
    else
        cellsProps = Sys.Serialization.JavaScriptSerializer.deserialize(hf.value);
    var props = null
    for (var i = 0; i < cellsProps.length; i++) {
        if (cellsProps[i].Key == $(cell).attr('cellKey')){
            props = cellsProps[i];
            break;
        }
    }
    if (props == null) {
        props = new Object();
        Array.add(cellsProps, props);
        props.Key = $(cell).attr('cellKey');
    }
    fillCellsSetColor(cell, props);
    
    hf.value = Sys.Serialization.JavaScriptSerializer.serialize(cellsProps);
}

function fillCellsSetColor(control, props) {
    if (_fillCells_SFont) {
        if (_fillCells_Clear || _fillCells_selectedColor == props.PColor)
            props.PColor = '';
        else
            props.PColor = _fillCells_selectedColor;
        if (control != null) control.style.color = props.PColor;
    }
    else {
        if (_fillCells_Clear || _fillCells_selectedColor == props.BColor)
            props.BColor = '';
        else
            props.BColor = _fillCells_selectedColor;
        if (control != null) control.style.backgroundColor = props.BColor;
    } 
}

var _fillCellsSetColumnPropsRememerParents = null;

function fillCellsSetColumnProps(cell, row, table) {
    if ($(row).attr('rowKey') != null || $(cell).attr('cellKey') != null)
        return;

    var hf = $get($(table).attr('hfColsID'));
    var colH = null;
    if (hf.value == '')
        return;
    else
        colH = Sys.Serialization.JavaScriptSerializer.deserialize(hf.value);

    _fillCellsSetColumnPropsRememerParents = new Object();
    var crossJournalID = $(table).attr('crossJournalID');
    var tree = fillCellsSetColumnPropsGetColHItem(cell, colH, crossJournalID == null ? table.id : crossJournalID);
    if (tree == null) return;
    var colHItem = _fillCellsSetColumnPropsRememerParents.item; 
    if (colHItem.ColumnKey == null && !fillCellsSetColumnPropsExistsChildColumnKey(colHItem.Childs))
        colHItem = fillCellsSetColumnPropsGetParentWithColumnKey(_fillCellsSetColumnPropsRememerParents);
    if (colHItem == null) return;

    createIndexesInCellsOfTable(table);
    fillCellsSetColumnPropsByColHItem(colHItem, table, crossJournalID == null ? table.id : crossJournalID);

    hf.value = Sys.Serialization.JavaScriptSerializer.serialize(colH);
}

function fillCellsSetColumnPropsExistsChildColumnKey(colH) {
    for (var i = 0; i < colH.length; i++) {
        if (colH[i].ColumnKey != null)
            return true;
        if (colH[i].Childs != null && fillCellsSetColumnPropsExistsChildColumnKey(colH[i].Childs))
            return true;
    }
    return false;
}

function fillCellsSetColumnPropsGetParentWithColumnKey(treeItem) {
    if (treeItem.item.ColumnKey != null)
        return treeItem.item;
    if (treeItem.parent == null) return null;
    return fillCellsSetColumnPropsGetParentWithColumnKey(treeItem.parent);
}

function fillCellsSetColumnPropsByColHItem(colHItem, table, crossJournalID, colIndex) {
    var id = colHItem.ClientID;
    var cell = $get(id);
    fillCellsSetColor(cell, colHItem);
    if (cell != null)
        fillCellsSetColorForColumn(table, cell.columnIndex, colHItem);
    if (colHItem.Childs != null)
        for (var i = 0; i < colHItem.Childs.length; i++)
            fillCellsSetColumnPropsByColHItem(colHItem.Childs[i], table, crossJournalID);
}

function fillCellsSetColorForColumn(table, columnIndex, colHItem) {
    var tBody = table.tBodies[0];
    
    var hf = $get($(table).attr('hfCellsID'));
    var cellsProps = null;
    if (hf.value == '')
        cellsProps = new Array();
    else
        cellsProps = Sys.Serialization.JavaScriptSerializer.deserialize(hf.value);
    var props = null;
    for (var i = 0; i < cellsProps.length; i++)
        cellsProps[cellsProps[i].Key] = cellsProps[i];

    for (var r = 0; r < tBody.rows.length; r++) {
        var row = tBody.rows[r];
        for (var c = 0; c < row.cells.length; c++) {
            var cell = row.cells[c];
            var $cell = $(cell);
            var cellProps = cellsProps[$cell.attr('cellKey')];
            if (cell.columnIndex == columnIndex) {
                if (_fillCells_SFont) {
                    if (cellProps == null || cellProps.PColor == null || cellProps.PColor == "")
                        cell.style.color = colHItem.PColor;
                }
                else if (cellProps == null || cellProps.BColor == null || cellProps.BColor == "")
                    cell.style.backgroundColor = colHItem.BColor;
            }
        }
    }
}

function fillCellsSetColumnPropsGetColHItem(cell, colH, crossJournalID) {
    for (var i = 0; i < colH.length; i++) {
        var id = colH[i].ClientID;
        if (cell.id == id) {
            _fillCellsSetColumnPropsRememerParents.colH = colH;
            _fillCellsSetColumnPropsRememerParents.item = colH[i];
            return _fillCellsSetColumnPropsRememerParents;
        }
        if (colH[i].Childs != null) {
            var colHItem = fillCellsSetColumnPropsGetColHItem(cell, colH[i].Childs, crossJournalID);
            if (colHItem != null) {
                colHItem.parent = new Object();
                colHItem.parent.colH = colH;
                colHItem.parent.item = colH[i];
                return colHItem.parent;
            }
        }
    }
    return null;
}

function createIndexesInCellsOfTable(table) {
    if (table.columnIndexCreated) return;
    table.columnIndexCreated = true;
    var rowSpans = new Array();
    for (var r = 0; r < table.rows.length; r++) {
        var rowSpansN = new Array();
        var row = table.rows[r];
        var indexCol = 0; //остановить обход по ячейкам если дошли до ограничевающей колнки
        for (var key in rowSpans) {
            if (!rowSpans.hasOwnProperty(key))
                continue;
            rowSpans[key] -= 1;
        }
        for (var c = 0; c < row.cells.length; c++) {
            var cell = row.cells[c];
            while (rowSpans[indexCol] > 0)
                indexCol++;
            for (var iSpan = 0; iSpan < cell.colSpan; iSpan++)
                rowSpansN[indexCol + iSpan] = cell.rowSpan;
            cell.columnIndex = indexCol;
            indexCol += cell.colSpan;
        }
        for (var key in rowSpansN) {
            if (!rowSpansN.hasOwnProperty(key))
                continue;
            rowSpans[key] = rowSpansN[key];
        }
    }
}

function logEvent(action) {
    var log = $get("logDiv");
    if (log == null) return;
    var e = event;
    log.innerHTML = "action:" + action
        + "; offsetX:" + e.offsetX
        + "; offsetY:" + e.offsetY
        + "; tag:" + e.srcElement.tagName
        + "; id:" + e.srcElement.id
        + "; parent.tag:" + e.srcElement.parentNode.tagName
        + "; parent.id:" + e.srcElement.parentNode.id
        + "<br/>" + log.innerHTML;
    if (log.innerHTML.length > 1000)
        log.innerHTML = log.innerHTML.substring(0, 1000);
}
//***** Create concatenate column
function concatenateColumnsFromLeftList(divId) {
    concatenateColumnsMoveListItem($(divId).attr('lvLeftColumnListID'), $(divId).attr('lvRightColumnListID'));

}
function concatenateColumnsFromRightList(divId) {
    concatenateColumnsMoveListItem($(divId).attr('lvRightColumnListID'), $(divId).attr('lvLeftColumnListID'));
}
function concatenateColumnsMoveListItem(ctrlSource, ctrlTarget) {
    var Source = document.getElementById(ctrlSource);
    var Target = document.getElementById(ctrlTarget);

    if ((Source != null) && (Target != null)) {
        while (Source.options.selectedIndex >= 0) {
            if (Source.options[Source.options.selectedIndex].value.indexOf('conc_') == -1) {
                var newOption = new Option(); // Create a new instance of ListItem
                newOption.text = Source.options[Source.options.selectedIndex].text;
                newOption.value = Source.options[Source.options.selectedIndex].value;

                Target.options[Target.length] = newOption; //Append the item in Target
                Source.remove(Source.options.selectedIndex);  //Remove the item from Source
            } else {
                Source.options[Source.options.selectedIndex].selected = false;
            }
        }
    }
}
function concatenateColumnsDeleteColumn(divId) {
    var columnList = document.getElementById($(divId).attr('ddlConcatenatedColumnsID'));
    if (columnList != null) {
        var hfConcColsRemove = document.getElementById($(divId).attr('hfConcColsRemoveID'));
        hfConcColsRemove.value += columnList.options[columnList.options.selectedIndex].value + ';';
        columnList.remove(columnList.options.selectedIndex);
    }
}

function createConcatenateColumns(divId) {
    var columnList = document.getElementById($(divId).attr('lvRightColumnListID'));
    var concList = '';
    for (i = 0; i < columnList.length; i++) {
        concList += columnList[i].value + ';';
    }
    var hfConcColsNew = document.getElementById($(divId).attr('hfConcColsNewID'));
    hfConcColsNew.value = concList;
}

function initIDDivTopDetecter() {
    var div = $('#idDivTopDetecter1');
    var div2 = $('#idDivTopDetecter2');
    var div3 = $('#idDivTopDetecter3');
    if (div.length == 1) {
        var top = div.offset().top + 10 + div3.offset().top - div2.offset().top;
        if (window.Ext != null || top < 0)
            top = '';
        else
            top = top + 'px';
        div3.next().css('top', top);

        $(window).resize(function () {
            if (div.length == 1) {
                top = div.offset().top + 10 + div3.offset().top - div2.offset().top;

                if (window.Ext != null || top < 0)
                    top = '';
                else
                    top = top + 'px';
                div3.next().css('top', top);
            }
        });
    }
}

function _resizeCrossTable(newHeight) {
    if (window.App == null || window.App.PlaceHolderMain_item_filter_FiltersPanel == null)
        return;

    if (newHeight == null)
        newHeight = window.App.PlaceHolderMain_item_filter_FiltersPanel.getHeight();
    var height = $(window).height() - newHeight - 52;
    if (height > 120)
        $('#PlaceHolderMain_item_Journal').parent().height(height);
    else
        $('#PlaceHolderMain_item_Journal').parent().height(120);
}

if (window.Ext) {
    Ext.onReady(function() {
        $(window).resize(function() { _resizeCrossTable(null); });
        App.PlaceHolderMain_item_filter_FiltersPanel.addListener('resize',
            function(ctrl, newWidth, newHeight) { _resizeCrossTable(newHeight); });
        _resizeCrossTable();
    });
}