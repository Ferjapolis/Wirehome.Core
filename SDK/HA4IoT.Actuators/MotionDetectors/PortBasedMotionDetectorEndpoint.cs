﻿using System;
using HA4IoT.Contracts.Actuators;
using HA4IoT.Contracts.Hardware;

namespace HA4IoT.Actuators
{
    public class PortBasedMotionDetectorEndpoint : IMotionDetectorEndpoint
    {
        public PortBasedMotionDetectorEndpoint(IBinaryInput input)
        {
            if (input == null) throw new ArgumentNullException(nameof(input));

            input.StateChanged += DispatchEvents;
        }

        public event EventHandler MotionDetected;

        public event EventHandler DetectionCompleted;

        private void DispatchEvents(object sender, BinaryStateChangedEventArgs eventArgs)
        {
            // The relay at the motion detector is awlays held to high.
            // The signal is set to false if motion is detected.
            if (eventArgs.NewState == BinaryState.Low)
            {
                MotionDetected?.Invoke(this, EventArgs.Empty);
            }
            else
            {
                DetectionCompleted?.Invoke(this, EventArgs.Empty);
            }
        }
    }
}
