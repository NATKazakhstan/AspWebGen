/*
 * Created by: Denis M. Silkov
 * Copyright (c) New Age Technologies
 * Created: 24 сентября 2007 г.
 */
Type.registerNamespace("Nat.Web.Controls.StatusInfo");
Nat.Web.Controls.StatusInfo.WaitStatus = function Nat$Web$Controls$StatusInfo$WaitStatus(element) {
    Nat.Web.Controls.StatusInfo.WaitStatus.initializeBase(this, [element]);
    this._beginRequest = null;
    this._endRequest = null;
}
Nat.Web.Controls.StatusInfo.WaitStatus.prototype = {
    
    // Constructor
    initialize : function() {
        Nat.Web.Controls.StatusInfo.WaitStatus.callBaseMethod(this, 'initialize');        
        this._beginRequest = Function.createDelegate(this, this.beginRequest);
        this._endRequest = Function.createDelegate(this, this.endRequest);
        var prm = Sys.WebForms.PageRequestManager.getInstance();
        if (prm != null) {
            prm.add_beginRequest(this._beginRequest);
            prm.add_endRequest(this._endRequest);
        }
    },
    
    // Destructor
    dispose : function() {
        var prm = Sys.WebForms.PageRequestManager.getInstance();
        if (prm != null) {
            if (this._beginRequest) {
                prm.remove_beginRequest(this._beginRequest);
                this._beginRequest = null;
            }
            if (this._endRequest) {
                prm.remove_endRequest(this._endRequest);
                this._endRequest = null;
            }
        }
        Nat.Web.Controls.StatusInfo.WaitStatus.callBaseMethod(this, 'dispose');
        
    },
    
    beginRequest : function(sender, args) {
        var element = this.get_element();
        if (element != null) {
            element.style.visibility = "visible";
            element.innerText = "Обработка...";
        }
    },
    
    endRequest : function(sender, args) {
        var element = this.get_element();
        if (element != null) {
            element.style.visibility = "hidden";
            element.innerText = "";
        }
    }
}
Nat.Web.Controls.StatusInfo.WaitStatus.registerClass('Nat.Web.Controls.StatusInfo.WaitStatus', Sys.UI.Control);