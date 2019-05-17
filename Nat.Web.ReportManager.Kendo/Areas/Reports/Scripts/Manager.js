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
        this.checkLanguage();
    };

    this.onGetData = function() {
        return {
            searchValue: this.options.searchValue
        };
    };

    this.onSelectionChange = function (e) {
        var dataItem = this.getSelected();
        if (dataItem && !dataItem.PluginName) {
            if (dataItem.expanded)
                this.grid.collapse(this.grid.select());
            else
                this.grid.expand(this.grid.select());
        }
    };

    this.onLangClick = function(isKz) {
        this.options.set('isKz', isKz);
        this.checkLanguage();
    };

    this.checkLanguage = function() {
        if (this.options.isKz) {
            $('#kzLang').addClass('k-state-selected');
            $('#ruLang').removeClass('k-state-selected');
        } else {
            $('#kzLang').removeClass('k-state-selected');
            $('#ruLang').addClass('k-state-selected');
        }
    };
};
