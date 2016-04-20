﻿using System;
using HA4IoT.Contracts.Areas;
using HA4IoT.Contracts.Components;
using HA4IoT.Contracts.Sensors;
using HA4IoT.Contracts.Triggers;
using HA4IoT.Sensors.Triggers;

namespace HA4IoT.Sensors.TemperatureSensors
{
    public static class TemperatureSensorExtensions
    {
        public static ITrigger GetTemperatureReachedTrigger(this ITemperatureSensor sensor, float target, float delta)
        {
            if (sensor == null) throw new ArgumentNullException(nameof(sensor));

            return new SensorValueReachedTrigger(sensor).WithTarget(target).WithDelta(delta);
        }

        public static ITrigger GetTemperatureUnderranTrigger(this ITemperatureSensor sensor, float target, float delta)
        {
            if (sensor == null) throw new ArgumentNullException(nameof(sensor));

            return new SensorValueUnderranTrigger(sensor).WithTarget(target).WithDelta(delta);
        }

        public static IArea WithTemperatureSensor(this IArea area, Enum id, INumericValueSensorEndpoint endpoint)
        {
            if (area == null) throw new ArgumentNullException(nameof(area));
            if (endpoint == null) throw new ArgumentNullException(nameof(endpoint));

            area.AddComponent(new TemperatureSensor(ComponentIdFactory.Create(area.Id, id), endpoint));
            return area;
        }
        
        public static ITemperatureSensor GetTemperatureSensor(this IArea area, Enum id)
        {
            if (area == null) throw new ArgumentNullException(nameof(area));

            return area.GetComponent<ITemperatureSensor>(ComponentIdFactory.Create(area.Id, id));
        }
    }
}
