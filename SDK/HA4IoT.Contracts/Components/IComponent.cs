﻿using System;
using System.Collections.Generic;
using HA4IoT.Contracts.Api;
using Newtonsoft.Json.Linq;

namespace HA4IoT.Contracts.Components
{
    public interface IComponent
    {
        event EventHandler<ComponentStateChangedEventArgs> StateChanged;

        ComponentId Id { get; }

        IComponentState GetState();

        // TODO: ToIJsonValue
        IList<IComponentState> GetSupportedStates();

        void HandleApiCall(IApiContext apiContext);

        JToken ExportConfiguration();

        JToken ExportStatus();
    }
}
