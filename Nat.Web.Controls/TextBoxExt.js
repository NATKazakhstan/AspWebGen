/*
 * Created by : Daniil Kovalev
 * Created    : 28.11.2007
 */

Type.registerNamespace("Nat.Web.Controls.TextBoxExt");

Nat.Web.Controls.TextBoxExt = function(element)
{
    Nat.Web.Controls.TextBoxExt.initializeBase(this, [element]);
    this._initializeRequest = null;
    this._onLoadHandler = null;
    this._textBoxID = null;
}

Nat.Web.Controls.TextBoxExt.prototype = {
    
    // Constructor
    initialize : function() {
        this._initializeRequest = Function.createDelegate(this, this.initializeRequest);
        var prm = Sys.WebForms.PageRequestManager.getInstance();
        prm.add_initializeRequest(this._initializeRequest);
        
        this._onLoadHandler = Function.createDelegate(this, this._onLoad);
        Sys.Application.add_load(this._onLoadHandler);


        var element = this.get_element();
        this._onTextChangeHandler = Function.createDelegate(this, this._onTextChange);
        $addHandler(element, "keyup", this._onTextChangeHandler);
        $addHandler(element, "change", this._onTextChangeHandler);
    },
    
    // Destructor
    dispose : function() {
        if (this._initializeRequest) {
            var prm = Sys.WebForms.PageRequestManager.getInstance();
            prm.remove_initializeRequest(this._initializeRequest);
            this._initializeRequest = null;
        }
        Nat.Web.Controls.TextBoxExt.callBaseMethod(this, 'dispose');
    },
    
    // Methods
    
    _onLoad : function() {
    },
    
    initializeRequest : function(sender, args)
    {
    },
    
    update : function()
    {
        var element = this.get_element();
        this._fireChanged();
    },
    
    _fireChanged : function() {
        /// <summary>
        /// Attempts to fire the change event on the attached textbox
        /// </summary>
        
        var elt = this.get_element();
        if (document.createEventObject) 
        {
            elt.fireEvent("onchange");
        } else if (document.createEvent) 
        {
            var e = document.createEvent("HTMLEvents");
            e.initEvent("change", true, true);
            elt.dispatchEvent(e);
        }
    },    
    
    _onTextChange : function()
    {
        var element = this.get_element();
        element.title = element.value;
    }
}
Nat.Web.Controls.TextBoxExt.registerClass('Nat.Web.Controls.TextBoxExt', Sys.UI.Control);