<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="SelectingColumn.ascx.cs"
    Inherits="Nat.Web.Controls.GenerationClasses.SelectingColumn" %>
<%@ Import Namespace="Nat.Web.Controls.Properties" %>
<%@ Register Assembly="Nat.Web.Controls" Namespace="Nat.Web.Controls" TagPrefix="NatC" %>
<%@ Register Assembly="Nat.Web.Controls" Namespace="Nat.Web.Controls.GenerationClasses"
    TagPrefix="NatC" %>
<%@ Register Assembly="AjaxControlToolkit" Namespace="AjaxControlToolkit" TagPrefix="ajax" %>

<asp:HiddenField runat="server" ID="hfCols" EnableViewState="false" />
<asp:HiddenField runat="server" ID="hfRowsH" EnableViewState="false" />
<asp:HiddenField runat="server" ID="hfRows" EnableViewState="false" />
<asp:HiddenField runat="server" ID="hfCells" EnableViewState="false" />
<asp:HiddenField runat="server" ID="hfConcCols" EnableViewState="false" />
<asp:HiddenField runat="server" ID="hfConcColsNew" EnableViewState="false" />
<asp:HiddenField runat="server" ID="hfConcColsRemove" EnableViewState="false" />

<%
    if (!Journal.DetailsRender && Journal.DrawPanelVisible)
    {
%>
<div style="width: 50%; float: right; z-index: 12; background-color: white; position: relative;" class="font13">
    <fieldset style="padding-top: 4px; padding-bottom: 4px; background-color: white;">
        <legend>
            <span runat=server id="legendSC" style="cursor: pointer;" EnableViewState=false>
                <%= Resources.SSCParameters %>
            </span>
        </legend>
        <div runat=server id="contentSC" EnableViewState=false>
            <asp:TextBox runat=server ID="colorPicker" Style="visibility:hidden; width:1px;float:left" EnableViewState=false Text="FFFFFF" />
            <asp:Panel runat="server" ID="panelColorSample" style="width:18px;height:18px;border:1px solid #000;margin:0 3px;float:left" EnableViewState=false />
            <asp:ImageButton runat="Server" ID="imgColorPicker" Style="float:left;" ImageUrl="/_themes/kvv/cp_button.png" EnableViewState=false />
            <ajax:ColorPickerExtender runat=server ID="colorExtender" TargetControlID="colorPicker" PopupButtonID="imgColorPicker" SampleControlID="panelColorSample" EnableViewState=false OnClientColorSelectionChanged="fillCells_ColorChanged" />
            <a href="javascript:void(0)" onclick="fillCellsByColor($get('<%= Journal.ClientID %>'), 'fillRow', this); return false;" title='<%= Resources.SRowColorFill %>' cancelTitle='<%= Resources.SCancelColorFill %>'><img src='<%= Themes.IconUrlFillRow %>' style="border:0px" alt='<%= Resources.SRowColorFill %>' /></a>
            <a href="javascript:void(0)" onclick="fillCellsByColor($get('<%= Journal.ClientID %>'), 'fillColumn', this); return false;" title='<%= Resources.SColumnColorFill %>' cancelTitle='<%= Resources.SCancelColorFill %>'><img src='<%= Themes.IconUrlFillColumn %>' style="border:0px" alt='<%= Resources.SColumnColorFill %>' /></a>
            <a href="javascript:void(0)" onclick="fillCellsByColor($get('<%= Journal.ClientID %>'), 'fillCell', this); return false;" title='<%= Resources.SCellColorFill %>' cancelTitle='<%= Resources.SCancelColorFill %>'><img src='<%= Themes.IconUrlFillCell %>' style="border:0px" alt='<%= Resources.SCellColorFill %>' /></a>
            <a href="javascript:void(0)" onclick="fillCellsByColor($get('<%= Journal.ClientID %>'), 'fillFontRow', this); return false;" title='<%= Resources.SRowFontColorFill %>' cancelTitle='<%= Resources.SCancelFontColorFill %>'><img src='<%= Themes.IconUrlFontRow %>' style="border:0px" alt='<%= Resources.SRowFontColorFill %>' /></a>
            <a href="javascript:void(0)" onclick="fillCellsByColor($get('<%= Journal.ClientID %>'), 'fillFontColumn', this); return false;" title='<%= Resources.SColumnFontColorFill %>' cancelTitle='<%= Resources.SCancelFontColorFill %>'><img src='<%= Themes.IconUrlFontColumn %>' style="border:0px" alt='<%= Resources.SColumnFontColorFill %>' /></a>
            <a href="javascript:void(0)" onclick="fillCellsByColor($get('<%= Journal.ClientID %>'), 'fillFontCell', this); return false;" title='<%= Resources.SCellFontColorFill %>' cancelTitle='<%= Resources.SCancelFontColorFill %>'><img src='<%= Themes.IconUrlFontCell %>' style="border:0px" alt='<%= Resources.SCellFontColorFill %>' /></a>
        </div>
    </fieldset>
</div>
<%
    }
