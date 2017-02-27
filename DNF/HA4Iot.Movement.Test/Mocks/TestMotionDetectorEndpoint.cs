﻿using System;
using HA4IoT.Contracts.Sensors;

namespace HA4Iot.Movement.Test
{
    public class TestMotionDetectorEndpoint : IMotionDetectorAdapter
    {
        public event EventHandler MotionDetected;
        public event EventHandler DetectionCompleted;

        public void DetectMotion()
        {
            MotionDetected?.Invoke(this, EventArgs.Empty);
        }

        public void CompleteDetection()
        {
            DetectionCompleted?.Invoke(this, EventArgs.Empty);
        }
    }
}
