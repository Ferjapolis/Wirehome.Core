using System;
using System.Diagnostics;
using System.IO;
using Windows.Web.Http;
using HA4IoT.Contracts.Api;
using HA4IoT.Contracts.Core;
using HA4IoT.Contracts.Logging;
using HA4IoT.Contracts.Services;
using HA4IoT.Contracts.Services.Daylight;
using HA4IoT.Contracts.Services.OutdoorHumidity;
using HA4IoT.Contracts.Services.OutdoorTemperature;
using HA4IoT.Contracts.Services.Settings;
using HA4IoT.Contracts.Services.System;
using HA4IoT.Contracts.Services.Weather;
using HA4IoT.Networking.Json;

namespace HA4IoT.ExternalServices.OpenWeatherMap
{
    [ApiServiceClass(typeof(OpenWeatherMapService))]
    public class OpenWeatherMapService : ServiceBase
    {
        private readonly string _cacheFilename = StoragePath.WithFilename("OpenWeatherMapCache.json");

        private readonly IOutdoorTemperatureService _outdoorTemperatureService;
        private readonly IOutdoorHumidityService _outdoorHumidityService;
        private readonly IDaylightService _daylightService;
        private readonly IWeatherService _weatherService;
        private readonly IDateTimeService _dateTimeService;
        private readonly ISystemInformationService _systemInformationService;
        
        private string _previousResponse;

        [JsonMember]
        public float Temperature { get; private set; }
        [JsonMember]
        public float Humidity { get; private set; }
        [JsonMember]
        public TimeSpan Sunrise { get; private set; }
        [JsonMember]
        public TimeSpan Sunset { get; private set; }
        [JsonMember]
        public Weather Weather { get; private set; }
        
        public OpenWeatherMapService(
            IOutdoorTemperatureService outdoorTemperatureService,
            IOutdoorHumidityService outdoorHumidityService,
            IDaylightService daylightService,
            IWeatherService weatherService,
            IDateTimeService dateTimeService, 
            ISchedulerService schedulerService, 
            ISystemInformationService systemInformationService,
            ISettingsService settingsService)
        {
            if (outdoorTemperatureService == null) throw new ArgumentNullException(nameof(outdoorTemperatureService));
            if (outdoorHumidityService == null) throw new ArgumentNullException(nameof(outdoorHumidityService));
            if (daylightService == null) throw new ArgumentNullException(nameof(daylightService));
            if (weatherService == null) throw new ArgumentNullException(nameof(weatherService));
            if (dateTimeService == null) throw new ArgumentNullException(nameof(dateTimeService));
            if (systemInformationService == null) throw new ArgumentNullException(nameof(systemInformationService));
            if (settingsService == null) throw new ArgumentNullException(nameof(settingsService));

            _outdoorTemperatureService = outdoorTemperatureService;
            _outdoorHumidityService = outdoorHumidityService;
            _daylightService = daylightService;
            _weatherService = weatherService;
            _dateTimeService = dateTimeService;
            _systemInformationService = systemInformationService;
            
            settingsService.CreateSettingsMonitor<OpenWeatherMapServiceSettings>(s => Settings = s);

            LoadPersistedValues();
            
            schedulerService.RegisterSchedule("OpenWeatherMapServiceUpdater", TimeSpan.FromMinutes(5), Refresh);
        }

        public OpenWeatherMapServiceSettings Settings { get; private set; }

        [ApiMethod(ApiCallType.Command)]
        public void Status(IApiContext apiContext)
        {
            apiContext.Response = this.ToJsonObject(ToJsonObjectMode.Explicit);
        }

        [ApiMethod(ApiCallType.Command)]
        public void Refresh(IApiContext apiContext)
        {
            Refresh();
        }

        private void PersistData(string weatherData)
        {
            File.WriteAllText(_cacheFilename, weatherData);
        }

        private void Refresh()
        {
            if (!Settings.IsEnabled)
            {
                Log.Verbose("Fetching Open Weather Map Service is disabled.");
                return;
            }

            Log.Verbose("Fetching Open Weather Map weather data.");

            var response = FetchWeatherData();

            if (!string.Equals(response, _previousResponse))
            {
                if (TryParseData(response))
                {
                    PersistData(response);
                }

                PushData();

                _previousResponse = response;

                _systemInformationService.Set("OpenWeatherMapService/LastUpdatedTimestamp", _dateTimeService.Now);
            }

            _systemInformationService.Set("OpenWeatherMapService/LastFetchedTimestamp", _dateTimeService.Now);
        }

        private void PushData()
        {
            if (Settings.UseTemperature)
            {
                _outdoorTemperatureService.Update(Temperature);
            }

            if (Settings.UseHumidity)
            {
                _outdoorHumidityService.Update(Humidity);
            }

            if (Settings.UseSunriseSunset)
            {
                _daylightService.Update(Sunrise, Sunset);
            }

            if (Settings.UseWeather)
            {
                _weatherService.Update(Weather);
            }
        }

        private string FetchWeatherData()
        {
            var uri = new Uri($"http://api.openweathermap.org/data/2.5/weather?lat={Settings.Latitude}&lon={Settings.Longitude}&APPID={Settings.AppId}&units=metric");

            _systemInformationService.Set("OpenWeatherMapService/Uri", uri.ToString());

            var stopwatch = Stopwatch.StartNew();
            try
            {
                using (var httpClient = new HttpClient())
                using (HttpResponseMessage result = httpClient.GetAsync(uri).AsTask().Result)
                {
                    return result.Content.ReadAsStringAsync().AsTask().Result;
                }
            }
            finally
            {
                _systemInformationService.Set("OpenWeatherMapService/LastFetchDuration", stopwatch.Elapsed);
            }
        }

        private bool TryParseData(string weatherData)
        {
            try
            {
                var parser = new OpenWeatherMapResponseParser();
                parser.Parse(weatherData);

                Weather = parser.Weather;
                Temperature = parser.Temperature;
                Humidity = parser.Humidity;

                Sunrise = parser.Sunrise;
                Sunset = parser.Sunset;

                return true;
            }
            catch (Exception exception)
            {
                Log.Warning(exception, $"Error while parsing Open Weather Map response ({weatherData}).");

                return false;
            }
        }

        private void LoadPersistedValues()
        {
            if (!File.Exists(_cacheFilename))
            {
                return;
            }

            try
            {
                TryParseData(File.ReadAllText(_cacheFilename));
            }
            catch (Exception exception)
            {
                Log.Warning(exception, "Unable to load cached weather data.");
                File.Delete(_cacheFilename);
            }
        }
    }
}