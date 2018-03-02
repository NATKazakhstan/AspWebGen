﻿;

// скрипт для CheckboxSelectionModel ExtNet гридов
// не снимаем проставленные галочки, при клике по ячейке, содержащей чек-бокс, но мимо чек-бокса
Ext.onReady(function () {
    var grids = Ext.ComponentQuery.query('grid');
    if (grids.length > 0)
    {
        $.each(grids, function (i, grid)
        {
            if (grid.selModel && grid.selModel.selType == "checkboxmodel" && grid.store)
            {
                grid.selModel.addListener("beforedeselect", OnGridSelectionModelBeforeDeselect);
                grid.store.addListener("load", OnGridStoreLoadForSelectionModel);
            }
        });
    }
});

var isGSM_CheckBoxCellClicked = false;
var isGSM_CheckBoxChanged = false;

var OnGridStoreLoadForSelectionModel = function (store) {
    if (store && store.storeId && store.storeId.length > 0)
    {
        var journalGridId = store.storeId.replace("_store", "_grid");
        $("#" + journalGridId).find(".x-grid-cell-row-checker").mousedown(function () {
            isGSM_CheckBoxCellClicked = true;
        });

        $("#" + journalGridId).find(".x-grid-cell-row-checker").find(".x-grid-row-checker").mousedown(function () {
            isGSM_CheckBoxChanged = true;
        });

        $("#" + journalGridId).find("tr.x-grid-row").mouseup(function () {
            if (isGSM_CheckBoxCellClicked || isGSM_CheckBoxChanged) {
                setTimeout(function () {
                    isGSM_CheckBoxCellClicked = false;
                    isGSM_CheckBoxChanged = false;
                }, 10);
            }
        });
    }
}

var OnGridSelectionModelBeforeDeselect = function () {
    if (isGSM_CheckBoxCellClicked && !isGSM_CheckBoxChanged) {
        return false;
    }
}