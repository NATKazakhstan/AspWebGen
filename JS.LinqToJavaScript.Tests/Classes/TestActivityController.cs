namespace JS.LinqToJavaScript.Tests.Classes
{
    using System;
    using System.Collections.Generic;
    using System.Web.UI;

    public partial class TestActivityController
    {
        partial void InitializePartial()
        {
            Field2BoolControl.EnabledExpression =
                Field3StringControl.EnabledExpression =
                Field4DateTimeControl.EnabledExpression =
                Field5DecimalControl.EnabledExpression = form => form.Field1LongControl.Value > 10;

            Field2BoolControl.VisibleExpression =
                Field3StringControl.VisibleExpression =
                Field4DateTimeControl.VisibleExpression =
                Field5DecimalControl.VisibleExpression = form => form.Field1LongControl.Value > 1;
        }
    }

    public partial class TestActivityController : ActivityController
    {
        partial void InitializePartial();

        public override void Initialize(Control form, Dictionary<string, object> values)
        {
            Field1LongControl = new TestFieldControl<TestActivityController, long>(this, "field1", "field1");
            Field2BoolControl = new TestFieldControl<TestActivityController, bool>(this, "field2", "field2");
            Field3StringControl = new TestFieldControlClassValue<TestActivityController, string>(this, "field3", "field3");
            Field4DateTimeControl = new TestFieldControl<TestActivityController, DateTime>(this, "field4", "field4");
            Field5DecimalControl = new TestFieldControl<TestActivityController, decimal>(this, "field5", "field5");

            InitializeControls(
                Field1LongControl,
                Field2BoolControl,
                Field3StringControl,
                Field4DateTimeControl,
                Field5DecimalControl);

            base.Initialize(form, values);
            
            InitializePartial();
        }

        public TestFieldControl<TestActivityController, long> Field1LongControl { get; protected set; }

        public TestFieldControl<TestActivityController, bool> Field2BoolControl { get; protected set; }

        public TestFieldControlClassValue<TestActivityController, string> Field3StringControl { get; protected set; }

        public TestFieldControl<TestActivityController, DateTime> Field4DateTimeControl { get; protected set; }

        public TestFieldControl<TestActivityController, decimal> Field5DecimalControl { get; protected set; }

        public long? Field1Long { get { return Field1LongControl.SavedValue; } }
        public bool? Field2Bool { get { return Field2BoolControl.SavedValue; } }
        public string Field3String { get { return Field3StringControl.SavedValue; } }
        public DateTime? Field4DateTime { get { return Field4DateTimeControl.SavedValue; } }
        public decimal? Field5Decimal { get { return Field5DecimalControl.SavedValue; } }
    }
}
