﻿using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Wirehome.Contracts.Components.Adapters;
using Wirehome.Contracts.Hardware;

namespace Wirehome.Hardware.Drivers.Knx
{
    public class KnxDigitalJoinEnpoint : IBinaryOutputAdapter
    {
        private readonly string _identifier;
        private readonly KnxController _knxController;

        public KnxDigitalJoinEnpoint(string identifier, KnxController knxController)
        {
            if (identifier == null) throw new ArgumentNullException(nameof(identifier));
            if (!ValidationJoin(identifier)) throw new ArgumentException("Identifier is in a wrong format");

            _identifier = identifier;
            _knxController = knxController ?? throw new ArgumentNullException(nameof(knxController));
        }

        public Task SetState(AdapterPowerState powerState, params IHardwareParameter[] parameters)
        {
            if (powerState == AdapterPowerState.On)
            {
                _knxController.SendDigitalJoinOn(_identifier);
            }
            else
            {
                _knxController.SendDigitalJoinOff(_identifier);
            }

            return Task.FromResult(0);
        }

        private bool ValidationJoin(string join)
        {
            return new Regex("([das])([0-9])").IsMatch(join);
        }
    }
}
