﻿using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;

using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq.Expressions;

namespace JS.LinqToJavaScript.Tests
{
    using System.Text.RegularExpressions;

    /// <summary>
    /// Summary description for SimpleExpressionTest
    /// </summary>
    [TestClass]
    public class SimpleExpressionTest
    {
        LinqToJavaScriptProvider Provider = new LinqToJavaScriptProvider();

        public SimpleExpressionTest()
        {
        }

        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext { get; set; }

        #region Additional test attributes
        //
        // You can use the following additional attributes as you write your tests:
        //
        // Use ClassInitialize to run code before running the first test in the class
        // [ClassInitialize()]
        // public static void MyClassInitialize(TestContext testContext) { }
        //
        // Use ClassCleanup to run code after all tests in a class have run
        // [ClassCleanup()]
        // public static void MyClassCleanup() { }
        //
        // Use TestInitialize to run code before running each test 
        // [TestInitialize()]
        // public void MyTestInitialize() { }
        //
        // Use TestCleanup to run code after each test has run
        // [TestCleanup()]
        // public void MyTestCleanup() { }
        //
        #endregion

        [TestMethod]
        public void TestMethodOperators()
        {
            var intValue = 1;
            var listValue = new List<int>(new[] { 1, 2, 3 });
            var arrayValue = new[] { 1, 2, 3 };
            var enumerate = (IEnumerable<int>)new[] { 1, 2, 3 };

            AssertAreEqual(() => intValue < 2, "(1 < 2)");
            AssertAreEqual(() => !(intValue < 2), "!(1 < 2)");
            AssertAreEqual(() => intValue == 2, "(1 == 2)");
            AssertAreEqual(() => intValue.Equals(2), "(1 == 2)");
            AssertAreEqual(() => intValue != 2, "(1 != 2)");
            AssertAreEqual(() => intValue != 2 && intValue < 2, "((1 != 2) && (1 < 2))");
            AssertAreEqual(() => (new[] { 1, 2, 3 }).Contains(intValue), "($.inArray(1, [1,2,3]) > -1)");
            AssertAreEqual(() => arrayValue.Contains(intValue), "($.inArray(1, [1,2,3]) > -1)");
            AssertAreEqual(() => (new List<int>(new[] { 1, 2, 3 })).Contains(intValue), "($.inArray(1, [1,2,3]) > -1)");
            AssertAreEqual(() => listValue.Contains(intValue), "($.inArray(1, [1,2,3]) > -1)");
            AssertAreEqual(() => intValue < 2 ? intValue < 1 : intValue > 2, "((1 < 2) ? (1 < 1) : (1 > 2))");
            AssertAreEqual(() => enumerate.Any(), "([1,2,3] && [1,2,3].length > 0)");
            AssertAreEqual(() => enumerate.Count(), "(([1,2,3]) ? ([1,2,3]).length : 0)");
        }

        [TestMethod]
        public void TestMethodNullable()
        {
            int? intValue = 1;
            int? intValueNull = null;
            var listValue = new List<int?>(new int?[] { 1, 2, 3, null });
            var arrayValue = new int?[] { 1, 2, 3, null };

            AssertAreEqual(() => intValue == null, "(1 == null)");
            AssertAreEqual(() => intValue != null, "(1 != null)");
            AssertAreEqual(() => intValueNull == 1, "(null == 1)");
            AssertAreEqual(() => (new int?[] { 1, 2, 3, null }).Contains(intValue), "($.inArray(1, [1,2,3,null]) > -1)");
            AssertAreEqual(() => arrayValue.Contains(intValue), "($.inArray(1, [1,2,3,null]) > -1)");
            AssertAreEqual(() => (new List<int?>(new int?[] { 1, 2, 3, null })).Contains(intValue), "($.inArray(1, [1,2,3,null]) > -1)");
            AssertAreEqual(() => listValue.Contains(intValue), "($.inArray(1, [1,2,3,null]) > -1)");
            AssertAreEqual(() => arrayValue.Contains(intValueNull), "($.inArray(null, [1,2,3,null]) > -1)");
            AssertAreEqual(() => listValue.Contains(intValueNull), "($.inArray(null, [1,2,3,null]) > -1)");
        }

        [TestMethod]
        public void TestMethodConverts()
        {
            int? intValueN = 1;
            int intValue = 1;
            long? longValueN = 1;
            long longValue = 1;

            AssertAreEqual(() => intValue == longValue, "(1 == 1)");
            AssertAreEqual(() => intValueN == longValueN, "(1 == 1)");
            AssertAreEqual(() => intValue == 1.1F, "(1 == 1.1)");
            AssertAreEqual(() => intValue == 1.1D, "(1 == 1.1)");
        }

        [TestMethod]
        public void TestStringMethod()
        {
            var x = "value";
            AssertAreEqual(() => x.Length > 1, "('value'.length > 1)");
        }

        [TestMethod]
        public void TestStringQuotes()
        {
            var x = "value 's'";
            AssertAreEqual(() => x.Length > 1, @"('value \'s\''.length > 1)");
        }

        [TestMethod]
        public void TestStringDoubleQuotes()
        {
            var x = "value \"s\"";
            AssertAreEqual(() => x.Length > 1, @"('value ""s""'.length > 1)");
        }

        [TestMethod]
        public void TestRegexMethod()
        {
            var regex = new Regex(@"^[\d\-]+$");
            var x = "value" ;
            AssertAreEqual(() => regex.IsMatch(x), @"'value'.match(/^[\d\-]+$/gi)");
        }

        [TestMethod]
        public void TestStaticValue()
        {
            AssertAreEqual(() => "01" == someStaticValue, "('01' == 'staticValue')");
        }

        [TestMethod]
        public void TestConstValue()
        {
            var x = "value";
            AssertAreEqual(() => x == someConstValue, "('value' == 'constValue')");
        }

        [TestMethod]
        public void TestPropertyValue()
        {
            AssertAreEqual(() => "01" == somePropertyValue, "('01' == 'propertyValue')");
        }

        private static string someStaticValue = "staticValue";
        private const string someConstValue = "constValue";
        private static string somePropertyValue => "propertyValue";

        private void AssertAreEqual(Expression<Func<bool>> exp, string script)
        {
            Assert.AreEqual(script, Provider.GetQueryText(exp));
        }

        private void AssertAreEqual(Expression<Func<int>> exp, string script)
        {
            Assert.AreEqual(script, Provider.GetQueryText(exp));
        }
    }
}
