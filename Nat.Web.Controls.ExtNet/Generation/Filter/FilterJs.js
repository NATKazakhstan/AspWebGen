// Скрипт устанавливает значение поумолчанию полям значений фильтра.
Array.prototype.SetDefaultValueToFilterValueControl = function(objFilterRepository, isFirstControl) {
    var defaultObjectsArray = window.defaultValuesFilterRepository
        .ObjectOfControlInDefaultRepository(objFilterRepository.FilterName);

    // Реализация присвоения значения по умолчанию для фильтров без контролов значения.
    if (objFilterRepository.ClientFirstFilterValueControl == undefined && isFirstControl) {
        if (defaultObjectsArray != undefined && defaultObjectsArray.length > 0) {
            setDefaultValueToFilterType(objFilterRepository, defaultObjectsArray);
        }
        return;
    }

    objFilterRepository.ClientFirstFilterValueControl["isSetDefaultValue"] = true;
    if (defaultObjectsArray != undefined && defaultObjectsArray.length > 0) {

        var filterOperation = defaultObjectsArray[0].FilterType;
        if (filterOperation.endsWith("ByRef")) {
            if (isFirstControl &&
                defaultObjectsArray[0].Value2 != undefined &&
                defaultObjectsArray[0].Value2.toString().length > 0) {
                objFilterRepository.ClientFirstFilterValueControl.setValue(defaultObjectsArray[0].Value2);
                setDefaultValueToFilterType(objFilterRepository, defaultObjectsArray);
            }
        } else if (objFilterRepository.ClientFirstFilterValueControl.xtype == "netmulticombo" ||
            objFilterRepository.ClientFirstFilterValueControl.xtype == "combobox") {
            var defaultValues = defaultObjectsArray.getDefaultValues();
            var defaultTextByValues = defaultObjectsArray.getDefaultValuesByText();
            if (defaultValues != undefined && defaultValues.length > 0) {
                if (objFilterRepository.ClientFirstFilterValueControl.xtype != "combobox") {
                    for (var i = 0; i < defaultObjectsArray.length; i++) {
                        if (objFilterRepository.ClientFirstFilterValueControl.store
                            .getById(defaultObjectsArray[i].Value1) ==
                            null)
                            objFilterRepository.ClientFirstFilterValueControl.store
                                .add({ id: defaultObjectsArray[i].Value1, RowName: defaultObjectsArray[i].Value2 });
                        objFilterRepository.ClientFirstFilterValueControl.displayTplData
                            .push({ id: defaultObjectsArray[i].Value1, RowName: defaultObjectsArray[i].Value2 });
                        objFilterRepository.ClientFirstFilterValueControl.value.push(defaultObjectsArray[i].Value1);
                    }
                } else {
                    objFilterRepository.filterRepositoryReady = false;
                    window.filterRepository.SetLookupValue(objFilterRepository.ClientFirstFilterValueControl.id, defaultValues);
                    objFilterRepository.ClientFirstFilterValueControl["isSetDefaultValue"] = true;
                    for (var i = 0; i < defaultObjectsArray.length; i++) {
                        objFilterRepository.ClientFirstFilterValueControl.store
                            .add({ id: defaultObjectsArray[i].Value1, RowName: defaultObjectsArray[i].Value2 });
                    }

                    objFilterRepository.ClientFirstFilterValueControl.store.on("load",
                        function(store, records, options, el) {

                            var element = objFilterRepository.ClientFirstFilterValueControl;
                            if (element.isSetDefaultValue != undefined && element.isSetDefaultValue) {
                                for (var i = 0; i < defaultObjectsArray.length; i++) {
                                    if (!store.getById(defaultObjectsArray[i].Value1))
                                        store.add({
                                            id: defaultObjectsArray[i].Value1,
                                            RowName: defaultObjectsArray[i].Value2
                                        });
                                }

                                objFilterRepository.filterRepositoryReady = true;
                                element.setValue(defaultValues);

                                element.on("change",
                                    function() {
                                        var el = Ext.getCmp(this.id);
                                        if (el.isSetDefaultValue != undefined && el.isSetDefaultValue) {
                                            el.isSetDefaultValue = false;
                                        }
                                    });
                            }
                        });

                    /*objFilterRepository.ClientFirstFilterValueControl.inputEl.on('click', function(){
                        var el = Ext.getCmp(this.dom.name);
                        el.clearValue();
                        window.filterRepository.SetLookupValue(el.id, undefined);
                        el.getTrigger(0).hide();
                        if (el.isSetDefaultValue != undefined && el.isSetDefaultValue){
                        el.isSetDefaultValue = false;
                        }
                    });*/
                }

                objFilterRepository.ClientFirstFilterValueControl.setValue(defaultValues);
                setDefaultValueToFilterType(objFilterRepository, defaultObjectsArray);
            }
        } else {
            if (isFirstControl) {
                if (defaultObjectsArray[0].Value1 != undefined && defaultObjectsArray[0].Value1.toString().length > 0) {
                    objFilterRepository.ClientFirstFilterValueControl.setValue(defaultObjectsArray[0].Value1);
                    setDefaultValueToFilterType(objFilterRepository, defaultObjectsArray);
                }
            } else {
                if (defaultObjectsArray[0].Value2 != undefined && defaultObjectsArray[0].Value2.toString().length > 0) {
                    objFilterRepository.ClientSecondFilterValueControl.setValue(defaultObjectsArray[0].Value2);
                }
            }
        }
    }

    function setDefaultValueToFilterType(objFilterRepository, defaultObjectsArray) {
        objFilterRepository.ClientFilterTypeControl.setValue(defaultObjectsArray[0].FilterType);
        objFilterRepository.ClientFilterTypeControl.defaultValue = defaultObjectsArray[0].FilterType;
    };
};

