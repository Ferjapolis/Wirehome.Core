﻿using System;

namespace HA4IoT.Networking.Http
{
    public class UpgradedToWebSocketSessionEventArgs : EventArgs
    {
        public UpgradedToWebSocketSessionEventArgs(HttpRequest httpRequest)
        {
            if (httpRequest == null) throw new ArgumentNullException(nameof(httpRequest));

            HttpRequest = httpRequest;
        }

        public HttpRequest HttpRequest { get; }
    }
}
