namespace JS.LinqToJavaScript.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;

    using JS.LinqToJavaScript.Attributes;
    using JS.LinqToJavaScript.Tests.Classes;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class DeclareActivityControllerTest
    {
        #region Constants

        private const string CreateClassScript = @"    $create(JS.LinqToJavaScript.Tests.DeclareClassTest, {""activityControls"":[""someid""],""arrayValue"":[1,2,3],""boolValue"":false,""changedControls"":[""testClientId""],""field1LongControl"":$create(JS.LinqToJavaScript.Tests.TestFieldControl, {""allowRequiredValidate"":true,""allowValidate"":true,""clientID"":""testClientId"",""readOnly"":false,""visible"":true}),""intValue"":1,""isNew"":false,""listValue"":[1,2,3],""readOnly"":false});
";

        private const string MainClass = @"Type.registerNamespace('JS.LinqToJavaScript.Tests');

JS.LinqToJavaScript.Tests.DeclareClassTest = function() {
    JS.LinqToJavaScript.Tests.DeclareClassTest.initializeBase(this);

    this._arrayValue = null;
    this._boolValue = null;
    this._field1LongControl = null;
    this._formID = null;
    this._intValue = null;
    this._isNew = null;
    this._listValue = null;
    this._readOnly = null;
    this._validationGroup = null;
}

JS.LinqToJavaScript.Tests.DeclareClassTest.prototype = {
    initialize: function() {
        JS.LinqToJavaScript.Tests.DeclareClassTest.callBaseMethod(this, 'initialize');
    },

    dispose: function() {
        JS.LinqToJavaScript.Tests.DeclareClassTest.callBaseMethod(this, 'dispose');
    },

    get_arrayValue: function() {
        return this._arrayValue;
    },

    set_arrayValue: function(value) {
        this._arrayValue = value;
    },

    get_boolValue: function() {
        return this._boolValue;
    },

    set_boolValue: function(value) {
        this._boolValue = value;
    },

    get_field1LongControl: function() {
        return this._field1LongControl;
    },

    set_field1LongControl: function(value) {
        this._field1LongControl = value;
    },

    get_formID: function() {
        return this._formID;
    },

    set_formID: function(value) {
        this._formID = value;
    },

    get_intValue: function() {
        return this._intValue;
    },

    set_intValue: function(value) {
        this._intValue = value;
    },

    get_isNew: function() {
        return this._isNew;
    },

    set_isNew: function(value) {
        this._isNew = value;
    },

    get_listValue: function() {
        return this._listValue;
    },

    set_listValue: function(value) {
        this._listValue = value;
    },

    get_readOnly: function() {
        return this._readOnly;
    },

    set_readOnly: function(value) {
        this._readOnly = value;
    },

    get_validationGroup: function() {
        return this._validationGroup;
    },

    set_validationGroup: function(value) {
        this._validationGroup = value;
    }
}

JS.LinqToJavaScript.Tests.DeclareClassTest.registerClass('JS.LinqToJavaScript.Tests.DeclareClassTest', Nat.Web.JSControls.ActivityController);";

        private const string SecondClass = @"Type.registerNamespace('JS.LinqToJavaScript.Tests');

JS.LinqToJavaScript.Tests.TestFieldControl = function() {
    JS.LinqToJavaScript.Tests.TestFieldControl.initializeBase(this);

    this._allowRequiredValidate = null;
    this._allowValidate = null;
    this._clientID = null;
    this._readOnly = null;
    this._savedValue = null;
    this._visible = null;
}

JS.LinqToJavaScript.Tests.TestFieldControl.prototype = {
    initialize: function() {
        JS.LinqToJavaScript.Tests.TestFieldControl.callBaseMethod(this, 'initialize');
    },

    dispose: function() {
        JS.LinqToJavaScript.Tests.TestFieldControl.callBaseMethod(this, 'dispose');
    },

    get_allowRequiredValidate: function() {
        return this._allowRequiredValidate;
    },

    set_allowRequiredValidate: function(value) {
        this._allowRequiredValidate = value;
    },

    get_allowValidate: function() {
        return this._allowValidate;
    },

    set_allowValidate: function(value) {
        this._allowValidate = value;
    },

    get_clientID: function() {
        return this._clientID;
    },

    set_clientID: function(value) {
        this._clientID = value;
    },

    get_control: function() {
        return (((this.get_clientID() == null) || (this.get_clientID() == '')) ? null : eval(this.get_clientID()));
    },

    get_enabled: function() {
        var resVal = (($.inArray(this.parent.get_intValue(), this.parent.get_arrayValue()) > -1) && ((this.parent.get_field1LongControl().get_value() == null) || (this.parent.get_intValue() > this.parent.get_field1LongControl().get_value())));
        resVal = (typeof resVal === 'string' || resVal instanceof String) ? resVal.toLowerCase() === 'true' : resVal;
        return resVal;
    },

    get_readOnly: function() {
        return this._readOnly;
    },

    set_readOnly: function(value) {
        this._readOnly = value;
    },

    get_savedValue: function() {
        return this._savedValue;
    },

    set_savedValue: function(value) {
        this._savedValue = value;
    },

    get_value: function() {
        return ((this.get_control() != null) ? this.get_control().getValue() : this.get_savedValue());
    },

    get_visible: function() {
        return this._visible;
    },

    set_visible: function(value) {
        this._visible = value;
    }
}

JS.LinqToJavaScript.Tests.TestFieldControl.registerClass('JS.LinqToJavaScript.Tests.TestFieldControl', Nat.Web.JSControls.ActivityControl);";

        #endregion

        #region Fields

        private readonly LinqToJavaScriptProvider provider = new LinqToJavaScriptProvider();

        #endregion

        #region Public Properties

        public TestContext TestContext { get; set; }

        #endregion

        #region Public Methods and Operators

        [TestMethod]
        public void GetCreateClassScriptTest()
        {
            var item = new TestActivityController
                           {
                               IntValue = 1,
                               BoolValue = false,
                               ArrayValue = new[] { 1, 2, 3 },
                               ListValue = new List<int>(new[] { 1, 2, 3 }),
                           };

            var script = provider.GetCreateClassScript(item);
            Assert.AreEqual(CreateClassScript, script);
        }

        [TestMethod]
        public void GetDeclareAllClassScriptTest()
        {
            var item = new TestActivityController();

            var script = provider.GetDeclareAllClassScript(item);
            Assert.AreEqual(MainClass + "\r\n\r\n" + SecondClass, script);
        }

        [TestMethod]
        public void GetDeclareClassScriptTest()
        {
            var item = new TestActivityController();

            var script = provider.GetDeclareClassScript(item);

            Assert.AreEqual(MainClass, script);
        }

        [TestMethod]
        public void GetValueOfThisTest()
        {
            AssertAreEqual(from => from.IntValue < 2, "(this.get_intValue() < 2)");
            AssertAreEqual(from => !(from.IntValue < 2), "!(this.get_intValue() < 2)");
            AssertAreEqual(from => from.IntValue == 2, "(this.get_intValue() == 2)");
            AssertAreEqual(from => from.IntValue != 2, "(this.get_intValue() != 2)");
            AssertAreEqual(from => from.BoolValue != null && from.BoolValue.Value, "((this.get_boolValue() != null) && this.get_boolValue())");
            AssertAreEqual(from => from.BoolValue != null && (bool)from.BoolValue, "((this.get_boolValue() != null) && this.get_boolValue())");
            AssertAreEqual(
                from => from.IntValue != 2 && from.IntValue < 2, "((this.get_intValue() != 2) && (this.get_intValue() < 2))");
            AssertAreEqual(
                from => (new[] { 1, 2, 3 }).Contains(from.IntValue), "($.inArray(this.get_intValue(), [1,2,3]) > -1)");
            AssertAreEqual(
                from => from.ArrayValue.Contains(from.IntValue),
                "($.inArray(this.get_intValue(), this.get_arrayValue()) > -1)");
            AssertAreEqual(
                from => (new List<int>(new[] { 1, 2, 3 })).Contains(from.IntValue),
                "($.inArray(this.get_intValue(), [1,2,3]) > -1)");
            AssertAreEqual(
                from => from.ListValue.Contains(from.IntValue), "($.inArray(this.get_intValue(), this.get_listValue()) > -1)");
            AssertAreEqual(
                from => from.IntValue < 2 ? from.IntValue < 1 : from.IntValue > 2,
                "((this.get_intValue() < 2) ? (this.get_intValue() < 1) : (this.get_intValue() > 2))");
            AssertAreEqual(
                from => from.Field1LongControl.ClientID.JQueryFindById().JQueryGetValue() == null,
                "(eval(this.get_field1LongControl().get_clientID()).getValue() == null)");
        }

        #endregion

        #region Methods

        private void AssertAreEqual(Expression<Func<TestActivityController, bool>> exp, string script)
        {
            Assert.AreEqual(script, provider.GetQueryText(exp));
        }

        #endregion

        [JavaScriptClass(ClassName = "DeclareClassTest", Namespace = "JS.LinqToJavaScript.Tests", BaseClassName = "Nat.Web.JSControls.ActivityController")]
        public class TestActivityController : ActivityController
        {
            #region Constructors and Destructors

            public TestActivityController()
            {
                Field1LongControl = new TestFieldControl<TestActivityController, long>(this, "someid", "someid");
                Field1LongControl.ClientID = "testClientId";
                Field1LongControl.EnabledExpression =
                    r => r.ArrayValue.Contains(r.IntValue)
                         && (r.Field1LongControl.Value == null || r.IntValue > r.Field1LongControl.Value);
                
                InitializeControls(Field1LongControl);
            }

            #endregion

            #region Public Properties

            [JavaScriptProperty]
            public int[] ArrayValue { get; set; }

            [JavaScriptProperty]
            public TestFieldControl<TestActivityController, long> Field1LongControl { get; set; }

            [JavaScriptProperty]
            public int IntValue { get; set; }

            [JavaScriptProperty]
            public bool? BoolValue { get; set; }

            [JavaScriptProperty]
            public List<int> ListValue { get; set; }

            #endregion
        }
    }
}