//Метод регистрирует в репозитории контрол отвечающий за выбор операции фильтрации.
Array.prototype.RegistrateFilterOperationControl = function() {
    function DefaultFilterValue(defaultFilterType, element) {
        if (element.defaultValue == null)
            element.setValue(defaultFilterType);
        else
            element.setValue(element.defaultValue);
    };

    this.forEach(function(filterObj) {
        eval("filterObj.ClientFilterTypeControl = App." + filterObj.FilterTypeId + ";");

        var idSelect = "#" + filterObj.ClientFilterTypeControl.id;
        var label = $(idSelect).closest("tbody").find("label");
        if (label != undefined) {
            var labelText = label.text();
            if (labelText != undefined && labelText.length > 22) {
                label.attr("data-qtip", labelText);
            }
        }
        if (filterObj.FirstFilterValueId != undefined && filterObj.FirstFilterValueId != "")
            eval("filterObj.ClientFirstFilterValueControl = App." + filterObj.FirstFilterValueId + ";");
        if (filterObj.SecondFilterValueId != undefined && filterObj.SecondFilterValueId != "")
            eval("filterObj.ClientSecondFilterValueControl = App." + filterObj.SecondFilterValueId + ";");

        if (filterObj.ClientFirstFilterValueControl != null &&
        (filterObj.ClientFirstFilterValueControl.xtype == "netmulticombo" ||
            filterObj.ClientFirstFilterValueControl.xtype == "combobox")) {
            filterObj.ClientFirstFilterValueControl
                .addListener("beforeQuery",
                    function(e) { e.cancel = filterObj.ClientFirstFilterValueControl.isTextFilter; });
            filterObj.ClientFilterTypeControl.on("change",
                function(item, newValue, oldValue) {
                    var isTextFilter = newValue != null && newValue.endsWith("ByRef");
                    var oldIsTextFilter = oldValue != null && oldValue.endsWith("ByRef");
                    var lookup = filterObj.ClientFirstFilterValueControl.xtype == "combobox";
                    filterObj.ClientFirstFilterValueControl.setEditable(isTextFilter || lookup);
                    filterObj.ClientFirstFilterValueControl.setHideTrigger(isTextFilter);
                    filterObj.ClientFirstFilterValueControl.forceSelection = !isTextFilter && lookup;
                    filterObj.ClientFirstFilterValueControl.isTextFilter = isTextFilter;
                    if (!isTextFilter && oldIsTextFilter) {
                        filterObj.ClientFirstFilterValueControl.clear();
                        filterObj.ClientFirstFilterValueControl.getTrigger(0).hide();
                        if (filterObj.LookupValues != undefined)
                            filterObj.LookupValues = undefined;
                    }
                });
        }

        // На случай если панель развернута
        filterObj.ClientFilterTypeControl.store.on("load",
            function(store, records, options, el) {
                DefaultFilterValue(filterObj.DefaultFilterType, Ext.getCmp(el.scope.el.id));
            });

        // На случай если панель свернута
        DefaultFilterValue(filterObj.DefaultFilterType, filterObj.ClientFilterTypeControl);

        filterObj.filterRepositoryReady = true;

        if (filterObj.ClientFirstFilterValueControl != null ||
            (filterObj.ClientFirstFilterValueControl == null && filterObj.ClientFirstFilterValueControl == null))
            window.defaultValuesFilterRepository.SetDefaultValueToFilterValueControl(filterObj, true);

        if (filterObj.ClientSecondFilterValueControl != null) {
            window.defaultValuesFilterRepository.SetDefaultValueToFilterValueControl(filterObj, false);
            
            // убираю это хардкодное скрытие второго поля, т.к. его отображение должно зависеть (и зависит на тек. момент) от типа фильтра
            /*
            var idControl = "#" + filterObj.ClientSecondFilterValueControl.id;
            $(idControl).hide();
            */
        }
    });

    this.InitToggleVisibleDependedControl();
};

