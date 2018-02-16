using System.Web;
using System.Web.UI;

namespace Nat.Web.Controls
{
    public class Themes
    {
        public static string Theme
        {
            get
            {
                var page = ((Page)HttpContext.Current.Items["CurrentPage"]);
                return page != null ? page.Theme : null;
            }
        }

        public static string ThemePath
        {
            get
            {
                var theme = Theme;
                return "/App_Themes/" + (string.IsNullOrEmpty(theme) ? "nat" : theme);
            }
        }

        public static string IconUrlActivationOrder { get { return ThemePath + "/activation-order.png"; } }
        public static string IconUrlActivationOrderNot { get { return ThemePath + "/activation-order-not.png"; } }
        public static string IconUrlAccepted { get { return ThemePath + "/accept.png"; } }
        public static string IconUrlAcceptDocument { get { return ThemePath + "/AcceptDocument.png"; } }
        public static string IconUrlAdd { get { return ThemePath + "/Add.png"; } }
        public static string IconUrlArchive { get { return ThemePath + "/Symbol-Archive.png"; } }
        public static string IconUrlArchiveHide { get { return ThemePath + "/Symbol-ArchiveHide.png"; } }
        public static string IconUrlArchiveForSelected { get { return ThemePath + "/Symbol-ArchiveForSelected.png"; } }
        public static string IconUrlArrowDown { get { return ThemePath + "/Arrow-Down.gif"; } }
        public static string IconUrlArrowUp { get { return ThemePath + "/Arrow-Up.gif"; } }
        public static string IconUrlArrowRight { get { return ThemePath + "/Arrow-Right.gif"; } }
        public static string IconUrlArrowRightMin { get { return ThemePath + "/Arrow-Right-min.png"; } }
        public static string IconUrlArrowLeft { get { return ThemePath + "/Arrow-Left.gif"; } }
        public static string IconUrlArrowLeftMin { get { return ThemePath + "/Arrow-Left-min.png"; } }
        public static string IconUrlArrowRightSearch { get { return ThemePath + "/Arrow-Right-search.png"; } }
        public static string IconUrlArrowLeftSearch { get { return ThemePath + "/Arrow-Left-search.png"; } }
        public static string IconUrlBrowse { get { return ThemePath + "/browse.gif"; } }
        public static string IconUrlBrowseInfo { get { return ThemePath + "/info.gif"; } }
        public static string IconUrlCalendar { get { return ThemePath + "/calendar.png"; } }
        public static string IconUrlCancel { get { return ThemePath + "/cancel.png"; } }
        public static string IconUrlCancelDocument { get { return ThemePath + "/CancelDocument.png"; } }
        public static string IconUrlCancelFilter { get { return ThemePath + "/CancelFilter.png"; } }
        public static string IconUrlClearBrowseValue { get { return ThemePath + "/Cancel.png"; } }
        public static string IconUrlConcatenateColumns { get { return ThemePath + "/ConcatenateColumns.png"; } }
        public static string IconUrlConcatenateColumnsArrowRight { get { return ThemePath + "/Arrow-Right.png"; } }
        public static string IconUrlConcatenateColumnsArrowLeft { get { return ThemePath + "/Arrow-Left.png"; } }
        public static string IconUrlColorPickerButton { get { return ThemePath + "/cp_button.png"; } }
        public static string IconUrlCrossJournalReport { get { return ThemePath + "/CrossJournalReport.png"; } }
        public static string IconUrlDelete { get { return ThemePath + "/Delete.png"; } }
        public static string IconUrlDefaultFilter { get { return ThemePath + "/DefaultFilter.png"; } }
        public static string IconUrlEdit { get { return ThemePath + "/Edit.png"; } }
        public static string IconUrlError { get { return ThemePath + "/Symbol-Error.gif"; } }
        public static string IconUrlStop { get { return ThemePath + "/Symbol-Stop.gif"; } }
        public static string IconUrlStopBlue { get { return ThemePath + "/Stop-Blue.png"; } }
        public static string IconUrlStopGreen { get { return ThemePath + "/Stop-Green.png"; } }
        public static string IconUrlExport { get { return ThemePath + "/export.gif"; } }
        public static string IconUrlFail { get { return ThemePath + "/fail.gif"; } }
        public static string IconUrlFilter { get { return ThemePath + "/filter.png"; } }
        public static string IconUrlFilterCancel { get { return ThemePath + "/CancelFilter.png"; } }

        public static string IconUrlFillRow { get { return ThemePath + "/FillRow.png"; } }
        public static string IconUrlFillColumn { get { return ThemePath + "/FillColumn.png"; } }
        public static string IconUrlFillCell { get { return ThemePath + "/FillCell.png"; } }
        public static string IconUrlFontRow { get { return ThemePath + "/FontRow.png"; } }
        public static string IconUrlFontColumn { get { return ThemePath + "/FontColumn.png"; } }
        public static string IconUrlFontCell { get { return ThemePath + "/FontCell.png"; } }

