using System.Web.UI;
using System.Web.UI.WebControls;

namespace Nat.Web.Tools
{
    public static class TextEditorHelper
    {
        #region Hide/Show Editor

        public static string ShowEditor(string HideElemID, string ShowElemID)
        {
            return string.Format(
                @"
                            $get('{0}').style.display = 'block'; 
                            $get('{1}').style.display = 'none';
                            return false;
                         ",
                ShowElemID, HideElemID);
        }

        public static string HideEditor(string HideElemID, string ShowElemID)
        {
            return string.Format(
                @"
                   $get('{0}').style.display = 'block';  
                   $get('{1}').style.display = 'none'; 
                 ",
                ShowElemID, HideElemID);
        }

        #endregion

        #region Hide/Show Editor with PopupControl

        public static string ShowEditor(string PopupControlID)
        {
            return string.Format(@"$find('{0}').show();return false;", PopupControlID);
        }

        public static string HideEditor(string PopupControlID)
        {
            return string.Format(@"$find('{0}').hide();", PopupControlID);
        }
        
        public static string HideEditor(string PopupControlID, bool result)
        {
            return string.Format(@"var popup =  $find('{0}');
if(popup) {{
    //popup.alwaysShow = '';
    popup.hide();
}}
return {1};", PopupControlID, result.ToString().ToLower());
        }

        #endregion

        public static void ShowTextInReadOnlyMode(Control ElementsOwner, string hiddenElementID, string LinkButttonID,
                                                  string ButtonShowText, string ButtonHideText)
        {
            var ctrl = ElementsOwner.FindControl(LinkButttonID);
            if (ctrl == null) return;
            var lb = (LinkButton) ctrl;
            var hiddenControl = ElementsOwner.FindControl(hiddenElementID);
            if (hiddenControl != null)
            {
                lb.Text = ((WebControl)hiddenControl).Style["display"] == "none" ? ButtonShowText : ButtonHideText;
                lb.OnClientClick = string.Format(
                    @"
                            var divelem = $get('{0}'); 
                            var elem = $get('{1}');  
                            if(divelem.style.display=='none')
                            {{                            
                                divelem.style.display = '';
                                elem.innerText = '{3}';
                            }}
                            else
                            {{
                                divelem.style.display = 'none'; 
                                elem.innerText = '{2}';                                  
                            }}; 
                            return false; 
                        ",
                    hiddenControl.ClientID, lb.ClientID, ButtonShowText, ButtonHideText);
            }
        }

        public static void FindControlsSet(Control ControlsContainer, string lbtnID, string PopupControlID, string ButtonID)
        {
            var ctrl = ControlHelper.FindControlRecursive(ControlsContainer, lbtnID);
            var pcCtrl = ControlHelper.FindControlRecursive(ControlsContainer,PopupControlID);
            var btnCtrl = ControlHelper.FindControlRecursive(ControlsContainer, ButtonID);
            if (ctrl != null && pcCtrl != null)
            {
                var lbtn = (LinkButton) ctrl;
                lbtn.OnClientClick = lbtn.Enabled ? ShowEditor(pcCtrl.ClientID) : "return false;";
            }
            if (btnCtrl != null && pcCtrl != null)
                ((Button)btnCtrl).OnClientClick = HideEditor(pcCtrl.ClientID);

        } 
        public static void FindControlsSet(Control ControlsContainer, string lbtnID, string onClientClick)
        {
            var ctrl = ControlHelper.FindControlRecursive(ControlsContainer, lbtnID);
            if (ctrl == null) return;
            var lbtn = (LinkButton) ctrl;
            lbtn.OnClientClick = lbtn.Enabled ? onClientClick : "return false;";
        }
    }
}