﻿using System;
using System.Collections.Generic;
using Windows.Data.Json;
using HA4IoT.Contracts.Api;
using HA4IoT.Contracts.Automations;
using HA4IoT.Contracts.Services;
using HA4IoT.Contracts.Services.System;
using HA4IoT.Services.System;
using HA4IoT.Settings;

namespace HA4IoT.Services.Automations
{
    public class AutomationService : ServiceBase, IAutomationService
    {
        private readonly IApiService _apiService;
        private readonly AutomationCollection _automations = new AutomationCollection();

        public AutomationService(
            ISystemEventsService systemEventsService,
            ISystemInformationService systemInformationService, 
            IApiService apiService)
        {
            if (systemEventsService == null) throw new ArgumentNullException(nameof(systemEventsService));
            if (systemInformationService == null) throw new ArgumentNullException(nameof(systemInformationService));
            if (apiService == null) throw new ArgumentNullException(nameof(apiService));
            if (apiService == null) throw new ArgumentNullException(nameof(apiService));

            _apiService = apiService;

            systemEventsService.StartupCompleted += (s, e) =>
            {
                systemInformationService.Set("Automations/Count", _automations.GetAll().Count);
            };

            apiService.StatusRequested += HandleApiStatusRequest;
        }

        public void AddAutomation(IAutomation automation)
        {
            if (automation == null) throw new ArgumentNullException(nameof(automation));

            _automations.AddOrUpdate(automation.Id, automation);

            new SettingsContainerApiDispatcher(automation.Settings, $"automation/{automation.Id}", _apiService).ExposeToApi();
        }

        public IList<TAutomation> GetAutomations<TAutomation>() where TAutomation : IAutomation
        {
            return _automations.GetAll<TAutomation>();
        }

        public TAutomation GetAutomation<TAutomation>(AutomationId id) where TAutomation : IAutomation
        {
            if (id == null) throw new ArgumentNullException(nameof(id));

            return _automations.Get<TAutomation>(id);
        }

        public IList<IAutomation> GetAutomations()
        {
            return _automations.GetAll();
        }

        private void HandleApiStatusRequest(object sender, ApiRequestReceivedEventArgs e)
        {
            var automations = new JsonObject();
            foreach (var automation in _automations.GetAll())
            {
                automations.SetNamedValue(automation.Id.Value, automation.ExportStatusToJsonObject());
            }

            e.Context.Response.SetNamedValue("automations", automations);
        }
    }
}