        public static string IconUrlHelpIcon { get { return ThemePath + "/HelpIcon.gif"; } }
        public static string IconUrlInformation { get { return ThemePath + "/Symbol-Information.gif"; } }
        public static string IconUrlInformation2 { get { return ThemePath + "/Information.png"; } }
        public static string IconUrlJournal { get { return ThemePath + "/ToJournal.png"; } }
        public static string IconUrlLoadSettings { get { return ThemePath + "/table_open.png"; } }
        public static string IconUrlMenu { get { return ThemePath + "/menu.png"; } }
        public static string IconUrlNoPicture { get { return ThemePath + "/no_pic.gif"; } }
        public static string IconUrlOK { get { return ThemePath + "/ok.gif"; } }
        public static string IconUrlOrder { get { return ThemePath + "/order.gif"; } }
        public static string IconUrlOrderDetails { get { return ThemePath + "/orderDetails.gif"; } }
        public static string IconUrlOrderByColumnAsc { get { return ThemePath + "/OrderByColumnAsc.png"; } }
        public static string IconUrlOrderByColumnAscSelected { get { return ThemePath + "/OrderByColumnAscS.png"; } }
        public static string IconUrlOrderByColumnDesc { get { return ThemePath + "/OrderByColumnDesc.png"; } }
        public static string IconUrlOrderByColumnDescSelected { get { return ThemePath + "/OrderByColumnDescS.png"; } }
        public static string IconUrlOrderByColumnRemove { get { return ThemePath + "/OrderByColumnRemove.png"; } }
        public static string IconUrlRemove { get { return ThemePath + "/SmallRemove.png"; } }
        public static string IconUrlRestricted { get { return ThemePath + "/Symbol-Restricted.gif"; } }
        public static string IconUrlRefresh { get { return ThemePath + "/update.gif"; } }
        public static string IconUrlRefreshGreen { get { return ThemePath + "/refresh_green.png"; } }
        public static string IconUrlRefreshYellow { get { return ThemePath + "/refresh_yellow.png"; } }
        public static string IconUrlSave { get { return ThemePath + "/save.png"; } }
        public static string IconUrlSaveSettings { get { return ThemePath + "/table_save.png"; } }
        public static string IconUrlSelect { get { return ThemePath + "/select.png"; } }
        public static string IconUrlSelectAll { get { return ThemePath + "/import.png"; } }
        public static string IconUrlSelectAllRecords { get { return ThemePath + "/selectall.png"; } }
        public static string IconUrlShowAllFilters { get { return ThemePath + "/ShowAllFilters.png"; } }
        public static string IconUrlShowMainGroupFilters { get { return ThemePath + "/ShowMainGroupFilters.png"; } }
        public static string IconUrlSmallRemove { get { return ThemePath + "/SmallRemove.png"; } }
        public static string IconUrlSmallArrowDown { get { return ThemePath + "/SmallArrowDown.png"; } }
        public static string IconUrlSmallArrowUp { get { return ThemePath + "/SmallArrowUp.png"; } }
        public static string IconUrlSubscription { get { return ThemePath + "/subscription_report.gif"; } }
        public static string IconUrlSuccess { get { return ThemePath + "/success.gif"; } }
        public static string IconUrlToReportPlugin { get { return ThemePath + "/ToReportPlugin.png"; } }
        public static string IconUrlUnSelect { get { return ThemePath + "/UnSelect.png"; } }
        public static string IconUrlUnselectAllRecords { get { return ThemePath + "/unselectall.png"; } }
        public static string IconUrlViewSettings { get { return ThemePath + "/ViewSettings.png"; } }
        public static string IconUrlViewSettingsFixedHeader { get { return ThemePath + "/ViewSettingsFixedHeader.png"; } }
        public static string IconUrlWarning { get { return ThemePath + "/Symbol-Warning.png"; } }
        
        public static string IconUrlPostFileImageFromClipboard { get { return ThemePath + "/PostFileImageFromClipboard.png"; } }
        public static string IconUrlPostFileTextFromClipboard { get { return ThemePath + "/PostFileTextFromClipboard.png"; } }
        public static string IconUrlPostFileFromClipboard { get { return ThemePath + "/PostFile.png"; } }
        public static string IconUrlPostFileFromDialog { get { return ThemePath + "/PostFileFromDialog.png"; } }
        public static string IconUrlPaste { get { return ThemePath + "/Paste.png"; } }
        public static string IconUrlFolderOpen { get { return ThemePath + "/Folder_Open.png"; } }
        public static string IconUrlCalculate { get { return ThemePath + "/calculate.png"; } }
        public static string IconUrlProcessing { get { return ThemePath + "/processing.png"; } }
        public static string IconUrlProcess { get { return ThemePath + "/process.png"; } }

