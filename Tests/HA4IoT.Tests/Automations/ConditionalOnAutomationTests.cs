﻿using FluentAssertions;
using HA4IoT.Automations;
using HA4IoT.Contracts.Actuators;
using HA4IoT.Contracts.Automations;
using HA4IoT.Contracts.Services.Daylight;
using HA4IoT.Contracts.Services.System;
using HA4IoT.Tests.Mockups;
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;

namespace HA4IoT.Tests.Automations
{
    [TestClass]
    public class ConditionalOnAutomationTests
    {
        [TestMethod]
        public void Empty_ConditionalOnAutomation()
        {
            var testController = new TestController();
            var automation = new ConditionalOnAutomation(AutomationIdGenerator.EmptyId,
                testController.GetInstance<ISchedulerService>(),
                testController.GetInstance<IDateTimeService>(),
                testController.GetInstance<IDaylightService>());

            var testButtonFactory = testController.GetInstance<TestButtonFactory>();
            var testStateMachineFactory = new TestStateMachineFactory();

            var testButton = testButtonFactory.CreateTestButton();
            var testOutput = testStateMachineFactory.CreateTestStateMachineWithOnOffStates();

            automation.WithTrigger(testButton.PressedShortlyTrigger);
            automation.WithActuator(testOutput);
            
            testOutput.GetState().ShouldBeEquivalentTo(BinaryStateId.Off);
            testButton.PressShortly();
            testOutput.GetState().ShouldBeEquivalentTo(BinaryStateId.On);
        }
    }
}
