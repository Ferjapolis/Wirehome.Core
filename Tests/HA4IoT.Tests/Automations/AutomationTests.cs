﻿using System;
using FluentAssertions;
using HA4IoT.Actuators.StateMachines;
using HA4IoT.Automations;
using HA4IoT.Conditions;
using HA4IoT.Conditions.Specialized;
using HA4IoT.Contracts.Actuators;
using HA4IoT.Contracts.Automations;
using HA4IoT.Services.Backup;
using HA4IoT.Services.StorageService;
using HA4IoT.Settings;
using HA4IoT.Tests.Mockups;
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;

namespace HA4IoT.Tests.Automations
{
    [TestClass]
    public class AutomationTests
    {
        [TestMethod]
        public void Automation_Toggle()
        {
            var timer = new TestTimerService();
            var testButtonFactory = new TestButtonFactory(timer, new SettingsService(new BackupService(), new StorageService()));
            var testStateMachineFactory = new TestStateMachineFactory();

            var testButton = testButtonFactory.CreateTestButton();
            var testOutput = testStateMachineFactory.CreateTestStateMachineWithOnOffStates();

            CreateAutomation()
                .WithTrigger(testButton.PressedShortlyTrigger)
                .WithActionIfConditionsFulfilled(testOutput.GetSetNextStateAction());

            testOutput.GetState().ShouldBeEquivalentTo(BinaryStateId.Off);
            testButton.PressShortly();
            testOutput.GetState().ShouldBeEquivalentTo(BinaryStateId.On);
            testButton.PressShortly();
            testOutput.GetState().ShouldBeEquivalentTo(BinaryStateId.Off);
            testButton.PressShortly();
            testOutput.GetState().ShouldBeEquivalentTo(BinaryStateId.On);
        }

        [TestMethod]
        public void Automation_WithCondition()
        {
            var testController = new TestController();
            
            var testButtonFactory = new TestButtonFactory(testController.TimerService, new SettingsService(new BackupService(), new StorageService()));
            var testStateMachineFactory = new TestStateMachineFactory();

            var testButton = testButtonFactory.CreateTestButton();
            var testOutput = testStateMachineFactory.CreateTestStateMachineWithOnOffStates();

            new Automation(AutomationIdGenerator.EmptyId)
                .WithTrigger(testButton.PressedShortlyTrigger)
                .WithCondition(ConditionRelation.And, new TimeRangeCondition(testController.DateTimeService).WithStart(TimeSpan.FromHours(1)).WithEnd(TimeSpan.FromHours(2)))
                .WithActionIfConditionsFulfilled(testOutput.GetSetNextStateAction());
            
            testOutput.GetState().ShouldBeEquivalentTo(BinaryStateId.Off);
            testController.SetTime(TimeSpan.FromHours(0));
            testButton.PressShortly();
            testOutput.GetState().ShouldBeEquivalentTo(BinaryStateId.Off);

            testController.SetTime(TimeSpan.FromHours(1.5));
            testButton.PressShortly();
            testOutput.GetState().ShouldBeEquivalentTo(BinaryStateId.On);
        }

        private Automation CreateAutomation()
        {
            return new Automation(AutomationIdGenerator.EmptyId);
        }
    }
}
