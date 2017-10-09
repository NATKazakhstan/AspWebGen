/*
 * Created by : Daniil Kovalev
 * Created    : 28.11.2007
 */

Type.registerNamespace("Nat.Web.Controls.AutoCompleteTextBoxPair");

Nat.Web.Controls.AutoCompleteTextBoxPair = function(element)
{
    Nat.Web.Controls.AutoCompleteTextBoxPair.initializeBase(this, [element]);
    this._initializeRequest = null;
    this._onLoadHandler = null;
    this._mainBehaviorID = null;
    this._codeBehaviorID = null;
    this._mainElem = null;
    this._codeElem = null;
    this._mainElemBehavior = null;
    this._codeElemBehavior = null;
}

Nat.Web.Controls.AutoCompleteTextBoxPair.prototype = {
    
    // Constructor
    initialize : function() {
        this._initializeRequest = Function.createDelegate(this, this.initializeRequest);
        var prm = Sys.WebForms.PageRequestManager.getInstance();
        prm.add_initializeRequest(this._initializeRequest);
        
        this._onLoadHandler = Function.createDelegate(this, this._onLoad);
        Sys.Application.add_load(this._onLoadHandler);
        
        this._onUnloadHandler = Function.createDelegate(this, this._onUnload);
        Sys.Application.add_unload(this._onUnloadHandler);
    },
    
    // Destructor
    dispose : function() {
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
        
        Nat.Web.Controls.AutoCompleteTextBoxPair.callBaseMethod(this, 'dispose');
    },
    
    // Properties
    get_dataValueField : function() {
        return this._dataValueField;
    },
        
    set_dataValueField : function(value) {
        if (this._dataValueField != value)
            this._dataValueField = value;
    },
    
    get_dataTextField : function() {
        return this._dataTextField;
    },
        
    set_dataTextField : function(value) {
        if (this._dataTextField != value)
            this._dataTextField = value;
    },
    
    get_dataCodeField : function() {
        return this._dataCodeField;
    },
        
    set_dataCodeField : function(value) {
        if (this._dataCodeField != value)
            this._dataCodeField = value;
    },
    
    get_mainBehaviorID : function() {
        return this._mainBehaviorID;
    },
        
    set_mainBehaviorID : function(value) {
        if (this._mainBehaviorID != value)
            this._mainBehaviorID = value;
    },
    
    get_codeBehaviorID : function() {
        return this._codeBehaviorID;
    },
        
    set_codeBehaviorID : function(value) {
        if (this._codeBehaviorID != value)
            this._codeBehaviorID = value;
    },
    
    
    // Methods
    clearValue : function(){
        this._mainElemBehavior.get_element().value = '';
        this._mainElem._hiddenField.value = '';
        $find(this._mainElemBehavior.get_element().id).update();
        
        if (this._codeElemBehavior !== null)
        {
            this._codeElemBehavior.get_element().value = '';
            this._codeElem._hiddenField.value = '';
            $find(this._codeElemBehavior.get_element().id).update();
        }
    },
    
    attachEvents : function(){
        this._onItemSelectedHandler = Function.createDelegate(this, this._onItemSelected);
    
        if (this._mainBehaviorID !== null) {
            this._mainElem = $find(this._mainBehaviorID);
            if (this._mainElem !== null)
            {
                this._mainElemBehavior = $find(this._mainElem._autoCompleteBehaviorID);         
                if (this._mainElemBehavior !== null)
                    this._mainElemBehavior.add_itemSelected(this._onItemSelectedHandler);
            }
        }
        
        if (this._codeBehaviorID !== null) {
            this._codeElem = $find(this._codeBehaviorID);
            if (this._codeElem !== null) {
                this._codeElemBehavior = $find(this._codeElem._autoCompleteBehaviorID);
                if (this._codeElemBehavior !== null)
                    this._codeElemBehavior.add_itemSelected(this._onItemSelectedHandler);
            }
        }
    },
    
    detachEvents : function(){
        if (this._mainBehaviorID !== null) {
            this._mainElem = $find(this._mainBehaviorID);
            if (this._mainElem !== null)
            {
                this._mainElemBehavior = $find(this._mainElem._autoCompleteBehaviorID);         
                if (this._mainElemBehavior !== null)
                    this._mainElemBehavior.remove_itemSelected(this._onItemSelectedHandler);
            }
        }
        
        if (this._codeBehaviorID !== null) {
            this._codeElem = $find(this._codeBehaviorID);
            if (this._codeElem !== null) {
                this._codeElemBehavior = $find(this._codeElem._autoCompleteBehaviorID);
                if (this._codeElemBehavior !== null)
                    this._codeElemBehavior.remove_itemSelected(this._onItemSelectedHandler);
            }
        }
        
        this._onItemSelectedHandler = null;
    },
    
    _onLoad : function() {
        this.attachEvents();
    },
    
    _onUnload : function(evt) {
        this.detachEvents();
    },
    
    _onItemSelected : function(sender, args) {
        var pair = Sys.Serialization.JavaScriptSerializer.deserialize('(' + args._value + ')');
        if (sender === this._mainElemBehavior && this._codeElemBehavior !== null)
        {
            this._codeElemBehavior.get_element().value = pair.Second;
            this._codeElem._hiddenField.value = pair.First;
            $find(this._codeElemBehavior.get_element().id).update();
        }
        else if (sender === this._codeElemBehavior && this._mainElemBehavior !== null)
        {
            this._mainElemBehavior.get_element().value = pair.Second;
            this._mainElem._hiddenField.value = pair.First;
            $find(this._mainElemBehavior.get_element().id).update();
        }
    },
    
    selectByValue : function(values) {
        this._mainElemBehavior.get_element().value = values[this._dataTextField];
        this._mainElem._hiddenField.value = values[this._dataValueField];
        $find(this._mainElemBehavior.get_element().id).update();
        
        if (this._dataCodeField !== '')
        {
            this._codeElemBehavior.get_element().value = values[this._dataCodeField];
            this._codeElem._hiddenField.value = values[this._dataValueField];
            $find(this._codeElemBehavior.get_element().id).update();
        }
    },
    
    initializeRequest : function(sender, args)
    {
        this.detachEvents();
    }
}
Nat.Web.Controls.AutoCompleteTextBoxPair.registerClass('Nat.Web.Controls.AutoCompleteTextBoxPair', Sys.UI.Control);