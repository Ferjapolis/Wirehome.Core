﻿using HA4IoT.Contracts.Hardware;
using HA4IoT.Contracts.Hardware.I2C;
using HA4IoT.Hardware.Drivers.CCTools.Devices;
using HA4IoT.Tests.Mockups;
using HA4IoT.Tests.Mockups.Services;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace HA4IoT.Tests.Hardware.CCTools
{
    [TestClass]
    public class CCToolsTests
    {
        [TestMethod]
        public void CCTools_Write_HSRel5_State()
        {
            var i2CBus = new TestI2CBusService();
            var hsrel5 = new HSREL5("Test", new I2CSlaveAddress(66), i2CBus, new TestLogger());
            hsrel5[HSREL5Pin.Relay0].Write(BinaryState.High);

            Assert.AreEqual(new I2CSlaveAddress(66), i2CBus.LastUsedI2CSlaveAddress);
            Assert.AreEqual(1, i2CBus.LastWrittenBytes.Length);

            // The bits are inverted using a hardware inverter. This requires checking
            // against inverted values too.
            Assert.AreEqual(254, i2CBus.LastWrittenBytes[0]);

            hsrel5[HSREL5Pin.Relay4].Write(BinaryState.High);

            Assert.AreEqual(new I2CSlaveAddress(66), i2CBus.LastUsedI2CSlaveAddress);
            Assert.AreEqual(1, i2CBus.LastWrittenBytes.Length);

            // The bits are inverted using a hardware inverter. This requires checking
            // against inverted values too.
            Assert.AreEqual(238, i2CBus.LastWrittenBytes[0]);
        }

        [TestMethod]
        public void CCTools_Read_HSPE16InputOnly_State()
        {
            // The hardware board contains pull-up resistors. This means that the inputs are inverted internally
            // and the test must respect this.
            var i2CBus = new TestI2CBusService();
            i2CBus.BufferForNextRead = new byte[] { 255, 255 };

            var hspe16 = new HSPE16InputOnly("Test", new I2CSlaveAddress(32), i2CBus, new TestLogger());

            hspe16.FetchState();

            Assert.AreEqual(new I2CSlaveAddress(32), i2CBus.LastUsedI2CSlaveAddress);
            Assert.AreEqual(BinaryState.Low, hspe16[HSPE16Pin.GPIO0].Read());

            i2CBus.BufferForNextRead = new byte[] { 0, 255 };
            hspe16.FetchState();

            Assert.AreEqual(new I2CSlaveAddress(32), i2CBus.LastUsedI2CSlaveAddress);
            Assert.AreEqual(BinaryState.High, hspe16[HSPE16Pin.GPIO0].Read());

            i2CBus.BufferForNextRead = new byte[] { 255, 127 };
            hspe16.FetchState();

            Assert.AreEqual(new I2CSlaveAddress(32), i2CBus.LastUsedI2CSlaveAddress);
            Assert.AreEqual(BinaryState.Low, hspe16[HSPE16Pin.GPIO0].Read());
            Assert.AreEqual(BinaryState.High, hspe16[HSPE16Pin.GPIO15].Read());
        }
    }
}
