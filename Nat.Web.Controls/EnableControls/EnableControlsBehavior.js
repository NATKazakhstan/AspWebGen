Type.registerNamespace("Nat.Web.Controls.EnableControls.EnableControlsBehavior");

Nat.Web.Controls.EnableControls.EnableControlsBehavior = function(element) {
    Nat.Web.Controls.EnableControls.EnableControlsBehavior.initializeBase(this, [element]);
    this._controls = null;
    this._changeHandler = null;
    this._items = null;
    this._changeNewTargetHandler = null;
    this._changeNewTargetCheckBoxHandler = null;
}

Nat.Web.Controls.EnableControls.EnableControlsBehavior.prototype = {

    // Constructor
    initialize: function () {

        Nat.Web.Controls.EnableControls.EnableControlsBehavior.callBaseMethod(this, 'initialize');
    },

    // Destructor
    dispose: function () {

        Nat.Web.Controls.EnableControls.EnableControlsBehavior.callBaseMethod(this, 'dispose');

        this._changeHandler = null;
        this._changeNewTargetHandler = null;
        this._changeNewTargetCheckBoxHandler = null;
    },

    // Properties
    get_controls: function () {

        return this._controls;

    },

    set_controls: function (value) {

        if (this._controls != value) {
            this._controls = Sys.Serialization.JavaScriptSerializer.deserialize(value);
            this._changeHandler = Function.createDelegate(this, this._change);
            this._changeNewTargetHandler = Function.createDelegate(this, this._changeNewTarget);
            this._changeNewTargetCheckBoxHandler = Function.createDelegate(this, this._changeNewTargetCheckBox);
            for (var i in this._controls) {
                var con = $get(this._controls[i]);
                var $con = null;
                if (con != null && (con.tagName == "INPUT" || con.tagName == "SELECT" || con.tagName == "TEXTAREA"))
                    $con = $(con);
                else
                    $con = $(con).find(":input");
                var thisItem = this;
                if ($con != null && $con.length > 0) {
                    if ($con[0].type == "checkbox" || $con[0].type == "radio")
                        $con.click(function() { thisItem._change(); });
                    else
                        $con.change(function() { thisItem._change(); });
                }
            }
        }
        this._raiseChange();
    },

    get_items: function () {

        return this._items;

    },

    set_items: function (value) {

        if (this._items != value) {
            this._items = Sys.Serialization.JavaScriptSerializer.deserialize(value);
        }
        this._raiseChange();
    },

    _getEnabledItem: function (item) {
        var enable;
        if (item.ListMode == "0")
            enable = true;
        else if (item.ListMode == "1")
            enable = false;

        for (var i in item.Items) {
            if (item.Items[i].TypeComponent == "0") {
                if (item.ListMode == "0") {
                    enable = enable && this._getEnabledItem(item.Items[i]);
                }
                else if (item.ListMode == "1") {
                    enable = enable || this._getEnabledItem(item.Items[i]);
                }
            }
            else if (item.Items[i].TypeComponent == "3") {

                if (item.ListMode == "0")
                    enable = enable && !item.Items[i].Disable;
                else if (item.ListMode == "1")
                    enable = enable || !item.Items[i].Disable;
            }
            else {
                if (item.Items[i].ControlID == null) continue;
                var control = $get(this._controls[item.Items[i].ControlID]);
                if (control == null) continue;

                var disable;
                var equal;
                var cValue;

                disable = item.Items[i].Disable;

                if (item.Items[i].TypeComponent == "1") {
                    cValue = control.tagName == "SPAN" || control.tagName == "A" ? $(control).attr('value') : control.value.toString();
                    equal = cValue == item.Items[i].Value.toString();
                }
                else if (item.Items[i].TypeComponent == "4") {
                    cValue = control.tagName == "SPAN" || control.tagName == "A" ? $(control).attr('value') : control.value.toString();
                    equal = cValue.split(',').contains(item.Items[i].Value.toString());
                }
                else if (item.Items[i].TypeComponent == "2") {
                    cValue = null;
                    if (control.type == "checkbox" || control.type == "radio")
                        cValue = control.checked.toString();
                    else {
                        cValue = $(control).find("> input:checked").val();
                        if (cValue != null)
                            cValue = cValue.toLowerCase();
                    }

                    if (cValue == null && control.tagName == "SPAN" || control.tagName == "A")
                        cValue = $(control).attr('value');
                    
                    equal = cValue == item.Items[i].Checked.toString();
                }

                if (item.ListMode == "0") {
                    if (equal)
                        enable = enable && !disable;
                    else
                        enable = enable && disable;
                }
                else if (item.ListMode == "1") {
                    if (equal)
                        enable = enable || !disable;
                    else
                        enable = enable || disable;
                }
            }
        }

        return enable;
    },

    _raiseChange: function () {

        if (this._items != null && this._controls != null)
            this._change();
    },

    _changeNewTarget: function (event) {

        if (event.target.newTarget != null)
            event.target.newTarget.value = event.target.value;
    },

    _changeNewTargetCheckBox: function (event) {

        if (event.target.newTarget != null)
            event.target.newTarget.checked = event.target.checked;
    },

    _change: function (event) {

        var element = this.get_element();
        if (!element) return;

        for (var i in this._items) {

            var target = $get(this._items[i].targetID);
            //if (target == null) continue;
            var enabled = this._getEnabledItem(this._items[i].EnableItems);
            if (target != null)
                this._setDisabled(enabled, target, this._items[i].EnableMode);
            for (var child in this._items[i].aditinalTargetID) {

                target = $get(this._items[i].aditinalTargetID[child]);
                if (target != null)
                    this._setDisabled(enabled, target, this._items[i].EnableMode);
            }
        }
    },

    _setDisabled: function (enabled, target, enableMode) {
        if (enabled) {

            if ((enableMode & 1) == 1) {
                target.disabled = false;
                this._setDisableFalse(target, false);
            }
            if ((enableMode & 2) == 2) {
                if (target.style.visibility !== 'hidden' || !($(target).attr('isIgnoreVisible') == 'true' || target.controltovalidate != null || target.id.indexOf('completionListElem') > 0))
                    target.style.display = "";
                this._setDisplay(target, "");
            }
            if ((enableMode & 4) == 4) {

                if (target.tagName == "SELECT" || (target.tagName == "INPUT" && target.type != "text")) {
                    target.style.display = "";
                    target.disabled = false;
                    if (target.newTarget != null) {
                        if (target.tagName == "SELECT")
                            $removeHandler(target, "change", this._changeNewTargetHandler);
                        else
                            $removeHandler(target, "click", this._changeNewTargetCheckBoxHandler);
                        target.parentNode.removeChild(target.newTarget);
                    }
                    target.newTarget = null;
                }
                else
                    target.readOnly = null;
                this._setReadOnly(target, null);
                
                if ($(target).is('input'))
                    $(target).removeClass('m-field-readOnly');
                else
                    $(target).find('input').removeClass('m-field-readOnly');
            }
            if ((enableMode & 8) == 8 && window.Page_Validators != null) {

                for (var i in Page_Validators) {

                    var validator = Page_Validators[i];
                    if (validator.controltovalidate == target.id && validator.disableValidationGroup != null) {
                        validator.validationGroup = validator.disableValidationGroup;
                        validator.disableValidationGroup = null;
                    }
                }
                if ($(target).attr('isValidationMessage'))
                    target.style.display = '';
            }
        }
        else {

            if ((enableMode & 1) == 1) {
                target.disabled = true;
                this._setDisableFalse(target, true);
            }
            if ((enableMode & 2) == 2) {
                if (target.style.visibility !== 'hidden' || !($(target).attr('isIgnoreVisible') == 'true' || target.controltovalidate != null || target.id.indexOf('completionListElem') > 0))
                    target.style.display = "none";
                this._setDisplayNone(target, "none");
            }
            if ((enableMode & 4) == 4) {

                if (target.tagName == "SELECT" || (target.tagName == "INPUT" && target.type != "text")) {
                    target.style.display = "none";
                    if (target.newTarget == null) {

                        if (target.tagName == "SELECT") {
                            var newTarget = document.createElement("input");
                            target.parentNode.insertBefore(newTarget, target);
                            newTarget.target = target;
                            newTarget.value = target.options[target.selectedIndex].text;
                            newTarget.readOnly = -1;
                            newTarget.width = this._getWidth(target.width);
                            newTarget.style.width = this._getWidth(target.style.width);
                            target.newTarget = newTarget;
                            $addHandler(target, "change", this._changeNewTargetHandler);
                        }
                        else if (target.type == "checkbox" || target.type == "radio") {
                            var newTarget = document.createElement("input");
                            newTarget.type = target.type;
                            target.parentNode.insertBefore(newTarget, target);
                            newTarget.target = target;
                            newTarget.checked = target.checked;
                            newTarget.disabled = true;
                            target.newTarget = newTarget;
                            $addHandler(target, "click", this._changeNewTargetCheckBoxHandler);
                        }
                    }
                }
                else
                    target.readOnly = -1;
                this._setReadOnly(target, -1);
                if ($(target).is('input'))
                    $(target).addClass('m-field-readOnly');
                else
                    $(target).find('input').addClass('m-field-readOnly');
            }
            if ((enableMode & 8) == 8 && window.Page_Validators != null) {

                for (var i in Page_Validators) {

                    var validator = Page_Validators[i];
                    if (validator.controltovalidate == target.id && validator.disableValidationGroup == null) {
                        validator.disableValidationGroup = validator.validationGroup;
                        validator.validationGroup = "disableValidationGroup";
                        if ((enableMode & 1) == 1 || enableMode == 8) {
                            if (validator.display != 'Dynamic')
                                validator.style.visibility = 'hidden';
                            else
                                validator.style.display = 'none';
                        }
                    }
                }
                if ($(target).attr('isValidationMessage'))
                    target.style.display = 'none';
            }
        }
    },

    _getWidth: function (width) {
        if (width != null && width.substring(width.length - 1) == "%")
            width = (width.substring(0, width.length - 1) - 1) + "%";
        return width;
    },

    _setReadOnly: function (control, readonly) {

        for (var i = 0; i < control.children.length; i++) {
            $(control.children[i]).attr('readOnly', readonly).trigger('ecbReadOnly');
            this._setReadOnly(control.children[i], readonly);
        }
    },

    _setDisableFalse: function (control, disabled) {

        for (var i = 0; i < control.children.length; i++) {
            $(control.children[i]).attr('disabled', disabled).trigger('ecbDisbled');
            this._setDisableFalse(control.children[i], disabled);
        }
    },

    _setDisplay: function (control, style) {

        if (control.parentNode != null && control.parentNode.style != null && control.parentNode.style.display !== style) {
            if (control.parentNode.style.visibility !== 'hidden' && $(control.parentNode).attr('isIgnoreVisible') != 'true')
                control.parentNode.style.display = style;
            this._setDisplay(control.parentNode, style);
        }
    },

    _setDisplayNone: function (control, style) {

        if (this._isEqualsDisplay(control.parentNode, style)) {
            if (control.parentNode.style.visibility !== 'hidden' && $(control.parentNode).attr('isIgnoreVisible') != 'true')
                control.parentNode.style.display = style;
            this._setDisplayNone(control.parentNode, style);
        }
    },

    _isEqualsDisplay: function (control, style) {

        for (var i = 0; i < control.children.length; i++) {
            if (control.children[i].id != null && control.children[i].id != "" &&
                control.children[i].type != "hidden" &&
                control.children[i].style.display !== style && (control.children[i].style.visibility !== 'hidden' &&
                !($(control.children[i]).attr('isIgnoreVisible') == 'true' || control.children[i].id.indexOf('completionListElem') > 0))) {
                return false;
            }
            if (control.children[i].style.display !== style && (control.children[i].style.visibility !== 'hidden' &&
                control.children[i].type != "hidden" &&
                !($(control.children[i]).attr('isIgnoreVisible') == 'true' || control.children[i].id.indexOf('completionListElem') > 0))
                && !this._isEqualsDisplay(control.children[i], style))
                return false;
        }
        return true;
    }

}
Nat.Web.Controls.EnableControls.EnableControlsBehavior.registerClass('Nat.Web.Controls.EnableControls.EnableControlsBehavior', Sys.UI.Behavior);
