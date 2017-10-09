<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="SavedUserFilterValues.ascx.cs" Inherits="Nat.Web.Controls.GenerationClasses.Filter.SavedUserFilterValues" %>
<%@ Register Assembly="Nat.Web.Controls" Namespace="Nat.Web.Controls" TagPrefix="cc1" %>
<%@ Register Assembly="Nat.Web.Controls" Namespace="Nat.Web.Controls.GenerationClasses" TagPrefix="cc1" %>
<%@ Register Assembly="Nat.Web.Controls" Namespace="Nat.Web.Controls.DateTimeControls" TagPrefix="cc1" %>

<div id='<%= "filter" + TableName + "UserSavedValues" %>' style="float:right" tableName='<%= TableName %>' hvSettings='<%= hvSettings.ClientID %>' tbName='<%= tbName.ClientID %>' delButton='<%= deleteSettings.ClientID %>' fControl='<%= string.IsNullOrEmpty(FilterControlName) ? "filter" + TableName : FilterControlName %>' >
    <table>
        <tr>
            <td>
                <asp:Label ID="Label1" runat=server AssociatedControlID="tbName" 
                    Text="Наименование фильтра:" meta:resourcekey="Label1Resource1" />
            </td>
            <td>
                <asp:TextBox runat=server ID="tbName" meta:resourcekey="tbNameResource1" ValidationGroup="FilterName" Width="220px" />
                <asp:RequiredFieldValidator ID="RequiredFieldValidator1" runat=server 
                    ControlToValidate="tbName" ValidationGroup="FilterName" Display=Dynamic 
                    meta:resourcekey="RequiredFieldValidator1Resource1">*</asp:RequiredFieldValidator>
                <asp:ImageButton runat=server ID="saveSettingsIB" ValidationGroup="FilterName"
                    OnClientClick="return savedUserFilter_saveSettings(this);"
                    AlternateText="Сохранить настройки фильтра" OnClick="saveSettings_Click" 
                    EnableViewState=False ImageUrl="/App_Themes/NAT/table_save.png" 
                    meta:resourcekey="saveSettingsIBResource1" />
            </td>
        </tr>
        <tr>
            <td>
                <asp:Label ID="Label2" runat=server AssociatedControlID="ddlFilter" 
                    Text="Список сохраненных фильтров: " meta:resourcekey="Label2Resource1" />
            </td>
            <td>
                <cc1:DropDownListExt runat=server ID="ddlFilter" DataTextField="Name" Width="226px"
                    DataValueField="id" DataValuesCollectionField="FilterValues,isDangerous"
                    onchange="return savedUserFilter_change(this);" NullText="" EnableViewState=False 
                    meta:resourcekey="ddlFilterResource1" >
                    <asp:ListItem></asp:ListItem>
                </cc1:DropDownListExt>
                <asp:ImageButton runat=server ID="deleteSettings"
                    AlternateText="Удалить сохраненный фильтр" OnClick="deleteSettings_Click"
                    EnableViewState=False ImageUrl="/App_Themes/NAT/Delete.png" 
                    meta:resourcekey="deleteSettingsResource1" />
                <asp:ImageButton runat=server ID="setAsDefultFilter"
                    AlternateText="Выбранный фильтр сделать фильтром по умолчанию" 
                    OnClick="setAsDefultFilter_Click"
                    EnableViewState=False ImageUrl="/App_Themes/NAT/DefaultFilter.png" 
                    meta:resourcekey="setAsDefultFilterResource1" />
            </td>
        </tr>
    </table>
    <asp:HiddenField runat=server ID="hvSettings" />
</div>