if (!window.Nat) window.Nat = {};

if (!Nat.Reports) Nat.Reports = {};

Nat.Reports.InitializeManager = function(options) {
    if (!window.VM) window.VM = {};

    VM.manager = new Nat.Reports.Manager(options || {});
    VM.manager.Initialize();
};

openDetailWindow = function (muId, personStatus, groupCode, onDate, isKz) {
    let w = $(window);
    let reportWindow = $('#reportWindow').data('kendoWindow');
    
    reportWindow.setOptions({ width: w.width() * 0.9, height: w.height() * 0.8 });
    reportWindow.title('');
    kendo.ui.progress($("#managerSplitter"), true);
    reportWindow.bind("refresh", function (e){        
        kendo.ui.progress($("#managerSplitter"), false);
    });
    reportWindow.content('<h6>Загрузка... <br/> Енгізу...</h6>');
    reportWindow.refresh({
        url: "/RPC/ULS_Persons/PersonShortInfo?muId=" + muId + "&personStatus=" + personStatus + "&groupCode=" + groupCode + "&onDate=" + onDate + "&isKz=" + isKz
        , iframe: true
    });
    kendo.ui.progress(reportWindow.element, true);
    reportWindow.open();
    reportWindow.center();
    let title = $.ajax({
        url: "/RPC/ULS_Persons/GetTitle",
        type: "POST",
        xhrFields: {
            withCredentials: true
        },
        data: { muId, personStatus, groupCode, isKz }
    }).done(function (result) {
        kendo.ui.progress($("#managerSplitter"), false);
        if (result.title) {
            reportWindow.title(result.title);
        }
    }).fail(function (e) {
    });
}
