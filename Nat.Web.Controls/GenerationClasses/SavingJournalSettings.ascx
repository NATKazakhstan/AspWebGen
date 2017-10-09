<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="SavingJournalSettings.ascx.cs" Inherits="Nat.Web.Controls.GenerationClasses.SavingJournalSettings" %>

<%@ Import Namespace="Nat.Web.Controls.Properties" %>

<%@ Register Assembly="Nat.Web.Controls" Namespace="Nat.Web.Controls" TagPrefix="NatC" %>
<%@ Register Assembly="Nat.Web.Controls" Namespace="Nat.Web.Controls.GenerationClasses" TagPrefix="NatC" %>

<div class="font13">
    <asp:HiddenField runat=server ID="_hfDeleteViewSettings" EnableViewState=false />
    <NatC:PopupControl runat=server ID="_ppcSave" AlwaysShow=false ShowWhileUpdating=false Width="624px" EnableViewState=false>
        <div runat=server EnableViewState=false>
            <div style="width:100%; padding: 8px;">
                <asp:Label ID="_lRewriteSettings" runat=server Text="Перезаписать настройки:" Style="width:200px;display: inline-block" EnableViewState=false />
                <NatC:DropDownListExt runat=server ID="_ddlSaveFilters" EnableViewState=false Style="width:400px"
                    DataTextField="name" DataValueField="id" ValidationGroup="savedLoad"
                    NullText="" IncludeNullItem=true DataValuesCollectionField="nameRu,nameKz,isShared,SaveFilters" />
                <a runat=server id="_hlDelSavedInSave" href="javascript:void(0);"><img style="border:0" src='<%= Themes.IconUrlDelete %>' alt='<%= Resources.SDeleteText %>' /></a>
                <br />
                <asp:CheckBox runat=server ID="_chkSaveFilters" Checked=false Text="Сохранить фильтры" EnableViewState=false />
                <br />
                <asp:CheckBox runat=server ID="_chkSaveAsShared" Checked=false Text="Показывать всем" EnableViewState=false />
                <br />
                <asp:Label ID="_nameKz" runat=server Text="Наименование (КАЗ):" Style="width:200px;display: inline-block;" EnableViewState=false />
                <asp:TextBox runat=server ID="_tbSaveNameKz" Style="width:400px;margin-bottom:6px;margin-top:4px;" EnableViewState=false ValidationGroup="saving"></asp:TextBox>
                <asp:RequiredFieldValidator runat=server ControlToValidate="_tbSaveNameKz" 
                    ErrorMessage="*" Text="*" ValidationGroup="saving" EnableViewState=false />
                <br />
                <asp:Label ID="_nameRu" runat=server Text="Наименование (РУС):" Style="width:200px;display: inline-block" EnableViewState=false />
                <asp:TextBox runat=server ID="_tbSaveNameRu" Style="width:400px" EnableViewState=false ValidationGroup="saving"></asp:TextBox>
                <asp:RequiredFieldValidator runat=server ControlToValidate="_tbSaveNameRu" 
                    ErrorMessage="*" Text="*" ValidationGroup="saving" EnableViewState=false />
            </div>
            <div style="float:right; padding-right:4px; padding-top:18px; padding-bottom: 8px;">
                <a class="linkAsButton" href="javascript:void(0)" id='<%= ClientID + "_OkSave" %>'><%= Resources.SSavingJournal_Save %></a>
                <a class="linkAsButton" href="javascript:void(0)" id='<%= ClientID + "_CancelSave" %>'><%= Resources.SSavingJournal_Cancel %></a>
            </div>
        </div>
    </NatC:PopupControl>
    <NatC:PopupControl runat=server ID="_ppcLoad" AlwaysShow=false ShowWhileUpdating=false Width="500px" EnableViewState=false>
        <div runat=server EnableViewState=false>
            <div style="width:100%; padding: 8px;">
                <asp:CheckBox runat=server ID="_chkLoadFilters" Checked=false Text="Загрузить фильтры (если сохранены)" EnableViewState=false />
                <br />
                <%= Resources.SLoadViewSettings_SavedSettings %>:
                <br />
                <NatC:DropDownListExt runat=server ID="_ddlLoadFilters" EnableViewState=false Style="width:450px"
                    DataTextField="name" DataValueField="id" ValidationGroup="savedLoad"
                    NullText="" IncludeNullItem=true />
                <a runat=server id="_hlDelSavedInOpen" href="javascript:void(0);"><img style="border:0" src='<%= Themes.IconUrlDelete %>' alt='<%= Resources.SDeleteText %>' /></a>
                <asp:RequiredFieldValidator runat=server ControlToValidate="_ddlLoadFilters" 
                    ErrorMessage="*" Text="*" ValidationGroup="savedLoad" EnableViewState=false />
            </div>
            <div style="float:right; padding-right:4px; padding-top:18px; padding-bottom: 8px;">
                <a class="linkAsButton" href="javascript:void(0)" id='<%= ClientID + "_OkLoad" %>'><%= Resources.SSavingJournal_Load %></a>
                <a class="linkAsButton" href="javascript:void(0)" id='<%= ClientID + "_CancelLoad" %>'><%= Resources.SSavingJournal_Cancel %></a>
            </div>
        </div>
    </NatC:PopupControl>
</div>