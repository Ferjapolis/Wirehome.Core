﻿using System;
using HA4IoT.Actuators;
using HA4IoT.Actuators.Lamps;
using HA4IoT.Actuators.StateMachines;
using HA4IoT.Automations;
using HA4IoT.Contracts.Areas;
using HA4IoT.Contracts.Components;
using HA4IoT.Contracts.Hardware;
using HA4IoT.Contracts.Services;
using HA4IoT.Contracts.Services.System;
using HA4IoT.Hardware.CCTools;
using HA4IoT.Hardware.I2CHardwareBridge;
using HA4IoT.PersonalAgent;
using HA4IoT.Sensors;
using HA4IoT.Sensors.HumiditySensors;
using HA4IoT.Sensors.MotionDetectors;
using HA4IoT.Sensors.TemperatureSensors;
using HA4IoT.Services.Areas;
using HA4IoT.Services.Devices;

namespace HA4IoT.Controller.Main.Rooms
{
    internal class UpperBathroomConfiguration
    {
        private readonly CCToolsBoardService _ccToolsBoardService;
        private readonly IDeviceService _deviceService;
        private readonly ISchedulerService _schedulerService;
        private readonly IAreaService _areaService;
        private readonly SynonymService _synonymService;
        private readonly AutomationFactory _automationFactory;
        private readonly ActuatorFactory _actuatorFactory;
        private readonly SensorFactory _sensorFactory;

        private enum UpperBathroom
        {
            TemperatureSensor,
            HumiditySensor,
            MotionDetector,

            LightCeilingDoor,
            LightCeilingEdge,
            LightCeilingMirrorCabinet,
            LampMirrorCabinet,

            Fan,

            CombinedCeilingLights
        }

        public UpperBathroomConfiguration(
            CCToolsBoardService ccToolsBoardService,
            IDeviceService deviceService,
            ISchedulerService schedulerService,
            IAreaService areaService,
            SynonymService synonymService,
            AutomationFactory automationFactory,
            ActuatorFactory actuatorFactory,
            SensorFactory sensorFactory)
        {
            if (ccToolsBoardService == null) throw new ArgumentNullException(nameof(ccToolsBoardService));
            if (deviceService == null) throw new ArgumentNullException(nameof(deviceService));
            if (schedulerService == null) throw new ArgumentNullException(nameof(schedulerService));
            if (areaService == null) throw new ArgumentNullException(nameof(areaService));
            if (synonymService == null) throw new ArgumentNullException(nameof(synonymService));
            if (automationFactory == null) throw new ArgumentNullException(nameof(automationFactory));
            if (actuatorFactory == null) throw new ArgumentNullException(nameof(actuatorFactory));
            if (sensorFactory == null) throw new ArgumentNullException(nameof(sensorFactory));

            _ccToolsBoardService = ccToolsBoardService;
            _deviceService = deviceService;
            _schedulerService = schedulerService;
            _areaService = areaService;
            _synonymService = synonymService;
            _automationFactory = automationFactory;
            _actuatorFactory = actuatorFactory;
            _sensorFactory = sensorFactory;
        }

        public void Apply()
        {
            var hsrel5 = _ccToolsBoardService.CreateHSREL5(InstalledDevice.UpperBathroomHSREL5, new I2CSlaveAddress(61));
            var input5 = _deviceService.GetDevice<HSPE16InputOnly>(InstalledDevice.Input5);
            var i2CHardwareBridge = _deviceService.GetDevice<I2CHardwareBridge>();

            const int SensorPin = 4;
            
            var room = _areaService.CreateArea(Room.UpperBathroom)
                .WithTemperatureSensor(UpperBathroom.TemperatureSensor, i2CHardwareBridge.DHT22Accessor.GetTemperatureSensor(SensorPin))
                .WithHumiditySensor(UpperBathroom.HumiditySensor, i2CHardwareBridge.DHT22Accessor.GetHumiditySensor(SensorPin))
                .WithStateMachine(UpperBathroom.Fan, (s, r) => SetupFan(s, hsrel5));

            _sensorFactory.RegisterMotionDetector(room, UpperBathroom.MotionDetector, input5.GetInput(15));

            _actuatorFactory.RegisterLamp(room, UpperBathroom.LightCeilingDoor, hsrel5.GetOutput(0));
            _actuatorFactory.RegisterLamp(room, UpperBathroom.LightCeilingEdge, hsrel5.GetOutput(1));
            _actuatorFactory.RegisterLamp(room, UpperBathroom.LightCeilingMirrorCabinet, hsrel5.GetOutput(2));
            _actuatorFactory.RegisterLamp(room, UpperBathroom.LampMirrorCabinet, hsrel5.GetOutput(3));

            var combinedLights =
                _actuatorFactory.RegisterLogicalActuator(room, UpperBathroom.CombinedCeilingLights)
                    .WithActuator(room.GetLamp(UpperBathroom.LightCeilingDoor))
                    .WithActuator(room.GetLamp(UpperBathroom.LightCeilingEdge))
                    .WithActuator(room.GetLamp(UpperBathroom.LightCeilingMirrorCabinet))
                    .WithActuator(room.GetLamp(UpperBathroom.LampMirrorCabinet));

            _automationFactory.RegisterTurnOnAndOffAutomation(room)
                .WithTrigger(room.GetMotionDetector(UpperBathroom.MotionDetector))
                .WithTarget(combinedLights)
                .WithOnDuration(TimeSpan.FromMinutes(8));
            
            new BathroomFanAutomation(AutomationIdFactory.CreateIdFrom<BathroomFanAutomation>(room), _schedulerService)
                .WithTrigger(room.GetMotionDetector(UpperBathroom.MotionDetector))
                .WithSlowDuration(TimeSpan.FromMinutes(8))
                .WithFastDuration(TimeSpan.FromMinutes(12))
                .WithActuator(room.GetStateMachine(UpperBathroom.Fan));

            _synonymService.AddSynonymsForArea(Room.UpperBathroom, "BadOben", "UpperBathroom");
        }

        private void SetupFan(StateMachine stateMachine, HSREL5 hsrel5)
        {
            var fanPort0 = hsrel5.GetOutput(4);
            var fanPort1 = hsrel5.GetOutput(5);

            stateMachine.AddOffState().WithOutput(fanPort0, BinaryState.Low).WithOutput(fanPort1, BinaryState.Low);
            stateMachine.AddState(new NamedComponentState("1")).WithOutput(fanPort0, BinaryState.High).WithOutput(fanPort1, BinaryState.Low);
            stateMachine.AddState(new NamedComponentState("2")).WithOutput(fanPort0, BinaryState.High).WithOutput(fanPort1, BinaryState.High);
            stateMachine.TryTurnOff();
        }
    }
}
