Type.registerNamespace("JS.Web.ActivityController");

JS.Web.ActivityController = function () {
    JS.Web.ActivityController.initializeBase(this);
    this._changedControls = null;
    this._activityControls = null;
    this._extNet = true;
    this._validationGroup = null;
    this._readOnly = null;
    this._isNew = null;
    this._formID = null;
},

JS.Web.ActivityController.prototype = {

    // Constructor
    initialize: function () {

        JS.Web.ActivityController.callBaseMethod(this, 'initialize');
    },

    // Destructor
    dispose: function () {

        JS.Web.ActivityController.callBaseMethod(this, 'dispose');
        if (this.get_extNet())
            this.unwireEventToExtNet();
        else
            this.unwireEventJQuery();
    },
    
    get_changedControls: function () {

        return this._changedControls;
    },

    set_changedControls: function (value) {

        if (this._changedControls != value) {
            
            this._changedControls = value;
            if (this.get_extNet()) {
                var activity = this;
                Ext.onReady(function() {
                    activity.wireEventToExtNet(activity);
                });
            } else
                this.wireEventJQuery();
        }
    },

    get_activityControls: function () {

        return this._activityControls;
    },

    set_activityControls: function (value) {

        this._activityControls = value;
    },
    
    get_extNet: function () {

        return this._extNet;
    },

    set_extNet: function (value) {

        this._extNet = value;
    },

    get_validationGroup: function () {

        return this._validationGroup;
    },

    set_validationGroup: function (value) {

        this._validationGroup = value;
    },

    get_readOnly: function () {

        return this._readOnly;
    },

    set_readOnly: function (value) {

        this._readOnly = value;
    },

    get_isNew: function () {

        return this._isNew;
    },

    set_isNew: function (value) {

        this._isNew = value;
    },
    
    get_formID: function () {

        return this._formID;
    },

    set_formID: function (value) {

        this._formID = value;
    },

    wireEventToExtNet: function (activity) {
        if (window.App == null) return;
        
        var controls = activity.get_changedControls();
        if (controls == null)
            return;
        
        for (var i = 0; i < controls.length; i++) {
            var control = eval(controls[i]);
            if (control != null) {
                control.addListener('change', function() {
                    activity.onValueChangedDelay(activity);
                });
            }
        }
    },

    wireEventJQuery: function () {
    },

    unwireEventToExtNet: function () {
        if (window.App == null) return;
        
        var controls = this.get_changedControls();
        if (controls == null)
            return;

        for (var i = 0; i < controls.length; i++) {
            var control = eval(controls[i]);
            
        }
    },

    unwireEventJQuery: function () {
    },

    setHiddenControlContainer: function(activity, container) {
        if (container == null) return;
        var hidden = true;
        for (var i = 0; i < container.items.length; i++) {
            var ctrl = container.items.items[i];
            if (ctrl.xtype == 'hiddenfield') continue;
            if (!ctrl.hidden) {
                hidden = false;
                break;
            }
        };
        container.setVisible(!hidden);
        
        var parentContainer = activity.getParentContainer(container);
        activity.setHiddenControlContainer(activity, parentContainer);
    },

    getParentContainer: function (control) {
        var parentContainer = window.App[control.container.id];

        if (parentContainer == null && control.container.parent != null)
            parentContainer = window.App[control.container.parent().id];

        if (parentContainer == null && control.container.parent != null && control.container.parent().parent != null)
            parentContainer = window.App[control.container.parent().parent().id];

        return parentContainer;
    },

    onValueChangedDelay: function(activity) {
        clearTimeout(activity.valueChangedTimeOut);
        activity.valueChangedTimeOut = setTimeout(function() {
                activity.onValueChanged(activity);
            },
            50);
    },

    onValueChanged: function (activity) {
        
        var activityControls = activity.get_activityControls();
        var existsChanges = false;
        for (var i = 0; i < activityControls.length; i++) {

            var activityItem = activity['get_' + activityControls[i]]();
            activityItem.parent = activity;
            
            var enabled = activityItem.get_enabled();
            var visible = activityItem.get_visible();
            var readOnly = activityItem.get_readOnly();
            var allowRequired = activityItem.get_allowRequiredValidate();
            var control = activityItem.get_control();
            
            if (!control)
                continue;

            if (!enabled)
                control.disable();
            else 
                control.enable();
            
            if (control.setReadOnly)
                control.setReadOnly(readOnly);

            control.setVisible(visible);
            activity.setHiddenControlContainer(activity, activity.getParentContainer(control));
            if (activity.setAllowBlank(control, !allowRequired))
                existsChanges = true;
        }

        if (existsChanges && activity.get_formID() != null)
            eval(activity.get_formID() + ".validate()");
    },
    
    setAllowBlank: function (control, allowBlank) {

        var result = true;
        if (control.xtype == "filefield") {
        }
        else if (control.setAllowBlank != null) {
            if (control.getAllowBlank != null)
                result = control.getAllowBlank() != allowBlank;
            control.setAllowBlank(allowBlank);
        } else {
            result = control.allowBlank != allowBlank;
            control.allowBlank = allowBlank;
        }

        var span = control.getEl().down('.requiredFieldMark');
        if (span != null) {
            
            if (allowBlank)
                span.dom.style.display = 'none';
            else
                span.dom.style.display = '';
        }

        return result;
    }
},

JS.Web.ActivityController.registerClass('JS.Web.ActivityController', Sys.Component);