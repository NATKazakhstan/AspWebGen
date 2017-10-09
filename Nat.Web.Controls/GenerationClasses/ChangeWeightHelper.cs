/*
* Created by: Sergey V. Shpakovskiy
* Created: 2012.05.28
* Copyright © JSC NAT Kazakhstan 2012
*/

namespace Nat.Web.Controls.GenerationClasses
{
    using System;
    using System.Text;
    using System.Web;
    using System.Web.UI;

    using Nat.Web.Controls.Properties;

    public delegate void ChangeWeightHandler(long id, int weight);

    public delegate void ChangeWeightHandler<T>(T key, int weight);

    public class ChangeWeightHelper
    {
        private const string MoveUpButton1 = "moveUp1:";
        private const string MoveDownButton1 = "moveDown1:";
        private const string MoveUpButton2 = "moveUp2:";
        private const string MoveDownButton2 = "moveDown2:";

        public static void AddAdditionalButtons(AdditionalButtons buttons, ref bool isFirstButton, long? selectedValueKey)
        {
            if (isFirstButton)
            {
                buttons.AddCustom("<br/><br/><div><input type=text id='ChangeWeight1' name='ChangeWeight1' size=2 maxLength=2 value='1' style='vertical-align: top;' />&nbsp;");
                buttons.AddImageButton(MoveUpButton1 + selectedValueKey, Themes.IconUrlArrowUp, Resources.SMoveUpWeight);
                buttons.AddCustom("&nbsp;");
                buttons.AddImageButton(MoveDownButton1 + selectedValueKey, Themes.IconUrlArrowDown, Resources.SMoveDownWeight);
                buttons.AddCustom("</div>");
            }
            else
            {
                buttons.AddCustom("<br/><br/><div><input type=text id='ChangeWeight2' name='ChangeWeight2' size=2 maxLength=2 value='1' style='vertical-align: top;' />&nbsp;");
                buttons.AddImageButton(MoveUpButton2 + selectedValueKey, Themes.IconUrlArrowUp, Resources.SMoveUpWeight);
                buttons.AddCustom("&nbsp;");
                buttons.AddImageButton(MoveDownButton2 + selectedValueKey, Themes.IconUrlArrowDown, Resources.SMoveDownWeight);
                buttons.AddCustom("</div>");
            }

            isFirstButton = false;
        }

        public static string MoveWeightButton(string argument, ChangeWeightHandler handlerDown, ChangeWeightHandler handlerUp)
        {
            var message = MoveWeightButton(argument, MoveDownButton1, "ChangeWeight1", handlerDown);
            if (message != null) return message;

            message = MoveWeightButton(argument, MoveDownButton2, "ChangeWeight2", handlerDown);
            if (message != null) return message;

            message = MoveWeightButton(argument, MoveUpButton1, "ChangeWeight1", handlerUp);
            if (message != null) return message;

            message = MoveWeightButton(argument, MoveUpButton2, "ChangeWeight2", handlerUp);
            return message;
        }

        private static string MoveWeightButton(string argument, string argumentName, string fieldName, ChangeWeightHandler handler)
        {
            if (argument.StartsWith(argumentName) && !string.IsNullOrEmpty(HttpContext.Current.Request.Form[fieldName]))
            {
                int changeWeight;
                if (!int.TryParse(HttpContext.Current.Request.Params[fieldName], out changeWeight))
                    return Resources.SNotCorrectNumberForWeight;
                long refSP = Convert.ToInt64(argument.Substring(argumentName.Length));
                handler(refSP, changeWeight);
            }

            return null;
        }

        public static void RenderLink(StringBuilder sb, string key, string controlKey)
        {
            sb.AppendFormat(
                @"
<a href=""javascript:$get('keyMove{0}').value = '{1}'; $('#MoveDown{0}').dialog('close'); $('#MoveUp{0}'  ).dialog('open'); void(0);""><img style='border:0' alt='{2}' src='{3}' /></a>
<a href=""javascript:$get('keyMove{0}').value = '{1}'; $('#MoveUp{0}'  ).dialog('close'); $('#MoveDown{0}').dialog('open'); void(0);""><img style='border:0' alt='{4}' src='{5}' /></a>",
                controlKey,
                key,
                Resources.SMoveUpRecord,
                Themes.IconUrlArrowUp,
                Resources.SMoveDownRecord,
                Themes.IconUrlArrowDown);
        }
        
