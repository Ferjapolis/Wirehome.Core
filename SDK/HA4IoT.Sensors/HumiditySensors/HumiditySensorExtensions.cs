﻿using System;
using HA4IoT.Contracts.Areas;
using HA4IoT.Contracts.Components;
using HA4IoT.Contracts.Sensors;
using HA4IoT.Contracts.Triggers;
using HA4IoT.Sensors.Triggers;

namespace HA4IoT.Sensors.HumiditySensors
{
    public static class HumiditySensorExtensions
    {
        public static ITrigger GetHumidityReachedTrigger(this IHumiditySensor sensor, float value, float delta = 5)
        {
            if (sensor == null) throw new ArgumentNullException(nameof(sensor));

            return new SensorValueReachedTrigger(sensor).WithTarget(value).WithDelta(delta);
        }

        public static ITrigger GetHumidityUnderranTrigger(this IHumiditySensor sensor, float value, float delta = 5)
        {
            if (sensor == null) throw new ArgumentNullException(nameof(sensor));

            return new SensorValueUnderranTrigger(sensor).WithTarget(value).WithDelta(delta);
        }

        public static IArea WithHumiditySensor(this IArea area, Enum id, INumericValueSensorEndpoint endpoint)
        {
            if (area == null) throw new ArgumentNullException(nameof(area));
            if (endpoint == null) throw new ArgumentNullException(nameof(endpoint));

            area.AddComponent(new HumiditySensor(ComponentIdFactory.Create(area.Id, id), endpoint));
            return area;
        }

        public static IHumiditySensor GetHumiditySensor(this IArea area, Enum id)
        {
            if (area == null) throw new ArgumentNullException(nameof(area));

            return area.GetComponent<IHumiditySensor>(ComponentIdFactory.Create(area.Id, id));
        }
    }
}
