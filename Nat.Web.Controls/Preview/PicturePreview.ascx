<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="PicturePreview.ascx.cs" Inherits="Nat.Web.Controls.Preview.PicturePreview" %>

<%@ Import Namespace="Nat.Web.Controls.Properties" %>

<%@ Register Assembly="Nat.Web.Controls" Namespace="Nat.Web.Controls" TagPrefix="NatC" %>
<%@ Register Assembly="Nat.Web.Controls" Namespace="Nat.Web.Controls.GenerationClasses" TagPrefix="NatC" %>

<div class="font13">
    <NatC:PopupControl runat=server ID="_ppcPreview" AlwaysShow=false ShowWhileUpdating=false Width="95%">
        <div runat=server>
            <div align=center vAlign=center style="width:100%; padding: 8px; cursor:pointer; height:600px"
                id='<%= ControlID %>'
                bID='<%= _ppcPreview.ModalPopupBehaviorID %>'
                imgID='<%= _img.ClientID %>'
                aID='<%= ClientID + "_DownLoad" %>'
                cbThumbSizeID='<%= cbThumbSize.ClientID %>'
                txtwidth='<%= txtwidth.ClientID %>' 
                cbxWithCorner='<%= cbxWithCorner.ClientID %>'
                txtheight='<%= txtheight.ClientID %>'>                                
                <table cellpadding="2" cellspacing="0" style="font-weight: bold; font-size: 10pt;
                    color: black; font-family: Arial" border="1" width="97%">
                    <tr>
                        <td align="center">
                            <asp:Image runat="server" ID="_img" EnableViewState="false" Style="max-width: 800px;
                                max-height: 600px;" />
                        </td>
                        <td valign="top" width="300">
                            <table border="1" width=100%>
                                <tr>
                                    <td>
                                        <table width=100%>
                                            <tr>
                                                <caption>
                                                    <%= Resources.SInputFormatList%>
                                                </caption>
                                            </tr>
                                            <tr>
                                                <td align="center">
                                                    <asp:DropDownList ID="cbThumbSize" runat="server"></asp:DropDownList>
                                                </td>                                                                            
                                            </tr>
                                        </table>
                                    </td>
                                </tr>
                                <tr>
                                    <td>
                                        <table width=100%>
                                            <tr>
                                                <caption>
                                                    <%= Resources.SInputFormat%>
                                                </caption>
                                            </tr>                                            
                                            <tr align="left">
                                                <td>
                                                    <a><%= Resources.SWidth%></a>
                                                </td>
                                                <td>
                                                    <asp:TextBox ID="txtwidth" runat="server" Text="9" Width="79"></asp:TextBox>
                                                    <a><%= Resources.Scm %></a>
                                                </td>
                                            </tr>
                                            <tr align="left">
                                                <td>
                                                    <a><%= Resources.SHeight%></a>
                                                </td>
                                                <td>
                                                    <asp:TextBox ID="txtheight" runat="server" Text="12" Width="79"></asp:TextBox>
                                                    <a><%= Resources.Scm %></a>
                                                </td>
                                            </tr>
                                            <tr align="left">
                                                <td>
                                                    <a><%= Resources.SWithCornerUpper%></a>
                                                </td>
                                                <td>
                                                    <asp:CheckBox ID="cbxWithCorner" runat="server" Text=" " />
                                                </td>
                                            </tr>
                                            <tr align="right">
                                                <td>
                                                </td>
                                                <td>
                                                    <asp:Button ID="btnThumbSize" runat="server" class="linkAsButton" 
                                                        Style="float: none;" />
                                                </td>
                                            </tr>
                                        </table>
                                    </td>
                                </tr>
                            </table>   
                        </td>
                    </tr>
                </table>
            </div>
            <div style="float:right; padding-right:4px; padding-top:18px; padding-bottom: 8px;">
                <a class="linkAsButton" href="javascript:void(0)" id='<%= ClientID + "_DownLoad" %>'><%= Resources.SDownLoad %></a>
                <a class="linkAsButton" href="javascript:void(0)" id='<%= ClientID + "_Close" %>'><%= Resources.SClose %></a>
            </div>
        </div>
    </NatC:PopupControl>
</div>