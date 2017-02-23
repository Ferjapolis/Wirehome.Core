﻿using System;
using HA4IoT.Actuators.Lamps;
using HA4IoT.Automations;
using HA4IoT.Components;
using HA4IoT.Contracts.Components;
using HA4IoT.Contracts.Components.States;
using HA4IoT.Contracts.Services.Daylight;
using HA4IoT.Contracts.Services.Settings;
using HA4IoT.Contracts.Services.System;
using HA4IoT.Sensors.Buttons;
using HA4IoT.Sensors.MotionDetectors;
using HA4IoT.Tests.Mockups;
using HA4IoT.Tests.Mockups.Adapters;
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;

namespace HA4IoT.Tests.Actuators
{
    [TestClass]
    public class AutomaticTurnOnAndOffAutomationTests
    {
        [TestMethod]
        public void Should_TurnOn_IfMotionDetected()
        {
            var testController = new TestController();
            var adapter = new TestMotionDetectorAdapter();
            var motionDetector = new MotionDetector(
                "Test", 
                adapter, 
                testController.GetInstance<ISchedulerService>(),
                testController.GetInstance<ISettingsService>());

            var automation = new FlipFlopAutomation(
                "Test",
                testController.GetInstance<IDateTimeService>(), 
                testController.GetInstance<ISchedulerService>(),
                testController.GetInstance<ISettingsService>(),
                testController.GetInstance<IDaylightService>());

            var output = new Lamp("Test", new TestBinaryOutputAdapter());
            Assert.AreEqual(true, output.GetState().Has(PowerState.Off));

            automation.WithTrigger(motionDetector);
            automation.WithTarget(output);

            adapter.Invoke();

            Assert.AreEqual(true, output.GetState().Has(PowerState.On));
        }

        [TestMethod]
        public void Should_TurnOn_IfButtonPressedShort()
        {
            var testController = new TestController();
            
            var automation = new FlipFlopAutomation(
                "Test",
                testController.GetInstance<IDateTimeService>(),
                testController.GetInstance<ISchedulerService>(),
                testController.GetInstance<ISettingsService>(),
                testController.GetInstance<IDaylightService>());

            var buttonAdapter = new TestButtonAdapter();
            var button = new Button("Test", buttonAdapter, testController.GetInstance<ITimerService>(), testController.GetInstance<ISettingsService>());
            var output = new Lamp("Test", new TestBinaryOutputAdapter());
            Assert.AreEqual(true, output.GetState().Has(PowerState.Off));

            automation.WithFlipTrigger(button.PressedShortlyTrigger);
            automation.WithTarget(output);

            buttonAdapter.Touch();

            Assert.AreEqual(true, output.GetState().Has(PowerState.On));
        }

        [TestMethod]
        public void Should_NotTurnOn_IfMotionDetected_AndTimeRangeConditionIs_NotFulfilled()
        {
            var testController = new TestController();
            testController.SetTime(TimeSpan.Parse("18:00:00"));
            var adapter = new TestMotionDetectorAdapter();
            var motionDetector = new MotionDetector(
                "Test",
                adapter,
                testController.GetInstance<ISchedulerService>(),
                testController.GetInstance<ISettingsService>());

            var automation = new FlipFlopAutomation(
                "Test",
                testController.GetInstance<IDateTimeService>(),
                testController.GetInstance<ISchedulerService>(),
                testController.GetInstance<ISettingsService>(),
                testController.GetInstance<IDaylightService>());

            var output = new Lamp("Test", new TestBinaryOutputAdapter());
            Assert.AreEqual(true, output.GetState().Has(PowerState.Off));

            automation.WithTurnOnWithinTimeRange(() => TimeSpan.Parse("10:00:00"), () => TimeSpan.Parse("15:00:00"));
            automation.WithTrigger(motionDetector);
            automation.WithTarget(output);

            adapter.Invoke();

            Assert.AreEqual(true, output.GetState().Has(PowerState.Off));
        }

        [TestMethod]
        public void Should_TurnOn_IfButtonPressed_EvenIfTimeRangeConditionIs_NotFulfilled()
        {
            var testController = new TestController();
            testController.SetTime(TimeSpan.Parse("18:00:00"));

            var automation = new FlipFlopAutomation(
                "Test",
                testController.GetInstance<IDateTimeService>(),
                testController.GetInstance<ISchedulerService>(),
                testController.GetInstance<ISettingsService>(),
                testController.GetInstance<IDaylightService>());

            var buttonAdapter = new TestButtonAdapter();
            var button = new Button("Test", buttonAdapter, testController.GetInstance<ITimerService>(), testController.GetInstance<ISettingsService>());
            var output = new Lamp("Test", new TestBinaryOutputAdapter());
            Assert.AreEqual(true, output.GetState().Has(PowerState.Off));

            automation.WithTurnOnWithinTimeRange(() => TimeSpan.Parse("10:00:00"), () => TimeSpan.Parse("15:00:00"));
            automation.WithFlipTrigger(button.PressedShortlyTrigger);
            automation.WithTarget(output);

            buttonAdapter.Touch();

            Assert.AreEqual(true, output.GetState().Has(PowerState.On));
        }

