using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Compilation;
using System.Web.Script.Serialization;
using System.Web.Services;
using Nat.Web.Controls.GenerationClasses;

namespace Nat.Web.Controls
{
    /// <summary>
    /// Summary description for WebServiceAdditinalFields
    /// </summary>
    [WebService(Namespace = "http://tempuri.org/")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    [System.ComponentModel.ToolboxItem(false)]
    [System.Web.Script.Services.ScriptService]
    public class WebServiceAdditinalFields : WebService
    {
        [WebMethod]
        public string GetNameOfValue(string contextKey)
        {
            string[] split = contextKey.Split(',');
            if (split.Length < 6) return "Erorr in contextKey";
            long? idValue = string.IsNullOrEmpty(split[0]) ? null : (long?)Convert.ToInt64(split[0]);
            string nameOfColumn = split[1];
            bool isSecond = string.IsNullOrEmpty(split[2]) ? true : Convert.ToBoolean(split[2]);
            bool isKz = string.IsNullOrEmpty(split[3]) ? false : Convert.ToBoolean(split[3]);
            var additinalFields = (IAdditionalFields)Activator.CreateInstance(BuildManager.GetType(split[4], true, true), null);
            string value = split[5];

            //todo: переключение языка потока

            if (!additinalFields.CheckPermit()) return "You have not permition";

            var jss = new JavaScriptSerializer();
            var values = additinalFields.GetNameOfValue(idValue, value, nameOfColumn, isSecond, isKz);
            return jss.Serialize(values);
        }
    }
}