// Скрипт записывает массив значений множественного выбора из окна справочника в репозиторий.
Array.prototype.SetLookupValue = function(controlId, values) {
    this.forEach(function(filterObj) {
        if (filterObj.ClientFirstFilterValueControl != undefined &&
            filterObj.ClientFirstFilterValueControl.id == controlId) {
            filterObj.LookupValues = values;
        }
    });
};

// Скрипт добавляет в массив значений множественного выбора значение.
Array.prototype.AddLookupValue = function(controlId, value) {
    this.forEach(function(filterObj) {
        if (filterObj.ClientFirstFilterValueControl != undefined &&
            filterObj.ClientFirstFilterValueControl.id == controlId) {
            if (filterObj.LookupValues == undefined) filterObj.LookupValues = new Array();
            if (value != undefined && filterObj.LookupValues.indexOf(value) == -1) filterObj.LookupValues.push(value);
        }
    });
};

// Метод очищает данные фильтра.
Array.prototype.CleanupFilter = function() {
    this.forEach(function(filterObj) {
        if (filterObj.ClientFirstFilterValueControl != undefined) {
            filterObj.ClientFirstFilterValueControl.clear();
            if (filterObj.LookupValues != undefined) {
                filterObj.LookupValues = undefined;
            }
            if (filterObj.ClientFirstFilterValueControl.xtype == "netmulticombo" ||
                filterObj.ClientFirstFilterValueControl.xtype == "combobox") {
                filterObj.ClientFirstFilterValueControl.getTrigger(0).hide();
            }
        }

        if (filterObj.ClientSecondFilterValueControl != undefined) {
            filterObj.ClientSecondFilterValueControl.clear();
            filterObj.ClientSecondFilterValueControl.setMinValue("");
            filterObj.ClientSecondFilterValueControl.allowBlank = true;
            filterObj.ClientSecondFilterValueControl.clearInvalid();
            filterObj.ClientFirstFilterValueControl.clearInvalid();
            filterObj.ClientFirstFilterValueControl
                .setMaxValue(filterObj.ClientFirstFilterValueControl.rememberMaxValue);
            filterObj.ClientFirstFilterValueControl.allowBlank = true;
        }

        if (filterObj.ClientFirstFilterValueControl == null)
            filterObj.ClientFilterTypeControl.setValue(null);
    });
};


