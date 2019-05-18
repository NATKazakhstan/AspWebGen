Nat.Reports.Manager = function (options) {

    Nat.Classes.SimpleGridController.call(this, 'Manager');

    this.options = kendo.observable(options);

    this.Initialize = function () {
        var me = this;

        this.grid = $('#pluginsTree').data('kendoTreeList');

        var searchDiv = kendo.template($('#pluginsSearch').html())({ divStyle: '', id: 'searchValueInput' });
        this.grid.toolbar.find('button[data-command="search"]').replaceWith(searchDiv);

        kendo.bind($('#searchValueInput'), this.options);

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
            var data = {
                PluginName: this.options.PluginName
            };
            if (data.PluginName)
                this.post('GetPluginInfo',
                    data,
                    function (result) {
                        if (!result.error)
                            me.InitParameters(result);
                        else
                            me.ClearParameters();
                    },
                    false);
            else {
                me.ClearParameters();
            }
        }
    };

    this.ClearParameters = function() {
        $('#reportParameters').html('');
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
                
            }
        }
    };

    this.FilterTypesInit = function(item) {
        if (item.FilterTypes.length <= 1) {
            var ddl = $('#' + item.Name + 'FilterType').data('kendoDropDownList');
            ddl.readonly();
            ddl.span.addClass('m-field-readOnly');
        } else
            item.bind('change', this.onChangeParameter);
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
        }
    };
};
