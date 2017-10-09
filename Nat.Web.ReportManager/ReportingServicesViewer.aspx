<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="ReportingServicesViewer.aspx.cs" Inherits="Nat.Web.ReportManager.ReportingServicesViewer" MasterPageFile="~/_layouts/NAT/NAT.master" %>

<%@ Register Assembly="Microsoft.ReportViewer.WebForms, Version=10.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a"
    Namespace="Microsoft.Reporting.WebForms" TagPrefix="rsweb" %>
 

<asp:Content ID="main" ContentPlaceHolderId="PlaceHolderMain" runat="server">
    <div>
        <script type="text/javascript">
            useUPBarOnPostBack = false;
        </script>
        <asp:Button ID="bBack" runat="server" OnClick="bBack_Click" Text="Button" Visible="False" />
        <br />
        <rsweb:ReportViewer ID="rv" runat="server" ProcessingMode="Remote" Width="100%" Height="700px" ShowParameterPrompts=false>
            <ServerReport ReportPath="/ProcessDatabase_Reports/ProjectPlan" ReportServerUrl="http://stendnat/ReportServer/" />
        </rsweb:ReportViewer>
    </div>
</asp:Content>
