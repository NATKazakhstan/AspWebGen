using System.Collections.Generic;
using System.Linq;
using System.ComponentModel;
using System.Web.UI;
using Nat.Web.Controls.GenerationClasses.BaseJournal;

namespace Nat.Web.Controls.EnableController
{
    public class ControllerItemValueEventArgs : CancelEventArgs
    {
        public ControllerItemValueEventArgs(IEnumerable<RenderContext> renderContexts)
        {
            RenderContexts = renderContexts;
        }

        public ControllerItemValueEventArgs(bool cancel, IEnumerable<RenderContext> renderContexts) : base(cancel)
        {
            RenderContexts = renderContexts;
        }

        public IEnumerable<RenderContext> RenderContexts { get; private set; }
        public IEnumerable<object> ResultValues { get; set; }
        public IEnumerable<object> ResultValuesWithoutControl { get; set; }
        public IEnumerable<string> ClientIDs { get; set; }
        public IEnumerable<Control> Controls { get; set; }

        public IEnumerable<object> GetValues(BaseColumn column)
        {
            var list = new List<object>();
            foreach (var context in RenderContexts.Where(r => r.Column == column))
                list.Add(column.GetValue(context));
            return list;
        }

        public void SetParametersByDefault(BaseColumn column)
        {
            var listValues = new List<object>();
            var valuesWithoutControl = new List<object>();
            var clientIDs = new List<string>();
            var controls = new List<Control>();
            foreach (var context in RenderContexts.Where(r => r.Column == column))
            {
                var value = column.GetValue(context);
                listValues.Add(value);
                if (!string.IsNullOrEmpty(context.EditClientID))
                {
                    clientIDs.Add(context.EditClientID);
                    controls.Add(context.Control);
                }
                else
                    valuesWithoutControl.Add(value);
            }
            ResultValues = listValues;
            ClientIDs = clientIDs;
            Controls = controls;
            ResultValuesWithoutControl = valuesWithoutControl;
        }
    }
}