//Скрипт обратывает данные репозитория и передает их в JS функцию для фильтрации знчений.
Array.prototype.ApplyFilter = function(applyFunctionName) {
    var isApplyFilter = true;
    var filtersList = new Array();
    this.forEach(function(filterObj) {
        if (filterObj.ClientFilterTypeControl != undefined &&
            filterObj.FilterName != undefined &&
            filterObj.FilterTypeId != undefined) {

            var filterOperation = filterObj.ClientFilterTypeControl.getValue();
            if (filterOperation == null || filterObj.disabledByDependeds) {
                if (filterObj.RequiredFilter) {
                    isApplyFilter = false;
                    filterObj.ClientFilterTypeControl.allowBlank = false;
                    filterObj.ClientFilterTypeControl.validate();
                }

                return;
            }

            filterObj.ClientFilterTypeControl.allowBlank = true;
            filterObj.ClientFilterTypeControl.clearInvalid();

            if (filterOperation !== "IsNull" &&
                filterOperation !== "IsNotNull" &&
                filterObj.ClientFirstFilterValueControl &&
                filterObj.ClientFirstFilterValueControl.rememberMaxValue &&
                filterObj.ClientFirstFilterValueControl.getValue() &&
                Date.parseInvariant(filterObj.ClientFirstFilterValueControl.rememberMaxValue, "dd.MM.yyyy") <
                filterObj.ClientFirstFilterValueControl.getValue()) {
                filterObj.ClientFirstFilterValueControl.validate();
                isApplyFilter = false;
                return;
            }

            if (filterOperation === "Between" &&
                filterObj.ClientSecondFilterValueControl &&
                filterObj.ClientSecondFilterValueControl.rememberMaxValue &&
                filterObj.ClientSecondFilterValueControl.getValue() &&
                Date.parseInvariant(filterObj.ClientSecondFilterValueControl.rememberMaxValue, "dd.MM.yyyy") <
                filterObj.ClientSecondFilterValueControl.getValue()) {
                filterObj.ClientSecondFilterValueControl.validate();
                isApplyFilter = false;
                return;
            }

            if (filterOperation === "Between") {

                if (filterObj.ClientFirstFilterValueControl.getValue() != undefined &&
                    filterObj.ClientSecondFilterValueControl.getValue() != undefined) {
                    filtersList.push({
                        FilterName: filterObj.FilterName,
                        FilterType: filterOperation,
                        Value1: getControlValue(filterObj.ClientFirstFilterValueControl),
                        Value2: getControlValue(filterObj.ClientSecondFilterValueControl)
                    });
                } else if (filterObj.ClientFirstFilterValueControl.getValue() != undefined ||
                    filterObj.ClientSecondFilterValueControl.getValue() != undefined ||
                    filterObj.RequiredFilter) {
                    isApplyFilter = false;
                    filterObj.ClientFirstFilterValueControl.allowBlank = false;
                    filterObj.ClientSecondFilterValueControl.allowBlank = false;
                    filterObj.ClientFirstFilterValueControl.validate();
                    filterObj.ClientSecondFilterValueControl.validate();
                } else {
                    filterObj.ClientFirstFilterValueControl.allowBlank = true;
                    filterObj.ClientSecondFilterValueControl.allowBlank = true;
                    filterObj.ClientFirstFilterValueControl.clearInvalid();
                    filterObj.ClientSecondFilterValueControl.clearInvalid();
                }
            } else if (filterOperation.endsWith("ByRef")) {
                if (filterObj.RequiredFilter && filterObj.ClientFirstFilterValueControl.getValue() == null) {
                    isApplyFilter = false;
                    filterObj.ClientFirstFilterValueControl.allowBlank = false;
                    filterObj.ClientFirstFilterValueControl.validate();
                } else {
                    filterObj.ClientFirstFilterValueControl.allowBlank = true;
                    filterObj.ClientFirstFilterValueControl.clearInvalid();
                    filtersList.push({
                        FilterName: filterObj.FilterName,
                        FilterType: filterOperation,
                        Value2: filterObj.ClientFirstFilterValueControl.getRawValue()
                    });
                }
            } else {
                var value = getControlValue(filterObj.ClientFirstFilterValueControl);
                if (filterObj.RequiredFilter && (value == null || value == "")) {
                    isApplyFilter = false;
                    filterObj.ClientFirstFilterValueControl.allowBlank = false;
                    filterObj.ClientFirstFilterValueControl.validate();
                } else {
                    if (filterObj.ClientFirstFilterValueControl !== undefined) {
                        filterObj.ClientFirstFilterValueControl.allowBlank = true;
                        filterObj.ClientFirstFilterValueControl.clearInvalid();
                        var itemsValue = value.toString().split(", ");
                        if (itemsValue != null && itemsValue.length > 0 && filterObj.LookupValues != undefined) {
                            for (var i = 0; i < filterObj.LookupValues.length; i++) {
                                filtersList.push({
                                    FilterName: filterObj.FilterName,
                                    FilterType: filterOperation,
                                    Value1: filterObj.LookupValues[i],
                                    Value2: filterObj.ClientFirstFilterValueControl.store.data.items
                                        .getTextByValue(filterObj.LookupValues[i])
                                });
                            }
                        } else if (itemsValue != undefined && itemsValue.length > 1) {
                            for (var i = 0; i < itemsValue.length; i++) {
                                filtersList.push({
                                    FilterName: filterObj.FilterName,
                                    FilterType: filterOperation,
                                    Value1: itemsValue[i],
                                    Value2: ""
                                });
                            }
                        } else {
                            filtersList.push({
                                FilterName: filterObj.FilterName,
                                FilterType: filterOperation,
                                Value1: getControlValue(filterObj.ClientFirstFilterValueControl),
                                Value2: ""
                            });
                        }
                    } else {
                        filtersList.push({
                            FilterName: filterObj.FilterName,
                            FilterType: filterOperation,
                            Value1: true,
                            Value2: ""
                        });
                    }
                }
            }
        }
    });

    if (!isApplyFilter) return;

    var filterValuesJSON = Sys.Serialization.JavaScriptSerializer.serialize(filtersList);
    window[applyFunctionName](filterValuesJSON);

    // встроенная функция

    function getControlValue(control) {
        if (control != undefined) {
            if (control.xtype != "datefield") {
                var val = control.getValue();
                if (val != undefined && val.constructor == Array) {
                    return val.join(", ");
                } else {
                    return val == null || val == undefined ? "" : val;
                }
            } else {
                var result = control.rawValue;
                var itemsDate = result.split(".");
                if (itemsDate.length == 3) {
                    var date = new Date(itemsDate[2], itemsDate[1], itemsDate[0]);
                    if (date != NaN) {
                        return result;
                    } else {
                        return "";
                    }
                } else {
                    return "";
                }
            }
        } else {
            return "";
        }
    }
};

