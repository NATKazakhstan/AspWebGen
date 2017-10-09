/*
 * Created by : Daniil Kovalev
 * Created    : 28.11.2007
 */

Type.registerNamespace("Nat.Web.Controls.PopupControl");

Nat.Web.Controls.PopupControl = function (element)
{
    Nat.Web.Controls.PopupControl.initializeBase(this, [element]);
    this._initializeRequest = null;
    this._endRequest = null;
    
    this._modalPopupBehaviorID = null;
    this.modalBehavior = null;

    this._resizeHandler = null;    
    this._scrollHandler = null;   
    this._onLoadHandler = null;
    
    this.scrollPrevValue = null;

}

Nat.Web.Controls.PopupControl.prototype = (function() {

var counter = 0;

return (
{
    
    // Constructor
    initialize : function() {
        var prm = Sys.WebForms.PageRequestManager.getInstance();
        
        this._initializeRequest = Function.createDelegate(this, this.initializeRequest);
        prm.add_initializeRequest(this._initializeRequest);
    
        this._endRequest = Function.createDelegate(this, this.endRequest);
        prm.add_endRequest(this._endRequest);    
    
        this._onLoadHandler = Function.createDelegate(this, this._onLoad);
        Sys.Application.add_load(this._onLoadHandler);
    },
    
    // Destructor
    dispose : function() {
        var prm = Sys.WebForms.PageRequestManager.getInstance();
        
        if (this._initializeRequest) 
        {
            prm.remove_initializeRequest(this._initializeRequest);
            this._initializeRequest = null;    
        }
        
        if (this._endRequest) 
        {
            prm.remove_endRequest(this._endRequest);
            this._endRequest = null;    
        }
        
        this._scrollHandler = null;
        this._resizeHandler = null;
        
        Nat.Web.Controls.PopupControl.callBaseMethod(this, 'dispose');
    },
    
    // Properties
    get_modalPopupBehaviorID : function() {
        return this._modalPopupBehaviorID;
    },
        
    set_modalPopupBehaviorID : function(value) {
        if (this._modalPopupBehaviorID != value)
            this._modalPopupBehaviorID = value;
    },
    
    get_hiddenFieldID : function() {
        return this._hiddenFieldID;
    },
        
    set_hiddenFieldID : function(value) {
        if (this._hiddenFieldID != value) {
            this._hiddenFieldID = value;
            if ($get(this._hiddenFieldID).value == "on") {
                this.show();
            }
        }
    },
    
    // Events
    _onLoad : function() {
    },
    
    showScroll : function() {
        if (counter == 0)
        {
            if (document.body.scroll != "")
                this.scrollPrevValue = document.body.scroll;
            else
                this.scrollPrevValue = null;
        }
        counter++;
        document.body.scroll = "no";
    },
    
    hideScroll : function() {
        counter--;
        if (counter == 0)
        {
            if (this.scrollPrevValue != null)
                document.body.scroll = this.scrollPrevValue;
            else            
                document.body.scroll = "";
        }            
    },
    
    show : function() {
        //this.showScroll();
        
        var elem = $find(this._modalPopupBehaviorID);
        if (elem != null)
        {
            elem.show();
            $get(this._hiddenFieldID).value = "on";
        }
    },

    hide : function() {
        //this.hideScroll();

        var elem = $find(this._modalPopupBehaviorID);
        if (elem != null) {
            elem.hide();
            $get(this._hiddenFieldID).value = "";
        }
    },
    
    initializeRequest : function(sender, args) {
        var $this = $(this.get_element());
        if ($this.attr('showWhileUpdating') == "on"
            || $this.attr('alwaysShow') == "on"
            || $get(this._hiddenFieldID).value == "on")
        {
            this.show();
        }
    },
    
    endRequest : function(sender, args)
    {
        var $this = $(this.get_element());
        if ($this.attr('showWhileUpdating') == "on")
        {
            this.hide();
        }
        if ($this.attr('alwaysShow') == "on")
        {
            this.show();
        }
    }
});
})();

Nat.Web.Controls.PopupControl.registerClass('Nat.Web.Controls.PopupControl', Sys.UI.Control);
