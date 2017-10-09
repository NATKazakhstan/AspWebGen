Type.registerNamespace("Nat.Web.Controls.DropDownListExtBehavior");

Nat.Web.Controls.DropDownListExtBehavior = function(element)
{
    Nat.Web.Controls.DropDownListExtBehavior.initializeBase(this, [element]);
    this._mouseOverHandler = null;
}

Nat.Web.Controls.DropDownListExtBehavior.prototype = {

    // Constructor
    initialize: function() {
        Nat.Web.Controls.DropDownListExtBehavior.callBaseMethod(this, 'initialize');
        var element = this.get_element();
        for (var i = 0; i < element.options.length; i++) {
            if (element.options[i].title == null || element.options[i].title == '')
                element.options[i].title = element.options[i].text;
        }
        this._mouseOverHandler = Function.createDelegate(this, this._onMouseOver);
        $addHandler(element, "mouseover", this._mouseOverHandler);
    },

    // Destructor
    dispose: function() {
        this._mouseOverHandler = null;
        Nat.Web.Controls.DropDownListExtBehavior.callBaseMethod(this, 'dispose');
    },

    _onMouseOver: function(event) {
        var element = this.get_element();
        if (!element) return;
        if (element.selectedIndex > -1)
            element.title = element.options[element.selectedIndex].title;
    }

}
Nat.Web.Controls.DropDownListExtBehavior.registerClass('Nat.Web.Controls.DropDownListExtBehavior', Sys.UI.Behavior);
