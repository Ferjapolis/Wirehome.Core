﻿using System;
using Wirehome.Actuators;
using Wirehome.Actuators.Connectors;
using Wirehome.Actuators.Lamps;
using Wirehome.Actuators.RollerShutters;
using Wirehome.Areas;
using Wirehome.Automations;
using Wirehome.Components;
using Wirehome.Components.Adapters.PortBased;
using Wirehome.Contracts.Areas;
using Wirehome.Contracts.Core;
using Wirehome.Contracts.Messaging;
using Wirehome.Hardware.Drivers.CCTools;
using Wirehome.Hardware.Drivers.CCTools.Devices;
using Wirehome.Hardware.Drivers.I2CHardwareBridge;
using Wirehome.Sensors;
using Wirehome.Sensors.Buttons;

namespace Wirehome.Controller.Main.Main.Rooms
{
    internal class ReadingRoomConfiguration
    {
        private readonly IAreaRegistryService _areaService;
        private readonly IDeviceRegistryService _deviceService;
        private readonly CCToolsDeviceService _ccToolsBoardService;
        private readonly AutomationFactory _automationFactory;
        private readonly ActuatorFactory _actuatorFactory;
        private readonly SensorFactory _sensorFactory;
        private readonly IMessageBrokerService _messageBroker;

        private enum ReadingRoom
        {
            TemperatureSensor,
            HumiditySensor,

            LightCeilingMiddle,

            RollerShutter,
            RollerShutterButtonUp,
            RollerShutterButtonDown,
            RollerShutterAutomation,

            Button,

            SocketWindow,
            SocketWallLeft,
            SocketWallRight,

            Window
        }

        public ReadingRoomConfiguration(
            IAreaRegistryService areaService,
            IDeviceRegistryService deviceService,
            CCToolsDeviceService ccToolsBoardService,
            AutomationFactory automationFactory,
            ActuatorFactory actuatorFactory,
            SensorFactory sensorFactory,
            IMessageBrokerService messageBroker)
        {
            _messageBroker = messageBroker ?? throw new ArgumentNullException(nameof(messageBroker));
            _areaService = areaService ?? throw new ArgumentNullException(nameof(areaService));
            _deviceService = deviceService ?? throw new ArgumentNullException(nameof(deviceService));
            _ccToolsBoardService = ccToolsBoardService ?? throw new ArgumentNullException(nameof(ccToolsBoardService));
            _automationFactory = automationFactory ?? throw new ArgumentNullException(nameof(automationFactory));
            _actuatorFactory = actuatorFactory ?? throw new ArgumentNullException(nameof(actuatorFactory));
            _sensorFactory = sensorFactory ?? throw new ArgumentNullException(nameof(sensorFactory));
        }

        public void Apply()
        {
            var hsrel5 = (HSREL5)_ccToolsBoardService.RegisterDevice(CCToolsDeviceType.HSRel5, InstalledDevice.ReadingRoomHSREL5.ToString(), 62);
            var input2 = _deviceService.GetDevice<HSPE16InputOnly>(InstalledDevice.Input2.ToString());
            var i2CHardwareBridge = _deviceService.GetDevice<I2CHardwareBridge>();

            const int SensorPin = 9;

            var area = _areaService.RegisterArea(Room.ReadingRoom);

            _sensorFactory.RegisterWindow(area, ReadingRoom.Window, new PortBasedWindowAdapter(input2.GetInput(8))); // Tilt = input2.GetInput(9) -- currently broken!

            _sensorFactory.RegisterTemperatureSensor(area, ReadingRoom.TemperatureSensor,
                i2CHardwareBridge.DHT22Accessor.GetTemperatureSensor(SensorPin));

            _sensorFactory.RegisterHumiditySensor(area, ReadingRoom.HumiditySensor,
                i2CHardwareBridge.DHT22Accessor.GetHumiditySensor(SensorPin));

            _actuatorFactory.RegisterLamp(area, ReadingRoom.LightCeilingMiddle, hsrel5[HSREL5Pin.GPIO0]);

            _actuatorFactory.RegisterRollerShutter(area, ReadingRoom.RollerShutter, hsrel5[HSREL5Pin.Relay4], hsrel5[HSREL5Pin.Relay3]);
            _sensorFactory.RegisterButton(area, ReadingRoom.RollerShutterButtonUp, input2.GetInput(12));
            _sensorFactory.RegisterButton(area, ReadingRoom.RollerShutterButtonDown, input2.GetInput(11));

            _actuatorFactory.RegisterSocket(area, ReadingRoom.SocketWindow, hsrel5[HSREL5Pin.Relay0]);
            _actuatorFactory.RegisterSocket(area, ReadingRoom.SocketWallLeft, hsrel5[HSREL5Pin.Relay1]);
            _actuatorFactory.RegisterSocket(area, ReadingRoom.SocketWallRight, hsrel5[HSREL5Pin.Relay2]);

            _sensorFactory.RegisterButton(area, ReadingRoom.Button, input2.GetInput(13));

            area.GetButton(ReadingRoom.Button).CreatePressedShortTrigger(_messageBroker).Attach(() => area.GetLamp(ReadingRoom.LightCeilingMiddle).TryTogglePowerState());

            _automationFactory.RegisterRollerShutterAutomation(area, ReadingRoom.RollerShutterAutomation)
                .WithRollerShutters(area.GetRollerShutter(ReadingRoom.RollerShutter));

            area.GetRollerShutter(ReadingRoom.RollerShutter)
                .ConnectWith(area.GetButton(ReadingRoom.RollerShutterButtonUp), area.GetButton(ReadingRoom.RollerShutterButtonDown), _messageBroker);
        }
    }
}