        public static bool ExecuteFunction<T>(string argument, string controlKey, ChangeWeightHandler<T> handlerDown, ChangeWeightHandler<T> handlerUp, Action<string> errorCallback)
        {
            var keyString = HttpContext.Current.Request.Form["keyMove" + controlKey];
            var weightKey = HttpContext.Current.Request.Form["valueMove" + controlKey];

            if (argument.StartsWith(MoveUpButton1))
            {
                if (string.IsNullOrEmpty(keyString) || string.IsNullOrEmpty(weightKey))
                    return true;

                ExecuteFunction(keyString, weightKey, handlerUp, errorCallback);
                return true;
            } 

            if (argument.StartsWith(MoveDownButton1))
            {
                if (string.IsNullOrEmpty(keyString) || string.IsNullOrEmpty(weightKey)) 
                    return true;

                ExecuteFunction(keyString, weightKey, handlerDown, errorCallback);
                return true;
            }

            return false;
        }

        private static void ExecuteFunction<T>(string keyString, string weightKey, ChangeWeightHandler<T> handler, Action<string> errorCallback)
        {
            T id;
            int weight;
            try
            {
                id = (T)Convert.ChangeType(keyString, typeof(T));
            }
            catch (Exception e)
            {
                errorCallback(e.ToString());
                return;
            }

            try
            {
                weight = Convert.ToInt32(weightKey);
            }
            catch (FormatException e)
            {
                errorCallback(Resources.SNotCorrectNumber);
                return;
            }

            handler(id, weight);
        }

        public static void RenderForm(StringBuilder sb, Control postControl, string controlKey)
        {
            RenderForm(sb, postControl, controlKey, MoveUpButton1, MoveDownButton1);
        }

        public static void RenderForm(StringBuilder sb, Control postControl, string controlKey, string argumentUp, string argumentDown)
        {
            var upScript = postControl.Page.ClientScript.GetPostBackEventReference(postControl, argumentUp);
            var downScript = postControl.Page.ClientScript.GetPostBackEventReference(postControl, argumentDown);

            sb.AppendFormat(
                @"
<input type=hidden id='keyMove{0}' name='keyMove{0}' />
<input type=hidden id='valueMove{0}' name='valueMove{0}' />
<div id='MoveUp{0}' title='{1}' class='ui-dialog' style='display:none'>
    <div>
        <span>{5}</span>
        <input type=text name='valueMoveUp{0}' id='valueMoveUp{0}' size=2 maxLength=2 value='1' style='vertical-align: top;' />
    </div>
</div>
<div id='MoveDown{0}' title='{2}' class='ui-dialog' style='display:none'>
    <div>
        <span>{6}</span>
        <input type=text name='valueMoveDown{0}' id='valueMoveDown{0}' size=2 maxLength=2 value='1' style='vertical-align: top;' />
        <input type=hidden name='keyMoveDown{0}' id='keyMoveDown{0}' />
    </div>
</div>
<Script>
    $(function() {{ 
        $('#MoveUp{0}').dialog({{ autoOpen: false, width: 450, height:200 }});
        $('#MoveUp{0}').dialog( 'option', 'buttons', {{ 
            '{1}': function() {{ 
                $get('valueMove{0}').value = $get('valueMoveUp{0}').value; 
                {3}; 
            }}
        }} );
        $('#MoveDown{0}').dialog({{ autoOpen: false, width: 450, height:200 }});
        $('#MoveDown{0}').dialog( 'option', 'buttons', {{ 
            '{2}': function() {{ 
                $get('valueMove{0}').value = $get('valueMoveDown{0}').value; {4}; 
            }} 
        }} );
    }});
</Script>
",
                controlKey,
                Resources.SMoveUpRecord,
                Resources.SMoveDownRecord,
                upScript,
                downScript,
                Resources.SMoveUpRecordOn,
                Resources.SMoveDownRecordOn);
        }
    }
}
