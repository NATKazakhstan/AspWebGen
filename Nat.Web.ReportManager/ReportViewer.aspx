<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="ReportViewer.aspx.cs" Inherits="Nat.Web.ReportManager.ReportViewer" MasterPageFile="~/_layouts/NAT/NAT.master" %>
<%@ Register Src="UserControls\ReportManagerControl.ascx" TagName="ReportManagerControl" TagPrefix="uc1" %>

<asp:Content ID="main" ContentPlaceHolderId="PlaceHolderMain" runat="server">
    <div>
        <uc1:ReportManagerControl id="ReportManagerControl1" runat="server"/>
    </div>
</asp:Content>
