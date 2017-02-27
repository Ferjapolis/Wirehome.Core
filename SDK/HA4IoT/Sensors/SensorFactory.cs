﻿using System;
using HA4IoT.Adapters;
using HA4IoT.Contracts.Adapters;
using HA4IoT.Contracts.Areas;
using HA4IoT.Contracts.Hardware;
using HA4IoT.Contracts.Sensors;
using HA4IoT.Contracts.Services.Settings;
using HA4IoT.Contracts.Services.System;
using HA4IoT.Sensors.Buttons;
using HA4IoT.Sensors.HumiditySensors;
using HA4IoT.Sensors.MotionDetectors;
using HA4IoT.Sensors.TemperatureSensors;
using HA4IoT.Sensors.Windows;

namespace HA4IoT.Sensors
{
    public class SensorFactory
    {
        private readonly ITimerService _timerService;
        private readonly ISchedulerService _schedulerService;
        private readonly ISettingsService _settingsService;

        public SensorFactory(ITimerService timerService, ISchedulerService schedulerService, ISettingsService settingsService)
        {
            if (timerService == null) throw new ArgumentNullException(nameof(timerService));
            if (schedulerService == null) throw new ArgumentNullException(nameof(schedulerService));
            if (settingsService == null) throw new ArgumentNullException(nameof(settingsService));

            _timerService = timerService;
            _schedulerService = schedulerService;
            _settingsService = settingsService;
        }

        public ITemperatureSensor RegisterTemperatureSensor(IArea area, Enum id, INumericSensorAdapter adapter)
        {
            if (area == null) throw new ArgumentNullException(nameof(area));
            if (adapter == null) throw new ArgumentNullException(nameof(adapter));

            var temperatureSensor = new TemperatureSensor($"{area.Id}.{id}", adapter, _settingsService);
            area.AddComponent(temperatureSensor);
            
            return temperatureSensor;
        }

        public IHumiditySensor RegisterHumiditySensor(IArea area, Enum id, INumericSensorAdapter adapter)
        {
            if (area == null) throw new ArgumentNullException(nameof(area));
            if (adapter == null) throw new ArgumentNullException(nameof(adapter));

            var humditySensor = new HumiditySensor($"{area.Id}.{id}", adapter, _settingsService);
            area.AddComponent(humditySensor);

            return humditySensor;
        }

        public IButton RegisterVirtualButton(IArea area, Enum id, Action<IButton> initializer = null)
        {
            if (area == null) throw new ArgumentNullException(nameof(area));

            var virtualButton = new Button($"{area.Id}.{id}", new VirtualButtonAdapter(), _timerService, _settingsService);
            initializer?.Invoke(virtualButton);

            area.AddComponent(virtualButton);
            return virtualButton;
        }

        public IButton RegisterButton(IArea area, Enum id, IBinaryInput input, Action<IButton> initializer = null)
        {
            if (area == null) throw new ArgumentNullException(nameof(area));
            if (input == null) throw new ArgumentNullException(nameof(input));

            var button = new Button(
                $"{area.Id}.{id}",
                new BinaryInputButtonAdapter(input),
                _timerService,
                _settingsService);

            initializer?.Invoke(button);

            area.AddComponent(button);
            return button;
        }

        public void RegisterRollerShutterButtons(
            IArea area,
            Enum upId,
            IBinaryInput upInput,
            Enum downId,
            IBinaryInput downInput)
        {
            if (area == null) throw new ArgumentNullException(nameof(area));
            if (upInput == null) throw new ArgumentNullException(nameof(upInput));
            if (downInput == null) throw new ArgumentNullException(nameof(downInput));

            var upButton = new Button(
                $"{area.Id}.{upId}",
                new BinaryInputButtonAdapter(upInput),
                _timerService,
                _settingsService);

            area.AddComponent(upButton);

            var downButton = new Button(
                $"{area.Id}.{downId}",
                new BinaryInputButtonAdapter(downInput),
                _timerService,
                _settingsService);

            area.AddComponent(downButton);
        }

        public IMotionDetector RegisterMotionDetector(IArea area, Enum id, IBinaryInput input)
        {
            if (area == null) throw new ArgumentNullException(nameof(area));
            if (input == null) throw new ArgumentNullException(nameof(input));

            var motionDetector = new MotionDetector(
                $"{area.Id}.{id}",
                new PortBasedMotionDetectorAdapter(input),
                _schedulerService,
                _settingsService);

            area.AddComponent(motionDetector);

            return motionDetector;
        }

        public IWindow RegisterWindow(IArea area, Enum id, IWindowAdapter adapter)
        {
            if (area == null) throw new ArgumentNullException(nameof(area));
            if (adapter == null) throw new ArgumentNullException(nameof(adapter));

            var window = new Window($"{area.Id}.{id}", adapter, _settingsService);
            area.AddComponent(window);
            return window;
        }
    }
}
