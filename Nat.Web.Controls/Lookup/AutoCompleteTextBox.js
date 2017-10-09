/*
 * Created by : Daniil Kovalev
 * Created    : 28.11.2007
 */

Type.registerNamespace("Nat.Web.Controls.AutoCompleteTextBox");

Nat.Web.Controls.AutoCompleteTextBox = function (element)
{
    Nat.Web.Controls.AutoCompleteTextBox.initializeBase(this, [element]);
    
    this._initializeRequest = null;
    this._onLoadHandler = null;
    this._hiddenFieldID = null;
    this._hiddenField = null;
    
    this._autoCompleteBehaviorID = null;
    this._autoCompleteBehavior = null;
}

Nat.Web.Controls.AutoCompleteTextBox.prototype = {
    
    // Constructor
    initialize : function() {
        this._initializeRequest = Function.createDelegate(this, this.initializeRequest);
        var prm = Sys.WebForms.PageRequestManager.getInstance();
        prm.add_initializeRequest(this._initializeRequest);
        
        this._onLoadHandler = Function.createDelegate(this, this._onLoad);
        Sys.Application.add_load(this._onLoadHandler);
        
        this._onItemSelectedHandler = Function.createDelegate(this, this._onItemSelected);
    },
    
    // Destructor
    dispose : function() {
        if (this._initializeRequest) {
            var prm = Sys.WebForms.PageRequestManager.getInstance();
            prm.remove_initializeRequest(this._initializeRequest);
            this._initializeRequest = null;
        }

        if (this._onItemSelectedHandler)
            if (this._autoCompleteBehavior !== null)
                this._autoCompleteBehavior.remove_itemSelected(this._onItemSelectedHandler);        
            
        Nat.Web.Controls.AutoCompleteTextBox.callBaseMethod(this, 'dispose');
    },
    
    // Properties
    get_hiddenFieldID : function() {
        return this._hiddenFieldID;
    },
        
    set_hiddenFieldID : function(value) {
        if (this._hiddenFieldID != value)
            this._hiddenFieldID = value;
    },
    
    get_autoCompleteBehaviorID : function() {
        return this._autoCompleteBehaviorID;
    },
        
    set_autoCompleteBehaviorID : function(value) {
        if (this._autoCompleteBehaviorID != value)
            this._autoCompleteBehaviorID = value;
    },
    
    // Methods
    clearValue : function(){
//        debugger;
//        this.get_element().value = "";
//        if (this._codeElem !== null)
//            this._codeElem.get_element().value = "";
//        this._hiddenField.value = "";
    },
    
    _onLoad : function() {
        this._hiddenField = $get(this._hiddenFieldID);
        this._autoCompleteBehavior = $find(this._autoCompleteBehaviorID);
        if (this._autoCompleteBehavior !== null)
            this._autoCompleteBehavior.add_itemSelected(this._onItemSelectedHandler);
        
    },
    
    _onItemSelected : function(sender, args)
    {
        var pair = Sys.Serialization.JavaScriptSerializer.deserialize('(' + args._value + ')');
        this._hiddenField.value = pair.First;
    },
    
    initializeRequest : function(sender, args)
    {
    }
}
Nat.Web.Controls.AutoCompleteTextBox.registerClass('Nat.Web.Controls.AutoCompleteTextBox', Sys.UI.Control);
