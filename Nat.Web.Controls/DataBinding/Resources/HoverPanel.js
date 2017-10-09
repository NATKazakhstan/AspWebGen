var IsMSIE = navigator.userAgent.indexOf('MSIE') > -1 ? true : false;

function StartCallback(ControlId,event,queryString,postData)
{
    var Context = eval( ControlId + '_Context' );
    var Url=eval(ControlId + '_Url');

    if (event != null)  
    {
        Context.LastMouseTop = event.clientY;  
        Context.LastMouseLeft = event.clientX;
    }
    else 
    {  Context.LastMouseTop = 0;  }
 
    if (queryString == null)
        queryString = '';
        
    if (Context.PostBackMode == 'GET' )
    {
        if (queryString != '')
            queryString = '&' + queryString;
        
        queryString = '__WWEVENTCALLBACK=' + Context.ControlId + queryString;
    }

    if (queryString != '')
    {
        if (Url.indexOf('?') > -1)
            Url = Url + "&" + queryString
        else
            Url =  Url + '?' + queryString;   
    }
        
    Context.Url = Url;
    Context.PostData = postData;
    
     if (Context.EventHandlerMode == 'ShowIFrameAtMousePosition' || 
         Context.EventHandlerMode == 'ShowIFrameInPanel')         
     {  
          setTimeout("ShowIFrame('" + ControlId + "');",Context.NavigateDelay);
          return;
     }

    if (Context.NavigateDelay > 0)
    {
        var Cmd = "GetServerUrl(eval('" + ControlId + "_Context'))";
        setTimeout(Cmd,Context.NavigateDelay);
    }
    else
        GetServerUrl( Context );
}
function HandleCallback(ControlId)
{
    var Context = eval(ControlId + '_Context');
    var Http = Context.Http;

    if (Http == null || Http.readyState != 4) 
        return;

    var Result = '';
    var ErrorException = null;

    Result = Http.responseText;
    if (Http.status != 200) 
        ErrorException = new CallbackExceptionObject(Http.statusText);

    Http = null;

    if (Context.EventHandlerMode == 'ShowHtmlAtMousePosition')
    {
    
        if (Context.LastMouseTop != -1)
        {
            var Panel = $(Context.ControlId);
            Panel.innerHTML = Result;
            MovePanelToPosition(Context);
        }
    }
    else if (Context.EventHandlerMode == 'ShowHtmlInPanel' )
    {
            var Panel = $(Context.ControlId);
            Panel.style.display = '';
            Panel.innerHTML = Result;
    }
    else
    {
        var FinalResult = Result;
        if (Context.EventHandlerMode == 'CallPageMethod')
        {
            if (ErrorException)    
            {
                Context.Callback(ErrorException);
                return;
            }
            FinalResult = eval( '(' + Result + ')');
         }
        Context.Callback(FinalResult);
    }
}
function ShowIFrame(ControlId)
{
    var Context = eval( ControlId + "_Context");
    $(ControlId + '_IFrame').src= Context.Url;
    if (Context.EventHandlerMode == 'ShowIFrameAtMousePosition')
         MovePanelToPosition(Context);
    else
    {
         $(ControlId).style.display='';
         //ShowPanel(ControlId);
    }
         
    return;
}
// *** x can be passed as an event object
function MovePanelToPosition(Context,x,y)
{
    try
    {
        var Panel = $(Context.ControlId);
        Panel.style.position = 'absolute';    
        if (typeof(x) == "object")
        {
            Context.LastMouseTop = x.clientY;  
            Context.LastMouseLeft = x.clientX;
        }
        else if (typeof(x) == "number")
        {
            Context.LastMouseTop = y;
            Context.LastMouseLeft = x;
        }
       
        var Top = 0;
        var ScrollTop = document.body.scrollTop;

        if (ScrollTop == 0)
        {
            if (window.pageYOffset)
                ScrollTop = window.pageYOffset;
            else
                ScrollTop = (document.body.parentElement) ? document.body.parentElement.scrollTop : 0;
        }

        Top = ScrollTop + Context.LastMouseTop + 3;
        
        Panel.style.top = Top.toString() + 'px';
        Panel.style.left = (Context.LastMouseLeft + 2).toString() + 'px';
        Panel.style.display = '';
        
        if (Context.AdjustWindowPosition && document.body) 
        {
            var mainHeight = 0; 
            if( typeof( window.innerWidth ) == 'number' ) 
                mainHeight = window.innerHeight; 
            else if( document.documentElement && document.documentElement.clientHeight ) 
                mainHeight = document.documentElement.clientHeight; 
            else if( document.body && document.body.clientHeight ) 
                mainHeight = document.body.clientHeight; 

            if ( mainHeight < Panel.clientHeight ) 
                Top = ScrollTop; 
            else 
            {
                if ( mainHeight < Context.LastMouseTop + Panel.clientHeight ) 
                   Top = mainHeight - Panel.clientHeight - 10 + ScrollTop; 
            } 
            Panel.style.top = Top.toString() + "px";
        }
        if (Context.ShadowOffset != 0) 
             ShowShadow(Panel.id,Context.ShadowOpacity,Context.ShadowOffset,true);
    }
    catch( e )
    {
         window.status =  'Moving of window failed: ' +  e.message  ;
    }

}
function HidePanel(ControlId) 
{
    var Context = eval( ControlId + "_Context");
    if (Context == null)
        return;
        
    Context.LastMouseTop = -1;
    HideShadowControl(Context.ControlId);    
}         
function ShowPanel(ControlId)
{
    var Context = eval( ControlId + "_Context");
    if (Context == null)
        return;
    
    $(Context.ControlId).style.display='';
    if (Context.ShadowOffset > 0)
        ShowShadow(Context.ControlId,Context.ShadowOpacity,Context.ShadowOffset);
}
function CallMethod(ControlId,MethodName,Parm1,Parm2,Parm3,Parm4,Parm5,Parm6,Parm7,Parm8)
{    
    var Parameters = 'CallbackMethod=' + MethodName + '&';
   
    var ParmCount = arguments.length;
    for (var x = 2; x < ParmCount-1; x++)
    {
        Parameters = Parameters + 'Parm' + (x-1).toString() + '=' + EncodeValue(JSON.serialize(arguments[x]).toString()) + '&';
    }
    Parameters = Parameters + 'CallbackParmCount=' + (ParmCount-3).toString();

    var Context = eval(ControlId + '_Context');
    Context.Callback = arguments[ParmCount-1]; 
    Context.LastMouseTop = 0;
    
    if (Context.PostBackMode == 'GET')
        StartCallback(ControlId,null,Parameters,null,1);
    else
        StartCallback(ControlId,null,null,Parameters,1);
    
} 
function CallbackContext(Url,Callback,ControlId,
                         NavigateDelay,PostBackMode,EventHandlerMode,
                         FormName,AdjustWindowPosition,PanelShadowOffset,
                         PanelShadowOpacity,PanelOpacity)
{
    if (NavigateDelay == null)
        NavigateDealy = 1;

    if (PostBackMode == null)
        PostBackMode = 'GET'; // POST , POSTNoViewState

    if (EventHandlerMode == null)
        EventHandlerMode = 'ShowHtmlAtMousePosition';
    
    if (AdjustWindowPosition == null)
        AdjustWindowPosition = false;
    
    if (PanelShadowOffset == null)
        PanelShadowOffset = 0;
    
    if (PanelShadowOpacity == null)
        PanelShadowOpacity = 0;
     
    if (PanelOpacity == null)
        PanelOpacity == 1.0;
    
    this.ControlId = ControlId;
    this.Http = GetXmlHttp();

    this.Url = Url;
    this.NavigateDelay = NavigateDelay;
    this.PostData = null;
    this.FormName = FormName;
    this.Callback = Callback;
    this.Method = '';
    this.Async = true;
    
    this.PostBackMode = PostBackMode;
    this.EventHandlerMode = EventHandlerMode; // CallPageMethod,CallExternalUrl

    this.LastMouseLeft = 0;
    this.LastMouseTop = -1;
    this.AdjustWindowPosition = AdjustWindowPosition;
    this.ShadowOffset = PanelShadowOffset;
    this.ShadowOpacity = PanelShadowOpacity;
}
function CallbackExceptionObject(Message)
{
    this.IsCallbackError = true;
    this.Message = Message;
}
function CallbackErrorResponse(Context,Message)
{
    Context.Callback( new CallbackExceptionObject(Message) );
}
function GetXmlHttp() 
{
 	var Http = null;
	if (typeof XMLHttpRequest != "undefined") 
    {
		Http = new XMLHttpRequest();
	}
    else 
    {
		try 
        {
			Http = new ActiveXObject("Msxml2.XMLHTTP");
		} catch (e) 
        {
			try 
            {
				Http = new ActiveXObject("Microsoft.XMLHTTP");
			} 
			catch (e) {}
		}
	}
	return Http;
}
function GetServerUrl(Context) 
{
    if (Context == null)
        return;

    if (Context.LastMouseTop == -1)
		return;
		
    try 
    {   
        var Http = GetXmlHttp();
        if (Http == null)
        {
            CallbackErrorResponse(Context,'Unable to create XmlHttp object');
            return;
        }
        Context.Http = Http;

        if (Context.Callback)
            Http.onreadystatechange = eval(Context.ControlId + '_HandleCallback');

        if (Context.PostBackMode != 'GET')
        {
            Http.open('POST',Context.Url,Context.Async);	
            Context.Async = true;
            Http.setRequestHeader('Pragma','no-cache');
            Http.setRequestHeader('Content-type','application/x-www-form-urlencoded');

            var PostBuffer = '';
            if (Context.PostBackMode != 'POSTMethodParametersOnly')
                PostBuffer = EncodeFormVars(Context);
            else
                PostBuffer = '__WWEVENTCALLBACK=' + Context.ControlId + '&';
            
            if (Context.PostData)
                PostBuffer = PostBuffer + Context.PostData;
                    
            Http.setRequestHeader('Content-length',PostBuffer.length.toString() );
            Http.send(PostBuffer);
        }
        else 
        {
		    Http.open('GET',Context.Url,Context.Async);	
		    Context.Async = true;
            Http.setRequestHeader('Pragma','no-cache');
		    Http.send(null);
        }
    }
    catch( e )
    {
        CallbackErrorResponse(Context,"XmlHttp Error: " + e.Message);
    } 
}
function EncodeFormVars(Context) 
{
    var PostData = '__WWEVENTCALLBACK=' + Context.ControlId + '&';
    
   Form = document.forms[Context.FormName]; 
    if (Form == null)
        Form = document.forms[0];
    if (Form == null)
        return PostData;

    var count = Form.length;
    var element;

    for (var i = 0; i < count; i++) 
    {
        element = Form.elements[i];
        var tagName = element.tagName.toLowerCase();
        if (tagName == 'input') 
        {
            var type = element.type;
            
            if (Context.PostBackMode == 'POSTNoViewstate')
            {
                // *** Don't send ASP.NET gunk
                if (element.name == '__VIEWSTATE' || element.name == '__EVENTTARGET' || 
                    element.name == '__EVENTARGUMENT' || element.name == '__EVENTVALIDATION')
                    continue;
            }
            if (type == 'text' || type == 'hidden' || type == 'password' ||
               ((type == 'checkbox' || type == 'radio') && element.checked  )) 
                PostData += element.name + '=' + EncodeValue(element.value) + '&';
        }
        else if (tagName == 'select') 
        {
            if (element.options == null)
                continue;
            var selectCount = element.options.length;
            for (var j = 0; j < selectCount; j++) 
            {
                var selectChild = element.options[j];
                if (selectChild.selected) 
                    PostData += element.name + '=' + EncodeValue(selectChild.value) + '&';
            }
        }
        else if (tagName == 'textarea') 
            PostData += element.name + '=' + EncodeValue(element.value) + '&';
    }
    return PostData;
}
function EncodeValue(parameter) {
    if (encodeURIComponent) 
        return encodeURIComponent(parameter);
    return escape(parameter);
}
function $(ElementId)
{
    return document.getElementById(ElementId);
}
function ShowToolTip(ControlId,Message,Timeout,Position)
{
    var Ctl = GetControlInstance(ControlId);
    if (Ctl == null)
        return;
    
    ControlId = Ctl.id;
    
    var ToolTip = $(ControlId + "_ToolTip");
    if ( ToolTip != null)
    {
        // Remove the nodes to get steady startup environment
        Ctl.parentNode.removeChild(ToolTip);
        Ctl.parentNode.removeChild( $(ControlId + "_ToolTipShadow"));
        ToolTip = null;
    }

    ToolTip = document.createElement('div');
    Ctl.parentNode.appendChild(ToolTip);
        
    if ( Position == null)
        Position = "BottomLeft";

    var Style=ToolTip.style;    
    Style.display = '';

    ToolTip.id = ControlId + "_ToolTip";
    Style.background = "cornsilk";
    Style.color = "black";
    Style.borderWidth="1px";
    Style.borderStyle="solid";
    Style.borderColor="gray"; 
    Style.padding="2px";
    Style.fontSize = "8pt";
    Style.fontWeight = "normal";

    var OldPosition = Ctl.style.position;

    ToolTip.innerHTML = Message;
    Style.position = "absolute";
    Ctl.style.position = "absolute";
   
    var CtlBounds = GetControlBounds(Ctl);
   
    var Left = CtlBounds.x + 10;
    var Top = CtlBounds.y + CtlBounds.height - 5;
    if( Position == "BottomRight")
    {
        Left = CtlBounds.x + CtlBounds.width - 10;
        Top = CtlBounds.y + CtlBounds.height - 5;
    }
    else if (Position == "TopLeft")
    {
        Left = CtlBounds.x + 2;
        Top = CtlBounds.y + 2;
    }
    else if( Position == "Mouse")
    {
        if (window.event) 
        {
            Left = window.event.clientX;
            Top = window.event.clientY;
        }
    }
    
    var Width = ToolTip.clientWidth;  // Message.length * 6.5;
    if (Width > 400)
        Width=400;
       
    Style.left = Left + "px";
    Style.top = Top + "px";
    Style.width= Width + "px";  // subtract padding/border

    ShowShadow(ToolTip.id,.30,2);

    Ctl.style.position = OldPosition;    
     
    if (Timeout && Timeout > 0)
        window.setTimeout("HideToolTip('" + ControlId + "');",Timeout);
}
function HideToolTip(ControlId)
{
    var Ctl = $(ControlId + "_ToolTip") ;
    if (Ctl == null)
       return;
 
    Fadeout(Ctl.id,true,3,100);
    
    Ctl = $(Ctl.id + "Shadow");
    if (Ctl == null)
       return;
    Fadeout(Ctl.id,true,2,30);   
}
function ShowShadow(CtlId,Opacity,Offset,DelayShadow)
{
   var Ctl = $(CtlId);
   if (Ctl == null)
      return;

   if (Opacity == null)
      Opacity = ".35";
   if (Offset == null)
      Offset = 8;
      
   if (DelayShadow != null)
   { // Force a delay to allow target control to resize properly
     window.setTimeout("ShowShadow('" + CtlId + "'," + Opacity.toString() + "," + Offset.toString() + ",null)",50);
     return;
   }

   Ctl.style.position = 'absolute';
   var Bounds = GetControlBounds(Ctl); 
      
   if (Opacity == null)
      Opacity = ".35";
   if (Offset == null)
      Offset = 8;
   
   var Shadow = $(CtlId + 'Shadow');
   if (Shadow == null)
   {
       Shadow = document.createElement('div');
       Shadow.id = CtlId + 'Shadow';
       Ctl.parentNode.appendChild(Shadow);
       Shadow.style.position='absolute';
       Shadow.style.background = 'black';
       
       SetOpacity(Shadow,Opacity);

       Ctl.style.zIndex = 99;
       Shadow.style.zIndex = 98;
   }
   
   Shadow.style.display = '';
   Shadow.style.top = Bounds.y +  Offset + "px";
   Shadow.style.left = Bounds.x + Offset + "px";
   Shadow.style.width = Bounds.width + "px";
   Shadow.style.height = Bounds.height + "px";
}
function HideShadowControl(CtlId,HideShadowOnly)
{
    var Ctl = $(CtlId);
    if (HideShadowOnly == null || HideShadowOnly == false)
    {
        if (Ctl == null)
            return;
        Ctl.style.display = 'none';
    }
    
    Ctl = $(CtlId + "Shadow");
    if (Ctl == null)
       return;
    Ctl.style.display = 'none';
}
function Fadeout(ControlId,HideOnZero,Step,Percent)
{
    var Ctl = GetControlInstance(ControlId);
    if (Ctl == null)
        return;

    if (HideOnZero == null)
        HideOnZero = false;
    if (Step == null)
       Step = 4;
    if (Percent == null)
       Percent = 100;
        
    if (Percent < 1 && HideOnZero == true)
    {
        Ctl.style.display='none';
        return;    
    }
                
    SetOpacity(Ctl,Percent / 100);
      
    if (Percent <= 0)
        return;
    
    Percent = Percent - Step;
    
    // *** Set up next fade
    window.setTimeout("Fadeout('" + ControlId + "'," + HideOnZero.toString() + "," + Step.toString() + "," + Percent.toString() + ")",30);
}
function SetOpacity(ControlId,Percent)
{
   var Ctl = GetControlInstance(ControlId);
   if (Ctl == null)
          return;
      
   if (IsMSIE)
        Ctl.style.filter = "alpha(opacity='" + Percent * 100 + "')";
   else
       Ctl.style.opacity = Percent;
}
function GetControlInstance(ControlId)
{
   var Ctl = null;
   if (typeof(ControlId) == "object")
      return ControlId;

   Ctl = $(ControlId);
   if (Ctl == null)
       return null;

   return Ctl; 
}
function GetControlLocation(element) 
{
    var offsetX = 0;
    var offsetY = 0;
    var parent;
    
    for (parent = element; parent; parent = parent.offsetParent) {
        if (parent.offsetLeft) {
            offsetX += parent.offsetLeft;
        }
        if (parent.offsetTop) {
            offsetY += parent.offsetTop;
        }
    }

    return { x: offsetX, y: offsetY };
}
function GetControlBounds(element) {
    var offset = GetControlLocation(element);
    
    var width = element.offsetWidth;
    var height = element.offsetHeight;
    
    return { x: offset.x, y: offset.y, width: width, height: height };
}
function ShowAtPosition(ControlId,x,y)
{
    var Ctl = $(ControlId);
    if (Ctl == null)
        return;

    Ctl.style.position="absolute";
    Ctl.style.display="";

    Ctl.style.left = x + "px";
    Ctl.style.top = y + "px";
}
/*
Copyright (c) 2005 JSON.org
Modifications by Rick Strahl
----------------------------
* Added support for dates in object parser

    The global object JSON contains two methods.

    JSON.stringify(value) takes a JavaScript value and produces a JSON text.
    The value must not be cyclical.

    JSON.parse(text) takes a JSON text and produces a JavaScript value. It will
    return false if there is an error.
*/
var JSON = {
    copyright: '(c)2005 JSON.org',
    license: 'http://www.crockford.com/JSON/license.html',
/*
    Stringify a JavaScript value, producing a JSON text.
*/
    serialize: function (v) {
        var a = [];
/*
    Emit a string.
*/
        function e(s) {
            a[a.length] = s;
        }

/*
    Convert a value.
*/
        function g(x) {
            var b, c, i, l, v;

            switch (typeof x) {
            case 'string':
                e('"');
                if (/["\\\x00-\x1f]/.test(x)) {
                    l = x.length;
                    for (i = 0; i < l; i += 1) {
                        c = x.charAt(i);
                        if (c >= ' ') {
                            if (c == '\\' || c == '"') {
                                e('\\');
                            }
                            e(c);
                        } else {
                            switch (c) {
                            case '\b':
                                e('\\b');
                                break;
                            case '\f':
                                e('\\f');
                                break;
                            case '\n':
                                e('\\n');
                                break;
                            case '\r':
                                e('\\r');
                                break;
                            case '\t':
                                e('\\t');
                                break;
                            default:
                                c = c.charCodeAt();
                                e('\\u00' +
                                    Math.floor(c / 16).toString(16) +
                                    (c % 16).toString(16));
                            }
                        }
                    }
                } else {
                    e(x);
                }
                e('"');
                return;
            case 'number':
                e(isFinite(x) ? x : 'null');
                return;
            case 'object':
                if (x) {
                    // RAS: Added Date Parsing
                    if (x.toUTCString) 
                       return e('new Date(' +  x.getUTCFullYear() + ',' + x.getUTCMonth() + ',' + x.getUTCDate() + ',' + x.getUTCHours() + ',' + x.getUTCMinutes() + ',' + x.getUTCSeconds() + ',' + x.getUTCMilliseconds() + ')' );

                    if (x instanceof Array) {
                        e('[');
                        l = a.length;
                        for (i = 0; i < x.length; i += 1) {
                            v = x[i];
                            if (typeof v != 'undefined' &&
                                    typeof v != 'function') {
                                if (b) {
                                    e(',');
                                }
                                g(v);
                                b = true;
                            }
                        }
                        e(']');
                        return;
                    } else if (typeof x.valueOf == 'function') {
                        e('{');
                        l = a.length;
                        for (i in x) {
                            v = x[i];
                            if (typeof v != 'undefined' &&
                                    typeof v != 'function' &&
                                    (!v || typeof v != 'object' ||
                                    typeof v.valueOf == 'function')) {
                                if (b) {
                                    e(',');
                                }
                                g(i);
                                e(':');
                                g(v);
                                b = true;
                            }
                        }
                        return e('}');
                    }
                }
                e('null');
                return;
            case 'boolean':
                e(x);
                return;
            default:
                e('null');
                return;
            }
        }
        g(v);
        return a.join('');
    },
/*
    Parse a JSON text, producing a JavaScript value.
    It returns false if there is a syntax error.
*/
    parse: function (text) {
        try {
            return !(/[^,:{}\[\]0-9.\-+Eaeflnr-u \n\r\t]/.test(
                    text.replace(/"(\\.|[^"\\])*"/g, ''))) &&
                    eval('(' + text + ')');
        } catch (e) {
            return false;
        }
    }
};
