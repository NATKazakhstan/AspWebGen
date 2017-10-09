<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="BaseMenuUserControl.ascx.cs" Inherits="Nat.Web.Controls.GenerationClasses.Navigator.BaseMenuUserControl" %>

<%@ Import Namespace="Nat.Web.Controls.Properties" %>

<%@ Register Assembly="Nat.Web.Controls" Namespace="Nat.Web.Controls" TagPrefix="NatC" %>
<%@ Register Assembly="Nat.Web.Controls" Namespace="Nat.Web.Controls.GenerationClasses" TagPrefix="NatC" %>
<%@ Register Assembly="Nat.Web.Controls" Namespace="Nat.Web.Controls.GenerationClasses.Navigator" TagPrefix="NatC" %>

<a href="javascript:void(0)" 
    onclick='$find("<%= ppc.ModalPopupBehaviorID  %>").show(); return false;'
    title='<%= Resources.SNavigationMenu %>'><img style="border:0;" src='<%= Themes.IconUrlMenu %>' alt='<%= Resources.SNavigationMenu %>' /></a>
<NatC:PopupControl runat=server ID="ppc" AlwaysShow=false ShowWhileUpdating=false Width="35%"
    Style="background-color:#F4FAFA">
    <div runat=server>
        <div style="padding: 14px; overflow:auto; height: 450px">
            <asp:PlaceHolder runat=server ID="ph" />
        </div>
        <div style="float:right; padding-right:4px; padding-top:18px; padding-bottom: 8px;">
            <a class="linkAsButton" href="javascript:void(0)" id='<%= ClientID + "_Cancel" %>'><%= Resources.SCancelText %></a>
        </div>
    </div>
</NatC:PopupControl>