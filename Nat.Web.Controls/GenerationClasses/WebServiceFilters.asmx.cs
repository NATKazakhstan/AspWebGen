using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web;
using System.Web.Script.Services;
using System.Web.Services;

namespace Nat.Web.Controls
{
    /// <summary>
    /// Summary description for WebServiceFilters
    /// </summary>
    [WebService(Namespace = "http://tempuri.org/")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    [ToolboxItem(false)]
    [ScriptService]
    public class WebServiceFilters : WebService
    {
        [WebMethod(EnableSession = true)]
        [ScriptMethod]
        public string SetFilterValue(string filterValue)
        {
            return SetFilterValue(filterValue, Guid.NewGuid().ToString());
        }

        private string SetFilterValue(string filterValue, string guid)
        {
            HttpContext.Current.Session[guid] = filterValue;
            return guid;
        }
    }
}
