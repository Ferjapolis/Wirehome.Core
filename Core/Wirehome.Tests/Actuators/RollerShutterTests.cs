﻿using System;
using System.Threading.Tasks;
using Wirehome.Actuators.RollerShutters;
using Wirehome.Components;
using Wirehome.Contracts.Components.States;
using Wirehome.Contracts.Core;
using Wirehome.Contracts.Settings;
using Wirehome.Tests.Mockups;
using Wirehome.Tests.Mockups.Adapters;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Wirehome.Tests.Actuators
{
    [TestClass]
    public class RollerShutterTests
    {
        [TestMethod]
        public void RollerShutter_Reset()
        {
            var testController = new TestController();
            var adapter = new TestRollerShutterAdapter();
            var rollerShutter = new RollerShutter("Test", adapter, testController.GetInstance<ITimerService>(), testController.GetInstance<ISettingsService>());

            rollerShutter.TryReset();
            Assert.AreEqual(1, adapter.StartMoveUpCalledCount);
            Assert.IsTrue(rollerShutter.GetState().Has(PowerState.On));
            Assert.IsTrue(rollerShutter.GetState().Has(VerticalMovingState.MovingUp));
        }

        [TestMethod]
        public void RollerShutter_MoveUp()
        {
            var testController = new TestController();
            var adapter = new TestRollerShutterAdapter();
            var rollerShutter = new RollerShutter("Test", adapter, testController.GetInstance<ITimerService>(), testController.GetInstance<ISettingsService>());
            rollerShutter.TryReset();
            rollerShutter.TryMoveUp();

            Assert.AreEqual(2, adapter.StartMoveUpCalledCount);
            Assert.IsTrue(rollerShutter.GetState().Has(PowerState.On));
            Assert.IsTrue(rollerShutter.GetState().Has(VerticalMovingState.MovingUp));
        }

        [TestMethod]
        public void RollerShutter_MoveDown()
        {
            var testController = new TestController();
            var adapter = new TestRollerShutterAdapter();
            var rollerShutter = new RollerShutter("Test", adapter, testController.GetInstance<ITimerService>(), testController.GetInstance<ISettingsService>());
            rollerShutter.TryReset();
            rollerShutter.TryMoveDown();

            Assert.AreEqual(1, adapter.StartMoveUpCalledCount);
            Assert.AreEqual(1, adapter.StartMoveDownCalledCount);
            Assert.IsTrue(rollerShutter.GetState().Has(PowerState.On));
            Assert.IsTrue(rollerShutter.GetState().Has(VerticalMovingState.MovingDown));
        }

        [TestMethod]
        public void RollerShutter_AutoOff()
        {
            var testController = new TestController();
            var adapter = new TestRollerShutterAdapter();
            var rollerShutter = new RollerShutter("Test", adapter, testController.GetInstance<ITimerService>(), testController.GetInstance<ISettingsService>());
            rollerShutter.TryReset();
            rollerShutter.TryMoveDown();

            Assert.AreEqual(1, adapter.StartMoveUpCalledCount);
            Assert.AreEqual(1, adapter.StartMoveDownCalledCount);

            testController.Tick(TimeSpan.FromHours(1));

            Assert.AreEqual(1, adapter.StopCalledCount);
        }

        [TestMethod]
        public void RollerShutter_Stop()
        {
            var testController = new TestController();
            var adapter = new TestRollerShutterAdapter();
            var rollerShutter = new RollerShutter("Test", adapter, testController.GetInstance<ITimerService>(), testController.GetInstance<ISettingsService>());
            rollerShutter.TryReset();
            rollerShutter.TryTurnOff();

            Assert.AreEqual(1, adapter.StartMoveUpCalledCount);
            Assert.AreEqual(1, adapter.StopCalledCount);

            Assert.IsTrue(rollerShutter.GetState().Has(VerticalMovingState.Stopped));
            Assert.IsTrue(rollerShutter.GetState().Has(PowerState.Off));
        }
    }
}