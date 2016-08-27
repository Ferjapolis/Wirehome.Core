﻿using System;
using FluentAssertions;
using HA4IoT.Contracts.Actuators;
using HA4IoT.Tests.Mockups;
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;

namespace HA4IoT.Automations.Tests
{
    [TestClass]
    public class RollerShutterAutomationTests
    {
        private TestController _controller;
        private TestRollerShutter _rollerShutter;
        private TestWeatherStation _weatherStation;
        private RollerShutterAutomation _automation;

        [TestMethod]
        public void SkipOpen_BecauseTooCold()
        {
            Setup();
            
            _weatherStation.OutdoorTemperature = 1.5F;
            _automation.WithDoNotOpenIfOutsideTemperatureIsBelowThan(2);
            _automation.PerformPendingActions();
            _rollerShutter.GetState().ShouldBeEquivalentTo(RollerShutterStateId.Off);

            Setup();

            _weatherStation.OutdoorTemperature = 2.5F;
            _automation.WithDoNotOpenIfOutsideTemperatureIsBelowThan(2);
            _automation.PerformPendingActions();
            _rollerShutter.GetState().ShouldBeEquivalentTo(RollerShutterStateId.MovingUp);
        }

        [TestMethod]
        public void Close_BecauseTooHot()
        {
            Setup();
            SkipOpenDueToSunrise();

            _weatherStation.OutdoorTemperature = 20F;
            _automation.WithCloseIfOutsideTemperatureIsGreaterThan(25);
            _automation.PerformPendingActions();
            _rollerShutter.GetState().ShouldBeEquivalentTo(RollerShutterStateId.Off);

            _weatherStation.OutdoorTemperature = 25.5F;
            _automation.PerformPendingActions();
            _rollerShutter.GetState().ShouldBeEquivalentTo(RollerShutterStateId.MovingDown);
        }

        [TestMethod]
        public void Open_AfterSunrise()
        {
            Setup();

            _rollerShutter.GetState().ShouldBeEquivalentTo(RollerShutterStateId.Off);
            _automation.PerformPendingActions();
            _rollerShutter.GetState().ShouldBeEquivalentTo(RollerShutterStateId.MovingUp);
        }

        [TestMethod]
        public void Close_AfterSunset()
        {
            Setup();
            SkipOpenDueToSunrise();

            _controller.SetTime(TimeSpan.Parse("18:31"));
            
            _automation.PerformPendingActions();
            _rollerShutter.GetState().ShouldBeEquivalentTo(RollerShutterStateId.MovingDown);
        }

        private void SkipOpenDueToSunrise()
        {
            _automation.PerformPendingActions();
            _rollerShutter.GetState().ShouldBeEquivalentTo(RollerShutterStateId.MovingUp);
            _rollerShutter.SetState(RollerShutterStateId.Off);
        }

        private void Setup()
        {
            _controller = new TestController();
            _controller.SetTime(TimeSpan.Parse("12:00"));

            var testRollerShutterFactory = new TestRollerShutterFactory(_controller.TimerService, _controller.SchedulerService);

            _weatherStation = new TestWeatherStation();
            _weatherStation.OutdoorTemperature = 20;
            
            _rollerShutter = testRollerShutterFactory.CreateTestRollerShutter();
            _controller.ComponentService.AddComponent(_rollerShutter);

            _automation = new RollerShutterAutomation(
                AutomationIdFactory.EmptyId,
                _controller.NotificationService,
                _controller.SchedulerService,
                _controller.DateTimeService,
                _controller.DaylightService,
                _weatherStation,
                _controller.ComponentService);

            _automation.WithRollerShutters(_rollerShutter);
        }
    }
}
