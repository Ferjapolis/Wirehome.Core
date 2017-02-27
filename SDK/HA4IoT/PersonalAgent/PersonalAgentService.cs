﻿using System;
using System.Linq;
using System.Text;
using HA4IoT.Contracts.Api;
using HA4IoT.Contracts.Areas;
using HA4IoT.Contracts.Commands;
using HA4IoT.Contracts.Components;
using HA4IoT.Contracts.Components.States;
using HA4IoT.Contracts.Logging;
using HA4IoT.Contracts.PersonalAgent;
using HA4IoT.Contracts.PersonalAgent.AmazonEcho;
using HA4IoT.Contracts.Sensors;
using HA4IoT.Contracts.Services;
using HA4IoT.Contracts.Services.OutdoorHumidity;
using HA4IoT.Contracts.Services.OutdoorTemperature;
using HA4IoT.Contracts.Services.Settings;
using HA4IoT.Contracts.Services.Weather;
using Newtonsoft.Json.Linq;

namespace HA4IoT.PersonalAgent
{
    [ApiServiceClass(typeof(IPersonalAgentService))]
    public class PersonalAgentService : ServiceBase, IPersonalAgentService
    {
        private readonly ISettingsService _settingsService;
        private readonly IComponentRegistryService _componentsRegistry;
        private readonly IAreaRegistryService _areaService;
        private readonly IWeatherService _weatherService;
        private readonly IOutdoorTemperatureService _outdoorTemperatureService;
        private readonly IOutdoorHumidityService _outdoorHumidityService;

        private MessageContext _latestMessageContext;

        public PersonalAgentService(
            ISettingsService settingsService,
            IComponentRegistryService componentRegistry,
            IAreaRegistryService areaService,
            IWeatherService weatherService,
            IOutdoorTemperatureService outdoorTemperatureService,
            IOutdoorHumidityService outdoorHumidityService)
        {
            if (settingsService == null) throw new ArgumentNullException(nameof(settingsService));
            if (componentRegistry == null) throw new ArgumentNullException(nameof(componentRegistry));
            if (areaService == null) throw new ArgumentNullException(nameof(areaService));
            if (weatherService == null) throw new ArgumentNullException(nameof(weatherService));
            if (outdoorTemperatureService == null) throw new ArgumentNullException(nameof(outdoorTemperatureService));
            if (outdoorHumidityService == null) throw new ArgumentNullException(nameof(outdoorHumidityService));

            _settingsService = settingsService;
            _componentsRegistry = componentRegistry;
            _areaService = areaService;
            _weatherService = weatherService;
            _outdoorTemperatureService = outdoorTemperatureService;
            _outdoorHumidityService = outdoorHumidityService;
        }

        [ApiMethod]
        public void ProcessSkillServiceRequest(IApiContext apiContext)
        {
            var request = apiContext.Parameter.ToObject<SkillServiceRequest>();

            var messageContextFactory = new MessageContextFactory(_areaService, _componentsRegistry, _settingsService);
            var messageContext = messageContextFactory.Create(request);

            ProcessMessage(messageContext);

            var response = new SkillServiceResponse();
            response.Response.OutputSpeech.Text = messageContext.Answer;

            apiContext.Result = JObject.FromObject(response);
        }

        [ApiMethod]
        public void Ask(IApiContext apiContext)
        {
            var text = (string) apiContext.Parameter["Message"];
            if (string.IsNullOrEmpty(text))
            {
                apiContext.ResultCode = ApiResultCode.InvalidParameter;
                return;
            }

            apiContext.Result["Answer"] = ProcessTextMessage(text);
        }

        public string ProcessTextMessage(string text)
        {
            if (text == null) throw new ArgumentNullException(nameof(text));

            var messageContextFactory = new MessageContextFactory(_areaService, _componentsRegistry, _settingsService);
            var messageContext = messageContextFactory.Create(text);

            ProcessMessage(messageContext);
            return messageContext.Answer;
        }

        [ApiMethod]
        public void GetLatestMessageContext(IApiContext apiContext)
        {
            if (_latestMessageContext == null)
            {
                return;
            }

            apiContext.Result = JObject.FromObject(_latestMessageContext);
        }

