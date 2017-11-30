namespace JS.LinqToJavaScript
{
    using System;
    using System.Collections.Generic;
    using System.Linq.Expressions;
    using System.Web.UI;
    using System.Web.UI.WebControls;

    using LinqToJavaScript.Attributes;

    public abstract class ActivityControl
    {
        #region Fields

        private string _clientID;

        #endregion

        #region Constructors and Destructors

        protected ActivityControl(string controlID, string controlName)
        {
            DefaultEnabled = true;
            DefaultVisible = true;
            DefaultAllowValidate = true;
            DefaultReadOnly = false;
            DefaultAllowRequiredValidate = true;
            Activities = new Dictionary<ActivityType, bool>(5);
            ControlName = controlName;
            ControlID = controlID;
        }

        #endregion

        #region Public Properties

        public event EventHandler<ActivityChangedEventArgs> ActivityChanged;

        public event EventHandler<ActivityChangedEventArgs> AllowRequiredValidateChanged;

        public event EventHandler<ActivityChangedEventArgs> AllowValidateChanged;

        public event EventHandler<ActivityChangedEventArgs> EnabledChanged;

        public event EventHandler<ActivityChangedEventArgs> ReadOnlyChanged;

        public event EventHandler<ActivityChangedEventArgs> VisibleChanged;

        public Dictionary<ActivityType, bool> Activities { get; private set; }

        [JavaScriptProperty(ExpressionGetProperty = "AllowRequiredValidateExpression", ContextOfGetProperty = "this.parent")]
        public bool AllowRequiredValidate
        {
            get { return this[ActivityType.AllowRequiredValidate]; }
            set { this[ActivityType.AllowRequiredValidate] = value; }
        }

        [JavaScriptProperty(ExpressionGetProperty = "AllowValidateExpression", ContextOfGetProperty = "this.parent")]
        public bool AllowValidate
        {
            get { return this[ActivityType.AllowValidate]; }
            set { this[ActivityType.AllowValidate] = value; }
        }

        [JavaScriptProperty]
        public string ClientID
        {
            get { return _clientID ?? (Control != null ? Control.ClientID : string.Empty); }
            set { _clientID = value; }
        }

        [JavaScriptProperty(ExpressionGetProperty = "ControlExpression")]
        public virtual Control Control { get; set; }

        public string ControlName { get; set; }

        public string ControlID { get; set; }

        public bool DefaultAllowRequiredValidate { get; set; }

        public bool DefaultAllowValidate { get; set; }

        public bool DefaultEnabled { get; set; }

        public bool DefaultReadOnly { get; set; }

        public bool DefaultVisible { get; set; }

        public Expression ControlExpression
        {
            get
            {
                return (Expression<Func<ActivityControl, object>>)
                       (r => (r.ClientID == null || r.ClientID == string.Empty) ? null : r.ClientID.JQueryFindById());
            }
        }

        [JavaScriptProperty(ExpressionGetProperty = "EnabledExpression", ContextOfGetProperty = "this.parent")]
        public bool Enabled
        {
            get { return this[ActivityType.Enabled]; }
            set { this[ActivityType.Enabled] = value; }
        }

        protected abstract object ControlValue { get; set; }

        [JavaScriptProperty(ExpressionGetProperty = "ReadOnlyExpression", ContextOfGetProperty = "this.parent")]
        public bool ReadOnly
        {
            get { return this[ActivityType.ReadOnly]; }
            set { this[ActivityType.ReadOnly] = value; }
        }

        [JavaScriptProperty(ExpressionGetProperty = "VisibleExpression", ContextOfGetProperty = "this.parent")]
        public bool Visible
        {
            get { return this[ActivityType.Visible]; }
            set { this[ActivityType.Visible] = value; }
        }

        public WebControl WebControl
        {
            get { return Control as WebControl; }
            set { Control = value; }
        }

        #endregion

        #region Indexers

        private bool this[ActivityType activity]
        {
            get
            {
                if (Activities.ContainsKey(activity))
                    return Activities[activity];
                return GetDefaultActivity(activity);
            }

            set
            {
                Activities[activity] = value;
                SetActivityToControl(activity, value);
            }
        }

        #endregion

        #region Public Methods and Operators

        public abstract void ComputeActivities();

        public void Initialize(Control form, object value)
        {
            Control = form == null ? null : form.FindControl(ControlID);
            if (Control == null)
                ControlValue = value;
        }

        #endregion

        #region Methods

        protected virtual void SetAllowRequiredValidateToControl(bool value)
        {
            OnAllowRequiredValidateChanged(new ActivityChangedEventArgs(ActivityType.AllowRequiredValidate, value));
        }

        protected virtual void SetAllowValidateToControl(bool value)
        {
            OnAllowValidateChanged(new ActivityChangedEventArgs(ActivityType.AllowValidate, value));
        }

        protected virtual void SetEnabledToControl(bool value)
        {
            if (WebControl != null)
                WebControl.Enabled = value;
            OnEnabledChanged(new ActivityChangedEventArgs(ActivityType.Enabled, value));
        }

        protected virtual void SetReadOnlyToControl(bool value)
        {
            OnReadOnlyChanged(new ActivityChangedEventArgs(ActivityType.ReadOnly, value));
        }

        protected virtual void SetVisibleToControl(bool value)
        {
            //if (Control != null) Control.Visible = value;
            OnVisibleChanged(new ActivityChangedEventArgs(ActivityType.Visible, value));
        }

        protected virtual void OnActivityChanged(ActivityChangedEventArgs e)
        {
            var handler = ActivityChanged;
            if (handler != null)
                handler(this, e);
        }

        protected virtual void OnAllowRequiredValidateChanged(ActivityChangedEventArgs e)
        {
            var handler = AllowRequiredValidateChanged;
            if (handler != null)
                handler(this, e);
        }

        protected virtual void OnAllowValidateChanged(ActivityChangedEventArgs e)
        {
            var handler = AllowValidateChanged;
            if (handler != null)
                handler(this, e);
        }

        protected virtual void OnEnabledChanged(ActivityChangedEventArgs e)
        {
            var handler = EnabledChanged;
            if (handler != null)
                handler(this, e);
        }

        protected virtual void OnReadOnlyChanged(ActivityChangedEventArgs e)
        {
            var handler = ReadOnlyChanged;
            if (handler != null)
                handler(this, e);
        }

        protected virtual void OnVisibleChanged(ActivityChangedEventArgs e)
        {
            var handler = VisibleChanged;
            if (handler != null)
                handler(this, e);
        }

        private bool GetDefaultActivity(ActivityType activity)
        {
            switch (activity)
            {
                case ActivityType.None:
                    return true;
                case ActivityType.Enabled:
                    return DefaultEnabled;
                case ActivityType.Visible:
                    return DefaultVisible;
                case ActivityType.ReadOnly:
                    return DefaultReadOnly;
                case ActivityType.AllowValidate:
                    return DefaultAllowValidate;
                case ActivityType.AllowRequiredValidate:
                    return DefaultAllowRequiredValidate;
                default:
                    throw new ArgumentOutOfRangeException("activity");
            }
        }

        private void SetActivityToControl(ActivityType activity, bool value)
        {
            switch (activity)
            {
                case ActivityType.None:
                    break;
                case ActivityType.Enabled:
                    SetEnabledToControl(value);
                    break;
                case ActivityType.Visible:
                    SetVisibleToControl(value);
                    break;
                case ActivityType.ReadOnly:
                    SetReadOnlyToControl(value);
                    break;
                case ActivityType.AllowValidate:
                    SetAllowValidateToControl(value);
                    break;
                case ActivityType.AllowRequiredValidate:
                    SetAllowRequiredValidateToControl(value);
                    break;
                default:
                    throw new ArgumentOutOfRangeException("activity");
            }

            OnActivityChanged(new ActivityChangedEventArgs(activity, value));
        }

        #endregion
    }

    public abstract class ActivityControl<TActivityController> : ActivityControl
        where TActivityController : ActivityController
    {
        #region Fields

        private Func<TActivityController, bool> allowRequiredValidateFunction;
        private Func<TActivityController, bool> allowValidateFunction;
        private Func<TActivityController, bool> enabledFunction;
        private Func<TActivityController, bool> readOnlyFunction;
        private Func<TActivityController, bool> visibleFunction;

        #endregion

        #region Constructors and Destructors

        protected ActivityControl(TActivityController activityController, string controlID, string controlName)
            : base(controlID, controlName)
        {
            if (activityController == null)
                throw new ArgumentNullException("activityController");
            ActivityController = activityController;
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// Если true, то проверяем обязательность поля, если false, то не важно заполнено ли поле или нет.
        /// </summary>
        public Expression<Func<TActivityController, bool>> AllowRequiredValidateExpression { get; set; }

        /// <summary>
        /// Если true, то выполняем валидацию поля, если false, то не важно чем заполнено поле.
        /// </summary>
        public Expression<Func<TActivityController, bool>> AllowValidateExpression { get; set; }

        /// <summary>
        /// Если true, то поле доступно для заполнения и данные сохраняются, если false, то поле не доступно для заполнения и данные не сохраняются.
        /// </summary>
        public Expression<Func<TActivityController, bool>> EnabledExpression { get; set; }

        /// <summary>
        /// Если false, то поле доступно для заполнения пользователю, если true, то пользователь не может менять значение.
        /// </summary>
        public Expression<Func<TActivityController, bool>> ReadOnlyExpression { get; set; }

        /// <summary>
        /// Если true, то поле видимо пользователю, если false, то поле невидимо.
        /// </summary>
        public Expression<Func<TActivityController, bool>> VisibleExpression { get; set; }

        #endregion

        #region Properties

        protected TActivityController ActivityController { get; set; }

        #endregion

        #region Public Methods and Operators

        public override void ComputeActivities()
        {
            if (allowRequiredValidateFunction == null)
            {
                allowRequiredValidateFunction = AllowRequiredValidateExpression == null
                                                     ? DefaultAllowRequiredValidateFunction
                                                     : AllowRequiredValidateExpression.Compile();
            }

            AllowRequiredValidate = allowRequiredValidateFunction(ActivityController);

            if (allowValidateFunction == null)
            {
                allowValidateFunction = AllowValidateExpression == null
                                             ? DefaultAllowValidateFunction
                                             : AllowValidateExpression.Compile();
            }

            AllowValidate = allowValidateFunction(ActivityController);

            if (visibleFunction == null)
            {
                visibleFunction = VisibleExpression == null
                                       ? DefaultVisibleFunction
                                       : VisibleExpression.Compile();
            }

            Visible = visibleFunction(ActivityController);

            if (enabledFunction == null)
            {
                enabledFunction = EnabledExpression == null
                                       ? DefaultEnabledFunction
                                       : EnabledExpression.Compile();
            }

            Enabled = enabledFunction(ActivityController);

            if (readOnlyFunction == null)
            {
                readOnlyFunction = ReadOnlyExpression == null
                                        ? DefaultReadOnlyFunction
                                        : ReadOnlyExpression.Compile();
            }

            ReadOnly = ActivityController.ReadOnly || readOnlyFunction(ActivityController);
        }

        #endregion

        #region Methods

        private bool DefaultAllowRequiredValidateFunction(TActivityController arg)
        {
            return DefaultAllowRequiredValidate;
        }

        private bool DefaultAllowValidateFunction(TActivityController arg)
        {
            return DefaultAllowValidate;
        }

        private bool DefaultEnabledFunction(TActivityController arg)
        {
            return DefaultEnabled;
        }

        private bool DefaultReadOnlyFunction(TActivityController arg)
        {
            return DefaultReadOnly;
        }

        private bool DefaultVisibleFunction(TActivityController arg)
        {
            return DefaultVisible;
        }

        #endregion
    }

    [JavaScriptClass(ClassName = "ActivityControlClassValue", Namespace = "JS.Web", BaseClassName = "Sys.Component")]
    public class ActivityControl<TActivityController, T> : ActivityControl<TActivityController>
        where TActivityController : ActivityController
        where T : struct
    {
        #region Fields

        private T? value;

        #endregion

        #region Constructors and Destructors

        public ActivityControl(TActivityController activityController, string controlID, string controlName)
            : base(activityController, controlID, controlName)
        {
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// Не типизированное значение поля. На клиенте, не доступно. Берется значение из компонента, если его нет, то берется сохраненное значение.
        /// </summary>
        protected override object ControlValue
        {
            get { return SavedValue; }
            set { SavedValue = (T?)value; }
        }

        public Expression ControlValueExpression
        {
            get
            {
                return (Expression<Func<ActivityControl<TActivityController, T>, object>>)
                       (r => r.Control != null ? r.Control.JQueryGetValue() : r.SavedValue);
            }
        }

        /// <summary>
        /// Типизированное значение поля. Берется значение из компонента, если его нет, то берется сохраненное значение.
        /// </summary>
        [JavaScriptProperty(ExpressionGetProperty = "ControlValueExpression")]
        public T? Value
        {
            get { return SavedValue; }
        }

        /// <summary>
        /// Типизированное значение поля. На клиенте, только сохраненное значение. Для сервере берется значение из компонента, если его нет, то берется сохраненное значение.
        /// </summary>
        [JavaScriptProperty]
        public T? SavedValue
        {
            get
            {
                return GetControlValue() ?? value;
            }

            set
            {
                if (Control == null)
                    this.value = value;
                else
                    throw new NotSupportedException("Выставление значения в контрол не поддерживается");
            }
        }

        public Func<T?> GetControlValueHandler { get; set; }

        #endregion

        #region Methods

        protected virtual T? GetControlValue()
        {
            if (GetControlValueHandler != null)
                return GetControlValueHandler();

            return null;
        }

        #endregion
    }

    [JavaScriptClass(ClassName = "ActivityControlClassValue", Namespace = "JS.Web", BaseClassName = "Sys.Component")]
    public class ActivityControlClassValue<TActivityController, T> : ActivityControl<TActivityController>
        where TActivityController : ActivityController
        where T : class
    {
        #region Fields

        private T value;

        #endregion

        #region Constructors and Destructors

        public ActivityControlClassValue(TActivityController activityController, string controlID, string controlName)
            : base(activityController, controlID, controlName)
        {
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// Не типизированное значение поля. На клиенте, не доступно. Берется значение из компонента, если его нет, то берется сохраненное значение.
        /// </summary>
        protected override object ControlValue
        {
            get { return SavedValue; }
            set { SavedValue = (T)value; }
        }

        public Expression ControlValueExpression
        {
            get
            {
                return (Expression<Func<ActivityControlClassValue<TActivityController, T>, object>>)
                       (r => r.Control != null ? r.Control.JQueryGetValue() : SavedValue);
            }
        }

        /// <summary>
        /// Типизированное значение поля. Берется значение из компонента, если его нет, то берется сохраненное значение.
        /// </summary>
        [JavaScriptProperty(ExpressionGetProperty = "ControlValueExpression")]
        public T Value
        {
            get { return SavedValue; }
        }

        /// <summary>
        /// Типизированное значение поля. На клиенте, только сохраненное значение. Для сервере берется значение из компонента, если его нет, то берется сохраненное значение.
        /// </summary>
        [JavaScriptProperty]
        public T SavedValue
        {
            get
            {
                return GetControlValue() ?? value;
            }

            set
            {
                if (Control == null)
                    this.value = value;
                else
                    throw new NotSupportedException("Выставление значения в контрол не поддерживается");
            }
        }

        public Func<T> GetControlValueHandler { get; set; }

        #endregion

        #region Methods

        protected virtual T GetControlValue()
        {
            if (GetControlValueHandler != null)
                return GetControlValueHandler();

            return null;
        }

        #endregion
    }
}