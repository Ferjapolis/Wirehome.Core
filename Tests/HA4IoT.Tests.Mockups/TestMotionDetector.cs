﻿using System;
using HA4IoT.Contracts.Components;
using HA4IoT.Contracts.Services;
using HA4IoT.Contracts.Services.System;
using HA4IoT.Sensors.MotionDetectors;

namespace HA4IoT.Tests.Mockups
{
    public class TestMotionDetector : MotionDetector
    {
        public TestMotionDetector(ComponentId id, TestMotionDetectorEndpoint endpoint, ISchedulerService schedulerService) 
            : base(id, endpoint, schedulerService)
        {
            if (endpoint == null) throw new ArgumentNullException(nameof(endpoint));
            if (schedulerService == null) throw new ArgumentNullException(nameof(schedulerService));

            Endpoint = endpoint;
        }

        public TestMotionDetectorEndpoint Endpoint { get; }

        public void DetectMotion()
        {
            OnMotionDetected();
        }

        public void CompleteMotionDetection()
        {
            OnDetectionCompleted();
        }

        public void TriggerMotionDetection()
        {
            OnMotionDetected();
            OnDetectionCompleted();
        }
    }
}