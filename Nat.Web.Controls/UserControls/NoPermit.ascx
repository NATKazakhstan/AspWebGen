<%@ Control Language="C#" AutoEventWireup="true" %>
<%
    switch (Attributes["codeError"])
    {
        case "subdivisionNull":
            Response.Write(Resources.SPersonHasNotSubdivision);
            break;
        default:
            Response.Write(Resources.SNoPermit);
            if (!string.IsNullOrEmpty(Request.QueryString["ErrorMessage"]))
            {
                Response.Write("<br/>");
                Response.Write(HttpUtility.HtmlEncode(Request["ErrorMessage"]));
            }
            if (!string.IsNullOrEmpty(Attributes["ErrorMessage"]))
            {
                Response.Write("<br/>");
                Response.Write(HttpUtility.HtmlEncode(Attributes["ErrorMessage"]));
            }
            break;
    } 
%>