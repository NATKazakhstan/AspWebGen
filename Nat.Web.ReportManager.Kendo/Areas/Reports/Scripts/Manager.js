Nat.Reports.Manager = function (options) {

    Nat.Classes.SimpleGridController.call(this, 'Reports/Manager');

    this.options = kendo.observable(options);

    this.Initialize = function () {
        var me = this;

        this.grid = $('#pluginsTree').data('kendoTreeList');
        this.reportWindow = $('#reportWindow').data('kendoWindow');

        var searchDiv = kendo.template($('#pluginsSearch').html())({ divStyle: '', id: 'searchValueInput' });
        this.grid.toolbar.find('button[data-command="search"]').replaceWith(searchDiv);

        kendo.bind($('#searchValueInput'), this.options);
        kendo.bind($('.reportToolbar'), this.options);

        this.bindSelectionChange();
        this.bindProgress();
        this.bindGetData();

        this.options.bind('change', function (e) { me.onFieldChange(e); });
        this.onFieldChange({ field: '*' });
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
                        me.reportWindow.setOptions({ width: w.width() * 0.95, height: w.height() * 0.9, title: dataItem.Name });
                        me.reportWindow.content('<h6>Загрузка... <br/> Енгізу...</h6>');
                        me.reportWindow.refresh({ url: result.Url.replace('/MainPage.aspx/', '/EmptyPage.aspx/'), iframe: true });
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
                if (anyText)
                    $('#reportResultDiv').show();
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

    this.GetParameters = function() {
        var parameters = this.parameters.toJSON();
        parameters.forEach(function(p) {
            p.Data = null;
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

        if (e.field === 'PluginName') {
            this.ClearParameters();
            $('#reportResultDiv').hide();
            $('#reportResultDiv').html('');

            var data = {
                PluginName: this.options.PluginName
            };
            if (data.PluginName)
                this.post('GetPluginInfo',
                    data,
                    function (result) {
                        if (!result.error)
                            me.InitParameters(result);
                    },
                    false);
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
    };

    this.ClearParameters = function() {
        $('#reportParameters').html('');
        this.options.set('showButtons', false);
        this.options.set('showCRButtons', false);
        $('#reportParameters').hide();
    };

    this.InitParametersTemplates = function() {
        this.templates = {
        };

        this.templates.DateTime = kendo.template($('#parametersDateTimeTemplate').html());
        this.templates.Int32 = kendo.template($('#parametersInt32Template').html());
        this.templates.Int64 = kendo.template($('#parametersInt64Template').html());
        this.templates.Boolean = kendo.template($('#parametersBooleanTemplate').html());
        this.templates.String = kendo.template($('#parametersStringTemplate').html());
        this.templates.DDL = kendo.template($('#parametersDDLTemplate').html());
        this.templates.Undefined = kendo.template($('#parametersUndefinedTemplate').html());
        this.templates.MultiColumn = kendo.template($('#parametersMultiColumnTemplate').html());
        this.templates.Grid = kendo.template($('#parametersGridTemplate').html());
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
        for (var i = 0; i < data.length; i++) {
            var item = data[i];
            if (!item.Visible)
                continue;

            var html = '';
            var func = null;
            if (item.Data) {
                this.DDLInit(item);
                html = $(this.templates.DDL(item));
            }
            else if (item.Columns && item.FilterType === 65536) {
                this.GridInit(item);
                html = $(this.templates.Grid(item));
                func = this.GridAfterInit;
            }
            else if (item.Columns) {
                this.MultiColumnInit(item);
                html = $(this.templates.MultiColumn(item));
            }
            else if (this.templates[item.DataType]) {
                html = $(this.templates[item.DataType](item));
            }
            else
                html = $(this.templates.Undefined(item));

            if (html) {
                params.append(html);
                kendo.bind(html, item);
                this.FilterTypesInit(item);
                if (func) func(item);
                countParameters++;
                if (item.FilterType === 65536) countParameters += 2;
            }
        }

        if (countParameters === 0) {
            this.onCreateClick();
            params.hide();
        } else {
            params.append('<div style="clear: both;"/>');
            params.show();
        }

        this.options.set('oneParameter', countParameters === 1);
        if (this.options.PluginType === 'CrossReport')
            this.options.set('showCRButtons', true);
        else
            this.options.set('showButtons', true);
    };

    this.FilterTypesInit = function(item) {
        if (item.FilterTypes.length <= 1) {
            var ddl = $('#' + item.Name + 'FilterType').data('kendoDropDownList');
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
                var d = $('#' + item.Name + 'Value1').data();
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
            }
        }
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
                    data: function() {
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
        var combobox = $('#' + item.Name + 'Value1').data('kendoMultiColumnComboBox');
        if (!combobox)
            return;

        if (item.Value1) {
            
        }
        var ds = ;
        combobox.setDataSource(ds);*/
    };

    this.DDLInit = function(item) {
        item.RequireReload = false;
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
        var grid = $('#' + item.Name + "Value1").data('kendoGrid');
        if (!grid) return;
        grid.setOptions({ persistSelection: true });
        grid.bind('change',
            function() {
                item.set('Values', grid.selectedKeyNames());
            });
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
                $('#' + this.Name + 'Label').attr('for', this.Name + 'FilterType');
            else
                $('#' + this.Name + 'Label').attr('for', this.Name + 'Value1');
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
