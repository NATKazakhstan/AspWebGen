namespace JS.LinqToJavaScript.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Web.UI;

    using JS.LinqToJavaScript.Tests.Classes;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class ActivityControllerServerSideTest
    {
        public TestContext TestContext { get; set; }

        [TestMethod]
        public void TestActivity()
        {
            var controller = new TestActivityController();
            controller.Initialize(
                new Control(),
                new Dictionary<string, object>
                    {
                        { "field1", 2L },
                        { "field2", true },
                        { "field3", string.Empty },
                        { "field4", new DateTime(2012, 1, 1) },
                        { "field5", (decimal)1.1 },
                    });
            controller.ComputeActivities();
            Assert.AreEqual(false, controller.Field2BoolControl.Enabled, "Must be disabled");
            Assert.AreEqual(true, controller.Field2BoolControl.Visible, "Must be visible");
        }
    }
}
