using System;
using System.Linq;
using System.Web.Services;
using System.Web.Script.Services;
using Nat.Tools.Specific;
using Nat.Web.Controls.Data;
using Nat.Web.Controls.GenerationClasses.Data;
using Nat.Web.Controls.Properties;
using Nat.Web.Tools;
using Nat.Web.Tools.Initialization;
using Nat.Web.Tools.Security;

namespace Nat.Web.Controls.GenerationClasses
{
    /// <summary>
    /// Summary description for WebServiceSavedJournalSettings
    /// </summary>
    [WebService(Namespace = "http://tempuri.org/")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    [System.ComponentModel.ToolboxItem(false)]
    [ScriptService]
    public class WebServiceSavedJournalSettings : WebService
    {
        [ScriptMethod]
        [WebMethod(EnableSession=false)]
        public bool DeleteSavedSettings(string id)
        {
            if (string.IsNullOrEmpty(id))
                return false;
            //note: проверка прав внутри функции
            return RvsSavedProperties.DeleteProperties(Convert.ToInt64(id));
        }

        [ScriptMethod]
        [WebMethod(EnableSession = false)]
        public string StillEditCrossData(string journalName, string rowID)
        {
            WebInitializer.Initialize();
            var db = new CrossJournalDataContext(SpecificInstances.DbFactory.CreateConnection());
            var rusult = db.SYS_UpdateCrossJournalEdits(Tools.Security.User.GetSID(), journalName, rowID).FirstOrDefault();
            LocalizationHelper.SetThreadCulture();
            if (!rusult.Result.Value) return Resources.SEndCrossDataEdit;
            return "";
        }
    }
}