// Метод ищет контрол в репозитории по Id.
Array.prototype.FindControlById = function(controlId) {
    var control = undefined;
    this.forEach(function(filterObj) {
        if (filterObj.ClientFirstFilterValueControl != undefined &&
            filterObj.ClientFirstFilterValueControl.id == controlId) {
            control = filterObj.ClientFirstFilterValueControl;
        }
        if (filterObj.ClientSecondFilterValueControl != undefined &&
            filterObj.ClientSecondFilterValueControl.id == controlId) {
            control = filterObj.ClientSecondFilterValueControl;
        }
    });
    return control;
};

// Метод ищет контрол в репозитории по FilterName.
Array.prototype.FindControlByFilterName = function(filterName) {

    for (var i = 0, len = this.length; i < len; ++i)
    {
        if (this[i] != undefined && this[i].FilterName == filterName) {
            if (this[i].ClientFirstFilterValueControl)
                return this[i].ClientFirstFilterValueControl;
            return this[i].ClientFilterTypeControl;
        }
    }

    return null;
};

// Определение метода foreach для IE.
if (!Array.prototype.forEach) {
    Array.prototype.forEach = function(fn, scope) {
        for (var i = 0, len = this.length; i < len; ++i) {
            if (this[i] != undefined) {
                fn.call(scope, this[i], i, this);
            }
        }
    };
};

// Скрипт ищет в репозитории объект по id контрола.
Array.prototype.ObjectOfControl = function(control) {
    var i = this.length;
    while (i--) {
        var items = control.id.split(this[i].FilterName);
        var exp = RegExp("_" + this[i].FilterName + "$");
        if (items[1] != undefined && items[1].length == 0 && exp.test(control.id)) {
            return this[i];
        }
    }
    return undefined;
};

