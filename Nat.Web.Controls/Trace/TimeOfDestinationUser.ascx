<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="TimeOfDestinationUser.ascx.cs" Inherits="Nat.Web.Controls.Trace.TimeOfDestinationUser" %>
<%@ Register assembly="Nat.Web.Controls" namespace="Nat.Web.Controls.Trace" tagPrefix="NatC" %>
<asp:UpdatePanel runat="server" UpdateMode="Always">
    <ContentTemplate>
        <NatC:TraceTimeOfDestinationUserControl runat="server"></NatC:TraceTimeOfDestinationUserControl>
    </ContentTemplate>
</asp:UpdatePanel>