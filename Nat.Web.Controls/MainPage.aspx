<%@ Page Title="" Language="C#" MasterPageFile="~/_layouts/NAT/nat.master" AutoEventWireup="true" Inherits="Nat.Web.Controls.BaseMainPage" %>
<%@ Register Assembly="Nat.Web.Controls" Namespace="Nat.Web.Controls" TagPrefix="NatC" %>

<%@ Register src="Trace/TimeOfDestinationUser.ascx" tagname="TimeOfDestinationUser" tagprefix="NatC" %>

<asp:Content ID="TitleContent" ContentPlaceHolderID="PlaceHolderPageTitle" runat="server">
	<%= PageTitle %>
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="PlaceHolderMain" runat="server">
    <div style="width:100%;height:100%">
		<asp:Panel ID="RootPanel" runat="server">
		    <asp:UpdatePanel ID="upUMAllways" runat="server" UpdateMode=Always>
		        <ContentTemplate>
		            <asp:PlaceHolder ID="phUMAllways" runat="server" />
		        </ContentTemplate>
		    </asp:UpdatePanel>
		    <asp:PlaceHolder ID="ph" runat="server" />
		    <asp:UpdatePanel ID="up" runat="server" UpdateMode="Conditional">
		        <ContentTemplate>
		            <asp:PlaceHolder ID="phInUP" runat="server" />
		        </ContentTemplate>
		    </asp:UpdatePanel>
		    <asp:UpdatePanel ID="upActions" runat="server" UpdateMode="Conditional">
		        <ContentTemplate>
		            <asp:PlaceHolder ID="phActions" runat="server" />
		        </ContentTemplate>
		    </asp:UpdatePanel>
		</asp:Panel>
    </div>
	<NatC:LoaderCssScript ID="LoaderCssScript1" runat="server" />

    <NatC:TimeOfDestinationUser ID="TimeOfDestinationUser1" runat="server" />

</asp:Content> 
<script runat="server" type="text/C#">
    protected override PlaceHolder PlaceHolder
    {
        get
        {
            return ph;
        }
    }

    protected override PlaceHolder PlaceHolderInUP
    {
        get
        {
            return phInUP;
        }
    }

    protected override PlaceHolder PlaceHolderInUpInternal
    {
        get { return phUMAllways; }
    }

    protected override UpdatePanel UpdatePanelForActions
    {
        get
        {
            return upActions;
        }
    }

    protected override PlaceHolder PlaceHolderForActions
    {
        get
        {
            return phActions;
        }
    }
    
    protected override string GetRedirectUrl(string userControl)
    {
        return null;
    }
</script>