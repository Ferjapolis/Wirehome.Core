﻿using HA4IoT.Actuators.Lamps;
using HA4IoT.Components;
using HA4IoT.Contracts.Components;
using HA4IoT.Contracts.Components.States;
using HA4IoT.Tests.Mockups;
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;

namespace HA4IoT.Tests.Components
{
    [TestClass]
    public class LampTests
    {
        [TestMethod]
        public void Lamp_Reset()
        {
            var adapter = new TestBinaryStateAdapter();
            var lamp = new Lamp(new ComponentId("Test"), adapter);
            lamp.ResetState();

            Assert.AreEqual(0, adapter.TurnOnCalledCount);
            Assert.AreEqual(1, adapter.TurnOffCalledCount);
            Assert.AreEqual(true, lamp.GetState().Has(PowerState.Off));
        }

        [TestMethod]
        public void Lamp_TurnOn()
        {
            var adapter = new TestBinaryStateAdapter();
            var lamp = new Lamp(new ComponentId("Test"), adapter);
            lamp.ResetState();

            Assert.AreEqual(0, adapter.TurnOnCalledCount);
            Assert.AreEqual(1, adapter.TurnOffCalledCount);
            Assert.AreEqual(true, lamp.GetState().Has(PowerState.Off));

            lamp.TryTurnOn();

            Assert.AreEqual(1, adapter.TurnOnCalledCount);
            Assert.AreEqual(1, adapter.TurnOffCalledCount);
            Assert.AreEqual(true, lamp.GetState().Has(PowerState.On));
        }

        [TestMethod]
        public void Lamp_Toggle()
        {
            var adapter = new TestBinaryStateAdapter();
            var lamp = new Lamp(new ComponentId("Test"), adapter);
            lamp.ResetState();

            Assert.AreEqual(0, adapter.TurnOnCalledCount);
            Assert.AreEqual(1, adapter.TurnOffCalledCount);
            Assert.AreEqual(true, lamp.GetState().Has(PowerState.Off));

            lamp.TogglePowerStateAction.Execute();

            Assert.AreEqual(1, adapter.TurnOnCalledCount);
            Assert.AreEqual(1, adapter.TurnOffCalledCount);
            Assert.AreEqual(true, lamp.GetState().Has(PowerState.On));

            lamp.TogglePowerStateAction.Execute();

            Assert.AreEqual(1, adapter.TurnOnCalledCount);
            Assert.AreEqual(2, adapter.TurnOffCalledCount);
            Assert.AreEqual(true, lamp.GetState().Has(PowerState.Off));
        }
    }
}