%>
<div class="font13">
    <NatC:PopupControl runat=server ID="ppc" AlwaysShow=false ShowWhileUpdating=false Width="95%" Style="display:none" EnableViewState=false>
        <div runat=server EnableViewState=false>
            <div id='<%= HeaderTableClientID %>' style="width:100%; overflow:scroll"
                behaviorID='<%= ppc.ModalPopupBehaviorID %>' 
                visibleText='<%= Resources.SViewSettings_Visible %>'
                hfColsID='<%= hfCols.ClientID %>'
                hfSortID='<%= Journal.GetHfSortClientID() %>'
                isVerticalHeaderText='<%= Resources.SViewSettings_VerticalHeader %>'
                SOrderByColumnRemove='<%= Resources.SOrderByColumnRemove %>'
                SOrderByColumn='<%= Resources.SOrderByColumn %>'
                SOrderByColumnDesc='<%= Resources.SOrderByColumnDesc %>'
                selectAggID='<%= ClientID + "_SelectAgg" %>'>
            </div>
            <div style="float: right; padding-right: 4px; padding-top: 18px; padding-bottom: 8px;">
                <a class="linkAsButton" href="javascript:void(0)" id='<%= ClientID + "_Ok" %>'>
                    <%= Resources.SSCApplyViewSettings %></a> <a class="linkAsButton" href="javascript:void(0)"
                        id='<%= ClientID + "_Cancel" %>'>
                        <%= Resources.SSCCancelViewSettings %></a>
            </div>
            <div style="display:none">
                <select id='<%= ClientID + "_SelectAgg" %>'>
                    <option value="None"><%= Resources.SAggregateNone %></option>
                    <optgroup label='<%= Resources.SAggregateNumericGroup %>'>
                        <option value="Sum"><%= Resources.SAggregateSum %></option>
                        <option value="Avg"><%= Resources.SAggregateAvg %></option>
                        <option value="Max"><%= Resources.SAggregateMax %></option>
                        <option value="Min"><%= Resources.SAggregateMin %></option>
                        <option value="Count"><%= Resources.SAggregateCount %></option>
                    </optgroup>
                    <optgroup label='<%= Resources.SAggregateTextGroup %>'>
                        <option value="GroupText" title='<%= Resources.SAggregateGroupTextToolTip %>'><%= Resources.SAggregateGroupText %></option>
                        <option value="GroupTextWithTotalPhrase" title='<%= Resources.SAggregateGroupTextWithTotalPhraseToolTip %>'><%= Resources.SAggregateGroupTextWithTotalPhrase %></option>
                    </optgroup>
                </select>
            </div>
        </div>
    </NatC:PopupControl>
    <NatC:PopupControl runat=server ID="ppcFixedHeader" AlwaysShow=false ShowWhileUpdating=false Width="450px" Style="display:none" EnableViewState=false>
        <div runat=server EnableViewState=false>
            <div style="width:100%; padding:8px" behaviorID='<%= ppcFixedHeader.ModalPopupBehaviorID %>'>
                <asp:CheckBox runat=server ID="chkHeader" Checked=true EnableViewState=false />
                <br/>
                <asp:TextBox runat=server ID="tRowsCount" Text="0" Columns=2 MaxLength=2 EnableViewState=false />&nbsp;<%= Resources.SSCRowsCountFixed %>
                <br/>
                <asp:TextBox runat=server ID="tColsCount" Text="1" Columns=2 MaxLength=2 EnableViewState=false />&nbsp;<%= Resources.SSCColsCountFixed %>
            </div>
            <div style="float: left; padding-left: 8px; padding-top: 8px; padding-bottom: 8px;">
                <a class="linkAsButton" id='<%= ClientID + "_FHOk" %>' href="javascript:void(0)">
                    <%= Resources.SSCApplyFixedHeader %>
                </a>
            </div>
        </div>
    </NatC:PopupControl>
    <NatC:PopupControl runat="server" ID="ccc" AlwaysShow="false" ShowWhileUpdating="false" Width="754px" Style="display: none">
        <div runat="server">
            <div id="<%= ConcatenateColumnDivClientID %>" hfConcColsID="<%= hfConcCols.ClientID %>" hfConcColsNewID="<%= hfConcColsNew.ClientID %>" 
                hfConcColsRemoveID="<%= hfConcColsRemove.ClientID %>" lvLeftColumnListID="<%= lvLeftColumnList.ClientID %>"
                lvRightColumnListID="<%= lvRightColumnList.ClientID %>" ddlConcatenatedColumnsID="<%= ddlConcatenatedColumns.ClientID %>">
            </div>
            <div style="float: left; padding-left: 4px; padding-top: 18px; padding-bottom: 8px;"
                behaviorid='<%= ccc.ModalPopupBehaviorID %>'>
            </div>
            <table>
                <tr align="left">
                    <td>
                        <b><%= Resources.SCCLeftColumnListTitle %></b>
                        <div>
                            <asp:ListBox runat="server" ID="lvLeftColumnList" Height="200px"  Width="350px" SelectionMode="Multiple"  EnableViewState="false" />
                        </div>
                    </td>
                    <td>
                        <table>
                            <tr>
                                <td>
                                    <a title='<%= Resources.SCCFromLeftToRight %>' href="javascript:void(0)" onclick="concatenateColumnsFromLeftList($get($(this).attr('divID')));" divID='<%= ConcatenateColumnDivClientID %>'><img src="<% = Nat.Web.Controls.Themes.IconUrlConcatenateColumnsArrowRight %>" border=0 /></a>
                                </td>
                            </tr>
                            <tr>
                                <td>
                                    <a title='<%= Resources.SCCFromRightToLeft %>' href="javascript:void(0)" onclick="concatenateColumnsFromRightList($get($(this).attr('divID')));" divID='<%= ConcatenateColumnDivClientID %>'><img src="<% = Nat.Web.Controls.Themes.IconUrlConcatenateColumnsArrowLeft %>" border=0 /></a>
                                </td>
                            </tr>
                        </table>
                    </td>
                    <td>
                        <b><%= Resources.SCCRightColumnListTitle %></b>
                        <div>
                            <asp:ListBox runat="server" ID="lvRightColumnList" Height="200px" Width="350px" SelectionMode="Multiple" EnableViewState="false" />
                        </div>
                    </td>
                </tr>
            </table>
            <table>
                <tr align="left">
                    <td>
                        <b><%= Resources.SCCConcatenatedColumnsTitle %></b>
                    </td>
                </tr>
                <tr align="left">
                    <td>
                        <asp:DropDownList runat="server" ID="ddlConcatenatedColumns" Width="500px" EnableViewState="false" />
                    </td>
                    <td>
                        <a class="linkAsButton" href="javascript:void(0)" onclick="concatenateColumnsDeleteColumn($get($(this).attr('divID')));" divID='<%= ConcatenateColumnDivClientID %>'><%= Resources.SCCCDeleteConcatenateColumns%></a>
                    </td>
                </tr>
            </table>
        </div>
        <div runat="server" style="float: right; padding-right: 4px; padding-top: 18px; padding-bottom: 8px;">
            <a class="linkAsButton" id='<%= ClientID + "_CCCOk" %>' href="javascript:void(0)">
                <%= Resources.SCCCApplyConcatenateColumns %></a> <a class="linkAsButton" id='<%= ClientID + "_CCCCancel" %>'
                    href="javascript:void(0)">
                    <%= Resources.SCCCCancelConcatenateColumns%></a>
        </div>
    </NatC:PopupControl>
</div>
