using System;
using System.Linq;
using System.Web;
using System.Web.Script.Serialization;
using System.Web.UI;
using Nat.Tools.Specific;
using Nat.Web.Controls.Data;
using Nat.Web.Controls.GenerationClasses.BaseJournal;
using Nat.Web.Controls.Properties;
using Nat.Web.Tools;
using Nat.Web.Tools.Security;

namespace Nat.Web.Controls.GenerationClasses
{
    public partial class SavingJournalSettings : UserControl
    {
        private ScriptManager _ScriptManager;
        public ScriptManager ScriptManager
        {
            get
            {
                if (_ScriptManager == null)
                    _ScriptManager = ScriptManager.GetCurrent(Page);
                return _ScriptManager;
            }
        }

        public BaseJournalControl Journal { get; set; }

        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);

            ScriptManager.Services.Add(new ServiceReference { Path = "/WebServiceSavedJournalSettings.asmx", });

            #region load

            _ppcLoad.HeaderText = Resources.SLoadViewSettingsHeader;
            _ppcLoad.OkControlID = ClientID + "_OkLoad";
            _ppcLoad.CancelControlID = ClientID + "_CancelLoad";
            var postback = Page.ClientScript.GetPostBackEventReference(
                new PostBackOptions(Journal, "{0}")
                    {
                        ValidationGroup = "savedLoad",
                        PerformValidation = true,
                    });

            var argument = string.Format("getArgumentsForLoadViewSettings('{0}', '{1}', '{2}')",
                                         BaseJournalControl.LoadViewSettings,
                                         _chkLoadFilters.ClientID,
                                         _ddlLoadFilters.ClientID);
            _ppcLoad.OnOkScript = postback.Replace("'{0}'", argument).Replace("\"{0}\"", argument);
            //_ppcLoad.OnCancelScript = string.Format("javascript:cancelDeleteSavedViewSettings('{0}', '{1}');", _ddlLoadFilters.ClientID, _hfDeleteViewSettings.ClientID);
            _hlDelSavedInOpen.Attributes["onclick"] = string.Format("deleteSavedViewSettings(this, '{0}', '{1}', '{2}');", _hlDelSavedInSave.ClientID, _ddlLoadFilters.ClientID, _ddlSaveFilters.ClientID);

            #endregion

            #region save

            _ppcSave.HeaderText = Resources.SSaveViewSettingsHeader;
            _ppcSave.OkControlID = ClientID + "_OkSave";
            _ppcSave.CancelControlID = ClientID + "_CancelSave";
            postback = Page.ClientScript.GetPostBackEventReference(
                new PostBackOptions(Journal, "{0}")
                    {
                        ValidationGroup = "",
                        PerformValidation = true,
                    });
            argument = string.Format("getArgumentsForSaveViewSettings('{0}', '{1}', '{2}', '{3}', '{4}', '{5}')",
                                     BaseJournalControl.SaveViewSettings,
                                     _chkSaveFilters.ClientID,
                                     _tbSaveNameRu.ClientID,
                                     _tbSaveNameKz.ClientID,
                                     _ddlSaveFilters.ClientID,
                                     _chkSaveAsShared.ClientID);
            _ppcSave.OnOkScript = postback.Replace("'{0}'", argument).Replace("\"{0}\"", argument);
            //_ppcLoad.OnCancelScript = string.Format("javascript:cancelDeleteSavedViewSettings('{0}', '{1}');", _ddlSaveFilters.ClientID, _hfDeleteViewSettings.ClientID);
            _hlDelSavedInSave.Attributes["onclick"] = string.Format("deleteSavedViewSettings(this, '{0}', '{1}', '{2}');", _hlDelSavedInOpen.ClientID, _ddlSaveFilters.ClientID, _ddlLoadFilters.ClientID);

            #endregion

            #region raisePostBack

            if (HttpContext.Current.Request.Form["__EVENTTARGET"] == Journal.UniqueID
                && HttpContext.Current.Request.Form["__EVENTARGUMENT"].StartsWith(
                       BaseJournalControl.LoadViewSettings, StringComparison.OrdinalIgnoreCase))
            {
                var arguments = HttpContext.Current.Request.Form["__EVENTARGUMENT"].
                    Substring(BaseJournalControl.LoadViewSettings.Length);
                LoadSettings(Journal, arguments, Journal.ParentUserControl.LogMonitor, Journal.ParentUserControl.LoadSettingsLog);
            }

            #endregion

