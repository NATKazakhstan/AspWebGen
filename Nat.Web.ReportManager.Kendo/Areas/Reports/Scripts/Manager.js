Nat.Reports.Manager = function (options) {

    Nat.Classes.SimpleGridController.call(this, 'Reports/Manager');

    this.options = kendo.observable(options);
    this.options.showSubscription = true;
    if (this.options.Name)
        this.options.DisplayName = ' / ' + this.options.Name;

    this.Initialize = function () {
        var me = this;

        this.grid = $('#pluginsTree').data('kendoTreeList');
        this.reportWindow = $('#reportWindow').data('kendoWindow');

        var searchDiv = kendo.template($('#pluginsSearch').html())({ divStyle: '', id: 'searchValueInput' });
        this.grid.toolbar.find('button[data-command="search"]').replaceWith(searchDiv);

        kendo.bind($('#searchValueInput'), this.options);
        kendo.bind($('.reportToolbar'), this.options);
        kendo.bind($('#spanReportName'), this.options);

        this.options.bind('change', function (e) { me.onFieldChange(e); });
        this.onFieldChange({ field: '*' });

        if (this.options.viewOne) {
            var splitter = $('#managerSplitter').data('kendoSplitter');
            $('#managerSplitter .k-splitbar').hide();
            splitter.collapse($('#pluginsTree').closest('.k-pane'));
            this.open();
        } else {
            this.bindSelectionChange();
            this.bindProgress(null, function () { me.onEndRequest(); });
            this.bindGetData();

            $(window).on('popstate', function(e) { me.onPopState(e); });
        }
    };

    this.onGetData = function() {
        return {
            searchValue: this.options.searchValue
        };
    };

    this.onSelectionChange = function () {
        var dataItem = this.getSelected();
        if (dataItem && !dataItem.PluginName) {
            if (dataItem.expanded)
                this.grid.collapse(this.grid.select());
            else
                this.grid.expand(this.grid.select());
        }

        this.options.set('PluginName', !dataItem || !dataItem.PluginName ? null : dataItem.PluginName);
        this.options.set('PluginType', !dataItem || !dataItem.PluginType ? null : dataItem.PluginType);
        this.options.set('Name', !dataItem || !dataItem.Name ? null : dataItem.Name);
        this.options.set('DisplayName', !dataItem || !dataItem.Name ? '' : ' / ' + dataItem.Name);
        if (!this._meInOpen)
            this.open(true);
    };

    this.onLangClick = function(isKz) {
        this.options.set('isKz', isKz);
    };

    this.onCreateClick = function (newTab) {
        var me = this;
        var data = {
            PluginName: this.options.PluginName,
            culture: this.options.isKz ? 'kz' : 'ru',
            parameters: this.GetParameters()
        };
        this.post('CreateReport',
            data,
            function(result) {
                if (result.error)
                    return;

                if (result.Url) {
                    if (newTab)
                        window.open(result.Url, "_blank");
                    else {
                        var w = $(window);
                        var dataItem = me.getSelected();
                        me.reportWindow.setOptions({ width: w.width() * 0.95, height: w.height() * 0.9, title: dataItem ? dataItem.Name : options.name || '' });
                        me.reportWindow.content('<h6>Загрузка... <br/> Енгізу...</h6>');
                        var url = result.Url.indexOf('MainPage.aspx') === -1
                            ? result.Url + '&HideMainMenu=true'
                            : result.Url.replace('/MainPage.aspx/', '/EmptyPage.aspx/');
                        me.reportWindow.refresh({ url: url, iframe: true });
                        kendo.ui.progress(me.reportWindow.element, true);
                        me.reportWindow.open();
                        me.reportWindow.center();
                    }
                    return;
                }

                //$('#reportResultDiv').html('<iframe width="100%" height="500px" style="border: 0px">' + result.ReportContent + '</iframe>');
                var content = $(result.ReportContent);
                $('#reportResultDiv').html(content.filter(function() {
                    return this.tagName !== 'SCRIPT' && this.tagName !== 'META';
                }));
                var anyText = $('#reportResultDiv > *').filter(function() {
                    return this.tagName !== 'STYLE' && this.tagName !== 'TITLE';
                }).text();
                if (anyText) {
                    $('#reportResultDiv').show();
                    var height = 300 +
                        $('.reportToolbar').height() +
                        $('#reportParameters').height() -
                        $('#reportParameters').closest('.k-pane').height();
                    if (height > 0)
                        $('#reportResultDiv').focus();
                }
                else
                    $('#reportResultDiv').hide();
            },
            false);
    };

    this.onWordExportClick = function() {
        this.ExportDocument('Word', ".doc");
    };

    this.onExcelExportClick = function() {
        this.ExportDocument('Excel', ".xls");
    };

    this.onPDFExportClick = function() {
        this.ExportDocument('Pdf', ".pdf");
    };

    this.onSubscriptionClick = function() {
        var me = this;
        var data = {
            PluginName: me.options.PluginName,
            culture: me.options.isKz ? 'kz' : 'ru',
            parameters: this.GetParameters(),
            subscription: true
        };

        this.post('CreateReport',
            data,
            function (result) {
                if (result.error)
                    return;

                if (result.Url)
                    window.open(result.Url, "_blank");
            },
            false);
    };

    this.onSaveSubscriptionClick = function() {
        var me = this;
        var data = {
            PluginName: me.options.PluginName,
            parameters: this.GetParameters(),
            idSubscription: this.options.idSubscription
        };

        this.post('SaveSubscription',
            data,
            function (result) {
                if (result.success)
                    window.location = me.options.url;
            },
            false);
    };

    this.onAddParametersClick = function() {
        var params = $('#reportParameters');

        for (var i = 0; i < this.parameters.length; i++) {
            var item = this.parameters[i];
            
            if (!item.Visible || !item.AllowAddParameter || item.ParameterClone)
                continue;

            item = item.toJSON();
            item.ParameterIndex = this.parameters.length;
            item.ParameterClone = true;
            item.Removed = false;
            this.parameters.push(item);
            item = this.parameters[this.parameters.length - 1];

            this.InitParameterItem(params, item);
        }
        $('#ClearParameters').remove();
        params.append('<div id="ClearParameters" style="clear: both;"/>');
    };

    this.onRemoveParameterClick = function (key, link, parameterIndex) {
        this.parameters[parameterIndex].set('Removed', true);
        $('#LabelID_' + parameterIndex).remove();
        $('#ParameterID_' + parameterIndex).remove();
    };

    this.onPopState = function(e) {
        var state = e.state;
        if (!state && e.originalEvent && e.originalEvent.state)
            state = e.originalEvent.state;

        if (state) {
            this.setOptions(state);
            this.open(false);
        }
        else
            window.location.reload();
    };

    this.onEndRequest = function () {
        var me = this;
        setTimeout(function() { me.open(false, true); }, 100);
    };

    this.open = function (pushState, replaceState) {
        var me = this;
        this._meInOpen = true;
        var dataItem = this.getSelected();
        if (this.options.PluginName && (!dataItem || dataItem.PluginName !== this.options.PluginName)) {
            var dsData = this.grid.dataSource.data();
            for (var i = 0; i < dsData.length; i++) {
                if (dsData[i].PluginName === this.options.PluginName || (dsData[i].Name === this.options.PluginName && dsData[i].PluginName)) {
                    this.grid.clearSelection();
                    this.gridSelectDataItem(dsData[i]);
                    var parent = !dsData[i].parentID ? null : this.grid.dataSource.get(dsData[i].parentID);
                    while (parent && parent.uid) {
                        var tr = this.grid.table.find('tr[data-uid="' + parent.uid + '"]');
                        this.grid.expand(tr);
                        parent = !parent.parentID ? null : this.grid.dataSource.get(parent.parentID);
                    }
                }
            }
        }
        this._meInOpen = false;

        this.ClearParameters();
        $('#reportResultDiv').hide();
        $('#reportResultDiv').html('');

        var data = {
            PluginName: this.options.PluginName,
            idrec: this.options.idrec,
            storageValuesKey: this.options.storageValuesKey,
            setDefaultParams: this.options.setDefaultParams
        };
        if (data.PluginName)
            this.post('GetPluginInfo',
                data,
                function (result) {
                    if (!result.error)
                        me.InitParameters(result);
                },
                false);

        if (!this.options.viewOne) {
            if (pushState && window.history && history.pushState && dataItem.PluginName) {
                history.pushState(this.options.toJSON(), this.options.Name, this.getUrl(this.options));
            }

            if (replaceState && window.history && window.history.replaceState) {
                window.history.replaceState(this.options.toJSON(), this.options.Name, this.getUrl(this.options));
            }
        }
    };

    this.setOptions = function(options) {
        this.options.set('PluginName', options.PluginName);
        this.options.set('PluginType', options.PluginType);
        this.options.set('Name', options.Name);
    };

    this.getUrl = function (data) {
        if (data.PluginName)
            return '/Reports/Manager/R/' + decodeURI(encodeURIComponent(data.PluginName)) + '/';
        return '/Reports/Manager';
    };

    this.GetParameters = function() {
        var parameters = this.parameters.toJSON();
        parameters.forEach(function(p) {
            p.Data = null;
            p.Columns = null;
            p.TemplateValue1 = null;
            p.TemplateValue2 = null;
            p.CheckedFilterConditionTooltip = null;
            if (p.Value1 instanceof Date)
                p.Value1 = p.Value1.toJSON();
            if (p.Value2 instanceof Date)
                p.Value2 = p.Value2.toJSON();
        });
        return parameters;
    };

    this.ExportDocument = function(format, ext) {
        var me = this;
        var data = {
            PluginName: me.options.PluginName,
            culture: me.options.isKz ? 'kz' : 'ru',
            parameters: this.GetParameters(),
            export: format
        };

        $('#downloadForm').remove();

        this.post('ValidateBeforeExport',
            data,
            function(result) {
                if (!result.success)
                    return;

                data.parametersStr = JSON.stringify(data.parameters);
                data.parameters = null;
                var form = document.createElement("form");
                $(form)
                    .attr('id', 'downloadForm')
                    .attr("method", 'POST')
                    .attr("action", "/" + me.controller + "/CreateReport")
                    .hide();

                for (var key in data) {
                    if (data.hasOwnProperty(key)) {
                        var hiddenField = document.createElement("input");
                        hiddenField.setAttribute("type", "hidden");
                        hiddenField.setAttribute("name", key);
                        hiddenField.setAttribute("value", data[key]);

                        form.appendChild(hiddenField);
                    }
                }

                document.body.appendChild(form);
                form.submit();
            },
            false);
    };

    this.onFieldChange = function (e) {
        var me = this;

        if (e.field === 'isKz' || e.field === '*') {
            if (this.options.isKz) {
                $('#kzLang').addClass('k-state-selected');
                $('#ruLang').removeClass('k-state-selected');
            } else {
                $('#kzLang').removeClass('k-state-selected');
                $('#ruLang').addClass('k-state-selected');
            }
        }
    };

    this.onParametersFieldChange = function(e) {
        if (e.field === "Value1" && e.items && e.items[0]) {
            if (e.items[0].AutoPostBack) {
                this.ReloadDataOnChange(e.items[0]);
            } else if (this.options.oneParameter &&
                (!e.items[0].VisibleValue2 || e.items[0].Value2)) {
                this.onCreateClick();
            }
        }
        else if (e.field === "Value2" && e.items && e.items[0]) {
            if (this.options.oneParameter && e.items[0].Value1) {
                this.onCreateClick();
            }
        }
        else if (e.field === 'CheckedFilterConditionValue' && e.items && e.items[0]) {
            this.ReloadItemDataOnChange(e.items[0]);
        }
    };

    this.ClearParameters = function() {
        $('#reportParameters').html('');
        this.options.set('showSubscription', false);
        this.options.set('showSaveSubscription', false);
        this.options.set('showButtons', false);
        this.options.set('showCRButtons', false);
        $('#reportParameters').hide();
    };

    this.InitParametersTemplates = function() {
        this.templates = {
        };

        this.templates.DateTime = kendo.template($('#parametersDateTimeTemplate').html());
        this.templates.Int16 = kendo.template($('#parametersInt16Template').html());
        this.templates.Int32 = kendo.template($('#parametersInt32Template').html());
        this.templates.Int64 = kendo.template($('#parametersInt64Template').html());
        this.templates.Boolean = kendo.template($('#parametersBooleanTemplate').html());
        this.templates.String = kendo.template($('#parametersStringTemplate').html());
        this.templates.CFC = kendo.template($('#parametersCFCTemplate').html());
        this.templates.DDL = kendo.template($('#parametersDDLTemplate').html());
        this.templates.DDL2 = kendo.template($('#parametersDDL2Template').html());
        this.templates.Undefined = kendo.template($('#parametersUndefinedTemplate').html());
        this.templates.MultiColumn = kendo.template($('#parametersMultiColumnTemplate').html());
        this.templates.Grid = kendo.template($('#parametersGridTemplate').html());
        this.templates.Tree = kendo.template($('#parametersTreeTemplate').html());
        this.templates.Value1 = kendo.template($('#parametersValue1Template').html());
        this.templates.Value2 = kendo.template($('#parametersValue2Template').html());
        this.InitParametersTemplates = function() {};
    };

    this.InitParameters = function (data) {
        var me = this;
        this.parameters = kendo.observableHierarchy(data);
        this.parameters.bind('change', function (e) { me.onParametersFieldChange(e); });
        data = this.parameters;
        var params = $('#reportParameters');
        this.InitParametersTemplates();
        params.html('');
        var countParameters = 0;
        var showAddParameters = false;
        for (var i = 0; i < data.length; i++) {
            var item = data[i];
            item.ParameterIndex = i;

            if (!item.Visible)
                continue;

            if (this.InitParameterItem(params, item)) {
                countParameters++;
                if (item.FilterType === 65536) countParameters += 2;
                if (item.AllowAddParameter) {
                    showAddParameters = true;
                    countParameters += 2;
                }
            }
        }

        if (countParameters === 0) {
            this.onCreateClick();
            params.hide();
        } else {
            params.append('<div id="ClearParameters" style="clear: both;"/>');
            params.show();
        }

        this.options.set('oneParameter', countParameters === 1);
        if (this.options.PluginType === 'CrossReport')
            this.options.set('showCRButtons', true);
        else {
            this.options.set('showButtons', true);
            this.options.set('showSubscription', true);
        }

        if (this.options.idSubscription) {
            this.options.set('showSubscription', false);
            this.options.set('showSaveSubscription', true);
        }

        this.options.set('showAddParameters', showAddParameters);

        if (this.options.viewOneOpen) {
            this.onCreateClick();
        }
    };

    this.InitParameterItem = function(paramsDiv, item) {
        var html;
        var func = null;
        if (item.TemplateValue1 && item.TemplateValue2) {
            html = $(this.templates.Value2(item));
            html.find('.m-editor-value1').html(kendo.template(item.TemplateValue1)(item));
            html.find('.m-editor-value2').html(kendo.template(item.TemplateValue2)(item));
        }
        else if (item.TemplateValue1) {
            html = $(this.templates.Value1(item));
            html.find('.m-editor-value').html(kendo.template(item.TemplateValue1)(item));
            $('#DynamicValue1').attr('id', 'Value1_' + item.ParameterIndex);
        }
        else if (item.Data) {
            this.DDLInit(item);
            if (item.CheckedFilterCondition) {
                html = $(this.templates.CFC(item));
                paramsDiv.append(html);
                kendo.bind(html, item);
            }
            // если есть тип фильтра Between
            if (item.FilterTypes.filter(function(f) { return f.id === 256; }).length > 0) {
                html = $(this.templates.DDL2(item));
            } else {
                html = $(this.templates.DDL(item));
            }
        } else if (item.Columns && item.FilterType === 65536) {
            if (item.ParentField) {
                this.TreeInit(item);
                html = $(this.templates.Tree(item));
                func = this.TreeAfterInit;
            } else {
                this.GridInit(item);
                html = $(this.templates.Grid(item));
                func = this.GridAfterInit;
            }
        } else if (item.Columns) {
            this.MultiColumnInit(item);
            html = $(this.templates.MultiColumn(item));
        } else if (this.templates[item.DataType]) {
            html = $(this.templates[item.DataType](item));
        } else
            html = $(this.templates.Undefined(item));

        if (html) {
            paramsDiv.append(html);

            var dv = $('#DynamicValue1');
            if (dv.length) {
                var dataBind1 = dv.attr('data-bind');
                dv.attr('data-bind', dataBind1.replace(/DynamicValue/gi, 'Value1'))
                    .attr('name', 'Value1_' + item.ParameterIndex)
                    .attr('id', 'Value1_' + item.ParameterIndex);
                $('#DynamicValue1_link').attr('id', 'Value1_' + item.ParameterIndex + '_link')
                    .attr('clientId', 'Value1_' + item.ParameterIndex);
            }

            dv = $('#DynamicValue2');
            if (dv.length) {
                var dataBind2 = dv.attr('data-bind');
                dv.attr('data-bind', dataBind2.replace(/DynamicValue/gi, 'Value2'))
                    .attr('name', 'Value2_' + item.ParameterIndex)
                    .attr('id', 'Value2_' + item.ParameterIndex);
                $('#DynamicValue2_link').attr('id', 'Value2_' + item.ParameterIndex + '_link')
                    .attr('clientId', 'Value2_' + item.ParameterIndex);
            }

            VM.bindModel = {
                value: item.Value1,
                keyField: item.ValueColumn
            };

            kendo.bind(html, item);

            VM.bindModel = null;

            this.FilterTypesInit(item);
            if (func) func(item);

            return true;
        }

        return false;
    };

    this.FilterTypesInit = function(item) {
        if (item.FilterTypes.length <= 1) {
            var ddl = $('#FilterType_' + item.ParameterIndex).data('kendoDropDownList');
            ddl.readonly();
            ddl.span.addClass('m-field-readOnly');
        } else
            item.bind('change', this.onChangeParameter);
        this.onChangeParameter.call(item, { field: 'FilterType' });
    };

    this.ReloadDataOnChange = function (startItem) {
        for (var i = 0; i < this.parameters.length; i++) {
            var item = this.parameters[i];
            if (item.RequireReload && item !== startItem) {
                ReloadItemDataOnChange(item);
            }
        }
    };

    this.ReloadItemDataOnChange = function(item) {
        var d = $('#Value1_' + item.ParameterIndex).data();
        if (d.kendoDropDownList)
            d.kendoDropDownList.dataSource.read();
        if (d.kendoMultiColumnComboBox)
            d.kendoMultiColumnComboBox.dataSource.read();
        if (d.kendoGrid) {
            d.kendoGrid.clearSelection();
            d.kendoGrid.dataSource.read();
        }
        if (d.kendoTreeList)
            d.kendoTreeList.dataSource.read();
    };

    this.MultiColumnInit = function (item) {
        item.Data = new kendo.data.DataSource({
            type: 'aspnetmvc-ajax',
            serverFiltering: true,
            serverPaging: true,
            pageSize: 80,
            transport: {
                read: {
                    type: 'POST',
                    url: "/Reports/Manager/GetConditionData",
                    data: function (e) {

                        if (window.VM && window.VM.bindModel && window.VM.bindModel.value && window.VM.bindModel.keyField) {
                            e.filter = { field: window.VM.bindModel.keyField, value: window.VM.bindModel.value, operator: 'eq' };
                        }

                        return {
                            PluginName: VM.manager.options.PluginName,
                            Key: item.Key,
                            parameters: VM.manager.GetParameters()
                        };
                    }
                }
            }
        });
        /*var addItem = {};
            addItem[item.ValueColumn] = item.Value1;
            addItem[item.DisplayColumn] = item.Value2;
            combobox.dataSource.add(addItem);*/
        /*
        var combobox = $('#Value1_' + item.ParameterIndex).data('kendoMultiColumnComboBox');
        if (!combobox)
            return;

        if (item.Value1) {
            
        }
        var ds = ;
        combobox.setDataSource(ds);*/
    };

    this.DDLInit = function(item) {
        //item.RequireReload = false;
        if (!item.RequireReload)
            return;

        item.Data = new kendo.data.DataSource({
            type: 'aspnetmvc-ajax',
            serverFiltering: true,
            serverPaging: true,
            pageSize: 80,
            transport: {
                read: {
                    type: 'POST',
                    url: "/Reports/Manager/GetConditionData",
                    data: function () {
                        return {
                            PluginName: VM.manager.options.PluginName,
                            Key: item.Key,
                            parameters: VM.manager.GetParameters()
                        };
                    }
                }
            }
        });
    };

    this.GridInit = function(item) {
        item.set('Data', new kendo.data.DataSource({
            type: 'aspnetmvc-ajax',
            serverFiltering: true,
            pageSize: 25,
            schema: {
                model: {
                    id: item.ValueColumn
                }
            },
            transport: {
                read: {
                    type: 'POST',
                    url: "/Reports/Manager/GetConditionData",
                    data: function () {
                        return {
                            PluginName: VM.manager.options.PluginName,
                            Key: item.Key,
                            parameters: VM.manager.GetParameters()
                        };
                    }
                }
            }
        }));
        item.Columns.splice(0, 0, { selectable: true, width: "50px" });
    };

    this.GridAfterInit = function (item) {
        var grid = $('#Value1_' + item.ParameterIndex).data('kendoGrid');
        if (!grid) return;
        grid.setOptions({ persistSelection: true });
        grid.bind('change',
            function() {
                item.set('Values', grid.selectedKeyNames());
            });
    };

    this.TreeInit = function (item) {
        var fields = {};
        if (item.DataType === "String") {
            fields[item.ValueColumn] = { nullable: false, type: 'string' };
            fields[item.ParentField] = { nullable: true, type: 'string' };
        }
        else {
            fields[item.ValueColumn] = { nullable: false, type: 'number' };
            fields[item.ParentField] = { nullable: true, type: 'number' };
        }
        item.set('Data', new kendo.data.TreeListDataSource({
            type: 'aspnetmvc-ajax',
            //serverFiltering: true,
            //pageSize: 25,
            schema: {
                model: {
                    id: item.ValueColumn,
                    parentId: item.ParentField,
                    expanded: false,
                    fields: fields
                }
            },
            transport: {
                read: {
                    type: 'POST',
                    url: "/Reports/Manager/GetConditionData",
                    data: function () {
                        return {
                            PluginName: VM.manager.options.PluginName,
                            Key: item.Key,
                            parameters: VM.manager.GetParameters()
                        };
                    }
                }
            },
            requestEnd: function(e) {
                if (e.type !== 'read')
                    return;

                var keys = {};
                for (var i = 0; i < e.response.length; i++) {
                    keys[e.response[i][item.ValueColumn]] = true;
                }

                for (var i = 0; i < e.response.length; i++) {
                    var row = e.response[i];
                    if (row[item.ParentField] === '')
                        row[item.ParentField] = null;
                    else if (!keys[row[item.ParentField]]) {
                        row[item.ParentField] = null;
                    }
                }

                setTimeout(function() {
                    for (var j = 0; j < item.Values.length; j++) {
                        var checkBox = $('#checked_' + item.ParameterIndex + '_' + VM.manager.FixId(item.Values[j], item.ParameterIndex))[0];
                            if (checkBox) checkBox.checked = true;
                        }
                    },
                    100);
            }
        }));
        item.Columns.splice(0,
            0,
            {
                headerTemplate: '<input type="checkbox" onclick="VM.manager.treeListToggleAll(event, this, ' + item.ParameterIndex + ')" />',
                template: '<input type="checkbox" idValue="#=' + item.ValueColumn + '#" id="checked_' + item.ParameterIndex + '_#= VM.manager.FixId(' + item.ValueColumn + ', ' + item.ParameterIndex + ') #" onclick="VM.manager.treeListToggleItem(event, this, ' + item.ParameterIndex + ')" />',
                width: 150
            });
    };

    this.FixId = function (id, parameterIndex) {
        if (this.parameters[parameterIndex].DataType === "String")
            return id.toString().replace(/\.|,/g, '_');
        return id;
    };

    this.TreeAfterInit = function (item) {
        var grid = $('#Value1_' + item.ParameterIndex).data('kendoTreeList');
        if (!grid) return;
        //grid.setOptions({ persistSelection: true });
        /*grid.bind('change',
            function() {
                item.set('Values', grid.selectedKeyNames());
            });*/
    };

    this.treeListToggleAll = function(e, checkBox, parameterIndex) {
        var item = this.parameters[parameterIndex];
        var tree = $('#Value1_' + item.ParameterIndex).data('kendoTreeList');
        item.Values.splice(0, item.Values.length);
        var data = tree.dataSource.data();
        for (var i = 0; i < data.length; i++) {
            var id = data[i][item.ValueColumn];
            if (checkBox.checked) {
                item.Values.push(id);
                data[i].checked = true;
            }
            else
                data[i].checked = false;
            var checkBoxItem = $('#checked_' + item.ParameterIndex + '_' + VM.manager.FixId(id, item.ParameterIndex))[0];
            if (checkBoxItem) checkBoxItem.checked = data[i].checked;
        }
    };

    this.treeListToggleItem = function (e, checkBox, parameterIndex) {
        var item = this.parameters[parameterIndex];
        var id = $(checkBox).attr('idValue');
        if (this.parameters[parameterIndex].DataType === "Int64" || this.parameters[parameterIndex].DataType === "Int32" || this.parameters[parameterIndex].DataType === "Int16")
            id = parseInt(id);

        var index = item.Values.indexOf(id);
        if (checkBox.checked && index === -1)
            item.Values.push(id);
        else if (!checkBox.checked && index > -1)
            item.Values.splice(index, 1);

        var tree = $('#Value1_' + item.ParameterIndex).data('kendoTreeList');
        this.treeListToggleChildNodes(item, tree.dataSource.childNodes({ id: id }), checkBox.checked, tree);
    };

    this.treeListToggleChildNodes = function (item, childNodes, checked, tree) {
        for (var i = 0; i < childNodes.length; i++) {
            var id = childNodes[i][item.ValueColumn];
            var index = item.Values.indexOf(id);
            if (checked && index === -1)
                item.Values.push(id);
            else if (!checked && index > -1)
                item.Values.splice(index, 1);
            childNodes[i].checked = checked;
            var checkBoxItem = $('#checked_' + item.ParameterIndex + '_' + VM.manager.FixId(id, item.ParameterIndex))[0];
            if (checkBoxItem) checkBoxItem.checked = checked;

            this.treeListToggleChildNodes(item, tree.dataSource.childNodes({ id: id }), checked, tree);
        }
    };

    this.onChangeParameter = function(e) {
        if (e.field === 'FilterType') {
            var fType = null;
            for (var i = 0; i < this.FilterTypes.length; i++) {
                if (this.FilterTypes[i].id === this.FilterType)
                    fType = this.FilterTypes[i];
            }

            if (!fType) {
                this.set('VisibleValue1', false);
                this.set('VisibleValue2', false);
            }
            else {
                this.set('VisibleValue1', fType.VisibleValue1);
                this.set('VisibleValue2', fType.VisibleValue2);
            }

            if (!this.VisibleValue1 && !this.VisibleValue2)
                $('#Label_' + this.ParameterIndex).attr('for', 'FilterType_' + this.ParameterIndex);
            else
                $('#Label_' + this.ParameterIndex).attr('for', 'Value1_' + this.ParameterIndex);
        }
    };

    this.progress = function (value, skipGrid) {

        var element = $('#managerSplitter');

        if (this.dataSourceProgress !== value) {
            kendo.ui.progress(element, value);
        }

        this.dataSourceProgress = value;
    };
};
