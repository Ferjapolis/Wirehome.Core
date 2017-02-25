﻿using System;
using System.Collections.Generic;
using System.Linq;
using HA4IoT.Contracts.Api;
using HA4IoT.Contracts.Components;
using HA4IoT.Contracts.Components.Features;
using HA4IoT.Contracts.Components.States;
using HA4IoT.Contracts.Sensors;
using HA4IoT.Contracts.Services;
using HA4IoT.Contracts.Services.Settings;
using Newtonsoft.Json.Linq;

namespace HA4IoT.Services.Status
{
    [ApiServiceClass(typeof(StatusService))] // TODO: Use IStatusService
    public class StatusService : ServiceBase
    {
        private readonly IComponentRegistryService _componentRegistry;
        private readonly ISettingsService _settingsService;

        public StatusService(IComponentRegistryService componentRegistry, IApiDispatcherService apiService, ISettingsService settingsService)
        {
            if (componentRegistry == null) throw new ArgumentNullException(nameof(componentRegistry));
            if (apiService == null) throw new ArgumentNullException(nameof(apiService));
            if (settingsService == null) throw new ArgumentNullException(nameof(settingsService));

            _componentRegistry = componentRegistry;
            _settingsService = settingsService;
        }

        [ApiMethod]
        public void GetStatus(IApiContext apiContext)
        {
            apiContext.Result = JObject.FromObject(CollectStatus());
        }

        private Status CollectStatus()
        {
            var status = new Status();

            status.OpenWindows.AddRange(GetOpenWindows());
            status.TiltWindows.AddRange(GetTiltWindows());
            status.ActiveComponents.AddRange(GetComponentStatus());

            return status;
        }

        private List<WindowStatus> GetOpenWindows()
        {
            return _componentRegistry.GetComponents<IWindow>()
                .Where(w => w.GetState().Has(WindowState.Open))
                .Select(w => new WindowStatus { Id = w.Id, Caption = _settingsService.GetComponentSettings(w.Id).Caption }).ToList();
        }

        private List<WindowStatus> GetTiltWindows()
        {
            return _componentRegistry.GetComponents<IWindow>()
                .Where(w => w.GetState().Has(WindowState.TildOpen))
                .Select(w => new WindowStatus { Id = w.Id, Caption = _settingsService.GetComponentSettings(w.Id).Caption }).ToList();
        }

        private List<ComponentStatus> GetComponentStatus()
        {
            var actuatorStatusList = new List<ComponentStatus>();

            var components = _componentRegistry.GetComponents();
            foreach (var component in components)
            {
                if (!component.GetFeatures().Supports<PowerStateFeature>())
                {
                    continue;
                }

                if (component.GetState().Has(PowerState.Off))
                {
                    continue;
                }

                var settings = _settingsService.GetComponentSettings<ComponentSettings>(component.Id);
                var actuatorStatus = new ComponentStatus { Id = component.Id, Caption = settings.Caption };
                actuatorStatusList.Add(actuatorStatus);
            }

            return actuatorStatusList;
        }
    }
}
