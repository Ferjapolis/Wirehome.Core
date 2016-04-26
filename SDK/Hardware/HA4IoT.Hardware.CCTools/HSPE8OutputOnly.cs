﻿using System;
using HA4IoT.Contracts.Hardware;
using HA4IoT.Hardware.PortExpanderDrivers;

namespace HA4IoT.Hardware.CCTools
{
    public class HSPE8OutputOnly : CCToolsBoardBase, IBinaryOutputController
    {
        public HSPE8OutputOnly(DeviceId id, I2CSlaveAddress address, II2CBus bus)
            : base(id, new PCF8574Driver(address, bus))
        {
            FetchState();
        }

        public IBinaryOutput GetOutput(int number)
        {
            if (number < 0 || number > 7) throw new ArgumentOutOfRangeException(nameof(number));

            return GetPort(number);
        }

        public IBinaryOutput this[HSPE8Pin pin] => GetOutput((int)pin);
    }
}
