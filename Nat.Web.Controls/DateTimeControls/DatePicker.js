Type.registerNamespace("Nat.Web.Controls.DateTimeControls");

Nat.Web.Controls.DateTimeControls.DatePicker = function(element)
{
    Nat.Web.Controls.DateTimeControls.DatePicker.initializeBase(this, [element]);

    this._calendarExtenderID = null;
    this._mode = null;

    this._onLoadHandler = null;
    this._onPropertyChangedHandler = null;
}

Nat.Web.Controls.DateTimeControls.DatePicker.prototype = {

    // Constructor
    initialize: function() {
        Nat.Web.Controls.DateTimeControls.DatePicker.callBaseMethod(this, "initialize");

        this._onLoadHandler = Function.createDelegate(this, this._onLoad);
        Sys.Application.add_load(this._onLoadHandler);

        var element = this.get_element();
        this._onPropertyChangedHandler = Function.createDelegate(this, this._onPropertyChanged);
        $addHandler(element, "propertychange", this._onPropertyChangedHandler)

        //this._set_selectedDateHandler = Function.createDelegate(this, this.set_selectedDate);
    },

    // Destructor
    dispose: function() {
        if (this._onPropertyChangedHandler) {
            var element = this.get_element();
            $removeHandler(element, "propertychange", this._onPropertyChangedHandler);
        }

        Nat.Web.Controls.DateTimeControls.DatePicker.callBaseMethod(this, "dispose");
    },

    // Properties
    get_imageButtonID: function() {
        return this._imageButtonID;
    },

    set_imageButtonID: function(value) {
        if (this._imageButtonID != value)
            this._imageButtonID = value;
    },

    get_calendarExtenderID: function() {
        return this._calendarExtenderID;
    },

    set_calendarExtenderID: function(value) {
        if (this._calendarExtenderID != value)
            this._calendarExtenderID = value;
    },

    get_popupBehaviorParentNode: function() {
        return this._popupBehaviorParentNode;
    },

    set_popupBehaviorParentNode: function(value) {
        if (this._popupBehaviorParentNode != value)
            this._popupBehaviorParentNode = value;
    },

    get_mode: function() {
        return this._mode;
    },

    set_mode: function(value) {
        if (this._mode != value)
            this._mode = value;
    },

    // Methods

    _onLoad: function() {
        if (this.get_mode() != 1) {
            var element = $find(this._calendarExtenderID);
            if (element !== null) {
                if (this._popupBehaviorParentNode != "") {
                    var node = $get(this._popupBehaviorParentNode);
                    element._popupBehaviorParentNode = node;
                }
            }
        }
    },

    _onPropertyChanged: function(source, args) {
        if (source.rawEvent.propertyName == "disabled") {
            var element = this.get_element();
            var img = $get(this._imageButtonID);
            if (element.disabled) {
                img.style.filter = "gray";
            }
            else
                img.style.filter = "";
        }
        if (source.rawEvent.propertyName == "readonly" || source.rawEvent.propertyName == "readOnly") {
            var element = this.get_element();
            var img = $get(this._imageButtonID);
            if (img != null) {
                if (element.readOnly)
                    img.style.visibility = "hidden";
                else
                    img.style.visibility = "";
            }
        }
    }
}

Nat.Web.Controls.DateTimeControls.DatePicker.registerClass('Nat.Web.Controls.DateTimeControls.DatePicker', Sys.UI.Control);