// Скрипт ищет в репозитории значений по умолчанию объект по id контрола.
Array.prototype.ObjectOfControlInDefaultRepository = function(filterName) {
    var arr = new Array();
    var i = this.length;
    while (i--) {
        if (this[i].FilterName != undefined && this[i].FilterName.length > 0 && filterName == this[i].FilterName) {
            arr.push(this[i]);
        }
    }
    return arr;
};

// Скрипт формирует массив значений для поля типа lookup.
Array.prototype.getDefaultValues = function() {
    var arr = new Array();
    var i = this.length;
    while (i--) {
        if (this[i].Value1 != "") {
            arr.push(this[i].Value1);
        }
    }
    return arr;
};

// Скрипт формирует массив значений для поля типа lookup.
Array.prototype.getDefaultValuesByText = function() {
    var text = "";
    var i = this.length;
    while (i--) {
        if (text == "") {
            text = this[i].Value2;
        } else {
            text = text + ", " + this[i].Value2;
        }
    }
    return text;
};

// Скрипт возращает текст позначению записи.
Array.prototype.getTextByValue = function(value) {
    var i = this.length;
    while (i--) {
        if (this[i].raw["id"] == value) return this[i].raw["RowName"];
    }
    return "";
};

// Обработчик: 
//  - переключает видимость второго контрола в диапазонной группе;
//  - устанавливает корректные значения валидируемым контролам.
Array.prototype.ToggleVisibleRangeControl = function(control, isShow) {
    var filterObj = this.ObjectOfControl(control);
    if (filterObj != undefined) {
        if (filterObj.ClientSecondFilterValueControl != undefined) {
            var idControl = "#" + filterObj.ClientSecondFilterValueControl.id;

            if (filterObj.ClientFirstFilterValueControl.isSetDefaultValue != undefined && isShow) {
                if (!filterObj.ClientFirstFilterValueControl.isSetDefaultValue) {
                    filterObj.ClientSecondFilterValueControl.clear();
                }
                filterObj.ClientFirstFilterValueControl.isSetDefaultValue = false;
            }

            filterObj.ClientFirstFilterValueControl
                .setMaxValue(filterObj.ClientFirstFilterValueControl.rememberMaxValue);
            filterObj.ClientSecondFilterValueControl.setMinValue("");
            filterObj.ClientFirstFilterValueControl.allowBlank = true;
            filterObj.ClientSecondFilterValueControl.allowBlank = true;
            filterObj.ClientFirstFilterValueControl.clearInvalid();
            filterObj.ClientSecondFilterValueControl.clearInvalid();

            isShow ? $(idControl).show() : $(idControl).hide();
            //filterObj.ClientFirstFilterValueControl.validate();
            //filterObj.ClientSecondFilterValueControl.validate();
        }
    }
};