        private void ProcessMessage(MessageContext messageContext)
        {
            try
            {
                _latestMessageContext = messageContext;
                messageContext.Answer = ProcessMessageInternal(messageContext);
            }
            catch (Exception exception)
            {
                messageContext.Answer =
                    $"{Emoji.Scream} Mist! Da ist etwas total schief gelaufen! Bitte stelle mir nie wieder solche Fragen!";
                Log.Error(exception, $"Error while processing message '{messageContext.Text}'.");
            }
        }

        private string ProcessMessageInternal(MessageContext messageContext)
        {
            if (messageContext.GetPatternMatch("Hi").Success)
            {
                return $"{Emoji.VictoryHand} Hi, was kann ich für Dich tun?";
            }

            if (messageContext.GetPatternMatch("Danke").Success)
            {
                return $"{Emoji.Wink} Habe ich doch gerne gemacht.";
            }

            if (messageContext.GetPatternMatch("Wetter").Success)
            {
                return GetWeatherStatus();
            }

            if (messageContext.GetPatternMatch("Fenster").Success)
            {
                return GetWindowStatus();
            }

            if (!messageContext.AffectedComponentIds.Any())
            {
                if (messageContext.IdentifiedComponentIds.Count > 0)
                {
                    return
                        $"{Emoji.Confused} Mit so vielen Anfragen kann ich nicht umgehen. Bitte nenne mir nur eine eindeutige Komponente.";
                }

                return $"{Emoji.Confused} Du musst mir schon einen Sensor oder Aktor nennen.";
            }

            if (messageContext.AffectedComponentIds.Count > 1)
            {
                return $"{Emoji.Flushed} Bitte nicht mehrere Komponenten auf einmal.";
            }

            if (messageContext.AffectedComponentIds.Count == 1)
            {
                var component = _componentsRegistry.GetComponent<IComponent>(messageContext.AffectedComponentIds.First());

                var temperatureSensor = component as ITemperatureSensor;
                if (temperatureSensor != null)
                {
                    return $"{Emoji.Fire} Die Temperatur dieses Sensor liegt aktuell bei {component.GetState()}°C";
                }

                var humiditySensor = component as IHumiditySensor;
                if (humiditySensor != null)
                {
                    return $"{Emoji.SweatDrops} Die Luftfeuchtigkeit dieses Sensor liegt aktuell bei {component.GetState()}%";
                }

                return InvokeCommand(component, messageContext);
            }

            return $"{Emoji.Confused} Das habe ich leider nicht verstanden. Bitte stelle Deine Anfrage präziser.";
        }

        private string InvokeCommand(IComponent component, MessageContext messageContext)
        {
            if (messageContext.IdentifiedCommands.Count == 0)
            {
                return $"{Emoji.Confused} Was soll ich damit machen?";
            }

            if (messageContext.IdentifiedCommands.Count > 1)
            {
                return $"{Emoji.Confused} Das was du möchtest ist nicht eindeutig.";
            }

            try
            {
                component.ExecuteCommand(messageContext.IdentifiedCommands.First());
            }
            catch (CommandNotSupportedException)
            {
                return $"{Emoji.Confused} Das was du möchtest hat nicht funktioniert.";
            }
            
            return $"{Emoji.ThumbsUp} Habe ich erledigt.";
        }

        private string GetWeatherStatus()
        {
            var response = new StringBuilder();
            response.AppendLine($"{Emoji.BarChart} Das Wetter ist aktuell:");
            response.AppendLine($"Temperatur: {_outdoorTemperatureService.OutdoorTemperature}°C");
            response.AppendLine($"Luftfeuchtigkeit: {_outdoorHumidityService.OutdoorHumidity}%");
            response.AppendLine($"Wetter: {_weatherService.Weather}");

            return response.ToString();
        }

        private string GetWindowStatus()
        {
            var allWindows = _componentsRegistry.GetComponents<IWindow>();
            var openWindows = allWindows.Where(w => !w.GetState().Has(WindowState.Closed)).ToList();

            string response;
            if (!openWindows.Any())
            {
                response = $"{Emoji.Lock} Ich habe nachgesehen. Alle Fenster sind geschlossen.";
            }
            else
            {
                response = $"{Emoji.Unlock} Ich habe nachgesehen. Die folgenden Fenster sind noch (ganz oder auf Kipp) geöffnet:\r\n";
                response += string.Join(Environment.NewLine, openWindows.Select(w => "- " + w.Id));
            }

            return response;
        }
    }
}
