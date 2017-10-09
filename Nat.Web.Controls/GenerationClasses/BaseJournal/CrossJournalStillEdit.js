Type.registerNamespace("Nat.Web.Controls");
Nat.Web.Controls.CrossJournalStillEdit = function (element) {
    Nat.Web.Controls.CrossJournalStillEdit.initializeBase(this, [element]);
    this._journalName = null;
    this._rowID = null;
    this._interval = 30000;
    this._timer = null;
    this._tickHandler = null;
    this._succeededCallbackHandler;
}
Nat.Web.Controls.CrossJournalStillEdit.prototype = {
    get_journalName: function () {
        return this._journalName;
    },

    set_journalName: function (value) {
        this._journalName = value;
    },

    get_rowID: function () {
        return this._rowID;
    },

    set_rowID: function (value) {
        this._rowID = value;
    },

    get_interval: function () {
        return this._interval;
    },

    set_interval: function (value) {
        this._interval = value;
    },

    initialize: function () {
        Nat.Web.Controls.CrossJournalStillEdit.callBaseMethod(this, 'initialize');
        this._tickHandler = Function.createDelegate(this, this.onTick);
        this._succeededCallbackHandler = Function.createDelegate(this, this.succeededCallback);
        this._timer = new Sys.Timer();
        this._timer.set_interval(this._interval);
        this._timer.add_tick(this._tickHandler);
        this._timer.set_enabled(true);
    },

    // Destructor

    dispose: function () {
        if (this._timer != null) {
            this._timer.dispose();
            this._timer = null;
        }
        this._tickHandler = null;
        this._succeededCallbackHandler = null;
        Nat.Web.Controls.CrossJournalStillEdit.callBaseMethod(this, 'dispose');
    },

    onTick: function () {
        Nat.Web.Controls.GenerationClasses.WebServiceSavedJournalSettings.StillEditCrossData(this.get_journalName(), this.get_rowID(), this._succeededCallbackHandler);
    },

    succeededCallback: function (result, eventArgs) {
        var element = this.get_element();
        if (result != null && result != '' && element != null) {
            element.innerHTML = result;
        }
    },

    initializeRequest: function (sender, args) {
        this.detachEvents();
    }
}
Nat.Web.Controls.CrossJournalStillEdit.registerClass('Nat.Web.Controls.CrossJournalStillEdit', Sys.UI.Behavior);

