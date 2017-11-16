using System.Web.UI;

[assembly: WebResource("JS.LinqToJavaScript.ActivityController.js", "text/javascript")]

namespace JS.LinqToJavaScript
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Web.UI;
    using System.Web.UI.WebControls;

    using AjaxControlToolkit;

    using JS.LinqToJavaScript.Attributes;

    [JavaScriptClass]
    [ClientScriptResource("JS.LinqToJavaScript.ActivityController", "JS.LinqToJavaScript.ActivityController.js")]
    public class ActivityController : IScriptControl
    {
        #region Public Properties

        [JavaScriptProperty(DeclaredInBaseClass = true)]
        public IEnumerable<string> ChangedControls
        {
            get { return Controls.Select(r => r.Value.ClientID).ToList(); }
        }

        [JavaScriptProperty(DeclaredInBaseClass = true)]
        public virtual IEnumerable<string> ActivityControls
        {
            get { return Controls.Select(r => r.Value.ControlName).ToList(); }
        }

        public Expression ChangedControlsSetExpression
        {
            get
            {
                return (Expression<Func<object, object, object>>)
                       ((from, value) => from.ExecuteJavaScriptFunction("addHandlers", value));
            }
        }

        [JavaScriptProperty(PropertyName = "isNew")]
        public bool IsNew { get; set; }

        [JavaScriptProperty(PropertyName = "readOnly")]
        public bool ReadOnly { get; set; }

        [JavaScriptProperty(PropertyName = "validationGroup")]
        public string ValidationGroup { get; set; }

        [JavaScriptProperty(PropertyName = "formID")]
        public string FormID { get; set; }

        #endregion

        #region Properties

        protected Dictionary<string, ActivityControl> Controls { get; private set; }

        #endregion

        #region Public Methods and Operators

        internal static string GetScriptUrl<T>()
            where T : ActivityController
        {
            return "/LinqToJavaScriptHandler.ashx?type=" + typeof(T).AssemblyQualifiedName;
        }

        public void ComputeActivities()
        {
            foreach (var control in Controls.Values)
            {
                control.ComputeActivities();
            }
        }

        internal string GetScriptForCreation()
        {
            var provider = new LinqToJavaScriptProvider();
            return provider.GetCreateClassScript(this);
        }

        internal string GetScriptUrl()
        {
            return "/LinqToJavaScriptHandler.ashx?type=" + GetType().AssemblyQualifiedName;
        }

        public virtual void Initialize(Control form, Dictionary<string, object> values)
        {
            if (Controls == null)
                return;

            foreach (var control in Controls.Values)
            {
                control.Initialize(form, values.ContainsKey(control.ControlName) ? values[control.ControlName] : null);
            }
        }

        #endregion

        #region Explicit Interface Methods

        IEnumerable<ScriptDescriptor> IScriptControl.GetScriptDescriptors()
        {
            return new List<ScriptDescriptor> { new LinqToJavaScriptDescriptor(this) };
        }

        IEnumerable<ScriptReference> IScriptControl.GetScriptReferences()
        {
            var references = new List<ScriptReference>();
            references.AddRange(ScriptObjectBuilder.GetScriptReferences(GetType()));
            references.Add(new ScriptReference(GetScriptUrl()));
            return references;
        }

        #endregion

        #region Methods

        protected void InitializeControls(params ActivityControl[] controls)
        {
            Controls = controls.Where(r => r.ControlName != null).ToDictionary(r => r.ControlName);
        }

        #endregion

        public static void SetEnabledToValidators(Page page, string validationGroup, string controlToValidate, bool enabled)
        {
            if (string.IsNullOrEmpty(controlToValidate))
                throw new ArgumentNullException("controlToValidate");

            var validators = page.GetValidators(validationGroup);
            var changeValidators = validators.OfType<BaseValidator>()
                                             .Where(r => controlToValidate.Equals(r.ControlToValidate))
                                             .Where(r => r.Enabled != enabled);

            foreach (var changeValidator in changeValidators)
                changeValidator.Enabled = enabled;
        }
    }
}