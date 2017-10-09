using System.Collections.Generic;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;
using AjaxControlToolkit;

[assembly: WebResource("Nat.Web.Controls.GenerationClasses.BaseJournal.CrossJournalStillEdit.js", "text/javascript")]

namespace Nat.Web.Controls.GenerationClasses.BaseJournal
{
    [RequiredScript(typeof(TimerScript))]
    [ClientScriptResource("Nat.Web.Controls.CrossJournalStillEdit", "Nat.Web.Controls.GenerationClasses.BaseJournal.CrossJournalStillEdit.js")]
    [TargetControlType(typeof(TextBox))]
    [TargetControlType(typeof(Label))]
    public class CrossJournalStillEditExtender : ExtenderControlBase
    {
        protected override string ClientControlType
        {
            get
            {
                return "Nat.Web.Controls.CrossJournalStillEdit";
            }
        }

        protected override IEnumerable<ScriptReference> GetScriptReferences()
        {
            var baseResult = base.GetScriptReferences();
            var scripts = new[]
                {
                    new ScriptReference(
                        "Nat.Web.Controls.GenerationClasses.BaseJournal.CrossJournalStillEdit.js",
                        typeof(CrossJournalStillEditExtender).Assembly.FullName)
                };
            if (baseResult != null)
                return baseResult.Union(scripts);
            return scripts;
        }

        protected override void RenderInnerScript(ScriptBehaviorDescriptor descriptor)
        {
            base.RenderInnerScript(descriptor);
            descriptor.AddProperty("interval", 20000);
            descriptor.AddProperty("journalName", JournalName);
            descriptor.AddProperty("rowID", RowID);
        }

        public string JournalName { get; set; }

        public string RowID { get; set; }
    }
}