// Обработчик: 
//  - переключает видимость для зависимых контролов (пока один не заполнен второй не доступен);
//  - устанавливает корректные значения валидируемым контролам.
Array.prototype.ToggleVisibleDependedControl = function (control) {
    if (!control || !window.filterRepositoryReady)
        return;
    var filterObj = this.ObjectOfControl(control);
    if (!filterObj ||
        !filterObj.DependedFilters ||
        filterObj.DependedFilters.length === 0 ||
        !filterObj.GetDependedVisible
        || !filterObj.filterRepositoryReady)
        return;

    if (!filterObj.dependedFilterControls) {
        var depended = filterObj.DependedFilters;
        var dependedControls = [];
        for (var i = 0; i < depended.length; i++) {
            var dependControl = window.filterRepository.FindControlByFilterName(depended[i]);
            if (dependControl == null)
                return;
            Array.add(dependedControls, dependControl);
        }

        filterObj.dependedFilterControls = dependedControls;
    }
    var values = [];
    for (var i = 0; i < filterObj.dependedFilterControls.length; i++) {
        values[i] = filterObj.dependedFilterControls[i].getValue();
    }

    var isShow = filterObj.GetDependedVisible(values);
    if (filterObj.ClientFirstFilterValueControl) {
        if (filterObj.ClientFirstFilterValueControl.isSetDefaultValue != undefined) {
            filterObj.ClientFirstFilterValueControl.isSetDefaultValue = false;
        }

        if (filterObj.ClientFirstFilterValueControl.setMaxValue &&
            filterObj.ClientFirstFilterValueControl.rememberMaxValue) {
            var maxValue = filterObj.ClientFirstFilterValueControl.rememberMaxValue;
            filterObj.ClientFirstFilterValueControl.setMaxValue(maxValue);
        }
        filterObj.ClientFirstFilterValueControl.allowBlank = true;
        filterObj.ClientFirstFilterValueControl.clearInvalid();
        /*if (!isShow) {
            if (filterObj.ClientFirstFilterValueControl.clearValue)
                filterObj.ClientFirstFilterValueControl.clearValue();
            else if (filterObj.ClientFirstFilterValueControl.clear)
                filterObj.ClientFirstFilterValueControl.clear();
        }*/
    }

    if (filterObj.ClientSecondFilterValueControl) {
        if (filterObj.ClientSecondFilterValueControl.setMinValue)
            filterObj.ClientSecondFilterValueControl.setMinValue("");
        filterObj.ClientSecondFilterValueControl.allowBlank = true;
        filterObj.ClientSecondFilterValueControl.clearInvalid();

        /*if (!isShow) {
            if (filterObj.ClientSecondFilterValueControl.clearValue)
                filterObj.ClientSecondFilterValueControl.clearValue();
            else if (filterObj.ClientSecondFilterValueControl.clear)
                filterObj.ClientSecondFilterValueControl.clear();
        }*/
    }

    var filedContainer = filterObj.ClientFilterTypeControl.up('[xtype="fieldcontainer"]');
    filterObj.disabledByDependeds = !isShow;
    filedContainer.setDisabled(!isShow);
};

Array.prototype.InitToggleVisibleDependedControl = function () {

    window.filterRepositoryReady = true;

    this.forEach(function (filterObj) {
        if (filterObj.ClientFirstFilterValueControl)
            window.filterRepository.ToggleVisibleDependedControl(filterObj.ClientFirstFilterValueControl);
        else if (filterObj.ClientFilterTypeControl)
            window.filterRepository.ToggleVisibleDependedControl(filterObj.ClientFilterTypeControl);
    });
}


// Скрипт обработки функциональности триггера(ComboBox, MultiCombo).
var lookuprefTypeClick = function(el, trigger, tag, auto, modalWindowDictionary) {
    switch (tag) {
    case "open":
        var control = window.filterRepository.FindControlById(el.id);
        var filterObj = window.filterRepository.ObjectOfControl(control);
        var value = filterObj.ClientFirstFilterValueControl.getValue();
        window.isClearValues = filterObj.LookupValues == null && (value == null || value.length === 0);

        modalWindowDictionary.editor = el;
        modalWindowDictionary.autoComplete = auto;
        var newUrl = "/EmptyPage.aspx/data/" +
            el.tag.tableName +
            "Journal/multipleselect?mode=none&viewmode=none" +
            GetLookupFiltersByValues(el.tag.browseUrl);

        var urlChanged = false;
        if (modalWindowDictionary.loader.url != newUrl) {
            modalWindowDictionary.loader.url = newUrl;
            urlChanged = true;
            window.el = el;
        }

        modalWindowDictionary.setTitle(el.tag.header);
        modalWindowDictionary.show();

        var frame;
        if (urlChanged || window.el != el) {
            modalWindowDictionary.clearContent();
            modalWindowDictionary.loader.load();
            frame = modalWindowDictionary.getFrame();
            frame.isClearValues = function() {
                return window.isClearValues;
            };

            frame.addSelectedValues = function(records) {
                modalWindowDictionary.hide();
                var lookupValues = new Array();
                var store = el.getStore();
                for (var i = 0; i < records.length; i++) {
                    var rowID = store.getFieldByName("id").type.convert(records[i].data.id);
                    var hasSameRow = store.getById(rowID) != undefined;
                    if (!hasSameRow) {
                        // возможно нужна будет возможность добавления записи 
                        if (!store.getById(rowID))
                            store.add({ id: rowID, RowName: records[i].data.RecordName });
                    }
                    lookupValues.push(rowID);
                }
                window.filterRepository.SetLookupValue(el.id, lookupValues);
                el.clearValue();
                el.setValue(lookupValues);
                el.getTrigger(0).show();
            };

            frame.closeModalWindow = function() {
                modalWindowDictionary.hide();
            };

            frame.removeSelectedValues = function(record) {
                el.removeByValue(record.id);
                el.clearValue();
            };

            window.modalframe = frame;
        }

        frame = modalWindowDictionary.getFrame();
        frame.selectedRecords = [];
        if (filterObj.LookupValues && value != null && value.length > 0)
            for (var iSel = 0; iSel < filterObj.LookupValues.length; iSel++) {
                var addRow = filterObj.ClientFirstFilterValueControl.store.getById(filterObj.LookupValues[iSel]);
                if (addRow) Array.add(frame.selectedRecords, addRow);
            }

        break;

    case "clear":
        el.clearValue();
        window.filterRepository.SetLookupValue(el.id, undefined);
        el.getTrigger(0).hide();
        break;
    }
};

