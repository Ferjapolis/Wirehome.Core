﻿using System;
using HA4IoT.Contracts.Api;
using HA4IoT.Contracts.Hardware;
using HA4IoT.Contracts.Services;
using HA4IoT.Contracts.Services.System;

namespace HA4IoT.Hardware.I2CHardwareBridge
{
    public class I2CHardwareBridge : IDevice
    {
        private readonly I2CSlaveAddress _address;
        private readonly II2CBusService _i2CBus;

        public I2CHardwareBridge(I2CSlaveAddress address, II2CBusService i2cBus, ISchedulerService schedulerService)
        {
            if (i2cBus == null) throw new ArgumentNullException(nameof(i2cBus));
            if (schedulerService == null) throw new ArgumentNullException(nameof(schedulerService));

            _address = address;
            _i2CBus = i2cBus;

            DHT22Accessor = new DHT22Accessor(this, schedulerService);
        }

        public DeviceId Id { get; } = new DeviceId("I2CHardwareBridge");

        public DHT22Accessor DHT22Accessor { get; }

        public void HandleApiCommand(IApiContext apiContext)
        {
        }

        public void HandleApiRequest(IApiContext apiContext)
        {
        }

        public void ExecuteCommand(I2CHardwareBridgeCommand command)
        {
            if (command == null) throw new ArgumentNullException(nameof(command));

            _i2CBus.Execute(_address, command.Execute, false);
        }
    }
}
