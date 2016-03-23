﻿using System;
using HA4IoT.Contracts.Core;
using HA4IoT.Contracts.Services;

namespace HA4IoT.Conditions.Specialized
{
    public class IsNightCondition : TimeRangeCondition
    {
        public IsNightCondition(IDaylightService daylightService, IHomeAutomationTimer timer) : base(timer)
        {
            if (daylightService == null) throw new ArgumentNullException(nameof(daylightService));

            WithStart(() => daylightService.Sunset);
            WithEnd(() => daylightService.Sunrise);
        }
    }
}
