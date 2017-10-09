using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using Nat.Web.Controls.GenerationClasses.BaseJournal;

namespace Nat.Web.Controls.EnableController
{
    public class ControllerItemValue
    {
        public bool IsInited { get; set; }
        public IEnumerable<object> Values { get; protected set; }
        public IEnumerable<object> ValuesWithoutControl { get; set; }
        public IEnumerable<string> ClientIDs { get; set; }
        public IEnumerable<Control> Controls { get; set; }
        public ControllerActiveType ActiveType { get; set; }
        public EventHandler<ControllerItemValueEventArgs> GetValuesByRenderContextHandler { get; set; }

        public void InitValues(IEnumerable<RenderContext> renderContexts, Controller controller)
        {
            var args = new ControllerItemValueEventArgs(renderContexts);
            if (GetValuesByRenderContextHandler != null)
                GetValuesByRenderContextHandler(controller, args);
            else
                args.Cancel = true;
            ClientIDs = args.ClientIDs;
            Controls = args.Controls;
            Values = args.ResultValues;
            ValuesWithoutControl = args.ResultValuesWithoutControl;
            IsInited = !args.Cancel;
        }
    }
}
