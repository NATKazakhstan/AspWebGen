;

// скрипт для CheckboxSelectionModel генеренных гридов
// не снимаем проставленные галочки, при клике по ячейке, содержащей чек-бокс, но мимо чек-бокса
Ext.onReady(function () {
    console.log("init CheckboxSelectionModel");
    var gridSelModel = App.PlaceHolderMain_item_gridSelectionModel;
    var gridStore = App.PlaceHolderMain_item_store;
    if (gridSelModel && gridStore) {
        console.log("CheckboxSelectionModel: gridSelModel and gridStore finded");
        gridSelModel.addListener("beforedeselect", OnGridSelectionModelBeforeDeselect);
        gridStore.addListener("load", OnGridStoreLoadForSelectionModel);
    }
});

var isGSM_CheckBoxCellClicked = false;
var isGSM_CheckBoxChanged = false;

var OnGridStoreLoadForSelectionModel = function () {
    $("#PlaceHolderMain_item_grid").find(".x-grid-cell-row-checker").mousedown(function () {
        isGSM_CheckBoxCellClicked = true;
    });

    $("#PlaceHolderMain_item_grid").find(".x-grid-cell-row-checker").find(".x-grid-row-checker").mousedown(function () {
        isGSM_CheckBoxChanged = true;
    });

    $("#PlaceHolderMain_item_grid").find(".x-grid-cell-row-checker").mouseup(function () {
        setTimeout(function () {
            isGSM_CheckBoxCellClicked = false;
            isGSM_CheckBoxChanged = false;
        }, 10);
    });
}

var OnGridSelectionModelBeforeDeselect = function () {
    if (isGSM_CheckBoxCellClicked && !isGSM_CheckBoxChanged) {
        return false;
    }
}