        public static string IconUrlUserMaleGo { get { return ThemePath + "/user_male_go.png"; } }
        public static string IconUrlUserMaleAccept { get { return ThemePath + "/user_male_accept.png"; } }
        public static string IconUrlUserMaleDelete { get { return ThemePath + "/user_male_delete.png"; } }

        public static string IconUrlUserMaleGoDisable { get { return ThemePath + "/user_male_go_disable.png"; } }
        public static string IconUrlUserMaleAcceptDisable { get { return ThemePath + "/user_male_accept_disable.png"; } }
        public static string IconUrlUserMaleDeleteDisable { get { return ThemePath + "/user_male_delete_disable.png"; } }

        public static string IconUrlGroupClassifierIndicators { get { return ThemePath + "/Group_ClassifierIndicators.png"; } }
        public static string IconUrlGroupSubdivisions { get { return ThemePath + "/Group_Subdivisions.png"; } }

        public static string IconUrlList { get { return ThemePath + "/List.png"; } }

        public static string IconUrlPersonsList { get { return ThemePath + "/PersonsList.png"; } }

        public static string IconUrlWord { get { return ThemePath + "/word.png"; } }

        public static string IconUrlAddLinkToExternalSystem { get { return ThemePath + "/Add.png"; } }
        public static string IconUrlViewExternalSystemRecord { get { return ThemePath + "/info.gif"; } }

        public static string IconUrlLocked { get { return ThemePath + "/Locked.png"; } }
        public static string IconUrlUnlocked { get { return ThemePath + "/Unlocked.png"; } }

        #region CrossJournal

        public static string IconUrlCrossJournalNoGroupS { get { return ThemePath + "/NoGroupS.png"; } }
        public static string IconUrlCrossJournalGroup1S { get { return ThemePath + "/Group1S.png"; } }
        public static string IconUrlCrossJournalGroup2S { get { return ThemePath + "/Group2S.png"; } }
        public static string IconUrlCrossJournalGroup3S { get { return ThemePath + "/Group3S.png"; } }
        public static string IconUrlCrossJournalGroup4S { get { return ThemePath + "/Group4S.png"; } }
        public static string IconUrlCrossJournalGroup5S { get { return ThemePath + "/Group5S.png"; } }
        public static string IconUrlCrossJournalGroup6S { get { return ThemePath + "/Group6S.png"; } }
        public static string IconUrlCrossJournalGroup7S { get { return ThemePath + "/Group7S.png"; } }
        public static string IconUrlCrossJournalGroup8S { get { return ThemePath + "/Group8S.png"; } }

        public static string IconUrlCrossJournalNoGroup { get { return ThemePath + "/NoGroup.png"; } }
        public static string IconUrlCrossJournalGroup1 { get { return ThemePath + "/Group1.png"; } }
        public static string IconUrlCrossJournalGroup2 { get { return ThemePath + "/Group2.png"; } }
        public static string IconUrlCrossJournalGroup3 { get { return ThemePath + "/Group3.png"; } }
        public static string IconUrlCrossJournalGroup4 { get { return ThemePath + "/Group4.png"; } }
        public static string IconUrlCrossJournalGroup5 { get { return ThemePath + "/Group5.png"; } }
        public static string IconUrlCrossJournalGroup6 { get { return ThemePath + "/Group6.png"; } }
        public static string IconUrlCrossJournalGroup7 { get { return ThemePath + "/Group7.png"; } }
        public static string IconUrlCrossJournalGroup8 { get { return ThemePath + "/Group8.png"; } }

        #endregion

        #region Светофор

        public static string IconUrlGreenCircle { get { return ThemePath + "/green.png"; } }
        public static string IconUrlGrayCircle { get { return ThemePath + "/gray.png"; } }
        public static string IconUrlRedCircle  { get { return ThemePath + "/red.png"; } }
        public static string IconUrlYellowCircle  { get { return ThemePath + "/yellow.png"; } }

        #endregion

        #region Warning

        public static string IconUrlWarningBlue { get { return ThemePath + "/WarningBlue.png"; } }
        public static string IconUrlWarningGreen { get { return ThemePath + "/WarningGreen.png"; } }
        public static string IconUrlWarningLightBlue { get { return ThemePath + "/WarningLightBlue.png"; } }
        public static string IconUrlWarningOrange { get { return ThemePath + "/WarningOrange.png"; } }
        public static string IconUrlWarningPink { get { return ThemePath + "/WarningPink.png"; } }
        public static string IconUrlWarningPurple { get { return ThemePath + "/WarningPurple.png"; } }
        public static string IconUrlWarningRed { get { return ThemePath + "/WarningRed.png"; } }
        public static string IconUrlWarningYellow { get { return ThemePath + "/WarningYellow.png"; } }

        #endregion

        public static string Icon(string filename) => $"{ThemePath}/{filename}";
    }
}