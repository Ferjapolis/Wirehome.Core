﻿using HA4IoT.Actuators.Fans;
using HA4IoT.Components;
using HA4IoT.Contracts.Commands;
using HA4IoT.Contracts.Components.Features;
using HA4IoT.Contracts.Components.States;
using HA4IoT.Tests.Mockups.Adapters;
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;

namespace HA4IoT.Tests.Components
{
    [TestClass]
    public class FanTests
    {
        [TestMethod]
        public void Fan_Feature()
        {
            var adapter = new TestFanAdapter
            {
                MaxLevel = 3,
                CurrentLevel = -1
            };

            var fan = new Fan("Fan1", adapter);
            Assert.AreEqual(3, fan.GetFeatures().Extract<LevelStateFeature>().MaxLevel);
            Assert.IsTrue(fan.GetFeatures().Supports<PowerStateFeature>());
            Assert.IsTrue(fan.GetFeatures().Supports<LevelStateFeature>());
        }

        [TestMethod]
        public void Fan_Reset()
        {
            var adapter = new TestFanAdapter
            {
                MaxLevel = 3,
                CurrentLevel = -1
            };

            var fan = new Fan("Fan1", adapter);
            fan.ResetState();
            Assert.AreEqual(0, fan.GetState().Extract<LevelState>().CurrentLevel);
            Assert.AreEqual(0, adapter.CurrentLevel);
        }

        [TestMethod]
        public void Fan_SetLevel1()
        {
            var adapter = new TestFanAdapter
            {
                MaxLevel = 3,
                CurrentLevel = -1
            };

            var fan = new Fan("Fan1", adapter);
            fan.ResetState();
            Assert.AreEqual(0, fan.GetState().Extract<LevelState>().CurrentLevel);
            Assert.AreEqual(0, adapter.CurrentLevel);

            fan.SetNextLevelAction.Execute();
            Assert.AreEqual(1, fan.GetState().Extract<LevelState>().CurrentLevel);
            Assert.AreEqual(1, adapter.CurrentLevel);
        }

        [TestMethod]
        public void Fan_TurnOn()
        {
            var adapter = new TestFanAdapter
            {
                MaxLevel = 3,
                CurrentLevel = -1
            };

            var fan = new Fan("Fan1", adapter);
            fan.ResetState();
            Assert.AreEqual(0, fan.GetState().Extract<LevelState>().CurrentLevel);
            Assert.AreEqual(0, adapter.CurrentLevel);

            fan.TryTurnOn();
            Assert.AreEqual(3, fan.GetState().Extract<LevelState>().CurrentLevel);
            Assert.AreEqual(3, adapter.CurrentLevel);
        }

        [TestMethod]
        public void Fan_SetLevel()
        {
            var adapter = new TestFanAdapter
            {
                MaxLevel = 3,
                CurrentLevel = -1
            };

            var fan = new Fan("Fan1", adapter);
            fan.ResetState();
            Assert.AreEqual(0, fan.GetState().Extract<LevelState>().CurrentLevel);
            Assert.AreEqual(0, adapter.CurrentLevel);

            fan.InvokeCommand(new SetLevelCommand { Level = 2 });
            Assert.AreEqual(2, fan.GetState().Extract<LevelState>().CurrentLevel);
            Assert.AreEqual(2, adapter.CurrentLevel);
        }

        [TestMethod]
        public void Fan_TurnOff()
        {
            var adapter = new TestFanAdapter
            {
                MaxLevel = 3,
                CurrentLevel = -1
            };

            var fan = new Fan("Fan1", adapter);
            fan.ResetState();
            Assert.AreEqual(0, fan.GetState().Extract<LevelState>().CurrentLevel);
            Assert.AreEqual(0, adapter.CurrentLevel);

            fan.InvokeCommand(new SetLevelCommand { Level = 2 });
            Assert.AreEqual(2, fan.GetState().Extract<LevelState>().CurrentLevel);
            Assert.AreEqual(2, adapter.CurrentLevel);

            fan.InvokeCommand(new TurnOffCommand());
            Assert.AreEqual(0, fan.GetState().Extract<LevelState>().CurrentLevel);
            Assert.AreEqual(0, adapter.CurrentLevel);
        }

        [TestMethod]
        public void Fan_LevelOverrun()
        {
            var adapter = new TestFanAdapter
            {
                MaxLevel = 3,
                CurrentLevel = -1
            };

            var fan = new Fan("Fan1", adapter);
            fan.ResetState();
            Assert.AreEqual(0, fan.GetState().Extract<LevelState>().CurrentLevel);
            Assert.AreEqual(0, adapter.CurrentLevel);

            fan.InvokeCommand(new SetLevelCommand { Level = 3 });
            Assert.AreEqual(3, fan.GetState().Extract<LevelState>().CurrentLevel);
            Assert.AreEqual(3, adapter.CurrentLevel);

            fan.InvokeCommand(new IncreaseLevelCommand());
            Assert.AreEqual(0, fan.GetState().Extract<LevelState>().CurrentLevel);
            Assert.AreEqual(0, adapter.CurrentLevel);
        }

        [TestMethod]
        public void Fan_LevelUnderrun()
        {
            var adapter = new TestFanAdapter
            {
                MaxLevel = 3,
                CurrentLevel = -1
            };

            var fan = new Fan("Fan1", adapter);
            fan.ResetState();
            Assert.AreEqual(0, fan.GetState().Extract<LevelState>().CurrentLevel);
            Assert.AreEqual(0, adapter.CurrentLevel);

            fan.InvokeCommand(new DecreaseLevelCommand());
            Assert.AreEqual(3, fan.GetState().Extract<LevelState>().CurrentLevel);
            Assert.AreEqual(3, adapter.CurrentLevel);
        }
    }
}
