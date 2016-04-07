﻿using System;
using HA4IoT.Contracts.Hardware;
using HA4IoT.Contracts.Sensors;

namespace HA4IoT.Sensors.Buttons
{
    public class PortBasedButtonEndpoint : IButtonEndpoint
    {
        public PortBasedButtonEndpoint(IBinaryInput input)
        {
            if (input == null) throw new ArgumentNullException(nameof(input));

            input.StateChanged += DispatchState;
        }

        public event EventHandler Pressed;
        public event EventHandler Released;

        private void DispatchState(object sender, BinaryStateChangedEventArgs e)
        {
            if (e.NewValue == BinaryState.High)
            {
                Pressed?.Invoke(this, EventArgs.Empty);
            }
            else if (e.NewValue == BinaryState.Low)
            {
                Released?.Invoke(this, EventArgs.Empty);
            }
        }
    }
}
