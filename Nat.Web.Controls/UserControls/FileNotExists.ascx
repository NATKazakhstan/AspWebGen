<%@ Import Namespace="Nat.CommonInfo.Properties"%>
<%@ Control Language="C#" AutoEventWireup="true" %>
<%
    switch (Attributes["codeError"])
    {
        default:
            Response.Write(Resources.SFileNotExists);
            if (!string.IsNullOrEmpty(Request.QueryString["ErrorMessage"]))
            {
                Response.Write("<br/>");
                Response.Write(System.Web.HttpUtility.HtmlEncode(Request["ErrorMessage"]));
            }
            if (!string.IsNullOrEmpty(Attributes["ErrorMessage"]))
            {
                Response.Write("<br/>");
                Response.Write(System.Web.HttpUtility.HtmlEncode(Attributes["ErrorMessage"]));
            }
            break;
    } 
%>