        [TestMethod]
        public void Should_NotTurnOn_IfMotionDetected_AndSkipConditionIs_Fulfilled()
        {
            var testController = new TestController();
            testController.SetTime(TimeSpan.Parse("14:00:00"));
            var adapter = new TestMotionDetectorAdapter();
            var motionDetector = new MotionDetector(
                "Test",
                adapter,
                testController.GetInstance<ISchedulerService>(),
                testController.GetInstance<ISettingsService>());

            var automation = new FlipFlopAutomation(
                "Test",
                testController.GetInstance<IDateTimeService>(),
                testController.GetInstance<ISchedulerService>(),
                testController.GetInstance<ISettingsService>(),
                testController.GetInstance<IDaylightService>());

            var output = new Lamp("Test", new TestBinaryOutputAdapter());
            Assert.AreEqual(true, output.GetState().Has(PowerState.Off));

            automation.WithTrigger(motionDetector);
            automation.WithTarget(output);

            var other2 = new Lamp("Test", new TestBinaryOutputAdapter());
            other2.TryTurnOn();

            IComponent[] otherActuators =
            {
                new Lamp("Test", new TestBinaryOutputAdapter()),
                other2
            };

            automation.WithSkipIfAnyActuatorIsAlreadyOn(otherActuators);

            adapter.Invoke();

            Assert.AreEqual(true, output.GetState().Has(PowerState.Off));
        }

        [TestMethod]
        public void Should_TurnOn_IfMotionDetected_AndSkipConditionIs_NotFulfilled()
        {
            var testController = new TestController();
            testController.SetTime(TimeSpan.Parse("14:00:00"));
            var adapter = new TestMotionDetectorAdapter();
            var motionDetector = new MotionDetector(
                "Test",
                adapter,
                testController.GetInstance<ISchedulerService>(),
                testController.GetInstance<ISettingsService>());

            var automation = new FlipFlopAutomation(
                "Test",
                testController.GetInstance<IDateTimeService>(),
                testController.GetInstance<ISchedulerService>(),
                testController.GetInstance<ISettingsService>(),
                testController.GetInstance<IDaylightService>());

            var output = new Lamp("Test", new TestBinaryOutputAdapter());
            Assert.AreEqual(true, output.GetState().Has(PowerState.Off));

            automation.WithTrigger(motionDetector);
            automation.WithTarget(output);

            IComponent[] otherActuators =
            {
                new Lamp("Test", new TestBinaryOutputAdapter()),
                new Lamp("Test", new TestBinaryOutputAdapter())
            };

            automation.WithSkipIfAnyActuatorIsAlreadyOn(otherActuators);

            adapter.Invoke();

            Assert.AreEqual(true, output.GetState().Has(PowerState.On));
        }

        [TestMethod]
        public void Should_TurnOff_IfButtonPressed_WhileTargetIsAlreadyOn()
        {
            var testController = new TestController();
            testController.SetTime(TimeSpan.Parse("14:00:00"));

            var automation = new FlipFlopAutomation(
                "Test",
                testController.GetInstance<IDateTimeService>(),
                testController.GetInstance<ISchedulerService>(),
                testController.GetInstance<ISettingsService>(),
                testController.GetInstance<IDaylightService>());

            var buttonAdapter = new TestButtonAdapter();
            var button = new Button("Test", buttonAdapter, testController.GetInstance<ITimerService>(), testController.GetInstance<ISettingsService>());
            var output = new Lamp("Test", new TestBinaryOutputAdapter());
            Assert.AreEqual(true, output.GetState().Has(PowerState.Off));

            automation.WithFlipTrigger(button.PressedShortlyTrigger);
            automation.WithTarget(output);

            IComponent[] otherActuators =
            {
                new Lamp("Test", new TestBinaryOutputAdapter()),
                new Lamp("Test", new TestBinaryOutputAdapter())
            };

            automation.WithSkipIfAnyActuatorIsAlreadyOn(otherActuators);

            buttonAdapter.Touch();
            Assert.AreEqual(true, output.GetState().Has(PowerState.On));

            buttonAdapter.Touch();
            Assert.AreEqual(true, output.GetState().Has(PowerState.On));

            automation.WithTurnOffIfButtonPressedWhileAlreadyOn();
            buttonAdapter.Touch();
            Assert.AreEqual(true, output.GetState().Has(PowerState.Off));
        }
    }
}
