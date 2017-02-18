﻿using System;
using HA4IoT.Contracts.Components;
using HA4IoT.Contracts.Services.System;
using HA4IoT.Contracts.Services.Settings;

namespace UnitTestProject1
{
    public class TestMotionDetectorFactory
    {
        private readonly ISchedulerService _schedulerService;
        private readonly ISettingsService _settingsService;

        public TestMotionDetectorFactory(ISchedulerService schedulerService, ISettingsService settingsService)
        {
            if (schedulerService == null) throw new ArgumentNullException(nameof(schedulerService));
            if (settingsService == null) throw new ArgumentNullException(nameof(settingsService));

            _schedulerService = schedulerService;
            _settingsService = settingsService;
        }

        public TestMotionDetector CreateTestMotionDetector()
        {
            return new TestMotionDetector(ComponentIdGenerator.EmptyId, new TestMotionDetectorEndpoint(),
                _schedulerService, _settingsService);
        }
    }
}
