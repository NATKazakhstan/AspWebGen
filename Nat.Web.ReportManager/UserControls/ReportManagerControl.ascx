<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="ReportManagerControl.ascx.cs" Inherits="Nat.Web.ReportManager.UserControls.ReportManagerControl" %>
<%@ Register Assembly="AjaxControlToolkit" Namespace="AjaxControlToolkit" TagPrefix="cc2" %>
<%@ Register Assembly="System.Web.Extensions, Version=3.5.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35"
    Namespace="System.Web.UI" TagPrefix="asp" %>
<%@ Register Assembly="Stimulsoft.Report.Web" Namespace="Stimulsoft.Report.Web" TagPrefix="cc1" %>
<%@ Register Assembly="Nat.Web.Tools" Namespace="Nat.Web.Tools" TagPrefix="cc1" %>
<%@ Register Assembly="Nat.Web.Controls" Namespace="Nat.Web.Controls" TagPrefix="cc1" %>
<%@ Register Assembly="Nat.Web.Controls" Namespace="Nat.Web.Controls.DataBinding.Extended" TagPrefix="cc1" %>
<%@ Register Assembly="Nat.Web.Controls" Namespace="Nat.Web.Controls.GenerationClasses" TagPrefix="cc1" %>

<cc1:SessionWorkerControl ID="swReport" runat="server" Key="swKey" OnSessionWorkerInit="swReport_SessionWorkerInit" />
<table width="100%" border="0" bordercolor="#006600" cellpadding="3" 
    cellspacing="1" class="report_tab" >
    <tr>
        <td ID="tdTreeView" runat="server" rowspan="3" class="report_border" valign="top">
            <asp:Panel runat="server" ID="pList" meta:resourcekey="pListResource1">
                &nbsp;
                <asp:Label ID="Label1" runat="server" Text="Список отчетов:" Font-Bold="True" 
                    CssClass="report_head" meta:resourcekey="Label1Resource2"></asp:Label>
                <br />
                <br />
                <asp:TreeView ID="TreeView1" runat="server" EnableViewState="False" 
                    OnSelectedNodeChanged="TreeView1_SelectedNodeChanged" 
                    meta:resourcekey="TreeView1Resource2">
                    <SelectedNodeStyle BackColor="#FFE499" />
                </asp:TreeView>
            </asp:Panel>
        </td>
        <td style="width: 100%; height: 100%;" valign="top" class="dic_tab2">
        <table runat="server" ID="checkboxPanel">
            <tr>
                <td>
                    <cc1:ImageCheckBox runat="server" ID="ImageCheckBox" HideIDControl="pList" 
                        Checked="True" ImageUrlChecked="/App_Themes/KVV/open_spr.gif" 
                        ImageUrlUnchecked="/App_Themes/KVV/close_spr.gif" 
                        ToolTipChecked="Скрыть список отчетов" 
                        ToolTipUnchecked="Показать список отчетов" 
                        Text="Показать/скрыть список отчетов" 
                        meta:resourcekey="ImageCheckBoxResource1" />
                </td>
                <td vAlign="top">
                    <cc1:HelpFileLink runat="server" ID="HelpFileLink" NavigateUrl="/HelpFiles/ReportView/ReportViewer.htm"/>
                </td>
            </tr>
        </table>
            <asp:UpdatePanel ID="upConditions" runat="server" UpdateMode="Conditional">
                <ContentTemplate>
                    &nbsp;
                    <asp:Label ID="lReport" runat="server" Font-Bold="True" Font-Strikeout="False" 
                        Font-Underline="True" CssClass="report_head" 
                        meta:resourcekey="lReportResource2"></asp:Label>
                    <br />
                    <br />
                    <asp:PlaceHolder ID="ph" runat="server"></asp:PlaceHolder>
                    <cc1:ErrorDisplay ID="errorDisplay" runat="server" 
                        UseFixedHeightWhenHiding="False" Width="400px" />
                </ContentTemplate>
            </asp:UpdatePanel>
            <asp:PlaceHolder ID="phCircle" runat="server"></asp:PlaceHolder>
        </td>
    </tr>
    <tr>
        <td align="left" style="width: 100%">
            <asp:UpdatePanel ID="upCreateReport" runat="server" UpdateMode="Conditional">
                <ContentTemplate>
            &nbsp;<asp:Button ID="btnCreateReportKz" runat="server" Text="Сформировать отчет" OnClientClick="useUPBarOnPostBack = false; return true;"
                        OnClick="btnCreateReportKz_Click" 
                        meta:resourcekey="btnCreateReportKzResource1" />
            &nbsp;<asp:Button ID="btnCreateReportRu" runat="server" Text="Сформировать отчет" OnClientClick="useUPBarOnPostBack = false; return true;"
                        OnClick="btnCreateReportRu_Click" 
                        meta:resourcekey="btnCreateReportRuResource1" />
            &nbsp;<asp:Button ID="btnClearValueConditions" runat="server" Text="Очистить форму" OnClientClick="return false;" Visible="False"                       
                        meta:resourcekey="SClearValueConditions" />
            &nbsp;<asp:Button ID="btnSubscriptionsJournal" runat="server" Text="Создать подписку" OnClientClick="useUPBarOnPostBack = false; return true;"
			OnClick="btnSubscriptionsJournal_Click" meta:resourcekey="btnSubscriptionsJournalResource1" />
            &nbsp;<asp:Button ID="btnSubscriptionsSaveParams" runat="server" 
                        Text="Сохранить параметры" OnClientClick="useUPBarOnPostBack = false; return true;"
			OnClick="btnSubscriptionsSaveParams_Click" Visible="False" 
                        meta:resourcekey="btnSubscriptionsSaveParamsResource1"/>			
                </ContentTemplate>
                <Triggers>
                    <asp:PostBackTrigger ControlID="btnCreateReportRu" />
                    <asp:PostBackTrigger ControlID="btnCreateReportKz" />
                    <asp:PostBackTrigger ControlID="btnSubscriptionsJournal" />
                    <asp:PostBackTrigger ControlID="btnSubscriptionsSaveParams" />                    
                </Triggers>
            </asp:UpdatePanel>
            &nbsp;
        </td>
    </tr>
</table>
<br />
<cc1:StiWebViewer id="StiWebViewer1" runat="server" UseCache="True"></cc1:StiWebViewer>
<cc1:LoaderCssScript runat="server" ID="LoaderCssScript1" 
    meta:resourcekey="LoaderCssScript1Resource2"/>

<script type="text/javascript">
    var listTemp = [{ id: 'id', value: '' }];
    function ClearFormValues(listID) {
        for (var i = 0; i < listID.length; i++) {
            var control = $get(listID[i].id);
            if (control == null)
                continue;

            var valueChanged = false;
            if (control.type == 'checkbox') {
                if (!control.checked) {
                    control.checked = false;
                    valueChanged = true;
                }
            } else if (control.value != listID[i].value) {
                control.value = listID[i].value;
                valueChanged = true;
            }

            if ($(control).attr('fireChangeWhenSetClear') == 'true' && valueChanged)
                fireEventOnChange(control);
        }
    }
</script>