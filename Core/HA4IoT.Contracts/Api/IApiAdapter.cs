﻿using HA4IoT.Contracts.Components;
using System;

namespace HA4IoT.Contracts.Api
{
    public interface IApiAdapter
    {
        event EventHandler<ApiRequestReceivedEventArgs> ApiRequestReceived;

        void NotifyStateChanged(IComponent component);
    }
}
