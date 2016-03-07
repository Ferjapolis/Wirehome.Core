﻿using System;
using HA4IoT.Contracts.Hardware;

namespace HA4IoT.Hardware.PortExpanderDrivers
{
    public class PCF8574Driver : IPortExpanderDriver
    {
        private readonly II2CBus _i2CBus;
        private readonly I2CSlaveAddress _address;

        public PCF8574Driver(I2CSlaveAddress address, II2CBus i2CBus)
        {
            if (address == null) throw new ArgumentNullException(nameof(address));
            if (i2CBus == null) throw new ArgumentNullException(nameof(i2CBus));

            _address = address;
            _i2CBus = i2CBus;
        }

        public int StateSize { get; } = 1;

        public void Write(byte[] state)
        {
            if (state == null) throw new ArgumentNullException(nameof(state));
            if (state.Length != StateSize) throw new ArgumentException("Length is invalid.", nameof(state));

            _i2CBus.Execute(_address, bus => bus.Write(state));
        }

        public byte[] Read()
        {
            var buffer = new byte[StateSize];
            _i2CBus.Execute(_address, bus => bus.Read(buffer));

            return buffer;
        }
    }
}