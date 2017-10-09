/*
 * Created by : Daniil Kovalev
 * Created    : 11.12.2007
 */
 
Type.registerNamespace("Nat.Web.Controls.ColumnFilterList");

Nat.Web.Controls.ColumnFilterList = function (element)
{
    Nat.Web.Controls.ColumnFilterList.initializeBase(this, [element]);
    
    this._initializeRequest = null;
    this._onLoadHandler = null;
    this._onUnloadHandler = null;
    
    this._hiddenFieldID = null;
    this._onFullBriefViewButton = null;
}

Nat.Web.Controls.ColumnFilterList.prototype = {
    
    // Constructor
    initialize : function() {
        Nat.Web.Controls.ColumnFilterList.callBaseMethod(this, 'initialize');
    
        this._initializeRequest = Function.createDelegate(this, this.initializeRequest);
        var prm = Sys.WebForms.PageRequestManager.getInstance();
        prm.add_initializeRequest(this._initializeRequest);
        
        this._endRequest = Function.createDelegate(this, this.endRequest);
        prm.add_endRequest(this._endRequest);    
        
        this._onLoadHandler = Function.createDelegate(this, this._onLoad);
        Sys.Application.add_load(this._onLoadHandler);

        this._onUnloadHandler = Function.createDelegate(this, this._onUnload);
        Sys.Application.add_unload(this._onUnloadHandler);
        
        this._onFullBriefClickHandler = Function.createDelegate(this, this._onFullBriefClick);
        var element = $get(this._fullBriefViewButtonID);
        $addHandler(element, "click", this._onFullBriefClickHandler);
        
        var element = $get(this._hiddenFieldID);
        this.setFullBriefView(element.value == "");
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

        
        if (this._onLoadHandler) {
            Sys.Application.remove_load(this._onLoadHandler);
            this._onLoadHandler = null;
        }
        
        if (this._onUnloadHandler) {
            Sys.Application.remove_load(this._onUnloadHandler);
            this._onUnloadHandler = null;
        }
        
        Nat.Web.Controls.ColumnFilterList.callBaseMethod(this, 'dispose');
    },
    
    // Properties
    get_hiddenFieldID : function() {
        return this._hiddenFieldID;
    },
        
    set_hiddenFieldID : function(value) {
        if (this._hiddenFieldID != value)
            this._hiddenFieldID = value;
    },
    
    get_fullBriefViewButtonID : function() {
        return this._fullBriefViewButtonID;
    },
        
    set_fullBriefViewButtonID : function(value) {
        if (this._fullBriefViewButtonID != value)
            this._fullBriefViewButtonID = value;
    },
    
    get_saveFiltersStateButtonID : function() {
        return this._saveFiltersStateButtonID;
    },
        
    set_saveFiltersStateButtonID : function(value) {
        if (this._saveFiltersStateButtonID != value)
            this._saveFiltersStateButtonID = value;
    },
    
    get_columnFilterProps : function() {
        return this._columnFilterProps;
    },
        
    set_columnFilterProps : function(value) {
        if (this._columnFilterProps != value)
            this._columnFilterProps = value;
    },
        
    get_filterPanelID : function() {
        return this._filterPanelID;
    },
        
    set_filterPanelID : function(value) {
        if (this._filterPanelID != value)
            this._filterPanelID = value;
    },
    
    get_imageControlID : function() {
        return this._imageControlID;
    },
        
    set_imageControlID : function(value) {
        if (this._imageControlID != value)
            this._imageControlID = value;
    },
    
    get_briefViewImage : function() {
        return this._briefViewImage;
    },
        
    set_briefViewImage : function(value) {
        if (this._briefViewImage != value)
            this._briefViewImage = value;
    },
    
    get_fullViewImage : function() {
        return this._fullViewImage;
    },
        
    set_fullViewImage : function(value) {
        if (this._fullViewImage != value)
            this._fullViewImage = value;
    },
    
    get_briefViewText : function() {
        return this._briefViewText;
    },
        
    set_briefViewText : function(value) {
        if (this._briefViewText != value)
            this._briefViewText = value;
    },
    
    get_fullViewText : function() {
        return this._fullViewText;
    },
        
    set_fullViewText : function(value) {
        if (this._fullViewText != value)
            this._fullViewText = value;
    },
    
    // Methods
    
    _onFullBriefClick : function() {
        this.changeFullBriefView();
        return false;        
    },
    
    changeFullBriefView: function() {
        var element = $get(this._hiddenFieldID);
        element.value = element.value == "" ? "on" : "";
        this.setFullBriefView(element.value == "");
    },
    
    setFullBriefView : function(briefView) {
        var rowsCount = 0;
        if (briefView)
        {
            Array.forEach(
                this._columnFilterProps, 
                function(b) {
                    var element = $get(b.First);
                    if (b.Second)
                        element.style.display = 'none';
                    else
                        rowsCount++;
                }
            );
        }
        else
        {
            Array.forEach(
                this._columnFilterProps, 
                function(b) {
                    var element = $get(b.First);
                    if (element.style.display == 'none')
                        element.style.display = '';
                }
            );
            rowsCount = this._columnFilterProps.length;
        }
        this.setHeight(rowsCount);
        this.setImage(briefView);
    },
    
    setHeight : function(rowsCount)
    {
        /*
        var maxHeight = 100;
        var element = $get(this._filterPanelID);
        var newHeight = rowsCount * 30;
        if (newHeight > maxHeight)
            element.style.height = maxHeight + 'px';
        else
            element.style.height = newHeight + 'px';*/
    },
    
    setImage : function(briefView)
    {
        var image;
        var title;
        if (briefView) {
            image = this._fullViewImage;
            title = this._fullViewText;
        }
        else {
            image = this._briefViewImage;
            title = this._briefViewText;
        }
        if (this._imageControlID && image) {
            var i = $get(this._imageControlID);
            if (i && i.src) {
                i.src = image;
                if (this._briefViewText || this._fullViewText) 
                    i.title = title;
            }
        }            
    
    },
    
    _onLoad : function(evt) {
        if (this.get_element().parentNode.nodeName == "FIELDSET")
            this.get_element().parentNode.style["position"] = "relative";
    },
    
    _onUnload : function(evt) {
    },
    
    initializeRequest : function(sender, args) {
    },

    endRequest : function(sender, args) 
    {
        if (this.get_element().parentNode.nodeName == "FIELDSET")
            this.get_element().parentNode.style["position"] = "relative";
    }
}

Nat.Web.Controls.ColumnFilterList.registerClass('Nat.Web.Controls.ColumnFilterList', Sys.UI.Control);


