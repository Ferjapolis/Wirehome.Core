﻿using System;
using HA4IoT.Contracts.Components;
using HA4IoT.Contracts.Components.States;
using HA4IoT.Contracts.Logging;

namespace HA4IoT.Components
{
    public static class ComponentStateExtensions
    {
        public static bool TryGetHumidity(this IComponent component, out float? value)
        {
            if (component == null) throw new ArgumentNullException(nameof(component));

            return TryGetStateValue<HumidityState, float?>(component, s => s.Value, out value);
        }

        public static bool TryGetTemperature(this IComponent component, out float? value)
        {
            if (component == null) throw new ArgumentNullException(nameof(component));

            return TryGetStateValue<TemperatureState, float?>(component, s => s.Value, out value);
        }

        public static bool TryGetStateValue<TState, TValue>(this IComponent component, Func<TState, TValue> valueResolver, out TValue value) where TState : IComponentFeatureState
        {
            if (component == null) throw new ArgumentNullException(nameof(component));
            if (valueResolver == null) throw new ArgumentNullException(nameof(valueResolver));

            value = default(TValue);

            var state = component.GetState();
            if (!state.Supports<TState>())
            {
                Log.Warning($"Component '{component.Id}' does not support state '{typeof(TState).Name}'.");
                return false;
            }

            var temperatureState = component.GetState().Extract<TState>();
            value = valueResolver(temperatureState);

            return true;
        }
    }
}
