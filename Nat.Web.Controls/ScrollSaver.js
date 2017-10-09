
Type.registerNamespace("Nat.Web.Controls");
Nat.Web.Controls.ScrollSaver= function Nat$Web$Controls$ScrollSaver(element)
{
    Nat.Web.Controls.ScrollSaver.initializeBase(this);
    this._scrollControls = null;
    this._clientID = null
    this._initializeRequest = null;
    this._endRequest = null;
    this._onLoadHanler = null;
    this._array = null;
}

Nat.Web.Controls.ScrollSaver.prototype = {
    
    // Constructor
    initialize : function() {
        var prm = Sys.WebForms.PageRequestManager.getInstance();

        this._initializeRequest = Function.createDelegate(this, this.initializeRequest);
        prm.add_initializeRequest(this._initializeRequest);

        this._endRequest = Function.createDelegate(this, this.endRequest);
        
        this._onLoadHandler = Function.createDelegate(this, this._onLoad);
        Sys.Application.add_load(this._onLoadHandler);
    },
    
    // Destructor
    dispose : function() {
        var prm = Sys.WebForms.PageRequestManager.getInstance();
        
        if (this._onLoadHandler) {
            Sys.Application.remove_load(this._onLoadHandler);
            this._onLoadHandler = null;
        }
        
        if (this._initializeRequest) {
            prm.remove_initializeRequest(this._initializeRequest);
            this._initializeRequest = null;
        }
        
        if (this._endRequest) {
            prm.remove_endRequest(this._endRequest);
            this._endRequest = null;
        }
        Nat.Web.Controls.ScrollSaver.callBaseMethod(this, 'dispose');
    },
    
    // Properties
    get_scrollControls : function() {
        return this._scrollControls;
    },
        
    set_scrollControls : function(value) {
        if (this._scrollControls != value)
            this._scrollControls = Sys.Serialization.JavaScriptSerializer.deserialize(value);
    },
    
    get_clientID : function() {
        return this._clientID;
    },
        
    set_clientID : function(value) {
        if (this._clientID != value)
            this._clientID = value;
    },
    
    // Methods
    _onLoad : function() {
        //var control = $get(this._clientID);
        //if (control.value == 666) {
        //    alert("There is something in the air. I can small it");
        //}
    },
     
    initializeRequest : function(sender, args) {
        var prm = Sys.WebForms.PageRequestManager.getInstance();
        prm.add_endRequest(this._endRequest);
        
        _array = new Array();
        for (var i in this._scrollControls)
        {
            var control = $get(this._scrollControls[i])
            
            if (control.scrollTop != null && control.scrollLeft != null)
            {
                if (control.scrollTop != 0 || control.scrollLeft != 0)
                {
                    _array[i] = 
                    {
                        "id" : this._scrollControls[i],
                        "scrollTop" : control.scrollTop,
                        "scrollLeft" : control.scrollLeft
                    };
                }        
            }
        }
    },
    
    endRequest : function (sender, args) {
        for (var i = 0; i < _array.length; i++) 
        {
            var control = $get(_array[i].id)
            control.scrollTop = _array[i].scrollTop;
            control.scrollLeft = _array[i].scrollLeft;
        }
    }
    
}
Nat.Web.Controls.ScrollSaver.registerClass('Nat.Web.Controls.ScrollSaver', Sys.Component);