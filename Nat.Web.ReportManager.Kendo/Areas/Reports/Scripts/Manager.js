Nat.Reports.Manager = function (options) {

    Nat.Classes.SimpleGridController.call(this, 'Manager');

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
    };

    this.onLangClick = function(isKz) {
        this.options.set('isKz', isKz);
    };

    this.onCreateClick = function () {
        var data = {
            PluginName: this.options.PluginName,
            culture: this.options.isKz ? 'kz' : 'ru',
            parameters: this.parameters.toJSON()
        };
        data.parameters.forEach(function(p) { p.Data = null; });
        this.post('CreateReport',
            data,
            function(result) {
                if (result.error)
                    return;

                //$('#reportResultDiv').html('<iframe width="100%" height="500px" style="border: 0px">' + result.ReportContent + '</iframe>');
                $('#reportResultDiv').html($(result.ReportContent).filter(function(t) {
                    return this.tagName === 'TABLE' || this.tagName === 'STYLE';
                }));
                $('#reportResultDiv').show();
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

    this.ExportDocument = function (format, ext) {
        //var me = this;
        var data = {
            PluginName: this.options.PluginName,
            culture: this.options.isKz ? 'kz' : 'ru',
            parameters: this.parameters.toJSON(),
            export: format
        };
        data.parameters.forEach(function (p) { p.Data = null; });
        data.parametersStr = JSON.stringify(data.parameters);
        data.parameters = undefined;

        $('#downloadForm').remove();
        var form = document.createElement("form");
        $(form)
            .attr('id', 'downloadForm')
            .attr("method", 'POST')
            .attr("action", "/" + this.controller + "/CreateReport")
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

    this.ClearParameters = function() {
        $('#reportParameters').html('');
        this.options.set('showButtons', false);
        $('#reportParameters').hide();
    };

    this.InitParametersTemplates = function() {
        this.templates = {
        };

        this.templates.DateTime = kendo.template($('#parametersDateTimeTemplate').html());
        this.templates.DDL = kendo.template($('#parametersDDLTemplate').html());
        this.templates.Undefined = kendo.template($('#parametersUndefinedTemplate').html());
        this.templates.MultiColumn = kendo.template($('#parametersMultiColumnTemplate').html());
        this.InitParametersTemplates = function() {};
    };

    this.InitParameters = function (data) {
        this.parameters = kendo.observableHierarchy(data);
        data = this.parameters;
        var params = $('#reportParameters');
        this.InitParametersTemplates();
        params.html('');
        var notExistsParameters = true;
        for (var i = 0; i < data.length; i++) {
            var item = data[i];
            if (!item.Visible)
                continue;

            var html = '';
            var func = null;
            if (item.Data) {
                html = $(this.templates.DDL(item));
            }
            else if (item.Columns) {
                html = $(this.templates.MultiColumn(item));
                func = this.MultiColumnInit;
            }
            else if (this.templates[item.DataType]) {
                html = $(this.templates[item.DataType](item));
            }
            else
                html = $(this.templates.Undefined(item));

            if (html) {
                params.append(html);
                kendo.bind(html, item);
                if (func) func(item);
                this.FilterTypesInit(item);
                notExistsParameters = false;
            }
        }

        if (notExistsParameters) {
            this.onCreateClick();
            params.hide();
        } else {
            params.append('<div style="clear: both;"/>');
            params.show();
        }

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

    this.MultiColumnInit = function(item) {
        var combobox = $('#' + item.Name + 'Value1').data('kendoMultiColumnComboBox');
        if (!combobox)
            return;

        if (item.Value1) {
            /*var addItem = {};
            addItem[item.ValueColumn] = item.Value1;
            addItem[item.DisplayColumn] = item.Value2;
            combobox.dataSource.add(addItem);*/
        }
        var ds = new kendo.data.DataSource({
            type: 'aspnetmvc-ajax',
            serverFiltering: true,
            transport: {
                read: {
                    type: 'POST',
                    url: "/Reports/Manager/GetConditionData",
                    data: {
                        PluginName: VM.manager.options.PluginName,
                        Key: item.Key
                    }
                }
            }
        });
        combobox.setDataSource(ds);
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
