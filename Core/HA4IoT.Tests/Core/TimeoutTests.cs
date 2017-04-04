﻿using System;
using HA4IoT.Contracts.Services.System;
using HA4IoT.Services.System;
using HA4IoT.Tests.Mockups;
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;

namespace HA4IoT.Tests.Core
{
    [TestClass]
    public class TimeoutTests
    {
        [TestMethod]
        public void Timeout_Elapsed()
        {
            var testController = new TestController();
            var timeout = new Timeout(testController.GetInstance<ITimerService>());

            var eventFired = false;
            timeout.Elapsed += (s,e) => eventFired = true;

            Assert.IsTrue(timeout.IsEnabled);
            Assert.IsTrue(timeout.IsElapsed);
            Assert.IsFalse(eventFired);

            timeout.Start(TimeSpan.FromSeconds(2));

            Assert.IsTrue(timeout.IsEnabled);
            Assert.IsFalse(timeout.IsElapsed);
            Assert.IsFalse(eventFired);

            testController.Tick(TimeSpan.FromSeconds(1));

            Assert.IsTrue(timeout.IsEnabled);
            Assert.IsFalse(timeout.IsElapsed);
            Assert.IsFalse(eventFired);

            testController.Tick(TimeSpan.FromSeconds(1));

            Assert.IsTrue(timeout.IsEnabled);
            Assert.IsTrue(timeout.IsElapsed);
            Assert.IsTrue(eventFired);
        }
    }
}
