namespace JS.LinqToJavaScript.Tests
{
    using System;
    using System.Linq.Expressions;

    using JS.LinqToJavaScript.Attributes;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class DeclareExpressionsInClassTest
    {
        #region Constants result of scripts

        public const string CreateClassScript = @"    $create(JS.LinqToJavaScript.Tests.DeclareClassTest, {});
";

        #region GetDeclareTestClass1ScriptTest

        public const string DeclareClassScript1 = @"Type.registerNamespace('JS.LinqToJavaScript.Tests');

JS.LinqToJavaScript.Tests.DeclareClassTest = function() {
    JS.LinqToJavaScript.Tests.DeclareClassTest.initializeBase(this);

    this._function1 = function(r) {
        return (((r.Field1.length > 100) && this.checkValue(r.Field1)) ? 'testMessage' : null);
    };
}

JS.LinqToJavaScript.Tests.DeclareClassTest.prototype = {
    initialize: function() {
        JS.LinqToJavaScript.Tests.DeclareClassTest.callBaseMethod(this, 'initialize');
    },

    dispose: function() {
        JS.LinqToJavaScript.Tests.DeclareClassTest.callBaseMethod(this, 'dispose');
    },

    get_function1: function() {
        return this._function1;
    }
}

JS.LinqToJavaScript.Tests.DeclareClassTest.registerClass('JS.LinqToJavaScript.Tests.DeclareClassTest', Nat.Web.JSControls.TestClass);";

        #endregion

        #region GetDeclareTestClass2ScriptTest

        public const string DeclareClassScript2 = @"Type.registerNamespace('JS.LinqToJavaScript.Tests');

JS.LinqToJavaScript.Tests.DeclareClassTest = function() {
    JS.LinqToJavaScript.Tests.DeclareClassTest.initializeBase(this);

    this._functionsCollection = [function(r) {
        return ((r.Field2.length == 0) ? 'testMessage' : null);
    }, function(r) {
        return ((r.Field2.length > 50) ? 'testMessage' : null);
    }];
}

JS.LinqToJavaScript.Tests.DeclareClassTest.prototype = {
    initialize: function() {
        JS.LinqToJavaScript.Tests.DeclareClassTest.callBaseMethod(this, 'initialize');
    },

    dispose: function() {
        JS.LinqToJavaScript.Tests.DeclareClassTest.callBaseMethod(this, 'dispose');
    },

    get_functionsCollection: function() {
        return this._functionsCollection;
    },

    set_functionsCollection: function(value) {
        this._functionsCollection = value;
    }
}

JS.LinqToJavaScript.Tests.DeclareClassTest.registerClass('JS.LinqToJavaScript.Tests.DeclareClassTest', Nat.Web.JSControls.TestClass);";

        #endregion

        #region GetDeclareTestClass3ScriptTest

        public const string DeclareClassScript3 = @"Type.registerNamespace('JS.LinqToJavaScript.Tests');

JS.LinqToJavaScript.Tests.DeclareClassTest = function() {
    JS.LinqToJavaScript.Tests.DeclareClassTest.initializeBase(this);

    this._function1 = function(r) {
        return (function(valid) {
        return (valid ? 'testMessage' : null);
    }).call(this, (function(str) {
        return (str == 'xxx');
    }).call(this, r.Field1));
    };
}

JS.LinqToJavaScript.Tests.DeclareClassTest.prototype = {
    initialize: function() {
        JS.LinqToJavaScript.Tests.DeclareClassTest.callBaseMethod(this, 'initialize');
    },

    dispose: function() {
        JS.LinqToJavaScript.Tests.DeclareClassTest.callBaseMethod(this, 'dispose');
    },

    get_function1: function() {
        return this._function1;
    }
}

JS.LinqToJavaScript.Tests.DeclareClassTest.registerClass('JS.LinqToJavaScript.Tests.DeclareClassTest', Nat.Web.JSControls.TestClass);";

        #endregion

        #region GetDeclareTestClass4ScriptTest

        public const string DeclareClassScript4 = @"Type.registerNamespace('JS.LinqToJavaScript.Tests');

JS.LinqToJavaScript.Tests.DeclareClassTest = function() {
    JS.LinqToJavaScript.Tests.DeclareClassTest.initializeBase(this);

    this._function1 = function(r) {
        return $create(JS.LinqToJavaScript.Tests.TestClassItem, { field1: r.Field1, field2: 'testMessage', field3: r });
    };
}

JS.LinqToJavaScript.Tests.DeclareClassTest.prototype = {
    initialize: function() {
        JS.LinqToJavaScript.Tests.DeclareClassTest.callBaseMethod(this, 'initialize');
    },

    dispose: function() {
        JS.LinqToJavaScript.Tests.DeclareClassTest.callBaseMethod(this, 'dispose');
    },

    get_function1: function() {
        return this._function1;
    }
}

JS.LinqToJavaScript.Tests.DeclareClassTest.registerClass('JS.LinqToJavaScript.Tests.DeclareClassTest', Nat.Web.JSControls.TestClass);";

        #endregion

        #region GetDeclareTestClass5ScriptTest

        public const string DeclareClassScript5 = @"Type.registerNamespace('JS.LinqToJavaScript.Tests');

JS.LinqToJavaScript.Tests.DeclareClassTest = function() {
    JS.LinqToJavaScript.Tests.DeclareClassTest.initializeBase(this);

    this._function1 = function(r) {
        return (function(valid) {
        return (valid ? 'testMessage' : null);
    }).call(this, (function(r, ind) {
        return ((r.Field1 == 'xxx') && (ind == 1));
    }).call(this, r, 1));
    };
}

JS.LinqToJavaScript.Tests.DeclareClassTest.prototype = {
    initialize: function() {
        JS.LinqToJavaScript.Tests.DeclareClassTest.callBaseMethod(this, 'initialize');
    },

    dispose: function() {
        JS.LinqToJavaScript.Tests.DeclareClassTest.callBaseMethod(this, 'dispose');
    },

    get_function1: function() {
        return this._function1;
    }
}

JS.LinqToJavaScript.Tests.DeclareClassTest.registerClass('JS.LinqToJavaScript.Tests.DeclareClassTest', Nat.Web.JSControls.TestClass);";

        #endregion

        #region GetDeclareTestClass6ScriptTest

        public const string DeclareClassScript6 = @"Type.registerNamespace('JS.LinqToJavaScript.Tests');

JS.LinqToJavaScript.Tests.DeclareClassTest = function() {
    JS.LinqToJavaScript.Tests.DeclareClassTest.initializeBase(this);

    this._function1 = function(r) {
        return (function(valid) {
        return (valid ? 'testMessage' : null);
    }).call(this, (function(str) {
        return (str == 'xxx');
    }).call(this, (function(r) {
        return r.Field1;
    }).call(this, r)));
    };
}

JS.LinqToJavaScript.Tests.DeclareClassTest.prototype = {
    initialize: function() {
        JS.LinqToJavaScript.Tests.DeclareClassTest.callBaseMethod(this, 'initialize');
    },

    dispose: function() {
        JS.LinqToJavaScript.Tests.DeclareClassTest.callBaseMethod(this, 'dispose');
    },

    get_function1: function() {
        return this._function1;
    }
}

JS.LinqToJavaScript.Tests.DeclareClassTest.registerClass('JS.LinqToJavaScript.Tests.DeclareClassTest', Nat.Web.JSControls.TestClass);";

        #endregion

        #endregion

        #region Public Properties

        public TestContext TestContext { get; set; }

        #endregion

        private readonly LinqToJavaScriptProvider provider = new LinqToJavaScriptProvider();


        [TestMethod]
        public void GetCreateTestClass1ScriptTest()
        {
            var item = new TestClass1();
            var script = provider.GetCreateClassScript(item);
            Assert.AreEqual(CreateClassScript, script);
        }

        [TestMethod]
        public void GetCreateTestClass2ScriptTest()
        {
            var item = new TestClass2();
            var script = provider.GetCreateClassScript(item);
            Assert.AreEqual(CreateClassScript, script);
        }

        [TestMethod]
        public void GetDeclareTestClass1ScriptTest()
        {
            var message = "testMessage";
            var item = new TestClass1();
            item.Function1 = r => r.Field1.Length > 100 && item.CheckValue(r.Field1) ? message : null;

            var script = provider.GetDeclareClassScript(item);
            Assert.AreEqual(DeclareClassScript1, script);
        }

        [TestMethod]
        public void GetDeclareTestClass2ScriptTest()
        {
            var message = "testMessage";
            var item = new TestClass2();
            item.FunctionsCollection = new Expression<Func<Row, string>>[]
                {
                    r => r.Field2.Length == 0 ? message : null,
                    r => r.Field2.Length > 50 ? message : null,
                };

            var script = provider.GetDeclareClassScript(item);
            Assert.AreEqual(DeclareClassScript2, script);
        }

        [TestMethod]
        public void GetDeclareTestClass3ScriptTest()
        {
            var message = "testMessage";
            var item = new TestClass1();
            Expression<Func<string, bool>> expInvokeBool = str => str == "xxx";
            Expression<Func<bool, string>> expInvokeMessage = valid => valid ? message : null;
            var param = Expression.Parameter(typeof(Row), "r");
            var body = Expression.Invoke(expInvokeBool, Expression.Property(param, "Field1"));
            body = Expression.Invoke(expInvokeMessage, body);
            
            item.Function1 = Expression.Lambda<Func<Row, string>>(body, param);

            var script = provider.GetDeclareClassScript(item);
            Assert.AreEqual(DeclareClassScript3, script);
        }

        [TestMethod]
        public void GetDeclareTestClass4ScriptTest()
        {
            var message = "testMessage";
            var item = new TestClass4();

            item.Function1 = r => new TestClass4Item { Field1 = r.Field1, Field2 = message, Field3 = r };

            var script = provider.GetDeclareClassScript(item);
            Assert.AreEqual(DeclareClassScript4, script);
        }

        [TestMethod]
        public void GetDeclareTestClass5ScriptTest()
        {
            var message = "testMessage";
            var item = new TestClass1();
            Expression<Func<Row, int, bool>> expInvokeBool = (r, ind) => r.Field1 == "xxx" && ind == 1;
            Expression<Func<bool, string>> expInvokeMessage = valid => valid ? message : null;
            var param = Expression.Parameter(typeof(Row), "r");
            var body = Expression.Invoke(expInvokeBool, param, Expression.Constant(1));
            body = Expression.Invoke(expInvokeMessage, body);

            item.Function1 = Expression.Lambda<Func<Row, string>>(body, param);

            var script = provider.GetDeclareClassScript(item);
            Assert.AreEqual(DeclareClassScript5, script);
        }

        [TestMethod]
        public void GetDeclareTestClass6ScriptTest()
        {
            var message = "testMessage";
            var item = new TestClass1();
            Expression<Func<Row, string>> expInvokeStr = GetExpressionTest6<Row>();
            Expression<Func<string, bool>> expInvokeBool = str => str == "xxx";
            Expression<Func<bool, string>> expInvokeMessage = valid => valid ? message : null;
            var param = Expression.Parameter(typeof(Row), "r");
            var body = Expression.Invoke(expInvokeBool, Expression.Invoke(expInvokeStr, param));
            body = Expression.Invoke(expInvokeMessage, body);

            item.Function1 = Expression.Lambda<Func<Row, string>>(body, param);

            var script = provider.GetDeclareClassScript(item);
            Assert.AreEqual(DeclareClassScript6, script);
        }

        private Expression<Func<TRow, string>> GetExpressionTest6<TRow>()
            where TRow : IRow
        {
            return r => r.Field1;
        }

        [JavaScriptClass(ClassName = "DeclareClassTest", Namespace = "JS.LinqToJavaScript.Tests", BaseClassName = "Nat.Web.JSControls.TestClass")]
        public class TestClass1
        {
            [JavaScriptProperty(ReadOnly = true)]
            public Expression<Func<Row, string>> Function1 { get; set; }

            [JavaScriptFunction(DeclaredInBaseClass = true)]
            public bool CheckValue(string v)
            {
                return true;
            }
        }

        [JavaScriptClass(ClassName = "DeclareClassTest", Namespace = "JS.LinqToJavaScript.Tests", BaseClassName = "Nat.Web.JSControls.TestClass")]
        public class TestClass2
        {
            [JavaScriptProperty]
            public Expression<Func<Row, string>>[] FunctionsCollection { get; set; }
        }

        [JavaScriptClass(ClassName = "DeclareClassTest", Namespace = "JS.LinqToJavaScript.Tests", BaseClassName = "Nat.Web.JSControls.TestClass")]
        public class TestClass4
        {
            [JavaScriptProperty(ReadOnly = true)]
            public Expression<Func<Row, TestClass4Item>> Function1 { get; set; }
        }

        [JavaScriptClass(ClassName = "TestClassItem", Namespace = "JS.LinqToJavaScript.Tests", BaseClassName = "Nat.Web.JSControls.TestClassItemBase")]
        public class TestClass4Item
        {
            public string Field1;
            public string Field2;
            public Row Field3;
        }

        public class Row : IRow
        {
            public string Field1 { get; set; }
            public string Field2 { get; set; }
        }

        public interface IRow
        {
            string Field1 { get; set; }
            string Field2 { get; set; }
        }
    }
}
