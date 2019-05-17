if (!window.Nat) window.Nat = {};

if (!Nat.Reports) Nat.Reports = {};

Nat.Reports.InitializeManager = function(options) {
    if (!window.VM) window.VM = {};

    VM.manager = new Nat.Reports.Manager(options || {});
    VM.manager.Initialize();
};
