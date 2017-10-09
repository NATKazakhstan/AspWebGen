Type.registerNamespace("Nat.Web.Controls");
Nat.Web.Controls.CopyValue = function(element) {
    Nat.Web.Controls.CopyValue.initializeBase(this, [element]);
    this._kzId = null;
    this._onChangeHandler = null;
    this._changeValueIfNull = true;
}
Nat.Web.Controls.CopyValue.prototype = {
    get_kzId : function() {
        return this._kzId;
    },
    
    set_kzId : function(value) {
        this._kzId = value;
    },

    get_changeValueIfNull : function() {
        return this._changeValueIfNull;
    },
    
    set_changeValueIfNull : function(value) {
        this._changeValueIfNull = value;
    },
    
    initialize : function() {
        this._onLoadHandler = Function.createDelegate(this, this._onLoad);
        Sys.Application.add_load(this._onLoadHandler);

        this._onUnloadHandler = Function.createDelegate(this, this._onUnload);
        Sys.Application.add_unload(this._onUnloadHandler);
    },

    // Destructor
    
    dispose : function() {
        if (this._onLoadHandler) {
            Sys.Application.remove_load(this._onLoadHandler);
            this._onLoadHandler = null;
        }
        
        if (this._onUnloadHandler) {
            Sys.Application.remove_load(this._onUnloadHandler);
            this._onUnloadHandler = null;
        }
        Nat.Web.Controls.CopyValue.callBaseMethod(this, 'dispose');
    },
    
    _onChange : function() {
        var kzElement = $get(this._kzId);
        if (!kzElement) return;
        if ((!kzElement.value || kzElement.value === '' || !this._changeValueIfNull)
	        && kzElement.value != this.get_element().value) {
	        kzElement.value = this.get_element().value;
	        kzElement.fireEvent('onchange');
	    }
    },

    attachEvents : function(){
        this._onChangeHandler = Function.createDelegate(this, this._onChange);
        $addHandler(this.get_element(), "change", this._onChangeHandler);
    },
    
    detachEvents : function(){
        var element = this.get_element();
        if (this._onChangeHandler && element) {
            $removeHandler(element, "change", this._onChangeHandler);
            this._onChangeHandler = null;
        }
    },
    
    _onLoad : function(evt) {
        this.attachEvents();
    },
    
    _onUnload : function(evt) {
        this.detachEvents();
    },
    
    initializeRequest : function(sender, args) {
        this.detachEvents();
    }
}
Nat.Web.Controls.CopyValue.registerClass('Nat.Web.Controls.CopyValue', Sys.UI.Behavior);