window.lookuprefTypeClick = lookuprefTypeClick;


// Обработчик: 
//  - переключает видимость дополнительных фильтров;
//  - устанавливает корректные значения валидируемым контролам.
Array.prototype.ToggleVisibleAdditionalFilters = function() {
    App.MainFilterFieldSet.setVisible(false);
    this.forEach(function(filterObj) {
        if (Boolean(filterObj.IsMain)) {
            var isMainFilter = filterObj.IsMain == "True";
            var isHiddenParentFieldSet = filterObj.IsHiddenParentFieldSet == "True";
            var isHideParentGroupFieldSet = filterObj.IsHideParentGroupFieldSet == "True";
            if (!isMainFilter) {

                var filedContainer = filterObj.ClientFilterTypeControl.up('[xtype="fieldcontainer"]');
                var isVisible = filedContainer.isVisible();

                if (isVisible) {
                    if (filterObj.ClientFirstFilterValueControl != undefined) {
                        filterObj.ClientFirstFilterValueControl.clear();
                        filterObj.ClientFirstFilterValueControl.allowBlank = true;
                        filterObj.ClientFirstFilterValueControl.clearInvalid();
                    }

                    if (filterObj.ClientSecondFilterValueControl != undefined) {
                        filterObj.ClientSecondFilterValueControl.clear();
                        filterObj.ClientSecondFilterValueControl.allowBlank = true;
                        filterObj.ClientSecondFilterValueControl.setMinValue("");
                        filterObj.ClientSecondFilterValueControl.clearInvalid();
                        filterObj.ClientFirstFilterValueControl
                            .setMaxValue(filterObj.ClientFirstFilterValueControl.rememberMaxValue);
                        //filterObj.ClientFirstFilterValueControl.validate();
                    }

                    if (isHideParentGroupFieldSet) {
                        var fieldSet = filedContainer.up('[xtype="fieldset"]');
                        var parentGroupFiledSet = fieldSet.up('[xtype="fieldset"]');
                        parentGroupFiledSet.setVisible(false);
                        parentGroupFiledSet.collapse();
                        fieldSet.collapse();
                    } else if (isHiddenParentFieldSet) {
                        var fieldSet = filedContainer.up('[xtype="fieldset"]');
                        fieldSet.setVisible(false);
                        fieldSet.collapse();
                    }

                    filedContainer.setVisible(false);
                } else {

                    filedContainer.setVisible(true);
                    if (isHideParentGroupFieldSet) {
                        var fieldSet = filedContainer.up('[xtype="fieldset"]');
                        var parentGroupFiledSet = fieldSet.up('[xtype="fieldset"]');
                        parentGroupFiledSet.collapse();
                        fieldSet.setVisible(true);
                        parentGroupFiledSet.setVisible(true);
                    } else if (isHiddenParentFieldSet) {
                        var fieldSet = filedContainer.up('[xtype="fieldset"]');
                        fieldSet.setVisible(true);
                        fieldSet.collapse();
                    }
                }
            }
        }
    });
    App.MainFilterFieldSet.setVisible(true);
};