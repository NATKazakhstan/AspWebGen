/*
 * Created by : Daniil Kovalev
 * Created    : 28.11.2007
 */

/// <reference name="MicrosoftAjaxTimer.debug.js" />
/// <reference name="MicrosoftAjaxWebForms.debug.js" />
/// <reference name="AjaxControlToolkit.ExtenderBase.BaseScripts.js" assembly="AjaxControlToolkit" />

Type.registerNamespace('Nat.Web.Controls');

Nat.Web.Controls.GridViewBehavior = function(element) {
    Nat.Web.Controls.GridViewBehavior.initializeBase(this, [element]);
    this._dataKeys = null;
    
    this.selectedDataKeys = null;
    this.selectedIndex = null;
    this._currentSelectedRowIndex = null;
    this._conditionValue = 1;
}

Nat.Web.Controls.GridViewBehavior.prototype = {
    
    // Constructor
    initialize : function() {
        Nat.Web.Controls.GridViewBehavior.callBaseMethod(this, 'initialize');

        var element = this.get_element();
        this._onClickHandler = Function.createDelegate(this, this._onClick);
        $addHandler(element, "dblclick", this._onClickHandler);
        $addHandler(element, "click", this._onClickHandler);
        
        this.initRows();
    },
    
    // Destructor
    dispose : function() {

        Nat.Web.Controls.GridViewBehavior.callBaseMethod(this, 'dispose');
    },

    // Properties
    get_dataKeys : function() {
        return Sys.Serialization.JavaScriptSerializer.serialize('(' + this._dataKeys + ')');;
    },
        
    set_dataKeys : function(value) {
        if (this._dataKeys != value)
            this._dataKeys = Sys.Serialization.JavaScriptSerializer.deserialize('(' + value + ')');
    },
    
    get_selectedRowCssClass : function() {
        return this._selectedRowCssClass;
    },
        
    set_selectedRowCssClass : function(value) {
        if (this._selectedRowCssClass != value) {
            this._selectedRowCssClass = value;
        }
    },
    
    get_rowCssClass : function() {
        return this._rowCssClass;
    },
        
    set_rowCssClass : function(value) {
        if (this._rowCssClass != value) {
            this._rowCssClass = value;
        }
    },
    
    get_alternatingRowCssClass: function() {
        return this._alternatingRowCssClass;
    },
        
    set_alternatingRowCssClass: function(value) {
        if (this._alternatingRowCssClass!= value) {
            this._alternatingRowCssClass= value;
        }
    },
    
    get_dataDisableRowField: function() {
        return this._dataDisableRowField;
    },
        
    set_dataDisableRowField: function(value) {
        if (this._dataDisableRowField!= value) {
            this._dataDisableRowField= value;
        }
    },
    
    get_conditionValue: function() {
        return this._conditionValue;
    },
        
    set_conditionValue: function(value) {
        if (this._conditionValue!= value) {
            this._conditionValue= value;
        }
    },
    
    // Methods
    selectRow : function(index) {
        var element = this.get_element();
        var _selectedRow = element.rows[index];
        
        this.selectedDataKeys = this._dataKeys[index - 1];
        this.selectedIndex = index;
        
        if (this._currentSelectedRowIndex != null) {
            var row = element.rows[this._currentSelectedRowIndex];
            if (row != null) {
                if ((this._currentSelectedRowIndex % 2) == 0)
                    row.className = this._alternatingRowCssClass;
                else
                    row.className = this._rowCssClass;
            }
        }
        
        if (null != _selectedRow) {
            this._currentSelectedRowIndex = index;
            this._currentSelectedRowClassName = _selectedRow.className;
            _selectedRow.className = this._selectedRowCssClass;
        }
    },
    
    initRows : function() {
        var element = this.get_element();
        element.unselectable = "on";
        
        for(i = 0; i < this._dataKeys.length; i++)
        {
            
            var row = element.rows[i + 1];
            row.unselectable = "on";
            for (j = 0; j < row.cells.length; j++)
            {
                row.cells[j].unselectable = "on";
            }
            
            if ((i % 2) != 0)
                row.className = this._alternatingRowCssClass;
            else
                row.className = this._rowCssClass;
        
            if (this._dataDisableRowField != "" && this._dataKeys.length > 0) 
            {
                var value = this._dataKeys[i].Values[this._dataDisableRowField];
                var enabled = this._conditionValue == value;
                if (!enabled)
                {
                    row.style.color = "green";
                    row.style.cursor = "default";
                }
                else
                {
                    row.style.cursor = "hand";
                }
            }
        }
    },
    
    _onClick : function(evt) {
        var index = evt.target.parentElement.rowIndex;
        if (index > 0 && index <= this._dataKeys.length)  {
            var enabled = true;
            if (this._dataDisableRowField != "") 
            {
                var value = this._dataKeys[index - 1].Values[this._dataDisableRowField];
                enabled = this._conditionValue == value;
            }
            
            if (enabled) {
                this.selectRow(index);
                this.raiseRowChanged(evt);
            }
        }
    },
    
    add_rowChanged : function(handler) {
        this.get_events().addHandler("rowChanged", handler);
    },
    
    remove_rowChanged : function(handler) {
        this.get_events().removeHandler("rowChanged", handler);
    },
    
    raiseRowChanged : function(evt) {
        var handler = this.get_events().getHandler('rowChanged');
        if (handler) {
            handler(this,  evt);
        }
    }
}
Nat.Web.Controls.GridViewBehavior.registerClass('Nat.Web.Controls.GridViewBehavior', AjaxControlToolkit.BehaviorBase);
