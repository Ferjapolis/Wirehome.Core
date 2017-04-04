﻿using System;
using HA4IoT.Actuators;
using HA4IoT.Actuators.Connectors;
using HA4IoT.Actuators.Lamps;
using HA4IoT.Actuators.RollerShutters;
using HA4IoT.Adapters;
using HA4IoT.Automations;
using HA4IoT.Components;
using HA4IoT.Contracts.Areas;
using HA4IoT.Contracts.Hardware.I2C;
using HA4IoT.Contracts.Services.System;
using HA4IoT.Hardware.CCTools;
using HA4IoT.Hardware.CCTools.Devices;
using HA4IoT.Hardware.I2C.I2CHardwareBridge;
using HA4IoT.Sensors;
using HA4IoT.Sensors.Buttons;
using HA4IoT.Services.Areas;

namespace HA4IoT.Controller.Main.Main.Rooms
{
    internal class ReadingRoomConfiguration
    {
        private readonly IAreaRegistryService _areaService;
        private readonly IDeviceRegistryService _deviceService;
        private readonly CCToolsDeviceService _ccToolsBoardService;
        private readonly AutomationFactory _automationFactory;
        private readonly ActuatorFactory _actuatorFactory;
        private readonly SensorFactory _sensorFactory;

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
            SensorFactory sensorFactory)
        {
            if (areaService == null) throw new ArgumentNullException(nameof(areaService));
            if (deviceService == null) throw new ArgumentNullException(nameof(deviceService));
            if (ccToolsBoardService == null) throw new ArgumentNullException(nameof(ccToolsBoardService));
            if (automationFactory == null) throw new ArgumentNullException(nameof(automationFactory));
            if (actuatorFactory == null) throw new ArgumentNullException(nameof(actuatorFactory));
            if (sensorFactory == null) throw new ArgumentNullException(nameof(sensorFactory));

            _areaService = areaService;
            _deviceService = deviceService;
            _ccToolsBoardService = ccToolsBoardService;
            _automationFactory = automationFactory;
            _actuatorFactory = actuatorFactory;
            _sensorFactory = sensorFactory;
        }

        public void Apply()
        {
            var hsrel5 = _ccToolsBoardService.RegisterHSREL5(InstalledDevice.ReadingRoomHSREL5.ToString(), new I2CSlaveAddress(62));
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
            _sensorFactory.RegisterRollerShutterButtons(area, ReadingRoom.RollerShutterButtonUp, input2.GetInput(12),
                ReadingRoom.RollerShutterButtonDown, input2.GetInput(11));

            _actuatorFactory.RegisterSocket(area, ReadingRoom.SocketWindow, hsrel5[HSREL5Pin.Relay0]);
            _actuatorFactory.RegisterSocket(area, ReadingRoom.SocketWallLeft, hsrel5[HSREL5Pin.Relay1]);
            _actuatorFactory.RegisterSocket(area, ReadingRoom.SocketWallRight, hsrel5[HSREL5Pin.Relay2]);

            _sensorFactory.RegisterButton(area, ReadingRoom.Button, input2.GetInput(13));

            area.GetButton(ReadingRoom.Button).PressedShortlyTrigger.Attach(() => area.GetLamp(ReadingRoom.LightCeilingMiddle).TryTogglePowerState());

            _automationFactory.RegisterRollerShutterAutomation(area, ReadingRoom.RollerShutterAutomation)
                .WithRollerShutters(area.GetRollerShutter(ReadingRoom.RollerShutter));

            area.GetRollerShutter(ReadingRoom.RollerShutter)
                .ConnectWith(area.GetButton(ReadingRoom.RollerShutterButtonUp), area.GetButton(ReadingRoom.RollerShutterButtonDown));
        }
    }
}