            _lRewriteSettings.Text = Resources.SRewriteSettings;
            _chkSaveFilters.Text = Resources.SSaveFilters;
            _chkLoadFilters.Text = Resources.SLoadFilters;
            _chkSaveAsShared.Text = Resources.SShowForAll;
            _nameRu.Text = Resources.SNameRu;
            _nameKz.Text = Resources.SNameKz;
        }

        protected override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);
            _chkSaveAsShared.Enabled = UserRoles.IsInRole(UserRoles.AllowSaveJournalSettingsAsShared);
            _tbSaveNameRu.Text = "";
            _tbSaveNameKz.Text = "";

            using (var db = new DB_RvsSettingsDataContext(LogMonitor.CreateConnection()))
            {
                var sid = User.GetSID();
                var journalTypeName = RvsSavedProperties.GetJournalTypeName(Journal.ParentUserControl);
                var data = (IQueryable<RVS_SavedProperty>)
                           db.RVS_SavedProperties.
                               Where(r => r.JournalTypeName == journalTypeName
                                          && (r.UserSID == sid || r.isSharedView)).
                               OrderByDescending(r => r.dateTime);
                var result = data.Select(r =>
                                         new ListItem
                                             {
                                                 id = r.id,
                                                 NameKz = r.nameKz,
                                                 NameRu = r.nameRu,
                                                 dateTime = r.dateTime,
                                                 isShared = r.isSharedView,
                                                 Sid = r.UserSID,
                                                 SaveFilters = r.RVS_Property.Filter != null,
                                             }).ToList();
                _ddlLoadFilters.DataSource = result;
                _ddlLoadFilters.DataBind();
                _ddlLoadFilters.Attributes["onchange"] =
                    string.Format("changeSelectedLoadViewSettings(this, '{0}');", _hlDelSavedInOpen.ClientID);

                if (!UserRoles.IsInRole(UserRoles.AllowChangeOrDeleteJournalSettingsAsShared))
                    _ddlSaveFilters.DataSource = result.Where(r => r.Sid == sid);
                else
                    _ddlSaveFilters.DataSource = result;
                _ddlSaveFilters.DataBind();
                _ddlSaveFilters.Attributes["onchange"] =
                    string.Format("changeSelectedSaveViewSettings(this, '{0}', '{1}', '{2}', '{3}', '{4}');",
                                  _tbSaveNameRu.ClientID, _tbSaveNameKz.ClientID, _chkSaveAsShared.ClientID,
                                  _chkSaveFilters.ClientID, _hlDelSavedInSave.ClientID);
                if (_ddlLoadFilters.SelectedValue == null || _ddlLoadFilters.SelectedValue == "")
                    _hlDelSavedInOpen.Style[HtmlTextWriterStyle.Display] = "none";
                if (_ddlSaveFilters.SelectedValue == null || _ddlSaveFilters.SelectedValue == "")
                    _hlDelSavedInSave.Style[HtmlTextWriterStyle.Display] = "none";
            }
        }

        public static void SaveSettings(BaseJournalControl journal, string argument, ILogMonitor logMonitor, LogMessageType logMessageType)
        {
            var jss = new JavaScriptSerializer();
            var saveArgument = jss.Deserialize<SaveArgument>(argument);
            var properties = RvsSavedProperties.GetFromJournal(journal.ParentUserControl);
            logMonitor.Log(new LogMessageEntry(logMessageType, properties.NameRu, properties));
            properties.SaveWithViewSettings(saveArgument, "Cross:" + journal.GetType().FullName);
        }

        private static void LoadSettings(BaseJournalControl control, string argument, ILogMonitor logMonitor, LogMessageType logMessageType)
        {
            var jss = new JavaScriptSerializer();
            var loadArgument = jss.Deserialize<LoadArgument>(argument);
            if (loadArgument.id.HasValue)
            {
                var properties = RvsSavedProperties.LoadBySavedViewSettings(loadArgument.id.Value, null);
                properties.StorageValues = null;
                logMonitor.Log(new LogMessageEntry(logMessageType, properties.NameRu, properties));
                properties.SetToJournal(control.ParentUserControl, !loadArgument.loadFilters, true);
            }
        }

        public string GetOpenSaveViewSettingsScript()
        {
            return string.Format("$find('{0}').show();", _ppcSave.ModalPopupBehaviorID);
        }

        public string GetOpenLoadViewSettingsScript()
        {
            return string.Format("$find('{0}').show();", _ppcLoad.ModalPopupBehaviorID);
        }

        // ReSharper disable InconsistentNaming
        public class SaveArgument
        {
            public bool saveFilters { get; set; }
            public bool saveAsShared { get; set; }
            public string nameRu { get; set; }
            public string nameKz { get; set; }
            public long? id { get; set; }
        }

        public class LoadArgument
        {
            public bool loadFilters { get; set; }
            public long? id { get; set; }
        }

        public class ListItem
        {
            public long id { get; set; }
            public string NameRu { get; set; }
            public string NameKz { get; set; }
            public DateTime dateTime { get; set; }
            public string Sid { get; set; }
            public bool isShared { get; set; }
            public bool SaveFilters { get; set; }

            public string Name
            {
                get
                {
                    return (LocalizationHelper.IsCultureKZ ? NameKz : NameRu) + " (№" + id + ", " + dateTime + ")";
                }
            }
        }
        // ReSharper restore InconsistentNaming
    }
}