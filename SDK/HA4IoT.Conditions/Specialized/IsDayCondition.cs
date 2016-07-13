﻿using System;
using HA4IoT.Contracts.Services;
using HA4IoT.Contracts.Services.System;

namespace HA4IoT.Conditions.Specialized
{
    public class IsDayCondition : TimeRangeCondition
    {
        public IsDayCondition(IDaylightService daylightService, IDateTimeService dateTimeService) 
            : base(dateTimeService)
        {
            if (daylightService == null) throw new ArgumentNullException(nameof(daylightService));

            WithStart(daylightService.GetSunrise);
            WithEnd(daylightService.GetSunset);
        }
    }
}
