<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="ReportResultPage.aspx.cs" Inherits="Nat.Web.ReportManager.ReportResultPage" MasterPageFile="~/_layouts/NAT/NAT.master"%>
<%@ Register Assembly="Stimulsoft.Report.Web" Namespace="Stimulsoft.Report.Web" TagPrefix="cc1" %>

<asp:Content ID="main" ContentPlaceHolderId="PlaceHolderMain" runat="server">
    <div>
        <script type="text/javascript">
            useUPBarOnPostBack = false;
        </script>
        <asp:Button ID="bBack" runat="server" OnClick="bBack_Click" Text="Button" Visible="False" />
        <br />
        <cc1:stiwebviewer id="StiWebViewer1" runat="server" CacheMode="ObjectSession" RenderMode="UseCache" AllowBookmarks="False" ShowExportToBmp="False" ShowExportToCsv="False" ShowExportToDbf="False" ShowExportToDif="False" ShowExportToGif="False" ShowExportToHtml="False" ShowExportToHtml5="False" ShowExportToDocument="False" ShowExportToMetafile="False" ShowExportToMht="False" ShowExportToOds="False" ShowExportToOdt="False" ShowExportToPcx="False" ShowExportToPng="False" ShowExportToPowerPoint="False" ShowExportToSvg="False" ShowExportToSvgz="False" ShowExportToSylk="False" ShowExportToText="False" ShowExportToTiff="False" ShowExportToXps="False" ShowExportToExcelXml="False" ShowExportToXml="False" ShowExportToRtf="False"></cc1:stiwebviewer>
    </div>
</asp:Content>

