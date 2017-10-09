/*
 * Created by : Daniil Kovalev
 * Created    : 28.11.2007
 */

Type.registerNamespace("Nat.Web.Controls.LookupTextBox");

Nat.Web.Controls.LookupTextBox = function (element)
{
    Nat.Web.Controls.LookupTextBox.initializeBase(this, [element]);
    
    this._initializeRequest = null;
    this._onLoadHandler = null;
    this._onUnloadHandler = null;
    
    this._hiddenFieldID = null;
    this._applyButtonID = null;
    this._okButtonID = null;
    this._cancelButtonID = null;
    this._showModalButtonID = null;
    this._nullButtonID = null;
    this._textBoxID = null;
    this._popupControlID = null;
    
    this._onApplyButtonClickHandler = null;
    this._onRowChangedHandler = null;
    this._onCollapseHandler = null;
}

Nat.Web.Controls.LookupTextBox.prototype = {

    // Constructor

    initialize: function() {
        this._initializeRequest = Function.createDelegate(this, this.initializeRequest);
        var prm = Sys.WebForms.PageRequestManager.getInstance();
        prm.add_initializeRequest(this._initializeRequest);

        this._onLoadHandler = Function.createDelegate(this, this._onLoad);
        Sys.Application.add_load(this._onLoadHandler);

        this._onUnloadHandler = Function.createDelegate(this, this._onUnload);
        Sys.Application.add_unload(this._onUnloadHandler);

        //this._onItemChangedHandler = Function.createDelegate(this, this._onItemChanged);

        var element = $get(this._applyButtonID);
        this._onApplyButtonClickHandler = Function.createDelegate(this, this._onApplyButtonClick);
        $addHandler(element, "click", this._onApplyButtonClickHandler);

        var element = $get(this._okButtonID);
        this._onOkButtonClickHandler = Function.createDelegate(this, this._onOkButtonClick);
        $addHandler(element, "click", this._onOkButtonClickHandler);

        var element = $get(this._cancelButtonID);
        this._onCancelButtonClickHandler = Function.createDelegate(this, this._onCancelButtonClick);
        $addHandler(element, "click", this._onCancelButtonClickHandler);

        var element = $get(this._showModalButtonID);
        this._onShowModalButtonClickHandler = Function.createDelegate(this, this._onShowModalButtonClick);
        $addHandler(element, "click", this._onShowModalButtonClickHandler);

        var element = $get(this._nullButtonID);
        this._onNullButtonClickHandler = Function.createDelegate(this, this._onNullButtonClick);
        $addHandler(element, "click", this._onNullButtonClickHandler);
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

        if (this._onCollapseHandler) {
            var element = $find(this._lookupTableID);
            element.remove_collapse(this._onCollapseHandler);
        }

        Nat.Web.Controls.LookupTextBox.callBaseMethod(this, 'dispose');
    },

    // Properties

    get_popupControlID: function() {
        return this._popupControlID;
    },

    set_popupControlID: function(value) {
        if (this._popupControlID != value)
            this._popupControlID = value;
    },

    get_textBoxID: function() {
        return this._textBoxID;
    },

    set_textBoxID: function(value) {
        if (this._textBoxID != value)
            this._textBoxID = value;
    },

    get_showModalButtonID: function() {
        return this._showModalButtonID;
    },

    set_showModalButtonID: function(value) {
        if (this._showModalButtonID != value)
            this._showModalButtonID = value;
    },

    get_nullButtonID: function() {
        return this._nullButtonID;
    },

    set_nullButtonID: function(value) {
        if (this._nullButtonID != value)
            this._nullButtonID = value;
    },

    get_cancelButtonID: function() {
        return this._cancelButtonID;
    },

    set_cancelButtonID: function(value) {
        if (this._cancelButtonID != value)
            this._cancelButtonID = value;
    },

    get_okButtonID: function() {
        return this._okButtonID;
    },

    set_okButtonID: function(value) {
        if (this._okButtonID != value)
            this._okButtonID = value;
    },

    get_applyButtonID: function() {
        return this._applyButtonID;
    },

    set_applyButtonID: function(value) {
        if (this._applyButtonID != value)
            this._applyButtonID = value;
    },

    get_hiddenFieldID: function() {
        return this._hiddenFieldID;
    },

    set_hiddenFieldID: function(value) {
        if (this._hiddenFieldID != value)
            this._hiddenFieldID = value;
    },

    get_lookupTableID: function() {
        return this._lookupTableID;
    },

    set_lookupTableID: function(value) {
        if (this._lookupTableID != value)
            this._lookupTableID = value;
    },

    // Methods
    setValue: function(evt, value){
        var textBoxPair = $find(this.get_textBoxID());
        if (textBoxPair != null) {
            var mainTextBox = textBoxPair.get_mainBehaviorID();
            var textBox = $find(mainTextBox);
            if (textBox != null) {
                var hiddenField = $get(textBox.get_hiddenFieldID());
                if (hiddenField != null)
                    hiddenField.value = value;
                
            }
        }   
    },

    getValue: function(evt) {
        var textBoxPair = $find(this.get_textBoxID());
        if (textBoxPair != null) {
            var mainTextBox = textBoxPair.get_mainBehaviorID();
            var textBox = $find(mainTextBox);
            if (textBox != null) {
                var hiddenField = $get(textBox.get_hiddenFieldID());
                if (hiddenField != null)
                    return hiddenField.value;
            }
        }
    },

    _onNullButtonClick: function(evt) {
        var behavior = $find(this.get_textBoxID());
        behavior.clearValue();
        return false;
    },

    _onShowModalButtonClick: function(evt) {
        var element = $find(this._popupControlID);
        element.show();

        return false;
    },

    _onCancelButtonClick: function(evt) {
        var element = $find(this._popupControlID);
        element.hide();

        return false;
    },

    _onOkButtonClick: function(evt) {
        var element = $find(this._lookupTableID);
        if (element.selectedDataKeys != null) {
            var values = element.selectedDataKeys.Values;

            var element = $find(this._textBoxID);
            element.selectByValue(values);

            var element = $find(this._popupControlID);
            element.hide();
        }
        else {
            alert(Nat.Web.Controls.Resources.NothingSelected);
        }
        return false;
    },

    _onRowChanged: function(evt, args) {
        if (args.type == "dblclick")
            this._onOkButtonClick(args);
    },

    _onApplyButtonClick: function(evt) {
        var element = $find(this._lookupTableID);
        element.applyFilter();
        return false;
    },

    _onCollapse: function(evt) {
        var element = $get(this._applyButtonID);
        if (element.style.display == '')
            element.style.display = 'none';
        else
            element.style.display = '';
    },

    attachEvents: function() {
        var element = $find(this._lookupTableID);
        if (element != null) {
            if (this._onCollapseHandler != null)
                element.remove_collapse(this._onCollapseHandler);
            this._onCollapseHandler = Function.createDelegate(this, this._onCollapse);
            element.add_collapse(this._onCollapseHandler);

            if (this._onRowChangedHandler != null)
                element.remove_rowChanged(this._onRowChangedHandler);
            this._onRowChangedHandler = Function.createDelegate(this, this._onRowChanged);
            element.add_rowChanged(this._onRowChangedHandler);
        }
    },

    detachEvents: function() {
        var element = $find(this._lookupTableID);
        if (element != null) {
            element.remove_collapse(this._onCollapseHandler);
            this._onCollapseHandler = null;

            element.remove_rowChanged(this._onRowChangedHandler);
            this._onRowChangedHandler = null;
        }
    },

    _onLoad: function(evt) {
        this.attachEvents();
    },

    _onUnload: function(evt) {
        this.detachEvents();
    },

    initializeRequest: function(sender, args) {
        this.detachEvents();
    }
}

Nat.Web.Controls.LookupTextBox.registerClass('Nat.Web.Controls.LookupTextBox', Sys.UI.Control);


