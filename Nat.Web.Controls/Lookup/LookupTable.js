/*
 * Created by : Daniil Kovalev
 * Created    : 28.11.2007
 */

Type.registerNamespace("Nat.Web.Controls.LookupTable");

Nat.Web.Controls.LookupTable = function (element)
{
    Nat.Web.Controls.LookupTable.initializeBase(this, [element]);
    
    this._initializeRequest = null;
    this._endRequest = null;
    this._onLoadHandler = null;
    this._hiddenFieldID = null;
    this._onItemSelectedHandler = null;
    this._rowChangedHandler = null;
    
    this._currentSelectedRowIndex = null;
    this._dataKeys = null;
    this.selectedDataKeys = null;
    this._selectedRowCssClass = null;
    this._rowCssClass = null;
    this._alternatingRowCssClass = null;
    this._currentSelectedRowClassName = null;
}

Nat.Web.Controls.LookupTable.prototype = {
    
    // Constructor
    
    initialize : function() {
        this._initializeRequest = Function.createDelegate(this, this.initializeRequest);
        var prm = Sys.WebForms.PageRequestManager.getInstance();
        prm.add_initializeRequest(this._initializeRequest);

        this._endRequest = Function.createDelegate(this, this.endRequest);
        prm.add_endRequest(this._endRequest);
        
        this._onLoadHandler = Function.createDelegate(this, this._onLoad);
        Sys.Application.add_load(this._onLoadHandler);
        
        this._onUnloadHandler = Function.createDelegate(this, this._onUnload);
        Sys.Application.add_unload(this._onUnloadHandler);
        
        //this._onItemSelectedHandler = Function.createDelegate(this, this._onItemSelected);
        
        this._onCollapseClickHandler = Function.createDelegate(this, this._onCollapseClick);
        var element = $get(this._collapseImageID);
        $addHandler(element, "click", this._onCollapseClickHandler);
        
    },
    
    // Destructor
    
    dispose : function() {
        var prm = Sys.WebForms.PageRequestManager.getInstance();
        
        if (this._initializeRequest) {
            prm.remove_initializeRequest(this._initializeRequest);
            this._initializeRequest = null;
        }

        if (this._endRequest) {
            prm.remove_endRequest(this._endRequest);
            this._endRequest = null;
        }

        if (this._onLoadHandler) {
            Sys.Application.remove_load(this._onLoadHandler);
            this._onLoadHandler = null;
        }
        
        if (this._onUnloadHandler) {
            Sys.Application.remove_load(this._onUnloadHandler);
            this._onUnloadHandler = null;
        }
        
        if (this._onCollapseClickHandler) {
            var element = $get(this._collapseImageID);
            $removeHandler(element, "click", this._onCollapseClickHandler);
        }
        
        Nat.Web.Controls.LookupTable.callBaseMethod(this, 'dispose');
    },
    
    // Properties
    
    get_gridViewBehaviorID : function() {
        return this._gridViewBehaviorID;
    },
        
    set_gridViewBehaviorID : function(value) {
        if (this._gridViewBehaviorID != value)
            this._gridViewBehaviorID = value;
    },
    
    get_collapseImageID : function() {
        return this._collapseImageID;
    },
        
    set_collapseImageID : function(value) {
        if (this._collapseImageID != value)
            this._collapseImageID = value;
    },
    
    get_applyButtonID : function() {
        return this._applyButtonID;
    },
        
    set_applyButtonID : function(value) {
        if (this._applyButtonID != value)
            this._applyButtonID = value;
    },
    
    get_gridViewID : function() {
        return this._gridViewID;
    },
        
    set_gridViewID : function(value) {
        if (this._gridViewID != value)
            this._gridViewID = value;
    },
    
    get_hiddenFieldID : function() {
        return this._hiddenFieldID;
    },
        
    set_hiddenFieldID : function(value) {
        if (this._hiddenFieldID != value)
            this._hiddenFieldID = value;
    },
    
    // Methods    
    
    _onLoad : function() {
        var element = $find(this._gridViewBehaviorID);
        if (element != null)
        {
            if (this._rowChangedHandler != null)
                element.remove_rowChanged(this._rowChangedHandler);
            this._rowChangedHandler = Function.createDelegate(this, this._rowChanged);
            element.add_rowChanged(this._rowChangedHandler);
            
            var _hiddenField = $get(this._hiddenFieldID);
            if (_hiddenField.value != "") {

                var index = parseInt(_hiddenField.value);
                if (index != -1) {
                    element._currentSelectedRowIndex = index + 1;
                }
            }
        }        
    },
    
    _onUnload : function() {
        var element = $find(this._gridViewBehaviorID);
        if (element != null)
        {
            element.remove_rowChanged(this._rowChangedHandler);
        }
    },
    
    _rowChanged : function(source, args){
    
        var element = $find(this._gridViewBehaviorID);
        this.selectedDataKeys = element.selectedDataKeys;
        
        var _hiddenField = $get(this._hiddenFieldID);
        _hiddenField.value = element.selectedIndex - 1;
        
        this.raiseRowChanged(args);
    },
    
    applyFilter : function(){
        var element = $find(this._gridViewBehaviorID);
        if (element != null)
            element._currentSelectedRowIndex = null;
            
        var _hiddenField = $get(this._hiddenFieldID);
        if (_hiddenField != null)
            _hiddenField.value = "";

        this.selectedDataKeys = null;
           
        var element = $get(this._applyButtonID);
        if (element != null)
            element.click();
    },
    
    _onCollapseClick : function(evt) {
        this.raiseCollapse();
    },
    
    add_collapse : function(handler) {
        this.get_events().addHandler("collapse", handler);
    },
    
    remove_collapse : function(handler) {
        this.get_events().removeHandler("collapse", handler);
    },
    
    raiseCollapse : function() {
        var handler = this.get_events().getHandler('collapse');
        if (handler) {
            handler(this,  Sys.EventArgs.Empty);
        }
    },    
    
    add_rowChanged : function(handler) {
        this.get_events().addHandler("rowChanged", handler);
    },
    
    remove_rowChanged : function(handler) {
        this.get_events().removeHandler("rowChanged", handler);
    },
    
    raiseRowChanged : function(args) {
        var handler = this.get_events().getHandler('rowChanged');
        if (handler) {
            handler(this,  args);
        }
    },    
    
    initializeRequest : function(sender, args)
    {
    },

    endRequest : function(sender, args)
    {
    }
}

Nat.Web.Controls.LookupTable.registerClass('Nat.Web.Controls.LookupTable', Sys.UI.Control);

