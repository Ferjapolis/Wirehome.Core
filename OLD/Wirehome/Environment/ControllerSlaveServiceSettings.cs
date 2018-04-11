﻿namespace Wirehome.Environment
{
    public class ControllerSlaveServiceSettings
    {
        public bool IsEnabled { get; set; }

        public string MasterAddress { get; set; }

        public bool UseTemperature { get; set; }

        public bool UseHumidity { get; set; }

        public bool UseSunriseSunset { get; set; }

        public bool UseWeather { get; set; }
    }
}
