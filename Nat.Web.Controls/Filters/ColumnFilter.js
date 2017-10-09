/*
 * Created by : Daniil Kovalev
 * Created    : 28.11.2007
 */

Type.registerNamespace('Nat.Web.Controls');

Nat.Web.Controls.FilterCondition = function() {
    throw Error.invalidOperation();
}
Nat.Web.Controls.FilterCondition.prototype = {
    NotSet:     1,
    None:       2,
    Between:    256,
    OutOf:      512,
    IsNull:     2048,
    NotNull:    4096
}
Nat.Web.Controls.FilterCondition.registerEnum("Nat.Web.Controls.FilterCondition", false);

Type.registerNamespace("Nat.Web.Controls.ColumnFilter");

Nat.Web.Controls.ColumnFilter = function (element)
{
    Nat.Web.Controls.ColumnFilter.initializeBase(this, [element]);
    
    this._initializeRequest = null;
    this._onLoadHandler = null;
    this._onUnloadHandler = null;
    
    this._hiddenFieldID = null;
    this._onItemChangedHandler = null;
}

Nat.Web.Controls.ColumnFilter.prototype = {

    // Constructor
    initialize: function() {
        this._initializeRequest = Function.createDelegate(this, this.initializeRequest);
        var prm = Sys.WebForms.PageRequestManager.getInstance();
        prm.add_initializeRequest(this._initializeRequest);

        this._onLoadHandler = Function.createDelegate(this, this._onLoad);
        Sys.Application.add_load(this._onLoadHandler);

        this._onUnloadHandler = Function.createDelegate(this, this._onUnload);
        Sys.Application.add_unload(this._onUnloadHandler);

        this._onItemChangedHandler = Function.createDelegate(this, this._onItemChanged);

        var element = $get(this._dropDownListID);
        $addHandler(element, "change", this._onItemChangedHandler);
        this.updateControls(element.value);
        $(element).attr('fireChangeWhenSetClear', 'true');
    },

    // Destructor
    dispose: function() {
        if (this._initializeRequest) {
            var prm = Sys.WebForms.PageRequestManager.getInstance();
            prm.remove_initializeRequest(this._initializeRequest);
            this._initializeRequest = null;
        }

        if (this._onLoadHandler) {
            Sys.Application.remove_load(this._onLoadHandler);
            this._onLoadHandler = null;
        }

        if (this._onUnloadHandler) {
            Sys.Application.remove_load(this._onUnloadHandler);
            this._onUnloadHandler = null;
        }

        Nat.Web.Controls.ColumnFilter.callBaseMethod(this, 'dispose');
    },

    // Properties
    get_dropDownListID: function() {
        return this._dropDownListID;
    },

    set_dropDownListID: function(value) {
        if (this._dropDownListID != value)
            this._dropDownListID = value;
    },

    get_cell0ID: function() {
        return this._cell0ID;
    },

    set_cell0ID: function(value) {
        if (this._cell0ID != value)
            this._cell0ID = value;
    },

    get_cell1ID: function() {
        return this._cell1ID;
    },

    set_cell1ID: function(value) {
        if (this._cell1ID != value)
            this._cell1ID = value;
    },

    // Methods
    updateControls: function(condition) {
        var cell0 = $get(this._cell0ID);
        var cell1 = this._cell1ID == null ? null : $get(this._cell1ID);

        if (condition == Nat.Web.Controls.FilterCondition.None
            || condition == Nat.Web.Controls.FilterCondition.IsNull
            || condition == Nat.Web.Controls.FilterCondition.NotNull
            || condition == Nat.Web.Controls.FilterCondition.NotSet) {
            cell0.style.display = 'none';
            if (cell1 != null)
                cell1.style.display = 'none';
        } else if (condition == Nat.Web.Controls.FilterCondition.Between ||
            condition == Nat.Web.Controls.FilterCondition.OutOf) {
            cell0.style.display = '';
            if (cell1 != null) {
                cell1.style.display = '';
                cell0.style.width = cell1.style.width;
            }
        } else {
            cell0.style.display = '';
            if (cell1 != null) {
                cell1.style.display = 'none';
                if (cell0.style.width != cell1.style.width)
                    cell0.style.width = this.getStyleWidth(cell1.style.width) * 2 + 9;
            }
        }
    },

    getStyleWidth: function(styleWidth) {
        if (styleWidth == "")
            return 0;
        else
            return styleWidth.substring(0, styleWidth.length - 2);
    },

    _onItemChanged: function(evt) {
        this.updateControls(evt.target.value);
    },

    _onLoad: function(evt) {
    },

    _onUnload: function(evt) {
    },

    initializeRequest: function(sender, args) {
    }
}

Nat.Web.Controls.ColumnFilter.registerClass('Nat.Web.Controls.ColumnFilter', Sys.UI.Control);

